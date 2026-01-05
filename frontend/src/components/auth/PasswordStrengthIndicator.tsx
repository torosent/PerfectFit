'use client';

/**
 * Password Strength Indicator Component
 *
 * Visual indicator showing password requirements and strength.
 * Requirements (must match backend):
 * - Minimum 8 characters
 * - At least one uppercase letter
 * - At least one lowercase letter
 * - At least one number
 */

interface PasswordStrengthIndicatorProps {
  password: string;
}

interface Requirement {
  id: string;
  label: string;
  check: (password: string) => boolean;
}

const requirements: Requirement[] = [
  {
    id: 'length',
    label: '8+ characters',
    check: (password: string) => password.length >= 8,
  },
  {
    id: 'uppercase',
    label: 'Uppercase letter',
    check: (password: string) => /[A-Z]/.test(password),
  },
  {
    id: 'lowercase',
    label: 'Lowercase letter',
    check: (password: string) => /[a-z]/.test(password),
  },
  {
    id: 'number',
    label: 'Number',
    check: (password: string) => /[0-9]/.test(password),
  },
];

/**
 * Calculate password strength based on requirements met
 */
function getStrength(password: string): 'weak' | 'medium' | 'strong' {
  const metCount = requirements.filter((req) => req.check(password)).length;
  if (metCount === requirements.length) {
    return 'strong';
  }
  if (metCount >= 2) {
    return 'medium';
  }
  return 'weak';
}

/**
 * Get color classes based on strength level
 */
function getStrengthColor(strength: 'weak' | 'medium' | 'strong'): string {
  switch (strength) {
    case 'strong':
      return 'text-green-400';
    case 'medium':
      return 'text-yellow-400';
    case 'weak':
    default:
      return 'text-red-400';
  }
}

export default function PasswordStrengthIndicator({ password }: PasswordStrengthIndicatorProps) {
  const strength = getStrength(password);

  return (
    <div
      data-testid="password-strength-indicator"
      data-strength={strength}
      className="mt-2 space-y-2"
    >
      <p className="text-xs text-gray-400">Password requirements:</p>
      <ul className="space-y-1">
        {requirements.map((req) => {
          const isMet = req.check(password);
          return (
            <li
              key={req.id}
              data-testid={`requirement-${req.id}`}
              data-met={isMet.toString()}
              className={`flex items-center gap-2 text-xs ${
                isMet ? 'text-green-400' : 'text-gray-500'
              }`}
            >
              {isMet ? (
                <svg
                  data-testid="requirement-met"
                  className="w-4 h-4 flex-shrink-0"
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
              ) : (
                <svg
                  className="w-4 h-4 flex-shrink-0"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                  aria-hidden="true"
                >
                  <circle cx="12" cy="12" r="9" strokeWidth={2} />
                </svg>
              )}
              <span>{req.label}</span>
            </li>
          );
        })}
      </ul>
      {password && (
        <div className="flex items-center gap-2">
          <div className="flex-1 h-1 bg-gray-700 rounded-full overflow-hidden">
            <div
              className={`h-full transition-all duration-300 ${
                strength === 'strong'
                  ? 'w-full bg-green-500'
                  : strength === 'medium'
                  ? 'w-2/3 bg-yellow-500'
                  : 'w-1/3 bg-red-500'
              }`}
            />
          </div>
          <span className={`text-xs font-medium ${getStrengthColor(strength)}`}>
            {strength.charAt(0).toUpperCase() + strength.slice(1)}
          </span>
        </div>
      )}
    </div>
  );
}

/**
 * Validate if password meets all requirements
 */
export function isPasswordValid(password: string): boolean {
  return requirements.every((req) => req.check(password));
}
