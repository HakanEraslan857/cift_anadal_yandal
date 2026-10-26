using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace cift_anadal_yandal.Models;

public partial class Dersler
{
    [Key]
    public int DersId { get; set; }
    public string? DersKodu { get; set; }
    public string DersAdi { get; set; } = null!;
    public int Akts { get; set; }
    public string? Aciklama { get; set; }
}