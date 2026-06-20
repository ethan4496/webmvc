using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebMVC.Migrations
{
    /// <inheritdoc />
    public partial class _22 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "TotalPriceVNDCN",
                table: "StaffTargets",
                type: "decimal(18,0)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalPriceVNDDT",
                table: "StaffTargets",
                type: "decimal(18,0)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalPriceVNDHT",
                table: "StaffTargets",
                type: "decimal(18,0)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalPriceVNDCN",
                table: "StaffTargets");

            migrationBuilder.DropColumn(
                name: "TotalPriceVNDDT",
                table: "StaffTargets");

            migrationBuilder.DropColumn(
                name: "TotalPriceVNDHT",
                table: "StaffTargets");
        }
    }
}
