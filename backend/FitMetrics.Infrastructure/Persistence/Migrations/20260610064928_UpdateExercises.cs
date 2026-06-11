using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FitMetrics.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateExercises : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "Bench Press (Barbell)");

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "Push-Up (Şınav)");

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 4,
                column: "Name",
                value: "Pull-Up (Barfiks)");

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 6,
                column: "Name",
                value: "Bent Over Row (Barbell)");

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 7,
                column: "Name",
                value: "Deadlift (Barbell)");

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 8,
                column: "Name",
                value: "Squat (Barbell)");

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 10,
                column: "Name",
                value: "Lunge (Dumbbell)");

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 11,
                column: "Name",
                value: "Lying Leg Curl");

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 12,
                column: "Name",
                value: "Overhead Press (Military Press)");

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 13,
                column: "Name",
                value: "Dumbbell Lateral Raise");

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 14,
                column: "Name",
                value: "Barbell Curl");

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 15,
                column: "Name",
                value: "Triceps Pushdown (Kablo)");

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 17,
                column: "Name",
                value: "Crunch (Mekik)");

            migrationBuilder.InsertData(
                table: "Exercises",
                columns: new[] { "Id", "CaloriesBurnedPerMinute", "Category", "CreatedAt", "MuscleGroup", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { 25, 5.5, 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "Chest Fly (Dumbbell / Cable)", null },
                    { 26, 7.0, 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "Dips (Göğüs Odaklı)", null },
                    { 27, 6.0, 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "Decline Press (Ters Eğimli)", null },
                    { 28, 6.0, 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "Seated Cable Row", null },
                    { 29, 6.5, 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "T-Bar Row", null },
                    { 30, 5.5, 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 5, "Dumbbell Shoulder Press", null },
                    { 31, 4.0, 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 5, "Front Raise (Dumbbell / Barbell)", null },
                    { 32, 4.5, 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 5, "Face Pull (Kablo)", null },
                    { 33, 4.0, 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 5, "Reverse Fly (Dumbbell / Makine)", null },
                    { 34, 7.5, 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, "Romanian Deadlift (RDL)", null },
                    { 35, 5.5, 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, "Leg Extension", null },
                    { 36, 4.0, 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 6, "Hammer Curl (Dumbbell)", null },
                    { 37, 4.0, 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 6, "Preacher Curl", null },
                    { 38, 4.5, 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 6, "Skull Crusher", null },
                    { 39, 4.0, 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 6, "Overhead Triceps Extension (Dumbbell)", null },
                    { 40, 5.0, 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 7, "Hanging Leg Raise", null },
                    { 41, 4.5, 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 7, "Russian Twist", null },
                    { 42, 4.0, 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 7, "Cable Crunch", null },
                    { 43, 5.0, 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 7, "Ab Wheel Rollout", null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 33);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 34);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 35);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 36);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 37);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 38);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 39);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 40);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 41);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 42);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 43);

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "Bench Press");

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "Şınav (Push-up)");

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 4,
                column: "Name",
                value: "Barfiks (Pull-up)");

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 6,
                column: "Name",
                value: "Barbell Row");

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 7,
                column: "Name",
                value: "Deadlift");

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 8,
                column: "Name",
                value: "Squat");

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 10,
                column: "Name",
                value: "Lunge");

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 11,
                column: "Name",
                value: "Leg Curl");

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 12,
                column: "Name",
                value: "Overhead Press");

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 13,
                column: "Name",
                value: "Lateral Raise");

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 14,
                column: "Name",
                value: "Biceps Curl");

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 15,
                column: "Name",
                value: "Triceps Pushdown");

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 17,
                column: "Name",
                value: "Mekik (Crunch)");
        }
    }
}
