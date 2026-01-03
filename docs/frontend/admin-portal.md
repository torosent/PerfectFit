# Admin Portal

## Overview

The Admin Portal provides a web interface for administrators to manage users, view audit logs, and perform bulk operations. It is accessible at `/admin` and requires admin authentication.

## Access Requirements

To access the admin portal:
1. Must be logged in (authenticated)
2. Must have `Admin` role
3. Role is assigned based on email in `Admin:Emails` configuration

Non-admin users are automatically redirected to the home page.

---

## Features

### Users Management

The Users tab displays all registered users with the following information:

| Column | Description |
|--------|-------------|
| ID | Unique user identifier |
| Email | User's email address (null for guests) |
| Display Name | User's display name |
| Provider | Authentication provider (Guest, Google, etc.) |
| Role | User role (User or Admin) |
| Created | Account creation date |
| Actions | Delete button |

#### Pagination

- Default page size: 20 users
- Navigate with Previous/Next buttons
- Shows current page and total pages

#### Delete User

1. Click the "Delete" button on a user row
2. Confirmation modal appears with user details
3. Click "Delete" to confirm soft-delete
4. User is marked as deleted (soft delete)

**Note**: You cannot delete your own account (self-delete protection).

#### Bulk Delete Guests

1. Click "Delete All Guests" button in header
2. Confirmation modal appears with warning
3. Click "Delete All Guests" to confirm
4. All guest users are soft-deleted
5. Success message shows count of deleted users

### Audit Logs

The Audit Logs tab displays all admin actions:

| Column | Description |
|--------|-------------|
| Timestamp | When the action occurred |
| Admin | ID of admin who performed action |
| Action | Action type with color-coded badge |
| Target | ID of affected user (if applicable) |
| Details | Additional information |

#### Action Badges

| Color | Actions |
|-------|---------|
| 🔴 Red | DeleteUser, BulkDeleteUsers |
| 🟢 Green | ViewUsers, ViewUser |
| 🔵 Blue | Other actions |

#### Pagination

- Default page size: 20 entries
- Navigate with Previous/Next buttons
- Logs sorted by timestamp (newest first)

---

## Components

### AdminGuard

Route protection component that:
- Initializes authentication state
- Checks if user is authenticated
- Checks if user has Admin role
- Shows loading spinner while checking
- Redirects non-admins to home page

```tsx
import { AdminGuard } from '@/components/admin';

<AdminGuard>
  <AdminContent />
</AdminGuard>
```

### UsersTable

Displays paginated user list with actions.

```tsx
import { UsersTable } from '@/components/admin';

<UsersTable />
```

Props: None (fetches data internally)

### DeleteUserModal

Confirmation modal for single user deletion.

```tsx
import { DeleteUserModal } from '@/components/admin';

<DeleteUserModal
  user={selectedUser}
  isOpen={isModalOpen}
  onClose={() => setIsModalOpen(false)}
  onSuccess={() => refetchUsers()}
/>
```

| Prop | Type | Description |
|------|------|-------------|
| user | AdminUser \| null | User to delete |
| isOpen | boolean | Modal visibility |
| onClose | () => void | Close handler |
| onSuccess | () => void | Success callback |

### BulkDeleteModal

Confirmation modal for bulk guest deletion.

```tsx
import { BulkDeleteModal } from '@/components/admin';

<BulkDeleteModal
  isOpen={isBulkModalOpen}
  onClose={() => setIsBulkModalOpen(false)}
  onSuccess={() => refetchUsers()}
/>
```

| Prop | Type | Description |
|------|------|-------------|
| isOpen | boolean | Modal visibility |
| onClose | () => void | Close handler |
| onSuccess | () => void | Success callback |

### AuditLogTable

Displays paginated audit logs.

```tsx
import { AuditLogTable } from '@/components/admin';

<AuditLogTable />
```

Props: None (fetches data internally)

---

## API Client

The admin API client provides functions for all admin operations:

```typescript
import { 
  getUsers, 
  getUser, 
  deleteUser, 
  bulkDeleteGuests, 
  getAuditLogs 
} from '@/lib/api/admin-client';

// Get paginated users
const users = await getUsers(1, 20);

// Get single user
const user = await getUser(42);

// Delete user (soft delete)
await deleteUser(42);

// Bulk delete all guests
const result = await bulkDeleteGuests();
console.log(`Deleted ${result.deletedCount} guests`);

// Get audit logs
const logs = await getAuditLogs(1, 20);
```

### Error Handling

```typescript
import { AdminApiError } from '@/lib/api/admin-client';

try {
  await deleteUser(42);
} catch (error) {
  if (error instanceof AdminApiError) {
    console.log(error.status); // HTTP status code
    console.log(error.message); // Error message
  }
}
```

---

## Auth Store Integration

### useIsAdmin Hook

Check if current user is an admin:

```typescript
import { useIsAdmin } from '@/lib/stores/auth-store';

function MyComponent() {
  const isAdmin = useIsAdmin();
  
  if (isAdmin) {
    return <AdminFeatures />;
  }
  return <RegularFeatures />;
}
```

### User Role in Profile

The user profile includes role information:

```typescript
interface UserProfile {
  id: number;
  displayName: string;
  email: string | null;
  provider: string;
  role?: string; // 'User' | 'Admin'
  // ... other fields
}
```

---

## Styling

The admin portal uses the same teal/dark theme as the rest of the application:

| Element | Color |
|---------|-------|
| Background | `#0a1628` |
| Card Background | `#0f1d32` |
| Border | `#1e3a5f` |
| Primary Accent | `#14b8a6` (teal) |
| Secondary Accent | `#0ea5e9` (cyan) |
| Text | `#ffffff` (white) |
| Muted Text | `#94a3b8` (gray) |

### Responsive Design

- Table scrolls horizontally on mobile
- Modal width adapts to screen size
- Touch-friendly button sizes

---

## File Structure

```
frontend/src/
├── app/(admin)/admin/
│   ├── layout.tsx      # Admin layout with guard
│   └── page.tsx        # Admin dashboard page
├── components/admin/
│   ├── index.ts        # Barrel exports
│   ├── AdminGuard.tsx  # Route protection
│   ├── UsersTable.tsx  # Users list component
│   ├── DeleteUserModal.tsx
│   ├── BulkDeleteModal.tsx
│   └── AuditLogTable.tsx
└── lib/api/
    └── admin-client.ts # API client functions
```

---

## Security Considerations

1. **Server-side Authorization**: All admin endpoints verify the admin role server-side
2. **Client-side Guard**: AdminGuard provides UX protection but is not security
3. **Audit Logging**: All admin actions are logged for accountability
4. **Self-Delete Protection**: Admins cannot delete themselves
5. **Soft Delete**: Users are not permanently deleted, allowing recovery if needed
