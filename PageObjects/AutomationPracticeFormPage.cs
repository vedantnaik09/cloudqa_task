using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System.Text.RegularExpressions;
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

        #region Last Name Field

        private IWebElement FindLastNameField()
        {
            return _elementFinder.FindElement(
                LocatorStrategy.ByLabel("Last Name"),
                LocatorStrategy.ByName("Last Name"),
                LocatorStrategy.ByPlaceholder("Last Name"),
                LocatorStrategy.ById("lname"),
                LocatorStrategy.ByXPath("//input[@type='text' and @class='form-control'][2]")
            );
        }

        public void EnterLastName(string lastName)
        {
            Console.WriteLine($"[LAST_NAME] Entering: {lastName}");
            var element = FindLastNameField();
            _elementFinder.SafeSendKeys(element, lastName);
        }

        public string GetLastNameValue()
        {
            var element = FindLastNameField();
            return element.GetAttribute("value") ?? string.Empty;
        }

        #endregion

        #region Gender Field

        public void SelectGender(string gender)
        {
            Console.WriteLine($"[GENDER] Selecting: {gender}");
            var xpath = $"//input[@type='radio' and @value='{gender}']";
            var element = _elementFinder.FindElement(
                LocatorStrategy.ByXPath(xpath),
                LocatorStrategy.ByXPath($"//label[contains(text(), '{gender}')]/preceding-sibling::input[@type='radio']")
            );
            _elementFinder.SafeClick(element);
        }

        public string GetSelectedGender()
        {
            var element = _driver.FindElement(By.XPath("//input[@type='radio' and @checked]"));
            return element.GetAttribute("value") ?? string.Empty;
        }

        #endregion

        #region Date of Birth Field

        private IWebElement FindDateOfBirthField()
        {
            return _elementFinder.FindElement(
                LocatorStrategy.ByLabel("Date of Birth"),
                LocatorStrategy.ByName("DateOfBirth"),
                LocatorStrategy.ById("dob"),
                LocatorStrategy.ByXPath("//input[@type='date']")
            );
        }

        public void EnterDateOfBirth(string date)
        {
            Console.WriteLine($"[DOB] Entering: {date}");
            var element = FindDateOfBirthField();
            _elementFinder.SafeSendKeys(element, date);
        }

        public string GetDateOfBirthValue()
        {
            var element = FindDateOfBirthField();
            return element.GetAttribute("value") ?? string.Empty;
        }

        #endregion

        #region Mobile Number Field

        private IWebElement FindMobileField()
        {
            return _elementFinder.FindElement(
                LocatorStrategy.ByLabel("Mobile #"),
                LocatorStrategy.ByName("Mobile Number"),
                LocatorStrategy.ByPlaceholder("Mobile Number"),
                LocatorStrategy.ById("mobile"),
                LocatorStrategy.ByXPath("//input[@type='text' and contains(@placeholder, 'Mobile')]")
            );
        }

        public void EnterMobileNumber(string mobile)
        {
            Console.WriteLine($"[MOBILE] Entering: {mobile}");
            var element = FindMobileField();
            _elementFinder.SafeSendKeys(element, mobile);
        }

        public string GetMobileNumberValue()
        {
            var element = FindMobileField();
            return element.GetAttribute("value") ?? string.Empty;
        }

        #endregion

        #region Country Field

        private IWebElement FindCountryField()
        {
            return _elementFinder.FindElement(
                LocatorStrategy.ByLabel("Country"),
                LocatorStrategy.ByName("Country"),
                LocatorStrategy.ById("country"),
                LocatorStrategy.ByXPath("//input[@type='text' and contains(@placeholder, 'Country')]")
            );
        }

        public void EnterCountry(string country)
        {
            Console.WriteLine($"[COUNTRY] Entering: {country}");
            var element = FindCountryField();
            _elementFinder.SafeSendKeys(element, country);
        }

        public string GetCountryValue()
        {
            var element = FindCountryField();
            return element.GetAttribute("value") ?? string.Empty;
        }

        #endregion

        #region Hobbies Field

        public void SelectHobby(string hobby)
        {
            Console.WriteLine($"[HOBBY] Selecting: {hobby}");
            var element = _elementFinder.FindElement(
                LocatorStrategy.ByXPath($"//input[@type='checkbox' and @name='{hobby}']"),
                LocatorStrategy.ByXPath($"//input[@type='checkbox' and @value='{hobby}']"),
                LocatorStrategy.ByXPath($"//label[contains(text(), '{hobby}')]/preceding-sibling::input[@type='checkbox']")
            );
            if (!element.Selected)
            {
                _elementFinder.SafeClick(element);
            }
        }

        public bool IsHobbySelected(string hobby)
        {
            var element = _driver.FindElement(By.XPath($"//input[@type='checkbox' and @value='{hobby}']"));
            return element.Selected;
        }

        #endregion

        #region About Yourself Field

        private IWebElement FindAboutField()
        {
            return _elementFinder.FindElement(
                LocatorStrategy.ByLabel("About Yourself"),
                LocatorStrategy.ByName("About"),
                LocatorStrategy.ById("about"),
                LocatorStrategy.ByXPath("//textarea[@class='form-control']")
            );
        }

        public void EnterAbout(string about)
        {
            Console.WriteLine($"[ABOUT] Entering: {about}");
            var element = FindAboutField();
            _elementFinder.SafeSendKeys(element, about);
        }

        public string GetAboutValue()
        {
            var element = FindAboutField();
            return element.GetAttribute("value") ?? string.Empty;
        }

        #endregion

        #region Username Field

        private IWebElement FindUsernameField()
        {
            return _elementFinder.FindElement(
                LocatorStrategy.ByName("Username"),
                LocatorStrategy.ById("username"),
                LocatorStrategy.ByXPath("//input[@type='text' and contains(@placeholder, 'Username')]"),
                LocatorStrategy.ByXPath("//label[contains(text(), 'Username')]/following::input[1]")
            );
        }

        public void EnterUsername(string username)
        {
            Console.WriteLine($"[USERNAME] Entering: {username}");
            var element = FindUsernameField();
            _elementFinder.SafeSendKeys(element, username);
        }

        public string GetUsernameValue()
        {
            var element = FindUsernameField();
            return element.GetAttribute("value") ?? string.Empty;
        }

        #endregion

        #region Password Fields

        private IWebElement FindPasswordField()
        {
            return _elementFinder.FindElement(
                LocatorStrategy.ByName("Password"),
                LocatorStrategy.ById("password"),
                LocatorStrategy.ByXPath("//input[@type='password'][1]"),
                LocatorStrategy.ByXPath("//label[contains(text(), 'Password')]/following::input[@type='password'][1]")
            );
        }

        public void EnterPassword(string password)
        {
            Console.WriteLine($"[PASSWORD] Entering: {password}");
            var element = FindPasswordField();
            _elementFinder.SafeSendKeys(element, password);
        }

        private IWebElement FindConfirmPasswordField()
        {
            return _elementFinder.FindElement(
                LocatorStrategy.ByName("Confirm Password"),
                LocatorStrategy.ById("confirmpassword"),
                LocatorStrategy.ByXPath("//input[@type='password'][2]"),
                LocatorStrategy.ByXPath("//label[contains(text(), 'Confirm Password')]/following::input[@type='password'][1]")
            );
        }

        public void EnterConfirmPassword(string confirmPassword)
        {
            Console.WriteLine($"[CONFIRM_PASSWORD] Entering: {confirmPassword}");
            var element = FindConfirmPasswordField();
            _elementFinder.SafeSendKeys(element, confirmPassword);
        }

        #endregion

        #region Terms and Conditions

        private IWebElement FindTermsCheckbox()
        {
            return _elementFinder.FindElement(
                LocatorStrategy.ById("Agree"),
                LocatorStrategy.ByName("Agree"),
                LocatorStrategy.ByXPath("//input[@type='checkbox' and @id='Agree']"),
                LocatorStrategy.ByXPath("//input[@type='checkbox' and contains(@value, 'Agree')]"),
                LocatorStrategy.ByXPath("//a[contains(text(), 'terms')]/preceding-sibling::input[@type='checkbox']")
            );
        }

        public void CheckTermsAndConditions()
        {
            Console.WriteLine("[TERMS] Checking terms and conditions");
            var element = FindTermsCheckbox();
            if (!element.Selected)
            {
                _elementFinder.SafeClick(element);
            }
        }

        public void UncheckTermsAndConditions()
        {
            Console.WriteLine("[TERMS] Unchecking terms and conditions");
            var element = FindTermsCheckbox();
            if (element.Selected)
            {
                _elementFinder.SafeClick(element);
            }
        }

        public bool IsTermsChecked()
        {
            var element = FindTermsCheckbox();
            return element.Selected;
        }

        /// <summary>
        /// Attempts to check a terms/agree checkbox inside a shadow host located by the given heading.
        /// Falls back to doing nothing if no checkbox is found in the shadow host.
        /// </summary>
        public void CheckShadowTermsIfPresent(string headingText)
        {
            Console.WriteLine($"[TERMS-SHADOW] Attempting to check terms inside shadow host: {headingText}");
            try
            {
                var el = FindElementInShadowByHeading(headingText, "input[type='checkbox'], input[name='Agree']");
                if (el != null && !el.Selected)
                {
                    _elementFinder.SafeClick(el);
                    Console.WriteLine("[TERMS-SHADOW] Checked shadow terms checkbox");
                }
            }
            catch (NoSuchElementException)
            {
                Console.WriteLine("[TERMS-SHADOW] No checkbox found in shadow host; skipping shadow terms check");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TERMS-SHADOW] Error checking shadow terms: {ex.Message}");
            }
        }

        /// <summary>
        /// Attempts to uncheck a terms/agree checkbox inside a shadow host located by the given heading.
        /// </summary>
        public void UncheckShadowTermsIfPresent(string headingText)
        {
            Console.WriteLine($"[TERMS-SHADOW] Attempting to uncheck terms inside shadow host: {headingText}");
            try
            {
                var el = FindElementInShadowByHeading(headingText, "input[type='checkbox'], input[name='Agree']");
                if (el != null && el.Selected)
                {
                    _elementFinder.SafeClick(el);
                    Console.WriteLine("[TERMS-SHADOW] Unchecked shadow terms checkbox");
                }
            }
            catch (NoSuchElementException)
            {
                Console.WriteLine("[TERMS-SHADOW] No checkbox found in shadow host; skipping shadow terms uncheck");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TERMS-SHADOW] Error unchecking shadow terms: {ex.Message}");
            }
        }

        #endregion

        #region Submit Response Validation

        /// <summary>
        /// Gets the submit response data displayed on the page after form submission
        /// Returns a dictionary with field names and their submitted values
        /// </summary>
        public Dictionary<string, string> GetSubmitResponseData()
        {
            Console.WriteLine("[RESPONSE] Parsing submit response data");
            var data = new Dictionary<string, string>();

            try
            {
                // Wait for the submit response to appear
                Thread.Sleep(2000); // Give page time to update
                
                var pageSource = _driver.PageSource;
                Console.WriteLine($"[RESPONSE] Page contains 'Submit data': {pageSource.Contains("Submit data")}");

                // Try multiple strategies to find the response
                IWebElement? responseElement = null;

                try
                {
                    responseElement = _driver.FindElement(By.XPath("//h2[contains(text(), 'Submit data')]/following::*[1]"));
                }
                catch { /* ignore */ }

                // If an element was found, try to parse its text
                if (responseElement != null)
                {
                    var responseText = responseElement.Text;
                    Console.WriteLine($"[RESPONSE] Raw response from element: {responseText}");
                    // Try regex-based parsing from the element text
                    ParseJsonResponse(responseText, data);
                    if (data.Count > 0) return data;
                }

                // If element parsing failed, try to extract a JSON block from the page source
                try
                {
                    var jsonBlocks = Regex.Matches(pageSource, "\\{[\\s\\S]*?\\}");
                    foreach (Match m in jsonBlocks)
                    {
                        var jsonText = m.Value;
                        // Heuristic: accept blocks that contain expected keys
                        if (jsonText.Contains("\"First Name\"") || jsonText.Contains("\"fname\"") || jsonText.Contains("\"lname\"") || jsonText.Contains("\"__RequestVerificationToken\""))
                        {
                            Console.WriteLine($"[RESPONSE] Found JSON block candidate: {jsonText}");
                            ParseJsonResponse(jsonText, data);
                            if (data.Count > 0) return data;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[RESPONSE] Error scanning for JSON blocks: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RESPONSE] Error parsing response: {ex.Message}");
            }

            return data;
        }

        private void ParseJsonResponse(string responseText, Dictionary<string, string> data)
        {
            try
            {
                // Find key/value pairs of the form "key": "value"
                var pairPattern = new Regex("\"(?<key>[^\"]+)\"\\s*:\\s*\"(?<value>[^\"]*)\"");
                var matches = pairPattern.Matches(responseText);
                foreach (Match m in matches)
                {
                    var key = m.Groups["key"].Value.Trim();
                    var value = m.Groups["value"].Value.Trim();
                    if (!string.IsNullOrWhiteSpace(key) && !key.StartsWith("__"))
                    {
                        data[key] = value;
                        Console.WriteLine($"[RESPONSE] Parsed: {key} = {value}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RESPONSE] Error parsing JSON response text: {ex.Message}");
            }
        }

        /// <summary>
        /// Checks if the submit response page is displayed
        /// This verifies that the form was actually submitted by checking for the response JSON data
        /// </summary>
        public bool IsSubmitResponseDisplayed()
        {
            try
            {
                // Give the form time to submit and page to update
                Thread.Sleep(1500);
                
                var pageSource = _driver.PageSource;
                
                // Check if the page contains the JSON response structure with actual data
                // The response should have the format: { "First Name": "value", ...
                // Look for the specific structure that appears after submission
                bool hasSubmitHeading = pageSource.Contains("<h2>Submit data</h2>") || 
                                       pageSource.Contains(">Submit data<");
                
                // Existing strict check (old format)
                bool hasJsonResponse = pageSource.Contains("\"First Name:\"") && 
                                      pageSource.Contains("\"Last Name:\"") &&
                                      pageSource.Contains("\"__RequestVerificationToken\":");

                // Flexible detection for other response formats (e.g., shadow DOM uses keys like "fname"/"lname")
                if (!hasJsonResponse)
                {
                    // Look for common field keys that may appear in alternative JSON payloads
                    if (pageSource.Contains("\"fname\"") || pageSource.Contains("\"lname\"") || pageSource.Contains("\"State\"") || pageSource.Contains("\"Email\"") || pageSource.Contains("\"__RequestVerificationToken\""))
                    {
                        hasJsonResponse = true;
                    }
                }

                // Regex fallback: detect at least two JSON-like key:value pairs somewhere in the page
                if (!hasJsonResponse)
                {
                    try
                    {
                        var jsonPattern = new Regex("\"[^\"]+\"\\s*:\\s*\"[^\"]*\"");
                        var matches = jsonPattern.Matches(pageSource);
                        if (matches != null && matches.Count >= 2)
                        {
                            hasJsonResponse = true;
                        }
                    }
                    catch { /* ignore regex failures */ }
                }
                
                // The form must have BOTH the heading AND the JSON response
                if (hasSubmitHeading && hasJsonResponse)
                {
                    Console.WriteLine("[RESPONSE] Submit response detected with JSON data");
                    return true;
                }
                
                // Additional check: Look for the pre/code element that contains the JSON
                var jsonElements = _driver.FindElements(By.XPath("//h2[text()='Submit data']/following-sibling::*[1]"));
                if (jsonElements.Count > 0)
                {
                    var jsonText = jsonElements[0].Text;
                    // Verify it's actual JSON data, not empty
                    if (jsonText.Contains("\"First Name\"") && jsonText.Length > 50)
                    {
                        Console.WriteLine("[RESPONSE] Submit response detected via element check");
                        return true;
                    }
                }
                
                Console.WriteLine("[RESPONSE] Submit response NOT detected - form likely did not submit");
                Console.WriteLine($"[RESPONSE] Has heading: {hasSubmitHeading}, Has JSON: {hasJsonResponse}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RESPONSE] Error checking submit response: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Checks if the form has any HTML5 validation errors preventing submission
        /// </summary>
        public bool HasFormValidationErrors()
        {
            try
            {
                var jsExecutor = (IJavaScriptExecutor)_driver;
                var isValid = (bool)jsExecutor.ExecuteScript(
                    "var form = document.querySelector('form'); " +
                    "return form ? form.checkValidity() : true;");
                return !isValid;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets validation message from a specific field
        /// </summary>
        public string GetFieldValidationMessage(IWebElement element)
        {
            try
            {
                return element.GetAttribute("validationMessage") ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

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
