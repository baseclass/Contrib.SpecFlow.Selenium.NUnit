using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using TechTalk.SpecFlow;
using Baseclass.Contrib.SpecFlow.Selenium.NUnit;
using Baseclass.Contrib.SpecFlow.Selenium.NUnit.Bindings;

namespace TestApplication.UiTests
{
    [Binding]
    public class CalculatorFeatureSteps
    {
        private WebDriverWait wait;
        public WebDriverWait Wait
        {
            get
            {
                if (wait == null)
                {
                    this.wait = new WebDriverWait(Browser.Current, TimeSpan.FromSeconds(10));
                }
                return wait;
            }
        }

        [Given(@"I have entered (.*) into (.*) calculator")]
        public void GivenIHaveEnteredIntoTheCalculator(int p0, string id)
        {
            Wait.Until(d => d.FindElement(By.Id(id))).SendKeys(p0.ToString());
        }
        
        [When(@"I press add")]
        public void WhenIPressAdd()
        {
            var addButton = Wait.Until(d => d.FindElement(By.Id("AddButton")));
            addButton.Submit();
        }
        
        [Then(@"the result should be (.*) on the screen")]
        public void ThenTheResultShouldBeOnTheScreen(int p0)
        {
            var result = Wait.Until(d => d.FindElement(By.Id("result")));

            Assert.AreEqual(p0.ToString(), result.Text);
        }

        [Then(@"browser title is (.*)")]
        public void ThenBrowserTitleIs(string title)
        {
            var result = Wait.Until(d => d.Title);

            Assert.AreEqual(title, result);
        }

    }
}
