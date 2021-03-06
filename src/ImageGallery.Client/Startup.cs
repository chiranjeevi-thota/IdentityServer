﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;
using System;
using System.IdentityModel.Tokens.Jwt;
using IdentityModel;
using ImageGallery.Client.HttpHandlers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace ImageGallery.Client
{
	public class Startup
	{
		public IConfiguration Configuration { get; }

		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
			JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
		}

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddControllersWithViews()
				 .AddJsonOptions(opts => opts.JsonSerializerOptions.PropertyNamingPolicy = null);

			services.AddAuthorization(authorizationOptions =>
			{
				authorizationOptions.AddPolicy(
				"CanOrderFrame",
				policyBuilder =>
				{
					policyBuilder.RequireAuthenticatedUser();
					policyBuilder.RequireClaim("country", "be"); // policyBuilder.RequireClaim("country", "be", "nl", "xyz", etc...);
					policyBuilder.RequireClaim("subscriptionlevel", "PayingUser");

					// You can also add required roles for policy builders
					// policyBuilder.RequireRole()
				});
			});

			services.AddHttpContextAccessor();

			services.AddTransient<BearerTokenHandler>();

			// create an HttpClient used for accessing the API
			services.AddHttpClient("APIClient", client =>
			{
				client.BaseAddress = new Uri("https://localhost:44366/");
				client.DefaultRequestHeaders.Clear();
				client.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
			}).AddHttpMessageHandler<BearerTokenHandler>();

			// create an HttpClient used for accessing the IDP
			services.AddHttpClient("IDPClient", client =>
			{
				client.BaseAddress = new Uri("https://localhost:44352/");
				client.DefaultRequestHeaders.Clear();
				client.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
			});

			// Add authentication middle ware
			services.AddAuthentication(options =>
				{
					options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
					options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
				})
				.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
				{
					options.AccessDeniedPath = "/Authorization/AccessDenied";
				})
				.AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
				{
					options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
					options.Authority = "https://localhost:44352/";
					options.ClientId = "imagegalleryclient";
					options.ResponseType = "code";
					options.UsePkce = true;  // This should be enabled default
					
					options.Scope.Add("openid"); // Requested by default by this middle ware
					options.Scope.Add("profile"); // Requested by default by this middle ware
					options.Scope.Add("address"); // Address Claim is not returned by default,
												// these claims has to be explicitly asked to return from userinfo endpoint
					options.Scope.Add("roles");
					options.Scope.Add("imagegalleryapi");

					options.Scope.Add("country");
					options.Scope.Add("subscriptionlevel"); 

					options.Scope.Add("offline_access");// Here the middle ware stores refresh token used next time to get the access token

					// options.ClaimActions.Remove("nbf"); // This method tells the middle ware that "remove/exclude" this claim from filtering out
					options.ClaimActions.DeleteClaim("sid");
					options.ClaimActions.DeleteClaim("idp");
					options.ClaimActions.DeleteClaim("s_hash");
					options.ClaimActions.DeleteClaim("auth_time");

					options.ClaimActions.MapUniqueJsonKey("role", "role");
					options.ClaimActions.MapUniqueJsonKey("country", "country");
					options.ClaimActions.MapUniqueJsonKey("subscriptionlevel", "subscriptionlevel");

					// Just by enabling this, middle ware extracts the claim from access token
					// Note: For address claim we didn't wanted the middle ware by default to get-it. As we need update information from UserInfoEndpoint
					// we used the IDP client for that.

					options.SaveTokens = true;
					options.ClientSecret = "secret";
					options.GetClaimsFromUserInfoEndpoint = true;

					options.TokenValidationParameters = new TokenValidationParameters
					{
						NameClaimType = JwtClaimTypes.GivenName,
						RoleClaimType = JwtClaimTypes.Role
					};
				});

		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			app.UseStaticFiles();

			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/Shared/Error");
				// The default HSTS value is 30 days. You may want to change this for
				// production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}
			app.UseHttpsRedirection();
			app.UseStaticFiles();

			app.UseRouting();

			app.UseAuthentication();
			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllerRoute(
					name: "default",
					pattern: "{controller=Gallery}/{action=Index}/{id?}");
			});
		}
	}
}
