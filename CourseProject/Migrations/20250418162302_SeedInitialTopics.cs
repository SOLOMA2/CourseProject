using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CourseProject.Migrations
{
    /// <inheritdoc />
    public partial class SeedInitialTopics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Добавление начальных тем
            migrationBuilder.InsertData(
                table: "Topics",
                columns: new[] { "Name" },
                values: new object[,]
                {
            { "Education" },
            { "Quiz" },
            { "Other" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
