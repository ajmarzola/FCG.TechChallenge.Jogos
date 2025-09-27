using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FCG.TechChallenge.Jogos.Infrastructure.Infrastructure.Persistence.ReadModel.Migrations
{
    /// <inheritdoc />
    public partial class Initial_ReadModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "jogo_read",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: true),
                    preco = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    categoria = table.Column<string>(type: "text", nullable: true),
                    version = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    created_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_jogo_read", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_jogo_read_categoria",
                schema: "public",
                table: "jogo_read",
                column: "categoria");

            migrationBuilder.CreateIndex(
                name: "ix_jogo_read_nome",
                schema: "public",
                table: "jogo_read",
                column: "nome");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "jogo_read",
                schema: "public");
        }
    }
}
