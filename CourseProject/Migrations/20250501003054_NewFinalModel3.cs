using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CourseProject.Migrations
{
    /// <inheritdoc />
    public partial class NewFinalModel3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsLinkAccessOnly",
                table: "Templates");

            migrationBuilder.DropColumn(
                name: "IsPrivateAccess",
                table: "Templates");

            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "Templates");

            migrationBuilder.AddColumn<int>(
                name: "AccessType",
                table: "Templates",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccessType",
                table: "Templates");

            migrationBuilder.AddColumn<bool>(
                name: "IsLinkAccessOnly",
                table: "Templates",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPrivateAccess",
                table: "Templates",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "Templates",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
