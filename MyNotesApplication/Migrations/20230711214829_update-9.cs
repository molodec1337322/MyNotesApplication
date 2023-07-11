using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyNotesApplication.Migrations
{
    /// <inheritdoc />
    public partial class update9 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrderPlace",
                table: "Notes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Notes",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrderPlace",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Notes");
        }
    }
}
