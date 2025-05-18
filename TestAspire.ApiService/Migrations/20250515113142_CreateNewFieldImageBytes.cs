using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestAspire.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class CreateNewFieldImageBytes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Image",
                table: "Datasets");

            migrationBuilder.AddColumn<byte[]>(
                name: "Image",
                table: "Datasets",
                type: "bytea",
                nullable: false,
                defaultValue: new byte[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Image",
                table: "Datasets");

            migrationBuilder.AddColumn<string>(
                name: "Image",
                table: "Datasets",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
