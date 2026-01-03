using System.Net.Http.Json;
using System.Web;
using PerfectFit.Core.Interfaces;

namespace PerfectFit.Infrastructure.Services;

/// <summary>
/// Profanity checker implementation using the PurgoMalum API.
/// https://www.purgomalum.com/
/// </summary>
public class PurgoMalumProfanityChecker : IProfanityChecker
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://www.purgomalum.com/service/containsprofanity";

    public PurgoMalumProfanityChecker(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Checks if the given text contains profanity using the PurgoMalum API.
    /// </summary>
    /// <param name="text">The text to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if profanity is detected, false if clean, null if the API call failed or returned unexpected response.</returns>
    public async Task<bool?> ContainsProfanityAsync(string text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
            return false;

        try
        {
            var encodedText = HttpUtility.UrlEncode(text);
            var url = $"{BaseUrl}?text={encodedText}";

            var response = await _httpClient.GetStringAsync(url, cancellationToken);

            // PurgoMalum returns "true" or "false" as plain text
            return response.Trim().ToLowerInvariant() switch
            {
                "true" => true,
                "false" => false,
                _ => null // Unexpected response (error message, etc.)
            };
        }
        catch (HttpRequestException)
        {
            // API call failed - return null to indicate check couldn't be performed
            return null;
        }
        catch (TaskCanceledException)
        {
            // Timeout or cancellation - return null
            return null;
        }
    }
}
