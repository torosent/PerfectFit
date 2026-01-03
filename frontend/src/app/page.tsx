import Link from "next/link";

// ============================================================
// CONFIGURATION: Change this URL to your actual game URL
// ============================================================
const PLAY_NOW_URL = "/play";
// ============================================================

// Theme colors - Ocean Blue + Teal + Gold
const theme = {
  // Backgrounds
  bgMain: 'linear-gradient(135deg, #0c1929 0%, #0a2540 50%, #0c1929 100%)',
  bgCard: 'rgba(13, 36, 61, 0.85)',
  bgCardHover: 'rgba(17, 45, 78, 0.9)',
  bgSection: 'rgba(6, 25, 45, 0.6)',
  
  // Primary gradient (buttons, accents)
  primaryGradient: 'linear-gradient(135deg, #14b8a6 0%, #0ea5e9 50%, #06b6d4 100%)',
  primaryGradientHover: 'linear-gradient(135deg, #0d9488 0%, #0284c7 50%, #0891b2 100%)',
  primaryShadow: '0 10px 40px rgba(20, 184, 166, 0.35)',
  
  // Accent gradients for text
  accentGold: 'linear-gradient(to right, #fbbf24, #f59e0b)',
  accentTeal: 'linear-gradient(to right, #2dd4bf, #22d3ee)',
  accentOcean: 'linear-gradient(to right, #06b6d4, #0ea5e9, #38bdf8)',
  accentWarm: 'linear-gradient(to right, #fb923c, #f97316)',
  accentCoral: 'linear-gradient(to right, #f472b6, #fb7185)',
  
  // Glow colors
  glowTeal: 'rgba(20, 184, 166, 0.4)',
  glowBlue: 'rgba(14, 165, 233, 0.4)',
  glowCyan: 'rgba(34, 211, 238, 0.35)',
  
  // Text colors
  textPrimary: '#ffffff',
  textSecondary: '#94a3b8',
  textMuted: '#64748b',
  textBody: '#cbd5e1',
  
  // Borders
  borderLight: 'rgba(255,255,255,0.12)',
  borderMedium: 'rgba(255,255,255,0.18)',
};

// Tetromino piece component for visual decoration
function TetrominoPiece({ 
  shape, 
  color, 
  className = "" 
}: { 
  shape: number[][]; 
  color: string; 
  className?: string;
}) {
  return (
    <div className={`grid gap-0.5 ${className}`} style={{ 
      gridTemplateColumns: `repeat(${shape[0].length}, 1fr)` 
    }}>
      {shape.flat().map((cell, i) => (
        <div
          key={i}
          className="w-4 h-4 md:w-5 md:h-5 rounded-sm transition-all duration-300"
          style={cell ? { background: color, boxShadow: '0 4px 6px rgba(0,0,0,0.3)' } : undefined}
        />
      ))}
    </div>
  );
}

// Floating animated pieces for background
function FloatingPieces() {
  const pieces = [
    { shape: [[1, 1], [1, 1]], color: "#fbbf24", delay: "0s", x: "10%", y: "20%" },
    { shape: [[1, 1, 1, 1]], color: "#2dd4bf", delay: "2s", x: "80%", y: "15%" },
    { shape: [[1, 0], [1, 0], [1, 1]], color: "#fb923c", delay: "4s", x: "15%", y: "70%" },
    { shape: [[0, 1], [1, 1], [1, 0]], color: "#34d399", delay: "1s", x: "85%", y: "60%" },
    { shape: [[1, 1, 1], [0, 1, 0]], color: "#38bdf8", delay: "3s", x: "5%", y: "45%" },
    { shape: [[1, 1, 0], [0, 1, 1]], color: "#f97316", delay: "5s", x: "90%", y: "35%" },
    { shape: [[0, 1, 1], [1, 1, 0]], color: "#22d3ee", delay: "2.5s", x: "75%", y: "80%" },
  ];

  return (
    <div className="absolute inset-0 overflow-hidden pointer-events-none">
      {pieces.map((piece, i) => (
        <div
          key={i}
          className="absolute animate-float opacity-20"
          style={{
            left: piece.x,
            top: piece.y,
            animationDelay: piece.delay,
          }}
        >
          <TetrominoPiece shape={piece.shape} color={piece.color} />
        </div>
      ))}
    </div>
  );
}

// Game board preview component
function GameBoardPreview() {
  // Create a sample game state for the preview with hex colors
  const board = Array(10).fill(null).map((_, row) => 
    Array(10).fill(null).map((_, col) => {
      // Create an interesting pattern
      if (row === 9 && col < 7) return "#2dd4bf"; // teal
      if (row === 8 && col >= 2 && col <= 5) return "#fbbf24"; // gold
      if (row === 7 && col >= 0 && col <= 2) return "#38bdf8"; // sky blue
      if (row === 6 && col >= 7 && col <= 9) return "#34d399"; // emerald
      if (row === 5 && col >= 4 && col <= 6) return "#fb923c"; // orange
      if (col === 9 && row >= 5) return "#22d3ee"; // cyan
      return null;
    })
  );

  return (
    <div className="relative">
      {/* Glow effect behind board */}
      <div className="absolute inset-0 blur-3xl scale-110" style={{ background: `linear-gradient(to right, ${theme.glowTeal}, ${theme.glowBlue}, ${theme.glowCyan})` }} />
      
      {/* The game board */}
      <div className="relative p-3 rounded-2xl shadow-2xl" style={{ background: theme.bgCard, border: `1px solid ${theme.borderMedium}` }}>
        <div className="grid grid-cols-10 gap-0.5">
          {board.flat().map((cell, i) => (
            <div
              key={i}
              className="w-5 h-5 md:w-6 md:h-6 rounded-sm transition-all duration-300"
              style={cell 
                ? { background: cell, boxShadow: '0 2px 4px rgba(0,0,0,0.3)' }
                : { background: 'rgba(30, 58, 95, 0.6)', border: '1px solid rgba(56, 97, 140, 0.4)' }
              }
            />
          ))}
        </div>
      </div>
    </div>
  );
}

// Feature card component
function FeatureCard({ 
  icon, 
  title, 
  description 
}: { 
  icon: string; 
  title: string; 
  description: string;
}) {
  return (
    <div 
      className="group rounded-2xl p-6 transition-all duration-300 hover:-translate-y-1 hover:shadow-lg"
      style={{ 
        background: theme.bgCard, 
        border: `1px solid ${theme.borderLight}`,
      }}
    >
      <div className="text-4xl mb-4">{icon}</div>
      <h3 className="text-xl font-bold mb-2" style={{ color: theme.textPrimary }}>{title}</h3>
      <p style={{ color: theme.textBody }}>{description}</p>
    </div>
  );
}

// Step component for how to play
function HowToPlayStep({ 
  number, 
  title, 
  description, 
  icon 
}: { 
  number: number; 
  title: string; 
  description: string;
  icon: string;
}) {
  return (
    <div className="flex flex-col items-center text-center group">
      <div className="relative mb-4">
        <div 
          className="w-20 h-20 rounded-2xl flex items-center justify-center text-4xl shadow-lg group-hover:scale-110 transition-transform duration-300"
          style={{ background: theme.primaryGradient }}
        >
          {icon}
        </div>
        <div 
          className="absolute -top-2 -right-2 w-8 h-8 rounded-full font-bold flex items-center justify-center text-sm shadow-md"
          style={{ background: '#fbbf24', color: '#0c1929' }}
        >
          {number}
        </div>
      </div>
      <h3 className="text-lg font-bold mb-2" style={{ color: theme.textPrimary }}>{title}</h3>
      <p className="text-sm max-w-xs" style={{ color: theme.textBody }}>{description}</p>
    </div>
  );
}

// Testimonial component
function Testimonial({ 
  quote, 
  author, 
  role 
}: { 
  quote: string; 
  author: string; 
  role: string;
}) {
  return (
    <div 
      className="rounded-2xl p-6 transition-all duration-300 hover:shadow-lg"
      style={{ background: theme.bgCard, border: `1px solid ${theme.borderLight}` }}
    >
      <div className="text-2xl mb-4" style={{ color: '#fbbf24' }}>&ldquo;</div>
      <p className="italic mb-4" style={{ color: '#e2e8f0' }}>{quote}</p>
      <div className="flex items-center gap-3">
        <div className="w-10 h-10 rounded-full" style={{ background: theme.primaryGradient }} />
        <div>
          <div className="font-semibold" style={{ color: theme.textPrimary }}>{author}</div>
          <div className="text-sm" style={{ color: theme.textSecondary }}>{role}</div>
        </div>
      </div>
    </div>
  );
}

// Play Now button component
function PlayNowButton({ size = "default" }: { size?: "default" | "large" }) {
  const sizeClasses = size === "large" 
    ? "px-12 py-5 text-xl" 
    : "px-8 py-4 text-lg";
  
  return (
    <Link 
      href={PLAY_NOW_URL}
      className={`group relative inline-flex items-center justify-center ${sizeClasses} font-bold rounded-full shadow-lg hover:shadow-xl transition-all duration-300 hover:scale-105 overflow-hidden`}
      style={{ 
        color: theme.textPrimary,
        background: theme.primaryGradient,
        boxShadow: theme.primaryShadow
      }}
    >
      <span className="relative z-10 flex items-center gap-2">
        Play Now
        <svg className="w-5 h-5 group-hover:translate-x-1 transition-transform" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 7l5 5m0 0l-5 5m5-5H6" />
        </svg>
      </span>
      <div className="absolute inset-0 opacity-0 group-hover:opacity-100 transition-opacity duration-300" style={{ background: theme.primaryGradientHover }} />
    </Link>
  );
}

export default function LandingPage() {
  return (
    <div className="min-h-screen text-white overflow-x-hidden" style={{ background: theme.bgMain }}>
      {/* Animated background */}
      <div className="fixed inset-0 overflow-hidden pointer-events-none" style={{ zIndex: 0 }}>
        <div className="absolute top-0 left-1/4 w-96 h-96 rounded-full blur-3xl animate-pulse-slow" style={{ background: theme.glowTeal }} />
        <div className="absolute bottom-1/4 right-1/4 w-80 h-80 rounded-full blur-3xl animate-pulse-slow" style={{ background: theme.glowBlue, animationDelay: "1s" }} />
        <div className="absolute top-1/2 left-1/2 w-64 h-64 rounded-full blur-3xl animate-pulse-slow" style={{ background: theme.glowCyan, animationDelay: "2s" }} />
      </div>
      
      <FloatingPieces />
      
      {/* Navigation */}
      <nav className="relative z-20 flex items-center justify-between px-6 md:px-12 py-6">
        <div className="flex items-center gap-2">
          <div className="w-10 h-10 rounded-xl flex items-center justify-center" style={{ background: theme.primaryGradient }}>
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
                d="M4 6a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2H6a2 2 0 01-2-2V6zM14 6a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2h-2a2 2 0 01-2-2V6zM4 16a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2H6a2 2 0 01-2-2v-2zM14 16a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2h-2a2 2 0 01-2-2v-2z"
              />
            </svg>
          </div>
          <span className="text-xl font-bold text-white">
            Perfect<span style={{ color: '#2dd4bf' }}>Fit</span>
          </span>
        </div>
        <PlayNowButton />
      </nav>

      {/* Hero Section */}
      <section className="relative z-10 flex flex-col lg:flex-row items-center justify-between gap-12 px-6 md:px-12 lg:px-24 py-12 lg:py-20">
        <div className="flex-1 text-center lg:text-left max-w-2xl">
          <div 
            className="inline-flex items-center gap-2 px-4 py-2 rounded-full text-sm font-medium mb-6"
            style={{ background: 'rgba(20, 184, 166, 0.15)', border: '1px solid rgba(20, 184, 166, 0.3)', color: theme.textPrimary }}
          >
            <span className="w-2 h-2 rounded-full animate-pulse" style={{ background: '#2dd4bf' }} />
            Free to Play • No Download Required
          </div>
          
          <h1 className="text-4xl sm:text-5xl md:text-6xl lg:text-7xl font-black leading-tight mb-6" style={{ color: theme.textPrimary }}>
            Every Block Has a{" "}
            <span style={{ background: theme.accentOcean, WebkitBackgroundClip: 'text', WebkitTextFillColor: 'transparent', backgroundClip: 'text' }}>
              Perfect Fit
            </span>
          </h1>
          
          <p className="text-lg md:text-xl mb-8 max-w-lg mx-auto lg:mx-0" style={{ color: theme.textBody }}>
            A relaxing yet strategic block puzzle game. Place colorful tetromino shapes, clear lines, 
            and chase high scores in this beautifully crafted brain teaser.
          </p>
          
          <div className="flex flex-col sm:flex-row items-center gap-4 justify-center lg:justify-start">
            <PlayNowButton size="large" />
            <div className="flex items-center gap-2" style={{ color: theme.textSecondary }}>
              <div className="flex -space-x-2">
                {[...Array(4)].map((_, i) => (
                  <div key={i} className="w-8 h-8 rounded-full" style={{ background: theme.primaryGradient, border: '2px solid #0c1929' }} />
                ))}
              </div>
              <span className="text-sm">1,000+ players online</span>
            </div>
          </div>
        </div>
        
        <div className="flex-1 flex justify-center lg:justify-end">
          <GameBoardPreview />
        </div>
      </section>

      {/* Features Section */}
      <section className="relative z-10 px-6 md:px-12 lg:px-24 py-20">
        <div className="text-center mb-16">
          <h2 className="text-3xl md:text-4xl font-black mb-4" style={{ color: theme.textPrimary }}>
            Simple Rules,{" "}
            <span style={{ background: theme.accentGold, WebkitBackgroundClip: 'text', WebkitTextFillColor: 'transparent', backgroundClip: 'text' }}>
              Endless Fun
            </span>
          </h2>
          <p className="text-lg max-w-2xl mx-auto" style={{ color: theme.textBody }}>
            The classic block puzzle formula, reimagined with modern design and satisfying gameplay.
          </p>
        </div>
        
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
          <FeatureCard 
            icon="🎮" 
            title="10×10 Grid" 
            description="A perfectly sized board that's challenging yet never overwhelming."
          />
          <FeatureCard 
            icon="🧩" 
            title="Classic Shapes" 
            description="All your favorite tetromino pieces—L's, T's, squares, lines, and more."
          />
          <FeatureCard 
            icon="💥" 
            title="Clear & Combo" 
            description="Complete rows and columns to clear them. Chain clears for massive combos!"
          />
          <FeatureCard 
            icon="🧘" 
            title="No Pressure" 
            description="No timers, no speed. Think carefully and play at your own pace."
          />
        </div>
      </section>

      {/* How to Play Section */}
      <section className="relative z-10 px-6 md:px-12 lg:px-24 py-20" style={{ background: theme.bgSection }}>
        <div className="text-center mb-16">
          <h2 className="text-3xl md:text-4xl font-black mb-4" style={{ color: theme.textPrimary }}>
            How to{" "}
            <span style={{ background: theme.accentTeal, WebkitBackgroundClip: 'text', WebkitTextFillColor: 'transparent', backgroundClip: 'text' }}>
              Play
            </span>
          </h2>
          <p className="text-lg" style={{ color: theme.textBody }}>
            Learn in seconds, master over time.
          </p>
        </div>
        
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-8 max-w-5xl mx-auto">
          <HowToPlayStep 
            number={1}
            icon="👆"
            title="Drag & Drop"
            description="Pick up a block from your queue and drag it onto the grid."
          />
          <HowToPlayStep 
            number={2}
            icon="📐"
            title="Find the Fit"
            description="Place blocks strategically to fill complete rows or columns."
          />
          <HowToPlayStep 
            number={3}
            icon="✨"
            title="Clear Lines"
            description="Filled lines disappear with a satisfying animation, scoring points."
          />
          <HowToPlayStep 
            number={4}
            icon="🏆"
            title="Beat Your Best"
            description="Keep playing until no pieces fit. Try to top the leaderboard!"
          />
        </div>
      </section>

      {/* Why Players Love It */}
      <section className="relative z-10 px-6 md:px-12 lg:px-24 py-20">
        <div className="text-center mb-16">
          <h2 className="text-3xl md:text-4xl font-black mb-4" style={{ color: theme.textPrimary }}>
            Why Players{" "}
            <span style={{ background: theme.accentCoral, WebkitBackgroundClip: 'text', WebkitTextFillColor: 'transparent', backgroundClip: 'text' }}>
              Love It
            </span>
          </h2>
        </div>
        
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6 max-w-5xl mx-auto">
          <Testimonial 
            quote="The perfect game to unwind after work. I love how there's no pressure, just pure puzzle satisfaction."
            author="Alex M."
            role="Puzzle Enthusiast"
          />
          <Testimonial 
            quote="Simple to pick up but so hard to put down! I've been chasing the high score for weeks."
            author="Jordan K."
            role="Casual Gamer"
          />
          <Testimonial 
            quote="Beautiful design and super satisfying when you clear multiple lines at once. Highly addictive!"
            author="Sam R."
            role="Tetris Fan"
          />
        </div>
        
        <div className="flex flex-wrap justify-center gap-8 mt-16 text-center">
          <div>
            <div className="text-4xl font-black" style={{ background: theme.accentGold, WebkitBackgroundClip: 'text', WebkitTextFillColor: 'transparent', backgroundClip: 'text' }}>
              10K+
            </div>
            <div style={{ color: theme.textSecondary }}>Games Played</div>
          </div>
          <div>
            <div className="text-4xl font-black" style={{ background: theme.accentTeal, WebkitBackgroundClip: 'text', WebkitTextFillColor: 'transparent', backgroundClip: 'text' }}>
              4.9★
            </div>
            <div style={{ color: theme.textSecondary }}>Player Rating</div>
          </div>
          <div>
            <div className="text-4xl font-black" style={{ background: theme.accentOcean, WebkitBackgroundClip: 'text', WebkitTextFillColor: 'transparent', backgroundClip: 'text' }}>
              24/7
            </div>
            <div style={{ color: theme.textSecondary }}>Available</div>
          </div>
        </div>
      </section>

      {/* Leaderboard Section */}
      <section className="relative z-10 px-6 md:px-12 lg:px-24 py-20" style={{ background: theme.bgSection }}>
        <div className="max-w-4xl mx-auto">
          <div className="flex flex-col lg:flex-row items-center gap-12">
            <div className="flex-1 text-center lg:text-left">
              <h2 className="text-3xl md:text-4xl font-black mb-4" style={{ color: theme.textPrimary }}>
                Compete{" "}
                <span style={{ background: theme.accentWarm, WebkitBackgroundClip: 'text', WebkitTextFillColor: 'transparent', backgroundClip: 'text' }}>
                  Globally
                </span>
              </h2>
              <p className="text-lg mb-6" style={{ color: theme.textBody }}>
                Sign in to save your scores and see how you rank against players worldwide. 
                Every game counts toward your personal best!
              </p>
              
              <div className="space-y-4">
                <div className="flex items-center gap-4">
                  <div className="w-12 h-12 rounded-xl flex items-center justify-center text-2xl" style={{ background: 'linear-gradient(to bottom right, #fbbf24, #f59e0b)' }}>
                    🏆
                  </div>
                  <div className="text-left">
                    <div className="font-bold" style={{ color: theme.textPrimary }}>Global Leaderboard</div>
                    <div className="text-sm" style={{ color: theme.textSecondary }}>See top scores from around the world</div>
                  </div>
                </div>
                
                <div className="flex items-center gap-4">
                  <div className="w-12 h-12 rounded-xl flex items-center justify-center text-2xl" style={{ background: theme.primaryGradient }}>
                    🔐
                  </div>
                  <div className="text-left">
                    <div className="font-bold" style={{ color: theme.textPrimary }}>Easy Sign In</div>
                    <div className="text-sm" style={{ color: theme.textSecondary }}>Login with Google, Facebook, or Microsoft</div>
                  </div>
                </div>
                
                <div className="flex items-center gap-4">
                  <div className="w-12 h-12 rounded-xl flex items-center justify-center text-2xl" style={{ background: 'linear-gradient(to bottom right, #22d3ee, #06b6d4)' }}>
                    📈
                  </div>
                  <div className="text-left">
                    <div className="font-bold" style={{ color: theme.textPrimary }}>Personal Stats</div>
                    <div className="text-sm" style={{ color: theme.textSecondary }}>Track your progress and beat your best</div>
                  </div>
                </div>
              </div>
            </div>
            
            {/* Mock leaderboard */}
            <div className="flex-1 w-full max-w-sm">
              <div className="rounded-2xl p-6" style={{ background: theme.bgCard, border: `1px solid ${theme.borderLight}` }}>
                <div className="text-center mb-4">
                  <span className="text-2xl">🏆</span>
                  <h3 className="font-bold text-lg" style={{ color: theme.textPrimary }}>Top Players</h3>
                </div>
                <div className="space-y-3">
                  {[
                    { rank: 1, name: "PuzzleMaster", score: "125,400", medal: "🥇" },
                    { rank: 2, name: "BlockChamp", score: "118,250", medal: "🥈" },
                    { rank: 3, name: "TetraKing", score: "112,800", medal: "🥉" },
                    { rank: 4, name: "GridWizard", score: "98,650", medal: "" },
                    { rank: 5, name: "LineClearer", score: "95,200", medal: "" },
                  ].map((player) => (
                    <div key={player.rank} className="flex items-center gap-3 p-3 rounded-lg" style={{ background: 'rgba(20, 184, 166, 0.1)' }}>
                      <span className="w-6 text-center font-bold" style={{ color: theme.textSecondary }}>
                        {player.medal || player.rank}
                      </span>
                      <span className="flex-1 font-medium truncate" style={{ color: theme.textPrimary }}>{player.name}</span>
                      <span className="font-mono" style={{ color: '#fbbf24' }}>{player.score}</span>
                    </div>
                  ))}
                </div>
                <div className="mt-4 pt-4 text-center" style={{ borderTop: `1px solid ${theme.borderLight}` }}>
                  <div className="text-sm" style={{ color: theme.textSecondary }}>Your rank: <span className="font-bold" style={{ color: theme.textPrimary }}>—</span></div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* Final CTA Section */}
      <section className="relative z-10 px-6 md:px-12 lg:px-24 py-24">
        <div className="relative max-w-4xl mx-auto text-center">
          {/* Background glow */}
          <div className="absolute inset-0 blur-3xl rounded-full" style={{ background: `linear-gradient(to right, ${theme.glowTeal}, ${theme.glowBlue}, ${theme.glowCyan})` }} />
          
          <div className="relative">
            <h2 className="text-4xl md:text-5xl lg:text-6xl font-black mb-6" style={{ color: theme.textPrimary }}>
              Ready to Find Your{" "}
              <span style={{ background: theme.accentOcean, WebkitBackgroundClip: 'text', WebkitTextFillColor: 'transparent', backgroundClip: 'text' }}>
                Perfect Fit?
              </span>
            </h2>
            <p className="text-xl mb-10 max-w-2xl mx-auto" style={{ color: theme.textBody }}>
              Join thousands of players in this relaxing, addictive puzzle experience. 
              No downloads, no sign-up required—just click and play!
            </p>
            <PlayNowButton size="large" />
          </div>
        </div>
      </section>

      {/* Footer */}
      <footer className="relative z-10 px-6 md:px-12 lg:px-24 py-12" style={{ borderTop: `1px solid ${theme.borderLight}` }}>
        <div className="max-w-6xl mx-auto flex flex-col md:flex-row items-center justify-between gap-6">
          <div className="flex items-center gap-2">
            <div className="w-8 h-8 rounded-lg flex items-center justify-center" style={{ background: theme.primaryGradient }}>
              <span className="text-sm font-black" style={{ color: theme.textPrimary }}>P</span>
            </div>
            <span className="font-bold" style={{ color: theme.textPrimary }}>PerfectFit</span>
          </div>
          
          <nav className="flex flex-wrap items-center justify-center gap-6 text-sm" style={{ color: theme.textSecondary }}>
            <Link href={PLAY_NOW_URL} className="hover:text-white transition-colors">
              Play Game
            </Link>
            <Link href="/leaderboard" className="hover:text-white transition-colors">
              Leaderboard
            </Link>
            <Link href="/privacy" className="hover:text-white transition-colors">
              Privacy Policy
            </Link>
            <Link href="/terms" className="hover:text-white transition-colors">
              Terms of Service
            </Link>
          </nav>
          
          <div className="text-sm" style={{ color: theme.textMuted }}>
            © {new Date().getFullYear()} PerfectFit. All rights reserved.
          </div>
        </div>
      </footer>
    </div>
  );
}
