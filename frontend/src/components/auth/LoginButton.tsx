'use client';

import { type OAuthProvider, getOAuthUrl } from '@/lib/api/auth-client';

/**
 * Props for the LoginButton component
 */
interface LoginButtonProps {
  provider: OAuthProvider;
  onClick?: () => void;
  disabled?: boolean;
  className?: string;
}

/**
 * Provider-specific styling and content
 */
const providerConfig: Record<OAuthProvider, {
  label: string;
  bgColor: string;
  hoverBgColor: string;
  textColor: string;
  icon: React.ReactNode;
}> = {
  google: {
    label: 'Continue with Google',
    bgColor: 'bg-white',
    hoverBgColor: 'hover:bg-gray-100',
    textColor: 'text-gray-800',
    icon: (
      <svg className="w-5 h-5" viewBox="0 0 24 24" aria-hidden="true">
        <path
          fill="#4285F4"
          d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z"
        />
        <path
          fill="#34A853"
          d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z"
        />
        <path
          fill="#FBBC05"
          d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l2.85-2.22.81-.62z"
        />
        <path
          fill="#EA4335"
          d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z"
        />
      </svg>
    ),
  },
  microsoft: {
    label: 'Continue with Microsoft',
    bgColor: 'bg-[#2F2F2F]',
    hoverBgColor: 'hover:bg-[#404040]',
    textColor: 'text-white',
    icon: (
      <svg className="w-5 h-5" viewBox="0 0 24 24" aria-hidden="true">
        <path fill="#F25022" d="M1 1h10v10H1z" />
        <path fill="#00A4EF" d="M1 13h10v10H1z" />
        <path fill="#7FBA00" d="M13 1h10v10H13z" />
        <path fill="#FFB900" d="M13 13h10v10H13z" />
      </svg>
    ),
  },
  apple: {
    label: 'Continue with Apple',
    bgColor: 'bg-black',
    hoverBgColor: 'hover:bg-gray-900',
    textColor: 'text-white',
    icon: (
      <svg className="w-5 h-5" viewBox="0 0 24 24" fill="currentColor" aria-hidden="true">
        <path d="M17.05 20.28c-.98.95-2.05.8-3.08.35-1.09-.46-2.09-.48-3.24 0-1.44.62-2.2.44-3.06-.35C2.79 15.25 3.51 7.59 9.05 7.31c1.35.07 2.29.74 3.08.8 1.18-.24 2.31-.93 3.57-.84 1.51.12 2.65.72 3.4 1.8-3.12 1.87-2.38 5.98.48 7.13-.57 1.5-1.31 2.99-2.54 4.09l.01-.01zM12.03 7.25c-.15-2.23 1.66-4.07 3.74-4.25.29 2.58-2.34 4.5-3.74 4.25z" />
      </svg>
    ),
  },
};

/**
 * OAuth login button component
 * Redirects to the OAuth provider when clicked
 */
export function LoginButton({
  provider,
  onClick,
  disabled = false,
  className = '',
}: LoginButtonProps) {
  const config = providerConfig[provider];

  const handleClick = () => {
    if (onClick) {
      onClick();
    }
    // Redirect to OAuth provider
    window.location.href = getOAuthUrl(provider);
  };

  return (
    <button
      onClick={handleClick}
      disabled={disabled}
      className={`
        flex items-center justify-center gap-3
        w-full py-3 px-4
        ${config.bgColor} ${config.hoverBgColor} ${config.textColor}
        font-medium rounded-lg
        transition-all duration-200
        disabled:opacity-50 disabled:cursor-not-allowed
        shadow-md hover:shadow-lg
        ${className}
      `}
      type="button"
      aria-label={config.label}
    >
      {config.icon}
      <span>{config.label}</span>
    </button>
  );
}

/**
 * Props for the GuestButton component
 */
interface GuestButtonProps {
  onClick: () => void;
  disabled?: boolean;
  isLoading?: boolean;
  className?: string;
}

/**
 * Guest login button component
 */
export function GuestButton({
  onClick,
  disabled = false,
  isLoading = false,
  className = '',
}: GuestButtonProps) {
  return (
    <button
      onClick={onClick}
      disabled={disabled || isLoading}
      className={`
        flex items-center justify-center gap-3
        w-full py-3 px-4
        bg-gray-700 hover:bg-gray-600 text-white
        font-medium rounded-lg
        transition-all duration-200
        disabled:opacity-50 disabled:cursor-not-allowed
        border border-gray-600 hover:border-gray-500
        ${className}
      `}
      type="button"
    >
      {isLoading ? (
        <>
          <div className="w-5 h-5 border-2 border-white border-t-transparent rounded-full animate-spin" />
          <span>Creating guest session...</span>
        </>
      ) : (
        <>
          <svg
            className="w-5 h-5"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
            aria-hidden="true"
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth={2}
              d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"
            />
          </svg>
          <span>Play as Guest</span>
        </>
      )}
    </button>
  );
}
