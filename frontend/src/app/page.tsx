import Link from "next/link";

// ============================================================
// CONFIGURATION: Change this URL to your actual game URL
// ============================================================
const PLAY_NOW_URL = "/play";
// ============================================================

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
    { shape: [[1, 1], [1, 1]], color: "#facc15", delay: "0s", x: "10%", y: "20%" },
    { shape: [[1, 1, 1, 1]], color: "#22d3ee", delay: "2s", x: "80%", y: "15%" },
    { shape: [[1, 0], [1, 0], [1, 1]], color: "#fb923c", delay: "4s", x: "15%", y: "70%" },
    { shape: [[0, 1], [1, 1], [1, 0]], color: "#4ade80", delay: "1s", x: "85%", y: "60%" },
    { shape: [[1, 1, 1], [0, 1, 0]], color: "#c084fc", delay: "3s", x: "5%", y: "45%" },
    { shape: [[1, 1, 0], [0, 1, 1]], color: "#f87171", delay: "5s", x: "90%", y: "35%" },
    { shape: [[0, 1, 1], [1, 1, 0]], color: "#f472b6", delay: "2.5s", x: "75%", y: "80%" },
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
      if (row === 9 && col < 7) return "#22d3ee"; // cyan
      if (row === 8 && col >= 2 && col <= 5) return "#facc15"; // yellow
      if (row === 7 && col >= 0 && col <= 2) return "#c084fc"; // purple
      if (row === 6 && col >= 7 && col <= 9) return "#4ade80"; // green
      if (row === 5 && col >= 4 && col <= 6) return "#fb923c"; // orange
      if (col === 9 && row >= 5) return "#f472b6"; // pink
      return null;
    })
  );

  return (
    <div className="relative">
      {/* Glow effect behind board */}
      <div className="absolute inset-0 blur-3xl scale-110" style={{ background: 'linear-gradient(to right, rgba(168,85,247,0.4), rgba(59,130,246,0.4), rgba(34,211,238,0.4))' }} />
      
      {/* The game board */}
      <div className="relative p-3 rounded-2xl shadow-2xl" style={{ background: 'rgba(15, 23, 42, 0.95)', border: '1px solid rgba(255,255,255,0.15)' }}>
        <div className="grid grid-cols-10 gap-0.5">
          {board.flat().map((cell, i) => (
            <div
              key={i}
              className="w-5 h-5 md:w-6 md:h-6 rounded-sm transition-all duration-300"
              style={cell 
                ? { background: cell, boxShadow: '0 2px 4px rgba(0,0,0,0.3)' }
                : { background: 'rgba(51, 65, 85, 0.7)', border: '1px solid rgba(71, 85, 105, 0.5)' }
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
      className="group rounded-2xl p-6 transition-all duration-300 hover:-translate-y-1"
      style={{ 
        background: 'rgba(30, 41, 59, 0.8)', 
        border: '1px solid rgba(255,255,255,0.15)',
      }}
    >
      <div className="text-4xl mb-4">{icon}</div>
      <h3 className="text-xl font-bold mb-2" style={{ color: '#ffffff' }}>{title}</h3>
      <p style={{ color: '#cbd5e1' }}>{description}</p>
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
          style={{ background: 'linear-gradient(to bottom right, #a855f7, #3b82f6)' }}
        >
          {icon}
        </div>
        <div 
          className="absolute -top-2 -right-2 w-8 h-8 rounded-full font-bold flex items-center justify-center text-sm shadow-md"
          style={{ background: '#facc15', color: '#0f172a' }}
        >
          {number}
        </div>
      </div>
      <h3 className="text-lg font-bold mb-2" style={{ color: '#ffffff' }}>{title}</h3>
      <p className="text-sm max-w-xs" style={{ color: '#cbd5e1' }}>{description}</p>
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
      className="rounded-2xl p-6 transition-all duration-300"
      style={{ background: 'rgba(30, 41, 59, 0.8)', border: '1px solid rgba(255,255,255,0.15)' }}
    >
      <div className="text-2xl mb-4" style={{ color: '#facc15' }}>&ldquo;</div>
      <p className="italic mb-4" style={{ color: '#e2e8f0' }}>{quote}</p>
      <div className="flex items-center gap-3">
        <div className="w-10 h-10 rounded-full" style={{ background: 'linear-gradient(to bottom right, #c084fc, #60a5fa)' }} />
        <div>
          <div className="font-semibold" style={{ color: '#ffffff' }}>{author}</div>
          <div className="text-sm" style={{ color: '#94a3b8' }}>{role}</div>
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
        color: '#ffffff',
        background: 'linear-gradient(to right, #9333ea, #3b82f6, #22d3ee)',
        boxShadow: '0 10px 40px rgba(147, 51, 234, 0.4)'
      }}
    >
      <span className="relative z-10 flex items-center gap-2">
        Play Now
        <svg className="w-5 h-5 group-hover:translate-x-1 transition-transform" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 7l5 5m0 0l-5 5m5-5H6" />
        </svg>
      </span>
      <div className="absolute inset-0 opacity-0 group-hover:opacity-100 transition-opacity duration-300" style={{ background: 'linear-gradient(to right, #7c3aed, #2563eb, #06b6d4)' }} />
    </Link>
  );
}

export default function LandingPage() {
  return (
    <div className="min-h-screen text-white overflow-x-hidden" style={{ background: 'linear-gradient(to bottom right, #0f172a, #1e1b4b, #0f172a)' }}>
      {/* Animated background */}
      <div className="fixed inset-0 overflow-hidden pointer-events-none" style={{ zIndex: 0 }}>
        <div className="absolute top-0 left-1/4 w-96 h-96 rounded-full blur-3xl animate-pulse-slow" style={{ background: 'rgba(168, 85, 247, 0.4)' }} />
        <div className="absolute bottom-1/4 right-1/4 w-80 h-80 rounded-full blur-3xl animate-pulse-slow" style={{ background: 'rgba(59, 130, 246, 0.4)', animationDelay: "1s" }} />
        <div className="absolute top-1/2 left-1/2 w-64 h-64 rounded-full blur-3xl animate-pulse-slow" style={{ background: 'rgba(34, 211, 238, 0.3)', animationDelay: "2s" }} />
      </div>
      
      <FloatingPieces />
      
      {/* Navigation */}
      <nav className="relative z-20 flex items-center justify-between px-6 md:px-12 py-6">
        <div className="flex items-center gap-2">
          <div className="w-10 h-10 rounded-xl flex items-center justify-center" style={{ background: 'linear-gradient(to bottom right, #a855f7, #3b82f6)' }}>
            <span className="text-xl font-black" style={{ color: '#ffffff' }}>P</span>
          </div>
          <span className="text-xl font-bold tracking-tight" style={{ color: '#ffffff' }}>PerfectFit</span>
        </div>
        <PlayNowButton />
      </nav>

      {/* Hero Section */}
      <section className="relative z-10 flex flex-col lg:flex-row items-center justify-between gap-12 px-6 md:px-12 lg:px-24 py-12 lg:py-20">
        <div className="flex-1 text-center lg:text-left max-w-2xl">
          <div 
            className="inline-flex items-center gap-2 px-4 py-2 rounded-full text-sm font-medium mb-6"
            style={{ background: 'rgba(255,255,255,0.15)', border: '1px solid rgba(255,255,255,0.25)', color: '#ffffff' }}
          >
            <span className="w-2 h-2 rounded-full animate-pulse" style={{ background: '#4ade80' }} />
            Free to Play • No Download Required
          </div>
          
          <h1 className="text-4xl sm:text-5xl md:text-6xl lg:text-7xl font-black leading-tight mb-6" style={{ color: '#ffffff' }}>
            Every Block Has a{" "}
            <span style={{ background: 'linear-gradient(to right, #c084fc, #60a5fa, #22d3ee)', WebkitBackgroundClip: 'text', WebkitTextFillColor: 'transparent', backgroundClip: 'text' }}>
              Perfect Fit
            </span>
          </h1>
          
          <p className="text-lg md:text-xl mb-8 max-w-lg mx-auto lg:mx-0" style={{ color: '#cbd5e1' }}>
            A relaxing yet strategic block puzzle game. Place colorful tetromino shapes, clear lines, 
            and chase high scores in this beautifully crafted brain teaser.
          </p>
          
          <div className="flex flex-col sm:flex-row items-center gap-4 justify-center lg:justify-start">
            <PlayNowButton size="large" />
            <div className="flex items-center gap-2" style={{ color: '#94a3b8' }}>
              <div className="flex -space-x-2">
                {[...Array(4)].map((_, i) => (
                  <div key={i} className="w-8 h-8 rounded-full" style={{ background: 'linear-gradient(to bottom right, #c084fc, #60a5fa)', border: '2px solid #0f172a' }} />
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
          <h2 className="text-3xl md:text-4xl font-black mb-4" style={{ color: '#ffffff' }}>
            Simple Rules,{" "}
            <span style={{ background: 'linear-gradient(to right, #facc15, #fb923c)', WebkitBackgroundClip: 'text', WebkitTextFillColor: 'transparent', backgroundClip: 'text' }}>
              Endless Fun
            </span>
          </h2>
          <p className="text-lg max-w-2xl mx-auto" style={{ color: '#cbd5e1' }}>
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
      <section className="relative z-10 px-6 md:px-12 lg:px-24 py-20" style={{ background: 'rgba(255,255,255,0.05)' }}>
        <div className="text-center mb-16">
          <h2 className="text-3xl md:text-4xl font-black mb-4" style={{ color: '#ffffff' }}>
            How to{" "}
            <span style={{ background: 'linear-gradient(to right, #4ade80, #22d3ee)', WebkitBackgroundClip: 'text', WebkitTextFillColor: 'transparent', backgroundClip: 'text' }}>
              Play
            </span>
          </h2>
          <p className="text-lg" style={{ color: '#cbd5e1' }}>
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
          <h2 className="text-3xl md:text-4xl font-black mb-4" style={{ color: '#ffffff' }}>
            Why Players{" "}
            <span style={{ background: 'linear-gradient(to right, #f472b6, #c084fc)', WebkitBackgroundClip: 'text', WebkitTextFillColor: 'transparent', backgroundClip: 'text' }}>
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
            <div className="text-4xl font-black" style={{ background: 'linear-gradient(to right, #facc15, #fb923c)', WebkitBackgroundClip: 'text', WebkitTextFillColor: 'transparent', backgroundClip: 'text' }}>
              10K+
            </div>
            <div style={{ color: '#94a3b8' }}>Games Played</div>
          </div>
          <div>
            <div className="text-4xl font-black" style={{ background: 'linear-gradient(to right, #4ade80, #22d3ee)', WebkitBackgroundClip: 'text', WebkitTextFillColor: 'transparent', backgroundClip: 'text' }}>
              4.9★
            </div>
            <div style={{ color: '#94a3b8' }}>Player Rating</div>
          </div>
          <div>
            <div className="text-4xl font-black" style={{ background: 'linear-gradient(to right, #c084fc, #f472b6)', WebkitBackgroundClip: 'text', WebkitTextFillColor: 'transparent', backgroundClip: 'text' }}>
              24/7
            </div>
            <div style={{ color: '#94a3b8' }}>Available</div>
          </div>
        </div>
      </section>

      {/* Leaderboard Section */}
      <section className="relative z-10 px-6 md:px-12 lg:px-24 py-20" style={{ background: 'rgba(255,255,255,0.05)' }}>
        <div className="max-w-4xl mx-auto">
          <div className="flex flex-col lg:flex-row items-center gap-12">
            <div className="flex-1 text-center lg:text-left">
              <h2 className="text-3xl md:text-4xl font-black mb-4" style={{ color: '#ffffff' }}>
                Compete{" "}
                <span style={{ background: 'linear-gradient(to right, #facc15, #f87171)', WebkitBackgroundClip: 'text', WebkitTextFillColor: 'transparent', backgroundClip: 'text' }}>
                  Globally
                </span>
              </h2>
              <p className="text-lg mb-6" style={{ color: '#cbd5e1' }}>
                Sign in to save your scores and see how you rank against players worldwide. 
                Every game counts toward your personal best!
              </p>
              
              <div className="space-y-4">
                <div className="flex items-center gap-4">
                  <div className="w-12 h-12 rounded-xl flex items-center justify-center text-2xl" style={{ background: 'linear-gradient(to bottom right, #eab308, #ca8a04)' }}>
                    🏆
                  </div>
                  <div className="text-left">
                    <div className="font-bold" style={{ color: '#ffffff' }}>Global Leaderboard</div>
                    <div className="text-sm" style={{ color: '#94a3b8' }}>See top scores from around the world</div>
                  </div>
                </div>
                
                <div className="flex items-center gap-4">
                  <div className="w-12 h-12 rounded-xl flex items-center justify-center text-2xl" style={{ background: 'linear-gradient(to bottom right, #3b82f6, #2563eb)' }}>
                    🔐
                  </div>
                  <div className="text-left">
                    <div className="font-bold" style={{ color: '#ffffff' }}>Easy Sign In</div>
                    <div className="text-sm" style={{ color: '#94a3b8' }}>Login with Google, Apple, or Microsoft</div>
                  </div>
                </div>
                
                <div className="flex items-center gap-4">
                  <div className="w-12 h-12 rounded-xl flex items-center justify-center text-2xl" style={{ background: 'linear-gradient(to bottom right, #a855f7, #9333ea)' }}>
                    📈
                  </div>
                  <div className="text-left">
                    <div className="font-bold" style={{ color: '#ffffff' }}>Personal Stats</div>
                    <div className="text-sm" style={{ color: '#94a3b8' }}>Track your progress and beat your best</div>
                  </div>
                </div>
              </div>
            </div>
            
            {/* Mock leaderboard */}
            <div className="flex-1 w-full max-w-sm">
              <div className="rounded-2xl p-6" style={{ background: 'rgba(30, 41, 59, 0.8)', border: '1px solid rgba(255,255,255,0.15)' }}>
                <div className="text-center mb-4">
                  <span className="text-2xl">🏆</span>
                  <h3 className="font-bold text-lg" style={{ color: '#ffffff' }}>Top Players</h3>
                </div>
                <div className="space-y-3">
                  {[
                    { rank: 1, name: "PuzzleMaster", score: "125,400", medal: "🥇" },
                    { rank: 2, name: "BlockChamp", score: "118,250", medal: "🥈" },
                    { rank: 3, name: "TetraKing", score: "112,800", medal: "🥉" },
                    { rank: 4, name: "GridWizard", score: "98,650", medal: "" },
                    { rank: 5, name: "LineClearer", score: "95,200", medal: "" },
                  ].map((player) => (
                    <div key={player.rank} className="flex items-center gap-3 p-3 rounded-lg" style={{ background: 'rgba(255,255,255,0.08)' }}>
                      <span className="w-6 text-center font-bold" style={{ color: '#94a3b8' }}>
                        {player.medal || player.rank}
                      </span>
                      <span className="flex-1 font-medium truncate" style={{ color: '#ffffff' }}>{player.name}</span>
                      <span className="font-mono" style={{ color: '#facc15' }}>{player.score}</span>
                    </div>
                  ))}
                </div>
                <div className="mt-4 pt-4 text-center" style={{ borderTop: '1px solid rgba(255,255,255,0.1)' }}>
                  <div className="text-sm" style={{ color: '#94a3b8' }}>Your rank: <span className="font-bold" style={{ color: '#ffffff' }}>—</span></div>
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
          <div className="absolute inset-0 blur-3xl rounded-full" style={{ background: 'linear-gradient(to right, rgba(168,85,247,0.3), rgba(59,130,246,0.3), rgba(34,211,238,0.3))' }} />
          
          <div className="relative">
            <h2 className="text-4xl md:text-5xl lg:text-6xl font-black mb-6" style={{ color: '#ffffff' }}>
              Ready to Find Your{" "}
              <span style={{ background: 'linear-gradient(to right, #c084fc, #60a5fa, #22d3ee)', WebkitBackgroundClip: 'text', WebkitTextFillColor: 'transparent', backgroundClip: 'text' }}>
                Perfect Fit?
              </span>
            </h2>
            <p className="text-xl mb-10 max-w-2xl mx-auto" style={{ color: '#cbd5e1' }}>
              Join thousands of players in this relaxing, addictive puzzle experience. 
              No downloads, no sign-up required—just click and play!
            </p>
            <PlayNowButton size="large" />
          </div>
        </div>
      </section>

      {/* Footer */}
      <footer className="relative z-10 px-6 md:px-12 lg:px-24 py-12" style={{ borderTop: '1px solid rgba(255,255,255,0.1)' }}>
        <div className="max-w-6xl mx-auto flex flex-col md:flex-row items-center justify-between gap-6">
          <div className="flex items-center gap-2">
            <div className="w-8 h-8 rounded-lg flex items-center justify-center" style={{ background: 'linear-gradient(to bottom right, #a855f7, #3b82f6)' }}>
              <span className="text-sm font-black" style={{ color: '#ffffff' }}>P</span>
            </div>
            <span className="font-bold" style={{ color: '#ffffff' }}>PerfectFit</span>
          </div>
          
          <nav className="flex flex-wrap items-center justify-center gap-6 text-sm" style={{ color: '#94a3b8' }}>
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
          
          <div className="text-sm" style={{ color: '#64748b' }}>
            © {new Date().getFullYear()} PerfectFit. All rights reserved.
          </div>
        </div>
      </footer>
    </div>
  );
}
