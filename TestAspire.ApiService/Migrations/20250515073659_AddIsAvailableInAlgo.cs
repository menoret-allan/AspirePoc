using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestAspire.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class AddIsAvailableInAlgo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAlive",
                table: "Algos",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAlive",
                table: "Algos");
        }
    }
}
