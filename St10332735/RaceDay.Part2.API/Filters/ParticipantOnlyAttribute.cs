using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RaceDay.API.Services;

namespace RaceDay.API.Filters;

public class ParticipantOnlyAttribute : AuthenticatedAttribute
{
    public new void OnAuthorization(AuthorizationFilterContext context)
    {
        base.OnAuthorization(context);
        if (context.Result != null)
        {
            return;
        }

        var user = (CurrentUser)context.HttpContext.Items["CurrentUser"]!;
        if (user.Role != UserRole.Participant)
        {
            context.Result = new ObjectResult(new { message = "Participant access required." })
            {
                StatusCode = 403
            };
        }
    }
}