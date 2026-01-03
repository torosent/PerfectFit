## Plan: Microsoft + Password Authentication

Simplify authentication to support only Microsoft OAuth and username/password login, removing Google and Facebook providers. Includes password infrastructure with bcrypt hashing, email verification, account lockout, and rate limiting security measures.

**Phases: 9**

### 1. Phase 1: Add Password Infrastructure to Backend
- **Objective:** Add password hashing capability and extend the User entity to support local authentication
- **Files/Functions to Modify/Create:**
  - `src/PerfectFit.Core/Entities/User.cs` - add `PasswordHash` field
  - `src/PerfectFit.Core/Identity/IPasswordHasher.cs` - new interface
  - `src/PerfectFit.Core/Identity/BCryptPasswordHasher.cs` - implementation
  - `src/PerfectFit.Core/Enums/AuthProvider.cs` - add `Local` enum value
  - Database migration for new field
- **Tests to Write:**
  - `BCryptPasswordHasherTests.cs` - test hash generation, verification, different passwords, null handling
- **Steps:**
  1. Write unit tests for password hasher (BCrypt)
  2. Run tests (expect failure)
  3. Implement `IPasswordHasher` interface and `BCryptPasswordHasher`
  4. Add `PasswordHash` nullable field to `User` entity
  5. Add `Local` to `AuthProvider` enum
  6. Register password hasher in DI
  7. Create EF migration
  8. Run tests to confirm passing

### 2. Phase 2: Add Email Verification Infrastructure
- **Objective:** Add email verification token generation and storage for new registrations
- **Files/Functions to Modify/Create:**
  - `src/PerfectFit.Core/Entities/User.cs` - add `EmailVerified`, `EmailVerificationToken`, `EmailVerificationTokenExpiry` fields
  - `src/PerfectFit.Core/Identity/IEmailVerificationService.cs` - interface
  - `src/PerfectFit.Core/Identity/EmailVerificationService.cs` - token generation
  - Database migration for new fields
- **Tests to Write:**
  - `EmailVerificationServiceTests.cs` - test token generation, expiry, validation
- **Steps:**
  1. Write unit tests for email verification service
  2. Run tests (expect failure)
  3. Add email verification fields to User entity
  4. Implement `IEmailVerificationService`
  5. Register service in DI
  6. Create EF migration
  7. Run tests to confirm passing

### 3. Phase 3: Create Registration and Login Endpoints
- **Objective:** Add endpoints for local user registration and login with proper validation
- **Files/Functions to Modify/Create:**
  - `src/PerfectFit.Core/Features/Auth/Commands/RegisterCommand.cs` - registration with email verification
  - `src/PerfectFit.Core/Features/Auth/Commands/LocalLoginCommand.cs` - login command
  - `src/PerfectFit.Core/Features/Auth/Commands/VerifyEmailCommand.cs` - email verification
  - `src/PerfectFit.Api/Endpoints/AuthEndpoints.cs` - add `/register`, `/login`, `/verify-email` endpoints
- **Tests to Write:**
  - `RegisterCommandTests.cs` - valid registration, duplicate email, weak password, email token sent
  - `LocalLoginCommandTests.cs` - valid login, wrong password, non-existent user, unverified email
  - `VerifyEmailCommandTests.cs` - valid token, expired token, invalid token
- **Steps:**
  1. Write tests for registration command
  2. Write tests for local login command
  3. Write tests for verify email command
  4. Run tests (expect failure)
  5. Implement `RegisterCommand` with email/password validation and verification token
  6. Implement `LocalLoginCommand` with password verification and email check
  7. Implement `VerifyEmailCommand`
  8. Add endpoints to `AuthEndpoints.cs`
  9. Run tests to confirm passing

### 4. Phase 4: Add Security Measures (Rate Limiting & Account Lockout)
- **Objective:** Protect login endpoints from brute force attacks with 15-minute lockout after 5 failures
- **Files/Functions to Modify/Create:**
  - `src/PerfectFit.Core/Entities/User.cs` - add `FailedLoginAttempts`, `LockoutEnd` fields
  - `src/PerfectFit.Core/Features/Auth/Commands/LocalLoginCommand.cs` - add lockout logic
  - `src/PerfectFit.Api/Endpoints/AuthEndpoints.cs` - add rate limiting to login/register endpoints
- **Tests to Write:**
  - `LoginLockoutTests.cs` - test lockout after 5 failures, lockout duration (15 min), reset on success
- **Steps:**
  1. Write lockout behavior tests
  2. Run tests (expect failure)
  3. Add lockout fields to User entity
  4. Update `LocalLoginCommand` to check/update lockout state
  5. Add rate limiting to auth endpoints (5 requests per minute)
  6. Create migration for new fields
  7. Run tests to confirm passing

### 5. Phase 5: Remove Google and Facebook Providers from Backend
- **Objective:** Clean up backend to only support Microsoft OAuth, Local, and Guest authentication
- **Files/Functions to Modify/Create:**
  - `src/PerfectFit.Core/Identity/OAuthSettings.cs` - remove Google, Facebook config classes
  - `src/PerfectFit.Api/Program.cs` - remove Google/Facebook OAuth schemes
  - `src/PerfectFit.Core/Features/Auth/Commands/OAuthLoginCommand.cs` - remove Google/Facebook handling
  - `src/PerfectFit.Api/appsettings.json` - remove Google/Facebook sections
- **Tests to Write:**
  - Update `OAuthLoginCommandTests.cs` - verify Google/Facebook are rejected
- **Steps:**
  1. Update tests to verify only Microsoft provider is accepted
  2. Run tests (may pass/fail depending on current state)
  3. Remove Google and Facebook settings classes
  4. Remove Google/Facebook OAuth schemes from `Program.cs`
  5. Update `OAuthLoginCommand` to reject non-Microsoft providers
  6. Clean up `appsettings.json`
  7. Run all tests to confirm system works

### 6. Phase 6: Update Frontend Login Page
- **Objective:** Replace OAuth buttons with Microsoft button + username/password form, keep Guest option
- **Files/Functions to Modify/Create:**
  - `frontend/src/app/(auth)/login/page.tsx` - redesign with email/password form, Microsoft button, Guest
  - `frontend/src/lib/api/auth-client.ts` - add `register()`, `login()`, `verifyEmail()` API functions
  - `frontend/src/lib/stores/auth-store.ts` - add local login/register methods
  - `frontend/src/types/game.ts` - update `AuthProvider` type
  - `frontend/src/components/auth/LoginButton.tsx` - simplify to Microsoft only
- **Tests to Write:**
  - `login.test.tsx` - test form validation, submission, error states
  - `auth-store.test.ts` - test local login/register flows
- **Steps:**
  1. Write frontend tests for login form
  2. Write tests for auth store local methods
  3. Run tests (expect failure)
  4. Update `auth-client.ts` with new API functions
  5. Update `auth-store.ts` with local auth methods
  6. Redesign login page with form, Microsoft button, and Guest option
  7. Update types
  8. Run tests to confirm passing

### 7. Phase 7: Add Registration and Email Verification Pages
- **Objective:** Create registration page and email verification flow for password users
- **Files/Functions to Modify/Create:**
  - `frontend/src/app/(auth)/register/page.tsx` - new registration page
  - `frontend/src/app/(auth)/verify-email/page.tsx` - email verification page
  - `frontend/src/components/auth/PasswordStrengthIndicator.tsx` - password strength UI
  - Links between login, register, and verification pages
- **Tests to Write:**
  - `register.test.tsx` - test form validation, password strength, submission
  - `verify-email.test.tsx` - test token handling, success/error states
- **Steps:**
  1. Write tests for registration page
  2. Write tests for verification page
  3. Run tests (expect failure)
  4. Create registration page with email/password/confirm fields
  5. Add password strength validation (min 8 chars, uppercase, lowercase, number)
  6. Create email verification page
  7. Add links between pages
  8. Run tests to confirm passing

### 8. Phase 8: Update Documentation
- **Objective:** Update all documentation to reflect new authentication system
- **Files/Functions to Modify/Create:**
  - `docs/backend/authentication.md` - update for local auth + Microsoft only
  - `docs/development.md` - update setup instructions
  - `docs/deployment.md` - update environment variables (remove Google/Facebook)
  - `docs/frontend/components.md` - update auth component docs
- **Tests to Write:** N/A (documentation only)
- **Steps:**
  1. Update backend authentication documentation
  2. Update development guide
  3. Update deployment guide
  4. Update frontend component documentation
  5. Review all docs for consistency

### 9. Phase 9: Add Email Sending with Azure Communication Services
- **Objective:** Send verification emails using Azure Communication Services
- **Files/Functions to Modify/Create:**
  - `src/PerfectFit.Core/Services/IEmailService.cs` - email service interface
  - `src/PerfectFit.Infrastructure/Email/EmailSettings.cs` - configuration
  - `src/PerfectFit.Infrastructure/Email/AzureEmailService.cs` - implementation
  - `src/PerfectFit.UseCases/Auth/Commands/RegisterCommand.cs` - integrate email sending
  - `appsettings.json` - add Email configuration section
  - `docs/deployment.md` - add Email environment variables
- **Tests to Write:**
  - `AzureEmailServiceTests.cs` - test email building, verification URL, error handling
- **Steps:**
  1. Add Azure.Communication.Email NuGet package
  2. Write unit tests for email service
  3. Create IEmailService interface
  4. Implement AzureEmailService with HTML template
  5. Update RegisterCommand to send verification email
  6. Register services in DI
  7. Update configuration files
  8. Update documentation
  9. Run tests to confirm passing

---

**Decisions Made:**
- No existing users to migrate (clean removal of Google/Facebook)
- Email verification required for new registrations
- Password requirements: min 8 chars, uppercase, lowercase, number
- Account lockout: 15 minutes after 5 failed attempts
- Guest login preserved
