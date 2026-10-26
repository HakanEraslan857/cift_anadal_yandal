using cift_anadal_yandal.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace cift_anadal_yandal.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            // Eğer giriş yapmışsa kendi paneline at
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Basvurular");
            }

            // Giriş YAPMAMIŞSA artık o 3 kartlı ekran yerine doğrudan yeni Login Panelimize gitsin
            return RedirectToAction("Login", "Kullanicilar");
        }

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
