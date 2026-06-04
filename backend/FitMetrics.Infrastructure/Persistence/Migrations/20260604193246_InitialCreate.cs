using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FitMetrics.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Exercises",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Category = table.Column<int>(type: "int", nullable: false),
                    MuscleGroup = table.Column<int>(type: "int", nullable: false),
                    CaloriesBurnedPerMinute = table.Column<double>(type: "float", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exercises", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Foods",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Brand = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Category = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    CaloriesPer100g = table.Column<double>(type: "float", nullable: false),
                    ProteinPer100g = table.Column<double>(type: "float", nullable: false),
                    CarbsPer100g = table.Column<double>(type: "float", nullable: false),
                    FatPer100g = table.Column<double>(type: "float", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Foods", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Age = table.Column<int>(type: "int", nullable: false),
                    Gender = table.Column<int>(type: "int", nullable: false),
                    HeightCm = table.Column<double>(type: "float", nullable: false),
                    CurrentWeightKg = table.Column<double>(type: "float", nullable: false),
                    ActivityLevel = table.Column<int>(type: "int", nullable: false),
                    GoalType = table.Column<int>(type: "int", nullable: false),
                    TargetWeightKg = table.Column<double>(type: "float", nullable: true),
                    DailyCalorieGoal = table.Column<int>(type: "int", nullable: false),
                    DailyProteinGoal = table.Column<int>(type: "int", nullable: false),
                    DailyWaterGoalMl = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NutritionLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    FoodId = table.Column<int>(type: "int", nullable: false),
                    AmountGrams = table.Column<double>(type: "float", nullable: false),
                    MealType = table.Column<int>(type: "int", nullable: false),
                    LoggedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NutritionLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NutritionLogs_Foods_FoodId",
                        column: x => x.FoodId,
                        principalTable: "Foods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NutritionLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WeightEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    WeightKg = table.Column<double>(type: "float", nullable: false),
                    BodyFatPercentage = table.Column<double>(type: "float", nullable: true),
                    RecordedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeightEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WeightEntries_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkoutLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ExerciseId = table.Column<int>(type: "int", nullable: false),
                    DurationMinutes = table.Column<int>(type: "int", nullable: true),
                    Sets = table.Column<int>(type: "int", nullable: true),
                    Reps = table.Column<int>(type: "int", nullable: true),
                    WeightKg = table.Column<double>(type: "float", nullable: true),
                    CaloriesBurned = table.Column<double>(type: "float", nullable: false),
                    PerformedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkoutLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkoutLogs_Exercises_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "Exercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkoutLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Exercises",
                columns: new[] { "Id", "CaloriesBurnedPerMinute", "Category", "CreatedAt", "MuscleGroup", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 6.0, 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "Bench Press", null },
                    { 2, 6.0, 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "Incline Dumbbell Press", null },
                    { 3, 7.0, 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "Şınav (Push-up)", null },
                    { 4, 8.0, 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "Barfiks (Pull-up)", null },
                    { 5, 6.0, 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "Lat Pulldown", null },
                    { 6, 6.5, 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "Barbell Row", null },
                    { 7, 9.0, 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "Deadlift", null },
                    { 8, 8.0, 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, "Squat", null },
                    { 9, 7.0, 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, "Leg Press", null },
                    { 10, 7.0, 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, "Lunge", null },
                    { 11, 5.5, 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, "Leg Curl", null },
                    { 12, 6.0, 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 5, "Overhead Press", null },
                    { 13, 4.5, 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 5, "Lateral Raise", null },
                    { 14, 4.0, 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 6, "Biceps Curl", null },
                    { 15, 4.0, 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 6, "Triceps Pushdown", null },
                    { 16, 4.0, 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 7, "Plank", null },
                    { 17, 4.5, 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 7, "Mekik (Crunch)", null },
                    { 18, 11.0, 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 8, "Koşu", null },
                    { 19, 9.0, 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 8, "Bisiklet", null },
                    { 20, 10.0, 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 8, "Kürek (Rowing)", null },
                    { 21, 12.0, 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 8, "İp Atlama", null },
                    { 22, 5.0, 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 8, "Yürüyüş", null },
                    { 23, 10.0, 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 8, "Yüzme", null },
                    { 24, 8.0, 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 8, "Eliptik", null }
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_Name",
                table: "Exercises",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Foods_Name",
                table: "Foods",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_NutritionLogs_FoodId",
                table: "NutritionLogs",
                column: "FoodId");

            migrationBuilder.CreateIndex(
                name: "IX_NutritionLogs_UserId_LoggedAt",
                table: "NutritionLogs",
                columns: new[] { "UserId", "LoggedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WeightEntries_UserId_RecordedAt",
                table: "WeightEntries",
                columns: new[] { "UserId", "RecordedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutLogs_ExerciseId",
                table: "WorkoutLogs",
                column: "ExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutLogs_UserId_PerformedAt",
                table: "WorkoutLogs",
                columns: new[] { "UserId", "PerformedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NutritionLogs");

            migrationBuilder.DropTable(
                name: "WeightEntries");

            migrationBuilder.DropTable(
                name: "WorkoutLogs");

            migrationBuilder.DropTable(
                name: "Foods");

            migrationBuilder.DropTable(
                name: "Exercises");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
