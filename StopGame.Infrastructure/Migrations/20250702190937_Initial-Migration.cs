using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace StopGame.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Topics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDefault = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Topics", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "Topics",
                columns: new[] { "Id", "CreatedAt", "CreatedByUserId", "IsDefault", "Name" },
                values: new object[,]
                {
                    { new Guid("063e535f-e823-4eb6-a760-3975adc25e22"), new DateTime(2025, 7, 2, 19, 9, 37, 568, DateTimeKind.Utc).AddTicks(4877), null, true, "Sport" },
                    { new Guid("24ced846-73f1-409c-8464-6349fedc2ad7"), new DateTime(2025, 7, 2, 19, 9, 37, 568, DateTimeKind.Utc).AddTicks(4865), null, true, "City" },
                    { new Guid("268ff43f-c261-4b7d-9784-0eab83c4ef0c"), new DateTime(2025, 7, 2, 19, 9, 37, 568, DateTimeKind.Utc).AddTicks(4867), null, true, "Color" },
                    { new Guid("382c98fd-7f3b-4935-be94-5f513a8abc1e"), new DateTime(2025, 7, 2, 19, 9, 37, 568, DateTimeKind.Utc).AddTicks(4869), null, true, "Name" },
                    { new Guid("7234761e-0fab-4895-9622-5de653ab6835"), new DateTime(2025, 7, 2, 19, 9, 37, 568, DateTimeKind.Utc).AddTicks(4870), null, true, "Profession" },
                    { new Guid("7ee6b65e-4416-423b-aa6c-e8bda86a6369"), new DateTime(2025, 7, 2, 19, 9, 37, 568, DateTimeKind.Utc).AddTicks(4875), null, true, "Movie" },
                    { new Guid("a8db7a67-491b-41ac-944d-6776627bfcb1"), new DateTime(2025, 7, 2, 19, 9, 37, 568, DateTimeKind.Utc).AddTicks(4864), null, true, "Country" },
                    { new Guid("c2040d1a-f439-4a05-b656-b606bf73cc80"), new DateTime(2025, 7, 2, 19, 9, 37, 568, DateTimeKind.Utc).AddTicks(4876), null, true, "Brand" },
                    { new Guid("d9795da9-b89e-4235-a6a8-fbd5f61ff09c"), new DateTime(2025, 7, 2, 19, 9, 37, 568, DateTimeKind.Utc).AddTicks(4866), null, true, "Food" },
                    { new Guid("fe8e1285-e936-481b-b7ea-95b57b709a33"), new DateTime(2025, 7, 2, 19, 9, 37, 568, DateTimeKind.Utc).AddTicks(4861), null, true, "Animal" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Topics_Name",
                table: "Topics",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Topics");
        }
    }
}
