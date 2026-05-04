using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestoranProjesi.Migrations
{
    /// <inheritdoc />
    public partial class AddTableNumberToOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TableNumber",
                table: "siparisler",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TableNumber",
                table: "siparisler");
        }
    }
}
