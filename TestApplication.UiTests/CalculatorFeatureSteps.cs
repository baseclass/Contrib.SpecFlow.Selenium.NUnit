using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using TechTalk.SpecFlow;

namespace TestApplication.UiTests
{
    [Binding]
    public class CalculatorFeatureSteps : BrowserSetup
    {
        private WebDriverWait wait;
        public WebDriverWait Wait
        {
            get
            {
                if (wait == null)
                {
                    this.wait = new WebDriverWait(this.Driver, TimeSpan.FromSeconds(10));
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
    }
}
