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

        /// <summary>
        /// Initialization Methods to Generate. MethodName => List of Argument Names
        /// </summary>
        private Dictionary<string, List<string>> initializeMethodsToGenerate = new Dictionary<string, List<string>>();
        /// <summary>
        /// List of unique field Names to Generate
        /// </summary>
        private HashSet<string> fieldsToGenerate = new HashSet<string>();
        bool hasBrowser = false;

        public void SetTestMethodCategories(TechTalk.SpecFlow.Generator.TestClassGenerationContext generationContext, System.CodeDom.CodeMemberMethod testMethod, IEnumerable<string> scenarioCategories)
        {
            this.codeDomHelper.AddAttributeForEachValue(testMethod, CATEGORY_ATTR, scenarioCategories.Where(cat => !cat.StartsWith("Browser:") && !cat.Contains(":")));

            Dictionary<string, List<string>> categoryTags = new Dictionary<string, List<string>>();
            
            bool hasTags = false;

            foreach(var tag in scenarioCategories.Where(cat => cat.Contains(":")).Select(cat => cat.Split(':')))
            {
                if (tag.Length != 2)
                    continue;
                hasTags = true;
                if (tag[0].Equals("Browser",StringComparison.OrdinalIgnoreCase))
                {
                    hasBrowser = true;
                }
                testMethod.UserData.Add(tag[0] + ":" + tag[1], tag[1]);
                List<string> tagValues = null;
                if (!categoryTags.TryGetValue(tag[0],out tagValues))
                {
                    tagValues = new List<string>();
                    categoryTags[tag[0]] = tagValues;
                }
                tagValues.Add(tag[1]);
            }

            if (hasTags)
            {
                //TestName and TestCategory Building
                //List of list of tags different values
                List<List<string>> values = new List<List<string>>();
                foreach (var kvp in categoryTags)
                {
                    values.Add(kvp.Value);
                }
                List<List<string>> combinations = new List<List<string>>();
                //Generate an exhaustive list of values combinations
                GeneratePermutations(values, combinations, 0, new List<string>());

                foreach (var combination in combinations)
                {
                    //Each combination is a different TestCase
                    var withTagArgs = combination.Select(s => new CodeAttributeArgument(new CodePrimitiveExpression(s))).ToList()
                        .Concat(new[] {
                                new CodeAttributeArgument("Category", new CodePrimitiveExpression(String.Join(",",combination))),
                                new CodeAttributeArgument("TestName", new CodePrimitiveExpression(string.Format("{0} with {1}", testMethod.Name, String.Join(",",combination))))
                            })
                        .ToArray();

                    this.codeDomHelper.AddAttribute(testMethod, ROW_ATTR, withTagArgs);    
                }
                

                List<System.CodeDom.CodeParameterDeclarationExpression> parameters = new List<CodeParameterDeclarationExpression>();
                int i = 0;

                List<string> orderedTags = new List<string>();
                foreach (var kvp in categoryTags)
                {
                    //Add the category name to category list
                    orderedTags.Add(kvp.Key);
                    //Mark the field to be generated
                    fieldsToGenerate.Add(kvp.Key);
                    //Add a parameter to the testMethod
                    testMethod.Parameters.Insert(i, new System.CodeDom.CodeParameterDeclarationExpression("System.String", kvp.Key.ToLowerInvariant()));
                    i = i + 1;
                }

                string methodName = "InitializeSelenium"+String.Join("",orderedTags);
                string initializeSeleniumArgs = String.Join(",",orderedTags).ToLowerInvariant();
                //Create the call to the initialization Method
                testMethod.Statements.Insert(0, new CodeSnippetStatement(methodName+"(" + initializeSeleniumArgs + ");"));
                List<string> nothing = null;
                if (!initializeMethodsToGenerate.TryGetValue(methodName, out nothing))
                {
                    //Mark the initialization method to be generated
                    initializeMethodsToGenerate[methodName] = orderedTags.Select(s => s.ToLowerInvariant()).ToList();
                }            
            }
        }

        private void GeneratePermutations(List<List<string>> Lists, List<List<string>> result, int depth, List<string> current)
        {
            //TODO rajouter les CodePrimitiveExpression
            if (depth == Lists.Count)
            {
                result.Add(current);
                return;
            }

            for (int i = 0; i < Lists[depth].Count; i++)
            {
                var newList = new List<string>(current);
                newList.Add(Lists[depth][i]);
                GeneratePermutations(Lists, result, depth + 1, newList);
            }
        }

        public void SetRow(TechTalk.SpecFlow.Generator.TestClassGenerationContext generationContext, System.CodeDom.CodeMemberMethod testMethod
            , IEnumerable<string> arguments, IEnumerable<string> tags, bool isIgnored)
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


            var categories = testMethod.UserData.Keys.OfType<string>()
                .Where(key => key.Contains(":"));
            

            var browsers = testMethod.UserData.Keys.OfType<string>()
                .Where(key => key.StartsWith("Browser:"))
                .Select(key => (string) testMethod.UserData[key]).ToArray();

            if (categories.Any())
            {
                //List of list of tags different values
                Dictionary<string,List<string>> values = new Dictionary<string,List<string>>();
                foreach (var userDataKey in categories)
                {
                    string catName = userDataKey.Substring(0, userDataKey.IndexOf(':'));
                    List<string> val = null;
                    if (!values.TryGetValue(catName, out val))
                    {
                        val = new List<string>();
                        values[catName] = val;
                    }
                    val.Add((string)testMethod.UserData[userDataKey]);
                }

                List<List<string>> combinations = new List<List<string>>();
                //Generate an exhaustive list of values combinations
                GeneratePermutations(values.Values.ToList(), combinations, 0, new List<string>());

                //Remove TestCase attributes
                foreach (var codeAttributeDeclaration in testMethod.CustomAttributes.Cast<CodeAttributeDeclaration>()
                    .Where(attr => attr.Name == ROW_ATTR && attr.Arguments.Count == 2+values.Keys.Count).ToList())
                {
                    testMethod.CustomAttributes.Remove(codeAttributeDeclaration);
                }

                foreach (var combination in combinations)
                {
                    var argsString = string.Concat(args.Take(args.Count - 1).Select(arg => string.Format("\"{0}\" ,", ((CodePrimitiveExpression)arg.Value).Value)));
                    argsString = argsString.TrimEnd(' ', ',');

                    //Each combination is a different TestCase
                    var withTagArgs = combination.Select(s => new CodeAttributeArgument(new CodePrimitiveExpression(s))).ToList()
                        .Concat(args)
                        .Concat(new[] {
                                new CodeAttributeArgument("Category", new CodePrimitiveExpression(String.Join(",",combination))),
                                new CodeAttributeArgument("TestName", new CodePrimitiveExpression(string.Format("{0} with {1} and {2}", testMethod.Name, String.Join(",",combination),argsString)))
                            })
                        .ToArray();

                    this.codeDomHelper.AddAttribute(testMethod, ROW_ATTR, withTagArgs);
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

        }

        private void CreateInitializeSeleniumMethod(TechTalk.SpecFlow.Generator.TestClassGenerationContext generationContext)
        {
            foreach (var kvp in initializeMethodsToGenerate)
            {
                var initializeSelenium = new CodeMemberMethod();
                initializeSelenium.Name = kvp.Key;
                foreach (var paramName in kvp.Value)
                {
                    initializeSelenium.Parameters.Add(new CodeParameterDeclarationExpression("System.String", paramName));
                    if (paramName.Equals("browser", StringComparison.OrdinalIgnoreCase))
                    {
                        initializeSelenium.Statements.Add(new CodeSnippetStatement("            this.driver = this.container.ResolveNamed<OpenQA.Selenium.IWebDriver>(" + paramName + ");"));
                    }
                    else
                    {
                        initializeSelenium.Statements.Add(new CodeSnippetStatement("            this._"+paramName+" = "+paramName+";"));
                    }
                }
                
                generationContext.TestClass.Members.Add(initializeSelenium);
            }            
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

            foreach (var field in fieldsToGenerate)
            {
                if (!field.Equals("Browser", StringComparison.OrdinalIgnoreCase))
                {
                    generationContext.TestClass.Members.Add(new CodeMemberField("System.String", "_" + field.ToLowerInvariant()));
                    generationContext.TestCleanupMethod.Statements.Add(new CodeSnippetStatement("            ScenarioContext.Current.Remove(\"" + field + "\");"));
                }
            }
            CreateInitializeSeleniumMethod(generationContext);

            if (!scenarioSetupMethodsAdded)
            {
                if (hasBrowser)
                {
                    generationContext.ScenarioInitializeMethod.Statements.Add(new CodeSnippetStatement("            if(this.driver != null)"));
                    generationContext.ScenarioInitializeMethod.Statements.Add(new CodeSnippetStatement("                ScenarioContext.Current.Add(\"Driver\", this.driver);"));
                    generationContext.ScenarioInitializeMethod.Statements.Add(new CodeSnippetStatement("            if(this.container != null)"));
                    generationContext.ScenarioInitializeMethod.Statements.Add(new CodeSnippetStatement("                ScenarioContext.Current.Add(\"Container\", this.container);"));
                }
                foreach (var field in fieldsToGenerate)
                {
                    if (!field.Equals("Browser", StringComparison.OrdinalIgnoreCase))
                    {
                        generationContext.ScenarioInitializeMethod.Statements.Add(new CodeSnippetStatement("            if(this._" + field.ToLowerInvariant() + " != null)"));
                        generationContext.ScenarioInitializeMethod.Statements.Add(new CodeSnippetStatement("                ScenarioContext.Current.Add(\"" + field + "\", this._" + field.ToLowerInvariant() + ");"));
                    }
                }
                scenarioSetupMethodsAdded = true;
            }
        }
    }
}
