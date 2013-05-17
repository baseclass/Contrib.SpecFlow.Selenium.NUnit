using OpenQA.Selenium.Remote;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Baseclass.Contrib.SpecFlow.Selenium.NUnit
{
    /// <summary>
    /// Extension to <see cref="OpenQA.Selenium.Remote.RemoteWebDriver"/> allowing the instantiation with two strings
    /// Simplifies the xml configuration of an IOC Container
    /// Uses reflection to retrieve the <see cref="DesiredCapabilities"/> static method to create the DesiredCapabilities for a specific browser.
    /// </summary>
    public class RemoteWebDriver : OpenQA.Selenium.Remote.RemoteWebDriver
    {
        /// <summary>
        /// Creates a new instance of <see cref="RemoteWebDriver"/>
        /// Retrieves the <see cref="DesiredCapabilities"/> by calling the static method on <see cref="DesiredCapabilities"/>
        /// with the same name as <paramref name="browser"/>
        /// Example: Calls DesiredCapabilites.InternetExplorer() if <paramref name="browser"/> is specified as InternetExplorer
        /// </summary>
        /// <param name="url">
        /// Url pointing to the Selenium web server
        /// </param>
        /// <param name="browser">
        /// Name of the browser to use for testing
        /// </param>
        public RemoteWebDriver(string url, string browser)
            : base(new Uri(url), GetCapabilities(browser))
        {

        }

        /// <summary>
        /// Creates a new instance of <see cref="RemoteWebDriver"/>
        /// Retrieves the <see cref="DesiredCapabilities"/> by calling the static method on <see cref="DesiredCapabilities"/>
        /// with the same name as <paramref name="browser"/>
        /// Example: Calls DesiredCapabilites.InternetExplorer() if <paramref name="browser"/> is specified as InternetExplorer
        /// </summary>
        /// <param name="url">
        /// Url pointing to the Selenium web server
        /// </param>
        /// <param name="browser">
        /// Name of the browser to use for testing
        /// </param>
        /// <param name="capabilities">
        /// Capabilities to set on the browsers desired capabilities
        /// </param>
        public RemoteWebDriver(string url, string browser, Dictionary<string, string> capabilities)
            : base(new Uri(url), GetCapabilities(browser, capabilities))
        {

        }


        /// <summary>
        /// Uses reflection to create an instance of <see cref="DesiredCapabilities"/>
        /// </summary>
        /// <param name="browser">
        /// Name of the browser to use for testing
        /// </param>
        /// <returns>
        /// Instance of DesiredCapabilities describing the browser
        /// </returns>
        private static DesiredCapabilities GetCapabilities(string browserName, Dictionary<string, string> additionalCapabilities = null)
        {
            var capabilityCreationMethod = typeof(DesiredCapabilities)
                .GetMethod(browserName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);


            if (capabilityCreationMethod == null)
            {
                throw new NotSupportedException("Can't find DesiredCapabilities with name " + browserName);
            }

            var capabilities = capabilityCreationMethod.Invoke(null, null) as DesiredCapabilities;

            if (capabilities == null)
            {
                throw new NotSupportedException("Can't find DesiredCapabilities with name " + browserName);
            }

            if (additionalCapabilities != null)
            {
                foreach (var capability in additionalCapabilities)
                {
                    capabilities.SetCapability(capability.Key, capability.Value);
                }
            }

            return capabilities;
        }
    }
}
