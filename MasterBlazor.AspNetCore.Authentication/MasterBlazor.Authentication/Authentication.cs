
using System;
using System.Collections.Generic;
using System.Linq;

using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

using MasterBlazor.AspNetCore.Authentication.PingFederate;

using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.WsFederation;
using Microsoft.IdentityModel.Tokens;

namespace MasterBlazor.Authentication
{
    public class Authentication
    {
        public static void Auth(IServiceCollection services, AuthenticationOption op)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                //options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            //services.AddAuthentication()
            services.AddAuthentication(delegate (AuthenticationOptions sharedOptions)
            {
                sharedOptions.DefaultScheme = "Cookies";
                sharedOptions.DefaultSignInScheme = "Cookies";
                sharedOptions.DefaultChallengeScheme = "WsFederation";
            })
                //.AddWsFederation(WsFederationDefaults.AuthenticationScheme, WsFederationDefaults.DisplayName,options =>
            .AddPingFederate("WsFederation", "PingFed", delegate (PingFederateOptions options)
                {

                    X509Certificate2 cert = new X509Certificate2(op.CertificatePath);
                    SecurityKey signingKey = new X509SecurityKey(cert);

                    options.RequireHttpsMetadata = true;
                    options.MetadataAddress = op.MetadataAddress;
                    options.Wtrealm = op.Wtrealm;
                    options.ClaimsIssuer = op.Issuer;
                    

                    options.Configuration = new WsFederationConfiguration()
                    {
                        Issuer = op.Issuer,
                        TokenEndpoint = op.Issuer,
                    };

                    options.Configuration.SigningKeys.Add(signingKey);

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        RequireAudience = true,
                        RequireSignedTokens = false,
                        AuthenticationType = CookieAuthenticationDefaults.AuthenticationScheme,
                        ValidIssuer = op.ValidIssuer,
                        IssuerSigningKeys = new List<SecurityKey>() { signingKey },
                        ValidAudience = op.ValidAudience,

                        
                        SignatureValidator = delegate (string token, TokenValidationParameters validationParameters)
                        {
                            XElement x = XElement.Parse(token);
                            var _X509Certificate = x.Descendants().Where(xg => xg.Name.LocalName == "X509Certificate").First().Value;
                            var _Signature = x.Descendants().Where(xg => xg.Name.LocalName == "SignatureValue").First().Value;
                            var _DigestValue = x.Descendants().Where(xg => xg.Name.LocalName == "DigestValue").First().Value;
                            var _NameIdentifier = x.Descendants().Where(xg => xg.Name.LocalName == "NameIdentifier").First().Value;

                            XmlReader xml = XmlDictionaryReader.CreateTextReader(Encoding.UTF8.GetBytes(token), XmlDictionaryReaderQuotas.Max);
                            Microsoft.IdentityModel.Tokens.Saml.SamlSerializer sr = new Microsoft.IdentityModel.Tokens.Saml.SamlSerializer();
                            Microsoft.IdentityModel.Tokens.Saml.SamlAssertion sr2 = sr.ReadAssertion(xml);
                            return (Microsoft.IdentityModel.Tokens.SecurityToken)new Microsoft.IdentityModel.Tokens.Saml.SamlSecurityToken(sr2);
                        },

                    };

                    options.Events.OnSecurityTokenValidated = (context) =>
                    {
                        //Condense claim data (roles) - otherwise we get errors for too large of claims
                        HashSet<Claim> filteredClaims = new HashSet<Claim>();
                        HashSet<string> groupSet = new HashSet<string>();

                        foreach (Claim claim in context.Principal.Claims)
                        {
                            if (claim.Type != ClaimTypes.Role)
                            {
                                filteredClaims.Add(claim);
                            }
                            else
                            {
                                groupSet.Add(claim.Value);
                            }
                        }
                        filteredClaims.Add(new Claim("admin", string.Join("admin", groupSet)));

                        List<ClaimsIdentity> filteredClaimsIdentity = new List<ClaimsIdentity>() { new ClaimsIdentity(filteredClaims, context.Principal.Identity.AuthenticationType) };
                        ClaimsPrincipal newClaimsPrincipal = new ClaimsPrincipal(filteredClaimsIdentity);
                        context.Principal = newClaimsPrincipal;

                        return Task.FromResult(0);
                    };

                })
            .AddCookie(cookieoption => {
                    cookieoption.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                    cookieoption.Events = new CookieAuthenticationEvents
                    {
                        OnValidatePrincipal = AdditionalValidation
                    };
            });
        }

        public static async Task AdditionalValidation(CookieValidatePrincipalContext context)
        {
            if (context != null & context.Request != null)
            {
                ClaimsPrincipal userPrincipal = context.Principal;

                // Add additional validation here or extend the identity of the claims principal here.
            }
        }
        public static async Task UseAuth(HttpContext context, Func<Task> next)
        {

            if (!context.User.Identity.IsAuthenticated && context.Request.Path != "/signin-wsfed")
            {
                await context.ChallengeAsync(PingFederateDefaults.AuthenticationScheme);
            }
            else
            {
                await next();
            }

        }
    }
}
