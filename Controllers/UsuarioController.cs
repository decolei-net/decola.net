using Decolei.net.DTOs;
using Decolei.net.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Decolei.net.Services;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Decolei.net.Data;

namespace Decolei.net.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly SignInManager<Usuario> _signInManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly EmailService _emailService;
        private readonly ILogger<UsuarioController> _logger;
        private readonly DecoleiDbContext _context;

        public UsuarioController(
            UserManager<Usuario> userManager,
            SignInManager<Usuario> signInManager,
            RoleManager<IdentityRole<int>> roleManager,
            IConfiguration configuration,
            EmailService emailService,
            ILogger<UsuarioController> logger,
            DecoleiDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _emailService = emailService;
            _logger = logger;
            _context = context;
        }

        [HttpPost("registrar")]
        public async Task<IActionResult> Registrar([FromBody] RegistroUsuarioDto registroDto)
        {
            try
            {
                var usuarioExistente = await _userManager.FindByEmailAsync(registroDto.Email!);
                if (usuarioExistente != null)
                {
                    return BadRequest(new { erro = "Este e-mail já está em uso." });
                }

                var cpfExistente = await _userManager.Users.FirstOrDefaultAsync(u => u.Documento == registroDto.Documento);
                if (cpfExistente != null)
                {
                    return BadRequest(new { erro = "Este documento já está em uso." });
                }

                var novoUsuario = new Usuario
                {
                    UserName = registroDto.Email,
                    Email = registroDto.Email,
                    Documento = registroDto.Documento,
                    PhoneNumber = registroDto.Telefone,
                    Perfil = "CLIENTE",
                    NomeCompleto = registroDto.Nome
                };

                var resultado = await _userManager.CreateAsync(novoUsuario, registroDto.Senha!);

                if (resultado.Succeeded)
                {
                    if (!await _roleManager.RoleExistsAsync("CLIENTE"))
                    {
                        await _roleManager.CreateAsync(new IdentityRole<int>("CLIENTE"));
                    }
                    await _userManager.AddToRoleAsync(novoUsuario, "CLIENTE");

                    return Ok(new { mensagem = "Usuário cliente registrado com sucesso!" });
                }

                var erros = resultado.Errors.ToDictionary(e => e.Code, e => e.Description);
                return BadRequest(new { erro = "Falha ao registrar usuário.", detalhes = erros });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro inesperado durante o registro do usuário {Email}.", registroDto.Email);
                return StatusCode(500, new { erro = "Ocorreu um erro interno no servidor." });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUsuarioDto loginDto)
        {
            try
            {
                var usuario = await _userManager.FindByEmailAsync(loginDto.Email!);
                if (usuario == null)
                {
                    return Unauthorized(new { erro = "Email ou senha inválidos." });
                }

                var resultado = await _signInManager.CheckPasswordSignInAsync(usuario, loginDto.Senha!, lockoutOnFailure: true);

                if (resultado.Succeeded)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                        new Claim(ClaimTypes.Name, usuario.UserName!),
                        new Claim(ClaimTypes.Email, usuario.Email!),
                        new Claim("NomeCompleto", usuario.NomeCompleto!)
                    };

                    var userRoles = await _userManager.GetRolesAsync(usuario);
                    foreach (var role in userRoles)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role));
                    }

                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]!));
                    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                    var expires = DateTime.Now.AddDays(7);

                    var token = new JwtSecurityToken(
                        issuer: _configuration["JwtSettings:Issuer"],
                        audience: _configuration["JwtSettings:Audience"],
                        claims: claims, expires: expires, signingCredentials: creds
                    );

                    var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

                    return Ok(new { token = tokenString, mensagem = "Login bem-sucedido!" });
                }

                if (resultado.IsLockedOut)
                {
                    return Unauthorized(new { erro = "Esta conta está bloqueada. Tente novamente mais tarde." });
                }

                return Unauthorized(new { erro = "Email ou senha inválidos." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro inesperado durante a tentativa de login do usuário {Email}.", loginDto.Email);
                return StatusCode(500, new { erro = "Ocorreu um erro interno no servidor." });
            }
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPost("registrar-admin")]
        public async Task<IActionResult> RegistrarAdmin([FromBody] RegistroUsuarioDto registroDto)
        {
            try
            {
                var usuarioExistente = await _userManager.FindByEmailAsync(registroDto.Email!);
                if (usuarioExistente != null)
                {
                    return BadRequest(new { erro = "Este e-mail já está em uso." });
                }

                var cpfExistente = await _userManager.Users.FirstOrDefaultAsync(u => u.Documento == registroDto.Documento);
                if (cpfExistente != null)
                {
                    return BadRequest(new { erro = "Este documento já está em uso." });
                }

                var novoUsuario = new Usuario
                {
                    UserName = registroDto.Email,
                    Email = registroDto.Email,
                    Documento = registroDto.Documento,
                    PhoneNumber = registroDto.Telefone,
                    Perfil = "ADMIN",
                    NomeCompleto = registroDto.Nome
                };

                var resultado = await _userManager.CreateAsync(novoUsuario, registroDto.Senha!);

                if (resultado.Succeeded)
                {
                    if (!await _roleManager.RoleExistsAsync("ADMIN"))
                    {
                        await _roleManager.CreateAsync(new IdentityRole<int>("ADMIN"));
                    }
                    await _userManager.AddToRoleAsync(novoUsuario, "ADMIN");

                    return Ok(new { mensagem = "Usuário administrador registrado com sucesso!" });
                }

                var erros = resultado.Errors.ToDictionary(e => e.Code, e => e.Description);
                return BadRequest(new { erro = "Falha ao registrar administrador.", detalhes = erros });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro inesperado durante o registro do administrador {Email}.", registroDto.Email);
                return StatusCode(500, new { erro = "Ocorreu um erro interno no servidor." });
            }
        }

        [HttpPost("recuperar-senha")]
        public async Task<IActionResult> RecuperarSenha([FromBody] RecuperarSenhaDto dto)
        {
            var usuario = await _userManager.FindByEmailAsync(dto.Email);
            if (usuario == null)
            {
                _logger.LogWarning("Tentativa de recuperação de senha para um e-mail não existente: {Email}", dto.Email);
                return Ok(new { mensagem = "Se um usuário com este e-mail existir, um link de recuperação foi enviado." });
            }

            try
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(usuario);
                var link = $"{_configuration["Frontend:ResetPasswordUrl"]}?token={Uri.EscapeDataString(token)}&email={dto.Email}";

                var corpo = $@"
                        <html>
                          <body style='font-family: Arial, sans-serif; color: #333;'>
                            <h2 style='color: #007bff;'>🔒 Redefinição de Senha</h2>
                            <p>Olá,</p>
                            <p>Recebemos uma solicitação para redefinir a senha da sua conta no <strong>Decolei.NET</strong>.</p>
                            <p>Clique no botão abaixo para criar uma nova senha:</p>
                            <p style='margin: 20px 0;'>
                              <a href='{link}' style='background-color: #007bff; color: white; padding: 12px 20px; text-decoration: none; border-radius: 5px;'>Redefinir Senha</a>
                            </p>
                            <hr style='margin: 30px 0;' />
                            <p><strong>Email associado:</strong> {dto.Email}</p>
                            <br />
                            <p style='font-size: 14px; color: #999;'>Se você não solicitou essa redefinição, pode ignorar este e-mail com segurança.</p>
                            <br />
                            <p>Agradecemos por viajar com a <strong>Decolei.NET</strong>!</p>
                            <p>Se precisar de algo, estamos aqui para ajudar.</p>
                            <br />
                            <p style='margin-top: 40px;'>Atenciosamente,<br /><em>Equipe Decolei.NET</em></p>
                          </body>
                        </html>";

                await _emailService.EnviarEmailAsync(dto.Email, "Recuperação de Senha - Decolei.Net", corpo);

                return Ok(new { mensagem = "Se um usuário com este e-mail existir, um link de recuperação foi enviado." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao enviar e-mail de recuperação de senha para o usuário {Email}.", dto.Email);
                return Ok(new { mensagem = "Se um usuário com este e-mail existir, um link de recuperação foi enviado." });
            }
        }

        [HttpPost("redefinir-senha")]
        public async Task<IActionResult> RedefinirSenha([FromBody] RedefinirSenhaDto dto)
        {
            try
            {
                var usuario = await _userManager.FindByEmailAsync(dto.Email);
                if (usuario == null)
                {
                    return BadRequest(new { erro = "Não foi possível redefinir a senha. O link pode ser inválido ou ter expirado." });
                }

                var resultado = await _userManager.ResetPasswordAsync(usuario, dto.Token, dto.NovaSenha);
                if (!resultado.Succeeded)
                {
                    var erros = resultado.Errors.ToDictionary(e => e.Code, e => e.Description);
                    return BadRequest(new { erro = "Não foi possível redefinir a senha.", detalhes = erros });
                }

                return Ok(new { mensagem = "Senha redefinida com sucesso!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro inesperado ao redefinir a senha do usuário {Email}.", dto.Email);
                return StatusCode(500, new { erro = "Ocorreu um erro interno no servidor." });
            }
        }

        // NOVO ENDPOINT DE ALTERAÇÃO DE SENHA
        [HttpPost("alterar-senha")]
        [Authorize]
        public async Task<IActionResult> AlterarSenha([FromBody] AlterarSenhaDto dto)
        {
            try
            {
                var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdString))
                {
                    return Unauthorized(new { erro = "Token de usuário inválido." });
                }

                var usuario = await _userManager.FindByIdAsync(userIdString);
                if (usuario == null)
                {
                    return NotFound(new { erro = "Usuário não encontrado." });
                }

                // Verifica a senha atual antes de permitir a alteração
                var resultado = await _userManager.ChangePasswordAsync(usuario, dto.SenhaAtual, dto.NovaSenha);

                if (!resultado.Succeeded)
                {
                    var erros = resultado.Errors.ToDictionary(e => e.Code, e => e.Description);
                    // O erro de senha incorreta geralmente é "PasswordMismatch"
                    if (erros.ContainsKey("PasswordMismatch"))
                    {
                        return BadRequest(new { erro = "A senha atual está incorreta." });
                    }
                    return BadRequest(new { erro = "Falha ao alterar a senha.", detalhes = erros });
                }

                _logger.LogInformation("Senha do usuário com ID {UserId} foi alterada com sucesso.", usuario.Id);
                return Ok(new { mensagem = "Senha alterada com sucesso!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro inesperado ao alterar a senha do usuário.");
                return StatusCode(500, new { erro = "Ocorreu um erro interno no servidor." });
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok(new { mensagem = "Logout realizado com sucesso!" });
        }

        [HttpPut("meu-perfil")]
        [Authorize(Roles = "CLIENTE,ADMIN,ATENDENTE")]
        public async Task<IActionResult> AtualizarMeuPerfil([FromBody] AtualizarUsuarioDto updateDto)
        {
            try
            {
                var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
                {
                    return Unauthorized(new { erro = "Token de usuário inválido." });
                }

                var usuario = await _userManager.FindByIdAsync(userIdString);
                if (usuario == null)
                {
                    return NotFound(new { erro = "Usuário não encontrado." });
                }

                if (!string.IsNullOrEmpty(updateDto.NomeCompleto))
                {
                    usuario.NomeCompleto = updateDto.NomeCompleto;
                }

                if (!string.IsNullOrEmpty(updateDto.Telefone))
                {
                    usuario.PhoneNumber = updateDto.Telefone;
                }

                // Lógica para atualizar o email
                if (!string.IsNullOrEmpty(updateDto.Email) && usuario.Email != updateDto.Email)
                {
                    var usuarioComNovoEmail = await _userManager.FindByEmailAsync(updateDto.Email);
                    if (usuarioComNovoEmail != null && usuarioComNovoEmail.Id != usuario.Id)
                    {
                        return BadRequest(new { erro = "Este e-mail já está em uso por outra conta." });
                    }

                    var resultadoEmail = await _userManager.SetEmailAsync(usuario, updateDto.Email);
                    if (!resultadoEmail.Succeeded)
                    {
                        var erros = resultadoEmail.Errors.ToDictionary(e => e.Code, e => e.Description);
                        return BadRequest(new { erro = "Falha ao atualizar o e-mail.", detalhes = erros });
                    }

                    await _userManager.SetUserNameAsync(usuario, updateDto.Email);
                }

                var resultado = await _userManager.UpdateAsync(usuario);
                if (!resultado.Succeeded)
                {
                    var erros = resultado.Errors.ToDictionary(e => e.Code, e => e.Description);
                    return BadRequest(new { erro = "Falha ao atualizar o perfil.", detalhes = erros });
                }

                _logger.LogInformation("Perfil do usuário com ID {UserId} foi atualizado por ele mesmo.", userId);

                return Ok(new
                {
                    mensagem = "Perfil atualizado com sucesso!",
                    usuarioAtualizado = new UsuarioDto
                    {
                        Id = usuario.Id,
                        NomeCompleto = usuario.NomeCompleto,
                        Email = usuario.Email,
                        Telefone = usuario.PhoneNumber,
                        Documento = usuario.Documento,
                        Perfil = usuario.Perfil
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro inesperado ao atualizar o perfil do usuário.");
                return StatusCode(500, new { erro = "Ocorreu um erro interno no servidor." });
            }
        }


        [HttpGet]
        [Authorize(Roles = "ADMIN,ATENDENTE")]
        public async Task<IActionResult> ListarUsuarios(
            [FromQuery] string? nome,
            [FromQuery] string? email,
            [FromQuery] string? documento)
        {
            try
            {
                var query = _userManager.Users.AsQueryable();

                if (!string.IsNullOrEmpty(nome))
                {
                    var filtroNome = nome.ToLower().Trim();
                    query = query.Where(u => u.NomeCompleto != null && u.NomeCompleto.ToLower().Contains(filtroNome));
                }

                if (!string.IsNullOrEmpty(email))
                {
                    var filtroEmail = email.ToLower().Trim();
                    query = query.Where(u => u.Email != null && u.Email.ToLower().Contains(filtroEmail));
                }

                if (!string.IsNullOrEmpty(documento))
                {
                    var filtroDocumento = documento.ToLower().Trim();
                    query = query.Where(u => u.Documento != null && u.Documento.ToLower().Contains(filtroDocumento));
                }

                var usuarios = await query.ToListAsync();
                var usuariosDto = new List<UsuarioDto>();

                foreach (var usuario in usuarios)
                {
                    var roles = await _userManager.GetRolesAsync(usuario);
                    usuariosDto.Add(new UsuarioDto
                    {
                        Id = usuario.Id,
                        NomeCompleto = usuario.NomeCompleto,
                        Email = usuario.Email,
                        Telefone = usuario.PhoneNumber,
                        Documento = usuario.Documento,
                        Perfil = roles.FirstOrDefault() ?? "Sem Perfil"
                    });
                }

                return Ok(usuariosDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro inesperado ao listar os usuários.");
                return StatusCode(500, new { erro = "Ocorreu um erro interno no servidor." });
            }
        }


        [HttpGet("{id:int}")]
        [Authorize(Roles = "ADMIN,ATENDENTE")]
        public async Task<IActionResult> ObterUsuarioPorId(int id)
        {
            if (id <= 0) return BadRequest(new { erro = "ID de usuário inválido." });

            try
            {
                var usuario = await _userManager.FindByIdAsync(id.ToString());
                if (usuario == null)
                {
                    return NotFound(new { erro = $"Usuário com ID {id} não encontrado." });
                }

                var roles = await _userManager.GetRolesAsync(usuario);
                var usuarioDto = new UsuarioDto
                {
                    Id = usuario.Id,
                    NomeCompleto = usuario.NomeCompleto,
                    Email = usuario.Email,
                    Telefone = usuario.PhoneNumber,
                    Documento = usuario.Documento,
                    Perfil = roles.FirstOrDefault() ?? "Sem Perfil"
                };

                return Ok(usuarioDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro inesperado ao buscar o usuário com ID {UserId}.", id);
                return StatusCode(500, new { erro = "Ocorreu um erro interno no servidor." });
            }
        }

        [HttpPut("admin/atualizar/{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> AtualizarUsuarioPorAdmin(int id, [FromBody] AdminAtualizarUsuarioDto updateDto)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Tentativa de atualização por admin com ID de usuário inválido: {UserId}", id);
                return BadRequest(new { erro = "ID de usuário inválido." });
            }

            try
            {
                var usuario = await _userManager.FindByIdAsync(id.ToString());
                if (usuario == null)
                {
                    _logger.LogWarning("Tentativa de atualização por admin de usuário não existente: {UserId}", id);
                    return NotFound(new { erro = $"Usuário com ID {id} não encontrado." });
                }

                if (!string.IsNullOrEmpty(updateDto.NomeCompleto))
                {
                    usuario.NomeCompleto = updateDto.NomeCompleto;
                }
                if (!string.IsNullOrEmpty(updateDto.Telefone))
                {
                    usuario.PhoneNumber = updateDto.Telefone;
                }
                if (!string.IsNullOrEmpty(updateDto.Documento))
                {
                    usuario.Documento = updateDto.Documento;
                }

                if (!string.IsNullOrEmpty(updateDto.Perfil))
                {
                    var novoPerfil = updateDto.Perfil.ToUpper();

                    var usuarioLogadoId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                    if (usuarioLogadoId == id && novoPerfil != "ADMIN")
                    {
                        return StatusCode(403, new { erro = "Um administrador não pode alterar o seu próprio perfil." });
                    }

                    if (!await _roleManager.RoleExistsAsync(novoPerfil))
                    {
                        return BadRequest(new { erro = $"O perfil '{updateDto.Perfil}' não é um papel válido." });
                    }

                    var perfisAtuais = await _userManager.GetRolesAsync(usuario);
                    var resultadoRemover = await _userManager.RemoveFromRolesAsync(usuario, perfisAtuais);

                    if (!resultadoRemover.Succeeded)
                    {
                        var erros = resultadoRemover.Errors.ToDictionary(e => e.Code, e => e.Description);
                        return BadRequest(new { erro = "Falha ao remover perfis antigos.", detalhes = erros });
                    }

                    var resultadoAdicionar = await _userManager.AddToRoleAsync(usuario, novoPerfil);
                    if (!resultadoAdicionar.Succeeded)
                    {
                        var erros = resultadoAdicionar.Errors.ToDictionary(e => e.Code, e => e.Description);
                        return BadRequest(new { erro = "Falha ao adicionar novo perfil.", detalhes = erros });
                    }

                    usuario.Perfil = novoPerfil;
                }

                var resultadoUpdate = await _userManager.UpdateAsync(usuario);
                if (!resultadoUpdate.Succeeded)
                {
                    var erros = resultadoUpdate.Errors.ToDictionary(e => e.Code, e => e.Description);
                    return BadRequest(new { erro = "Falha ao atualizar dados do usuário.", detalhes = erros });
                }

                return Ok(new { mensagem = "Usuário atualizado com sucesso!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro inesperado ao atualizar o usuário com ID {UserId} pelo admin.", id);
                return StatusCode(500, new { erro = "Ocorreu um erro interno no servidor." });
            }
        }

        [HttpDelete("admin/deletar/{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> DeletarUsuarioPorAdmin(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Tentativa de exclusão com ID de usuário inválido: {UserId}", id);
                return BadRequest(new { erro = "ID de usuário inválido." });
            }

            var usuarioLogadoId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            if (usuarioLogadoId == id)
            {
                return StatusCode(403, new { erro = "Você não pode excluir seu próprio usuário." });
            }

            try
            {
                var usuario = await _userManager.FindByIdAsync(id.ToString());
                if (usuario == null)
                {
                    _logger.LogWarning("Tentativa de exclusão de usuário não existente: {UserId}", id);
                    return NotFound(new { erro = $"Usuário com ID {id} não encontrado." });
                }

                var resultado = await _userManager.DeleteAsync(usuario);
                if (!resultado.Succeeded)
                {
                    var erros = resultado.Errors.ToDictionary(e => e.Code, e => e.Description);
                    return BadRequest(new { erro = "Falha ao excluir o usuário.", detalhes = erros });
                }

                _logger.LogInformation("Usuário com ID {UserId} foi excluído por um administrador.", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro inesperado ao excluir o usuário com ID {UserId}.", id);
                return StatusCode(500, new { erro = "Ocorreu um erro interno no servidor." });
            }
        }

        [HttpGet("meu-perfil")]
        [Authorize]
        public async Task<ActionResult<MeuPerfilDto>> GetMeuPerfil()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("Token inválido");
                }

                // O UserManager já tem métodos para buscar por ID.
                var usuario = await _userManager.FindByIdAsync(userId);
                if (usuario == null)
                {
                    return NotFound("Usuário não encontrado");
                }

                var perfis = await _userManager.GetRolesAsync(usuario);
                var perfilDto = new MeuPerfilDto
                {
                    Id = usuario.Id,
                    NomeCompleto = usuario.NomeCompleto,
                    Email = usuario.Email,
                    Telefone = usuario.PhoneNumber,
                    Documento = usuario.Documento,
                    Role = perfis.FirstOrDefault() ?? "Sem Perfil"
                };

                return Ok(perfilDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = "Erro interno do servidor" });
            }
        }
    }
}