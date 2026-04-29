using cine_go_mvc.Data;
using cine_go_mvc.Models;
using cine_go_mvc.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;

namespace cine_go_mvc.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly CineDbContext _context;
        private const int PageSize = 8;
        private readonly LlmService _llmService;

        public HomeController(ILogger<HomeController> logger, CineDbContext context, LlmService llmService)
        {
            _logger = logger;
            _context = context;
            _llmService = llmService;
        }

        public async Task<IActionResult> Index(int pagina = 1, string txtBusqueda = "", int generoId = 0, int origenPagina = 0)
        {
            if (pagina < 1) pagina = 1;
            if (!string.IsNullOrEmpty(txtBusqueda))
            {
                if (origenPagina == 0)
                {
                    origenPagina = pagina;
                }
            }
            else
            {
                origenPagina = pagina;
            }

            var consulta = _context.Peliculas.AsQueryable(); // la consulta todavia no se ejecuta, solo se construye
            if (!string.IsNullOrEmpty(txtBusqueda))
            {
                consulta = consulta.Where(p => p.Titulo.Contains(txtBusqueda));
            }

            if (generoId > 0)
            {
                consulta = consulta.Where(p => p.GeneroId == generoId);
            }

            var totalPeliculas = await consulta.CountAsync();
            var totalPaginas = (int)Math.Ceiling(totalPeliculas / (double)PageSize);

            if (pagina > totalPaginas && totalPaginas > 0) pagina = totalPaginas;

            var peliculas = await consulta
                .Skip((pagina - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = totalPaginas;
            ViewBag.TotalPeliculas = totalPeliculas;
            ViewBag.TxtBusqueda = txtBusqueda;
            ViewBag.OrigenPagina = origenPagina;

            var generos = await _context.Generos.OrderBy(g => g.Descripcion).ToListAsync();
            generos.Insert(0, new Genero { Id = 0, Descripcion = "Genero" }); // si es 0 , se muestran todas las peliculas sin filtrar por genero
            ViewBag.Generos = new SelectList(
                generos,
                "Id", // clave
                "Descripcion", // valor
                generoId
                );
            ViewBag.GeneroId = generoId;

            return View(peliculas);
        }

        public async Task<IActionResult> Details(int id)
        {
            var pelicula = await _context.Peliculas
                .Include(p => p.Genero)
                .Include(p => p.ListaReviews)
                .ThenInclude(r => r.Usuario)
                .FirstOrDefaultAsync(p => p.Id == id);

            ViewBag.UserReview = false;
            if (User?.Identity?.IsAuthenticated == true && pelicula.ListaReviews != null)
            {
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                ViewBag.UserReview = !(pelicula.ListaReviews.FirstOrDefault(r => r.UsuarioId == userId) == null);
            }

            // Calculo de promedio, conteos y porcentajes de las calificaciones para mostrar las estrellas
            if (pelicula != null)
            {
                var reviews = pelicula.ListaReviews ?? new List<Review>();
                int total = reviews.Count;
                double avg = total > 0 ? reviews.Average(r => r.Rating) : 0.0;

                ViewBag.Promedio = Math.Round(avg, 1);
                ViewBag.Average = avg; // valor exacto (double)
                ViewBag.AveragePercent = total > 0 ? (avg / 5.0) * 100.0 : 0.0;
                ViewBag.TotalReviews = total;

                var counts = new int[6]; // índices 1..5
                for (int i = 1; i <= 5; i++) counts[i] = reviews.Count(r => r.Rating == i);
                ViewBag.RatingCounts = counts;

                var percentages = new int[6];
                for (int i = 1; i <= 5; i++)
                {
                    percentages[i] = total > 0 ? (int)Math.Round(counts[i] * 100.0 / total) : 0;
                }
                ViewBag.RatingPercentages = percentages;
            }
            else
            {
                ViewBag.Promedio = 0.0;
                ViewBag.RoundedPromedio = 0.0;
                ViewBag.TotalReviews = 0;
                ViewBag.RatingCounts = new int[6];
                ViewBag.RatingPercentages = new int[6];
            }

            return View(pelicula);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        public async Task<IActionResult> Spoiler(string titulo)
        {
            try
            {
                var spoiler = await _llmService.ObtenerSpoilerAsync(titulo);
                return Json(new { success = true, data = spoiler });

            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Resumen(string titulo)
        {
            try
            {
                var resumen = await _llmService.ObtenerResumenAsync(titulo);
                return Json(new { success = true, data = resumen });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
