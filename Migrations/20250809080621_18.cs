using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebMVC.Migrations
{
    /// <inheritdoc />
    public partial class _18 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccountArrivedAtTQWarehouse",
                table: "Transportations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AccountArrivedAtVNWarehouse",
                table: "Transportations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AccountCompleted",
                table: "Transportations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AccountCustomsInspectedGoods",
                table: "Transportations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AccountExitedFromTQWarehouse",
                table: "Transportations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AccountReturningToVNWarehouse",
                table: "Transportations",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountArrivedAtTQWarehouse",
                table: "Transportations");

            migrationBuilder.DropColumn(
                name: "AccountArrivedAtVNWarehouse",
                table: "Transportations");

            migrationBuilder.DropColumn(
                name: "AccountCompleted",
                table: "Transportations");

            migrationBuilder.DropColumn(
                name: "AccountCustomsInspectedGoods",
                table: "Transportations");

            migrationBuilder.DropColumn(
                name: "AccountExitedFromTQWarehouse",
                table: "Transportations");

            migrationBuilder.DropColumn(
                name: "AccountReturningToVNWarehouse",
                table: "Transportations");
        }
    }
}
