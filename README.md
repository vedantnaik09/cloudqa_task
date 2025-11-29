# CloudQA Automation

Short summary
----------------
CloudQA Automation is a compact .NET-based test suite that verifies a practice form's behavior. Tests exercise three core fields (First Name, Email, State) and cover interactions in the main page, inside iframes (with and without ids), and within Shadow DOM. The suite captures screenshots and HTML snapshots to help diagnose failures.

Who this README is for
- Developers and QA engineers running tests locally on Windows
- CI engineers who need reproducible test commands

Why this repo exists (one line)
- To provide resilient automation that validates form behavior across common web contexts and surface validation bugs quickly.

Quick Start — top commands
--------------------------
Start here: three important commands you'll use most often.

- Testing for submission (run submission validations):

```powershell
# Run the helper script (restores, builds Release, runs tests)
.\RunTests.ps1

# Or run only the form submission test category directly
dotnet test --configuration Release --filter "Category=FormSubmission"
```

Validation tests included in `FormSubmissionTests` (what we check during submission)
- Test_100: Submit complete valid form (all fields filled)
- Test_101: Submit with minimum required fields
- Test_102: Prevent submission without Terms & Conditions checked
- Test_103: Reject incorrect date format (DD/MM/YYYY)
- Test_104: Reject invalid email formats
- Test_105: Reject mismatched Password / Confirm Password
- Test_106: Prevent submission when required fields (Email) are empty
- Test_107: Preserve and accept special characters in fields
- Test_108: Enforce maximum length limits for text fields
- Test_109: Submit with multiple hobbies selected
- Test_110: Submit with each gender option (Male/Female/Transgender)
- Test_111: Reject future Date of Birth values
- Test_112: Accept default state value when State is optional

- Three level testing for all levels (Main, IFrame, Shadow DOM):

```powershell
# Run Level_Main, Level_IFrame_NoId, Level_IFrame_WithId, Level_ShadowDom
dotnet test --configuration Release --filter "Category=Level_Main|Category=Level_IFrame_NoId|Category=Level_IFrame_WithId|Category=Level_ShadowDom"
```

- Three level testing for a specific level:

```powershell
# Replace LEVEL with one of: Level_Main, Level_IFrame_NoId, Level_IFrame_WithId, Level_ShadowDom
dotnet test --configuration Release --filter "Category=LEVEL"

# Example: run only main-document three-field tests
dotnet test --configuration Release --filter "Category=Level_Main"
```

Prerequisites
-------------
- .NET 8 SDK installed (run `dotnet --version` to verify).
- A modern browser (Chrome or Edge) and matching WebDriver.
- PowerShell on Windows (examples below use PowerShell syntax).

If you prefer automatic driver management, the repo includes Selenium Manager binaries under `bin/**/selenium-manager/windows/`.

Basic commands (friendly)
-------------------------
- Restore packages:

```powershell
dotnet restore
```

- Build (release):

```powershell
dotnet build --configuration Release
```

- Run whole test suite:

```powershell
dotnet test --configuration Release
```

- Run tests with detailed console output:

```powershell
dotnet test --configuration Release --logger "console;verbosity=detailed"
```

- Run a single test by fully-qualified name (copy name from the test file):

```powershell
dotnet test --filter "FullyQualifiedName=CloudQAAutomation.Tests.ThreeFieldTests.Test_11_Integration_FillAllThreeFieldsTogether"
```

Logs and artifacts
------------------
- HTML snapshots: `logs/html-snippets/`
- Screenshots saved during teardown: `logs/screenshots/`

Troubleshooting
---------------
- PowerShell blocked the script? Run:

```powershell
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass -Force
```

- `dotnet` not found: install .NET 8 SDK at https://dotnet.microsoft.com/download
- Browser/driver mismatch: update WebDriver or use the included Selenium Manager binary

