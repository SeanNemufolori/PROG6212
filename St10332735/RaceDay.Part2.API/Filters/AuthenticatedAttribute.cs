using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RaceDay.API.Services;

namespace RaceDay.API.Filters;

public class AuthenticatedAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        if (context.HttpContext.Items["CurrentUser"] is not CurrentUser)
        {
            context.Result = new UnauthorizedObjectResult(new { message = "Authentication required. Please log in." });
        }
    }
}