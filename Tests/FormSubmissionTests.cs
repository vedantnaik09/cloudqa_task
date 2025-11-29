using NUnit.Framework;
using CloudQAAutomation.PageObjects;
using CloudQAAutomation.Helpers;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace CloudQAAutomation.Tests
{
    /// <summary>
    /// Comprehensive Form Submission Tests for CloudQA Automation Practice Form
    /// 
    /// This test suite validates form submission behavior across all form levels:
    /// - Main Document (default page)
    /// - IFrame without ID (located by heading)
    /// - IFrame with ID (iframeId)
    /// - Shadow DOM form
    /// 
    /// Test Categories:
    /// - FormSubmission: All submission tests
    /// - Submit_Main: Main document form submissions
    /// - Submit_IFrameNoId: IFrame without ID submissions
    /// - Submit_IFrameWithId: IFrame with ID submissions
    /// - Submit_ShadowDOM: Shadow DOM form submissions
    /// 
    /// Test Types:
    /// - Positive Tests: Valid data that should be accepted
    /// - Negative Tests: Invalid data that should be rejected (reveals validation bugs)
    /// </summary>
    [TestFixture]
    [Category("FormSubmission")]
    public class FormSubmissionTests : TestBase
    {
        private AutomationPracticeFormPage? _page;

        [SetUp]
        public void Setup()
        {
            InitializeDriver(); // Initialize driver from TestBase
            Console.WriteLine("=== Test Setup ===");
            _page = new AutomationPracticeFormPage(Driver!);
            _page.Navigate();
            Console.WriteLine("=== Setup Complete ===\n");
        }

        [TearDown]
        public void Teardown()
        {
            Console.WriteLine("\n=== Test Teardown ===");
            SaveScreenshot(TestContext.CurrentContext.Test.MethodName!);
            CleanupDriver(); // Cleanup driver from TestBase
            Console.WriteLine("=== Teardown Complete ===\n");
        }

        #region Main Document - Form Submission Tests

        [Test, Order(100)]
        [Category("Submit_Main")]
        [Category("PositiveTest")]
        public void Test_100_Main_Submit_CompleteValidForm()
        {
            // Arrange
            Console.WriteLine("\n[MAIN] Test: Submit complete valid form with all fields filled");
            var testData = new
            {
                FirstName = "John",
                LastName = "Doe",
                Gender = "Male",
                DateOfBirth = "1990-05-15",
                Mobile = "1234567890",
                Email = "john.doe@example.com",
                Country = "United States",
                State = "United States",
                Hobby = "Dance",
                About = "This is a test automation submission for CloudQA practice form.",
                Username = "johndoe123",
                Password = "SecurePass@123",
                ConfirmPassword = "SecurePass@123"
            };

            // Act - Fill all form fields
            _page!.EnterFirstName(testData.FirstName);
            _page.EnterLastName(testData.LastName);
            _page.SelectGender(testData.Gender);
            _page.EnterDateOfBirth(testData.DateOfBirth);
            _page.EnterMobileNumber(testData.Mobile);
            _page.EnterEmail(testData.Email);
            _page.EnterCountry(testData.Country);
            _page.SelectState(testData.State);
            _page.SelectHobby(testData.Hobby);
            _page.EnterAbout(testData.About);
            _page.EnterUsername(testData.Username);
            _page.EnterPassword(testData.Password);
            _page.EnterConfirmPassword(testData.ConfirmPassword);
            _page.CheckTermsAndConditions();

            Console.WriteLine("[SUBMIT] All fields filled, submitting form...");
            _page.SubmitForm();

            Thread.Sleep(3000);

            // Assert - Validate response
            Assert.That(_page.IsSubmitResponseDisplayed(), Is.True, 
                "Submit response page should be displayed");

            var responseData = _page.GetSubmitResponseData();
            
            Assert.That(responseData, Is.Not.Empty, "Response data should not be empty");
            Assert.That(responseData["First Name"], Is.EqualTo(testData.FirstName), 
                "First Name should match in response");
            Assert.That(responseData["Last Name"], Is.EqualTo(testData.LastName), 
                "Last Name should match in response");
            Assert.That(responseData["Email"], Is.EqualTo(testData.Email), 
                "Email should match in response");

            Console.WriteLine("✓ Form submitted successfully with all fields validated");
        }

        [Test, Order(101)]
        [Category("Submit_Main")]
        [Category("PositiveTest")]
        public void Test_101_Main_Submit_MinimumRequiredFields()
        {
            // Arrange
            Console.WriteLine("\n[MAIN] Test: Submit form with only essential fields");
            var testData = new
            {
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@test.com",
                Gender = "Female",
                DateOfBirth = "1995-06-20"
            };

            // Act
            _page!.EnterFirstName(testData.FirstName);
            _page.EnterLastName(testData.LastName);
            _page.EnterEmail(testData.Email);
            _page.SelectGender(testData.Gender);
            _page.EnterDateOfBirth(testData.DateOfBirth);
            _page.CheckTermsAndConditions();

            Console.WriteLine("[SUBMIT] Essential fields filled, submitting form...");
            _page.SubmitForm();

            Thread.Sleep(2000);

            // Assert
            Assert.That(_page.IsSubmitResponseDisplayed(), Is.True, 
                "Submit response should be displayed for essential fields");

            var responseData = _page.GetSubmitResponseData();
            Assert.That(responseData["First Name"], Is.EqualTo(testData.FirstName));
            Assert.That(responseData["Last Name"], Is.EqualTo(testData.LastName));
            Assert.That(responseData["Email"], Is.EqualTo(testData.Email));

            Console.WriteLine("✓ Form submitted with essential required fields");
        }

        [Test, Order(102)]
        [Category("Submit_Main")]
        [Category("NegativeTest")]
        public void Test_102_Main_Submit_WithoutTermsCheckbox()
        {
            // Arrange
            Console.WriteLine("\n[MAIN] Test: Submit form without terms checkbox");
            _page!.EnterFirstName("Test");
            _page.EnterLastName("User");
            _page.EnterEmail("testuser@example.com");
            _page.SelectGender("Male");
            _page.EnterDateOfBirth("1990-01-15");
            _page.UncheckTermsAndConditions();

            // Act
            Console.WriteLine("[SUBMIT] Attempting to submit without terms checkbox...");
            _page.SubmitForm();

            Thread.Sleep(2000);

            // Assert
            bool responseDisplayed = _page.IsSubmitResponseDisplayed();
            
            Assert.That(responseDisplayed, Is.False, 
                "Form should prevent submission without terms checkbox checked");

            Console.WriteLine("✓ Form correctly prevented submission without terms checkbox");
        }

        [Test, Order(103)]
        [Category("Submit_Main")]
        [Category("NegativeTest")]
        public void Test_103_Main_Submit_IncorrectDateFormat()
        {
            // Arrange
            Console.WriteLine("\n[MAIN] Test: Date field with incorrect format");
            _page!.EnterFirstName("Test");
            _page.EnterLastName("DateFormat");
            _page.EnterEmail("date.test@example.com");
            _page.SelectGender("Female");
            
            Console.WriteLine("[DOB] Entering invalid date format: 15/05/1990");
            _page.EnterDateOfBirth("15/05/1990");
            
            // Act
            var enteredDate = _page.GetDateOfBirthValue();
            Console.WriteLine($"[DOB] Date value after entry: '{enteredDate}'");

            _page.CheckTermsAndConditions();
            _page.SubmitForm();
            
            Thread.Sleep(2000);
            bool submitted = _page.IsSubmitResponseDisplayed();
            
            // Assert
            Assert.That(submitted, Is.False, 
                "Form should prevent submission with DD/MM/YYYY date format");

            Console.WriteLine("✓ Form validation prevented submission with incorrect date format");
        }

        [Test, Order(104)]
        [Category("Submit_Main")]
        [Category("NegativeTest")]
        public void Test_104_Main_Submit_InvalidEmail()
        {
            // Arrange
            Console.WriteLine("\n[MAIN] Test: Submit form with invalid email format");
            _page!.EnterFirstName("Invalid");
            _page.EnterLastName("Email");
            _page.SelectGender("Male");
            _page.EnterDateOfBirth("1988-03-22");
            _page.EnterEmail("notanemail");
            _page.CheckTermsAndConditions();

            // Act
            Console.WriteLine("[SUBMIT] Attempting to submit with invalid email...");
            _page.SubmitForm();

            Thread.Sleep(2000);

            // Assert - EXPECTED: Should reject, ACTUAL: Bug - accepts invalid email
            bool responseDisplayed = _page.IsSubmitResponseDisplayed();
            
            Assert.That(responseDisplayed, Is.False, 
                "Form should reject invalid email format 'notanemail' (missing @ and domain). " +
                "Email validation is a basic requirement for data quality.");

            Console.WriteLine("✓ Form correctly prevented submission with invalid email");
        }

        [Test, Order(105)]
        [Category("Submit_Main")]
        [Category("NegativeTest")]
        public void Test_105_Main_Submit_MismatchedPasswords()
        {
            // Arrange
            Console.WriteLine("\n[MAIN] Test: Submit form with mismatched passwords");
            _page!.EnterFirstName("Password");
            _page!.EnterLastName("Mismatch");
            _page.EnterEmail("password.test@example.com");
            _page.SelectGender("Transgender");
            _page.EnterDateOfBirth("1992-11-08");
            _page.EnterPassword("Password123!");
            _page.EnterConfirmPassword("DifferentPassword456!");
            _page.CheckTermsAndConditions();

            // Act
            Console.WriteLine("[SUBMIT] Attempting to submit with mismatched passwords...");
            _page.SubmitForm();

            Thread.Sleep(2000);

            // Assert - EXPECTED: Should reject, ACTUAL: Bug - accepts mismatched passwords
            bool responseDisplayed = _page.IsSubmitResponseDisplayed();
            
            Assert.That(responseDisplayed, Is.False, 
                "Form should reject submission when Password and Confirm Password don't match. " +
                "This is a critical security validation that should be enforced.");

            Console.WriteLine("✓ Form correctly prevented submission with mismatched passwords");
        }

        [Test, Order(106)]
        [Category("Submit_Main")]
        [Category("NegativeTest")]
        public void Test_106_Main_Submit_EmptyRequiredFields()
        {
            // Arrange
            Console.WriteLine("\n[MAIN] Test: Submit form with empty required fields");
            _page!.CheckTermsAndConditions();
            
            // Act
            Console.WriteLine("[SUBMIT] Attempting to submit with empty Email field...");
            _page.SubmitForm();

            Thread.Sleep(2000);

            // Assert
            bool responseDisplayed = _page.IsSubmitResponseDisplayed();
            
            Assert.That(responseDisplayed, Is.False,
                "Form should prevent submission with empty Email field");

            Console.WriteLine("✓ Form correctly prevented submission with empty required Email");
        }

        [Test, Order(107)]
        [Category("Submit_Main")]
        [Category("PositiveTest")]
        public void Test_107_Main_Submit_SpecialCharacters()
        {
            // Arrange
            Console.WriteLine("\n[MAIN] Test: Submit form with special characters");
            var testData = new
            {
                FirstName = "O'Brien-José",
                LastName = "Müller-Schmidt",
                Email = "special.chars+test@example.com",
                About = "Testing with special chars: <>&\"'@#$%"
            };

            // Act
            _page!.EnterFirstName(testData.FirstName);
            _page.EnterLastName(testData.LastName);
            _page.EnterEmail(testData.Email);
            _page.SelectGender("Male");
            _page.EnterDateOfBirth("1985-07-30");
            _page.EnterAbout(testData.About);
            _page.CheckTermsAndConditions();

            Console.WriteLine("[SUBMIT] Submitting form with special characters...");
            _page.SubmitForm();

            // Assert
            Assert.That(_page.IsSubmitResponseDisplayed(), Is.True, 
                "Form should accept valid special characters");

            var responseData = _page.GetSubmitResponseData();
            Assert.That(responseData["First Name"], Is.EqualTo(testData.FirstName), 
                "Special characters should be preserved");

            Console.WriteLine("✓ Form correctly handled special characters");
        }

        [Test, Order(108)]
        [Category("Submit_Main")]
        [Category("NegativeTest")]
        public void Test_108_Main_Submit_MaxLength()
        {
            // Arrange
            Console.WriteLine("\n[MAIN] Test: Submit form with very long text");
            var longText = new string('A', 500);
            
            _page!.EnterFirstName(longText);
            _page.EnterLastName(longText);
            _page.EnterEmail("maxlength@test.com");
            _page.SelectGender("Female");
            _page.EnterDateOfBirth("1993-12-25");
            _page.EnterAbout(longText);
            _page.CheckTermsAndConditions();

            // Act
            var firstNameLength = _page.GetFirstNameValue().Length;
            Console.WriteLine($"[VALIDATION] First name length: {firstNameLength}");
            
            // Assert - EXPECTED: Should limit to 255, ACTUAL: Bug - accepts 500+ chars
            Assert.That(firstNameLength, Is.LessThanOrEqualTo(255), 
                "Form should enforce maximum length limits (e.g., 255 characters). " +
                $"Actual length: {firstNameLength}");
            
            _page.SubmitForm();
            Thread.Sleep(2000);

            Assert.That(_page.IsSubmitResponseDisplayed(), Is.True,
                "Form should submit after enforcing length limits");

            Console.WriteLine($"✓ Form enforced max length: {firstNameLength} characters");
        }

        [Test, Order(109)]
        [Category("Submit_Main")]
        [Category("PositiveTest")]
        public void Test_109_Main_Submit_MultipleHobbies()
        {
            // Arrange
            Console.WriteLine("\n[MAIN] Test: Submit form with multiple hobbies");
            _page!.EnterFirstName("Hobby");
            _page.EnterLastName("Tester");
            _page.EnterEmail("hobby.test@example.com");
            _page.SelectGender("Male");
            _page.EnterDateOfBirth("1991-04-18");
            _page.SelectHobby("Dance");
            _page.SelectHobby("Reading");
            _page.SelectHobby("Cricket");
            _page.CheckTermsAndConditions();

            // Act
            Console.WriteLine("[SUBMIT] Submitting form with multiple hobbies...");
            _page.SubmitForm();

            // Assert
            Assert.That(_page.IsSubmitResponseDisplayed(), Is.True, 
                "Form should submit with multiple hobbies");

            Console.WriteLine("✓ Form successfully submitted with multiple hobbies");
        }

        [Test, Order(110)]
        [Category("Submit_Main")]
        [Category("PositiveTest")]
        public void Test_110_Main_Submit_AllGenderOptions()
        {
            // Arrange & Act - Test each gender option
            var genders = new[] { "Male", "Female", "Transgender" };

            foreach (var gender in genders)
            {
                Console.WriteLine($"\n[MAIN] Test: Submit form with gender: {gender}");
                
                _page!.Navigate();
                
                _page.EnterFirstName("Gender");
                _page.EnterLastName("Test");
                _page.EnterEmail($"gender.{gender.ToLower()}@test.com");
                _page.SelectGender(gender);
                _page.EnterDateOfBirth("1990-06-15");
                _page.CheckTermsAndConditions();

                Console.WriteLine($"[SUBMIT] Submitting form with gender: {gender}");
                _page.SubmitForm();

                Assert.That(_page.IsSubmitResponseDisplayed(), Is.True, 
                    $"Form should submit with gender: {gender}");

                Console.WriteLine($"✓ Successfully submitted with gender: {gender}");
            }
        }

        [Test, Order(111)]
        [Category("Submit_Main")]
        [Category("NegativeTest")]
        public void Test_111_Main_Submit_FutureDateOfBirth()
        {
            // Arrange
            Console.WriteLine("\n[MAIN] Test: Submit form with future date of birth");
            _page!.EnterFirstName("Future");
            _page.EnterLastName("Date");
            _page.EnterEmail("future.date@test.com");
            _page.SelectGender("Female");
            
            var futureDate = DateTime.Now.AddYears(1).ToString("yyyy-MM-dd");
            Console.WriteLine($"[DOB] Entering future date: {futureDate}");
            _page.EnterDateOfBirth(futureDate);
            
            _page.CheckTermsAndConditions();

            // Act
            Console.WriteLine("[SUBMIT] Attempting to submit with future date of birth...");
            _page.SubmitForm();

            Thread.Sleep(2000);

            // Assert - EXPECTED: Should reject, ACTUAL: Bug - accepts future dates
            bool responseDisplayed = _page.IsSubmitResponseDisplayed();
            
            Assert.That(responseDisplayed, Is.False, 
                "Form should reject future date of birth as it's logically invalid");

            Console.WriteLine("✓ Form correctly rejected future date of birth");
        }

        [Test, Order(112)]
        [Category("Submit_Main")]
        [Category("PositiveTest")]
        public void Test_112_Main_Submit_DefaultStateValue()
        {
            // Arrange
            Console.WriteLine("\n[MAIN] Test: Submit form with default state value");
            _page!.EnterFirstName("Default");
            _page.EnterLastName("State");
            _page.EnterEmail("default.state@test.com");
            _page.SelectGender("Male");
            _page.EnterDateOfBirth("1987-09-14");
            _page.CheckTermsAndConditions();

            // Act
            Console.WriteLine("[SUBMIT] Submitting with default state value...");
            _page.SubmitForm();

            Thread.Sleep(2000);

            // Assert
            var responseDisplayed = _page.IsSubmitResponseDisplayed();
            
            Assert.That(responseDisplayed, Is.True,
                "Form should accept default state value (State is optional)");

            var responseData = _page.GetSubmitResponseData();
            Assert.That(responseData["State"], Is.EqualTo("-- Select Country --"),
                "Default state value should be preserved");

            Console.WriteLine("✓ Form accepted default state value");
        }

        #endregion

        #region IFrame without ID - Form Submission Tests (TODO: Implement when iframe methods are added)
        [Test, Order(200)]
        [Category("Submit_IFrameNoId")]
        [Category("PositiveTest")]
        public void Test_200_IFrameNoId_Submit_CompleteValidForm()
        {
            Console.WriteLine("\n[IFRAME-NO-ID] Test: Submit complete valid form");
            try
            {
                _page!.SwitchToIframeContainingHeading("IFrame without ID");

                var testData = new
                {
                    FirstName = "IFrameNoId",
                    LastName = "Test",
                    Email = "iframe.noid@test.com",
                    Gender = "Male",
                    DateOfBirth = "1988-09-10"
                };

                _page.EnterFirstName(testData.FirstName);
                _page.EnterLastName(testData.LastName);
                _page.EnterEmail(testData.Email);
                _page.SelectGender(testData.Gender);
                _page.EnterDateOfBirth(testData.DateOfBirth);
                _page.CheckTermsAndConditions();

                Console.WriteLine("[SUBMIT] Submitting iframe (no-id) form...");
                _page.SubmitForm();

                Thread.Sleep(2000);

                Assert.That(_page.IsSubmitResponseDisplayed(), Is.True,
                    "IFrame (no-id) form should submit successfully");

                var response = _page.GetSubmitResponseData();
                Assert.That(response, Is.Not.Empty);
                Assert.That(response.ContainsKey("First Name") ? response["First Name"] : string.Empty, Is.EqualTo(testData.FirstName));

                Console.WriteLine("✓ IFrame without ID form submitted successfully");
            }
            finally
            {
                _page!.SwitchToDefaultContent();
            }
        }

        [Test, Order(201)]
        [Category("Submit_IFrameNoId")]
        [Category("NegativeTest")]
        public void Test_201_IFrameNoId_Submit_WithoutTermsCheckbox()
        {
            Console.WriteLine("\n[IFRAME-NO-ID] Test: Submit without terms checkbox");
            try
            {
                _page!.SwitchToIframeContainingHeading("IFrame without ID");

                _page.EnterFirstName("IFrame");
                _page.EnterLastName("NoTerms");
                _page.EnterEmail("iframe.noterms@test.com");
                _page.SelectGender("Female");
                _page.EnterDateOfBirth("1992-05-20");
                _page.UncheckTermsAndConditions();

                Console.WriteLine("[SUBMIT] Attempting to submit without terms...");
                _page.SubmitForm();

                Thread.Sleep(2000);

                Assert.That(_page.IsSubmitResponseDisplayed(), Is.False,
                    "IFrame (no-id) form should prevent submission without terms checkbox");

                Console.WriteLine("✓ IFrame form correctly prevented submission");
            }
            finally
            {
                _page!.SwitchToDefaultContent();
            }
        }

        #endregion

        #region IFrame with ID - Form Submission Tests (TODO: Implement when iframe methods are added)
        [Test, Order(300)]
        [Category("Submit_IFrameWithId")]
        [Category("PositiveTest")]
        public void Test_300_IFrameWithId_Submit_CompleteValidForm()
        {
            Console.WriteLine("\n[IFRAME-WITH-ID] Test: Submit complete valid form");
            try
            {
                _page!.SwitchToIframeById("iframeId");

                var testData = new
                {
                    FirstName = "IFrameId",
                    LastName = "Test",
                    Email = "iframe.withid@test.com",
                    Gender = "Transgender",
                    DateOfBirth = "1995-03-15"
                };

                _page.EnterFirstName(testData.FirstName);
                _page.EnterLastName(testData.LastName);
                _page.EnterEmail(testData.Email);
                _page.SelectGender(testData.Gender);
                _page.EnterDateOfBirth(testData.DateOfBirth);
                _page.CheckTermsAndConditions();

                Console.WriteLine("[SUBMIT] Submitting iframe (with id) form...");
                _page.SubmitForm();

                Thread.Sleep(2000);

                Assert.That(_page.IsSubmitResponseDisplayed(), Is.True,
                    "IFrame (with id) form should submit successfully");

                var response = _page.GetSubmitResponseData();
                Assert.That(response, Is.Not.Empty);
                Assert.That(response.ContainsKey("Email") ? response["Email"] : string.Empty, Is.EqualTo(testData.Email));

                Console.WriteLine("✓ IFrame with ID form submitted successfully");
            }
            finally
            {
                _page!.SwitchToDefaultContent();
            }
        }

        [Test, Order(301)]
        [Category("Submit_IFrameWithId")]
        [Category("NegativeTest")]
        public void Test_301_IFrameWithId_Submit_InvalidEmail()
        {
            Console.WriteLine("\n[IFRAME-WITH-ID] Test: Submit with invalid email");
            try
            {
                _page!.SwitchToIframeById("iframeId");

                _page.EnterFirstName("Invalid");
                _page.EnterLastName("Email");
                _page.EnterEmail("notanemail");
                _page.SelectGender("Male");
                _page.EnterDateOfBirth("1990-07-22");
                _page.CheckTermsAndConditions();

                Console.WriteLine("[SUBMIT] Attempting to submit with invalid email...");
                _page.SubmitForm();

                Thread.Sleep(2000);

                Assert.That(_page.IsSubmitResponseDisplayed(), Is.False,
                    "IFrame (with id) form should reject invalid email format");

                Console.WriteLine("✓ IFrame form correctly prevented submission");
            }
            finally
            {
                _page!.SwitchToDefaultContent();
            }
        }

        #endregion

        #region Shadow DOM - Form Submission Tests (Shadow DOM doesn't have submission, only field interaction)

        // Note: replaced the field-only Shadow DOM test with the submission-capable one below.

        [Test, Order(400)]
        [Category("Submit_ShadowDOM")]
        [Category("PositiveTest")]
        public void Test_400_ShadowDOM_Submit_CompleteValidForm()
        {
            Console.WriteLine("\n[SHADOW-DOM] Test: Submit complete valid form (shadow section)");

            // Fill shadow-hosted fields using page object helpers
            _page!.EnterShadowFirstName("Shadow DOM", "ShadowDOM");
            _page.EnterShadowLastName("Shadow DOM", "Test");
            _page.SelectShadowState("Shadow DOM", "India");

            // The submit button for this form is outside the shadow host but inside the same form
            try
            {
                var formSubmit = Driver!.FindElement(By.CssSelector("form#shadowdomautomationtestform button"));
                if (formSubmit == null)
                {
                    Assert.Inconclusive("Submit button for shadow DOM form not found by selector 'form#shadowdomautomationtestform button'.");
                    return;
                }

                // Only check the terms checkbox inside the shadow host (do NOT fall back to main form checkbox)
                _page.CheckShadowTermsIfPresent("Shadow DOM");

                try
                {
                    formSubmit.Click();
                }
                catch (OpenQA.Selenium.ElementNotInteractableException)
                {
                    // Fallback to JS click when element is not interactable (e.g., wrapped by custom element)
                    ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].click();", formSubmit);
                }
                catch (Exception)
                {
                    // As a last resort, try JS click
                    ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].click();", formSubmit);
                }

                // Wait a moment for submission to be processed and page to update
                Thread.Sleep(2000);

                Console.WriteLine("[RESPONSE] Checking for submit response after shadow form submit...");
                Assert.That(_page.IsSubmitResponseDisplayed(), Is.True, "Submitting shadow DOM form should show response");
                var response = _page.GetSubmitResponseData();
                Assert.That(response, Is.Not.Empty);

                Console.WriteLine("✓ Shadow DOM form submitted and response verified");
            }
            catch (NoSuchElementException)
            {
                Assert.Inconclusive("Submit button for shadow DOM form not present in DOM.");
            }
        }

        [Test, Order(401)]
        [Category("Submit_ShadowDOM")]
        [Category("NegativeTest")]
        public void Test_402_ShadowDOM_MissingFirstNamePreventsSubmit_WhenRequired()
        {
            Console.WriteLine("\n[SHADOW-DOM] Test: Submit with missing First Name (only if field is required)");

            // Locate the shadow-hosted first name element
            var firstNameEl = _page!.FindElementInShadowByHeading("Shadow DOM", "section[slot='fname'] input, input#fname, input[name='fname']");

            // If the field isn't marked required, we cannot assert negative HTML5 validation reliably
            var required = firstNameEl.GetAttribute("required");
            if (string.IsNullOrEmpty(required))
            {
                Assert.Ignore("Shadow DOM First Name field is not marked 'required'; skipping negative validation test.");
                return;
            }

            // Clear the field and attempt to submit the enclosing form
            firstNameEl.Clear();

            try
            {
                var form = Driver!.FindElement(By.CssSelector("form#shadowdomautomationtestform"));
                var isValid = (bool)((IJavaScriptExecutor)Driver).ExecuteScript("return arguments[0].checkValidity();", form);

                // The form should be invalid when required field is empty
                Assert.That(isValid, Is.False, "Form validity should be false when required first name is missing");

                Console.WriteLine("✓ Shadow DOM form validity prevented submission due to missing First Name");
            }
            catch (NoSuchElementException)
            {
                Assert.Ignore("Shadow DOM form element not found; skipping negative validation test.");
            }
        }

        #endregion
    }
}
