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

        // Injetamos o DbContext para acessar o banco de dados
        public ImagensController(DecoleiDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // 1. UPLOAD COM ASSOCIAÇÃO A UM PACOTE
        //    - Rota agora espera o ID do pacote: api/Imagens/upload/5
        //    - Apenas ADMINS podem acessar.
        [HttpPost("upload/{pacoteId}")]
        [Authorize(Roles = "ADMIN,ADMINISTRADOR")]
        public async Task<IActionResult> Upload(int pacoteId, IFormFile file)
        {
            // Verifica se o pacote existe
            var pacote = await _context.PacotesViagem.FindAsync(pacoteId);
            if (pacote == null)
            {
                return NotFound($"Pacote com ID {pacoteId} não encontrado.");
            }

            if (file == null || file.Length == 0)
            {
                return BadRequest("Nenhum arquivo enviado.");
            }

            // Caminho para salvar o arquivo físico
            var uploadsFolderPath = Path.Combine(_env.WebRootPath, "uploads", "pacotes");
            if (!Directory.Exists(uploadsFolderPath))
            {
                Directory.CreateDirectory(uploadsFolderPath);
            }

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsFolderPath, fileName);

            // Salva o arquivo no disco
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Cria a entidade Imagem e associa ao pacote
            var imagem = new Imagem
            {
                Url = $"uploads/pacotes/{fileName}", // Salva o caminho relativo
                PacoteViagemId = pacoteId
            };

            // Salva a nova entidade no banco de dados
            _context.Imagens.Add(imagem);
            await _context.SaveChangesAsync();

            // Retorna a informação da imagem criada
            return Ok(new { id = imagem.Id, url = imagem.Url });
        }

        // 2. DELETAR UMA IMAGEM
        //    - Rota espera o ID da imagem: api/Imagens/5
        //    - Apenas ADMINS podem acessar.
        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN,ADMINISTRADOR")]
        public async Task<IActionResult> Delete(int id)
        {
            // Busca a imagem no banco de dados
            var imagem = await _context.Imagens.FindAsync(id);
            if (imagem == null)
            {
                return NotFound($"Imagem com ID {id} não encontrada.");
            }

            // Monta o caminho completo do arquivo físico
            var filePath = Path.Combine(_env.WebRootPath, imagem.Url);

            // Tenta deletar o arquivo físico (se ele existir)
            if (System.IO.File.Exists(filePath))
            {
                try
                {
                    System.IO.File.Delete(filePath);
                }
                catch (IOException ex)
                {
                    // Loga o erro, mas continua para remover do DB
                    Console.WriteLine($"Erro ao deletar arquivo físico: {ex.Message}");
                }
            }

            // Remove o registro da imagem do banco de dados
            _context.Imagens.Remove(imagem);
            await _context.SaveChangesAsync();

            return Ok(new { mensagem = "Imagem deletada com sucesso." });
        }
    }
}