using PaymentCVSTS.BlazorApp.Components;
using PaymentCVSTS.Services.Implements;
using PaymentCVSTS.Services.Interfaces;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using PaymentCVSTS.BlazorApp.Authentication;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Register services
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IUserAccountService, UserAccountService>();
builder.Services.AddHttpContextAccessor();

// Add authentication services
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.ReturnUrlParameter = "returnUrl";
        options.Cookie.Name = "PaymentCVSTS.Auth";
    });

// Add authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAuthentication", policy =>
        policy.RequireAuthenticatedUser());

    // Add role-based policies
    options.AddPolicy("RequireAdminRole", policy =>
        policy.RequireRole("1")); // RoleId 1 is Administrator

    options.AddPolicy("RequireAdminOrReceptionistRole", policy =>
        policy.RequireRole("1", "3")); // RoleId 1 is Administrator, 3 is Receptionist
});

// Add AuthenticationStateProvider
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

// Add authentication middleware
// Important: Order matters here! Authentication must come before Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Map the login handler to a different route to avoid conflicts
app.MapPost("/api/account/login", async (HttpContext context, IUserAccountService userAccountService) =>
{
    var form = context.Request.Form;
    string username = form["username"];
    string password = form["password"];
    string? returnUrl = form["returnUrl"];

    var user = await userAccountService.Authenticate(username, password);

    if (user != null)
    {
        // Create claims for the authenticated user
        var claims = new List<System.Security.Claims.Claim>
        {
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, user.UserName),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, user.RoleId.ToString()),
            new System.Security.Claims.Claim("FullName", user.FullName),
            new System.Security.Claims.Claim("UserId", user.UserAccountId.ToString())
        };

        var claimsIdentity = new System.Security.Claims.ClaimsIdentity(
            claims, CookieAuthenticationDefaults.AuthenticationScheme);

        var authProperties = new Microsoft.AspNetCore.Authentication.AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
        };

        await context.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new System.Security.Claims.ClaimsPrincipal(claimsIdentity),
            authProperties);

        // Redirect to return URL or default page
        if (!string.IsNullOrEmpty(returnUrl) && Uri.IsWellFormedUriString(returnUrl, UriKind.Relative))
        {
            context.Response.Redirect(returnUrl);
        }
        else
        {
            context.Response.Redirect("/Payment/PaymentList");
        }
        return;
    }

    // If authentication failed
    context.Response.Redirect("/Account/Login?error=InvalidCredentials");
});

// Map logout endpoint
app.MapGet("/Account/Logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    context.Response.Redirect("/Account/Login?fresh=true");
});

app.Run();