using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace cift_anadal_yandal.Models;

public partial class CiftAnadalYandalDbContext : DbContext
{
    public CiftAnadalYandalDbContext()
    {
    }

    public CiftAnadalYandalDbContext(DbContextOptions<CiftAnadalYandalDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Basvurular> Basvurulars { get; set; }

    public virtual DbSet<Bolumler> Bolumlers { get; set; }

    public virtual DbSet<Kullanicilar> Kullanicilars { get; set; }

    public virtual DbSet<Ogrenciler> Ogrencilers { get; set; }

    public virtual DbSet<Dersler> Derslers { get; set; }

    public virtual DbSet<OgrenciDersSecimleri> OgrenciDersSecimleris { get; set; }

    public virtual DbSet<Mesajlar> Mesajlars { get; set; }

//    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
//        => optionsBuilder.UseSqlServer("Server=.\\EXPRESS_SQL;Database=CiftAnadalYandalDB;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Basvurular>(entity =>
        {
            entity.HasKey(e => e.BasvuruId).HasName("PK__Basvurul__5704D2A8E8403CE8");

            entity.ToTable("Basvurular");

            entity.Property(e => e.BasvuruId).HasColumnName("BasvuruID");
            entity.Property(e => e.BasvuruTarihi)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.BasvuruTuru).HasMaxLength(20);
            entity.Property(e => e.Durum)
                .HasMaxLength(20)
                .HasDefaultValue("Bekliyor");
            entity.Property(e => e.HedefBolumId).HasColumnName("HedefBolumID");
            entity.Property(e => e.OgrenciId).HasColumnName("OgrenciID");

            entity.HasOne(d => d.HedefBolum).WithMany(p => p.Basvurulars)
                .HasForeignKey(d => d.HedefBolumId)
                .HasConstraintName("FK__Basvurula__Hedef__5165187F");

            entity.HasOne(d => d.Ogrenci).WithMany(p => p.Basvurulars)
                .HasForeignKey(d => d.OgrenciId)
                .HasConstraintName("FK__Basvurula__Ogren__5070F446");
        });

        modelBuilder.Entity<Bolumler>(entity =>
        {
            entity.HasKey(e => e.BolumId).HasName("PK__Bolumler__7BAD4B5CCF5AAB82");

            entity.ToTable("Bolumler");

            entity.Property(e => e.BolumId).HasColumnName("BolumID");
            entity.Property(e => e.BolumAdi).HasMaxLength(100);
            entity.Property(e => e.FakulteAdi).HasMaxLength(100);
        });

        modelBuilder.Entity<Kullanicilar>(entity =>
        {
            entity.HasKey(e => e.KullaniciId).HasName("PK__Kullanic__E011F09B7C17509D");

            entity.ToTable("Kullanicilar");

            entity.HasIndex(e => e.KullaniciAdi, "UQ__Kullanic__5BAE6A750C213A9D").IsUnique();

            entity.Property(e => e.KullaniciId).HasColumnName("KullaniciID");
            entity.Property(e => e.KullaniciAdi).HasMaxLength(50);
            entity.Property(e => e.Rol).HasMaxLength(20);
            entity.Property(e => e.Sifre).HasMaxLength(50);
        });

        modelBuilder.Entity<Ogrenciler>(entity =>
        {
            entity.HasKey(e => e.OgrenciId).HasName("PK__Ogrencil__E497E6D45A485DD3");

            entity.ToTable("Ogrenciler");

            entity.HasIndex(e => e.OgrenciNo, "UQ__Ogrencil__E497FE1A89071198").IsUnique();

            entity.Property(e => e.OgrenciId).HasColumnName("OgrenciID");
            entity.Property(e => e.Ad).HasMaxLength(50);
            entity.Property(e => e.AnaBolumId).HasColumnName("AnaBolumID");
            entity.Property(e => e.GenelNotOrtalamasi).HasColumnType("decimal(3, 2)");
            entity.Property(e => e.OgrenciNo).HasMaxLength(15);
            entity.Property(e => e.Soyad).HasMaxLength(50);

            entity.HasOne(d => d.AnaBolum).WithMany(p => p.Ogrencilers)
                .HasForeignKey(d => d.AnaBolumId)
                .HasConstraintName("FK__Ogrencile__AnaBo__4D94879B");
        });
        modelBuilder.Entity<Dersler>(entity =>
        {
            entity.HasKey(e => e.DersId);
            entity.ToTable("Dersler");
        });

        modelBuilder.Entity<OgrenciDersSecimleri>(entity =>
        {
            entity.HasKey(e => e.SecimId);
            entity.ToTable("OgrenciDersSecimleri");
        });

        modelBuilder.Entity<Mesajlar>(entity =>
        {
            entity.HasKey(e => e.MesajId);
            entity.ToTable("Mesajlar");
            entity.Property(e => e.GonderimTarihi).HasColumnType("datetime");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
