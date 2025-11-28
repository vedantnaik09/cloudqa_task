using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.IO;

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

        /// <summary>
        /// Saves a screenshot for the current test to `logs/screenshots`.
        /// </summary>
        protected void SaveScreenshot(string testName)
        {
            try
            {
                // Resolve repository root by walking up from the test assembly directory
                string? repoRoot = null;
                try
                {
                    var dirInfo = new DirectoryInfo(AppContext.BaseDirectory);
                    while (dirInfo != null)
                    {
                        // identify repo root by presence of the project file
                        if (File.Exists(Path.Combine(dirInfo.FullName, "CloudQAAutomation.csproj")))
                        {
                            repoRoot = dirInfo.FullName;
                            break;
                        }
                        dirInfo = dirInfo.Parent;
                    }
                }
                catch { repoRoot = null; }

                // fallback to current directory if repo root not found
                var logsRoot = repoRoot ?? Directory.GetCurrentDirectory();
                var dir = Path.Combine(logsRoot, "logs", "screenshots");
                Directory.CreateDirectory(dir);

                var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                // sanitize testName for file system
                var safeName = string.Concat(testName.Split(Path.GetInvalidFileNameChars()));
                var fileName = $"{safeName}_{timestamp}.png";
                var filePath = Path.Combine(dir, fileName);

                if (Driver is ITakesScreenshot ts)
                {
                    var screenshot = ts.GetScreenshot();
                    // write bytes directly to avoid depending on ScreenshotImageFormat symbol
                    File.WriteAllBytes(filePath, screenshot.AsByteArray);
                    Console.WriteLine($"[SCREENSHOT] Saved to: {filePath}");
                }
                else
                {
                    Console.WriteLine("[SCREENSHOT] Driver does not support screenshots.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SCREENSHOT] Error saving screenshot: {ex.Message}");
            }
        }
    }
}
