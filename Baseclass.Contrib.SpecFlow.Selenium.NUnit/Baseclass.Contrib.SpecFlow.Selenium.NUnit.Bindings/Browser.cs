using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace Baseclass.Contrib.SpecFlow.Selenium.NUnit.Bindings
{
    [Binding]
    public class Browser
    {
        public static IWebDriver Current
        {
            get
            {
                return (IWebDriver)ScenarioContext.Current["Driver"];
            }
        }

        [Given(@"I navigated to (.*)")]
        public void GivenINavigatedTo(string url)
        {
            var driver = Current;
            driver.Manage().Window.Maximize();
            string baseUrl = ConfigurationManager.AppSettings["seleniumBaseUrl"];
            driver.Navigate().GoToUrl(string.Format("{0}{1}", baseUrl, url));
        }
    }
}
