using Decolei.net.DTOs;
using Decolei.net.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Decolei.net.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly SignInManager<Usuario> _signInManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager; // Adicionado para gerenciar papéis

        public UsuarioController(
            UserManager<Usuario> userManager,
            SignInManager<Usuario> signInManager,
            RoleManager<IdentityRole<int>> roleManager) // Adicionado ao construtor
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
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

            var novoUsuario = new Usuario
            {
                UserName = registroDto.Nome,
                Email = registroDto.Email,
                Documento = registroDto.Documento,
                PhoneNumber = registroDto.Telefone,
                Perfil = "CLIENTE"
            };

            var resultado = await _userManager.CreateAsync(novoUsuario, registroDto.Senha!);

            if (resultado.Succeeded)
            {
                // --- LÓGICA DE PAPÉIS ---
                // Garante que o papel "CLIENTE" existe no banco. Se não, ele o cria.
                if (!await _roleManager.RoleExistsAsync("CLIENTE"))
                {
                    await _roleManager.CreateAsync(new IdentityRole<int>("CLIENTE"));
                }
                // Adiciona o novo usuário ao papel "CLIENTE".
                await _userManager.AddToRoleAsync(novoUsuario, "CLIENTE");

                // Faz o login do usuário recém-criado.
                await _signInManager.SignInAsync(novoUsuario, isPersistent: false);
                return Ok(new { Message = "Usuário registrado com sucesso!" });
            }

            foreach (var erro in resultado.Errors)
            {
                ModelState.AddModelError(string.Empty, erro.Description);
            }
            return BadRequest(ModelState);
        }

        // --- ENDPOINT DE LOGIN (LÓGICA CORRIGIDA) ---
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUsuarioDto loginDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // 1. BUSCAR O USUÁRIO PELO E-MAIL
            // Em vez de deixar o SignInManager fazer isso, nós fazemos manualmente.
            var usuario = await _userManager.FindByEmailAsync(loginDto.Email!);

            // Se o usuário não for encontrado, retornamos um erro genérico por segurança.
            if (usuario == null)
            {
                return Unauthorized("Email ou senha inválidos.");
            }

            // 2. VERIFICAR A SENHA
            // Agora que temos o objeto 'usuario', usamos CheckPasswordSignInAsync.
            // Este método apenas compara a senha, sem tentar encontrar o usuário novamente.
            var resultado = await _signInManager.CheckPasswordSignInAsync(usuario, loginDto.Senha!, lockoutOnFailure: true);

            if (resultado.Succeeded)
            {
                // 3. FAZER O LOGIN
                // Se a senha estiver correta, nós explicitamente fazemos o login.
                await _signInManager.SignInAsync(usuario, isPersistent: false);
                return Ok(new { Message = "Login bem-sucedido!" });
            }

            if (resultado.IsLockedOut)
            {
                return Unauthorized("Esta conta está bloqueada. Tente novamente mais tarde.");
            }

            // Se a senha estiver incorreta, retornamos o mesmo erro genérico.
            return Unauthorized("Email ou senha inválidos.");
        }
    }
}
