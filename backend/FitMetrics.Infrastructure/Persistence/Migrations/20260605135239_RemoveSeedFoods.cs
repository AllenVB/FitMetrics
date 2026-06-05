using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FitMetrics.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSeedFoods : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Foods",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Foods",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Foods",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Foods",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Foods",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Foods",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Foods",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Foods",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Foods",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Foods",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Foods",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Foods",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Foods",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Foods",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Foods",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Foods",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Foods",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Foods",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "Foods",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "Foods",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "Foods",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "Foods",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "Foods",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "Foods",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "Foods",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "Foods",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "Foods",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "Foods",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "Foods",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "Foods",
                keyColumn: "Id",
                keyValue: 30);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Foods",
                columns: new[] { "Id", "Brand", "CaloriesPer100g", "CarbsPer100g", "Category", "CreatedAt", "FatPer100g", "Name", "ProteinPer100g", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, null, 165.0, 0.0, "Protein", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3.6000000000000001, "Tavuk Göğsü (Izgara)", 31.0, null },
                    { 2, null, 155.0, 1.1000000000000001, "Protein", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 11.0, "Yumurta (Tam)", 13.0, null },
                    { 3, null, 389.0, 66.0, "Tahıl", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 6.9000000000000004, "Yulaf Ezmesi", 16.899999999999999, null },
                    { 4, null, 130.0, 28.0, "Tahıl", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0.29999999999999999, "Beyaz Pirinç (Pişmiş)", 2.7000000000000002, null },
                    { 5, null, 247.0, 41.0, "Tahıl", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3.3999999999999999, "Tam Buğday Ekmeği", 13.0, null },
                    { 6, null, 89.0, 23.0, "Meyve", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0.29999999999999999, "Muz", 1.1000000000000001, null },
                    { 7, null, 52.0, 14.0, "Meyve", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0.20000000000000001, "Elma", 0.29999999999999999, null },
                    { 8, null, 579.0, 22.0, "Kuruyemiş", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 50.0, "Badem", 21.0, null },
                    { 9, null, 654.0, 14.0, "Kuruyemiş", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 65.0, "Ceviz", 15.0, null },
                    { 10, null, 50.0, 5.0, "Süt Ürünü", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1.8, "Süt (Yarım Yağlı)", 3.3999999999999999, null },
                    { 11, null, 61.0, 4.7000000000000002, "Süt Ürünü", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3.2999999999999998, "Yoğurt (Tam Yağlı)", 3.5, null },
                    { 12, null, 98.0, 3.3999999999999999, "Süt Ürünü", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4.2999999999999998, "Lor Peyniri", 11.0, null },
                    { 13, null, 208.0, 0.0, "Protein", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 13.0, "Somon (Izgara)", 20.0, null },
                    { 14, null, 116.0, 0.0, "Protein", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1.0, "Ton Balığı (Suda)", 26.0, null },
                    { 15, null, 250.0, 0.0, "Protein", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 15.0, "Dana Eti (Yağsız)", 26.0, null },
                    { 16, null, 116.0, 20.0, "Baklagil", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0.40000000000000002, "Mercimek (Pişmiş)", 9.0, null },
                    { 17, null, 164.0, 27.0, "Baklagil", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2.6000000000000001, "Nohut (Pişmiş)", 8.9000000000000004, null },
                    { 18, null, 34.0, 7.0, "Sebze", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0.40000000000000002, "Brokoli", 2.7999999999999998, null },
                    { 19, null, 87.0, 20.0, "Sebze", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0.10000000000000001, "Patates (Haşlanmış)", 1.8999999999999999, null },
                    { 20, null, 86.0, 20.0, "Sebze", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0.10000000000000001, "Tatlı Patates", 1.6000000000000001, null },
                    { 21, null, 400.0, 8.0, "Takviye", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 6.0, "Whey Protein Tozu", 80.0, null },
                    { 22, null, 884.0, 0.0, "Yağ", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 100.0, "Zeytinyağı", 0.0, null },
                    { 23, null, 160.0, 9.0, "Meyve", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 15.0, "Avokado", 2.0, null },
                    { 24, null, 304.0, 82.0, "Tatlandırıcı", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0.0, "Bal", 0.29999999999999999, null },
                    { 25, null, 131.0, 25.0, "Tahıl", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1.1000000000000001, "Makarna (Pişmiş)", 5.0, null },
                    { 26, null, 209.0, 0.0, "Protein", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 11.0, "Tavuk But", 26.0, null },
                    { 27, null, 264.0, 2.0, "Süt Ürünü", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 21.0, "Beyaz Peynir", 17.0, null },
                    { 28, null, 83.0, 19.0, "Tahıl", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0.20000000000000001, "Bulgur (Pişmiş)", 3.0, null },
                    { 29, null, 135.0, 0.0, "Protein", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1.0, "Hindi Göğsü", 30.0, null },
                    { 30, null, 546.0, 61.0, "Atıştırmalık", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 31.0, "Bitter Çikolata", 4.9000000000000004, null }
                });
        }
    }
}
