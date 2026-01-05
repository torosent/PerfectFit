/**
 * EmojiPicker Component Tests
 * 
 * Tests for the reusable emoji picker component that displays
 * a grid of avatar emojis for selection.
 */

import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import { EmojiPicker } from '@/components/profile/EmojiPicker';
import { AVATAR_EMOJIS } from '@/lib/emojis';

describe('EmojiPicker', () => {
  describe('renders all emojis', () => {
    it('should render all emojis from AVATAR_EMOJIS', () => {
      render(<EmojiPicker selected={null} onSelect={jest.fn()} />);

      AVATAR_EMOJIS.forEach((emoji) => {
        expect(screen.getByRole('button', { name: emoji })).toBeInTheDocument();
      });
    });

    it('should render correct number of emoji buttons', () => {
      render(<EmojiPicker selected={null} onSelect={jest.fn()} />);

      const buttons = screen.getAllByRole('button');
      expect(buttons).toHaveLength(AVATAR_EMOJIS.length);
    });
  });

  describe('highlights selected emoji', () => {
    it('should highlight the currently selected emoji', () => {
      const selectedEmoji = '😎';
      render(<EmojiPicker selected={selectedEmoji} onSelect={jest.fn()} />);

      const selectedButton = screen.getByRole('button', { name: selectedEmoji });
      expect(selectedButton).toHaveAttribute('aria-pressed', 'true');
    });

    it('should not highlight non-selected emojis', () => {
      const selectedEmoji = '😎';
      render(<EmojiPicker selected={selectedEmoji} onSelect={jest.fn()} />);

      const nonSelectedEmoji = '🐶';
      const nonSelectedButton = screen.getByRole('button', { name: nonSelectedEmoji });
      expect(nonSelectedButton).toHaveAttribute('aria-pressed', 'false');
    });

    it('should not highlight any emoji when selected is null', () => {
      render(<EmojiPicker selected={null} onSelect={jest.fn()} />);

      const buttons = screen.getAllByRole('button');
      buttons.forEach((button) => {
        expect(button).toHaveAttribute('aria-pressed', 'false');
      });
    });
  });

  describe('calls onSelect when emoji clicked', () => {
    it('should call onSelect with the clicked emoji', () => {
      const onSelect = jest.fn();
      render(<EmojiPicker selected={null} onSelect={onSelect} />);

      const emojiToClick = '🚀';
      const button = screen.getByRole('button', { name: emojiToClick });
      fireEvent.click(button);

      expect(onSelect).toHaveBeenCalledTimes(1);
      expect(onSelect).toHaveBeenCalledWith(emojiToClick);
    });

    it('should call onSelect when clicking a different emoji than selected', () => {
      const onSelect = jest.fn();
      render(<EmojiPicker selected="😎" onSelect={onSelect} />);

      const emojiToClick = '🐶';
      const button = screen.getByRole('button', { name: emojiToClick });
      fireEvent.click(button);

      expect(onSelect).toHaveBeenCalledWith(emojiToClick);
    });

    it('should call onSelect when clicking the already selected emoji', () => {
      const onSelect = jest.fn();
      const selectedEmoji = '😎';
      render(<EmojiPicker selected={selectedEmoji} onSelect={onSelect} />);

      const button = screen.getByRole('button', { name: selectedEmoji });
      fireEvent.click(button);

      expect(onSelect).toHaveBeenCalledWith(selectedEmoji);
    });
  });

  describe('accessibility', () => {
    it('should have aria-label on emoji buttons', () => {
      render(<EmojiPicker selected={null} onSelect={jest.fn()} />);

      const button = screen.getByRole('button', { name: '😎' });
      expect(button).toHaveAttribute('aria-label', '😎');
    });

    it('should have proper role for the container', () => {
      render(<EmojiPicker selected={null} onSelect={jest.fn()} />);

      const grid = screen.getByRole('group');
      expect(grid).toBeInTheDocument();
    });
  });
});
