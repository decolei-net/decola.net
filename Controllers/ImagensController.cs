using Decolei.net.Data;
using Decolei.net.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Decolei.net.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImagensController : ControllerBase
    {
        private readonly DecoleiDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ImagensController(DecoleiDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // Endpoint para UPLOAD DE IMAGENS
        [HttpPost("upload/{pacoteId}")]
        [Authorize(Roles = "ADMIN,ADMINISTRADOR")]
        public async Task<IActionResult> UploadImagem(int pacoteId, IFormFile file)
        {
            var pacote = await _context.PacotesViagem.FindAsync(pacoteId);
            if (pacote == null) return NotFound($"Pacote com ID {pacoteId} não encontrado.");
            if (file == null || file.Length == 0) return BadRequest("Nenhum arquivo de imagem enviado.");

            var uploadsFolderPath = Path.Combine(_env.WebRootPath, "uploads", "pacotes");
            Directory.CreateDirectory(uploadsFolderPath); // Garante que a pasta exista

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsFolderPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var midia = new Imagem
            {
                Url = $"uploads/pacotes/{fileName}",
                IsVideo = false,
                PacoteViagemId = pacoteId
            };

            _context.Imagens.Add(midia);
            await _context.SaveChangesAsync();
            return Ok(new { id = midia.Id, url = midia.Url, isVideo = midia.IsVideo });
        }

        // Endpoint para ADICIONAR VÍDEOS
        [HttpPost("add-video/{pacoteId}")]
        [Authorize(Roles = "ADMIN,ADMINISTRADOR")]
        public async Task<IActionResult> AddVideo(int pacoteId, [FromBody] VideoDto videoDto)
        {
            var pacote = await _context.PacotesViagem.FindAsync(pacoteId);
            if (pacote == null) return NotFound($"Pacote com ID {pacoteId} não encontrado.");
            if (videoDto == null || string.IsNullOrWhiteSpace(videoDto.Url)) return BadRequest("A URL do vídeo é inválida.");

            var midia = new Imagem
            {
                Url = videoDto.Url,
                IsVideo = true,
                PacoteViagemId = pacoteId
            };

            _context.Imagens.Add(midia);
            await _context.SaveChangesAsync();
            return Ok(new { id = midia.Id, url = midia.Url, isVideo = midia.IsVideo });
        }

        // ******** NOVO ENDPOINT PARA BUSCAR TODAS AS MÍDIAS DE UM PACOTE ********
        [HttpGet("all/{pacoteId}")]
        [Authorize(Roles = "ADMIN,ADMINISTRADOR")]
        public async Task<IActionResult> GetAllMidiaPorPacote(int pacoteId)
        {
            var midias = await _context.Imagens
                .Where(m => m.PacoteViagemId == pacoteId)
                .Select(m => new { m.Id, m.Url, m.IsVideo }) // Retorna ID, URL e se é vídeo
                .ToListAsync();

            return Ok(midias);
        }

        // Endpoint para DELETAR MÍDIA
        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN,ADMINISTRADOR")]
        public async Task<IActionResult> Delete(int id)
        {
            var midia = await _context.Imagens.FindAsync(id);
            if (midia == null) return NotFound($"Mídia com ID {id} não encontrada.");

            if (!midia.IsVideo)
            {
                var filePath = Path.Combine(_env.WebRootPath, midia.Url);
                if (System.IO.File.Exists(filePath))
                {
                    try { System.IO.File.Delete(filePath); }
                    catch (IOException ex) { Console.WriteLine($"Erro ao deletar arquivo físico: {ex.Message}"); }
                }
            }

            _context.Imagens.Remove(midia);
            await _context.SaveChangesAsync();
            return Ok(new { mensagem = "Mídia deletada com sucesso." });
        }
    }

    // DTO para receber a URL do vídeo
    public class VideoDto
    {
        public required string Url { get; set; }
    }
}