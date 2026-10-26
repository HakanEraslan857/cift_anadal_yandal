using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using cift_anadal_yandal.Models;

namespace cift_anadal_yandal.Controllers
{
    [Authorize]
    public class BasvurularController : Controller
    {
        private readonly CiftAnadalYandalDbContext _context;

        public BasvurularController(CiftAnadalYandalDbContext context)
        {
            _context = context;
        }

        // GET: Basvurular
        public async Task<IActionResult> Index()
        {
            var sorgu = _context.Basvurulars
                .Include(b => b.HedefBolum)
                .Include(b => b.Ogrenci)
                    .ThenInclude(o => o.AnaBolum)
                .AsQueryable();

            // Giriş yapan Öğrenci ise sadece kendi başvurularını görsün
            if (User.IsInRole("Ogrenci"))
            {
                var ogrenciNo = User.Identity?.Name;
                var ogrenci = await _context.Ogrencilers.FirstOrDefaultAsync(o => o.OgrenciNo == ogrenciNo);
                ViewBag.BasvuruAcikMi = ogrenci?.BasvuruAcikMi ?? false;

                sorgu = sorgu.Where(b => b.Ogrenci != null && b.Ogrenci.OgrenciNo == ogrenciNo);
            }

            return View(await sorgu.OrderByDescending(b => b.BasvuruTarihi).ToListAsync());
        }

        // GET: Basvurular/Details/5
        [Authorize(Roles = "Koordinator,BolumBaskani,Ogrenci")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var basvuru = await _context.Basvurulars
                .Include(b => b.HedefBolum)
                .Include(b => b.Ogrenci)
                    .ThenInclude(o => o.AnaBolum)
                .FirstOrDefaultAsync(m => m.BasvuruId == id);

            if (basvuru == null)
            {
                return NotFound();
            }

            return View(basvuru);
        }

        // GET: Basvurular/Create
        [Authorize(Roles = "Ogrenci")]
        public async Task<IActionResult> Create()
        {
            var ogrenciNo = User.Identity?.Name;
            var ogrenci = await _context.Ogrencilers
                .Include(o => o.AnaBolum)
                .FirstOrDefaultAsync(o => o.OgrenciNo == ogrenciNo);

            if (ogrenci == null)
            {
                return Content("Hata: Koordinatör tarafından tanımlanmış öğrenci kaydınız bulunamadı!");
            }

            ViewBag.BasvuruAcikMi = ogrenci.BasvuruAcikMi;
            ViewBag.Ogrenci = ogrenci;
            return View();
        }

        // POST: Basvurular/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Ogrenci")]
        public async Task<IActionResult> Create(IFormFile transkriptDosyasi, IFormFile protokolDosyasi, IFormFile dersKayitDosyasi)
        {
            var ogrenciNo = User.Identity?.Name;
            var mevcutOgrenci = await _context.Ogrencilers
                .Include(o => o.AnaBolum)
                .FirstOrDefaultAsync(o => o.OgrenciNo == ogrenciNo);

            if (mevcutOgrenci == null)
            {
                return Unauthorized();
            }

            // Öğrencinin açık başvuru hakkı yoksa başvuruyu engelle
            if (!mevcutOgrenci.BasvuruAcikMi)
            {
                TempData["BilgiMesaji"] = "Mevcut başvuru hakkınızı kullandınız. Yeni bir başvuru açılmadığı sürece tekrar başvuru yapamazsınız.";
                return RedirectToAction(nameof(Index));
            }

            string basvuruTuru = mevcutOgrenci.BasvuruTuru ?? "Çift Anadal";

            // 1. KONTROL: 3 evraktan herhangi biri eksik mi?
            if (transkriptDosyasi == null || transkriptDosyasi.Length == 0 ||
                protokolDosyasi == null || protokolDosyasi.Length == 0 ||
                dersKayitDosyasi == null || dersKayitDosyasi.Length == 0)
            {
                ViewBag.Hata = "Hata: Lütfen tüm zorunlu evrakları eksiksiz yükleyiniz! Eksik evrak ile başvuru yapılamaz.";
                ViewBag.BasvuruAcikMi = mevcutOgrenci.BasvuruAcikMi;
                ViewBag.Ogrenci = mevcutOgrenci;
                return View();
            }

            // 2. KONTROL: Yüklenen tüm dosyalar gerçekten PDF formatında mı?
            string ext1 = Path.GetExtension(transkriptDosyasi.FileName).ToLower();
            string ext2 = Path.GetExtension(protokolDosyasi.FileName).ToLower();
            string ext3 = Path.GetExtension(dersKayitDosyasi.FileName).ToLower();

            if (ext1 != ".pdf" || ext2 != ".pdf" || ext3 != ".pdf")
            {
                ViewBag.Hata = "Hata: Yüklediğiniz tüm evraklar yalnızca PDF (.pdf) formatında olmalıdır!";
                ViewBag.BasvuruAcikMi = mevcutOgrenci.BasvuruAcikMi;
                ViewBag.Ogrenci = mevcutOgrenci;
                return View();
            }

            // 3. EVRAKLARI KLASÖRE KAYDETME (wwwroot/evraklar)
            string klasorYolu = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "evraklar");
            if (!Directory.Exists(klasorYolu))
            {
                Directory.CreateDirectory(klasorYolu);
            }

            async Task<string> DosyaKaydet(IFormFile dosya, string belgeTuru)
            {
                string dosyaAdi = $"{mevcutOgrenci.OgrenciNo}_{belgeTuru}_{Guid.NewGuid()}{Path.GetExtension(dosya.FileName)}";
                string tamYol = Path.Combine(klasorYolu, dosyaAdi);
                using (var stream = new FileStream(tamYol, FileMode.Create))
                {
                    await dosya.CopyToAsync(stream);
                }
                return dosyaAdi;
            }

            string transkriptAdi = await DosyaKaydet(transkriptDosyasi, "Transkript");
            string protokolAdi = await DosyaKaydet(protokolDosyasi, "ProtokolFormu");
            string dersKayitAdi = await DosyaKaydet(dersKayitDosyasi, "DersKayitFormu");

            // 4. BAŞVURUYU VERİTABANINA KAYDETME VE HAKKI KAPATMA
            var pcBolum = await _context.Bolumlers.FirstOrDefaultAsync(b => b.BolumAdi.Contains("Bilgisayar"));

            var yeniBasvuru = new Basvurular
            {
                OgrenciId = mevcutOgrenci.OgrenciId,
                HedefBolumId = pcBolum?.BolumId,
                BasvuruTuru = basvuruTuru,
                BasvuruTarihi = DateTime.Now,
                Durum = "Beklemede",
                TranskriptYolu = transkriptAdi,
                ProtokolFormuYolu = protokolAdi,
                DersKayitFormuYolu = dersKayitAdi
            };

            // Öğrenci başvuru yaptığı için hakkını kapatıyoruz
            mevcutOgrenci.BasvuruAcikMi = false;

            _context.Basvurulars.Add(yeniBasvuru);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Basvurular/Onayla/5
        [Authorize(Roles = "Koordinator")]
        public async Task<IActionResult> Onayla(int id)
        {
            var basvuru = await _context.Basvurulars.FindAsync(id);
            if (basvuru != null && basvuru.Durum == "Beklemede")
            {
                basvuru.Durum = "Kabul Edildi";
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Basvurular/Reddet/5
        [Authorize(Roles = "Koordinator")]
        public async Task<IActionResult> Reddet(int id)
        {
            var basvuru = await _context.Basvurulars.FindAsync(id);
            if (basvuru != null && basvuru.Durum == "Beklemede")
            {
                basvuru.Durum = "Reddedildi";
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: Basvurular/EvrakGuncelle
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Koordinator")]
        public async Task<IActionResult> EvrakGuncelle(int basvuruId, string belgeTuru, IFormFile yeniDosya, string? returnUrl)
        {
            var basvuru = await _context.Basvurulars
                .Include(b => b.Ogrenci)
                .FirstOrDefaultAsync(b => b.BasvuruId == basvuruId);

            if (basvuru == null)
            {
                return NotFound();
            }

            if (yeniDosya == null || yeniDosya.Length == 0)
            {
                TempData["Hata"] = "Lütfen güncellenecek yeni PDF dosyasını seçiniz!";
            }
            else if (Path.GetExtension(yeniDosya.FileName).ToLower() != ".pdf")
            {
                TempData["Hata"] = "Hata: Yüklenen evrak yalnızca PDF (.pdf) formatında olmalıdır!";
            }
            else
            {
                string klasorYolu = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "evraklar");
                if (!Directory.Exists(klasorYolu))
                {
                    Directory.CreateDirectory(klasorYolu);
                }

                string ogrenciNo = basvuru.Ogrenci?.OgrenciNo ?? "Ogrenci";
                string yeniDosyaAdi = $"{ogrenciNo}_{belgeTuru}_{Guid.NewGuid()}.pdf";
                string tamYol = Path.Combine(klasorYolu, yeniDosyaAdi);

                using (var stream = new FileStream(tamYol, FileMode.Create))
                {
                    await yeniDosya.CopyToAsync(stream);
                }

                // İlgili belge türünün dosya yolunu güncelle
                if (belgeTuru == "Transkript")
                {
                    basvuru.TranskriptYolu = yeniDosyaAdi;
                }
                else if (belgeTuru == "ProtokolFormu")
                {
                    basvuru.ProtokolFormuYolu = yeniDosyaAdi;
                }
                else if (belgeTuru == "DersKayitFormu")
                {
                    basvuru.DersKayitFormuYolu = yeniDosyaAdi;
                }

                await _context.SaveChangesAsync();
                TempData["Basari"] = "Kayıt evrağı başarıyla güncellendi ve değiştirildi.";
            }

            if (!string.IsNullOrEmpty(returnUrl))
            {
                TempData["AcikOgrenciId"] = basvuru.OgrenciId;
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(Details), new { id = basvuruId });
        }
    }
}