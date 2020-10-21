// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Models;
using System.Collections.Generic;
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
				new IdentityResources.Profile()
			};

		// API scopes give API related information
		public static IEnumerable<ApiScope> ApiScopes =>
			new ApiScope[]
			{ };

		public static IEnumerable<Client> Clients =>
			new Client[]
			{
				new Client
				{
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
						IdentityServerConstants.StandardScopes.Profile
					},
					ClientSecrets =
					{
						new Secret("secret".Sha256())
					}
				}
			};
	}
}