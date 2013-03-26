using Autofac;
using Autofac.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TechTalk.SpecFlow;

namespace TestApplication.UiTests
{
    [Binding]
    public class SpecflowDefault
    {
        public static IContainer Container { get; private set; }

        [Before]
        public static void Before()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new ConfigurationSettingsReader());
            Container = builder.Build();
        }
    }
}
