using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace ImageGallery.Client.HttpHandlers
{
    public class BearerTokenHandler : DelegatingHandler
    {
	    private readonly IHttpContextAccessor _httpContextAccessor;
	    private readonly IHttpClientFactory _httpClientFactory;

	    public BearerTokenHandler(IHttpContextAccessor httpContextAccessor, IHttpClientFactory httpClientFactory)
	    {
		    _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
		    _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
	    }

	    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	    {
			var accessToken = await GetAccessTokenAsync();
			
			if (!string.IsNullOrWhiteSpace(accessToken))
			{
				request.SetBearerToken(accessToken);
			}

			return await base.SendAsync(request, cancellationToken);
	    }

	    public async Task<string> GetAccessTokenAsync()
	    {
		    var expiresAt = await _httpContextAccessor.HttpContext.GetTokenAsync("expires_at"); // only applicable for access tokens
		    var expiresAtDateTimeOffset = DateTimeOffset.Parse(expiresAt, CultureInfo.InvariantCulture);
		    if (expiresAtDateTimeOffset.AddSeconds(-60).ToUniversalTime() > DateTimeOffset.Now)
		    {
			    // no need to refresh, return the access token
			    return await _httpContextAccessor.HttpContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken);
		    }

		    var idpClient = _httpClientFactory.CreateClient("IDPClient");

		    var discoveryResponse = await idpClient.GetDiscoveryDocumentAsync();

		    var refreshToken =
			    await _httpContextAccessor.HttpContext.GetTokenAsync(OpenIdConnectParameterNames.RefreshToken);

		    var response = await idpClient.RequestRefreshTokenAsync(
			    new RefreshTokenRequest
			    {
				    Address = discoveryResponse.TokenEndpoint,
				    ClientId = "imagegalleryclient",
				    ClientSecret = "secret",
				    RefreshToken = refreshToken
			    });

		    // store the tokens
		    var updatedTokens = new List<AuthenticationToken>();
		    updatedTokens.Add(new AuthenticationToken
		    {
			    Name = OpenIdConnectParameterNames.IdToken,
			    Value = response.IdentityToken
		    });

		    updatedTokens.Add(new AuthenticationToken
		    {
			    Name = OpenIdConnectParameterNames.AccessToken,
			    Value = response.AccessToken
		    });

		    updatedTokens.Add(new AuthenticationToken
		    {
			    Name = OpenIdConnectParameterNames.RefreshToken,
			    Value = response.RefreshToken
		    });

			updatedTokens.Add(new AuthenticationToken
			{
				Name = "expires_at",
				Value = (DateTime.UtcNow + TimeSpan.FromSeconds(response.ExpiresIn)).ToString("o", CultureInfo.InvariantCulture)
				// o means round trip format
			});

			// get authenticate result, containing the current principal & properties
			var currentAuthenticateResult = await _httpContextAccessor
												.HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
			// store the updates tokens
			currentAuthenticateResult.Properties.StoreTokens(updatedTokens);

			await _httpContextAccessor.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
				currentAuthenticateResult.Principal,
				currentAuthenticateResult.Properties);

			return response.AccessToken;
	    }
    }
}
