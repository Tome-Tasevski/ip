using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Client_Saml.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;

namespace Client_Saml.Controllers
{
    public class HomeController : Controller

    {
        public IActionResult Index() => View();
        public async Task<IActionResult> Logout() {

            await HttpContext.SignOutAsync("cookie");
            await HttpContext.SignOutAsync("saml2p");

            return View();
        }
      
        public async Task<IActionResult> ChallengeScheme(string scheme)
        {
            var result = await HttpContext.AuthenticateAsync(scheme);
            if (result.Succeeded && result.Principal != null) return RedirectToAction("Index");

            return Challenge(scheme);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
