using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitMetrics.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDietitianRoleAndClients : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Role",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "DietitianClients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DietitianId = table.Column<int>(type: "int", nullable: false),
                    ClientId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DietitianClients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DietitianClients_Users_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DietitianClients_Users_DietitianId",
                        column: x => x.DietitianId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DietitianClients_ClientId",
                table: "DietitianClients",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_DietitianClients_DietitianId_ClientId",
                table: "DietitianClients",
                columns: new[] { "DietitianId", "ClientId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DietitianClients");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "Users");
        }
    }
}
