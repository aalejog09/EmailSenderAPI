using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MailSenderAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailExtensionsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmailExtensions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Extension = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailExtensions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmailExtensions_Extension",
                table: "EmailExtensions",
                column: "Extension",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailExtensions");
        }
    }
}
