'use client';

import { useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { useIsAuthenticated, useIsAdmin, useIsAuthInitialized } from '@/lib/stores/auth-store';

export interface AdminGuardProps {
  children: React.ReactNode;
}

/**
 * AdminGuard - Role protection component for admin routes.
 * 
 * Checks if user is authenticated AND has admin role.
 * If not admin, redirects to home page.
 * Shows loading state while checking authentication.
 */
export function AdminGuard({ children }: AdminGuardProps) {
  const router = useRouter();
  const isAuthenticated = useIsAuthenticated();
  const isAdmin = useIsAdmin();
  const isInitialized = useIsAuthInitialized();

  useEffect(() => {
    // Wait for auth to be initialized
    if (!isInitialized) return;

    // Redirect non-authenticated or non-admin users to home
    if (!isAuthenticated || !isAdmin) {
      router.push('/');
    }
  }, [isAuthenticated, isAdmin, isInitialized, router]);

  // Show loading state while auth is initializing
  if (!isInitialized) {
    return (
      <div className="min-h-screen flex items-center justify-center" style={{ backgroundColor: '#0a1628' }}>
        <div className="text-center">
          <div
            className="w-12 h-12 border-4 border-t-teal-500 border-gray-700 rounded-full animate-spin mx-auto mb-4"
          />
          <p className="text-gray-400">Loading...</p>
        </div>
      </div>
    );
  }

  // Don't render children if not authenticated or not admin
  if (!isAuthenticated || !isAdmin) {
    return null;
  }

  // Render children when authorized
  return <>{children}</>;
}
