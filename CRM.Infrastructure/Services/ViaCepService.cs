using CRM.Application.DTOs;
using CRM.Application.Services;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using System.Net.Http.Json;

namespace CRM.Infrastructure.Services;

public class ViaCepService : IZipCodeService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ViaCepService> _logger;
    private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;

    public ViaCepService(HttpClient httpClient, ILogger<ViaCepService> logger)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://viacep.com.br");
        _logger = logger;

        _retryPolicy = Policy
            .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .Or<HttpRequestException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    _logger.LogWarning(
                        "Retry {RetryCount} after {Delay}s due to: {Reason}",
                        retryCount,
                        timespan.TotalSeconds,
                        outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString());
                });
    }

    public async Task<ZipCodeInfoDto?> GetAddressByZipCodeAsync(string zipCode, CancellationToken cancellationToken = default)
    {
        try
        {
            var cleanZipCode = new string(zipCode.Where(char.IsDigit).ToArray());

            if (cleanZipCode.Length != 8)
            {
                _logger.LogWarning("Invalid zip code format: {ZipCode}", zipCode);
                return null;
            }

            _logger.LogInformation("Fetching address for zip code: {ZipCode}", cleanZipCode);

            var response = await _retryPolicy.ExecuteAsync(async () =>
                await _httpClient.GetAsync($"/ws/{cleanZipCode}/json/", cancellationToken));

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to fetch address for zip code {ZipCode}. Status: {StatusCode}",
                    cleanZipCode, response.StatusCode);
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<ZipCodeInfoDto>(cancellationToken: cancellationToken);

            if (result?.Erro == true)
            {
                _logger.LogWarning("Zip code not found: {ZipCode}", cleanZipCode);
                return result;
            }

            _logger.LogInformation("Successfully fetched address for zip code: {ZipCode}", cleanZipCode);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching address for zip code: {ZipCode}", zipCode);
            return null;
        }
    }
}
