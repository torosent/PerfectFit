'use client';

import { useState } from 'react';
import Link from 'next/link';
import PasswordStrengthIndicator, {
  isPasswordValid,
} from '@/components/auth/PasswordStrengthIndicator';
import { EmojiPicker } from '@/components/profile/EmojiPicker';
import {
  useAuthStore,
  useIsAuthLoading,
  useAuthError,
} from '@/lib/stores/auth-store';

interface ValidationErrors {
  email?: string;
  displayName?: string;
  password?: string;
  confirmPassword?: string;
}

/**
 * Registration page for password-based authentication
 * Allows users to create an account with email and password
 */
export default function RegisterPage() {
  const isLoading = useIsAuthLoading();
  const storeError = useAuthError();
  const localRegister = useAuthStore((state) => state.localRegister);
  const clearError = useAuthStore((state) => state.clearError);

  // Form state
  const [email, setEmail] = useState('');
  const [displayName, setDisplayName] = useState('');
  const [avatar, setAvatar] = useState<string | null>(null);
  const [showEmojiPicker, setShowEmojiPicker] = useState(false);
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [validationErrors, setValidationErrors] = useState<ValidationErrors>({});
  const [apiError, setApiError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  const validateForm = (): boolean => {
    const errors: ValidationErrors = {};

    if (!email.trim()) {
      errors.email = 'Email is required';
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
      errors.email = 'Please enter a valid email';
    }

    if (!displayName.trim()) {
      errors.displayName = 'Display name is required';
    }

    if (!password) {
      errors.password = 'Password is required';
    } else if (!isPasswordValid(password)) {
      errors.password = 'Password does not meet requirements';
    }

    if (password && confirmPassword && password !== confirmPassword) {
      errors.confirmPassword = 'Passwords do not match';
    }

    setValidationErrors(errors);
    return Object.keys(errors).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    clearError();
    setApiError(null);
    setSuccessMessage(null);

    if (!validateForm()) {
      return;
    }

    const response = await localRegister(email, password, displayName, avatar || undefined);

    if (response.success) {
      setSuccessMessage(
        response.message ||
          'Registration successful! Please check your email to verify your account.'
      );
    } else {
      setApiError(response.error || 'Registration failed');
    }
  };

  const displayError = apiError || storeError;

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

      {/* Registration card */}
      <div className="relative z-10 w-full max-w-md">
        <div className="glass-panel rounded-2xl p-8 shadow-2xl">
          {/* Logo/Title */}
          <div className="text-center mb-8">
            <h1 className="text-4xl font-bold text-white mb-2">
              Perfect<span style={{ color: '#2dd4bf' }}>Fit</span>
            </h1>
            <p className="text-gray-400">Create your account</p>
          </div>

          {/* Success message */}
          {successMessage && (
            <div
              className="mb-6 p-4 bg-green-900/50 border border-green-500 rounded-lg text-green-200 text-sm"
              role="alert"
            >
              <div className="flex items-start gap-2">
                <svg
                  className="w-5 h-5 flex-shrink-0 mt-0.5"
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
                <div>
                  <p className="font-medium">Check your email!</p>
                  <p className="mt-1">{successMessage}</p>
                  <Link
                    href="/login"
                    className="mt-2 inline-block text-teal-400 hover:text-teal-300 underline"
                  >
                    Go to Sign In
                  </Link>
                </div>
              </div>
            </div>
          )}

          {/* Error display */}
          {displayError && !successMessage && (
            <div
              className="mb-6 p-4 bg-red-900/50 border border-red-500 rounded-lg text-red-200 text-sm"
              role="alert"
            >
              <div className="flex items-start gap-2">
                <svg
                  className="w-5 h-5 flex-shrink-0 mt-0.5"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                  aria-hidden="true"
                >
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth={2}
                    d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
                  />
                </svg>
                <div>
                  <p>{displayError}</p>
                  <button
                    onClick={() => {
                      clearError();
                      setApiError(null);
                    }}
                    className="mt-1 text-xs underline hover:no-underline"
                  >
                    Dismiss
                  </button>
                </div>
              </div>
            </div>
          )}

          {/* Registration form */}
          {!successMessage && (
            <form onSubmit={handleSubmit} className="space-y-4">
              <div>
                <label
                  htmlFor="email"
                  className="block text-sm font-medium text-gray-300 mb-1"
                >
                  Email
                </label>
                <input
                  id="email"
                  type="email"
                  value={email}
                  onChange={(e) => {
                    setEmail(e.target.value);
                    if (validationErrors.email) {
                      setValidationErrors((prev) => ({ ...prev, email: undefined }));
                    }
                  }}
                  disabled={isLoading}
                  className="w-full px-4 py-3 rounded-lg bg-gray-800/50 border border-gray-600 text-white placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-teal-500 focus:border-transparent disabled:opacity-50 disabled:cursor-not-allowed"
                  placeholder="you@example.com"
                  autoComplete="email"
                />
                {validationErrors.email && (
                  <p className="mt-1 text-sm text-red-400">{validationErrors.email}</p>
                )}
              </div>

              <div>
                <label
                  htmlFor="displayName"
                  className="block text-sm font-medium text-gray-300 mb-1"
                >
                  Display Name
                </label>
                <input
                  id="displayName"
                  type="text"
                  value={displayName}
                  onChange={(e) => {
                    setDisplayName(e.target.value);
                    if (validationErrors.displayName) {
                      setValidationErrors((prev) => ({ ...prev, displayName: undefined }));
                    }
                  }}
                  disabled={isLoading}
                  className="w-full px-4 py-3 rounded-lg bg-gray-800/50 border border-gray-600 text-white placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-teal-500 focus:border-transparent disabled:opacity-50 disabled:cursor-not-allowed"
                  placeholder="How you'll appear on leaderboards"
                  autoComplete="name"
                />
                {validationErrors.displayName && (
                  <p className="mt-1 text-sm text-red-400">
                    {validationErrors.displayName}
                  </p>
                )}
              </div>

              {/* Avatar picker */}
              <div>
                <label className="block text-sm font-medium text-gray-300 mb-2">
                  Avatar (optional)
                </label>
                <div className="flex items-center gap-3">
                  <button
                    type="button"
                    onClick={() => setShowEmojiPicker(true)}
                    className="w-14 h-14 rounded-full flex items-center justify-center text-2xl border-2 border-gray-600 hover:border-teal-500 transition-colors"
                    style={{ backgroundColor: 'rgba(10, 37, 64, 0.7)' }}
                  >
                    {avatar || '😀'}
                  </button>
                  <button
                    type="button"
                    onClick={() => setShowEmojiPicker(true)}
                    className="text-sm text-teal-400 hover:text-teal-300 underline"
                  >
                    {avatar ? 'Change avatar' : 'Choose an avatar'}
                  </button>
                  {avatar && (
                    <button
                      type="button"
                      onClick={() => setAvatar(null)}
                      className="text-sm text-gray-400 hover:text-gray-300"
                    >
                      Remove
                    </button>
                  )}
                </div>
              </div>

              {/* Avatar picker popup */}
              {showEmojiPicker && (
                <div 
                  className="fixed inset-0 z-50 flex items-center justify-center p-4"
                  style={{ backgroundColor: 'rgba(0, 0, 0, 0.7)' }}
                  onClick={() => setShowEmojiPicker(false)}
                >
                  <div 
                    className="glass-panel rounded-2xl p-6 max-w-sm w-full max-h-[80vh] overflow-hidden"
                    onClick={(e) => e.stopPropagation()}
                  >
                    <div className="flex items-center justify-between mb-4">
                      <h3 className="text-lg font-semibold text-white">Choose Avatar</h3>
                      <button
                        type="button"
                        onClick={() => setShowEmojiPicker(false)}
                        className="text-gray-400 hover:text-white p-1"
                      >
                        <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                        </svg>
                      </button>
                    </div>
                    <EmojiPicker 
                      selected={avatar} 
                      onSelect={(emoji) => {
                        setAvatar(emoji);
                        setShowEmojiPicker(false);
                      }}
                    />
                  </div>
                </div>
              )}

              <div>
                <label
                  htmlFor="password"
                  className="block text-sm font-medium text-gray-300 mb-1"
                >
                  Password
                </label>
                <input
                  id="password"
                  type="password"
                  value={password}
                  onChange={(e) => {
                    setPassword(e.target.value);
                    if (validationErrors.password) {
                      setValidationErrors((prev) => ({ ...prev, password: undefined }));
                    }
                  }}
                  disabled={isLoading}
                  className="w-full px-4 py-3 rounded-lg bg-gray-800/50 border border-gray-600 text-white placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-teal-500 focus:border-transparent disabled:opacity-50 disabled:cursor-not-allowed"
                  placeholder="••••••••"
                  autoComplete="new-password"
                />
                {validationErrors.password && (
                  <p className="mt-1 text-sm text-red-400">{validationErrors.password}</p>
                )}
                <PasswordStrengthIndicator password={password} />
              </div>

              <div>
                <label
                  htmlFor="confirmPassword"
                  className="block text-sm font-medium text-gray-300 mb-1"
                >
                  Confirm Password
                </label>
                <input
                  id="confirmPassword"
                  type="password"
                  value={confirmPassword}
                  onChange={(e) => {
                    setConfirmPassword(e.target.value);
                    if (validationErrors.confirmPassword) {
                      setValidationErrors((prev) => ({
                        ...prev,
                        confirmPassword: undefined,
                      }));
                    }
                  }}
                  disabled={isLoading}
                  className="w-full px-4 py-3 rounded-lg bg-gray-800/50 border border-gray-600 text-white placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-teal-500 focus:border-transparent disabled:opacity-50 disabled:cursor-not-allowed"
                  placeholder="••••••••"
                  autoComplete="new-password"
                />
                {validationErrors.confirmPassword && (
                  <p className="mt-1 text-sm text-red-400">
                    {validationErrors.confirmPassword}
                  </p>
                )}
              </div>

              <button
                type="submit"
                disabled={isLoading}
                className="w-full py-3 px-4 bg-teal-600 hover:bg-teal-700 text-white font-medium rounded-lg transition-all duration-200 disabled:opacity-50 disabled:cursor-not-allowed shadow-md hover:shadow-lg"
              >
                {isLoading ? (
                  <span className="flex items-center justify-center gap-2">
                    <div className="w-5 h-5 border-2 border-white border-t-transparent rounded-full animate-spin" />
                    Creating account...
                  </span>
                ) : (
                  'Create Account'
                )}
              </button>
            </form>
          )}

          {/* Login link */}
          <p className="mt-6 text-center text-sm text-gray-400">
            Already have an account?{' '}
            <Link href="/login" className="text-teal-400 hover:text-teal-300 underline">
              Sign in
            </Link>
          </p>

          {/* Footer text */}
          <p className="mt-4 text-center text-xs text-gray-500">
            By creating an account, you agree to our terms of service and privacy policy.
          </p>
        </div>
      </div>
    </div>
  );
}
