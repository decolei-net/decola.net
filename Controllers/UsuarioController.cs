using Decolei.net.DTOs;
using Decolei.net.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration; // Importar para acessar configurações

// Usings para JWT
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Decolei.net.Services;

namespace Decolei.net.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly SignInManager<Usuario> _signInManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly IConfiguration _configuration; // Adicionado para acessar JwtSettings
        private readonly EmailService _emailService;

        public UsuarioController(
            UserManager<Usuario> userManager,
            SignInManager<Usuario> signInManager,
            RoleManager<IdentityRole<int>> roleManager,
            IConfiguration configuration, // Injetar IConfiguration NO CONSTRUTOR
            EmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _configuration = configuration; // Atribuir
            _emailService = emailService;
        }

        // --- ENDPOINT DE REGISTRO (COM LÓGICA DE PAPÉIS) ---
        [HttpPost("registrar")]
        public async Task<IActionResult> Registrar([FromBody] RegistroUsuarioDto registroDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var usuarioExistente = await _userManager.FindByEmailAsync(registroDto.Email!);
            if (usuarioExistente != null)
            {
                return BadRequest("Este e-mail já está em uso.");
            }

            var cpfExistente = await _userManager.Users.FirstOrDefaultAsync(u => u.Documento == registroDto.Documento);
            if (cpfExistente != null)
            {
                return BadRequest("Este documento já esta em uso.");
            }

            var novoUsuario = new Usuario
            {
                UserName = registroDto.Email, // UserName do Identity será o email
                Email = registroDto.Email,
                Documento = registroDto.Documento,
                PhoneNumber = registroDto.Telefone,
                Perfil = "CLIENTE",
                NomeCompleto = registroDto.Nome // Nome completo com espaços
            };

            var resultado = await _userManager.CreateAsync(novoUsuario, registroDto.Senha!);

            if (resultado.Succeeded)
            {
                // Garante que o papel "CLIENTE" existe no banco. Se não, ele o cria.
                if (!await _roleManager.RoleExistsAsync("CLIENTE"))
                {
                    await _roleManager.CreateAsync(new IdentityRole<int>("CLIENTE"));
                }
                // Adiciona o novo usuário ao papel "CLIENTE".
                await _userManager.AddToRoleAsync(novoUsuario, "CLIENTE");

                // Para APIs com JWT, não fazemos SignInAsync no registro
                // await _signInManager.SignInAsync(novoUsuario, isPersistent: false);
                return Ok(new { Message = "Usuário cliente registrado com sucesso!" });
            }

            foreach (var erro in resultado.Errors)
            {
                ModelState.AddModelError(string.Empty, erro.Description);
            }
            return BadRequest(ModelState);
        }

        // --- ENDPOINT DE LOGIN (LÓGICA CORRIGIDA E GERANDO JWT) ---
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUsuarioDto loginDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // 1. BUSCAR O USUÁRIO PELO E-MAIL
            var usuario = await _userManager.FindByEmailAsync(loginDto.Email!);

            if (usuario == null)
            {
                return Unauthorized("Email ou senha inválidos.");
            }

            // 2. VERIFICAR A SENHA
            var resultado = await _signInManager.CheckPasswordSignInAsync(usuario, loginDto.Senha!, lockoutOnFailure: true);

            if (resultado.Succeeded)
            {
                // --- GERAÇÃO DO JWT ---
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                    new Claim(ClaimTypes.Name, usuario.UserName!), // Usando UserName (que é o email)
                    new Claim(ClaimTypes.Email, usuario.Email!),
                    new Claim("NomeCompleto", usuario.NomeCompleto!) // Adicione o nome completo como uma claim customizada
                };

                // Adicionar as roles do usuário como claims
                var userRoles = await _userManager.GetRolesAsync(usuario);
                foreach (var role in userRoles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]!));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var expires = DateTime.Now.AddDays(7); // Token válido por 7 dias (ajuste conforme necessidade)

                var token = new JwtSecurityToken(
                    issuer: _configuration["JwtSettings:Issuer"],
                    audience: _configuration["JwtSettings:Audience"],
                    claims: claims,
                    expires: expires,
                    signingCredentials: creds
                );

                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

                return Ok(new { Token = tokenString, Message = "Login bem-sucedido!" });
            }

            if (resultado.IsLockedOut)
            {
                return Unauthorized("Esta conta está bloqueada. Tente novamente mais tarde.");
            }

            return Unauthorized("Email ou senha inválidos.");
        }

        // --- NOVO ENDPOINT PARA REGISTRAR ADMINISTRADORES ---
        // Apenas usuários com a role "ADMIN" podem acessar este endpoint
        [Authorize(Roles = "ADMIN")] // <--- AGORA ESTÁ EM MAIÚSCULAS PARA CONDIZER COM A ROLE
        [HttpPost("registrar-admin")]
        public async Task<IActionResult> RegistrarAdmin([FromBody] RegistroUsuarioDto registroDto)
        {
            // Verifica se o corpo da requisição está válido com base nas anotações do DTO
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // busca o usuario pelo email informado no DTO
            var usuarioExistente = await _userManager.FindByEmailAsync(registroDto.Email!);

            // Verifica se o email já está em uso
            if (usuarioExistente != null)
            {
                return BadRequest("Este e-mail já está em uso.");
            }

            var cpfExistente = await _userManager.Users.FirstOrDefaultAsync(u => u.Documento == registroDto.Documento);
            if (cpfExistente != null)
            {
                return BadRequest("Este documento já esta em uso.");
            }

            var novoUsuario = new Usuario
            {
                UserName = registroDto.Email, // UserName do Identity será o email
                Email = registroDto.Email,
                Documento = registroDto.Documento,
                PhoneNumber = registroDto.Telefone,
                Perfil = "ADMIN", // Definindo o perfil como ADMIN
                NomeCompleto = registroDto.Nome // Nome completo com espaços
            };

            var resultado = await _userManager.CreateAsync(novoUsuario, registroDto.Senha!);

            if (resultado.Succeeded)
            {
                // Garante que o papel "ADMIN" existe. Se não, ele o cria.
                if (!await _roleManager.RoleExistsAsync("ADMIN"))
                {
                    await _roleManager.CreateAsync(new IdentityRole<int>("ADMIN"));
                }
                // Adiciona o novo usuário ao papel "ADMIN".
                await _userManager.AddToRoleAsync(novoUsuario, "ADMIN");

                return Ok(new { Message = "Usuário administrador registrado com sucesso!" });
            }

            foreach (var erro in resultado.Errors)
            {
                ModelState.AddModelError(string.Empty, erro.Description);
            }
            return BadRequest(ModelState);
        }

        // ENDPOINT POST - RECUPERAR SENHA   
        [HttpPost("recuperar-senha")]
        public async Task<IActionResult> RecuperarSenha([FromBody] RecuperarSenhaDto dto)
        {
            // Verifica se o corpo da requisição está válido com base nas anotações do DTO
            if (!ModelState.IsValid) return BadRequest(ModelState);

            //busca o usuário pelo email informado no DTO
            var usuario = await _userManager.FindByEmailAsync(dto.Email);
            if (usuario == null)
                return NotFound("Usuário não encontrado.");

            // gera token de redefinição de senha e cria o link para o front-end
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

                            <h3>Token de redefinição</h3>
                            <p>Se preferir, copie o token abaixo e use diretamente no Swagger ou em outro cliente:</p>
                            <p style='font-size: 18px; font-weight: bold; color: #555;'>{token}</p>

                            <p><strong>Email associado:</strong> {dto.Email}</p>

                            <br />
                            <p style='font-size: 14px; color: #999;'>Se você não solicitou essa redefinição, pode ignorar este e-mail com segurança.</p>

                            <p style='margin-top: 40px;'>Atenciosamente,<br /><em>Equipe Decolei.NET</em></p>
                          </body>
                        </html>";

            // enia o email com mensagem, token e link
            await _emailService.EnviarEmailAsync(dto.Email, "Recuperação de Senha - Decolei.Net", corpo);

            // etorna uma mensagem de sucesso
            return Ok(new { message = "Link de recuperação enviado para seu e-mail." });
        }

        // ENDPOINT POST - REDEFINIR SENHA
        [HttpPost("redefinir-senha")]
        public async Task<IActionResult> RedefinirSenha([FromBody] RedefinirSenhaDto dto)
        {
            // Verifica se o corpo da requisição está válido com base nas anotações do DTO
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // procura o usuário pelo email informado no DTO
            var usuario = await _userManager.FindByEmailAsync(dto.Email);
            if (usuario == null)
                return NotFound("Usuário não encontrado.");

            // realiza a redefinição de senha usando o token e a nova senha fornecida
            var resultado = await _userManager.ResetPasswordAsync(usuario, dto.Token, dto.NovaSenha);

            // se houver erros retorna com detalhes
            if (!resultado.Succeeded)
            {
                var erros = resultado.Errors.Select(e => e.Description);
                return BadRequest(new { errors = erros });
            }

            // mensagem de sucesso
            return Ok(new { message = "Senha redefinida com sucesso!" });
        }
         

        // ENDPOINT POST - LOGOUT
        [HttpPost("logout")]
        [Authorize] // Garante que apenas usuários logados possam chamar este endpoint
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok(new { message = "Logout realizado com sucesso!" });
        }

        // --- ENDPOINT GET PARA LISTAR TODOS OS USUÁRIOS (APENAS ADMINS) ---
        [HttpGet]
        [Authorize(Roles = "ADMIN")] // Proteção máxima!
        public async Task<IActionResult> ListarUsuarios()
        {
            // Pega todos os usuários do banco de dados
            var usuarios = await _userManager.Users.ToListAsync();

            var usuariosDto = new List<UsuarioDto>();

            // Para cada usuário, busca seu papel (role) e o mapeia para o DTO
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
                    // Pega o primeiro papel da lista (geralmente só haverá um)
                    Perfil = roles.FirstOrDefault() ?? "Sem Perfil"
                });
            }

            return Ok(usuariosDto);
        }


        // --- ENDPOINT GET PARA OBTER UM USUÁRIO PELO ID (APENAS ADMINS) ---
        [HttpGet("{id:int}")]
        [Authorize(Roles = "ADMIN,ATENDENTE")] // Proteção máxima!
        public async Task<IActionResult> ObterUsuarioPorId(int id)
        {
            // Busca o usuário pelo ID. Note que FindByIdAsync espera uma string.
            var usuario = await _userManager.FindByIdAsync(id.ToString());

            if (usuario == null)
            {
                return NotFound(new { message = $"Usuário com ID {id} não encontrado." });
            }

            // Busca os papéis (roles) do usuário encontrado
            var roles = await _userManager.GetRolesAsync(usuario);

            // Mapeia o usuário para o DTO de resposta
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
    }
}