using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebMVC.Migrations
{
    /// <inheritdoc />
    public partial class _9 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserUploadDimensions",
                table: "Transportations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserUploadImage",
                table: "Transportations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserUploadName",
                table: "Transportations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserUploadOtherInfor",
                table: "Transportations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserUploadQuantity",
                table: "Transportations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "UserUploadVolume",
                table: "Transportations",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "UserUploadWeight",
                table: "Transportations",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SpecialShipId",
                table: "Accounts",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserUploadDimensions",
                table: "Transportations");

            migrationBuilder.DropColumn(
                name: "UserUploadImage",
                table: "Transportations");

            migrationBuilder.DropColumn(
                name: "UserUploadName",
                table: "Transportations");

            migrationBuilder.DropColumn(
                name: "UserUploadOtherInfor",
                table: "Transportations");

            migrationBuilder.DropColumn(
                name: "UserUploadQuantity",
                table: "Transportations");

            migrationBuilder.DropColumn(
                name: "UserUploadVolume",
                table: "Transportations");

            migrationBuilder.DropColumn(
                name: "UserUploadWeight",
                table: "Transportations");

            migrationBuilder.DropColumn(
                name: "SpecialShipId",
                table: "Accounts");
        }
    }
}
