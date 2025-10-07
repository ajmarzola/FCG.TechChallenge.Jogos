using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FCG.TechChallenge.Jogos.Infrastructure.ReadModels.Elasticsearch;
using FCG.TechChallenge.Jogos.Infrastructure.Persistence.ReadModel;
// using Microsoft.AspNetCore.Authorization;

namespace FCG.TechChallenge.Jogos.Api.Controllers.Admin
{
    // [Authorize(Roles = "ADMIN")] // habilite se o Gateway propaga JWT e roles
    [ApiController]
    [Route("admin/search")]
    public sealed class SearchAdminController(JogoIndexer indexer, ReadModelDbContext db) : ControllerBase
    {
        private readonly JogoIndexer _indexer = indexer;
        private readonly ReadModelDbContext _db = db;

        /// <summary>
        /// Recria o índice do Elasticsearch e reindexa todos os jogos (full rebuild).
        /// </summary>
        [HttpPost("rebuild")]
        public async Task<IActionResult> Rebuild(CancellationToken ct)
        {
            await _indexer.RebuildIndexAsync(_db, ct);
            return Ok(new { message = "Índice recriado e reindexado com sucesso." });
        }

        /// <summary>
        /// Indexa em massa (sem dropar índice). Útil após inserções no read-model.
        /// </summary>
        [HttpPost("bulk-index")]
        public async Task<IActionResult> BulkIndex(CancellationToken ct)
        {
            var jogos = await _db.Jogos.AsNoTracking().ToListAsync(ct);
            await _indexer.BulkIndexAsync(jogos, ct);
            return Ok(new { message = $"Bulk index finalizado. Documentos: {jogos.Count}" });
        }

        /// <summary>
        /// Remove todos os documentos do índice (mantém o índice).
        /// </summary>
        [HttpDelete("delete-all")]
        public async Task<IActionResult> DeleteAll(CancellationToken ct)
        {
            await _indexer.DeleteAllAsync(ct);
            return Ok(new { message = "Todos os documentos foram removidos do índice." });
        }

        /// <summary>
        /// Remove um documento por ID (GUID).
        /// </summary>
        [HttpDelete("delete/{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            await _indexer.DeleteAsync(id, ct);
            return Ok(new { message = $"Documento {id} removido do índice." });
        }
    }
}
