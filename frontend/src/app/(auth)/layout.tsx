import type { Metadata } from 'next';

export const metadata: Metadata = {
  title: 'Sign In - PerfectFit',
  description: 'Sign in to PerfectFit to save your scores and compete on the leaderboard',
};

/**
 * Auth pages layout
 * Shared layout for login and callback pages
 */
export default function AuthLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return <>{children}</>;
}
