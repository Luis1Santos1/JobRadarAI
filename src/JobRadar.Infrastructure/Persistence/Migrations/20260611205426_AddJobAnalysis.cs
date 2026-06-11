using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobRadar.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddJobAnalysis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "JobAnalyses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JobPostId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PromptVersion = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Model = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    IsJobPost = table.Column<bool>(type: "bit", nullable: false),
                    DetectedTitle = table.Column<string>(type: "nvarchar(220)", maxLength: 220, nullable: false),
                    DetectedCompany = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    Seniority = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    WorkModel = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    ContractType = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Location = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    RequiredTechnologiesJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NiceToHaveTechnologiesJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ResponsibilitiesJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequirementsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BenefitsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RedFlagsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FitReasonsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcernsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: false),
                    AiFitScore = table.Column<int>(type: "int", nullable: false),
                    HybridScore = table.Column<int>(type: "int", nullable: false),
                    ConfidenceScore = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    Recommendation = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    RawModelResponse = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobAnalyses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobAnalyses_JobPosts_JobPostId",
                        column: x => x.JobPostId,
                        principalTable: "JobPosts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobAnalyses_JobPostId",
                table: "JobAnalyses",
                column: "JobPostId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobAnalyses_UserId_HybridScore",
                table: "JobAnalyses",
                columns: new[] { "UserId", "HybridScore" });

            migrationBuilder.CreateIndex(
                name: "IX_JobAnalyses_UserId_JobPostId",
                table: "JobAnalyses",
                columns: new[] { "UserId", "JobPostId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobAnalyses");
        }
    }
}
