import type { Metadata, Viewport } from "next";
import { Geist, Geist_Mono } from "next/font/google";
import "./globals.css";
import { ServiceWorkerRegistration } from "@/components/ServiceWorkerRegistration";

const geistSans = Geist({
  variable: "--font-geist-sans",
  subsets: ["latin"],
});

const geistMono = Geist_Mono({
  variable: "--font-geist-mono",
  subsets: ["latin"],
});

export const viewport: Viewport = {
  width: 'device-width',
  initialScale: 1,
  maximumScale: 1,
  userScalable: false,
  viewportFit: 'cover',
  themeColor: '#0d9488',
};

export const metadata: Metadata = {
  title: "PerfectFit - Relaxing Block Puzzle Game",
  description: "A relaxing yet strategic 10x10 block puzzle game. Place colorful tetromino shapes, clear lines, and chase high scores. No timers, no pressure—just pure puzzle satisfaction. Play free in your browser!",
  keywords: ["puzzle game", "block puzzle", "tetris", "tetromino", "brain game", "relaxing game", "strategy game", "free game", "browser game"],
  authors: [{ name: "PerfectFit" }],
  manifest: "/manifest.json",
  appleWebApp: {
    capable: true,
    statusBarStyle: "black-translucent",
    title: "PerfectFit",
  },
  icons: {
    icon: [
      { url: "/icons/icon-192x192.png", sizes: "192x192", type: "image/png" },
      { url: "/icons/icon-512x512.png", sizes: "512x512", type: "image/png" },
    ],
    apple: [
      { url: "/icons/icon-192x192.png", sizes: "192x192", type: "image/png" },
    ],
  },
  openGraph: {
    title: "PerfectFit - Every Block Has a Perfect Fit",
    description: "A relaxing yet strategic block puzzle game. Place colorful shapes, clear lines, and chase high scores.",
    type: "website",
    locale: "en_US",
  },
  twitter: {
    card: "summary_large_image",
    title: "PerfectFit - Relaxing Block Puzzle Game",
    description: "Place colorful tetromino shapes, clear lines, and chase high scores. No timers, no pressure!",
  },
  robots: {
    index: true,
    follow: true,
  },
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en">
      <body
        className={`${geistSans.variable} ${geistMono.variable} antialiased`}
      >
        <ServiceWorkerRegistration />
        {children}
      </body>
    </html>
  );
}
