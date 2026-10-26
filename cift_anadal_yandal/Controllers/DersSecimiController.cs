using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using cift_anadal_yandal.Models;

namespace cift_anadal_yandal.Controllers
{
    [Authorize]
    public class DersSecimiController : Controller
    {
        private readonly CiftAnadalYandalDbContext _context;

        public DersSecimiController(CiftAnadalYandalDbContext context)
        {
            _context = context;
        }

        // ÖĞRENCİ DERS SEÇİM EKRANI
        [Authorize(Roles = "Ogrenci")]
        public async Task<IActionResult> Index()
        {
            var ogrenciNo = User.Identity?.Name;
            var ogrenci = await _context.Ogrencilers
                .Include(o => o.AnaBolum)
                .FirstOrDefaultAsync(o => o.OgrenciNo == ogrenciNo);

            if (ogrenci == null) return Unauthorized();

            // Öğrencinin onaylanmış başvurusu var mı kontrol et
            var onayliBasvuru = await _context.Basvurulars
                .FirstOrDefaultAsync(b => b.OgrenciId == ogrenci.OgrenciId &&
                                         (b.Durum == "Onaylandı" || b.Durum == "Kabul Edildi"));

            ViewBag.BasvuruOnayliMi = (onayliBasvuru != null);
            ViewBag.Ogrenci = ogrenci;

            if (onayliBasvuru == null)
            {
                return View(new List<Dersler>());
            }

            // Öğrencinin daha önce seçtiği dersleri getir
            var mevcutSecimler = await _context.OgrenciDersSecimleris
                .Include(s => s.Ders)
                .Where(s => s.OgrenciId == ogrenci.OgrenciId)
                .ToListAsync();

            ViewBag.MevcutSecimler = mevcutSecimler;
            ViewBag.SecimDurumu = mevcutSecimler.FirstOrDefault()?.Durum;

            var tumDersler = await _context.Derslers.OrderBy(d => d.DersId).ToListAsync();
            return View(tumDersler);
        }

        // ÖĞRENCİ DERS SEÇİMİNİ KAYDETME (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Ogrenci")]
        public async Task<IActionResult> Kaydet(List<int> secilenDersIds)
        {
            var ogrenciNo = User.Identity?.Name;
            var ogrenci = await _context.Ogrencilers.FirstOrDefaultAsync(o => o.OgrenciNo == ogrenciNo);
            if (ogrenci == null) return Unauthorized();

            var onayliBasvuru = await _context.Basvurulars
                .FirstOrDefaultAsync(b => b.OgrenciId == ogrenci.OgrenciId &&
                                         (b.Durum == "Onaylandı" || b.Durum == "Kabul Edildi"));
            if (onayliBasvuru == null) return Unauthorized();

            if (secilenDersIds == null || !secilenDersIds.Any())
            {
                TempData["Hata"] = "Lütfen en az bir ders seçiniz!";
                return RedirectToAction(nameof(Index));
            }

            var eskiSecimler = await _context.OgrenciDersSecimleris
                .Where(s => s.OgrenciId == ogrenci.OgrenciId)
                .ToListAsync();

            if (eskiSecimler.Any() && eskiSecimler.First().Durum != "Reddedildi")
            {
                return RedirectToAction(nameof(Index));
            }

            if (eskiSecimler.Any())
            {
                _context.OgrenciDersSecimleris.RemoveRange(eskiSecimler);
            }

            var simdi = DateTime.Now;
            // Distinct() ile bir dersin birden fazla kez eklenmesini engelliyoruz
            foreach (var dersId in secilenDersIds.Distinct())
            {
                _context.OgrenciDersSecimleris.Add(new OgrenciDersSecimleri
                {
                    OgrenciId = ogrenci.OgrenciId,
                    DersId = dersId,
                    SecimTarihi = simdi,
                    Durum = "Koordinatör Onayı Bekliyor"
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        // KOORDİNATÖR VE BÖLÜM BAŞKANI İÇİN DERS SEÇİM ONAY LİSTESİ (YENİDEN ESKİYE SIRALI)
        [Authorize(Roles = "Koordinator,BolumBaskani")]
        public async Task<IActionResult> OnayListesi()
        {
            var secimler = await _context.OgrenciDersSecimleris
                .Include(s => s.Ogrenci)
                    .ThenInclude(o => o.AnaBolum)
                .Include(s => s.Ders)
                .ToListAsync();

            // Öğrenciye göre grupla ve en son yapılan ders seçiminden en eskiye doğru (yeniden eskiye) sırala
            var gruplanmis = secimler
                .GroupBy(s => s.Ogrenci)
                .OrderByDescending(g => g.Max(x => x.SecimTarihi))
                .ToList();

            // Koordinatörün ders değiştirebilmesi için tüm dersleri ViewBag ile gönderiyoruz
            ViewBag.TumDersler = await _context.Derslers.OrderBy(d => d.DersId).ToListAsync();
            ViewBag.Basvurular = await _context.Basvurulars.OrderByDescending(b => b.BasvuruTarihi).ToListAsync();
            return View(gruplanmis);
        }

        // KOORDİNATÖR: TEK BİR DERSİ BAŞKA BİR DERSLE DEĞİŞTİRME
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Koordinator")]
        public async Task<IActionResult> DersDegistir(int secimId, int yeniDersId)
        {
            var secim = await _context.OgrenciDersSecimleris.FindAsync(secimId);
            if (secim != null)
            {
                // Öğrencinin tercih listesinde bu ders zaten var mı kontrol et
                bool dersZatenVar = await _context.OgrenciDersSecimleris
                    .AnyAsync(s => s.OgrenciId == secim.OgrenciId && s.DersId == yeniDersId);

                if (dersZatenVar)
                {
                    TempData["Hata"] = "Bu ders öğrencinin tercih listesinde zaten mevcut! Bir ders yalnızca 1 kere tercih edilebilir.";
                }
                else
                {
                    secim.DersId = yeniDersId;
                    await _context.SaveChangesAsync();
                    TempData["Basari"] = "Öğrencinin dersi başarıyla değiştirildi.";
                }

                // İşlem sonrası aynı öğrencinin satırı açık kalsın
                TempData["AcikOgrenciId"] = secim.OgrenciId;
            }
            return RedirectToAction(nameof(OnayListesi));
        }

        // KOORDİNATÖR: TEK BİR DERSİ İPTAL ETME (LİSTEDEN ÇIKARMA)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Koordinator")]
        public async Task<IActionResult> DersIptalEt(int secimId)
        {
            var secim = await _context.OgrenciDersSecimleris.FindAsync(secimId);
            if (secim != null)
            {
                int ogrenciId = secim.OgrenciId;
                _context.OgrenciDersSecimleris.Remove(secim);
                await _context.SaveChangesAsync();

                TempData["Basari"] = "Seçilen ders iptal edildi ve öğrencinin tercih listesinden çıkarıldı.";
                TempData["AcikOgrenciId"] = ogrenciId;
            }
            return RedirectToAction(nameof(OnayListesi));
        }

        // KOORDİNATÖR: ÖĞRENCİNİN TÜM DERS SEÇİMİNİ ONAYLA
        [Authorize(Roles = "Koordinator")]
        public async Task<IActionResult> SecimOnayla(int ogrenciId)
        {
            var ogrenciSecimleri = await _context.OgrenciDersSecimleris
                .Where(s => s.OgrenciId == ogrenciId)
                .ToListAsync();

            foreach (var secim in ogrenciSecimleri)
            {
                secim.Durum = "Onaylandı";
            }

            await _context.SaveChangesAsync();
            TempData["Basari"] = "Öğrencinin ders seçimi onaylandı.";
            TempData["AcikOgrenciId"] = ogrenciId;
            return RedirectToAction(nameof(OnayListesi));
        }

        // KOORDİNATÖR: ÖĞRENCİNİN TÜM DERS SEÇİMİNİ REDDET / İPTAL ET
        [Authorize(Roles = "Koordinator")]
        public async Task<IActionResult> SecimReddet(int ogrenciId)
        {
            var ogrenciSecimleri = await _context.OgrenciDersSecimleris
                .Where(s => s.OgrenciId == ogrenciId)
                .ToListAsync();

            foreach (var secim in ogrenciSecimleri)
            {
                secim.Durum = "Reddedildi";
            }

            await _context.SaveChangesAsync();
            TempData["Basari"] = "Öğrencinin ders seçimi reddedildi.";
            TempData["AcikOgrenciId"] = ogrenciId;
            return RedirectToAction(nameof(OnayListesi));
        }
    }
}