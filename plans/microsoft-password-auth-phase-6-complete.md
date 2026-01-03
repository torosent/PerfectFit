## Phase 6 Complete: Update Frontend Login Page

Redesigned the frontend login page to support email/password authentication alongside Microsoft OAuth and Guest login. Removed Google and Facebook OAuth options from the UI.

**Files created:**
- frontend/src/__tests__/lib/stores/auth-store.test.ts
- frontend/src/__tests__/app/(auth)/login/page.test.tsx

**Files changed:**
- frontend/src/lib/api/auth-client.ts
- frontend/src/lib/api/index.ts
- frontend/src/lib/stores/auth-store.ts
- frontend/src/types/game.ts
- frontend/src/app/(auth)/login/page.tsx
- frontend/src/components/auth/LoginButton.tsx

**Functions created:**
- authClient.register(email, password, displayName) - API function
- authClient.login(email, password) - API function
- authClient.verifyEmail(email, token) - API function
- authStore.localLogin(email, password) - store method
- authStore.localRegister(email, password, displayName) - store method
- useLockoutEnd() - selector hook for lockout state

**Types added:**
- RegisterResponse, LoginResponse, VerifyEmailResponse
- AuthProvider updated to include 'local'

**UI changes:**
- Login form with email/password inputs
- Client-side validation for email format and required fields
- Microsoft OAuth button (only OAuth option)
- Guest login option preserved
- "Don't have an account? Register" link
- Error message display
- Lockout end time display when account is locked
- Loading states during form submission

**Tests created:**
- auth-store.test.ts (7 tests) - localLogin, localRegister, lockout handling
- login/page.test.tsx (11 tests) - form rendering, validation, errors, lockout

**Review Status:** APPROVED

**Git Commit Message:**
```
feat: redesign login page with password auth and Microsoft OAuth

- Add register, login, verifyEmail API functions to auth-client
- Add localLogin, localRegister methods to auth-store
- Add lockout state tracking for brute force protection UX
- Redesign login page with email/password form
- Keep Microsoft OAuth and Guest login options
- Remove Google and Facebook buttons from UI
- Add client-side form validation
- Display lockout end time when account is locked
- Add 18 frontend tests for auth functionality
```
