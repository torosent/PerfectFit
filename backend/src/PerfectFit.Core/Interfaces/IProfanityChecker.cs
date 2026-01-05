namespace PerfectFit.Core.Interfaces;

/// <summary>
/// Interface for profanity checking service.
/// </summary>
public interface IProfanityChecker
{
    /// <summary>
    /// Checks if the given text contains profanity.
    /// </summary>
    /// <param name="text">The text to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if profanity is detected, false if clean, null if check failed.</returns>
    Task<bool?> ContainsProfanityAsync(string text, CancellationToken cancellationToken = default);
}
