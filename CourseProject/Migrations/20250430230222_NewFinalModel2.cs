using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CourseProject.Migrations
{
    /// <inheritdoc />
    public partial class NewFinalModel2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AppUserId",
                table: "TemplateAccess",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TemplateAccess_AppUserId",
                table: "TemplateAccess",
                column: "AppUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_TemplateAccess_AspNetUsers_AppUserId",
                table: "TemplateAccess",
                column: "AppUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TemplateAccess_AspNetUsers_AppUserId",
                table: "TemplateAccess");

            migrationBuilder.DropIndex(
                name: "IX_TemplateAccess_AppUserId",
                table: "TemplateAccess");

            migrationBuilder.DropColumn(
                name: "AppUserId",
                table: "TemplateAccess");
        }
    }
}
