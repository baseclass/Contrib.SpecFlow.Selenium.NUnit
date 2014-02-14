using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Baseclass.Contrib.SpecFlow.Selenium.NUnit
{
    public class SauceLabsWebDriver : RemoteWebDriver, ISauceLabsWebDriver
    {
        /// <summary>
        /// Gets the username to authenticate against the SauceLabs REST API.
        /// </summary>
        public string UserName { get; private set; }

        /// <summary>
        /// Gets the key to authenticate against the SauceLabs REST API.
        /// </summary>
        public string AccessKey { get; private set; }

        /// <summary>
        /// Creates a new instance of <see cref="SauceLabsRemoteWebDriver"/>
        /// </summary>
        /// <param name="url">
        /// Url pointing to the SauceLabs selenium web server
        /// </param>
        /// <param name="browser">
        /// Name of the browser to use for testing
        /// </param>
        /// <param name="userName">
        /// SauceLabs userName
        /// </param>
        /// <param name="accessKey">
        /// SauceLabs accessKey
        /// </param>
        /// <param name="capabilities">
        /// Capabilities to set on the browsers desired capabilities
        /// </param>
        public SauceLabsWebDriver(string url, string browser, string userName, string accessKey, Dictionary<string, string> capabilities)
            : base(url, browser, AddUserNameAndAccessKey(userName, accessKey, capabilities))
        {
            // Grab username and accessKey as they can't be found in the base.Capabilities collection.
            this.UserName = userName;
            this.AccessKey = accessKey;
        }

        /// <summary>
        /// Adds the <paramref name="userName"/> and <paramref name="accessKey"/> to the <paramref name="capabilities"/> dictionary.
        /// </summary>
        /// <param name="userName">
        /// SauceLabs userName
        /// </param>
        /// <param name="accessKey">
        /// SauceLabs accessKey
        /// </param>
        /// <param name="capabilities">
        /// Capabilities to set on the browsers desired capabilities
        /// </param>
        /// <returns>
        /// The modified <paramref name="capabilities"/>.
        /// </returns>
        private static Dictionary<string, string> AddUserNameAndAccessKey(string userName, string accessKey, Dictionary<string, string> capabilities)
        {
            capabilities.Add("username", userName);
            capabilities.Add("accessKey", accessKey);

            return capabilities;
        }

        /// <summary>
        /// Uses SauceLabs REST API to update the test result on sauce labs.
        /// </summary>
        /// <param name="name">
        /// The name of the test which should be updated.
        /// </param>
        /// <param name="buildNumber">
        /// The build number.
        /// </param>
        /// <param name="status">
        /// If the test failed or succeeded.
        /// </param>
        public void UpdateSauceLabsResult(string name, string buildNumber, bool status)
        {
            var webclient = new WebClient();

            webclient.Headers.Add("Content-Type", "text/json");

            var address = new Uri(string.Format("http://saucelabs.com/rest/v1/{0}/jobs/{1}", this.UserName, this.SessionId));

            webclient.Credentials = new NetworkCredential(this.UserName, this.AccessKey);

            var update = string.Format("{{\"name\": \"{0}\", \"build\": \"{1}\", \"passed\": {2}}}", name, buildNumber, status.ToString().ToLower());

            webclient.UploadString(address, "PUT", update);
        }

        /// <summary>
        /// Gets the current SessionId. Useful to access SauceLabs REST API.
        /// </summary>
        string ISauceLabsWebDriver.SessionId
        {
            get { return this.SessionId.ToString(); }
        }
    }
}
