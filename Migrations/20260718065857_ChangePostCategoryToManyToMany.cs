using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebMVC.Migrations
{
    /// <inheritdoc />
    public partial class ChangePostCategoryToManyToMany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PostCategories",
                columns: table => new
                {
                    PostId = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostCategories", x => new { x.PostId, x.CategoryId });
                    table.ForeignKey(
                        name: "FK_PostCategories_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PostCategories_Posts_PostId",
                        column: x => x.PostId,
                        principalTable: "Posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PostCategories_CategoryId",
                table: "PostCategories",
                column: "CategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Campaigns_EmailTemplates_TemplateId",
                table: "Campaigns");

            migrationBuilder.DropForeignKey(
                name: "FK_ContactListContacts_ContactLists_ContactListId",
                table: "ContactListContacts");

            migrationBuilder.DropTable(
                name: "AccountSignatures");

            migrationBuilder.DropTable(
                name: "MailLogs");

            migrationBuilder.DropTable(
                name: "PostCategories");

            migrationBuilder.DropIndex(
                name: "IX_Campaigns_TemplateId",
                table: "Campaigns");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ContactLists",
                table: "ContactLists");

            migrationBuilder.DropColumn(
                name: "EmailSent",
                table: "Campaigns");

            migrationBuilder.RenameTable(
                name: "ContactLists",
                newName: "ContacLists");

            migrationBuilder.RenameColumn(
                name: "TemplateId",
                table: "Campaigns",
                newName: "TotalRecipients");

            migrationBuilder.RenameColumn(
                name: "SendAt",
                table: "Campaigns",
                newName: "SentAt");

            migrationBuilder.RenameColumn(
                name: "ContactId",
                table: "Campaigns",
                newName: "EmailTemplateId");

            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "Posts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ContacLists",
                table: "ContacLists",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_CategoryId",
                table: "Posts",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_EmailTemplateId",
                table: "Campaigns",
                column: "EmailTemplateId");

            migrationBuilder.AddForeignKey(
                name: "FK_Campaigns_EmailTemplates_EmailTemplateId",
                table: "Campaigns",
                column: "EmailTemplateId",
                principalTable: "EmailTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ContactListContacts_ContacLists_ContactListId",
                table: "ContactListContacts",
                column: "ContactListId",
                principalTable: "ContacLists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_Categories_CategoryId",
                table: "Posts",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
