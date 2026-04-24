using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlantCareSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddIsRareToPlant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRare",
                table: "Plants",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRare",
                table: "Plants");
        }
    }
}
