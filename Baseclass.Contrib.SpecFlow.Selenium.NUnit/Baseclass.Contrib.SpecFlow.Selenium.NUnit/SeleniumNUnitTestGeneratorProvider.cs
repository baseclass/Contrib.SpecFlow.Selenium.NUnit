using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechTalk.SpecFlow.Generator.UnitTestProvider;
using TechTalk.SpecFlow.Infrastructure;
using TechTalk.SpecFlow.Utils;

namespace Baseclass.Contrib.SpecFlow.Selenium.NUnit
{
    public class SeleniumNUnitTestGeneratorProvider : IUnitTestGeneratorProvider
    {
        private const string TESTFIXTURE_ATTR = "NUnit.Framework.TestFixtureAttribute";
        private const string TEST_ATTR = "NUnit.Framework.TestAttribute";
        private const string ROW_ATTR = "NUnit.Framework.TestCaseAttribute";
        private const string CATEGORY_ATTR = "NUnit.Framework.CategoryAttribute";
        private const string TESTSETUP_ATTR = "NUnit.Framework.SetUpAttribute";
        private const string TESTFIXTURESETUP_ATTR = "NUnit.Framework.TestFixtureSetUpAttribute";
        private const string TESTFIXTURETEARDOWN_ATTR = "NUnit.Framework.TestFixtureTearDownAttribute";
        private const string TESTTEARDOWN_ATTR = "NUnit.Framework.TearDownAttribute";
        private const string IGNORE_ATTR = "NUnit.Framework.IgnoreAttribute";
        private const string DESCRIPTION_ATTR = "NUnit.Framework.DescriptionAttribute";

        public bool SupportsRowTests { get { return true; } }
        public bool SupportsAsyncTests { get { return false; } }

        private CodeDomHelper codeDomHelper;

        private bool scenarioSetupMethodsAdded = false;

        public SeleniumNUnitTestGeneratorProvider(CodeDomHelper codeDomHelper)
        {
            this.codeDomHelper = codeDomHelper;
        }

        public void SetTestMethodCategories(TechTalk.SpecFlow.Generator.TestClassGenerationContext generationContext, System.CodeDom.CodeMemberMethod testMethod, IEnumerable<string> scenarioCategories)
        {
            this.codeDomHelper.AddAttributeForEachValue(testMethod, CATEGORY_ATTR, scenarioCategories.Where(cat => !cat.StartsWith("Browser:")));
            
            bool hasBrowser = false;

            foreach(var browser in scenarioCategories.Where(cat => cat.StartsWith("Browser:")).Select(cat => cat.Replace("Browser:", "")))
            {
                testMethod.UserData.Add("Browser:" + browser, browser);

                var withBrowserArgs = new[] { new CodeAttributeArgument(new CodePrimitiveExpression(browser)) }
                        .Concat(new[] {
                                new CodeAttributeArgument("Category", new CodePrimitiveExpression(browser)),
                                new CodeAttributeArgument("TestName", new CodePrimitiveExpression(string.Format("{0} on {1}", testMethod.Name, browser)))
                            })
                        .ToArray();

                this.codeDomHelper.AddAttribute(testMethod, ROW_ATTR, withBrowserArgs);

                hasBrowser = true;
            }

            if (hasBrowser)
            {
                if (!scenarioSetupMethodsAdded)
                {
                    generationContext.ScenarioInitializeMethod.Statements.Add(new CodeSnippetStatement("            if(this.driver != null)"));
                    generationContext.ScenarioInitializeMethod.Statements.Add(new CodeSnippetStatement("                ScenarioContext.Current.Add(\"Driver\", this.driver);"));
                    generationContext.ScenarioInitializeMethod.Statements.Add(new CodeSnippetStatement("            if(this.container != null)"));
                    generationContext.ScenarioInitializeMethod.Statements.Add(new CodeSnippetStatement("                ScenarioContext.Current.Add(\"Container\", this.container);"));
                    scenarioSetupMethodsAdded = true;
                }
                
                testMethod.Statements.Insert(0, new CodeSnippetStatement("            InitializeSelenium(browser);"));
                testMethod.Parameters.Insert(0, new System.CodeDom.CodeParameterDeclarationExpression("System.string" , "browser"));
            }
        }

        public void SetRow(TechTalk.SpecFlow.Generator.TestClassGenerationContext generationContext, System.CodeDom.CodeMemberMethod testMethod, IEnumerable<string> arguments, IEnumerable<string> tags, bool isIgnored)
        {
            var args = arguments.Select(
              arg => new CodeAttributeArgument(new CodePrimitiveExpression(arg))).ToList();

            // addressing ReSharper bug: TestCase attribute with empty string[] param causes inconclusive result - https://github.com/techtalk/SpecFlow/issues/116
            var exampleTagExpressionList = tags.Select(t => new CodePrimitiveExpression(t)).ToArray();
            CodeExpression exampleTagsExpression = exampleTagExpressionList.Length == 0 ?
                (CodeExpression)new CodePrimitiveExpression(null) :
                new CodeArrayCreateExpression(typeof(string[]), exampleTagExpressionList);
            args.Add(new CodeAttributeArgument(exampleTagsExpression));

            if (isIgnored)
                args.Add(new CodeAttributeArgument("Ignored", new CodePrimitiveExpression(true)));

            var browsers = testMethod.UserData.Keys.OfType<string>()
                .Where(key => key.StartsWith("Browser:"))
                .Select(key => (string) testMethod.UserData[key]).ToArray();

            if (browsers.Any())
            {
                foreach (var codeAttributeDeclaration in testMethod.CustomAttributes.Cast<CodeAttributeDeclaration>().Where(attr => attr.Name == ROW_ATTR && attr.Arguments.Count == 3).ToList())
                {
                    testMethod.CustomAttributes.Remove(codeAttributeDeclaration);
                }

                foreach (var browser in browsers)
                {
                    var argsString = string.Concat(args.Take(args.Count - 1).Select(arg => string.Format("\"{0}\" ,", ((CodePrimitiveExpression)arg.Value).Value)));
                    argsString = argsString.TrimEnd(' ', ',');

                    var withBrowserArgs = new[] { new CodeAttributeArgument(new CodePrimitiveExpression(browser)) }
                        .Concat(args)
                        .Concat(new [] {
                                new CodeAttributeArgument("Category", new CodePrimitiveExpression(browser)),
                                new CodeAttributeArgument("TestName", new CodePrimitiveExpression(string.Format("{0} on {1} with: {2}", testMethod.Name, browser, argsString)))
                            })
                        .ToArray();

                    this.codeDomHelper.AddAttribute(testMethod, ROW_ATTR, withBrowserArgs);
                }
            }
            else
            {
                this.codeDomHelper.AddAttribute(testMethod, ROW_ATTR, args.ToArray());
            }
        }

        public void SetTestClass(TechTalk.SpecFlow.Generator.TestClassGenerationContext generationContext, string featureTitle, string featureDescription)
        {
            codeDomHelper.AddAttribute(generationContext.TestClass, TESTFIXTURE_ATTR);
            codeDomHelper.AddAttribute(generationContext.TestClass, DESCRIPTION_ATTR, featureTitle);

            generationContext.Namespace.Imports.Add(new CodeNamespaceImport("Autofac"));
            generationContext.Namespace.Imports.Add(new CodeNamespaceImport("Autofac.Configuration"));

            generationContext.TestClass.Members.Add(new CodeMemberField("OpenQA.Selenium.IWebDriver", "driver"));
            generationContext.TestClass.Members.Add(new CodeMemberField("IContainer", "container"));

            CreateInitializeSeleniumMethod(generationContext);
        }

        private static void CreateInitializeSeleniumMethod(TechTalk.SpecFlow.Generator.TestClassGenerationContext generationContext)
        {
            var initializeSelenium = new CodeMemberMethod();
            initializeSelenium.Name = "InitializeSelenium";
            initializeSelenium.Parameters.Add(new CodeParameterDeclarationExpression("System.String", "browser"));
            initializeSelenium.Statements.Add(new CodeSnippetStatement("            this.driver = this.container.ResolveNamed<OpenQA.Selenium.IWebDriver>(browser);"));

            generationContext.TestClass.Members.Add(initializeSelenium);
        }

        public void SetTestClassCategories(TechTalk.SpecFlow.Generator.TestClassGenerationContext generationContext, IEnumerable<string> featureCategories)
        {
            this.codeDomHelper.AddAttributeForEachValue(generationContext.TestClass, CATEGORY_ATTR, featureCategories);
        }

        public void SetTestClassCleanupMethod(TechTalk.SpecFlow.Generator.TestClassGenerationContext generationContext)
        {
            this.codeDomHelper.AddAttribute(generationContext.TestClassCleanupMethod, TESTFIXTURETEARDOWN_ATTR);
        }

        public void SetTestClassIgnore(TechTalk.SpecFlow.Generator.TestClassGenerationContext generationContext)
        {
            this.codeDomHelper.AddAttribute(generationContext.TestClass, IGNORE_ATTR);
        }

        public void SetTestClassInitializeMethod(TechTalk.SpecFlow.Generator.TestClassGenerationContext generationContext)
        {
            this.codeDomHelper.AddAttribute(generationContext.TestClassInitializeMethod, TESTFIXTURESETUP_ATTR);

            generationContext.TestClassInitializeMethod.Statements.Add(new CodeSnippetStatement("            var builder = new ContainerBuilder();"));
            generationContext.TestClassInitializeMethod.Statements.Add(new CodeSnippetStatement("            builder.RegisterModule(new ConfigurationSettingsReader());"));
            generationContext.TestClassInitializeMethod.Statements.Add(new CodeSnippetStatement("            this.container = builder.Build();"));
        }

        public void SetTestCleanupMethod(TechTalk.SpecFlow.Generator.TestClassGenerationContext generationContext)
        {
            this.codeDomHelper.AddAttribute(generationContext.TestCleanupMethod, TESTTEARDOWN_ATTR);
        }

        public void SetTestInitializeMethod(TechTalk.SpecFlow.Generator.TestClassGenerationContext generationContext)
        {
            this.codeDomHelper.AddAttribute(generationContext.TestInitializeMethod, TESTSETUP_ATTR);
        }

        public void SetTestMethod(TechTalk.SpecFlow.Generator.TestClassGenerationContext generationContext, System.CodeDom.CodeMemberMethod testMethod, string scenarioTitle)
        {
            this.codeDomHelper.AddAttribute(testMethod, TEST_ATTR);
            this.codeDomHelper.AddAttribute(testMethod, DESCRIPTION_ATTR, scenarioTitle);
        }

        public void SetTestMethodIgnore(TechTalk.SpecFlow.Generator.TestClassGenerationContext generationContext, System.CodeDom.CodeMemberMethod testMethod)
        {
            this.codeDomHelper.AddAttribute(testMethod, IGNORE_ATTR);
        }

        public void SetRowTest(TechTalk.SpecFlow.Generator.TestClassGenerationContext generationContext, System.CodeDom.CodeMemberMethod testMethod, string scenarioTitle)
        {
            this.SetTestMethod(generationContext, testMethod, scenarioTitle);
        }

        public void SetTestMethodAsRow(TechTalk.SpecFlow.Generator.TestClassGenerationContext generationContext, System.CodeDom.CodeMemberMethod testMethod, string scenarioTitle, string exampleSetName, string variantName, IEnumerable<KeyValuePair<string, string>> arguments)
        {
            
        }

        public void FinalizeTestClass(TechTalk.SpecFlow.Generator.TestClassGenerationContext generationContext)
        {
            // Can't be move to SetTestCleanupMethod, as the code at that point misses the .OnScenarioEnd() call.
            // Make sure this code is at the end!
            generationContext.TestCleanupMethod.Statements.Add(new CodeSnippetStatement("            try { System.Threading.Thread.Sleep(50); this.driver.Quit(); } catch (System.Exception) {}"));
            generationContext.TestCleanupMethod.Statements.Add(new CodeSnippetStatement("            this.driver = null;"));
            generationContext.TestCleanupMethod.Statements.Add(new CodeSnippetStatement("            ScenarioContext.Current.Remove(\"Driver\");"));
            generationContext.TestCleanupMethod.Statements.Add(new CodeSnippetStatement("            ScenarioContext.Current.Remove(\"Container\");"));
        }
    }
}
