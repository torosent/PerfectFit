/**
 * CosmeticSelector Component Tests
 *
 * Tests for the cosmetic selection grid with accessibility.
 */

import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import { CosmeticSelector } from '@/components/gamification/CosmeticSelector';
import type { Cosmetic, CosmeticType } from '@/types/gamification';

// Mock motion/react to avoid animation issues in tests
jest.mock('motion/react', () => {
  const React = require('react');
  return {
    motion: {
      div: React.forwardRef(({ children, onClick, className, ...props }: React.PropsWithChildren<{ onClick?: () => void; className?: string }>, ref: React.Ref<HTMLDivElement>) => 
        React.createElement('div', { onClick, className, ...props, ref }, children)),
      button: React.forwardRef(({ children, onClick, className, ...props }: React.PropsWithChildren<{ onClick?: () => void; className?: string }>, ref: React.Ref<HTMLButtonElement>) => 
        React.createElement('button', { onClick, className, ...props, ref }, children)),
    },
    AnimatePresence: ({ children }: React.PropsWithChildren) => React.createElement(React.Fragment, null, children),
  };
});

describe('CosmeticSelector', () => {
  const mockCosmetics: Cosmetic[] = [
    {
      id: 1,
      name: 'Ocean Theme',
      description: 'A calm ocean board theme',
      type: 'BoardTheme',
      rarity: 'Rare',
      previewUrl: '/themes/ocean.png',
      isOwned: true,
    },
    {
      id: 2,
      name: 'Fire Theme',
      description: 'A fiery board theme',
      type: 'BoardTheme',
      rarity: 'Epic',
      previewUrl: '/themes/fire.png',
      isOwned: true,
    },
    {
      id: 3,
      name: 'Locked Theme',
      description: 'A locked theme',
      type: 'BoardTheme',
      rarity: 'Legendary',
      previewUrl: '/themes/locked.png',
      isOwned: false,
    },
  ];

  const mockEquippedIds: Record<CosmeticType, number | null> = {
    BoardTheme: 1,
    AvatarFrame: null,
    Badge: null,
  };

  const mockOnEquip = jest.fn();

  beforeEach(() => {
    mockOnEquip.mockClear();
  });

  describe('renders cosmetics', () => {
    it('should render the cosmetics header', () => {
      render(
        <CosmeticSelector
          cosmetics={mockCosmetics}
          equippedIds={mockEquippedIds}
          onEquip={mockOnEquip}
        />
      );
      expect(screen.getByText('Cosmetics')).toBeInTheDocument();
    });

    it('should render type tabs', () => {
      render(
        <CosmeticSelector
          cosmetics={mockCosmetics}
          equippedIds={mockEquippedIds}
          onEquip={mockOnEquip}
        />
      );
      expect(screen.getByRole('tab', { name: /board themes/i })).toBeInTheDocument();
      expect(screen.getByRole('tab', { name: /avatar frames/i })).toBeInTheDocument();
      expect(screen.getByRole('tab', { name: /badges/i })).toBeInTheDocument();
    });

    it('should render cosmetic names', () => {
      render(
        <CosmeticSelector
          cosmetics={mockCosmetics}
          equippedIds={mockEquippedIds}
          onEquip={mockOnEquip}
        />
      );
      expect(screen.getByText('Ocean Theme')).toBeInTheDocument();
      expect(screen.getByText('Fire Theme')).toBeInTheDocument();
    });
  });

  describe('tab functionality', () => {
    it('should have tablist role', () => {
      render(
        <CosmeticSelector
          cosmetics={mockCosmetics}
          equippedIds={mockEquippedIds}
          onEquip={mockOnEquip}
        />
      );
      expect(screen.getByRole('tablist', { name: /cosmetic types/i })).toBeInTheDocument();
    });

    it('should switch tabs when clicked', () => {
      render(
        <CosmeticSelector
          cosmetics={mockCosmetics}
          equippedIds={mockEquippedIds}
          onEquip={mockOnEquip}
        />
      );
      
      const avatarFramesTab = screen.getByRole('tab', { name: /avatar frames/i });
      fireEvent.click(avatarFramesTab);
      
      expect(avatarFramesTab).toHaveAttribute('aria-selected', 'true');
    });
  });

  describe('equip button accessibility', () => {
    it('should render equip button for owned non-equipped cosmetics', () => {
      render(
        <CosmeticSelector
          cosmetics={mockCosmetics}
          equippedIds={{ ...mockEquippedIds, BoardTheme: null }}
          onEquip={mockOnEquip}
        />
      );
      
      const equipButtons = screen.getAllByText('Equip');
      expect(equipButtons.length).toBeGreaterThan(0);
    });

    it('should have focus:opacity-100 class for keyboard visibility', () => {
      render(
        <CosmeticSelector
          cosmetics={mockCosmetics}
          equippedIds={{ ...mockEquippedIds, BoardTheme: null }}
          onEquip={mockOnEquip}
        />
      );
      
      const equipButtons = screen.getAllByRole('button', { name: /equip/i });
      // Find the overlay button (contains 'Equip' span)
      const overlayButton = equipButtons.find(btn => btn.className.includes('opacity-0'));
      
      expect(overlayButton).toBeDefined();
      expect(overlayButton?.className).toContain('focus:opacity-100');
      expect(overlayButton?.className).toContain('focus:ring-2');
    });

    it('should have group-focus-within:opacity-100 class for focus-within visibility', () => {
      render(
        <CosmeticSelector
          cosmetics={mockCosmetics}
          equippedIds={{ ...mockEquippedIds, BoardTheme: null }}
          onEquip={mockOnEquip}
        />
      );
      
      const equipButtons = screen.getAllByRole('button', { name: /equip/i });
      const overlayButton = equipButtons.find(btn => btn.className.includes('opacity-0'));
      
      expect(overlayButton?.className).toContain('group-focus-within:opacity-100');
    });

    it('should call onEquip when equip button is clicked', () => {
      render(
        <CosmeticSelector
          cosmetics={mockCosmetics}
          equippedIds={{ ...mockEquippedIds, BoardTheme: null }}
          onEquip={mockOnEquip}
        />
      );
      
      const equipButtons = screen.getAllByRole('button', { name: /equip/i });
      const overlayButton = equipButtons.find(btn => btn.className.includes('opacity-0'));
      
      if (overlayButton) {
        fireEvent.click(overlayButton);
        expect(mockOnEquip).toHaveBeenCalled();
      }
    });

    it('should be focusable via keyboard', () => {
      render(
        <CosmeticSelector
          cosmetics={mockCosmetics}
          equippedIds={{ ...mockEquippedIds, BoardTheme: null }}
          onEquip={mockOnEquip}
        />
      );
      
      const equipButtons = screen.getAllByRole('button', { name: /equip/i });
      const overlayButton = equipButtons.find(btn => btn.className.includes('opacity-0'));
      
      if (overlayButton) {
        overlayButton.focus();
        expect(document.activeElement).toBe(overlayButton);
      }
    });
  });

  describe('loading state', () => {
    it('should show loading skeleton when isLoading is true', () => {
      render(
        <CosmeticSelector
          cosmetics={[]}
          equippedIds={mockEquippedIds}
          onEquip={mockOnEquip}
          isLoading={true}
        />
      );
      
      const skeletons = document.querySelectorAll('.animate-pulse');
      expect(skeletons.length).toBeGreaterThan(0);
    });
  });

  describe('empty state', () => {
    it('should show empty message when no cosmetics available', () => {
      render(
        <CosmeticSelector
          cosmetics={[]}
          equippedIds={mockEquippedIds}
          onEquip={mockOnEquip}
        />
      );
      
      expect(screen.getByText('No cosmetics available')).toBeInTheDocument();
    });
  });
});
