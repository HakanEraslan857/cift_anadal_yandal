using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace cift_anadal_yandal.Models;

public partial class OgrenciDersSecimleri
{
    [Key]
    public int SecimId { get; set; }
    public int OgrenciId { get; set; }
    public int DersId { get; set; }
    public DateTime? SecimTarihi { get; set; }
    public string? Durum { get; set; }

    [ForeignKey("OgrenciId")]
    public virtual Ogrenciler? Ogrenci { get; set; }

    [ForeignKey("DersId")]
    public virtual Dersler? Ders { get; set; }
}