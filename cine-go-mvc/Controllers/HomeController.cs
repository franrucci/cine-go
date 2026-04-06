using cine_go_mvc.Data;
using cine_go_mvc.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace cine_go_mvc.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly CineDbContext _context;

        public HomeController(ILogger<HomeController> logger, CineDbContext cineDbContext)
        {
            _logger = logger;
            _context = cineDbContext;
        }

        public async Task<IActionResult> Index()
        {
            var peliculas = await _context.Peliculas.ToListAsync();
            return View(peliculas);
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
    }
}
