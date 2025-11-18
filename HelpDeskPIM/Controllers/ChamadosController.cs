using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using OpenAI.Chat; // IMPORTANTE

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
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

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
        if (ModelState.IsValid)
        {
            chamado.UsuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            _context.Chamados.Add(chamado);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        return View(chamado);
    }

    // 🚀 IA integrada — entende chamados e sugere soluções
    [HttpPost]
    public async Task<IActionResult> PerguntarIA(string pergunta)
    {
        if (string.IsNullOrWhiteSpace(pergunta))
            return Json(new { resposta = "Nenhuma pergunta recebida." });

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        // Buscar chamados do usuário
        var chamados = await _context.Chamados
            .Where(c => c.UsuarioId == userId)
            .OrderByDescending(c => c.DataAbertura)
            .ToListAsync();

        // Construir contexto enviado à IA
        string contexto = "Aqui estão os chamados do usuário:\n\n";

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
            "Com base nesses chamados, responda a pergunta a seguir e ofereça soluções técnicas claras e objetivas.\n\n" +
            "Pergunta: " + pergunta;

        // 🔥 Chama a IA pelo IAService (OpenAI 2.7.0)
        var resposta = await _ia.PerguntarAsync(promptFinal);

        return Json(new { resposta });
    }
}
