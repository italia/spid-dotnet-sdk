using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Italia.Spid.AspNetCore.WebApp.Extensions
{
    public static class CookiesExtensions
    {
        public static string GetCookie(this Controller controller, string key)
        {
            return controller.Request.Cookies[key];
        }

        public static void SetCookie(this Controller controller, string key, string value, int? expireTime)
        {
            CookieOptions option = new CookieOptions();
            if (expireTime.HasValue)
            {
                option.Expires = DateTime.Now.AddMinutes(expireTime.Value);
            }
            else
            {
                option.Expires = DateTime.Now.AddMilliseconds(10);
            }
            controller.Response.Cookies.Append(key, value, option);
        }

        public static void RemoveCookie(this Controller controller, string key)
        {
            controller.Response.Cookies.Delete(key);
        }
    }
}
