'use client';

import { useEffect, useState, Suspense } from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import { useAuthStore } from '@/lib/stores/auth-store';
import { getCurrentUser, AuthError } from '@/lib/api/auth-client';

/**
 * OAuth callback content component
 * Handles the token from URL and sets up auth state
 */
function CallbackContent() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const login = useAuthStore((state) => state.login);
  const setError = useAuthStore((state) => state.setError);
  const [status, setStatus] = useState<'processing' | 'success' | 'error'>('processing');
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  useEffect(() => {
    async function handleCallback() {
      // Get token from URL
      const token = searchParams.get('token');
      const error = searchParams.get('error');
      const errorDescription = searchParams.get('error_description');

      // Handle error from OAuth provider
      if (error) {
        const message = errorDescription || `Authentication failed: ${error}`;
        setStatus('error');
        setErrorMessage(message);
        setError(message);
        return;
      }

      // No token provided
      if (!token) {
        setStatus('error');
        setErrorMessage('No authentication token received');
        setError('No authentication token received');
        return;
      }

      try {
        // Fetch user profile with the token
        const user = await getCurrentUser(token);
        
        // Store auth state
        login(token, user);
        
        setStatus('success');
        
        // Redirect to play page
        setTimeout(() => {
          router.push('/play');
        }, 500);
      } catch (err) {
        const message = err instanceof AuthError 
          ? err.message 
          : 'Failed to complete authentication';
        setStatus('error');
        setErrorMessage(message);
        setError(message);
      }
    }

    handleCallback();
  }, [searchParams, login, setError, router]);

  return (
    <div className="min-h-screen game-background flex items-center justify-center p-4">
      <div className="glass-panel rounded-2xl p-8 max-w-md w-full text-center">
        {status === 'processing' && (
          <>
            <div className="animate-spin rounded-full h-12 w-12 border-4 border-blue-500 border-t-transparent mx-auto mb-4" />
            <h2 className="text-xl font-semibold text-white mb-2">
              Completing sign in...
            </h2>
            <p className="text-gray-400 text-sm">
              Please wait while we set up your account.
            </p>
          </>
        )}

        {status === 'success' && (
          <>
            <div className="w-12 h-12 bg-green-500 rounded-full flex items-center justify-center mx-auto mb-4">
              <svg
                className="w-6 h-6 text-white"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
                aria-hidden="true"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M5 13l4 4L19 7"
                />
              </svg>
            </div>
            <h2 className="text-xl font-semibold text-white mb-2">
              Welcome!
            </h2>
            <p className="text-gray-400 text-sm">
              Redirecting you to the game...
            </p>
          </>
        )}

        {status === 'error' && (
          <>
            <div className="w-12 h-12 bg-red-500 rounded-full flex items-center justify-center mx-auto mb-4">
              <svg
                className="w-6 h-6 text-white"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
                aria-hidden="true"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M6 18L18 6M6 6l12 12"
                />
              </svg>
            </div>
            <h2 className="text-xl font-semibold text-white mb-2">
              Authentication Failed
            </h2>
            <p className="text-gray-400 text-sm mb-6">
              {errorMessage || 'An error occurred during sign in.'}
            </p>
            <button
              onClick={() => router.push('/login')}
              className={`
                py-2 px-6 bg-blue-600 hover:bg-blue-500
                text-white font-medium rounded-lg
                transition-colors
              `}
            >
              Try Again
            </button>
          </>
        )}
      </div>
    </div>
  );
}

/**
 * Loading fallback for Suspense
 */
function CallbackLoading() {
  return (
    <div className="min-h-screen game-background flex items-center justify-center p-4">
      <div className="glass-panel rounded-2xl p-8 max-w-md w-full text-center">
        <div className="animate-spin rounded-full h-12 w-12 border-4 border-blue-500 border-t-transparent mx-auto mb-4" />
        <h2 className="text-xl font-semibold text-white mb-2">
          Loading...
        </h2>
      </div>
    </div>
  );
}

/**
 * OAuth callback page
 * Wrapped in Suspense for useSearchParams
 */
export default function CallbackPage() {
  return (
    <Suspense fallback={<CallbackLoading />}>
      <CallbackContent />
    </Suspense>
  );
}
