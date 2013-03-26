using OpenQA.Selenium.Remote;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestApplication.UiTests
{
    public sealed class IERemoteWebDriver : OpenQA.Selenium.Remote.RemoteWebDriver
    {
        public IERemoteWebDriver(string url)
            : base(new Uri(url), DesiredCapabilities.InternetExplorer())
        {
            
        }
    }

    public sealed class ChromeRemoteWebDriver : OpenQA.Selenium.Remote.RemoteWebDriver
    {
        public ChromeRemoteWebDriver(string url)
            : base(new Uri(url), DesiredCapabilities.Chrome())
        {
            
        }
    }
}
