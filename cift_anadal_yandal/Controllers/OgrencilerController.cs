
using cift_anadal_yandal.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

[Authorize(Roles = "Koordinator")]
public class OgrencilerController : Controller
{
    private readonly CiftAnadalYandalDbContext _context;

    public OgrencilerController(CiftAnadalYandalDbContext context)
    {
        _context = context;
    }

    // GET: OGRENCILERS
    public async Task<IActionResult> Index()    
    {
        // Öğrencileri çekerken AnaBolum tablosundaki verileri de peşine takıyoruz
        var ciftAnadalYandalDbContext = _context.Ogrencilers.Include(o => o.AnaBolum);
        return View(await ciftAnadalYandalDbContext.ToListAsync());
    }

    // GET: OGRENCILERS/Details/5
    public async Task<IActionResult> Details(int? ogrenciid)
    {
        if (ogrenciid == null)
        {
            return NotFound();
        }

        var ogrenciler = await _context.Ogrencilers
            .FirstOrDefaultAsync(m => m.OgrenciId == ogrenciid);
        if (ogrenciler == null)
        {
            return NotFound();
        }

        return View(ogrenciler);
    }

    // GET: OGRENCILERS/Create
    public IActionResult Create()
    {
        ViewData["AnaBolumId"] = new SelectList(_context.Bolumlers, "BolumId", "BolumAdi");
        return View();
    }

    // POST: OGRENCILERS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("OgrenciId,OgrenciNo,Ad,Soyad,GenelNotOrtalamasi,AnaBolumId,BasvuruTuru")] Ogrenciler ogrenciler, string OgrenciSifre)
    {
        if (ModelState.IsValid)
        {
            ogrenciler.BasvuruAcikMi = true;
            // 1. Öğrenciyi (Başvuru Türü dahil) Ogrenciler tablosuna ekle
            _context.Add(ogrenciler);

            // 2. Aynı öğrenci için Kullanicilar tablosunda giriş hesabı oluştur
            var mevcutKullanici = await _context.Kullanicilars.FirstOrDefaultAsync(k => k.KullaniciAdi == ogrenciler.OgrenciNo);
            if (mevcutKullanici == null)
            {
                var yeniKullanici = new Kullanicilar
                {
                    KullaniciAdi = ogrenciler.OgrenciNo,
                    Sifre = OgrenciSifre,
                    Rol = "Ogrenci"
                };
                _context.Kullanicilars.Add(yeniKullanici);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        ViewData["AnaBolumId"] = new SelectList(_context.Bolumlers, "BolumId", "BolumAdi", ogrenciler.AnaBolumId);
        return View(ogrenciler);
    }

    // GET: OGRENCILERS/Edit/5
    public async Task<IActionResult> Edit(int? ogrenciid)
    {
        if (ogrenciid == null)
        {
            return NotFound();
        }

        var ogrenciler = await _context.Ogrencilers.FindAsync(ogrenciid);
        if (ogrenciler == null)
        {
            return NotFound();
        }
        return View(ogrenciler);
    }

    // POST: OGRENCILERS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? ogrenciid, [Bind("OgrenciId,OgrenciNo,Ad,Soyad,GenelNotOrtalamasi,AnaBolumId,AnaBolum,Basvurulars")] Ogrenciler ogrenciler)
    {
        if (ogrenciid != ogrenciler.OgrenciId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(ogrenciler);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OgrencilerExists(ogrenciler.OgrenciId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(ogrenciler);
    }

    // GET: OGRENCILERS/Delete/5
    public async Task<IActionResult> Delete(int? ogrenciid)
    {
        if (ogrenciid == null)
        {
            return NotFound();
        }

        var ogrenciler = await _context.Ogrencilers
            .FirstOrDefaultAsync(m => m.OgrenciId == ogrenciid);
        if (ogrenciler == null)
        {
            return NotFound();
        }

        return View(ogrenciler);
    }

    // POST: OGRENCILERS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? ogrenciid)
    {
        var ogrenciler = await _context.Ogrencilers.FindAsync(ogrenciid);
        if (ogrenciler != null)
        {
            _context.Ogrencilers.Remove(ogrenciler);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // POST: Ogrenciler/YeniBasvuruAc/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> YeniBasvuruAc(int id, string? yeniBasvuruTuru)
    {
        var ogrenci = await _context.Ogrencilers.FindAsync(id);
        if (ogrenci != null)
        {
            ogrenci.BasvuruAcikMi = true;
            if (!string.IsNullOrEmpty(yeniBasvuruTuru))
            {
                ogrenci.BasvuruTuru = yeniBasvuruTuru;
            }
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    private bool OgrencilerExists(int? ogrenciid)
    {
        return _context.Ogrencilers.Any(e => e.OgrenciId == ogrenciid);
    }
}
