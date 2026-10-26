using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using cift_anadal_yandal.Models;

namespace cift_anadal_yandal.Controllers
{
    [Authorize(Roles = "Ogrenci,Koordinator")]
    public class MesajlarController : Controller
    {
        private readonly CiftAnadalYandalDbContext _context;

        public MesajlarController(CiftAnadalYandalDbContext context)
        {
            _context = context;
        }

        // ÖĞRENCİNİN KENDİ MESAJLAŞMA EKRANI
        [Authorize(Roles = "Ogrenci")]
        public async Task<IActionResult> Index()
        {
            var ogrenciNo = User.Identity?.Name;
            var ogrenci = await _context.Ogrencilers
                .Include(o => o.AnaBolum)
                .FirstOrDefaultAsync(o => o.OgrenciNo == ogrenciNo);

            if (ogrenci == null) return Unauthorized();

            var mesajlar = await _context.Mesajlars
                .Where(m => m.OgrenciId == ogrenci.OgrenciId)
                .OrderBy(m => m.GonderimTarihi)
                .ToListAsync();

            ViewBag.Ogrenci = ogrenci;
            return View(mesajlar);
        }

        // ÖĞRENCİNİN KOORDİNATÖRE MESAJ GÖNDERMESİ
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Ogrenci")]
        public async Task<IActionResult> OgrenciMesajGonder(string mesajMetni)
        {
            var ogrenciNo = User.Identity?.Name;
            var ogrenci = await _context.Ogrencilers.FirstOrDefaultAsync(o => o.OgrenciNo == ogrenciNo);
            if (ogrenci == null) return Unauthorized();

            if (!string.IsNullOrWhiteSpace(mesajMetni))
            {
                var yeniMesaj = new Mesajlar
                {
                    OgrenciId = ogrenci.OgrenciId,
                    GonderenRol = "Ogrenci",
                    GonderenAd = $"{ogrenci.Ad} {ogrenci.Soyad} ({ogrenci.OgrenciNo})",
                    MesajMetni = mesajMetni.Trim(),
                    GonderimTarihi = DateTime.Now
                };

                _context.Mesajlars.Add(yeniMesaj);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // KOORDİNATÖRÜN TÜM ÖĞRENCİ MESAJLARINI GÖRDÜĞÜ PANEL
        [Authorize(Roles = "Koordinator")]
        public async Task<IActionResult> Panel()
        {
            var ogrenciler = await _context.Ogrencilers
                .Include(o => o.AnaBolum)
                .OrderBy(o => o.Ad)
                .ToListAsync();

            var tumMesajlar = await _context.Mesajlars.ToListAsync();
            ViewBag.TumMesajlar = tumMesajlar;

            return View(ogrenciler);
        }

        // KOORDİNATÖRÜN SEÇİLEN ÖĞRENCİ İLE SOHBET EKRANI
        [Authorize(Roles = "Koordinator")]
        public async Task<IActionResult> Sohbet(int ogrenciId)
        {
            var ogrenci = await _context.Ogrencilers
                .Include(o => o.AnaBolum)
                .FirstOrDefaultAsync(o => o.OgrenciId == ogrenciId);

            if (ogrenci == null) return NotFound();

            var mesajlar = await _context.Mesajlars
                .Where(m => m.OgrenciId == ogrenciId)
                .OrderBy(m => m.GonderimTarihi)
                .ToListAsync();

            ViewBag.Ogrenci = ogrenci;
            return View(mesajlar);
        }

        // KOORDİNATÖRÜN ÖĞRENCİYE MESAJ GÖNDERMESİ
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Koordinator")]
        public async Task<IActionResult> KoordinatorMesajGonder(int ogrenciId, string mesajMetni)
        {
            var ogrenci = await _context.Ogrencilers.FindAsync(ogrenciId);
            if (ogrenci == null) return NotFound();

            if (!string.IsNullOrWhiteSpace(mesajMetni))
            {
                var yeniMesaj = new Mesajlar
                {
                    OgrenciId = ogrenciId,
                    GonderenRol = "Koordinator",
                    GonderenAd = $"ÇAP/Yandal Koordinatörü ({User.Identity?.Name})",
                    MesajMetni = mesajMetni.Trim(),
                    GonderimTarihi = DateTime.Now
                };

                _context.Mesajlars.Add(yeniMesaj);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Sohbet), new { ogrenciId = ogrenciId });
        }
    }
}