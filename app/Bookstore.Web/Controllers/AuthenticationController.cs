using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using BobsBookstoreClassic.Data;
using Bookstore.Domain;
using Bookstore.Data;

namespace Bookstore.Web.Controllers
{
    public class AuthenticationController : Controller
    {
        public ActionResult Login(string redirectUri = null)
        {
            if(string.IsNullOrWhiteSpace(redirectUri)) return RedirectToAction("Index", "Home");

            return Redirect(redirectUri);
        }

        public ActionResult LogOut()
        {
            var authType = GetConfigurationValue("Services/Authentication");
            return authType == "aws" ? CognitoSignOut() : LocalSignOut();
        }

        private ActionResult LocalSignOut()
        {
            if (HttpContext.Request.Cookies["LocalAuthentication"] != null)
            {
                HttpContext.Response.Cookies.Delete("LocalAuthentication");
            }

            return RedirectToAction("Index", "Home");
        }

        private ActionResult CognitoSignOut()
        {
            if (Request.Cookies[".AspNet.Cookies"] != null)
            {
                Response.Cookies.Delete(".AspNet.Cookies");
            }

            var domain = GetConfigurationValue("Authentication/Cognito/CognitoDomain");
            var clientId = GetConfigurationValue("Authentication/Cognito/LocalClientId");
            var logoutUri = $"{Request.Scheme}://{Request.Host}/";

            return Redirect($"{domain}/logout?client_id={clientId}&logout_uri={logoutUri}");
        }

        private string GetConfigurationValue(string key)
        {
            // TODO: Implement the actual configuration retrieval logic here
            // This is a placeholder implementation and should be replaced with the actual logic
            // to retrieve configuration values from your application's configuration system
            throw new NotImplementedException($"Configuration retrieval for key '{key}' is not implemented.");
        }
    }
}