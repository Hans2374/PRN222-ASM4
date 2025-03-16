using PaymentCVSTS.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Antiforgery;

namespace PaymentCVSTS.BlazorApp.Components
{
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IAntiforgery _antiforgery;

        public AuthenticationMiddleware(RequestDelegate next, IAntiforgery antiforgery)
        {
            _next = next;
            _antiforgery = antiforgery;
        }

        public async Task InvokeAsync(HttpContext context, IUserAccountService userAccountService)
        {
            if (context.Request.Path.StartsWithSegments("/Account/Login") && context.Request.Method == "POST")
            {
                try
                {
                    // Validate antiforgery token
                    await _antiforgery.ValidateRequestAsync(context);

                    var form = await context.Request.ReadFormAsync();
                    var username = form["username"];
                    var password = form["password"];

                    var userAccount = await userAccountService.Authenticate(username, password);
                    if (userAccount != null)
                    {
                        // Map RoleId to actual role names
                        string roleName = userAccount.RoleId switch
                        {
                            1 => "Admin",
                            2 => "Doctor",
                            _ => "User"
                        };

                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, userAccount.UserName),
                            new Claim(ClaimTypes.Role, roleName),
                            // Also store the numeric RoleId for reference if needed
                            new Claim("RoleId", userAccount.RoleId.ToString())
                        };

                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                        await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal,
                            new AuthenticationProperties { IsPersistent = true });

                        context.Response.Redirect("/Payment/PaymentList");
                        return;
                    }
                    else
                    {
                        context.Response.Redirect("/Account/Login?error=InvalidCredentials");
                        return;
                    }
                }
                catch (AntiforgeryValidationException)
                {
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync("Invalid antiforgery token.");
                    return;
                }
            }

            await _next(context);
        }
    }
}