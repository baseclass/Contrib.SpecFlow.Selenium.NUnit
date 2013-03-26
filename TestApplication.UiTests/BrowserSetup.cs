using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using Autofac;

namespace TestApplication.UiTests
{
    public abstract class BrowserSetup
    {
        protected IWebDriver Driver { get; private set; }

        [Given(@"I navigated to (.*) with (.*)")]
        public void GivenIamUsingBrowser(string url, string browser)
        {
            this.Driver = SpecflowDefault.Container.ResolveNamed<IWebDriver>(browser);
            //this.Driver = new RemoteWebDriver(new Uri("http://127.0.0.1:4444/wd/hub"), DesiredCapabilities.InternetExplorer());
            this.Driver.Manage().Window.Maximize();
            this.Driver.Navigate().GoToUrl("http://localhost:58909" + url);
        }

        [AfterScenario]
        public void CleanUp()
        {
            try
            {
                Thread.Sleep(50);
                // Stop the web driver
                this.Driver.Quit();
            }
            catch (Exception)
            {
            }
        }

    }
}
