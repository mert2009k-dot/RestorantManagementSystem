using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestoranProjesi.Migrations
{
    /// <inheritdoc />
    public partial class AddUserSoftDeleteProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "musteribilgi",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsRestored",
                table: "musteribilgi",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "musteribilgi");

            migrationBuilder.DropColumn(
                name: "IsRestored",
                table: "musteribilgi");
        }
    }
}
