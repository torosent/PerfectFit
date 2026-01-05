// Re-export all game types from this index file
export * from './game';

// Admin types
export interface AdminUser {
  id: number;
  email: string | null;
  displayName: string | null;
  avatar: string | null;
  provider: string;
  role: string;
  createdAt: string;
  lastLoginAt: string | null;
  highScore: number;
  gamesPlayed: number;
  isDeleted: boolean;
  deletedAt: string | null;
}

export interface PaginatedResponse<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface AuditLog {
  id: string;
  adminUserId: number;
  adminEmail: string | null;
  action: string;
  targetUserId: number | null;
  targetUserEmail: string | null;
  details: string | null;
  timestamp: string;
}

export interface BulkDeleteResponse {
  deletedCount: number;
  message: string;
}
