/**
 * Curated list of ~80 emojis suitable for profile avatars
 * These are universally recognizable and render well across platforms
 */
export const AVATAR_EMOJIS = [
  // Smileys
  '😀', '😃', '😄', '😁', '😆', '😅', '🤣', '😂', '🙂', '😉', '😊', '😇',
  // Cool/Fun
  '😎', '🤩', '🥳', '😈', '👻', '🤖', '👽', '🎃',
  // Animals
  '🐶', '🐱', '🐭', '🐹', '🐰', '🦊', '🐻', '🐼', '🐨', '🐯', '🦁', '🐮',
  '🐷', '🐸', '🐵', '🐔', '🐧', '🐦', '🐤', '🦄', '🐝', '🦋', '🐢', '🐙',
  // Sports/Activities
  '⚽', '🏀', '🏈', '⚾', '🎾', '🏐', '🎱', '🏓', '🎯', '🎮', '🕹️', '🎲',
  // Food
  '🍕', '🍔', '🌮', '🍣', '🍩', '🍪', '🎂', '🍦', '🍫', '☕',
  // Nature
  '🌸', '🌺', '🌻', '🌹', '🍀', '🌈', '⭐', '🌙', '☀️', '🔥', '💧', '❄️',
  // Objects
  '🎸', '🎹', '🎤', '🎧', '📚', '💻', '🚀', '✈️', '🏠', '💎', '🔮', '🎭',
] as const;

/**
 * Type representing any valid avatar emoji from the curated list
 */
export type AvatarEmoji = typeof AVATAR_EMOJIS[number];

/**
 * Check if a string is a valid avatar emoji
 * @param emoji - The string to check
 * @returns True if the emoji is in the curated list
 */
export function isValidAvatarEmoji(emoji: string): emoji is AvatarEmoji {
  return (AVATAR_EMOJIS as readonly string[]).includes(emoji);
}
