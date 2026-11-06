using Microsoft.AspNetCore.Mvc;

namespace LampFactory.API.Authorization
{
    public class HasPermissionAttribute : TypeFilterAttribute
    {
        public HasPermissionAttribute(string resource, string action)
            : base(typeof(PermissionAuthorizationFilter))
        {
            Arguments = new object[] { resource, action };
        }
    }

}
