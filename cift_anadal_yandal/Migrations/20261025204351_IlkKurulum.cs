using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cift_anadal_yandal.Migrations
{
    /// <inheritdoc />
    public partial class IlkKurulum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Bolumler",
                columns: table => new
                {
                    BolumID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BolumAdi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FakulteAdi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CiftAnadalKontenjani = table.Column<int>(type: "int", nullable: false),
                    YandalKontenjani = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Bolumler__7BAD4B5CCF5AAB82", x => x.BolumID);
                });

            migrationBuilder.CreateTable(
                name: "Dersler",
                columns: table => new
                {
                    DersId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DersKodu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DersAdi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Akts = table.Column<int>(type: "int", nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dersler", x => x.DersId);
                });

            migrationBuilder.CreateTable(
                name: "Kullanicilar",
                columns: table => new
                {
                    KullaniciID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KullaniciAdi = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Sifre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Rol = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Kullanic__E011F09B7C17509D", x => x.KullaniciID);
                });

            migrationBuilder.CreateTable(
                name: "Ogrenciler",
                columns: table => new
                {
                    OgrenciID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OgrenciNo = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    Ad = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Soyad = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    GenelNotOrtalamasi = table.Column<decimal>(type: "decimal(3,2)", nullable: false),
                    AnaBolumID = table.Column<int>(type: "int", nullable: true),
                    BasvuruTuru = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BasvuruAcikMi = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Ogrencil__E497E6D45A485DD3", x => x.OgrenciID);
                    table.ForeignKey(
                        name: "FK__Ogrencile__AnaBo__4D94879B",
                        column: x => x.AnaBolumID,
                        principalTable: "Bolumler",
                        principalColumn: "BolumID");
                });

            migrationBuilder.CreateTable(
                name: "Basvurular",
                columns: table => new
                {
                    BasvuruID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OgrenciID = table.Column<int>(type: "int", nullable: true),
                    HedefBolumID = table.Column<int>(type: "int", nullable: true),
                    BasvuruTuru = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    BasvuruTarihi = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    Durum = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "Bekliyor"),
                    TranskriptYolu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProtokolFormuYolu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DersKayitFormuYolu = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Basvurul__5704D2A8E8403CE8", x => x.BasvuruID);
                    table.ForeignKey(
                        name: "FK__Basvurula__Hedef__5165187F",
                        column: x => x.HedefBolumID,
                        principalTable: "Bolumler",
                        principalColumn: "BolumID");
                    table.ForeignKey(
                        name: "FK__Basvurula__Ogren__5070F446",
                        column: x => x.OgrenciID,
                        principalTable: "Ogrenciler",
                        principalColumn: "OgrenciID");
                });

            migrationBuilder.CreateTable(
                name: "Mesajlar",
                columns: table => new
                {
                    MesajId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OgrenciId = table.Column<int>(type: "int", nullable: false),
                    GonderenRol = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GonderenAd = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MesajMetni = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GonderimTarihi = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mesajlar", x => x.MesajId);
                    table.ForeignKey(
                        name: "FK_Mesajlar_Ogrenciler_OgrenciId",
                        column: x => x.OgrenciId,
                        principalTable: "Ogrenciler",
                        principalColumn: "OgrenciID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OgrenciDersSecimleri",
                columns: table => new
                {
                    SecimId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OgrenciId = table.Column<int>(type: "int", nullable: false),
                    DersId = table.Column<int>(type: "int", nullable: false),
                    SecimTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Durum = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OgrenciDersSecimleri", x => x.SecimId);
                    table.ForeignKey(
                        name: "FK_OgrenciDersSecimleri_Dersler_DersId",
                        column: x => x.DersId,
                        principalTable: "Dersler",
                        principalColumn: "DersId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OgrenciDersSecimleri_Ogrenciler_OgrenciId",
                        column: x => x.OgrenciId,
                        principalTable: "Ogrenciler",
                        principalColumn: "OgrenciID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Basvurular_HedefBolumID",
                table: "Basvurular",
                column: "HedefBolumID");

            migrationBuilder.CreateIndex(
                name: "IX_Basvurular_OgrenciID",
                table: "Basvurular",
                column: "OgrenciID");

            migrationBuilder.CreateIndex(
                name: "UQ__Kullanic__5BAE6A750C213A9D",
                table: "Kullanicilar",
                column: "KullaniciAdi",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Mesajlar_OgrenciId",
                table: "Mesajlar",
                column: "OgrenciId");

            migrationBuilder.CreateIndex(
                name: "IX_OgrenciDersSecimleri_DersId",
                table: "OgrenciDersSecimleri",
                column: "DersId");

            migrationBuilder.CreateIndex(
                name: "IX_OgrenciDersSecimleri_OgrenciId",
                table: "OgrenciDersSecimleri",
                column: "OgrenciId");

            migrationBuilder.CreateIndex(
                name: "IX_Ogrenciler_AnaBolumID",
                table: "Ogrenciler",
                column: "AnaBolumID");

            migrationBuilder.CreateIndex(
                name: "UQ__Ogrencil__E497FE1A89071198",
                table: "Ogrenciler",
                column: "OgrenciNo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Basvurular");

            migrationBuilder.DropTable(
                name: "Kullanicilar");

            migrationBuilder.DropTable(
                name: "Mesajlar");

            migrationBuilder.DropTable(
                name: "OgrenciDersSecimleri");

            migrationBuilder.DropTable(
                name: "Dersler");

            migrationBuilder.DropTable(
                name: "Ogrenciler");

            migrationBuilder.DropTable(
                name: "Bolumler");
        }
    }
}
