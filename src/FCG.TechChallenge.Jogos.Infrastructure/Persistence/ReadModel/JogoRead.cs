using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FCG.TechChallenge.Jogos.Infrastructure.Persistence.ReadModel
{
    [Table("jogo_read", Schema = "public")]
    public sealed class JogoRead
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [Column("nome")]
        public string Nome { get; set; } = null!;

        [Column("descricao")]
        public string? Descricao { get; set; }

        [Column("preco")]
        [Precision(10, 2)]
        public decimal Preco { get; set; }

        [Column("categoria")]
        public string? Categoria { get; set; }

        [Column("version")]
        public int Version { get; set; }

        [Column("created_utc")]
        public DateTime CreatedUtc { get; set; }

        [Column("updated_utc")]
        public DateTime? UpdatedUtc { get; set; }
    }
}