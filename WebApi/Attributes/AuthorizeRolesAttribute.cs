using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;

namespace ComicBackend.WebApi.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class AuthorizeRolesAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _roles;

        public AuthorizeRolesAttribute(params string[] roles)
        {
            _roles = roles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // 1. Kiểm tra xem User đã đăng nhập chưa
            var user = context.HttpContext.Items["User"];
            if (user == null)
            {
                context.Result = new JsonResult(new { error = "Unauthorized. Please login first." }) 
                { StatusCode = StatusCodes.Status401Unauthorized };
                return;
            }

            // 2. Kiểm tra quyền (Role)
            var userRole = context.HttpContext.Items["UserRole"] as string;
            
            if (string.IsNullOrEmpty(userRole) || !_roles.Contains(userRole))
            {
                context.Result = new JsonResult(new { error = "Forbidden. You do not have permission to access this resource." }) 
                { StatusCode = StatusCodes.Status403Forbidden };
                return;
            }
        }
    }
}