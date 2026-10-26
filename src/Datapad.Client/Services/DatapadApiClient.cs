using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Datapad.Contracts;

namespace Datapad.Client.Services;

internal sealed class DatapadApiClient(HttpClient httpClient) : IDatapadApi
{
    public async Task<IReadOnlyList<CharacterSummary>> GetCharactersAsync(CancellationToken cancellationToken) =>
        await GetAsync<List<CharacterSummary>>("api/characters", "the character list", cancellationToken);

    public Task<CharacterDetails> GetCharacterAsync(int id, CancellationToken cancellationToken) =>
        GetAsync<CharacterDetails>($"api/characters/{id}", "this character", cancellationToken);

    private async Task<T> GetAsync<T>(string path, string what, CancellationToken cancellationToken)
    {
        HttpResponseMessage response;
        try
        {
            response = await httpClient.GetAsync(new Uri(path, UriKind.Relative), cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            throw new DatapadApiException("Could not reach the Datapad server. Check your connection and try again.", ex);
        }

        using (response)
        {
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new DatapadApiException("That character could not be found.", isNotFound: true);
            }

            if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
            {
                throw new DatapadApiException("The Star Wars API is unavailable right now. Please try again shortly.");
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new DatapadApiException($"Something went wrong loading {what}.");
            }

            try
            {
                return await response.Content.ReadFromJsonAsync<T>(cancellationToken)
                    ?? throw new DatapadApiException($"The server returned no data for {what}.");
            }
            catch (JsonException ex)
            {
                throw new DatapadApiException($"The server returned data for {what} that could not be read.", ex);
            }
        }
    }
}
