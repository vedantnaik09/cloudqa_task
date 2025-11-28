using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System.IO;

namespace CloudQAAutomation.Helpers
{
    /// <summary>
    /// Finds elements using multiple fallback strategies to ensure tests remain stable
    /// even when HTML attributes change
    /// </summary>
    public class RobustElementFinder
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;
        private readonly TimeSpan _timeout;

        public RobustElementFinder(IWebDriver driver, TimeSpan? timeout = null)
        {
            _driver = driver;
            _timeout = timeout ?? TimeSpan.FromSeconds(10);
            _wait = new WebDriverWait(_driver, _timeout);
        }

        /// <summary>
        /// Finds an element using multiple strategies with fallback
        /// </summary>
        public IWebElement FindElement(params LocatorStrategy[] strategies)
        {
            var exceptions = new List<Exception>();

            foreach (var strategy in strategies)
            {
                try
                {
                    Console.WriteLine($"[LOCATOR] Trying strategy: {strategy.Description}");
                    var element = TryFindElement(strategy);
                    
                    if (element != null && IsElementInteractable(element))
                    {
                        Console.WriteLine($"[LOCATOR] ✓ Success with: {strategy.Description}");
                        return element;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[LOCATOR] ✗ Failed: {strategy.Description} - {ex.Message}");
                    exceptions.Add(ex);
                }
            }

            try
            {
                // Attempt to capture a screenshot for debugging before failing
                var strategyNames = string.Join("_", strategies.Select(s => s.Description.Replace(" ", "_").Replace(":", "")));
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                var fileName = $"element_not_found_{timestamp}_{strategyNames}.png";
                SaveScreenshot(fileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SCREENSHOT] Failed to capture screenshot: {ex.Message}");
            }

            throw new NoSuchElementException(
                $"Element not found using any of {strategies.Length} strategies.\n" +
                $"Strategies tried: {string.Join(", ", strategies.Select(s => s.Description))}\n" +
                $"Last error: {exceptions.LastOrDefault()?.Message}");

        }

        private IWebElement? TryFindElement(LocatorStrategy strategy)
        {
            return strategy.Type switch
            {
                LocatorType.LabelText => FindByLabelText(strategy.Value),
                LocatorType.Id => _wait.Until(d => d.FindElement(By.Id(strategy.Value))),
                LocatorType.Name => _wait.Until(d => d.FindElement(By.Name(strategy.Value))),
                LocatorType.Placeholder => _wait.Until(d => d.FindElement(
                    By.XPath($"//input[@placeholder='{strategy.Value}'] | //textarea[@placeholder='{strategy.Value}']"))),
                LocatorType.XPath => _wait.Until(d => d.FindElement(By.XPath(strategy.Value))),
                LocatorType.CssSelector => _wait.Until(d => d.FindElement(By.CssSelector(strategy.Value))),
                LocatorType.PartialText => _wait.Until(d => d.FindElement(
                    By.XPath($"//*[contains(@*, '{strategy.Value}')]"))),
                _ => throw new ArgumentException($"Unknown locator type: {strategy.Type}")
            };
        }

        /// <summary>
        /// Finds element by its associated label text (most stable strategy)
        /// </summary>
        private IWebElement FindByLabelText(string labelText)
        {
            // Strategy 1: Label with 'for' attribute
            try
            {
                var label = _wait.Until(d => d.FindElement(
                    By.XPath($"//label[contains(normalize-space(text()), '{labelText}')]")));
                
                var forAttribute = label.GetAttribute("for");
                if (!string.IsNullOrEmpty(forAttribute))
                {
                    return _driver.FindElement(By.Id(forAttribute));
                }
            }
            catch { }

            // Strategy 2: Input following the label
            try
            {
                return _wait.Until(d => d.FindElement(
                    By.XPath($"//label[contains(normalize-space(text()), '{labelText}')]/following::input[1]")));
            }
            catch { }

            // Strategy 3: Input within same parent as label
            try
            {
                return _wait.Until(d => d.FindElement(
                    By.XPath($"//label[contains(normalize-space(text()), '{labelText}')]/..//input | " +
                            $"//label[contains(normalize-space(text()), '{labelText}')]/..//select | " +
                            $"//label[contains(normalize-space(text()), '{labelText}')]/..//textarea")));
            }
            catch { }

            throw new NoSuchElementException($"Could not find element by label text: {labelText}");
        }

        /// <summary>
        /// Checks if element is actually interactable
        /// </summary>
        private bool IsElementInteractable(IWebElement element)
        {
            try
            {
                return element.Displayed && element.Enabled;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Waits for element to be clickable
        /// </summary>
        public IWebElement WaitForElementClickable(By locator)
        {
            return _wait.Until(ExpectedConditions.ElementToBeClickable(locator));
        }

        /// <summary>
        /// Safe click with retry mechanism
        /// </summary>
        public void SafeClick(IWebElement element, int maxRetries = 3)
        {
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    _wait.Until(d => element.Displayed && element.Enabled);
                    element.Click();
                    return;
                }
                catch (Exception ex) when (i < maxRetries - 1)
                {
                    Console.WriteLine($"[CLICK] Retry {i + 1}/{maxRetries}: {ex.Message}");
                    Thread.Sleep(500);
                }
            }
        }

        /// <summary>
        /// Safe send keys with clear and retry
        /// </summary>
        public void SafeSendKeys(IWebElement element, string text, int maxRetries = 3)
        {
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    _wait.Until(d => element.Displayed && element.Enabled);
                    element.Clear();
                    element.SendKeys(text);
                    
                    // Verify the text was entered
                    if (element.GetAttribute("value") == text)
                    {
                        return;
                    }
                }
                catch (Exception ex) when (i < maxRetries - 1)
                {
                    Console.WriteLine($"[SEND_KEYS] Retry {i + 1}/{maxRetries}: {ex.Message}");
                    Thread.Sleep(500);
                }
            }
        }

        /// <summary>
        /// Saves a screenshot to `logs/screenshots` with the provided filename.
        /// </summary>
        private void SaveScreenshot(string fileName)
        {
            try
            {
                var dir = Path.Combine("logs", "screenshots");
                Directory.CreateDirectory(dir);

                var filePath = Path.Combine(dir, fileName);

                if (_driver is ITakesScreenshot ts)
                {
                    var screenshot = ts.GetScreenshot();
                    // Use byte array write to avoid dependency on ScreenshotImageFormat symbol
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

    /// <summary>
    /// Represents a locator strategy with description for logging
    /// </summary>
    public class LocatorStrategy
    {
        public string Description { get; }
        public string Value { get; }
        public LocatorType Type { get; }

        public LocatorStrategy(string description, string value, LocatorType type)
        {
            Description = description;
            Value = value;
            Type = type;
        }

        /// <summary>
        /// Factory methods for common strategies
        /// </summary>
        public static LocatorStrategy ByLabel(string labelText) => 
            new("Label Text", labelText, LocatorType.LabelText);
        
        public static LocatorStrategy ById(string id) => 
            new($"ID: {id}", id, LocatorType.Id);
        
        public static LocatorStrategy ByName(string name) => 
            new($"Name: {name}", name, LocatorType.Name);
        
        public static LocatorStrategy ByPlaceholder(string placeholder) => 
            new($"Placeholder: {placeholder}", placeholder, LocatorType.Placeholder);
        
        public static LocatorStrategy ByXPath(string xpath) => 
            new($"XPath", xpath, LocatorType.XPath);
        
        public static LocatorStrategy ByCss(string css) => 
            new($"CSS: {css}", css, LocatorType.CssSelector);
    }

    public enum LocatorType
    {
        LabelText,
        Id,
        Name,
        Placeholder,
        XPath,
        CssSelector,
        PartialText
    }
}
