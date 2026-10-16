using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RaceDay.API.Services;

namespace RaceDay.API.Filters;

public class OrganiserOnlyAttribute : AuthenticatedAttribute
{
    public new void OnAuthorization(AuthorizationFilterContext context)
    {
        base.OnAuthorization(context);
        if (context.Result != null)
        {
            return;
        }

        var user = (CurrentUser)context.HttpContext.Items["CurrentUser"]!;
        if (user.Role != UserRole.Organiser)
        {
            context.Result = new ObjectResult(new { message = "Organiser access required." })
            {
                StatusCode = 403
            };
        }
    }
}