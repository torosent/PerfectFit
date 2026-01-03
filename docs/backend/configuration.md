# Backend Configuration

The backend uses ASP.NET Core 10 configuration system with support for JSON files, environment variables, and user secrets.

## Configuration Files

### appsettings.json (Base Configuration)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=perfectfit;Username=perfectfit;Password=perfectfit_dev"
  },
  "Cors": {
    "AllowedOrigins": ["http://localhost:3000"]
  },
  "Jwt": {
    "Secret": "",
    "Issuer": "PerfectFit",
    "Audience": "PerfectFit",
    "ExpirationDays": 7
  },
  "OAuth": {
    "Google": {
      "ClientId": "",
      "ClientSecret": ""
    },
    "Microsoft": {
      "ClientId": "",
      "ClientSecret": ""
    },
    "Facebook": {
      "AppId": "",
      "AppSecret": ""
    }
  }
}
```

### appsettings.Development.json (Development Overrides)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  },
  "DatabaseProvider": "SQLite",
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=perfectfit;Username=perfectfit;Password=perfectfit_dev",
    "SqliteConnection": "Data Source=perfectfit.db"
  },
  "Cors": {
    "AllowedOrigins": ["http://localhost:3000", "http://localhost:5050"]
  }
}
```

---

## Configuration Options Reference

### Logging

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| `Logging:LogLevel:Default` | string | `Information` | Default log level |
| `Logging:LogLevel:Microsoft.AspNetCore` | string | `Warning` | ASP.NET Core logs |
| `Logging:LogLevel:Microsoft.EntityFrameworkCore` | string | `Warning` | EF Core SQL logs |

**Log Levels**: `Trace`, `Debug`, `Information`, `Warning`, `Error`, `Critical`, `None`

### Database

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| `DatabaseProvider` | string | `PostgreSQL` | Database type: `PostgreSQL` or `SQLite` |
| `ConnectionStrings:DefaultConnection` | string | - | PostgreSQL connection string |
| `ConnectionStrings:SqliteConnection` | string | `Data Source=perfectfit.db` | SQLite connection string |

**PostgreSQL Connection String Format**:
```
Host=<host>;Port=<port>;Database=<database>;Username=<user>;Password=<password>
```

**SQLite Connection String Format**:
```
Data Source=<filename.db>
```

### CORS

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| `Cors:AllowedOrigins` | string[] | `["http://localhost:3000"]` | Allowed frontend origins |

**Example**:
```json
{
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:3000",
      "https://perfectfit.example.com"
    ]
  }
}
```

### JWT Authentication

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| `Jwt:Secret` | string | - | **Required**. Secret key for signing tokens (min 32 characters) |
| `Jwt:Issuer` | string | `PerfectFit` | Token issuer identifier |
| `Jwt:Audience` | string | `PerfectFit` | Token audience identifier |
| `Jwt:ExpirationDays` | int | `7` | Token validity duration in days |

**Generating a Secret Key**:
```bash
# Using OpenSSL
openssl rand -base64 32

# Using .NET
dotnet user-secrets set "Jwt:Secret" "$(openssl rand -base64 32)"
```

### OAuth Providers

#### Google OAuth

| Key | Type | Description |
|-----|------|-------------|
| `OAuth:Google:ClientId` | string | Google OAuth client ID |
| `OAuth:Google:ClientSecret` | string | Google OAuth client secret |

**Setup**:
1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Create/select a project
3. Enable Google+ API
4. Create OAuth 2.0 credentials
5. Add authorized redirect URI: `http://localhost:5050/api/auth/callback/google`

#### Microsoft OAuth

| Key | Type | Description |
|-----|------|-------------|
| `OAuth:Microsoft:ClientId` | string | Microsoft app client ID |
| `OAuth:Microsoft:ClientSecret` | string | Microsoft app client secret |

**Setup**:
1. Go to [Azure Portal](https://portal.azure.com/)
2. Register an application in Azure AD
3. Add redirect URI: `http://localhost:5050/api/auth/callback/microsoft`
4. Create a client secret

#### Facebook OAuth

| Key | Type | Description |
|-----|------|-------------|
| `OAuth:Facebook:AppId` | string | Facebook App ID |
| `OAuth:Facebook:AppSecret` | string | Facebook App Secret |

**Setup**:
1. Go to [Facebook Developers](https://developers.facebook.com/)
2. Create a new app
3. Add Facebook Login product
4. Add redirect URI: `http://localhost:5050/api/auth/callback/facebook`

---

## Environment Variables

All configuration can be overridden using environment variables with `__` as separator:

```bash
# Database
export ConnectionStrings__DefaultConnection="Host=prod-db;..."

# JWT
export Jwt__Secret="your-production-secret"

# OAuth
export OAuth__Google__ClientId="your-client-id"
export OAuth__Google__ClientSecret="your-client-secret"
```

---

## User Secrets (Development)

Store sensitive values securely during development:

```bash
cd backend/src/PerfectFit.Web

# Initialize user secrets
dotnet user-secrets init

# Set secrets
dotnet user-secrets set "Jwt:Secret" "your-development-secret-key-minimum-32-chars"
dotnet user-secrets set "OAuth:Google:ClientId" "your-google-client-id"
dotnet user-secrets set "OAuth:Google:ClientSecret" "your-google-client-secret"

# List secrets
dotnet user-secrets list

# Clear secrets
dotnet user-secrets clear
```

---

## Configuration Priority

Configuration is loaded in this order (later overrides earlier):

1. `appsettings.json`
2. `appsettings.{Environment}.json`
3. User secrets (Development only)
4. Environment variables
5. Command-line arguments

---

## Example Configurations

### Local Development (SQLite)

```json
{
  "DatabaseProvider": "SQLite",
  "ConnectionStrings": {
    "SqliteConnection": "Data Source=perfectfit.db"
  },
  "Jwt": {
    "Secret": "development-secret-key-at-least-32-characters-long"
  }
}
```

### Docker Compose (PostgreSQL)

```json
{
  "DatabaseProvider": "PostgreSQL",
  "ConnectionStrings": {
    "DefaultConnection": "Host=postgres;Port=5432;Database=perfectfit;Username=perfectfit;Password=perfectfit_dev"
  }
}
```

### Production

```bash
# Use environment variables for all sensitive config
export ASPNETCORE_ENVIRONMENT=Production
export ConnectionStrings__DefaultConnection="Host=prod-db.example.com;..."
export Jwt__Secret="$(cat /run/secrets/jwt_secret)"
export OAuth__Google__ClientId="$(cat /run/secrets/google_client_id)"
export OAuth__Google__ClientSecret="$(cat /run/secrets/google_client_secret)"
```

---

## Validation

The application validates required configuration on startup:

- **JWT Secret**: Required and must be at least 32 characters
- **Database**: Connection string must be valid for the selected provider
- **OAuth**: Providers are only registered if credentials are provided

Missing required configuration will cause startup failure with descriptive error messages.
