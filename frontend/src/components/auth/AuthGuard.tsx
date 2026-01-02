'use client';

import { useEffect, type ReactNode } from 'react';
import { useRouter } from 'next/navigation';
import {
  useAuthStore,
  useIsAuthenticated,
  useIsAuthLoading,
  useIsAuthInitialized,
} from '@/lib/stores/auth-store';

/**
 * Props for AuthGuard component
 */
interface AuthGuardProps {
  children: ReactNode;
  /** URL to redirect to if not authenticated */
  redirectTo?: string;
  /** If true, shows login prompt instead of redirecting */
  showPrompt?: boolean;
  /** If true, allows unauthenticated access but initializes auth state */
  optional?: boolean;
}

/**
 * Loading skeleton component
 */
function LoadingSkeleton() {
  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-b from-gray-900 to-black">
      <div className="flex flex-col items-center gap-4">
        <div className="animate-spin rounded-full h-12 w-12 border-4 border-blue-500 border-t-transparent" />
        <p className="text-gray-400 text-sm">Loading...</p>
      </div>
    </div>
  );
}

/**
 * Login prompt component for optional auth
 */
interface LoginPromptProps {
  onLogin: () => void;
}

function LoginPrompt({ onLogin }: LoginPromptProps) {
  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-b from-gray-900 to-black p-4">
      <div className="text-center max-w-md">
        <h2 className="text-2xl font-bold text-white mb-4">Sign In Required</h2>
        <p className="text-gray-400 mb-6">
          Please sign in to access this page and save your progress.
        </p>
        <button
          onClick={onLogin}
          className={`
            py-3 px-8 bg-blue-600 hover:bg-blue-500
            text-white font-medium rounded-lg
            transition-colors
          `}
        >
          Sign In
        </button>
      </div>
    </div>
  );
}

/**
 * AuthGuard component for protecting routes
 * 
 * - If not authenticated and redirectTo is set, redirects to login page
 * - If not authenticated and showPrompt is true, shows login prompt
 * - If optional is true, renders children regardless of auth state
 * - Shows loading state while auth is initializing
 */
export function AuthGuard({
  children,
  redirectTo = '/login',
  showPrompt = false,
  optional = false,
}: AuthGuardProps) {
  const router = useRouter();
  const isAuthenticated = useIsAuthenticated();
  const isLoading = useIsAuthLoading();
  const isInitialized = useIsAuthInitialized();
  const initializeAuth = useAuthStore((state) => state.initializeAuth);

  // Initialize auth on mount
  useEffect(() => {
    initializeAuth();
  }, [initializeAuth]);

  // Handle redirect for non-authenticated users
  useEffect(() => {
    if (isInitialized && !isAuthenticated && !optional && !showPrompt) {
      router.push(redirectTo);
    }
  }, [isInitialized, isAuthenticated, optional, showPrompt, redirectTo, router]);

  // Show loading while initializing
  if (!isInitialized || isLoading) {
    return <LoadingSkeleton />;
  }

  // If optional, always render children (auth state available for components)
  if (optional) {
    return <>{children}</>;
  }

  // If authenticated, render children
  if (isAuthenticated) {
    return <>{children}</>;
  }

  // If not authenticated and showPrompt, show login prompt
  if (showPrompt) {
    return <LoginPrompt onLogin={() => router.push(redirectTo)} />;
  }

  // Redirecting, show loading
  return <LoadingSkeleton />;
}

/**
 * Hook to require authentication in a component
 * @returns Object with auth state and redirect function
 */
export function useRequireAuth(redirectTo = '/login') {
  const router = useRouter();
  const isAuthenticated = useIsAuthenticated();
  const isInitialized = useIsAuthInitialized();
  const initializeAuth = useAuthStore((state) => state.initializeAuth);

  useEffect(() => {
    initializeAuth();
  }, [initializeAuth]);

  useEffect(() => {
    if (isInitialized && !isAuthenticated) {
      router.push(redirectTo);
    }
  }, [isInitialized, isAuthenticated, redirectTo, router]);

  return {
    isAuthenticated,
    isReady: isInitialized,
    redirectToLogin: () => router.push(redirectTo),
  };
}
