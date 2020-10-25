// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Models;
using System.Collections.Generic;
using IdentityModel;
using IdentityServer4;

namespace Chiru.IDP
{
	public static class Config
	{
		// Identity resources maps to scopes that give identity related information
		public static IEnumerable<IdentityResource> IdentityResources =>
			new IdentityResource[]
			{
				new IdentityResources.OpenId(),
				new IdentityResources.Profile(),
				new IdentityResources.Address(),
				new IdentityResource(
					"roles", // scope name
					"Your role(s)", // display name
					new List<string>{"role"}),

				new IdentityResource(
					"country",
					"The country you are living in",
					new List<string>() {"country"}),

				new IdentityResource(
					"subscriptionlevel",
					"Your subscription level",
					new List<string>() {"subscriptionlevel"})
			};

		// API scopes give API related information
		public static IEnumerable<ApiScope> ApiScopes =>
			new ApiScope[]
			{
				new ApiScope("imagegalleryapi", "Image Gallery API", new List<string> {"role"})
			};

		// https://stackoverflow.com/questions/62930426/missing-aud-claim-in-access-token
		public static IEnumerable<ApiResource> ApiResources =>
			new ApiResource[]
			{
				new ApiResource("imagegalleryapi")
				{
					UserClaims =
					{
						JwtClaimTypes.Audience
					},
					Scopes = new List<string>()
					{
						"imagegalleryapi"
					},
					ApiSecrets = {new Secret("apisecret".Sha256())}
				}
			};


		public static IEnumerable<Client> Clients =>
			new Client[]
			{
				new Client
				{
					AccessTokenType = AccessTokenType.Reference,

					AccessTokenLifetime = 120,
					AllowOfflineAccess = true, // Supports refresh tokens
					// AbsoluteRefreshTokenLifetime = 30days
					// RefreshTokenExpiration = TokenExpiration.Sliding,
					UpdateAccessTokenClaimsOnRefresh = true,
					ClientName = "Image Gallery",
					ClientId = "imagegalleryclient",
					AllowedGrantTypes = GrantTypes.Code,
					RequirePkce = true, // This should be enabled default
					RequireConsent = true,
					RedirectUris = new List<string>()
					{
						"https://localhost:44389/signin-oidc"
					},
					PostLogoutRedirectUris = new List<string>()
						{
							"https://localhost:44389/signout-callback-oidc"
						},
					AllowedScopes =
					{
						IdentityServerConstants.StandardScopes.OpenId,
						IdentityServerConstants.StandardScopes.Profile,
						IdentityServerConstants.StandardScopes.Address,
						"roles",
						"imagegalleryapi",
						"country",
						"subscriptionlevel"
					},
					ClientSecrets =
					{
						new Secret("secret".Sha256())
					}
				}
			};
	}
}