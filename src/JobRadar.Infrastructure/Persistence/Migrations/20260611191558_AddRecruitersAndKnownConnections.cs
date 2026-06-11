using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobRadar.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRecruitersAndKnownConnections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KnownConnections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    NormalizedName = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    LinkedInUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NormalizedLinkedInUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Company = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: true),
                    NormalizedCompany = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: true),
                    Title = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Location = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: true),
                    ConnectionStatus = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ImportedFrom = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    ImportedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastConfirmedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KnownConnections", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Recruiters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    NormalizedName = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    Company = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    NormalizedCompany = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    LinkedInUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NormalizedLinkedInUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    Location = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: true),
                    ConnectionStatus = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Source = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    LastContactAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recruiters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RecruiterTags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RecruiterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    NormalizedName = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecruiterTags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecruiterTags_Recruiters_RecruiterId",
                        column: x => x.RecruiterId,
                        principalTable: "Recruiters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_KnownConnections_UserId_ConnectionStatus",
                table: "KnownConnections",
                columns: new[] { "UserId", "ConnectionStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_KnownConnections_UserId_NormalizedLinkedInUrl",
                table: "KnownConnections",
                columns: new[] { "UserId", "NormalizedLinkedInUrl" });

            migrationBuilder.CreateIndex(
                name: "IX_KnownConnections_UserId_NormalizedName_NormalizedCompany",
                table: "KnownConnections",
                columns: new[] { "UserId", "NormalizedName", "NormalizedCompany" });

            migrationBuilder.CreateIndex(
                name: "IX_Recruiters_UserId_ConnectionStatus",
                table: "Recruiters",
                columns: new[] { "UserId", "ConnectionStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_Recruiters_UserId_NormalizedLinkedInUrl",
                table: "Recruiters",
                columns: new[] { "UserId", "NormalizedLinkedInUrl" });

            migrationBuilder.CreateIndex(
                name: "IX_Recruiters_UserId_NormalizedName_NormalizedCompany",
                table: "Recruiters",
                columns: new[] { "UserId", "NormalizedName", "NormalizedCompany" });

            migrationBuilder.CreateIndex(
                name: "IX_RecruiterTags_RecruiterId_NormalizedName",
                table: "RecruiterTags",
                columns: new[] { "RecruiterId", "NormalizedName" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KnownConnections");

            migrationBuilder.DropTable(
                name: "RecruiterTags");

            migrationBuilder.DropTable(
                name: "Recruiters");
        }
    }
}
