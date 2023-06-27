using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Client.AspNetCore;
using OpenIddict.Sandbox.AspNetCore.Client.Models;
using static OpenIddict.Abstractions.OpenIddictConstants;

var builder = WebApplication.CreateSlimBuilder(args);
builder.WebHost.UseKestrelHttpsConfiguration();

OpenIddict.Client.SystemNetHttp.Todo.Options = SourceGenerationContext.Default.Options;

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseInMemoryDatabase("db");
    options.UseOpenIddict();
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
   .AddCookie(options =>
   {
       options.LoginPath = "/login";
       options.LogoutPath = "/logout";
       options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
       options.SlidingExpiration = true;
   });

builder.Services.AddHttpClient();

builder.Services.AddOpenIddict()
       .AddCore(options =>
       {
           options.UseEntityFrameworkCore()
                  .UseDbContext<ApplicationDbContext>();
       })
       .AddClient(options =>
       {
           options.AllowAuthorizationCodeFlow();

           options.AddDevelopmentEncryptionCertificate()
                  .AddDevelopmentSigningCertificate();

           options.UseAspNetCore()
                  .EnableStatusCodePagesIntegration()
                  .EnableRedirectionEndpointPassthrough()
                  .EnablePostLogoutRedirectionEndpointPassthrough();

           options.UseSystemNetHttp().SetProductInformation(typeof(Program).Assembly);

           options.UseWebProviders()
                  .AddGoogle(options =>
                  {
                      options.SetClientId("471859164193-qfmcdbvj91ji33h8olnmf120r1dgogfq.apps.googleusercontent.com")
                             .SetClientSecret("lRMO9Gf0odNYUmPpkYYRtHqe")
                             .SetRedirectUri("callback/login/google").AddScopes(new[] { Scopes.Email, Scopes.Profile });
                  });
       });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/test", () => "aaa").RequireAuthorization();

app.MapGet("/login", () =>
{
    var properties = new AuthenticationProperties(new Dictionary<string, string?>
    {
        // Note: when only one client is registered in the client options,
        // specifying the issuer URI or the provider name is not required.
        [OpenIddictClientAspNetCoreConstants.Properties.ProviderName] = null
    })
    {
        // Only allow local return URLs to prevent open redirect attacks.
        RedirectUri = "/"
    };

    // Ask the OpenIddict client middleware to redirect the user agent to the identity provider.
    return Results.Challenge(properties, new[] { OpenIddictClientAspNetCoreDefaults.AuthenticationScheme });

});

app.MapGet("/callback/login/google", async (HttpContext context) =>
{
    var result = await context.AuthenticateAsync(OpenIddictClientAspNetCoreDefaults.AuthenticationScheme);

    await Console.Out.WriteLineAsync($"Claim count: {result.Principal?.Claims.ToList().Count}");

    return Results.Ok();
});

app.Run();
