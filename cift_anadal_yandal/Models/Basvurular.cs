using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace cift_anadal_yandal.Models;

public partial class Basvurular
{
    [Key]
    public int BasvuruId { get; set; }

    public int? OgrenciId { get; set; }

    [Display(Name = "Öğrenci Adı")]
    public virtual Ogrenciler? Ogrenci { get; set; }

    [Display(Name = "Hedef Bölüm ID")]
    public int? HedefBolumId { get; set; }

    [Display(Name = "Hedef Bölüm")]
    public virtual Bolumler? HedefBolum { get; set; }

    [Display(Name = "Başvuru Türü")]
    public string BasvuruTuru { get; set; } = null!;

    [Display(Name = "Başvuru Tarihi")]
    public DateTime? BasvuruTarihi { get; set; }

    [Display(Name = "Başvuru Durumu")]
    public string? Durum { get; set; }

    public string? TranskriptYolu { get; set; }

    public string? ProtokolFormuYolu { get; set; }

    public string? DersKayitFormuYolu { get; set; }
}
