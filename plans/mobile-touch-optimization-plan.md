## Plan: Mobile Touch Optimization for PerfectFit

Enhance the PerfectFit game frontend for mobile devices by fixing touch event handling, adding proper CSS touch-action properties, increasing touch target sizes (minimum iPhone SE @ 375px), and improving the overall mobile UX with responsive layouts, haptic feedback, iOS safe-area support, and PWA offline capabilities.

**Design Decisions (from user):**
- ❌ No piece rotation via gestures
- 📱 Minimum target: iPhone SE (375px width)
- ✅ PWA features for offline mobile play
- ✅ iOS safe-area inset handling (notch/home indicator)

---

**Phases (7 phases)**

### 1. **Phase 1: Core Touch Event Handling**
- **Objective:** Fix fundamental touch interaction issues - add proper touch-action CSS and prevent browser gestures during gameplay
- **Files/Functions to Modify/Create:**
  - `frontend/src/components/game/DndProvider.tsx` - Add touch-action CSS to container
  - `frontend/src/app/globals.css` - Add mobile touch utility classes
  - `frontend/src/components/game/GameBoard.tsx` - Add touch-action to game board
- **Tests to Write:**
  - `touch-action-applied.test.tsx` - Verify touch-action CSS is applied to game containers
- **Steps:**
    1. Write tests to verify touch-action CSS is applied to key game elements
    2. Run tests to see them fail
    3. Add `touch-action: none` to DndProvider container and GameBoard
    4. Add global CSS utility classes for touch interaction management
    5. Run tests to confirm they pass

### 2. **Phase 2: Increase Touch Target Sizes**
- **Objective:** Ensure all interactive elements meet the 44px minimum touch target size for iPhone SE (375px)
- **Files/Functions to Modify/Create:**
  - `frontend/src/components/game/GameBoard.tsx` - Increase cell sizes on mobile
  - `frontend/src/components/game/PieceSelection.tsx` - Increase piece padding/sizing
  - `frontend/src/components/game/GamePiece.tsx` - Add mobile-specific cell sizes
- **Tests to Write:**
  - `touch-target-sizes.test.tsx` - Verify minimum touch targets on mobile
- **Steps:**
    1. Write tests verifying touch target sizes meet minimums
    2. Run tests to see them fail
    3. Modify GameBoard to use larger cells on mobile viewports (375px target)
    4. Update PieceSelection with better touch spacing
    5. Adjust GamePiece cell sizes for mobile
    6. Run tests to confirm they pass

### 3. **Phase 3: Touch Drag Offset & Visual Feedback**
- **Objective:** Add touch offset so dragged pieces are visible above the finger, and improve visual feedback for touch interactions
- **Files/Functions to Modify/Create:**
  - `frontend/src/components/game/DraggablePiece.tsx` - Add touch offset compensation
  - `frontend/src/components/game/DndProvider.tsx` - Enhance drag overlay positioning
  - `frontend/src/hooks/useTouchDevice.ts` - Create hook to detect touch devices (NEW)
- **Tests to Write:**
  - `useTouchDevice.test.ts` - Test touch detection hook
  - `drag-offset.test.tsx` - Verify drag offset is applied on touch devices
- **Steps:**
    1. Write tests for touch device detection hook
    2. Run tests to see them fail
    3. Create `useTouchDevice` hook
    4. Run tests to confirm they pass
    5. Write tests for drag offset functionality
    6. Run tests to see them fail
    7. Modify DndProvider and DraggablePiece to apply offset on touch
    8. Run tests to confirm they pass

### 4. **Phase 4: Responsive Mobile Layout & iOS Safe Areas**
- **Objective:** Optimize game layout for mobile screens (iPhone SE minimum), add iOS safe-area inset handling
- **Files/Functions to Modify/Create:**
  - `frontend/src/app/(game)/play/page.tsx` - Add mobile-optimized layout
  - `frontend/src/components/game/ScoreDisplay.tsx` - Improve mobile typography
  - `frontend/src/app/globals.css` - Add safe-area insets and mobile utilities
  - `frontend/src/app/layout.tsx` - Add viewport meta optimizations with viewport-fit=cover
- **Tests to Write:**
  - `mobile-layout.test.tsx` - Test responsive layout classes are applied
- **Steps:**
    1. Write tests for mobile layout responsiveness
    2. Run tests to see them fail
    3. Update play page with mobile-first layout
    4. Add safe-area inset handling to globals.css (env(safe-area-inset-*))
    5. Update viewport meta in layout.tsx with viewport-fit=cover
    6. Run tests to confirm they pass

### 5. **Phase 5: Haptic Feedback & Enhanced Touch UX**
- **Objective:** Add haptic feedback for game actions and improve touch interaction states
- **Files/Functions to Modify/Create:**
  - `frontend/src/hooks/useHaptics.ts` - Create haptic feedback hook (NEW)
  - `frontend/src/lib/stores/game-store.ts` - Integrate haptics on game events
  - `frontend/src/components/game/DndProvider.tsx` - Add haptic on drop
  - `frontend/src/app/globals.css` - Add touch feedback states (active, pressed)
- **Tests to Write:**
  - `useHaptics.test.ts` - Test haptic feedback hook
- **Steps:**
    1. Write tests for haptic feedback hook
    2. Run tests to see them fail
    3. Create `useHaptics` hook with vibration API
    4. Run tests to confirm they pass
    5. Integrate haptics into DndProvider for drag/drop events
    6. Add CSS touch feedback states

### 6. **Phase 6: Mobile Navigation**
- **Objective:** Add mobile-friendly navigation with hamburger menu
- **Files/Functions to Modify/Create:**
  - `frontend/src/components/ui/MobileNav.tsx` - Create mobile navigation component (NEW)
  - `frontend/src/app/layout.tsx` - Integrate mobile nav
  - `frontend/src/components/game/GameBoard.tsx` - Add landscape orientation support
- **Tests to Write:**
  - `MobileNav.test.tsx` - Test mobile navigation component
- **Steps:**
    1. Write tests for mobile navigation component
    2. Run tests to see them fail
    3. Create MobileNav component with hamburger menu
    4. Run tests to confirm they pass
    5. Integrate mobile nav into layout
    6. Add landscape orientation handling to game board

### 7. **Phase 7: PWA Offline Support**
- **Objective:** Add Progressive Web App features for offline mobile play
- **Files/Functions to Modify/Create:**
  - `frontend/public/manifest.json` - Create PWA manifest (NEW)
  - `frontend/public/sw.js` - Create service worker for offline caching (NEW)
  - `frontend/src/app/layout.tsx` - Add manifest link and SW registration
  - `frontend/public/icons/` - Add PWA icons (192x192, 512x512)
- **Tests to Write:**
  - `pwa-manifest.test.ts` - Verify manifest is valid
  - `service-worker.test.ts` - Test SW registration
- **Steps:**
    1. Write tests for PWA manifest validity
    2. Run tests to see them fail
    3. Create manifest.json with app metadata
    4. Create service worker with cache-first strategy
    5. Add manifest link and SW registration to layout
    6. Create placeholder PWA icons
    7. Run tests to confirm they pass
    8. Run all tests to verify complete mobile experience

---

**Summary**: This 7-phase plan addresses all mobile/touch requirements:
- Phases 1-2: Fix fundamental touch problems (CSS touch-action, 44px+ target sizes for iPhone SE)
- Phase 3: Improve drag UX on touch devices
- Phase 4: Responsive layout + iOS safe-area insets
- Phase 5: Enhanced UX with haptics
- Phase 6: Mobile navigation
- Phase 7: PWA offline support
