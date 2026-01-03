# Admin API Reference

## Overview

The Admin API provides endpoints for user management, including viewing all users, soft-deleting users, bulk operations, and audit logging. All admin endpoints require authentication with an Admin role.

## Authorization

All admin endpoints are protected by the `AdminPolicy` which requires:
1. Valid JWT authentication
2. User must have `Admin` role

### Admin Role Assignment

Users are automatically granted admin role when:
- Their email matches an entry in `Admin:Emails` configuration
- Promotion happens on login (new users get admin role immediately, existing users are promoted)

### Configuration

Add admin emails in `appsettings.json`:

```json
{
  "Admin": {
    "Emails": [
      "admin@example.com",
      "another-admin@example.com"
    ]
  }
}
```

---

## Admin Endpoints

### List All Users

Get a paginated list of all users.

```http
GET /api/admin/users
```

**Headers** (Required):
```
Authorization: Bearer <admin_token>
```

**Query Parameters**:
| Name | Type | Default | Description |
|------|------|---------|-------------|
| page | number | 1 | Page number (1-indexed) |
| pageSize | number | 20 | Items per page (max 100) |

**Response**: `200 OK`
```json
{
  "items": [
    {
      "id": 1,
      "email": "user@example.com",
      "displayName": "John Doe",
      "username": "johndoe",
      "avatar": "🎮",
      "provider": "Google",
      "role": "User",
      "createdAt": "2025-12-01T10:00:00Z",
      "lastLoginAt": "2026-01-02T08:30:00Z",
      "highScore": 5000,
      "gamesPlayed": 42,
      "isDeleted": false,
      "deletedAt": null
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalCount": 150,
  "totalPages": 8
}
```

**Errors**:
- `401 Unauthorized` - Not authenticated
- `403 Forbidden` - Not an admin

---

### Get User Details

Get detailed information about a specific user.

```http
GET /api/admin/users/{id}
```

**Headers** (Required):
```
Authorization: Bearer <admin_token>
```

**Parameters**:
| Name | Type | Description |
|------|------|-------------|
| id | number | User ID |

**Response**: `200 OK`
```json
{
  "id": 1,
  "email": "user@example.com",
  "displayName": "John Doe",
  "username": "johndoe",
  "avatar": "🎮",
  "provider": "Google",
  "role": "User",
  "createdAt": "2025-12-01T10:00:00Z",
  "lastLoginAt": "2026-01-02T08:30:00Z",
  "highScore": 5000,
  "gamesPlayed": 42,
  "isDeleted": false,
  "deletedAt": null
}
```

**Errors**:
- `401 Unauthorized` - Not authenticated
- `403 Forbidden` - Not an admin
- `404 Not Found` - User not found

---

### Delete User (Soft Delete)

Soft delete a user. Sets `isDeleted = true` and `deletedAt` timestamp.

```http
DELETE /api/admin/users/{id}
```

**Headers** (Required):
```
Authorization: Bearer <admin_token>
```

**Parameters**:
| Name | Type | Description |
|------|------|-------------|
| id | number | User ID to delete |

**Response**: `204 No Content`

**Errors**:
- `400 Bad Request` - Cannot delete yourself
- `401 Unauthorized` - Not authenticated
- `403 Forbidden` - Not an admin
- `404 Not Found` - User not found

**Self-Delete Protection**:
Admins cannot delete their own account. This prevents accidental lockout.

---

### Bulk Delete Guest Users

Soft delete all guest (anonymous) users.

```http
DELETE /api/admin/users/bulk/guests
```

**Headers** (Required):
```
Authorization: Bearer <admin_token>
```

**Response**: `200 OK`
```json
{
  "deletedCount": 45,
  "message": "Successfully deleted 45 guest users"
}
```

**Errors**:
- `401 Unauthorized` - Not authenticated
- `403 Forbidden` - Not an admin

---

### Get Audit Logs

View admin action audit logs.

```http
GET /api/admin/audit-logs
```

**Headers** (Required):
```
Authorization: Bearer <admin_token>
```

**Query Parameters**:
| Name | Type | Default | Description |
|------|------|---------|-------------|
| page | number | 1 | Page number (1-indexed) |
| pageSize | number | 20 | Items per page (max 100) |

**Response**: `200 OK`
```json
{
  "items": [
    {
      "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
      "adminUserId": 1,
      "adminEmail": "admin@example.com",
      "action": "DeleteUser",
      "targetUserId": 42,
      "targetUserEmail": "deleted-user@example.com",
      "details": null,
      "timestamp": "2026-01-02T14:30:00Z"
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalCount": 25,
  "totalPages": 2
}
```

**Errors**:
- `401 Unauthorized` - Not authenticated
- `403 Forbidden` - Not an admin

---

## Audit Logging

All admin actions are automatically logged for accountability. The following actions are tracked:

| Action | Description |
|--------|-------------|
| `ViewUsers` | Admin viewed the users list |
| `ViewUser` | Admin viewed a specific user's details |
| `DeleteUser` | Admin soft-deleted a user |
| `BulkDeleteUsers` | Admin performed bulk delete operation |

### Audit Log Entry Structure

| Field | Type | Description |
|-------|------|-------------|
| id | GUID | Unique audit log entry ID |
| adminUserId | number | ID of admin who performed action |
| adminEmail | string | Email of admin (for display) |
| action | string | Action type (see table above) |
| targetUserId | number? | ID of affected user (if applicable) |
| targetUserEmail | string? | Email of affected user (if applicable) |
| details | string? | Additional JSON details |
| timestamp | datetime | When the action occurred |

---

## Data Types

### UserRole

| Value | Description |
|-------|-------------|
| `User` | Standard user (default) |
| `Admin` | Administrator with full access |

### AdminUser DTO

| Field | Type | Description |
|-------|------|-------------|
| id | number | User ID |
| email | string? | User email (null for guests) |
| displayName | string? | Display name |
| username | string? | Username |
| avatar | string? | Avatar emoji |
| provider | string | Auth provider (Guest, Google, etc.) |
| role | string | User role (User, Admin) |
| createdAt | datetime | Account creation date |
| lastLoginAt | datetime? | Last login timestamp |
| highScore | number | User's high score |
| gamesPlayed | number | Total games played |
| isDeleted | boolean | Soft delete flag |
| deletedAt | datetime? | When user was deleted |

### PaginatedResponse<T>

| Field | Type | Description |
|-------|------|-------------|
| items | T[] | Array of items |
| page | number | Current page (1-indexed) |
| pageSize | number | Items per page |
| totalCount | number | Total items across all pages |
| totalPages | number | Total number of pages |

---

## Soft Delete Behavior

When a user is soft-deleted:
1. `isDeleted` is set to `true`
2. `deletedAt` is set to current UTC timestamp
3. User is excluded from normal queries (global query filter)
4. User's data is preserved in database
5. User cannot log in (treated as non-existent)
6. Related data (game sessions, leaderboard entries) remains intact

### Viewing Deleted Users

Soft-deleted users are automatically excluded from most queries. The admin API includes both active and deleted users in responses, with the `isDeleted` flag indicating status.
