using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using cift_anadal_yandal.Models; // Kendi proje adına göre burayı kontrol et
using Microsoft.EntityFrameworkCore;

namespace cift_anadal_yandal.Controllers
{
    public class KullanicilarController : Controller
    {
        private readonly CiftAnadalYandalDbContext _context;

        public KullanicilarController(CiftAnadalYandalDbContext context)
        {
            _context = context;
        }

        // GET: Kullanicilar/Login
        public IActionResult Login()
        {
            // Zaten giriş yapmış biri login ekranına gelirse onu paneline (Başvurular) at
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Basvurular");
            }

            // Giriş yapmamışsa direkt giriş formunu göster (Artık rolü bu sayfada seçecek)
            return View();
        }

        // POST: Giriş İşlemini Yap
        [HttpPost]
        public async Task<IActionResult> Login(string kullaniciAdi, string sifre, string rol)
        {
            // Veritabanında bu kullanıcı adı, şifre VE GİRİLMEYE ÇALIŞILAN ROL ile eşleşen biri var mı?
            // Hem şifreyi hem rolü aynı anda kontrol ediyoruz.
            var kullanici = await _context.Kullanicilars
                .FirstOrDefaultAsync(k => k.KullaniciAdi == kullaniciAdi && k.Sifre == sifre && k.Rol == rol);

            // Eğer tam eşleşme varsa giriş başarılıdır
            if (kullanici != null)
            {
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, kullanici.KullaniciAdi),
            new Claim(ClaimTypes.Role, kullanici.Rol),
            new Claim("KullaniciId", kullanici.KullaniciId.ToString())
        };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                return RedirectToAction("Index", "Home");
            }

            // Şifre yanlışsa, kullanıcı adı yoksa veya ROL yanlışsa 
            // HİÇBİR İPUCU VERMEDEN aynı jenerik hata mesajını gösteriyoruz.
            ViewBag.Hata = "Kullanıcı adı veya şifre hatalı!";
            ViewBag.SecilenRol = rol; // Ekran başlığının (Öğrenci Girişi vb.) kaybolmaması için
            return View();
        }

        // Çıkış Yapma İşlemi
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Kullanicilar");
        }

        // Yetkisiz Erişim Ekranı
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}