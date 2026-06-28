using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebMVC.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceContactsWithContactListSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "MailLogs");
            migrationBuilder.DropTable(name: "Contacts");

            migrationBuilder.CreateTable(
                name: "Contacts",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),

                    FirstName = table.Column<string>(maxLength: 255, nullable: false),

                    LastName = table.Column<string>(maxLength: 255, nullable: false),

                    Email = table.Column<string>(maxLength: 320, nullable: false),

                    Created = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<int>(nullable: false),

                    Updated = table.Column<DateTime>(nullable: true),
                    UpdateBy = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contact", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Contact_Email",
                table: "Contact",
                column: "Email"
            );

            // =========================
            // CONTACT LIST
            // =========================
            migrationBuilder.CreateTable(
                name: "ContactLists",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),

                    Name = table.Column<string>(maxLength: 255, nullable: false),

                    Created = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<int>(nullable: false),

                    Updated = table.Column<DateTime>(nullable: true),
                    UpdateBy = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactList", x => x.Id);
                });

            // =========================
            // CONTACT LIST CONTACT (junction table)
            // =========================
            migrationBuilder.CreateTable(
                name: "ContactListContacts",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),

                    ContactListId = table.Column<int>(nullable: false),

                    ContactId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactListContact", x => x.Id);

                    table.ForeignKey(
                        name: "FK_ContactListContact_ContactList_ContactListId",
                        column: x => x.ContactListId,
                        principalTable: "ContactList",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );

                    table.ForeignKey(
                        name: "FK_ContactListContact_Contact_ContactId",
                        column: x => x.ContactId,
                        principalTable: "Contact",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContactListContact_ContactListId",
                table: "ContactListContact",
                column: "ContactListId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_ContactListContact_ContactId",
                table: "ContactListContact",
                column: "ContactId"
            );

            // Tránh trùng cặp ContactList-Contact
            migrationBuilder.CreateIndex(
                name: "IX_ContactListContact_ContactListId_ContactId",
                table: "ContactListContact",
                columns: new[] { "ContactListId", "ContactId" },
                unique: true
            );

            // =========================
            // MAIL LOGS (re-created, FK to Campaigns now)
            // =========================
            migrationBuilder.CreateTable(
                name: "MailLogs",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),

                    CampaignId = table.Column<int>(nullable: false),

                    Status = table.Column<int>(nullable: false),

                    ErrorMessage = table.Column<string>(maxLength: 2000, nullable: true),

                    SentAt = table.Column<DateTime>(nullable: true),

                    Created = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<int>(nullable: false),

                    Updated = table.Column<DateTime>(nullable: true),
                    UpdateBy = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MailLogs", x => x.Id);

                    table.ForeignKey(
                        name: "FK_MailLogs_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                });

            migrationBuilder.CreateIndex(
                name: "IX_MailLogs_CampaignId",
                table: "MailLogs",
                column: "CampaignId"
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "MailLogs");
            migrationBuilder.DropTable(name: "ContactListContacts");
            migrationBuilder.DropTable(name: "ContactLists");
            migrationBuilder.DropTable(name: "Contacts");
        }
    }
}
