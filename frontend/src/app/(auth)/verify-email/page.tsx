'use client';

import { useEffect, useState, useRef, useMemo, Suspense } from 'react';
import { useSearchParams } from 'next/navigation';
import Link from 'next/link';
import { verifyEmail } from '@/lib/api/auth-client';

type VerificationState = 'loading' | 'success' | 'error';

/**
 * Email verification content component
 * Reads email and token from URL params and verifies the email
 */
function VerifyEmailContent() {
  const searchParams = useSearchParams();
  const [verificationResult, setVerificationResult] = useState<{
    state: VerificationState;
    error: string | null;
  } | null>(null);
  const hasVerified = useRef(false);

  const email = searchParams.get('email');
  const token = searchParams.get('token');

  // Compute initial validation state synchronously (not in effect)
  const paramsError = useMemo(() => {
    if (!email || !token) {
      return 'Invalid verification link. Email and token are required.';
    }
    return null;
  }, [email, token]);

  useEffect(() => {
    // Skip if params are invalid (handled by paramsError)
    if (paramsError) return;
    
    // Prevent double execution in StrictMode
    if (hasVerified.current) return;
    hasVerified.current = true;

    // Perform verification
    verifyEmail(email!, token!)
      .then((response) => {
        if (response.success) {
          setVerificationResult({ state: 'success', error: null });
        } else {
          setVerificationResult({
            state: 'error',
            error: response.error || 'Email verification failed',
          });
        }
      })
      .catch((err) => {
        setVerificationResult({
          state: 'error',
          error: err instanceof Error ? err.message : 'Email verification failed',
        });
      });
  }, [email, token, paramsError]);

  // Derive current state from params validation and verification result
  const state: VerificationState = paramsError
    ? 'error'
    : verificationResult?.state ?? 'loading';
  const error = paramsError ?? verificationResult?.error ?? null;

  return (
    <div className="min-h-screen game-background flex items-center justify-center p-4">
      {/* Animated background elements */}
      <div className="fixed inset-0 overflow-hidden pointer-events-none" aria-hidden="true">
        <div
          className="absolute top-1/4 left-1/4 w-64 h-64 rounded-full blur-3xl animate-pulse"
          style={{ backgroundColor: 'rgba(20, 184, 166, 0.1)' }}
        />
        <div
          className="absolute bottom-1/4 right-1/4 w-96 h-96 rounded-full blur-3xl animate-pulse"
          style={{ backgroundColor: 'rgba(14, 165, 233, 0.1)', animationDelay: '1s' }}
        />
        <div
          className="absolute top-1/2 left-1/2 w-48 h-48 rounded-full blur-3xl animate-pulse"
          style={{ backgroundColor: 'rgba(34, 211, 238, 0.05)', animationDelay: '2s' }}
        />
      </div>

      {/* Verification card */}
      <div className="relative z-10 w-full max-w-md">
        <div className="glass-panel rounded-2xl p-8 shadow-2xl">
          {/* Logo/Title */}
          <div className="text-center mb-8">
            <h1 className="text-4xl font-bold text-white mb-2">
              Perfect<span style={{ color: '#2dd4bf' }}>Fit</span>
            </h1>
            <p className="text-gray-400">Email Verification</p>
          </div>

          {/* Loading state */}
          {state === 'loading' && (
            <div className="text-center py-8">
              <div className="inline-flex items-center justify-center w-16 h-16 rounded-full bg-teal-900/30 mb-4">
                <div className="w-8 h-8 border-3 border-teal-400 border-t-transparent rounded-full animate-spin" />
              </div>
              <p className="text-gray-300 text-lg">Verifying your email...</p>
              <p className="text-gray-500 text-sm mt-2">Please wait a moment</p>
            </div>
          )}

          {/* Success state */}
          {state === 'success' && (
            <div className="text-center py-8">
              <div className="inline-flex items-center justify-center w-16 h-16 rounded-full bg-green-900/30 mb-4">
                <svg
                  className="w-8 h-8 text-green-400"
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
              <p className="text-green-400 text-lg font-medium">Email Verified!</p>
              <p className="text-gray-400 text-sm mt-2">
                Your email has been successfully verified. You can now sign in to your
                account.
              </p>
              <Link
                href="/login"
                className="mt-6 inline-block w-full py-3 px-4 bg-teal-600 hover:bg-teal-700 text-white font-medium rounded-lg transition-all duration-200 text-center shadow-md hover:shadow-lg"
              >
                Sign In
              </Link>
            </div>
          )}

          {/* Error state */}
          {state === 'error' && (
            <div className="text-center py-8">
              <div className="inline-flex items-center justify-center w-16 h-16 rounded-full bg-red-900/30 mb-4">
                <svg
                  className="w-8 h-8 text-red-400"
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
              <p className="text-red-400 text-lg font-medium">Verification Failed</p>
              <p className="text-gray-400 text-sm mt-2">{error}</p>
              <div className="mt-6 space-y-3">
                <Link
                  href="/login"
                  className="block w-full py-3 px-4 bg-gray-700 hover:bg-gray-600 text-white font-medium rounded-lg transition-all duration-200 text-center"
                >
                  Go to Sign In
                </Link>
                <Link
                  href="/register"
                  className="block w-full py-3 px-4 bg-transparent border border-gray-600 hover:border-gray-500 text-gray-300 hover:text-white font-medium rounded-lg transition-all duration-200 text-center"
                >
                  Create New Account
                </Link>
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

/**
 * Loading fallback for Suspense
 */
function VerifyEmailLoading() {
  return (
    <div className="min-h-screen game-background flex items-center justify-center p-4">
      <div className="glass-panel rounded-2xl p-8 max-w-md w-full text-center">
        <div className="animate-spin rounded-full h-12 w-12 border-4 mx-auto mb-4" style={{ borderColor: '#14b8a6', borderTopColor: 'transparent' }} />
        <h2 className="text-xl font-semibold text-white mb-2">
          Loading...
        </h2>
      </div>
    </div>
  );
}

/**
 * Email verification page
 * Wrapped in Suspense for useSearchParams
 */
export default function VerifyEmailPage() {
  return (
    <Suspense fallback={<VerifyEmailLoading />}>
      <VerifyEmailContent />
    </Suspense>
  );
}
