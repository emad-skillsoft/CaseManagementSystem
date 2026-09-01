using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CaseManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class Session4ChallengeAndImportReview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CaseChallenges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CaseId = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ResolvedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SupervisorComment = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseChallenges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CaseChallenges_AspNetUsers_ResolvedByUserId",
                        column: x => x.ResolvedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CaseChallenges_AspNetUsers_StartedByUserId",
                        column: x => x.StartedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CaseChallenges_Cases_CaseId",
                        column: x => x.CaseId,
                        principalTable: "Cases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ImportReviewItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExternalCaseId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmployeeNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Issue = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RowNumber = table.Column<int>(type: "int", nullable: false),
                    IsResolved = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportReviewItems", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CaseChallenges_CaseId",
                table: "CaseChallenges",
                column: "CaseId");

            migrationBuilder.CreateIndex(
                name: "IX_CaseChallenges_ResolvedByUserId",
                table: "CaseChallenges",
                column: "ResolvedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CaseChallenges_StartedByUserId",
                table: "CaseChallenges",
                column: "StartedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CaseChallenges");

            migrationBuilder.DropTable(
                name: "ImportReviewItems");
        }
    }
}
