using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Threading.Tasks;
using System.Collections.Generic;

public class AccountController : Controller
{
    private readonly ApplicationDbContext _context;
    public AccountController(ApplicationDbContext context) { _context = context; }

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string email, string password)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ModelState.AddModelError("", "Por favor, preencha todos os campos.");
            return View();
        }

        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);

        if (usuario != null && BCrypt.Net.BCrypt.Verify(password, usuario.SenhaHash))
        {
            var claims = new List<Claim>
            {
                // Claim para o ID do usuário (essencial para buscar os dados dele)
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),

            // Claim para o Nome do usuário (para exibir "Bem-vindo, [Nome]!")
            new Claim(ClaimTypes.Name, usuario.Nome)
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
            return RedirectToAction("Index", "Chamados");
        }

        ModelState.AddModelError("", "Email ou senha inválidos.");
        return View();
    }

    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(string nome, string email, string password)
    {
        if (string.IsNullOrEmpty(nome) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ModelState.AddModelError("", "Por favor, preencha todos os campos.");
            return View();
        }
        if (password.Length < 6)
        {
            ModelState.AddModelError("", "A senha deve ter no mínimo 6 caracteres.");
            return View();
        }

        if (await _context.Usuarios.AnyAsync(u => u.Email == email))
        {
            ModelState.AddModelError("email", "Este e-mail já está em uso.");
            return View();
        }

        var novoUsuario = new Usuario
        {
            Nome = nome,
            Email = email,
            SenhaHash = BCrypt.Net.BCrypt.HashPassword(password)
        };
        _context.Usuarios.Add(novoUsuario);
        await _context.SaveChangesAsync();
        return RedirectToAction("Login");
    }

    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");
    }
}
