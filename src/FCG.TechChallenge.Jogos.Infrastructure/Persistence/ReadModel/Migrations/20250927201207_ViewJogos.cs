using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FCG.TechChallenge.Jogos.Infrastructure.Infrastructure.Persistence.ReadModel.Migrations
{
    /// <inheritdoc />
    public partial class ViewJogos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS pg_trgm;");

            // VIEW para leitura
            migrationBuilder.Sql(@"CREATE OR REPLACE VIEW public.jogo_view AS SELECT id, nome, descricao, preco, categoria FROM public.jogo_read;");

            // índices trigram para ILIKE (opcional, acelera buscas por termo)
            migrationBuilder.Sql(@"CREATE INDEX IF NOT EXISTS ix_jogo_read_nome_trgm ON public.jogo_read USING gin (nome gin_trgm_ops);");
            migrationBuilder.Sql(@"CREATE INDEX IF NOT EXISTS ix_jogo_read_categoria_trgm ON public.jogo_read USING gin (categoria gin_trgm_ops);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP VIEW IF EXISTS public.jogo_view;");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ix_jogo_read_nome_trgm;");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ix_jogo_read_categoria_trgm;");
        }
    }
}
