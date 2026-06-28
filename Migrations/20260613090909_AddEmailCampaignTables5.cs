using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebMVC.Migrations
{
    public partial class AddEmailCampaignTables5 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // =========================
            // EMAIL TEMPLATES
            // =========================
            migrationBuilder.CreateTable(
                name: "EmailTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),

                    Name = table.Column<string>(maxLength: 255, nullable: false),

                    Status = table.Column<string>(maxLength: 100, nullable: false),

                    Subject = table.Column<string>(maxLength: 500, nullable: false),

                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),

                    Created = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<int>(nullable: false),

                    Updated = table.Column<DateTime>(nullable: true),
                    UpdateBy = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailTemplates", x => x.Id);
                });

            // =========================
            // CAMPAIGNS
            // =========================
            migrationBuilder.CreateTable(
                name: "Campaigns",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),

                    Name = table.Column<string>(maxLength: 255, nullable: false),

                    EmailTemplateId = table.Column<int>(nullable: true), // MUST nullable for SetNull

                    Subject = table.Column<string>(maxLength: 500, nullable: false),

                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),

                    SendAt = table.Column<DateTime>(nullable: false),

                    Status = table.Column<int>(nullable: false),

                    TotalRecipients = table.Column<int>(nullable: false, defaultValue: 0),
                    SentCount = table.Column<int>(nullable: false, defaultValue: 0),
                    FailedCount = table.Column<int>(nullable: false, defaultValue: 0),

                    Created = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<int>(nullable: false),

                    Updated = table.Column<DateTime>(nullable: true),
                    UpdateBy = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Campaigns", x => x.Id);

                    table.ForeignKey(
                        name: "FK_Campaigns_EmailTemplates_EmailTemplateId",
                        column: x => x.EmailTemplateId,
                        principalTable: "EmailTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull
                    );
                });

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_EmailTemplateId",
                table: "Campaigns",
                column: "EmailTemplateId"
            );

            // =========================
            // CONTACT LISTS
            // =========================
            migrationBuilder.CreateTable(
                name: "Contacts",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),

                    CampaignId = table.Column<int>(nullable: false),

                    Email = table.Column<string>(maxLength: 320, nullable: false),

                    Status = table.Column<int>(nullable: false),

                    SentAt = table.Column<DateTime>(nullable: true),

                    RetryCount = table.Column<int>(nullable: false, defaultValue: 0),

                    Created = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<int>(nullable: false),

                    Updated = table.Column<DateTime>(nullable: true),
                    UpdateBy = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contacs", x => x.Id);

                    table.ForeignKey(
                        name: "FK_Contacts_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                });

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_CampaignId",
                table: "Contacts",
                column: "CampaignId"
            );

            // =========================
            // MAIL LOGS
            // =========================
            migrationBuilder.CreateTable(
                name: "MailLogs",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),

                    CampaignRecipientId = table.Column<int>(nullable: false),

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
                        name: "FK_MailLogs_Contacts_CampaignRecipientId",
                        column: x => x.CampaignRecipientId,
                        principalTable: "Contacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                });

            migrationBuilder.CreateIndex(
                name: "IX_MailLogs_CampaignRecipientId",
                table: "MailLogs",
                column: "CampaignRecipientId"
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "MailLogs");
            migrationBuilder.DropTable(name: "Contacts");
            migrationBuilder.DropTable(name: "Campaigns");
            migrationBuilder.DropTable(name: "EmailTemplates");
        }
    }
}