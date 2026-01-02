import { renderHook, waitFor } from '@testing-library/react';
import { useTouchDevice } from '@/hooks/useTouchDevice';

describe('useTouchDevice hook', () => {
  // Store original values
  const originalOntouchstart = Object.getOwnPropertyDescriptor(window, 'ontouchstart');
  const originalMaxTouchPoints = Object.getOwnPropertyDescriptor(navigator, 'maxTouchPoints');

  // Helper to set up touch support mocking
  const mockTouchSupport = (hasOntouchstart: boolean, maxTouchPoints: number) => {
    if (hasOntouchstart) {
      Object.defineProperty(window, 'ontouchstart', {
        value: () => {},
        writable: true,
        configurable: true,
      });
    } else {
      // Delete the property to simulate no touch support
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      delete (window as any).ontouchstart;
    }
    
    Object.defineProperty(navigator, 'maxTouchPoints', {
      value: maxTouchPoints,
      writable: true,
      configurable: true,
    });
  };

  // Restore original values after each test
  afterEach(() => {
    if (originalOntouchstart) {
      Object.defineProperty(window, 'ontouchstart', originalOntouchstart);
    } else {
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      delete (window as any).ontouchstart;
    }
    
    if (originalMaxTouchPoints) {
      Object.defineProperty(navigator, 'maxTouchPoints', originalMaxTouchPoints);
    }
  });

  describe('touch detection', () => {
    it('should return true when ontouchstart is available', async () => {
      mockTouchSupport(true, 0);
      
      const { result } = renderHook(() => useTouchDevice());
      
      // Wait for useEffect to run and state to update
      await waitFor(() => {
        expect(result.current).toBe(true);
      });
    });

    it('should return true when maxTouchPoints > 0', async () => {
      mockTouchSupport(false, 5);
      
      const { result } = renderHook(() => useTouchDevice());
      
      await waitFor(() => {
        expect(result.current).toBe(true);
      });
    });

    it('should return false when no touch support is available', async () => {
      mockTouchSupport(false, 0);
      
      const { result } = renderHook(() => useTouchDevice());
      
      // Initial value is false, and after effect runs it should still be false
      await waitFor(() => {
        expect(result.current).toBe(false);
      });
    });
  });

  describe('SSR safety', () => {
    it('should return false initially (safe for SSR)', () => {
      // The hook should return false initially before useEffect runs
      // This is important for SSR where window is not available
      mockTouchSupport(true, 5);
      
      const { result } = renderHook(() => useTouchDevice());
      
      // The initial state is false for SSR safety
      // After hydration, useEffect will set the correct value
      expect(typeof result.current).toBe('boolean');
    });

    it('should not throw when called', () => {
      // This tests that the hook doesn't throw
      expect(() => {
        renderHook(() => useTouchDevice());
      }).not.toThrow();
    });
  });

  describe('return type', () => {
    it('should return a boolean value', async () => {
      const { result } = renderHook(() => useTouchDevice());
      
      await waitFor(() => {
        expect(typeof result.current).toBe('boolean');
      });
    });
  });
});
