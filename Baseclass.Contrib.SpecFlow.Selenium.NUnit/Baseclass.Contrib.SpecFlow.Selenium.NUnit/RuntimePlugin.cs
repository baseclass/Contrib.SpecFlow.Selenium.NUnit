using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechTalk.SpecFlow.Infrastructure;
using TechTalk.SpecFlow.UnitTestProvider;

[assembly: RuntimePlugin(typeof(Baseclass.Contrib.SpecFlow.Selenium.NUnit.RuntimePlugin))]

namespace Baseclass.Contrib.SpecFlow.Selenium.NUnit
{
    public class RuntimePlugin : IRuntimePlugin
    {
        public void RegisterConfigurationDefaults(TechTalk.SpecFlow.Configuration.RuntimeConfiguration runtimeConfiguration)
        {

        }

        public void RegisterCustomizations(BoDi.ObjectContainer container, TechTalk.SpecFlow.Configuration.RuntimeConfiguration runtimeConfiguration)
        {

        }

        public void RegisterDependencies(BoDi.ObjectContainer container)
        {
            var runtimeProvider = new NUnitRuntimeProvider();

            container.RegisterInstanceAs<IUnitTestRuntimeProvider>(runtimeProvider, "SeleniumNUnit");
        }
    }
}
