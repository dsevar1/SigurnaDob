using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace SigurnaDob.App.Auth;

/// <summary>
/// Čita/sprema JWT u ProtectedLocalStorage i drži ga sinkroniziranog s Authorization headerom
/// scoped HttpClienta (SigurnaDobApi) korištenog za pozive prema Api-ju.
///
/// NAPOMENA: Authorization header se namjerno postavlja OVDJE, direktno na scoped HttpClient,
/// a NE kroz DelegatingHandler (AddHttpMessageHandler). IHttpClientFactory pool-a handler
/// instance po imenu klijenta (HandlerLifetime, default 2 min) NEOVISNO o circuitu koji je
/// zatražio CreateClient - handler bi zarobio provider instancu PRVOG circuita koji ga je
/// izgradio, pa bi svi kasniji korisnici u istom vremenskom prozoru dijelili njegov (tuđi) token.
/// HttpClient sam po sebi je ispravno scoped (jedna instanca po circuitu preko AddScoped), pa
/// je postavljanje headera izravno na njega jedini pouzdan način da svaki korisnik šalje SVOJ token.
/// </summary>
public class JwtAuthenticationStateProvider : AuthenticationStateProvider
{
    private const string StorageKey = "sigurnadob_auth_token";

    private readonly ProtectedLocalStorage _localStorage;
    private readonly HttpClient _httpClient;
    private readonly ClaimsPrincipal _anonymous = new(new ClaimsIdentity());
    private string? _cachedToken;
    private bool _cacheLoaded;

    public JwtAuthenticationStateProvider(ProtectedLocalStorage localStorage, HttpClient httpClient)
    {
        _localStorage = localStorage;
        _httpClient = httpClient;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await GetTokenAsync();
        if (string.IsNullOrWhiteSpace(token))
        {
            return new AuthenticationState(_anonymous);
        }

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        if (jwt.ValidTo < DateTime.UtcNow)
        {
            await MarkUserAsLoggedOutAsync();
            return new AuthenticationState(_anonymous);
        }

        var identity = new ClaimsIdentity(jwt.Claims, authenticationType: "jwt");
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    public async Task<string?> GetTokenAsync()
    {
        if (!_cacheLoaded)
        {
            var result = await _localStorage.GetAsync<string>(StorageKey);
            _cachedToken = result.Success ? result.Value : null;
            _cacheLoaded = true;
            ApplyAuthorizationHeader();
        }

        return _cachedToken;
    }

    public async Task MarkUserAsAuthenticatedAsync(string token)
    {
        _cachedToken = token;
        _cacheLoaded = true;
        ApplyAuthorizationHeader();
        await _localStorage.SetAsync(StorageKey, token);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        var identity = new ClaimsIdentity(jwt.Claims, authenticationType: "jwt");
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity))));
    }

    public async Task MarkUserAsLoggedOutAsync()
    {
        _cachedToken = null;
        _cacheLoaded = true;
        ApplyAuthorizationHeader();
        await _localStorage.DeleteAsync(StorageKey);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));
    }

    private void ApplyAuthorizationHeader()
    {
        _httpClient.DefaultRequestHeaders.Authorization = string.IsNullOrWhiteSpace(_cachedToken)
            ? null
            : new AuthenticationHeaderValue("Bearer", _cachedToken);
    }
}
