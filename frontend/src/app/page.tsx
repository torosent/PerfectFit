import Link from "next/link";
import { theme } from "@/lib/landing-theme";
import { FloatingPieces } from "@/components/landing/FloatingPieces";
import { GameBoardPreview } from "@/components/landing/GameBoardPreview";

// ============================================================
// CONFIGURATION: Change this URL to your actual game URL
// ============================================================
const PLAY_NOW_URL = "/play";
// ============================================================

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

// Tutorial tip component
function TutorialTip({ 
  icon, 
  title, 
  description,
  highlight
}: { 
  icon: string; 
  title: string; 
  description: string;
  highlight?: string;
}) {
  return (
    <div 
      className="rounded-2xl p-6 transition-all duration-300 hover:-translate-y-1 hover:shadow-lg"
      style={{ background: theme.bgCard, border: `1px solid ${theme.borderLight}` }}
    >
      <div className="flex items-start gap-4">
        <div 
          className="w-14 h-14 rounded-xl flex items-center justify-center text-2xl shrink-0"
          style={{ background: theme.primaryGradient }}
        >
          {icon}
        </div>
        <div>
          <h4 className="text-lg font-bold mb-2" style={{ color: theme.textPrimary }}>{title}</h4>
          <p className="text-sm" style={{ color: theme.textBody }}>{description}</p>
          {highlight && (
            <div className="mt-3 inline-block px-3 py-1 rounded-full text-xs font-medium" style={{ background: 'rgba(251, 191, 36, 0.15)', color: '#fbbf24', border: '1px solid rgba(251, 191, 36, 0.3)' }}>
              💡 {highlight}
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

// FAQ Item component
function FAQItem({ 
  question, 
  answer,
  defaultOpen = false
}: { 
  question: string; 
  answer: string;
  defaultOpen?: boolean;
}) {
  return (
    <details 
      className="group rounded-2xl transition-all duration-300"
      style={{ background: theme.bgCard, border: `1px solid ${theme.borderLight}` }}
      open={defaultOpen}
    >
      <summary 
        className="flex items-center justify-between p-6 cursor-pointer list-none"
        style={{ color: theme.textPrimary }}
      >
        <span className="font-semibold pr-4">{question}</span>
        <span className="text-2xl transition-transform duration-300 group-open:rotate-45" style={{ color: theme.textSecondary }}>+</span>
      </summary>
      <div className="px-6 pb-6 pt-0" style={{ color: theme.textBody }}>
        <p>{answer}</p>
      </div>
    </details>
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
            title="8×8 Grid" 
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

      {/* Tutorial Section - Pro Tips */}
      <section id="tutorial" className="relative z-10 px-6 md:px-12 lg:px-24 py-20">
        <div className="text-center mb-16">
          <h2 className="text-3xl md:text-4xl font-black mb-4" style={{ color: theme.textPrimary }}>
            Pro{" "}
            <span style={{ background: theme.accentGold, WebkitBackgroundClip: 'text', WebkitTextFillColor: 'transparent', backgroundClip: 'text' }}>
              Tips & Strategies
            </span>
          </h2>
          <p className="text-lg max-w-2xl mx-auto" style={{ color: theme.textBody }}>
            Master these strategies to maximize your score and climb the leaderboard.
          </p>
        </div>
        
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6 max-w-4xl mx-auto">
          <TutorialTip 
            icon="🎯"
            title="Corner Strategy"
            description="Start placing pieces in one corner and work outward. This keeps your options open and prevents awkward gaps in the center of the board."
            highlight="Best for beginners"
          />
          <TutorialTip 
            icon="🔄"
            title="Plan Ahead"
            description="Always look at your upcoming pieces before placing the current one. Planning 2-3 moves ahead can be the difference between a good and great score."
          />
          <TutorialTip 
            icon="📊"
            title="Prioritize Lines"
            description="Focus on completing lines rather than filling random spaces. A nearly complete row is more valuable than scattered pieces across the board."
            highlight="Score multiplier"
          />
          <TutorialTip 
            icon="🧩"
            title="Save Large Pieces"
            description="Don't rush to place large pieces like the I-piece or L-piece. Save them for when you can clear multiple lines at once for combo bonuses."
          />
          <TutorialTip 
            icon="⚖️"
            title="Balance the Board"
            description="Keep the height of your stacked pieces relatively even. Tall towers on one side make it harder to place new pieces efficiently."
          />
          <TutorialTip 
            icon="🔥"
            title="Chain Combos"
            description="Clearing multiple lines with a single placement triggers combo bonuses. Set up your board so one piece can clear both a row and column!"
            highlight="Maximum points"
          />
        </div>
        
        {/* Video Tutorial Placeholder */}
        <div className="mt-16 max-w-3xl mx-auto">
          <div 
            className="rounded-2xl p-8 text-center"
            style={{ background: theme.bgCard, border: `1px solid ${theme.borderLight}` }}
          >
            <div className="text-6xl mb-4">🎬</div>
            <h3 className="text-xl font-bold mb-2" style={{ color: theme.textPrimary }}>Watch & Learn</h3>
            <p className="mb-6" style={{ color: theme.textBody }}>
              See these strategies in action! Our video tutorial shows you exactly how the pros achieve high scores.
            </p>
            <button 
              className="inline-flex items-center gap-2 px-6 py-3 rounded-full font-semibold transition-all duration-300 hover:scale-105"
              style={{ background: theme.primaryGradient, color: theme.textPrimary }}
            >
              <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 24 24">
                <path d="M8 5v14l11-7z"/>
              </svg>
              Coming Soon
            </button>
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

      {/* FAQ Section */}
      <section id="faq" className="relative z-10 px-6 md:px-12 lg:px-24 py-20">
        <div className="text-center mb-16">
          <h2 className="text-3xl md:text-4xl font-black mb-4" style={{ color: theme.textPrimary }}>
            Frequently Asked{" "}
            <span style={{ background: theme.accentOcean, WebkitBackgroundClip: 'text', WebkitTextFillColor: 'transparent', backgroundClip: 'text' }}>
              Questions
            </span>
          </h2>
          <p className="text-lg max-w-2xl mx-auto" style={{ color: theme.textBody }}>
            Everything you need to know about PerfectFit.
          </p>
        </div>
        
        <div className="max-w-3xl mx-auto space-y-4">
          <FAQItem 
            question="Is PerfectFit free to play?"
            answer="Yes! PerfectFit is completely free to play. There are no hidden fees, no premium subscriptions, and no pay-to-win mechanics. Just pure puzzle fun!"
            defaultOpen={true}
          />
          <FAQItem 
            question="Do I need to create an account?"
            answer="No account is required to play! You can start playing immediately. However, creating a free account allows you to save your high scores, appear on the global leaderboard, and track your progress over time."
          />
          <FAQItem 
            question="How is the score calculated?"
            answer="Your score increases based on the number of cells you clear. Clearing a single line earns base points, but clearing multiple lines simultaneously triggers combo multipliers for bonus points. The more lines you clear at once, the higher your score!"
          />
          <FAQItem 
            question="What happens when I can't place any more pieces?"
            answer="When none of your available pieces can fit on the board, the game ends. Your final score is recorded, and if you're logged in, it will be saved to your profile and potentially added to the leaderboard."
          />
          <FAQItem 
            question="Can I play on mobile devices?"
            answer="Absolutely! PerfectFit is fully optimized for mobile devices. Simply drag and drop pieces with your finger. The game works great on phones, tablets, and desktop browsers."
          />
          <FAQItem 
            question="How do combo bonuses work?"
            answer="Combos occur when you clear multiple lines with a single piece placement, or when clearing one line causes another line to complete (chain reactions). Each consecutive clear multiplies your points, so setting up big combo opportunities is key to high scores!"
          />
          <FAQItem 
            question="Is there a time limit?"
            answer="No! PerfectFit is a relaxing, untimed puzzle game. Take as long as you need to think about your next move. There's no pressure—play at your own pace."
          />
          <FAQItem 
            question="Can I play offline?"
            answer="Yes, PerfectFit works offline once the page has loaded. However, you'll need an internet connection to save scores to the leaderboard and sync your progress across devices."
          />
        </div>
        
        {/* Contact Support */}
        <div className="mt-12 text-center">
          <p style={{ color: theme.textSecondary }}>
            Still have questions?{" "}
            <a 
              href="mailto:support@perfectfit.game" 
              className="underline hover:no-underline transition-all"
              style={{ color: '#2dd4bf' }}
            >
              Contact our support team
            </a>
          </p>
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
            <a href="#tutorial" className="hover:text-white transition-colors">
              Tutorial
            </a>
            <a href="#faq" className="hover:text-white transition-colors">
              FAQ
            </a>
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
