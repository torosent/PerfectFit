## Phase 5 Complete: Remove Google and Facebook Providers from Backend

Removed Google and Facebook OAuth providers from the backend, leaving only Microsoft OAuth, Local (password), and Guest authentication. Enum values preserved for backward compatibility with existing data.

**Files changed:**
- backend/src/PerfectFit.Infrastructure/Identity/OAuthSettings.cs
- backend/src/PerfectFit.Web/Program.cs
- backend/src/PerfectFit.UseCases/Auth/Commands/OAuthLoginCommand.cs
- backend/src/PerfectFit.Web/appsettings.json
- backend/src/PerfectFit.Web/appsettings.Development.json
- backend/src/PerfectFit.Web/appsettings.Test.json
- backend/tests/PerfectFit.UnitTests/UseCases/Auth/Commands/OAuthLoginCommandTests.cs

**Functions removed:**
- GoogleSettings class
- FacebookSettings class
- Google OAuth scheme configuration in Program.cs

**Functions modified:**
- OAuthLoginCommand - now rejects Google and Facebook providers at runtime

**Tests created/updated:**
- OAuthLogin_GoogleProvider_ReturnsRejected
- OAuthLogin_FacebookProvider_ReturnsRejected
- Handle_Should_SupportAllowedProviders (Microsoft, Guest, Local)
- Updated existing tests to use allowed providers

**Design decisions:**
- AuthProvider enum preserved (Google=1, Facebook=2) for backward compatibility
- Runtime rejection in OAuthLoginCommand with descriptive error messages
- Complete removal of OAuth scheme configuration for Google

**Review Status:** APPROVED

**Git Commit Message:**
```
refactor: remove Google and Facebook OAuth providers

- Remove GoogleSettings and FacebookSettings classes
- Remove Google OAuth scheme from Program.cs configuration
- Add runtime rejection for Google/Facebook in OAuthLoginCommand
- Clean up appsettings files (json, Development, Test)
- Preserve AuthProvider enum values for backward compatibility
- Update tests for provider rejection scenarios
```
