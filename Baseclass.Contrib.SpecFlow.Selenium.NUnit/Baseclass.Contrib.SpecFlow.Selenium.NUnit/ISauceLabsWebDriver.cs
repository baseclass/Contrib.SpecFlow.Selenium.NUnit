using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Baseclass.Contrib.SpecFlow.Selenium.NUnit
{
    /// <summary>
    /// Webdriver with utility methods to modify test results on SauceLabs.com
    /// </summary>
    public interface ISauceLabsWebDriver
    {
        /// <summary>
        /// Uses SauceLabs REST API to update the test result on sauce labs.
        /// </summary>
        /// <param name="name">
        /// The name of the test which should be updated.
        /// </param>
        /// <param name="status">
        /// If the test failed or succeeded.
        /// </param>
        void UpdateSauceLabsResult(string name, bool status);

        /// <summary>
        /// Gets the current SessionId. Useful to access SauceLabs REST API.
        /// </summary>
        string SessionId { get; }

        /// <summary>
        /// Gets the username to authenticate against the SauceLabs REST API.
        /// </summary>
        string UserName { get; }

        /// <summary>
        /// Gets the key to authenticate against the SauceLabs REST API.
        /// </summary>
        string AccessKey { get; }
    }
}
