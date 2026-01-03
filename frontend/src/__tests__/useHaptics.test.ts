import { renderHook } from '@testing-library/react';
import { useHaptics, HapticFeedback } from '@/hooks/useHaptics';

describe('useHaptics hook', () => {
  // Store original navigator.vibrate
  const originalVibrate = navigator.vibrate;
  
  // Mock vibrate function
  let mockVibrate: jest.Mock;

  beforeEach(() => {
    mockVibrate = jest.fn().mockReturnValue(true);
    Object.defineProperty(navigator, 'vibrate', {
      value: mockVibrate,
      writable: true,
      configurable: true,
    });
  });

  afterEach(() => {
    // Restore original vibrate
    Object.defineProperty(navigator, 'vibrate', {
      value: originalVibrate,
      writable: true,
      configurable: true,
    });
  });

  describe('return type', () => {
    it('should return an object with haptic functions', () => {
      const { result } = renderHook(() => useHaptics());
      
      expect(result.current).toBeDefined();
      expect(typeof result.current.isSupported).toBe('boolean');
      expect(typeof result.current.lightTap).toBe('function');
      expect(typeof result.current.mediumTap).toBe('function');
      expect(typeof result.current.lineClear).toBe('function');
      expect(typeof result.current.gameOver).toBe('function');
      expect(typeof result.current.success).toBe('function');
      expect(typeof result.current.error).toBe('function');
    });

    it('should have stable function references (memoized)', () => {
      const { result, rerender } = renderHook(() => useHaptics());
      
      const firstRender: HapticFeedback = { ...result.current };
      rerender();
      
      expect(result.current.lightTap).toBe(firstRender.lightTap);
      expect(result.current.mediumTap).toBe(firstRender.mediumTap);
      expect(result.current.lineClear).toBe(firstRender.lineClear);
      expect(result.current.gameOver).toBe(firstRender.gameOver);
      expect(result.current.success).toBe(firstRender.success);
      expect(result.current.error).toBe(firstRender.error);
    });
  });

  describe('isSupported', () => {
    it('should return true when navigator.vibrate is available', () => {
      const { result } = renderHook(() => useHaptics());
      
      expect(result.current.isSupported).toBe(true);
    });

    it('should return false when navigator.vibrate is unavailable', () => {
      // Remove vibrate to simulate iOS Safari
      Object.defineProperty(navigator, 'vibrate', {
        value: undefined,
        writable: true,
        configurable: true,
      });
      
      const { result } = renderHook(() => useHaptics());
      
      expect(result.current.isSupported).toBe(false);
    });
  });

  describe('vibration patterns', () => {
    it('should call vibrate(10) for lightTap', () => {
      const { result } = renderHook(() => useHaptics());
      
      result.current.lightTap();
      
      expect(mockVibrate).toHaveBeenCalledWith(10);
    });

    it('should call vibrate(20) for mediumTap', () => {
      const { result } = renderHook(() => useHaptics());
      
      result.current.mediumTap();
      
      expect(mockVibrate).toHaveBeenCalledWith(20);
    });

    it('should call vibrate([50, 30, 50]) for lineClear', () => {
      const { result } = renderHook(() => useHaptics());
      
      result.current.lineClear();
      
      expect(mockVibrate).toHaveBeenCalledWith([50, 30, 50]);
    });

    it('should call vibrate([100, 50, 100, 50, 100]) for gameOver', () => {
      const { result } = renderHook(() => useHaptics());
      
      result.current.gameOver();
      
      expect(mockVibrate).toHaveBeenCalledWith([100, 50, 100, 50, 100]);
    });

    it('should call vibrate(30) for success', () => {
      const { result } = renderHook(() => useHaptics());
      
      result.current.success();
      
      expect(mockVibrate).toHaveBeenCalledWith(30);
    });

    it('should call vibrate([50, 100, 50]) for error', () => {
      const { result } = renderHook(() => useHaptics());
      
      result.current.error();
      
      expect(mockVibrate).toHaveBeenCalledWith([50, 100, 50]);
    });
  });

  describe('graceful fallback when vibration unavailable', () => {
    beforeEach(() => {
      // Simulate iOS Safari - no vibration support
      Object.defineProperty(navigator, 'vibrate', {
        value: undefined,
        writable: true,
        configurable: true,
      });
    });

    it('should not throw when lightTap is called', () => {
      const { result } = renderHook(() => useHaptics());
      
      expect(() => result.current.lightTap()).not.toThrow();
    });

    it('should not throw when mediumTap is called', () => {
      const { result } = renderHook(() => useHaptics());
      
      expect(() => result.current.mediumTap()).not.toThrow();
    });

    it('should not throw when lineClear is called', () => {
      const { result } = renderHook(() => useHaptics());
      
      expect(() => result.current.lineClear()).not.toThrow();
    });

    it('should not throw when gameOver is called', () => {
      const { result } = renderHook(() => useHaptics());
      
      expect(() => result.current.gameOver()).not.toThrow();
    });

    it('should not throw when success is called', () => {
      const { result } = renderHook(() => useHaptics());
      
      expect(() => result.current.success()).not.toThrow();
    });

    it('should not throw when error is called', () => {
      const { result } = renderHook(() => useHaptics());
      
      expect(() => result.current.error()).not.toThrow();
    });
  });

  describe('error handling', () => {
    it('should not throw when vibrate throws an exception', () => {
      // Simulate vibrate throwing an error (e.g., browser restrictions)
      mockVibrate.mockImplementation(() => {
        throw new Error('Vibration not allowed');
      });
      
      const { result } = renderHook(() => useHaptics());
      
      expect(() => result.current.lightTap()).not.toThrow();
      expect(() => result.current.mediumTap()).not.toThrow();
      expect(() => result.current.lineClear()).not.toThrow();
      expect(() => result.current.gameOver()).not.toThrow();
    });

    it('should silently handle vibration failure', () => {
      // Vibration API can return false to indicate failure
      mockVibrate.mockReturnValue(false);
      
      const { result } = renderHook(() => useHaptics());
      
      // Should not throw and should still call vibrate
      expect(() => result.current.lightTap()).not.toThrow();
      expect(mockVibrate).toHaveBeenCalled();
    });
  });
});
