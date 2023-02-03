using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyNotesApplication.Migrations
{
    /// <inheritdoc />
    public partial class Update3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ConfirmationTokens_UserId",
                table: "ConfirmationTokens");

            migrationBuilder.CreateIndex(
                name: "IX_ConfirmationTokens_UserId",
                table: "ConfirmationTokens",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ConfirmationTokens_UserId",
                table: "ConfirmationTokens");

            migrationBuilder.CreateIndex(
                name: "IX_ConfirmationTokens_UserId",
                table: "ConfirmationTokens",
                column: "UserId");
        }
    }
}
