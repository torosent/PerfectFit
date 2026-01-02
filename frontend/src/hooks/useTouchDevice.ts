'use client';

import { useState, useEffect } from 'react';

/**
 * Hook to detect if the current device supports touch input
 * Returns false on SSR and true only when touch support is detected in the browser
 */
export function useTouchDevice(): boolean {
  const [isTouch, setIsTouch] = useState(false);

  useEffect(() => {
    const checkTouch = () => {
      setIsTouch(
        'ontouchstart' in window ||
        navigator.maxTouchPoints > 0 ||
        // @ts-expect-error - msMaxTouchPoints is IE specific
        navigator.msMaxTouchPoints > 0
      );
    };
    checkTouch();
  }, []);

  return isTouch;
}
