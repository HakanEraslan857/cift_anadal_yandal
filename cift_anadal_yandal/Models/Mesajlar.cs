using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace cift_anadal_yandal.Models;

public partial class Mesajlar
{
    [Key]
    public int MesajId { get; set; }

    public int OgrenciId { get; set; }

    public string GonderenRol { get; set; } = null!;

    public string GonderenAd { get; set; } = null!;

    public string MesajMetni { get; set; } = null!;

    public DateTime GonderimTarihi { get; set; }

    [ForeignKey("OgrenciId")]
    public virtual Ogrenciler? Ogrenci { get; set; }
}