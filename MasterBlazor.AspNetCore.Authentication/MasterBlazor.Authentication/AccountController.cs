using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication; //HttpContext.AuthenticateAsync
using MasterBlazor.AspNetCore.Authentication.PingFederate;
using System.Security.Claims;
using Newtonsoft.Json;

namespace MasterBlazor.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    [IgnoreAntiforgeryToken]
    public class AccountController : ControllerBase
    {
        public async Task SignIn()
        {
            if (!HttpContext.User.Identity.IsAuthenticated && Visited<2)
            {
                
                var authenticateResult = await HttpContext.AuthenticateAsync(PingFederateDefaults.AuthenticationScheme).ConfigureAwait(false);
                var principal = authenticateResult?.Principal;


                if (principal?.Identities != null && principal.Identities.Any(i => i.IsAuthenticated))
                {
                    if (principal.Identities.Any(i => i.AuthenticationType == "CookieAuth"))
                    {
                        
                    }
                    else
                    {
                        
                        await SignInCookie(principal.Identity.Name, new[] { "FedAuth" }, null).ConfigureAwait(false);
                    }

                    // already auth
                    HttpContext.Response.Redirect("/");

                }
                //test
                Visited++;
                IsAuthenticatedTest =true;
                
                // not auth, challenge adfs
                await HttpContext.ChallengeAsync(PingFederateDefaults.AuthenticationScheme, new AuthenticationProperties { RedirectUri = "/" });

                //return RedirectToAction("Index"); // or Unauthorized
            }
            else
            {
                
                if (Visited > 0) Visited=0;
                HttpContext.Response.Redirect("/");
            }
            
        }

        private Task SignInCookie(string username, string[] roles, string salesRepid)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username)
            };

            if (roles != null)
            {
                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, "CookieAuth", ClaimTypes.Name, ClaimTypes.Role));

            return HttpContext.SignInAsync(PingFederateDefaults.AuthenticationScheme, claimsPrincipal, new AuthenticationProperties { RedirectUri = "/" });
        }

        [IgnoreAntiforgeryToken]
        public void SignOut()
        {
            string callbackUrl = "/";
            HttpContext.SignOutAsync("Cookies");
            var prop = new AuthenticationProperties()
            {
                RedirectUri = callbackUrl
            };
            // after signout this will redirect to your provided target
            HttpContext.SignOutAsync(PingFederateDefaults.AuthenticationScheme, prop);
        }
       

        public Task<String> IsAuthenticated()
        {
            var ret = "";
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                foreach (var claim in HttpContext.User.Claims)
                {
                    if(claim.Type.Contains("nameidentifier"))
                        ret = claim.Value;
                }
            }
            return Task.FromResult(ret);
        }
        public Task<String> GetUser0()
        {
            if (HttpContext == null) return Task.FromResult("HttpContext null");
            else if (HttpContext.User == null) return Task.FromResult("HttpContext.User null");

            else
            {
                try
                {
                    var ret = JsonConvert.SerializeObject(HttpContext.User.Identities.ToList());
                    return Task.FromResult(ret);
                }
                catch(Exception e)
                {
                    return Task.FromResult(e.Message);
                }
            }
        }

        public dynamic GetUser()
        {
            return HttpContext.User as dynamic;
        }
        public Task<String> GetClaims()
        {
            String ret = "";
            foreach(var claim in HttpContext.User.Claims)
            {
                if (ret != "") ret += ",";
                ret += "\""+claim.Type+"\" : \""+claim.Value+"\"";
            }
            ret= "{" + ret +"}";

            return Task.FromResult(ret);
        }

        public Task<String> GetIdentity()
        {
            if (HttpContext == null) return Task.FromResult("HttpContext null");
            else if (HttpContext.User == null) return Task.FromResult("HttpContext.User null");
            else if (HttpContext.User.Identity == null) return Task.FromResult("HttpContext.User.Identity null");
            else return Task.FromResult(JsonConvert.SerializeObject(HttpContext.User.Identities.ToList()));
        }

        static bool _isAuthenticated = false;
        

        public bool IsAuthenticatedTest { get { return _isAuthenticated; } set { _isAuthenticated = value; } }

        

        static int _visited = 0;
        public int Visited {
            get { return _visited; }
            set { _visited = value; } 
        }

        public Task<int> GetVisits()
        {
            return Task.FromResult(Visited);
        }

        public Task<String> GetStatus1()
        {

            return Task.FromResult(HttpContext.User.Identity.IsAuthenticated.ToString());
        }
    }
}
