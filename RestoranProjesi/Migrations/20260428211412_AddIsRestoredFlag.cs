using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestoranProjesi.Migrations
{
    /// <inheritdoc />
    public partial class AddIsRestoredFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRestored",
                table: "siparisler",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRestored",
                table: "siparisler");
        }
    }
}
