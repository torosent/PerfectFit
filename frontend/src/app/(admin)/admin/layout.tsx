import type { Metadata } from 'next';
import { AdminGuard } from '@/components/admin/AdminGuard';

export const metadata: Metadata = {
  title: 'Admin Portal - PerfectFit',
  description: 'Admin portal for managing PerfectFit users and viewing audit logs',
};

/**
 * Admin layout - protects all admin routes with AdminGuard
 */
export default function AdminLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return <AdminGuard>{children}</AdminGuard>;
}
