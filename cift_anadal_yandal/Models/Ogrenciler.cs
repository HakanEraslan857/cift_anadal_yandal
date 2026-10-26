using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace cift_anadal_yandal.Models;

public partial class Ogrenciler
{
    [Key]
    public int OgrenciId { get; set; }

    [Display(Name ="Öğrenci Numarası")]
    public string OgrenciNo { get; set; } = null!;

    [Display(Name = "Ad")]
    public string Ad { get; set; } = null!;

    [Display(Name = "Soyad")]
    public string Soyad { get; set; } = null!;

    [Display(Name = "GANO")]
    public decimal GenelNotOrtalamasi { get; set; }

    [Display(Name = "Ana Bölüm ID")]
    public int? AnaBolumId { get; set; }


    public string? BasvuruTuru { get; set; }

    public bool BasvuruAcikMi { get; set; } = true;

    [Display(Name = "Ana Bölüm")]
    public virtual Bolumler? AnaBolum { get; set; }

    public virtual ICollection<Basvurular> Basvurulars { get; set; } = new List<Basvurular>();
}
