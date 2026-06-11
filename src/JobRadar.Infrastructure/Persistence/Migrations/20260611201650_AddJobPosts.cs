using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobRadar.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddJobPosts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "JobPosts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(220)", maxLength: 220, nullable: false),
                    Company = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    Location = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: true),
                    SourceUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    NormalizedSourceUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RecruiterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OriginalText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    AnalysisStatus = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    AnalysisRequestedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    AnalysisStartedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    AnalysisCompletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    AnalysisError = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobPosts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobPosts_Recruiters_RecruiterId",
                        column: x => x.RecruiterId,
                        principalTable: "Recruiters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobPosts_RecruiterId",
                table: "JobPosts",
                column: "RecruiterId");

            migrationBuilder.CreateIndex(
                name: "IX_JobPosts_UserId_AnalysisStatus",
                table: "JobPosts",
                columns: new[] { "UserId", "AnalysisStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_JobPosts_UserId_CreatedAt",
                table: "JobPosts",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_JobPosts_UserId_NormalizedSourceUrl",
                table: "JobPosts",
                columns: new[] { "UserId", "NormalizedSourceUrl" });

            migrationBuilder.CreateIndex(
                name: "IX_JobPosts_UserId_RecruiterId",
                table: "JobPosts",
                columns: new[] { "UserId", "RecruiterId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobPosts");
        }
    }
}
