import type { NextConfig } from "next";

function getRemotePattern(apiUrl: string | undefined):
  | { protocol: 'http' | 'https'; hostname: string; port?: string; pathname: string }
  | null {
  if (!apiUrl) return null;
  try {
    const url = new URL(apiUrl);
    const protocol = url.protocol.replace(':', '');
    if (protocol !== 'http' && protocol !== 'https') return null;
    return {
      protocol,
      hostname: url.hostname,
      port: url.port || undefined,
      pathname: '/**',
    };
  } catch {
    return null;
  }
}

const nextConfig: NextConfig = {
  // Enable standalone output for Docker deployments
  output: process.env.BUILD_STANDALONE === 'true' ? 'standalone' : undefined,

  images: {
    remotePatterns: [
      // Local dev backend (common)
      { protocol: 'http', hostname: 'localhost', port: '5050', pathname: '/**' },
      { protocol: 'http', hostname: '127.0.0.1', port: '5050', pathname: '/**' },
      // Optional: backend host via env
      ...(getRemotePattern(process.env.NEXT_PUBLIC_API_URL)
        ? [getRemotePattern(process.env.NEXT_PUBLIC_API_URL)!]
        : []),
    ],
  },
  
  async headers() {
    return [
      {
        source: '/:path*',
        headers: [
          {
            key: 'X-DNS-Prefetch-Control',
            value: 'on'
          },
          {
            key: 'Strict-Transport-Security',
            value: 'max-age=63072000; includeSubDomains; preload'
          },
          {
            key: 'X-Frame-Options',
            value: 'SAMEORIGIN'
          },
          {
            key: 'X-Content-Type-Options',
            value: 'nosniff'
          },
          {
            key: 'Referrer-Policy',
            value: 'origin-when-cross-origin'
          },
          {
            key: 'Content-Security-Policy',
            value: `default-src 'self'; script-src 'self' 'unsafe-eval' 'unsafe-inline' https://static.cloudflareinsights.com; style-src 'self' 'unsafe-inline'; img-src 'self' blob: data:; font-src 'self' data:; connect-src 'self' ${process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5050'} https://cloudflareinsights.com;`
          }
        ]
      }
    ];
  }
};

export default nextConfig;
