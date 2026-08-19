using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaaSInventoryManagement.Migrations
{
    /// <inheritdoc />
    public partial class LowStockThreshold : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "LowStockThreshold",
                table: "Tenants",
                type: "decimal(18,3)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LowStockThreshold",
                table: "Tenants");
        }
    }
}
