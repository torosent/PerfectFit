/**
 * Password Strength Indicator Component Tests
 *
 * Tests for the password strength indicator component that shows
 * visual feedback for password requirements.
 */

import React from 'react';
import { render, screen } from '@testing-library/react';
import PasswordStrengthIndicator from '@/components/auth/PasswordStrengthIndicator';

describe('PasswordStrengthIndicator', () => {
  describe('shows all requirements unchecked for empty password', () => {
    it('should show all requirements as not met for empty password', () => {
      render(<PasswordStrengthIndicator password="" />);

      // All requirements should be visible
      expect(screen.getByText(/8\+ characters/i)).toBeInTheDocument();
      expect(screen.getByText(/uppercase letter/i)).toBeInTheDocument();
      expect(screen.getByText(/lowercase letter/i)).toBeInTheDocument();
      expect(screen.getByText(/number/i)).toBeInTheDocument();

      // None should be marked as met (no checkmarks)
      const checkmarks = screen.queryAllByTestId('requirement-met');
      expect(checkmarks).toHaveLength(0);
    });
  });

  describe('checks length requirement when 8+ chars', () => {
    it('should mark length requirement as met when password has 8+ characters', () => {
      render(<PasswordStrengthIndicator password="abcdefgh" />);

      const lengthRequirement = screen.getByTestId('requirement-length');
      expect(lengthRequirement).toHaveAttribute('data-met', 'true');
    });

    it('should not mark length requirement as met when password has less than 8 characters', () => {
      render(<PasswordStrengthIndicator password="abcdefg" />);

      const lengthRequirement = screen.getByTestId('requirement-length');
      expect(lengthRequirement).toHaveAttribute('data-met', 'false');
    });
  });

  describe('checks uppercase requirement when has uppercase', () => {
    it('should mark uppercase requirement as met when password has uppercase letter', () => {
      render(<PasswordStrengthIndicator password="Abcdefgh" />);

      const uppercaseRequirement = screen.getByTestId('requirement-uppercase');
      expect(uppercaseRequirement).toHaveAttribute('data-met', 'true');
    });

    it('should not mark uppercase requirement as met when password has no uppercase', () => {
      render(<PasswordStrengthIndicator password="abcdefgh" />);

      const uppercaseRequirement = screen.getByTestId('requirement-uppercase');
      expect(uppercaseRequirement).toHaveAttribute('data-met', 'false');
    });
  });

  describe('checks lowercase requirement when has lowercase', () => {
    it('should mark lowercase requirement as met when password has lowercase letter', () => {
      render(<PasswordStrengthIndicator password="ABCDEFGh" />);

      const lowercaseRequirement = screen.getByTestId('requirement-lowercase');
      expect(lowercaseRequirement).toHaveAttribute('data-met', 'true');
    });

    it('should not mark lowercase requirement as met when password has no lowercase', () => {
      render(<PasswordStrengthIndicator password="ABCDEFGH" />);

      const lowercaseRequirement = screen.getByTestId('requirement-lowercase');
      expect(lowercaseRequirement).toHaveAttribute('data-met', 'false');
    });
  });

  describe('checks number requirement when has number', () => {
    it('should mark number requirement as met when password has a number', () => {
      render(<PasswordStrengthIndicator password="abcdefg1" />);

      const numberRequirement = screen.getByTestId('requirement-number');
      expect(numberRequirement).toHaveAttribute('data-met', 'true');
    });

    it('should not mark number requirement as met when password has no number', () => {
      render(<PasswordStrengthIndicator password="abcdefgh" />);

      const numberRequirement = screen.getByTestId('requirement-number');
      expect(numberRequirement).toHaveAttribute('data-met', 'false');
    });
  });

  describe('shows green when all requirements met', () => {
    it('should show green/success color when all requirements are met', () => {
      render(<PasswordStrengthIndicator password="Abcdefg1" />);

      const indicator = screen.getByTestId('password-strength-indicator');
      expect(indicator).toHaveAttribute('data-strength', 'strong');
    });

    it('should show yellow/warning color when some requirements are met', () => {
      render(<PasswordStrengthIndicator password="Abcdefgh" />);

      const indicator = screen.getByTestId('password-strength-indicator');
      expect(indicator).toHaveAttribute('data-strength', 'medium');
    });

    it('should show red/weak color when few requirements are met', () => {
      render(<PasswordStrengthIndicator password="abc" />);

      const indicator = screen.getByTestId('password-strength-indicator');
      expect(indicator).toHaveAttribute('data-strength', 'weak');
    });
  });
});
