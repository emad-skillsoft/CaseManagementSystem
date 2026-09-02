using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CaseManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class Session5CompletionAndImportReviewResolution : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "ImportReviewItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "ImportReviewItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "SLAStartDate",
                table: "ImportReviewItems",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "ImportReviewItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletionRequestedAt",
                table: "Cases",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompletionSummary",
                table: "Cases",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "ImportReviewItems");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "ImportReviewItems");

            migrationBuilder.DropColumn(
                name: "SLAStartDate",
                table: "ImportReviewItems");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "ImportReviewItems");

            migrationBuilder.DropColumn(
                name: "CompletionRequestedAt",
                table: "Cases");

            migrationBuilder.DropColumn(
                name: "CompletionSummary",
                table: "Cases");
        }
    }
}
