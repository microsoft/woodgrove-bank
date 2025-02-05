using ITfoxtec.Identity.Saml2;
using ITfoxtec.Identity.Saml2.Schemas;
using ITfoxtec.Identity.Saml2.MvcCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Authentication;
using woodgrove_bank.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

// The code is based on https://github.com/ITfoxtec/ITfoxtec.Identity.Saml2/blob/master/test/TestWebAppCore/Controllers/AuthController.cs
namespace woodgrove_bank.Controllers
{
    [AllowAnonymous]
    [Route("Auth")]
    public class AuthController : Controller
    {
        const string relayStateReturnUrl = "ReturnUrl";
        private readonly Saml2Configuration config;
        private TelemetryClient _telemetry;

        public AuthController(Saml2Configuration config, TelemetryClient telemetry)
        {
            this.config = config;
            _telemetry = telemetry;
        }

        [Route("Login")]
        public IActionResult Login(string returnUrl = null)
        {
            // Application insights telemetry
            PageViewTelemetry pageView = new PageViewTelemetry("Login");
            pageView.Properties.Add("Area", "Demo");
            _telemetry.TrackPageView(pageView);

            var binding = new Saml2RedirectBinding();
            binding.SetRelayStateQuery(new Dictionary<string, string> { { relayStateReturnUrl, returnUrl ?? Url.Content("~/") } });

            return binding.Bind(new Saml2AuthnRequest(config) { }).ToActionResult();
        }

        [Route("ForceAuthLogin")]
        public IActionResult ForceAuthLogin(string returnUrl = null)
        {
            // Application insights telemetry
            PageViewTelemetry pageView = new PageViewTelemetry("ForceAuthLogin");
            pageView.Properties.Add("Area", "Demo");
            _telemetry.TrackPageView(pageView);

            var binding = new Saml2RedirectBinding();
            binding.SetRelayStateQuery(new Dictionary<string, string> { { relayStateReturnUrl, returnUrl ?? Url.Content("~/") } });

            return binding.Bind(new Saml2AuthnRequest(config)
            {
                ForceAuthn = true
            }).ToActionResult();
        }

        [Route("NameIDPolicy")]
        public IActionResult NameIDPolicy(string returnUrl = null, string ID = null)
        {
            // Application insights telemetry
            PageViewTelemetry pageView = new PageViewTelemetry("NameIDPolicy");
            pageView.Properties.Add("Area", "Demo");
            _telemetry.TrackPageView(pageView);

            var binding = new Saml2RedirectBinding();
            binding.SetRelayStateQuery(new Dictionary<string, string> { { relayStateReturnUrl, returnUrl ?? Url.Content("~/") } });

            NameIdPolicy nameIDPolicy = new NameIdPolicy();

            switch (ID.ToLower())
            {
                case "persistent":
                    nameIDPolicy.Format = "urn:oasis:names:tc:SAML:2.0:nameid-format:persistent";
                    break;

                case "emailaddress":
                    nameIDPolicy.Format = "urn:oasis:names:tc:SAML:1.1:nameid-format:emailAddress";
                    break;

                case "unspecified":
                    nameIDPolicy.Format = "urn:oasis:names:tc:SAML:1.1:nameid-format:unspecified";
                    break;

                case "transient":
                    nameIDPolicy.Format = "urn:oasis:names:tc:SAML:2.0:nameid-format:transient";
                    break;
                default:
                    nameIDPolicy.Format = "urn:oasis:names:tc:SAML:1.1:nameid-format:unspecified";
                    break;
            }


            return binding.Bind(new Saml2AuthnRequest(config)
            {
                NameIdPolicy = nameIDPolicy
            }).ToActionResult();
        }

        [Route("AssertionConsumerService")]
        public async Task<IActionResult> AssertionConsumerService()
        {
            // Application insights telemetry
            PageViewTelemetry pageView = new PageViewTelemetry("AssertionConsumerService");
            pageView.Properties.Add("Area", "SAML protocol");
            _telemetry.TrackPageView(pageView);

            var binding = new Saml2PostBinding();
            var saml2AuthnResponse = new Saml2AuthnResponse(config);

            binding.ReadSamlResponse(Request.ToGenericHttpRequest(), saml2AuthnResponse);
            if (saml2AuthnResponse.Status != Saml2StatusCodes.Success)
            {
                throw new AuthenticationException($"SAML Response status: {saml2AuthnResponse.Status}");
            }

            // Add SAML into the session
            HttpContext.Session.SetString("SamlResponse", binding.XmlDocument.OuterXml);

            // Continue with the SAML response handling
            binding.Unbind(Request.ToGenericHttpRequest(), saml2AuthnResponse);
            await saml2AuthnResponse.CreateSession(HttpContext, claimsTransform: (claimsPrincipal) => ClaimsTransform.Transform(claimsPrincipal));

            var relayStateQuery = binding.GetRelayStateQuery();
            var returnUrl = relayStateQuery.ContainsKey(relayStateReturnUrl) ? relayStateQuery[relayStateReturnUrl] : Url.Content("~/");

            // var identity = new ClaimsIdentity("Custom");
            // HttpContext.User = new ClaimsPrincipal(identity);
            return Redirect(returnUrl);
        }

        [HttpPost("Logout")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // Application insights telemetry
            PageViewTelemetry pageView = new PageViewTelemetry("Logout");
            pageView.Properties.Add("Area", "Demo");
            _telemetry.TrackPageView(pageView);

            if (!User.Identity.IsAuthenticated)
            {
                return Redirect(Url.Content("~/"));
            }

            var binding = new Saml2PostBinding();
            var saml2LogoutRequest = await new Saml2LogoutRequest(config, User).DeleteSession(HttpContext);
            return binding.Bind(saml2LogoutRequest).ToActionResult();
        }

        [Route("LoggedOut")]
        public IActionResult LoggedOut()
        {
            // Application insights telemetry
            PageViewTelemetry pageView = new PageViewTelemetry("LoggedOut");
            pageView.Properties.Add("Area", "SAML protocol");
            _telemetry.TrackPageView(pageView);

            var binding = new Saml2RedirectBinding(); //Saml2PostBinding();
            binding.Unbind(Request.ToGenericHttpRequest(), new Saml2LogoutResponse(config));

            return Redirect(Url.Content("~/"));
        }

        [Route("SingleLogout")]
        public async Task<IActionResult> SingleLogout()
        {
            // Application insights telemetry
            PageViewTelemetry pageView = new PageViewTelemetry("SingleLogout");
            pageView.Properties.Add("Area", "SAML protocol");
            _telemetry.TrackPageView(pageView);

            Saml2StatusCodes status;
            var requestBinding = new Saml2PostBinding();
            var logoutRequest = new Saml2LogoutRequest(config, User);
            try
            {
                requestBinding.Unbind(Request.ToGenericHttpRequest(), logoutRequest);
                status = Saml2StatusCodes.Success;
                await logoutRequest.DeleteSession(HttpContext);
            }
            catch (Exception exc)
            {
                // log exception
                Debug.WriteLine("SingleLogout error: " + exc.ToString());
                status = Saml2StatusCodes.RequestDenied;
            }

            var responsebinding = new Saml2PostBinding();
            responsebinding.RelayState = requestBinding.RelayState;
            var saml2LogoutResponse = new Saml2LogoutResponse(config)
            {
                InResponseToAsString = logoutRequest.IdAsString,
                Status = status,
            };
            return responsebinding.Bind(saml2LogoutResponse).ToActionResult();
        }
    }
}