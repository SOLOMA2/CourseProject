using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CourseProject.Migrations
{
    /// <inheritdoc />
    public partial class NewFinalModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TemplateAccess_Templates_TemplateId",
                table: "TemplateAccess");

            migrationBuilder.AddForeignKey(
                name: "FK_TemplateAccess_Templates_TemplateId",
                table: "TemplateAccess",
                column: "TemplateId",
                principalTable: "Templates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TemplateAccess_Templates_TemplateId",
                table: "TemplateAccess");

            migrationBuilder.AddForeignKey(
                name: "FK_TemplateAccess_Templates_TemplateId",
                table: "TemplateAccess",
                column: "TemplateId",
                principalTable: "Templates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
