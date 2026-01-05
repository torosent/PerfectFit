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
        text-white
        font-medium rounded-lg
        transition-all duration-200
        disabled:opacity-50 disabled:cursor-not-allowed
        ${className}
      `}
      style={{
        backgroundColor: 'rgba(13, 36, 61, 0.8)',
        borderWidth: 1,
        borderStyle: 'solid',
        borderColor: 'rgba(20, 184, 166, 0.3)'
      }}
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
