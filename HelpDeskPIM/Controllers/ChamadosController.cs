using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using OpenAI.Chat;

[Authorize]
public class ChamadosController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IAService _ia;

    public ChamadosController(ApplicationDbContext context, IAService ia)
    {
        _context = context;
        _ia = ia;
    }

    public async Task<IActionResult> Index()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdStr, out int userId))
            return Unauthorized();

        var chamados = await _context.Chamados
            .Where(c => c.UsuarioId == userId)
            .OrderByDescending(c => c.DataAbertura)
            .ToListAsync();

        ViewData["UserName"] = User.FindFirstValue(ClaimTypes.Name);

        return View(chamados);
    }

    [HttpGet]
    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Chamado chamado)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdStr, out int userId))
            return Unauthorized();

        if (ModelState.IsValid)
        {
            chamado.UsuarioId = userId;
            _context.Chamados.Add(chamado);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        return View(chamado);
    }

    // IA geral
    [HttpPost]
    public async Task<IActionResult> PerguntarIA(string pergunta)
    {
        if (string.IsNullOrWhiteSpace(pergunta))
            return Json(new { resposta = "Envie uma pergunta válida." });

        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdStr, out int userId))
            return Unauthorized();

        var chamados = await _context.Chamados
            .Where(c => c.UsuarioId == userId)
            .ToListAsync();

        string contexto = "Chamados do usuário:\n\n";

        foreach (var c in chamados)
        {
            contexto +=
                $"Título: {c.Titulo}\n" +
                $"Descrição: {c.Descricao}\n" +
                $"Status: {c.Status}\n" +
                $"Data: {c.DataAbertura:dd/MM/yyyy}\n\n";
        }

        string promptFinal =
            contexto +
            "Com base nos chamados acima, responda e sugira soluções técnicas:\n\n" +
            pergunta;

        string resposta = await _ia.PerguntarAsync(promptFinal);

        return Json(new { resposta });
    }

    // IA para sugerir solução na tela de create
    [HttpPost]
    public async Task<IActionResult> SugerirSolucaoIA(string titulo, string descricao)
    {
        if (string.IsNullOrWhiteSpace(titulo) || string.IsNullOrWhiteSpace(descricao))
            return Json(new { resposta = "Informe título e descrição." });

        string prompt = $@"
Você é um técnico de Help Desk Nível 2.
Com base no chamado abaixo, gere uma solução objetiva e técnica.

Título: {titulo}
Descrição: {descricao}

Retorne apenas a solução técnica.
";

        string resposta = await _ia.PerguntarAsync(prompt);

        return Json(new { resposta });
    }
}

