using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechTalk.SpecFlow.Generator.Interfaces;
using TechTalk.SpecFlow.Generator.Plugins;
using TechTalk.SpecFlow.Generator.UnitTestProvider;
using TechTalk.SpecFlow.Infrastructure;
using TechTalk.SpecFlow.UnitTestProvider;
using TechTalk.SpecFlow.Utils;

[assembly: GeneratorPlugin(typeof(Baseclass.Contrib.SpecFlow.Selenium.NUnit.GeneratorPlugin))]

namespace Baseclass.Contrib.SpecFlow.Selenium.NUnit
{
    public class GeneratorPlugin : IGeneratorPlugin
    {
        public void RegisterConfigurationDefaults(TechTalk.SpecFlow.Generator.Configuration.SpecFlowProjectConfiguration specFlowConfiguration)
        {
            
        }

        public void RegisterCustomizations(BoDi.ObjectContainer container, TechTalk.SpecFlow.Generator.Configuration.SpecFlowProjectConfiguration generatorConfiguration)
        {
            
        }

        public void RegisterDependencies(BoDi.ObjectContainer container)
        {
            var projectSettings = container.Resolve<ProjectSettings>();

            var codeDomHelper = container.Resolve<CodeDomHelper>(projectSettings.ProjectPlatformSettings.Language);

            var generatorProvider = new SeleniumNUnitTestGeneratorProvider(codeDomHelper);
            
            container.RegisterInstanceAs<IUnitTestGeneratorProvider>(generatorProvider, "SeleniumNUnit");
        }
    }
}
