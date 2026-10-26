using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace cift_anadal_yandal.Models;

public partial class Kullanicilar
{
    [Key]
    public int KullaniciId { get; set; }

    [Display(Name = "Kullanıcı Adı")]
    public string KullaniciAdi { get; set; } = null!;

    [Display(Name = "Şifre")]
    public string Sifre { get; set; } = null!;

    [Display(Name = "Rol")]
    public string Rol { get; set; } = null!;
}
