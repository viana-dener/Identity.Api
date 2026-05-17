using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EBL.FIG.Process.Identity.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPasswordResetTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Tabela criada manualmente via Create-Tables.sql.
            // Migration registrada para sincronizar o historico do EF Core.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PasswordResetTokens",
                schema: "dbo");
        }
    }
}
