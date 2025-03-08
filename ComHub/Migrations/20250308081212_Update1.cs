using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComHub.Migrations
{
    /// <inheritdoc />
    public partial class Update1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_items_categories_categories_category_id",
                table: "items_categories");

            migrationBuilder.AddForeignKey(
                name: "FK_items_categories_categories_category_id",
                table: "items_categories",
                column: "category_id",
                principalTable: "categories",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_items_categories_categories_category_id",
                table: "items_categories");

            migrationBuilder.AddForeignKey(
                name: "FK_items_categories_categories_category_id",
                table: "items_categories",
                column: "category_id",
                principalTable: "categories",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
