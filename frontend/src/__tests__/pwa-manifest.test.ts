/**
 * PWA Manifest Tests
 * 
 * Tests to verify PWA manifest.json exists and contains required fields
 * for Progressive Web App functionality.
 */

import * as fs from 'fs';
import * as path from 'path';

describe('PWA Manifest', () => {
  const manifestPath = path.join(__dirname, '../..', 'public', 'manifest.json');
  let manifest: Record<string, unknown>;

  beforeAll(() => {
    // Read and parse manifest.json
    const manifestContent = fs.readFileSync(manifestPath, 'utf-8');
    manifest = JSON.parse(manifestContent);
  });

  describe('manifest.json file', () => {
    it('should exist in public/ directory', () => {
      expect(fs.existsSync(manifestPath)).toBe(true);
    });

    it('should be valid JSON', () => {
      const manifestContent = fs.readFileSync(manifestPath, 'utf-8');
      expect(() => JSON.parse(manifestContent)).not.toThrow();
    });
  });

  describe('required PWA fields', () => {
    it('should have a name field', () => {
      expect(manifest.name).toBeDefined();
      expect(typeof manifest.name).toBe('string');
      expect((manifest.name as string).length).toBeGreaterThan(0);
    });

    it('should have a short_name field', () => {
      expect(manifest.short_name).toBeDefined();
      expect(typeof manifest.short_name).toBe('string');
      expect((manifest.short_name as string).length).toBeGreaterThan(0);
    });

    it('should have a start_url field', () => {
      expect(manifest.start_url).toBeDefined();
      expect(typeof manifest.start_url).toBe('string');
    });

    it('should have display set to standalone', () => {
      expect(manifest.display).toBe('standalone');
    });

    it('should have icons array defined', () => {
      expect(manifest.icons).toBeDefined();
      expect(Array.isArray(manifest.icons)).toBe(true);
      expect((manifest.icons as unknown[]).length).toBeGreaterThan(0);
    });
  });

  describe('theme and colors', () => {
    it('should have theme_color defined', () => {
      expect(manifest.theme_color).toBeDefined();
      expect(typeof manifest.theme_color).toBe('string');
      // Should be a valid hex color
      expect(manifest.theme_color).toMatch(/^#[0-9A-Fa-f]{6}$/);
    });

    it('should have background_color defined', () => {
      expect(manifest.background_color).toBeDefined();
      expect(typeof manifest.background_color).toBe('string');
      // Should be a valid hex color
      expect(manifest.background_color).toMatch(/^#[0-9A-Fa-f]{6}$/);
    });

    it('should use teal theme color matching app theme', () => {
      expect(manifest.theme_color).toBe('#0d9488');
    });

    it('should use dark background color matching app background', () => {
      expect(manifest.background_color).toBe('#111827');
    });
  });

  describe('icons configuration', () => {
    let icons: Array<{ src: string; sizes: string; type: string; purpose?: string }>;

    beforeAll(() => {
      icons = manifest.icons as typeof icons;
    });

    it('should have at least one icon defined', () => {
      expect(icons.length).toBeGreaterThanOrEqual(1);
    });

    it('should have a 192x192 icon for Android', () => {
      const icon192 = icons.find(icon => icon.sizes === '192x192');
      expect(icon192).toBeDefined();
      expect(icon192?.src).toBeDefined();
      expect(icon192?.type).toBe('image/png');
    });

    it('should have a 512x512 icon for splash screens', () => {
      const icon512 = icons.find(icon => icon.sizes === '512x512');
      expect(icon512).toBeDefined();
      expect(icon512?.src).toBeDefined();
      expect(icon512?.type).toBe('image/png');
    });

    it('each icon should have required properties', () => {
      icons.forEach(icon => {
        expect(icon.src).toBeDefined();
        expect(typeof icon.src).toBe('string');
        expect(icon.sizes).toBeDefined();
        expect(typeof icon.sizes).toBe('string');
        expect(icon.type).toBeDefined();
        expect(typeof icon.type).toBe('string');
      });
    });
  });

  describe('optional but recommended fields', () => {
    it('should have a description', () => {
      expect(manifest.description).toBeDefined();
      expect(typeof manifest.description).toBe('string');
    });

    it('should have orientation set to portrait for game', () => {
      expect(manifest.orientation).toBe('portrait');
    });
  });
});

describe('PWA Icon Files', () => {
  const iconsDir = path.join(__dirname, '../..', 'public', 'icons');

  it('should have icons directory in public/', () => {
    expect(fs.existsSync(iconsDir)).toBe(true);
  });

  it('should have 192x192 icon file', () => {
    const iconPath = path.join(iconsDir, 'icon-192x192.png');
    expect(fs.existsSync(iconPath)).toBe(true);
  });

  it('should have 512x512 icon file', () => {
    const iconPath = path.join(iconsDir, 'icon-512x512.png');
    expect(fs.existsSync(iconPath)).toBe(true);
  });
});
