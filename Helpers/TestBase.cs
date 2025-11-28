using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace CloudQAAutomation.Helpers
{
    /// <summary>
    /// Base class for all tests providing common setup and teardown
    /// </summary>
    public class TestBase
    {
        protected IWebDriver? Driver;
        protected RobustElementFinder? ElementFinder;

        protected void InitializeDriver()
        {
            var options = new ChromeOptions();
            
            // Uncomment for headless mode
            // options.AddArgument("--headless=new");
            
            options.AddArgument("--start-maximized");
            options.AddArgument("--disable-notifications");
            options.AddArgument("--disable-popup-blocking");
            options.AddArgument("--disable-blink-features=AutomationControlled");
            
            Driver = new ChromeDriver(options);
            Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            Driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(30);
            
            ElementFinder = new RobustElementFinder(Driver);
        }

        protected void CleanupDriver()
        {
            try
            {
                Driver?.Quit();
                Driver?.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during driver cleanup: {ex.Message}");
            }
        }
    }
}
