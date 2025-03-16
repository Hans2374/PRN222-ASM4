using PaymentCVSTS.BlazorApp.Components;
using PaymentCVSTS.Services.Implements;
using PaymentCVSTS.Services.Interfaces;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication.Cookies;
using PaymentCVSTS.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Register services
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IUserAccountService, UserAccountService>();
builder.Services.AddHttpContextAccessor();

// Add authentication
builder.Services.AddAuthentication()
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.LoginPath = new PathString("/Account/Login");
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    });

builder.Services.AddAuthorization();

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

// Configure CSRF token for login form
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/Account/Login") && context.Request.Method == "POST")
    {
        var antiforgery = context.RequestServices.GetRequiredService<IAntiforgery>();
        var tokens = antiforgery.GetAndStoreTokens(context);
        context.Request.Headers["RequestVerificationToken"] = tokens.RequestToken;
    }

    await next();
});

app.UseMiddleware<AuthenticationMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();