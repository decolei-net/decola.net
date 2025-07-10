using Decolei.net.DTOs;
using Decolei.net.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Decolei.net.Controllers
{
    [ApiController]
    [Route("admin")]
    public class AdminController : ControllerBase
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly SignInManager<Usuario> _signInManager;

        public AdminController(UserManager<Usuario> userManager, SignInManager<Usuario> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email); // Busca usu�rio pelo email

            if (user == null || user.Perfil != "admin") // Verifica se existe e se � admin
                return Unauthorized("Usu�rio n�o autorizado.");

            var result = await _signInManager.PasswordSignInAsync(user, request.Senha, false, false);

            if (!result.Succeeded)
                return Unauthorized("Credenciais inv�lidas.");

            return Ok(new { message = "Login bem-sucedido", redirectUrl = "/admin/dashboard" });
        }
    }
}
