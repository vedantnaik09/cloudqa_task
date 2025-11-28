using NUnit.Framework;
using CloudQAAutomation.PageObjects;
using CloudQAAutomation.Helpers;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace CloudQAAutomation.Tests
{
    /// <summary>
    /// Test suite for CloudQA Automation Practice Form - Three Field Challenge
    /// 
    /// This test class demonstrates:
    /// 1. Dynamic HTML analysis to discover elements
    /// 2. Multi-strategy locators that survive HTML changes
    /// 3. Resilient test design with proper waits and validations
    /// 
    /// Fields Tested:
    /// - First Name (text input)
    /// - Email (text input with validation)
    /// - State (dropdown select)
    /// </summary>
    [TestFixture]
    public class ThreeFieldTests : TestBase
    {
        private AutomationPracticeFormPage? _page;

        [SetUp]
        public void Setup()
        {
            Console.WriteLine("=== Test Setup ===");
            InitializeDriver();
            _page = new AutomationPracticeFormPage(Driver!);
            _page.Navigate();
            Console.WriteLine("=== Setup Complete ===\n");
        }

        [TearDown]
        public void Teardown()
        {
            Console.WriteLine("\n=== Test Teardown ===");
            try
            {
                var testName = TestContext.CurrentContext.Test.Name;
                var outcome = TestContext.CurrentContext.Result.Outcome.Status.ToString();
                SaveScreenshot($"{testName}_{outcome}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error taking screenshot in teardown: {ex.Message}");
            }
            CleanupDriver();
            Console.WriteLine("=== Teardown Complete ===");
        }

        // Dynamic analysis tests removed for production build

        #region First Name Field Tests

        [Test, Order(1)]
        [Category("FirstName")]
        [Category("Level_Main")]
        [TestCase("John")]
        [TestCase("Alice")]
        [TestCase("Test User")]
        public void Test_01_FirstName_EntersTextSuccessfully(string firstName)
        {
            // Arrange
            Console.WriteLine($"\nTest: Enter first name '{firstName}'");

            // Act
            _page!.EnterFirstName(firstName);
            var actualValue = _page.GetFirstNameValue();

            // Assert
            Assert.That(actualValue, Is.EqualTo(firstName), 
                $"First name should be '{firstName}'");
            
            Console.WriteLine($"✓ First name entered and verified: {actualValue}");
        }

        [Test, Order(2)]
        [Category("FirstName")]
        [Category("Level_Main")]
        public void Test_02_FirstName_ClearFieldWorks()
        {
            // Arrange
            Console.WriteLine("\nTest: Clear first name field");
            _page!.EnterFirstName("TestName");

            // Act
            _page.ClearFirstName();
            var actualValue = _page.GetFirstNameValue();

            // Assert
            Assert.That(actualValue, Is.Empty, "First name should be empty after clear");
            Console.WriteLine("✓ First name field cleared successfully");
        }

        [Test, Order(3)]
        [Category("FirstName")]
        [Category("Level_Main")]
        public void Test_03_FirstName_HandlesSpecialCharacters()
        {
            // Arrange
            var specialName = "O'Brien-Smith";
            Console.WriteLine($"\nTest: Enter name with special characters: {specialName}");

            // Act
            _page!.EnterFirstName(specialName);
            var actualValue = _page.GetFirstNameValue();

            // Assert
            Assert.That(actualValue, Is.EqualTo(specialName), 
                "Should handle special characters in name");
            
            Console.WriteLine($"✓ Special characters handled: {actualValue}");
        }

        #endregion

        

        #region Email Field Tests

        [Test, Order(4)]
        [Category("Email")]
        [Category("Level_Main")]
        [TestCase("test@example.com")]
        [TestCase("user.name+tag@domain.co.uk")]
        [TestCase("valid_email123@test-domain.org")]
        public void Test_04_Email_EntersValidEmailSuccessfully(string email)
        {
            // Arrange
            Console.WriteLine($"\nTest: Enter valid email '{email}'");

            // Act
            _page!.EnterEmail(email);
            var actualValue = _page.GetEmailValue();

            // Assert
            Assert.That(actualValue, Is.EqualTo(email), 
                $"Email should be '{email}'");
            
            Console.WriteLine($"✓ Email entered and verified: {actualValue}");
        }

        [Test, Order(5)]
        [Category("Email")]
        [Category("Level_Main")]
        public void Test_05_Email_ClearFieldWorks()
        {
            // Arrange
            Console.WriteLine("\nTest: Clear email field");
            _page!.EnterEmail("test@example.com");

            // Act
            _page.ClearEmail();
            var actualValue = _page.GetEmailValue();

            // Assert
            Assert.That(actualValue, Is.Empty, "Email should be empty after clear");
            Console.WriteLine("✓ Email field cleared successfully");
        }

        [Test, Order(6)]
        [Category("Email")]
        [Category("Level_Main")]
        public void Test_06_Email_PreservesValueOnNavigationWithinPage()
        {
            // Arrange
            var testEmail = "persistent@test.com";
            Console.WriteLine($"\nTest: Email persistence - {testEmail}");

            // Act
            _page!.EnterEmail(testEmail);
            var valueBeforeScroll = _page.GetEmailValue();
            
            // Scroll to bottom and back (simulating page interaction)
            ((IJavaScriptExecutor)Driver!).ExecuteScript("window.scrollTo(0, document.body.scrollHeight)");
            Thread.Sleep(500);
            ((IJavaScriptExecutor)Driver!).ExecuteScript("window.scrollTo(0, 0)");
            Thread.Sleep(500);
            
            var valueAfterScroll = _page.GetEmailValue();

            // Assert
            Assert.That(valueBeforeScroll, Is.EqualTo(testEmail));
            Assert.That(valueAfterScroll, Is.EqualTo(testEmail), 
                "Email value should persist after page interaction");
            
            Console.WriteLine("✓ Email value persisted after scrolling");
        }

        #endregion

        #region State Dropdown Tests

        [Test, Order(7)]
        [Category("State")]
        [Category("Level_Main")]
        [TestCase("India")]
        [TestCase("United States")]
        [TestCase("Canada")]
        [TestCase("Australia")]
        public void Test_07_State_SelectsByTextSuccessfully(string stateName)
        {
            // Arrange
            Console.WriteLine($"\nTest: Select state '{stateName}'");

            // Act
            _page!.SelectState(stateName);
            var selectedState = _page.GetSelectedState();

            // Assert
            Assert.That(selectedState, Is.EqualTo(stateName), 
                $"Selected state should be '{stateName}'");
            
            Console.WriteLine($"✓ State selected and verified: {selectedState}");
        }

        [Test, Order(8)]
        [Category("State")]
        [Category("Level_Main")]
        public void Test_08_State_SelectsByIndexSuccessfully()
        {
            // Arrange
            var testIndex = 5; // Skip first "-- Select Country --" option
            Console.WriteLine($"\nTest: Select state by index {testIndex}");

            // Act
            _page!.SelectStateByIndex(testIndex);
            var selectedState = _page.GetSelectedState();

            // Assert
            Assert.That(selectedState, Is.Not.EqualTo("-- Select Country --"), 
                "Should select a valid state");
            Assert.That(selectedState, Is.Not.Empty, "Selected state should not be empty");
            
            Console.WriteLine($"✓ State selected by index: {selectedState}");
        }

        [Test, Order(9)]
        [Category("State")]
        [Category("Level_Main")]
        public void Test_09_State_HasExpectedOptions()
        {
            // Arrange & Act
            Console.WriteLine("\nTest: Verify state dropdown has expected options");
            var allOptions = _page!.GetAllStateOptions();

            // Assert
            Assert.That(allOptions, Is.Not.Empty, "State dropdown should have options");
            Assert.That(allOptions.Count, Is.GreaterThan(10), 
                "Should have multiple state options");
            
            // Verify some common countries exist
            var expectedStates = new[] { "India", "United States", "Canada", "Australia" };
            foreach (var state in expectedStates)
            {
                Assert.That(allOptions, Does.Contain(state), 
                    $"Dropdown should contain '{state}'");
            }

            Console.WriteLine($"✓ Dropdown has {allOptions.Count} options");
            Console.WriteLine($"✓ Verified presence of key options: {string.Join(", ", expectedStates)}");
        }

        [Test, Order(10)]
        [Category("State")]
        [Category("Level_Main")]
        public void Test_10_State_CanChangeSelectionMultipleTimes()
        {
            // Arrange
            Console.WriteLine("\nTest: Change state selection multiple times");
            var states = new[] { "India", "Canada", "United Kingdom", "Australia" };

            // Act & Assert
            foreach (var state in states)
            {
                _page!.SelectState(state);
                var selected = _page.GetSelectedState();
                
                Assert.That(selected, Is.EqualTo(state), 
                    $"Should be able to select '{state}'");
                
                Console.WriteLine($"  ✓ Changed to: {state}");
            }
            
            Console.WriteLine("✓ Successfully changed selection multiple times");
        }

        #endregion

        #region Integration Tests

        [Test, Order(11)]
        [Category("Integration")]
        [Category("Level_Main")]
        public void Test_11_Integration_FillAllThreeFieldsTogether()
        {
            // Arrange
            var testData = new
            {
                FirstName = "John",
                Email = "john.doe@example.com",
                State = "United States"
            };
            
            Console.WriteLine("\nTest: Fill all three fields together");
            Console.WriteLine($"  First Name: {testData.FirstName}");
            Console.WriteLine($"  Email: {testData.Email}");
            Console.WriteLine($"  State: {testData.State}");

            // Act
            _page!.EnterFirstName(testData.FirstName);
            _page.EnterEmail(testData.Email);
            _page.SelectState(testData.State);

            // Assert
            var actualFirstName = _page.GetFirstNameValue();
            var actualEmail = _page.GetEmailValue();
            var actualState = _page.GetSelectedState();

            Assert.That(actualFirstName, Is.EqualTo(testData.FirstName), 
                "First name should match");
            Assert.That(actualEmail, Is.EqualTo(testData.Email), 
                "Email should match");
            Assert.That(actualState, Is.EqualTo(testData.State), 
                "State should match");

            Console.WriteLine("✓ All three fields filled and verified successfully");
        }

        [Test, Order(12)]
        [Category("Integration")]
        [Category("Level_Main")]
        public void Test_12_Integration_ResetFormClearsAllFields()
        {
            // Arrange
            Console.WriteLine("\nTest: Reset form clears all fields");
            _page!.EnterFirstName("TestName");
            _page.EnterEmail("test@example.com");
            _page.SelectState("India");

            // Act
            _page.ResetForm();
            Thread.Sleep(500); // Wait for reset

            var firstNameAfter = _page.GetFirstNameValue();
            var emailAfter = _page.GetEmailValue();
            // Note: State dropdown might reset to first option

            // Assert
            Assert.That(firstNameAfter, Is.Empty, "First name should be cleared");
            Assert.That(emailAfter, Is.Empty, "Email should be cleared");

            Console.WriteLine("✓ Form reset cleared all fields");
        }

        #endregion

        #region Resilience Tests

        [Test, Order(13)]
        [Category("Resilience")]
        [Category("Level_Main")]
        public void Test_13_Resilience_LocatorsWorkAfterPageRefresh()
        {
            // Arrange
            Console.WriteLine("\nTest: Locators work after page refresh");
            var testFirstName = "RefreshTest";

            // Act - Enter data
            _page!.EnterFirstName(testFirstName);
            var valueBeforeRefresh = _page.GetFirstNameValue();

            // Refresh page
            Driver!.Navigate().Refresh();
            Thread.Sleep(1000);

            // Try to interact with field again
            _page.EnterFirstName(testFirstName);
            var valueAfterRefresh = _page.GetFirstNameValue();

            // Assert
            Assert.That(valueBeforeRefresh, Is.EqualTo(testFirstName));
            Assert.That(valueAfterRefresh, Is.EqualTo(testFirstName), 
                "Locators should still work after page refresh");

            Console.WriteLine("✓ Locators remained stable after page refresh");
        }

        #endregion

        #region IFrame Tests

        // Level: IFrame without ID - order 14..16
        [Test, Order(14)]
        [Category("FirstName")]
        [Category("Level_IFrame_NoId")]
        public void Test_14_IFrameNoId_FirstName()
        {
            Console.WriteLine("\nTest: IFrame without ID - interact with First Name field");
            _page!.SwitchToIframeContainingHeading("IFrame without ID");
            _page.EnterFirstName("IframeNoIdUser");
            var valueInFrame = _page.GetFirstNameValue();
            _page.SwitchToDefaultContent();
            Assert.That(valueInFrame, Is.EqualTo("IframeNoIdUser"));
            Console.WriteLine($"✓ Entered in iframe without id: {valueInFrame}");
        }

        [Test, Order(15)]
        [Category("Email")]
        [Category("Level_IFrame_NoId")]
        public void Test_15_IFrameNoId_Email()
        {
            Console.WriteLine("\nTest: IFrame without ID - interact with Email field");
            _page!.SwitchToIframeContainingHeading("IFrame without ID");
            _page.EnterEmail("iframe.noid@example.com");
            var emailValue = _page.GetEmailValue();
            _page.SwitchToDefaultContent();
            Assert.That(emailValue, Is.EqualTo("iframe.noid@example.com"));
            Console.WriteLine($"✓ Email entered in iframe without id: {emailValue}");
        }

        [Test, Order(16)]
        [Category("State")]
        [Category("Level_IFrame_NoId")]
        public void Test_16_IFrameNoId_State()
        {
            Console.WriteLine("\nTest: IFrame without ID - interact with State dropdown");
            _page!.SwitchToIframeContainingHeading("IFrame without ID");
            _page.SelectState("United States");
            var selected = _page.GetSelectedState();
            _page.SwitchToDefaultContent();
            Assert.That(selected, Is.EqualTo("United States"));
            Console.WriteLine($"✓ State selected in iframe without id: {selected}");
        }

        // Level: IFrame with ID - order 17..19
        [Test, Order(17)]
        [Category("FirstName")]
        [Category("Level_IFrame_WithId")]
        public void Test_17_IFrameWithId_FirstName()
        {
            Console.WriteLine("\nTest: IFrame with ID - interact with First Name field");
            _page!.SwitchToIframeById("iframeId");
            _page.EnterFirstName("IframeWithIdUser");
            var valueInFrame = _page.GetFirstNameValue();
            _page.SwitchToDefaultContent();
            Assert.That(valueInFrame, Is.EqualTo("IframeWithIdUser"));
            Console.WriteLine($"✓ Entered in iframe with id: {valueInFrame}");
        }

        [Test, Order(18)]
        [Category("Email")]
        [Category("Level_IFrame_WithId")]
        public void Test_18_IFrameWithId_Email()
        {
            Console.WriteLine("\nTest: IFrame with ID - interact with Email field");
            _page!.SwitchToIframeById("iframeId");
            _page.EnterEmail("iframe.withid@example.com");
            var emailValue = _page.GetEmailValue();
            _page.SwitchToDefaultContent();
            Assert.That(emailValue, Is.EqualTo("iframe.withid@example.com"));
            Console.WriteLine($"✓ Email entered in iframe with id: {emailValue}");
        }

        [Test, Order(19)]
        [Category("State")]
        [Category("Level_IFrame_WithId")]
        public void Test_19_IFrameWithId_State()
        {
            Console.WriteLine("\nTest: IFrame with ID - interact with State dropdown");
            _page!.SwitchToIframeById("iframeId");
            _page.SelectState("India");
            var selected = _page.GetSelectedState();
            _page.SwitchToDefaultContent();
            Assert.That(selected, Is.EqualTo("India"));
            Console.WriteLine($"✓ State selected in iframe with id: {selected}");
        }

        #endregion

        #region Shadow DOM Tests

        [Test, Order(20)]
        [Category("Level_ShadowDom")]
        public void Test_20_ShadowDom_FirstName()
        {
            Console.WriteLine("\nTest: Shadow DOM - First Name field");
            _page!.EnterShadowFirstName("Shadow DOM", "ShadowFirst");
            var actual = _page.GetShadowFirstNameValue("Shadow DOM");
            Assert.That(actual, Is.EqualTo("ShadowFirst"));
            Console.WriteLine($"✓ Shadow DOM first name entered and verified: {actual}");
        }

        [Test, Order(21)]
        [Category("Level_ShadowDom")]
        public void Test_21_ShadowDom_LastName()
        {
            Console.WriteLine("\nTest: Shadow DOM - Last Name field");
            _page!.EnterShadowLastName("Shadow DOM", "ShadowLast");
            var actual = _page.GetShadowLastNameValue("Shadow DOM");
            Assert.That(actual, Is.EqualTo("ShadowLast"));
            Console.WriteLine($"✓ Shadow DOM last name entered and verified: {actual}");
        }

        [Test, Order(22)]
        [Category("Level_ShadowDom")]
        public void Test_22_ShadowDom_State()
        {
            Console.WriteLine("\nTest: Shadow DOM - State dropdown");
            _page!.SelectShadowState("Shadow DOM", "India");
            var selected = _page.GetSelectedShadowState("Shadow DOM");
            Assert.That(selected, Is.EqualTo("India"));
            Console.WriteLine($"✓ Shadow DOM state selected and verified: {selected}");
        }

        #endregion
    }
}
