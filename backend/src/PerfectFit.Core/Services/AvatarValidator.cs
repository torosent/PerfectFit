namespace PerfectFit.Core.Services;

/// <summary>
/// Validates avatar emojis against a curated list.
/// </summary>
public static class AvatarValidator
{
    /// <summary>
    /// Curated list of valid avatar emojis (matches frontend emojis.ts)
    /// </summary>
    public static readonly HashSet<string> ValidAvatars = new(StringComparer.Ordinal)
    {
        // Smileys
        "😀", "😃", "😄", "😁", "😆", "😅", "🤣", "😂", "🙂", "😉", "😊", "😇",
        // Cool/Fun
        "😎", "🤩", "🥳", "😈", "👻", "🤖", "👽", "🎃",
        // Animals
        "🐶", "🐱", "🐭", "🐹", "🐰", "🦊", "🐻", "🐼", "🐨", "🐯", "🦁", "🐮",
        "🐷", "🐸", "🐵", "🐔", "🐧", "🐦", "🐤", "🦄", "🐝", "🦋", "🐢", "🐙",
        // Sports/Activities
        "⚽", "🏀", "🏈", "⚾", "🎾", "🏐", "🎱", "🏓", "🎯", "🎮", "🕹️", "🎲",
        // Food
        "🍕", "🍔", "🌮", "🍣", "🍩", "🍪", "🎂", "🍦", "🍫", "☕",
        // Nature
        "🌸", "🌺", "🌻", "🌹", "🍀", "🌈", "⭐", "🌙", "☀️", "🔥", "💧", "❄️",
        // Objects
        "🎸", "🎹", "🎤", "🎧", "📚", "💻", "🚀", "✈️", "🏠", "💎", "🔮", "🎭"
    };

    /// <summary>
    /// Validates if the provided avatar is valid.
    /// </summary>
    /// <param name="avatar">The avatar string to validate.</param>
    /// <returns>True if avatar is null, empty, or in the valid list; otherwise false.</returns>
    public static bool IsValidAvatar(string? avatar)
    {
        if (string.IsNullOrEmpty(avatar))
        {
            return true;
        }

        return ValidAvatars.Contains(avatar);
    }
}
