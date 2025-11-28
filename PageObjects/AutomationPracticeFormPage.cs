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
