using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaaSInventoryManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddIsPermissionManagedToApplicationUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPermissionManaged",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPermissionManaged",
                table: "AspNetUsers");
        }
    }
}
