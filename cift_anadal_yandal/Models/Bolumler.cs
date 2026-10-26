using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace cift_anadal_yandal.Models;

public partial class Bolumler
{
    [Key]
    public int BolumId { get; set; }

    [Display(Name = "Bölüm Adı")]
    public string BolumAdi { get; set; } = null!;

    [Display(Name = "Fakülte Adı")]
    public string FakulteAdi { get; set; } = null!;

    [Display(Name = "Çift Anadal Kontenjanı")]
    public int CiftAnadalKontenjani { get; set; }

    [Display(Name = "Yandal Kontenjanı")]
    public int YandalKontenjani { get; set; }

    public virtual ICollection<Basvurular> Basvurulars { get; set; } = new List<Basvurular>();

    public virtual ICollection<Ogrenciler> Ogrencilers { get; set; } = new List<Ogrenciler>();
}
