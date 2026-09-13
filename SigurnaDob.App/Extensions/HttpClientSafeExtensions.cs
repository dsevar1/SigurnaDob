using MudBlazor;

namespace SigurnaDob.App.Extensions;

public static class HttpClientSafeExtensions
{
    private const string DefaultNetworkErrorMessage = "Ne mogu se spojiti s API-jem. Provjeri je li pokrenut.";

    /// <summary>
    /// Omata HTTP poziv u try/catch(HttpRequestException). U Blazor Serveru neuhvaćena iznimka u
    /// event handleru ruši cijeli SignalR circuit (korisnik vidi "An error has occurred" banner i
    /// mora reload-ati stranicu, gubeći stanje) umjesto da vidi Snackbar poruku. Vraća null ako je
    /// mrežni poziv pao (nakon što je Snackbar već prikazan) - pozivatelj samo provjeri null i
    /// prekine; postojeća provjera response.IsSuccessStatusCode/ShowErrorAsync za POSLOVNE greške
    /// (400 itd.) ide dalje nepromijenjena kad poziv uspije.
    /// </summary>
    public static async Task<HttpResponseMessage?> SafeSendAsync(
        this HttpClient http,
        ISnackbar snackbar,
        Func<Task<HttpResponseMessage>> request,
        string errorMessage = DefaultNetworkErrorMessage)
    {
        try
        {
            return await request();
        }
        catch (HttpRequestException)
        {
            snackbar.Add(errorMessage, Severity.Error);
            return null;
        }
    }
}
