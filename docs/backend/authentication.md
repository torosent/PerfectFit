```markdown
# Authentication

## Overview

PerfectFit supports three authentication methods:
1. **Local Authentication** - Email/password registration and login
2. **Microsoft OAuth** - Sign in with Microsoft account
3. **Guest Authentication** - Play without registration

## Authentication Methods

### Local Authentication (Email/Password)

Users can register with an email address and password. This provides a traditional authentication experience with email verification.

#### Registration

**Endpoint**: `POST /api/auth/register`

**Request Body**:
```json
{
  "email": "user@example.com",
  "password": "SecurePass123",
  "displayName": "PlayerName"
}
```

**Response** (201 Created):
```json
{
  "message": "Registration successful. Please check your email to verify your account.",
  "userId": 123
}
```

**Password Requirements**:
- Minimum 8 characters
- At least one uppercase letter (A-Z)
- At least one lowercase letter (a-z)
- At least one number (0-9)

#### Email Verification

After registration, users receive an email with a verification link. The email must be verified before the user can log in.

**Email Features:**
- Professional HTML email template with PerfectFit branding
- Personalized greeting with user's display name
- Clear call-to-action button to verify email
- Plain text fallback for email clients that don't support HTML
- 24-hour expiry notice
- "If you didn't create an account" notice

**Endpoint**: `POST /api/auth/verify-email`

**Request Body**:
```json
{
  "token": "verification-token-from-email"
}
```

**Response** (200 OK):
```json
{
  "message": "Email verified successfully. You can now log in."
}
```

#### Login

**Endpoint**: `POST /api/auth/login`

**Request Body**:
```json
{
  "email": "user@example.com",
  "password": "SecurePass123"
}
```

**Response** (200 OK):
```json
{
  "token": "jwt-token",
  "user": {
    "id": 123,
    "email": "user@example.com",
    "displayName": "PlayerName",
    "authProvider": "Local"
  }
}
```

**Error Responses**:
- `401 Unauthorized` - Invalid credentials
- `403 Forbidden` - Email not verified
- `423 Locked` - Account locked due to too many failed attempts

### Microsoft OAuth

Users can sign in using their Microsoft account (personal or work/school accounts).

**Endpoint**: `GET /api/auth/microsoft`

Redirects to Microsoft's OAuth consent page. After successful authentication, redirects back to the application with a JWT token.

**Callback**: `GET /api/auth/microsoft/callback`

Handles the OAuth callback from Microsoft and issues a JWT token.

### Guest Authentication

Users can play as guests without registration. Guest accounts are temporary and can later be converted to full accounts.

**Endpoint**: `POST /api/auth/guest`

**Response** (200 OK):
```json
{
  "token": "jwt-token",
  "user": {
    "id": 456,
    "displayName": "Guest_abc123",
    "authProvider": "Guest"
  }
}
```

## Security Features

### Password Hashing

Passwords are hashed using BCrypt with a work factor of 12. This provides strong protection against brute-force attacks while maintaining reasonable performance.

### Account Lockout

To prevent brute-force password attacks:
- **Lockout Threshold**: 5 failed login attempts
- **Lockout Duration**: 15 minutes
- **Tracking**: Failed attempts are tracked per email address

After 5 failed attempts, the account is locked for 15 minutes. The lockout counter resets after a successful login.

### Rate Limiting

Authentication endpoints are rate-limited to prevent abuse:

| Endpoint | Rate Limit |
|----------|------------|
| `POST /api/auth/login` | 5 requests per minute |
| `POST /api/auth/register` | 3 requests per minute |
| `POST /api/auth/verify-email` | 10 requests per minute |
| `GET /api/auth/microsoft` | 10 requests per minute |
| `POST /api/auth/guest` | 10 requests per minute |

Rate limits are applied per IP address.

### JWT Token Configuration

| Setting | Value |
|---------|-------|
| Algorithm | HS256 |
| Expiration | 7 days |
| Issuer | PerfectFit |
| Audience | PerfectFit |

### Email Verification

- Verification tokens expire after 24 hours
- Users can request a new verification email via `POST /api/auth/resend-verification`
- Unverified accounts cannot log in via local authentication

## Configuration

### Environment Variables

Configure authentication in `appsettings.json` or via environment variables:

```json
{
  "Jwt": {
    "Secret": "your-secret-key-at-least-32-characters-long",
    "Issuer": "PerfectFit",
    "Audience": "PerfectFit",
    "ExpirationInDays": 7
  },
  "OAuth": {
    "Microsoft": {
      "ClientId": "your-microsoft-client-id",
      "ClientSecret": "your-microsoft-client-secret",
      "TenantId": "common"
    }
  },
  "Email": {
    "ConnectionString": "endpoint=https://xxx.communication.azure.com/;accesskey=xxx",
    "SenderAddress": "DoNotReply@xxx.azurecomm.net",
    "VerificationEmailSubject": "Verify your PerfectFit account",
    "FrontendUrl": "https://your-frontend-url.com"
  },
  "Auth": {
    "LockoutThreshold": 5,
    "LockoutDurationMinutes": 15,
    "EmailVerificationRequired": true
  }
}
```

### Email Configuration

PerfectFit uses Azure Communication Services for sending verification emails.

| Setting | Description |
|---------|-------------|
| `Email:ConnectionString` | Azure Communication Services connection string |
| `Email:SenderAddress` | Sender email address (e.g., DoNotReply@xxx.azurecomm.net) |
| `Email:VerificationEmailSubject` | Subject line for verification emails (default: "Verify your PerfectFit account") |
| `Email:FrontendUrl` | Frontend URL used to build verification links |

**Setting up Azure Communication Services:**
1. Create an Azure Communication Services resource in the Azure Portal
2. Add a verified email domain (or use Azure-managed domain)
3. Copy the connection string from the resource's "Keys" section
4. Configure the sender address using your verified domain

**For local development:**
If the connection string is empty, emails will be logged but not sent. The verification token is logged for manual testing.

### User Secrets (Development)

For local development, use .NET user secrets:

```bash
cd backend/src/PerfectFit.Web

# Set JWT secret
dotnet user-secrets set "Jwt:Secret" "your-secret-key-at-least-32-characters-long"

# Set Microsoft OAuth credentials
dotnet user-secrets set "OAuth:Microsoft:ClientId" "your-client-id"
dotnet user-secrets set "OAuth:Microsoft:ClientSecret" "your-client-secret"
```

## Database Schema

### User Table

| Column | Type | Description |
|--------|------|-------------|
| `Id` | int | Primary key |
| `Email` | string | Unique email address (nullable for guests) |
| `PasswordHash` | string | BCrypt password hash (nullable for OAuth/guest) |
| `DisplayName` | string | User's display name |
| `AuthProvider` | string | 'Local', 'Microsoft', or 'Guest' |
| `ExternalId` | string | Microsoft account ID (for OAuth users) |
| `EmailVerified` | bool | Whether email has been verified |
| `EmailVerificationToken` | string | Token for email verification |
| `EmailVerificationExpiry` | DateTime | When verification token expires |
| `FailedLoginAttempts` | int | Count of failed login attempts |
| `LockoutEndTime` | DateTime | When lockout expires (null if not locked) |
| `CreatedAt` | DateTime | Account creation timestamp |
| `LastLoginAt` | DateTime | Last successful login |

## Error Codes

| Code | Description |
|------|-------------|
| `AUTH_INVALID_CREDENTIALS` | Email or password is incorrect |
| `AUTH_EMAIL_NOT_VERIFIED` | Email address has not been verified |
| `AUTH_ACCOUNT_LOCKED` | Account is locked due to too many failed attempts |
| `AUTH_EMAIL_ALREADY_EXISTS` | Email address is already registered |
| `AUTH_INVALID_PASSWORD` | Password does not meet requirements |
| `AUTH_INVALID_VERIFICATION_TOKEN` | Verification token is invalid or expired |
| `AUTH_RATE_LIMITED` | Too many requests, try again later |

## API Response Examples

### Successful Login
```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "user": {
    "id": 123,
    "email": "user@example.com",
    "displayName": "PlayerName",
    "authProvider": "Local",
    "emailVerified": true
  }
}
```

### Registration Validation Error
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Validation Error",
  "status": 400,
  "errors": {
    "password": [
      "Password must be at least 8 characters",
      "Password must contain at least one uppercase letter"
    ]
  }
}
```

### Account Locked
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.3",
  "title": "Account Locked",
  "status": 423,
  "detail": "Account is locked due to too many failed login attempts. Try again in 14 minutes."
}
```
```
