using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EBL.FIG.Process.Identity.Infra.Data.Migrations
{
    /// <summary>
    /// Baseline migration: representa o estado inicial do banco ja existente.
    /// Os metodos Up e Down estao intencionalmente vazios — as tabelas ja existem no banco.
    /// Esta migration e registrada manualmente via INSERT em __EFMigrationsHistory.
    /// </summary>
    public partial class Baseline : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Banco ja existia antes do controle de migrations ser introduzido.
            // Nenhuma acao necessaria — tabelas ja foram criadas pelo Create-Tables.sql.
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Rollback intencional nao suportado para a baseline.
        }
    }
}
