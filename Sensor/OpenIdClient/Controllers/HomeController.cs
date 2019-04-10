using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Model;
using Newtonsoft.Json;
using ServiceProviderMultiTenant.Models;
using ServiceProviderMultiTenant.Services;

namespace ServiceProviderMultiTenant.Controllers
{
    public class HomeController : Controller
    {
        private readonly ISensorDataHttpClient _sensorDataHttpClient;

        public HomeController(ISensorDataHttpClient sensorDataHttpClient)
        {
            _sensorDataHttpClient = sensorDataHttpClient;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var idToken = await HttpContext.GetTokenAsync(OpenIdConnectParameterNames.IdToken);
            var accessToken = await HttpContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken);

            return View(new TokensViewModel { IdToken = idToken, AccessToken = accessToken });
        }


        public async Task<IActionResult> FetchDataUser()
        {
            var client = await _sensorDataHttpClient.GetClientAsync();
            var response = await client.GetAsync("/api/sensors/user");

            return await HandleApiResponse(response, async () =>
            {
                var jsonContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var sensorData = JsonConvert.DeserializeObject<IEnumerable<SensorData>>(jsonContent)
                    .ToList();

                return View(sensorData);
            });
        }


        public async Task<IActionResult> FetchDataAdmin()
        {
            var client = await _sensorDataHttpClient.GetClientAsync();
            var response = await client.GetAsync("/api/sensors/admin");

            return await HandleApiResponse(response, async () =>
            {
                var jsonContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var sensorData = JsonConvert.DeserializeObject<IEnumerable<SensorData>>(jsonContent)
                    .ToList();

                return View(sensorData);
            });
        }
        [Authorize(Roles = "SP2.Admin")]
        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult AccessDenied()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private async Task<IActionResult> HandleApiResponse(HttpResponseMessage response, Func<Task<IActionResult>> onSuccess)
        {
            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                    {
                        return await onSuccess();
                    }
                case HttpStatusCode.Unauthorized:
                case HttpStatusCode.Forbidden:
                    return RedirectToAction("AccessDenied", "Home");
                default:
                    throw new Exception($"A problem happened while calling the API: {response.ReasonPhrase}");
            }
        }
    }
}
