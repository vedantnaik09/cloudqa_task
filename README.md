**CloudQA Automation Practice — README**

- **Project**: CloudQAAutomation — C# + Selenium tests for the AutomationPracticeForm page.

**Task**
- **Description**: Implement automated tests (C# + Selenium) for any three fields on `app.cloudqa.io/home/AutomationPracticeForm` so tests keep working even if element positions or HTML attributes change.

**What This Repo Contains**
- **Three automated fields**: First Name (text input), Email (text input with validation), State (dropdown select).
- **Primary test class**: `Tests/ThreeFieldTests.cs` — contains unit tests exercising the three fields and integration/resilience checks.
- **Page object**: `PageObjects/AutomationPracticeFormPage.cs` — encapsulates interactions with the form.
- **Helpers**: `Helpers/RobustElementFinder.cs` — multi-strategy element lookup used to make tests resilient to DOM changes.
- **Run script**: `RunTests.ps1` — convenience PowerShell script to run the test suite.

**Design & Resilience Approach**
- **Multi-strategy locators**: The project uses `RobustElementFinder` to locate elements using multiple strategies (label association, relative XPath, ARIA attributes, visible text) rather than relying on brittle IDs or fixed positions.
- **Page Object Pattern**: `AutomationPracticeFormPage` centralizes selectors and actions, so locator changes are isolated in one place.
- **Explicit waits / JS interactions**: Tests use waits and, where appropriate, JavaScript execution to interact reliably with the UI.
- **Tests assert behavior, not markup**: Assertions verify field values and behaviors (persistence after scroll/refresh, dropdown options), which is more robust against HTML reshuffles.

**Prerequisites**
- **.NET SDK**: Install .NET 8 SDK (project targets `net8.0`).
- **Browser**: Chrome/Chromium or Edge installed. Selenium Manager is used to obtain the appropriate driver automatically.
- **PowerShell**: Tests can be run from PowerShell (Windows). If `RunTests.ps1` is blocked, you may need to set execution policy or run PowerShell as Administrator.

**Run The Tests**
Open PowerShell in the repository root and run one of the commands below.

PowerShell (recommended):
```
.\RunTests.ps1
```

or using dotnet test (build + run):
```
dotnet test --configuration Release
```

Run a specific test category (example: FirstName) — using `dotnet test` with a filter:
```
dotnet test --configuration Release --filter "Category=FirstName"
```

**Key Files**
- `PageObjects/AutomationPracticeFormPage.cs` — page object for form interactions.
- `Tests/ThreeFieldTests.cs` — NUnit tests for the three fields and resilience checks.
- `Helpers/RobustElementFinder.cs` — logic for resilient element lookup.
- `RunTests.ps1` — test runner script (PowerShell).


**Notes**
- The suite includes tests that verify resiliency (refresh, scroll, repeated selection) to demonstrate the multi-strategy locator approach.
- If you run into driver issues, ensure the browser is up-to-date; Selenium Manager (used by the WebDriver) will typically install the correct driver.

