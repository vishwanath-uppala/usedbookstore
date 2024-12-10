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
        private readonly IBookstoreConfiguration _configuration;

        public AuthenticationController(IBookstoreConfiguration configuration)
        {
            _configuration = configuration;
        }

        public ActionResult Login(string redirectUri = null)
        {
            if(string.IsNullOrWhiteSpace(redirectUri)) return RedirectToAction("Index", "Home");

            return Redirect(redirectUri);
        }

        public ActionResult LogOut()
        {
            return _configuration.Get("Services/Authentication") == "aws" ? CognitoSignOut() : LocalSignOut();
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

            var domain = _configuration.Get("Authentication/Cognito/CognitoDomain");
            var clientId = _configuration.Get("Authentication/Cognito/LocalClientId");
            var logoutUri = $"{Request.Scheme}://{Request.Host}/";

            return Redirect($"{domain}/logout?client_id={clientId}&logout_uri={logoutUri}");
        }
    }
}