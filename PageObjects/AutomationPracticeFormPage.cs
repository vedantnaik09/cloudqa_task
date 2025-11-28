using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using CloudQAAutomation.Helpers;

namespace CloudQAAutomation.PageObjects
{
    /// <summary>
    /// Page Object for CloudQA Automation Practice Form
    /// Uses dynamic HTML analysis and multi-strategy locators for resilience
    /// </summary>
    public class AutomationPracticeFormPage
    {
        private readonly IWebDriver _driver;
        private readonly RobustElementFinder _elementFinder;
        private const string PageUrl = "https://app.cloudqa.io/home/AutomationPracticeForm";

        public AutomationPracticeFormPage(IWebDriver driver)
        {
            _driver = driver;
            _elementFinder = new RobustElementFinder(driver);
        }

        #region Navigation

        public void Navigate()
        {
            _driver.Navigate().GoToUrl(PageUrl);
            WaitForPageLoad();
        }

        private void WaitForPageLoad()
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));
        }

        #endregion

        #region Shadow DOM Helpers

        /// <summary>
        /// Finds the <shadow-form> host that follows a heading with the specified text.
        /// </summary>
        public IWebElement FindShadowHostByHeading(string headingText)
        {
            var xpath = $"//h1[normalize-space(text())='{headingText}']/following::shadow-form[1]";
            Console.WriteLine($"[SHADOW] Locating shadow host by heading: {headingText}");
            return _driver.FindElement(By.XPath(xpath));
        }

        /// <summary>
        /// Finds an element inside the shadow root of the shadow-form host located after the given heading.
        /// Tries Selenium 4 shadow root API first, falls back to JS querySelector if needed.
        /// </summary>
        public IWebElement FindElementInShadowByHeading(string headingText, string cssSelector)
        {
            var host = FindShadowHostByHeading(headingText);

            // First try the host's light DOM children (slotted content is often placed here)
            try
            {
                var hostChild = host.FindElements(By.CssSelector(cssSelector)).FirstOrDefault();
                if (hostChild != null)
                {
                    Console.WriteLine($"[SHADOW] Found element as host child (light DOM): {cssSelector}");
                    return hostChild;
                }
            }
            catch { /* ignore */ }

            // Next try Selenium shadow root API
            try
            {
                var shadowRoot = host.GetShadowRoot();
                var el = shadowRoot.FindElements(By.CssSelector(cssSelector)).FirstOrDefault();
                if (el != null)
                {
                    Console.WriteLine($"[SHADOW] Found element in shadow root using Selenium API: {cssSelector}");
                    return el;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SHADOW] Selenium shadow API lookup failed: {ex.Message}. Trying JS fallback for selector: {cssSelector}");
            }

            // Finally try JS fallback that queries shadowRoot or host
            try
            {
                var js = (IJavaScriptExecutor)_driver;
                var result = js.ExecuteScript("return (arguments[0].shadowRoot || arguments[0]).querySelector(arguments[1])", host, cssSelector);
                if (result == null) throw new NoSuchElementException($"Element not found in shadow host: {cssSelector}");
                Console.WriteLine($"[SHADOW] Found element in shadow root using JS fallback: {cssSelector}");
                return (IWebElement)result;
            }
            catch (Exception ex)
            {
                throw new NoSuchElementException($"Element not found in shadow host (all strategies): {cssSelector}. Details: {ex.Message}");
            }
        }

        public void EnterShadowFirstName(string headingText, string value)
        {
            // Prefer the fname slot first to avoid matching the generic placeholder
            var el = FindElementInShadowByHeading(headingText, "section[slot='fname'] input, input#fname, input[name='fname']");
            _elementFinder.SafeSendKeys(el, value);
        }

        public string GetShadowFirstNameValue(string headingText)
        {
            var el = FindElementInShadowByHeading(headingText, "section[slot='fname'] input, input#fname, input[name='fname']");
            return el.GetAttribute("value") ?? string.Empty;
        }

        public void EnterShadowLastName(string headingText, string value)
        {
            // Prefer the lname slot first to avoid accidentally matching the first name input
            var el = FindElementInShadowByHeading(headingText, "section[slot='lname'] input, input#lname, input[name='lname']");
            _elementFinder.SafeSendKeys(el, value);
        }

        public string GetShadowLastNameValue(string headingText)
        {
            var el = FindElementInShadowByHeading(headingText, "section[slot='lname'] input, input#lname, input[name='lname']");
            return el.GetAttribute("value") ?? string.Empty;
        }

        public void SelectShadowState(string headingText, string visibleText)
        {
            var el = FindElementInShadowByHeading(headingText, "select#state, select[name='State']");
            var select = new SelectElement(el);
            try { select.SelectByText(visibleText); }
            catch { select.SelectByValue(visibleText); }
        }

        public string GetSelectedShadowState(string headingText)
        {
            var el = FindElementInShadowByHeading(headingText, "select#state, select[name='State']");
            var select = new SelectElement(el);
            return select.SelectedOption.Text;
        }

        #endregion

        #region IFrame Helpers

        /// <summary>
        /// Switches driver's context into an iframe by its `id` attribute.
        /// Falls back to locating the iframe element by XPath if direct id lookup fails.
        /// </summary>
        public void SwitchToIframeById(string id)
        {
            Console.WriteLine($"[IFRAME] Switching to iframe by id: {id}");
            var iframe = _elementFinder.FindElement(
                LocatorStrategy.ById(id),
                LocatorStrategy.ByXPath($"//iframe[@id='{id}']")
            );

            _driver.SwitchTo().Frame(iframe);
            WaitForPageLoad();
        }

        /// <summary>
        /// Switches into the first iframe that follows a heading with the provided text.
        /// Useful for iframes without stable attributes — we locate them by nearby visible content.
        /// </summary>
        public void SwitchToIframeContainingHeading(string headingText)
        {
            Console.WriteLine($"[IFRAME] Switching to iframe following heading: {headingText}");
            var iframe = _elementFinder.FindElement(
                LocatorStrategy.ByXPath($"//h1[contains(normalize-space(text()), '{headingText}')]/following::iframe[1]"),
                LocatorStrategy.ByXPath($"//h2[contains(normalize-space(text()), '{headingText}')]/following::iframe[1]"),
                LocatorStrategy.ByXPath($"//label[contains(normalize-space(text()), '{headingText}')]/following::iframe[1]")
            );

            _driver.SwitchTo().Frame(iframe);
            WaitForPageLoad();
        }

        /// <summary>
        /// Returns driver to the top-level browsing context.
        /// </summary>
        public void SwitchToDefaultContent()
        {
            Console.WriteLine("[IFRAME] Switching back to default content");
            _driver.SwitchTo().DefaultContent();
            WaitForPageLoad();
        }

        #endregion

        #region Dynamic HTML Analysis

        // HtmlAnalyzer removed for production; dynamic analysis methods intentionally omitted.

        #endregion

        #region First Name Field

        /// <summary>
        /// Locates First Name field using multiple resilient strategies
        /// Strategy Priority:
        /// 1. Label text (most stable - business logic)
        /// 2. Name attribute (semantic identifier)
        /// 3. Placeholder text (user-facing hint)
        /// 4. ID (least stable - can change)
        /// </summary>
        private IWebElement FindFirstNameField()
        {
            return _elementFinder.FindElement(
                LocatorStrategy.ByLabel("First Name"),
                LocatorStrategy.ByName("First Name"),
                LocatorStrategy.ByPlaceholder("Name"),
                LocatorStrategy.ById("fname"),
                LocatorStrategy.ByXPath("//input[@type='text' and @class='form-control'][1]")
            );
        }

        public void EnterFirstName(string firstName)
        {
            Console.WriteLine($"[FIRST_NAME] Entering: {firstName}");
            var element = FindFirstNameField();
            _elementFinder.SafeSendKeys(element, firstName);
        }

        public string GetFirstNameValue()
        {
            var element = FindFirstNameField();
            return element.GetAttribute("value") ?? string.Empty;
        }

        public void ClearFirstName()
        {
            var element = FindFirstNameField();
            element.Clear();
        }

        #endregion

        #region Email Field

        /// <summary>
        /// Locates Email field using multiple resilient strategies
        /// Strategy Priority:
        /// 1. Label text "Email"
        /// 2. Name attribute
        /// 3. Placeholder
        /// 4. ID
        /// 5. Type attribute with text matching
        /// </summary>
        private IWebElement FindEmailField()
        {
            return _elementFinder.FindElement(
                LocatorStrategy.ByLabel("Email"),
                LocatorStrategy.ByName("Email"),
                LocatorStrategy.ByPlaceholder("Email"),
                LocatorStrategy.ById("email"),
                LocatorStrategy.ByXPath("//input[@type='text' and contains(@placeholder, 'Email')]"),
                LocatorStrategy.ByXPath("//label[contains(text(), 'Email')]/following::input[1]")
            );
        }

        public void EnterEmail(string email)
        {
            Console.WriteLine($"[EMAIL] Entering: {email}");
            var element = FindEmailField();
            _elementFinder.SafeSendKeys(element, email);
        }

        public string GetEmailValue()
        {
            var element = FindEmailField();
            return element.GetAttribute("value") ?? string.Empty;
        }

        public void ClearEmail()
        {
            var element = FindEmailField();
            element.Clear();
        }

        /// <summary>
        /// Validates if entered email has proper format
        /// </summary>
        public bool IsEmailValid()
        {
            var element = FindEmailField();
            var validationMessage = element.GetAttribute("validationMessage");
            return string.IsNullOrEmpty(validationMessage);
        }

        #endregion

        #region State Dropdown

        /// <summary>
        /// Locates State dropdown using multiple resilient strategies
        /// Strategy Priority:
        /// 1. Label text "State"
        /// 2. Name attribute
        /// 3. ID
        /// 4. Element type (select with form-control class)
        /// </summary>
        private IWebElement FindStateDropdown()
        {
            return _elementFinder.FindElement(
                LocatorStrategy.ByLabel("State"),
                LocatorStrategy.ByName("State"),
                LocatorStrategy.ById("state"),
                LocatorStrategy.ByXPath("//select[@class='form-control']"),
                LocatorStrategy.ByXPath("//label[contains(text(), 'State')]/..//select")
            );
        }

        public void SelectState(string stateName)
        {
            Console.WriteLine($"[STATE] Selecting: {stateName}");
            var element = FindStateDropdown();
            var select = new SelectElement(element);
            
            // Try by visible text first, then by value
            try
            {
                select.SelectByText(stateName);
            }
            catch
            {
                select.SelectByValue(stateName);
            }
        }

        public void SelectStateByIndex(int index)
        {
            Console.WriteLine($"[STATE] Selecting by index: {index}");
            var element = FindStateDropdown();
            var select = new SelectElement(element);
            select.SelectByIndex(index);
        }

        public string GetSelectedState()
        {
            var element = FindStateDropdown();
            var select = new SelectElement(element);
            return select.SelectedOption.Text;
        }

        public List<string> GetAllStateOptions()
        {
            var element = FindStateDropdown();
            var select = new SelectElement(element);
            return select.Options.Select(o => o.Text).ToList();
        }

        #endregion

        #region Form Submission

        public void SubmitForm()
        {
            var submitButton = _elementFinder.FindElement(
                LocatorStrategy.ByXPath("//button[@type='submit' and contains(text(), 'Submit')]"),
                LocatorStrategy.ByCss("button.btn-primary[type='submit']")
            );
            _elementFinder.SafeClick(submitButton);
        }

        public void ResetForm()
        {
            var resetButton = _elementFinder.FindElement(
                LocatorStrategy.ByXPath("//button[@type='reset']"),
                LocatorStrategy.ByCss("button[type='reset']")
            );
            _elementFinder.SafeClick(resetButton);
        }

        #endregion
    }
}
