using cine_go_mvc.Data;
using cine_go_mvc.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.CodeDom;

namespace cine_go_mvc.Controllers
{
    public class ReviewController : Controller
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly CineDbContext _context;
        public ReviewController(UserManager<Usuario> userManager, CineDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }
        // GET: ReviewController
        // autorizar a todo menos a los administradores porque los administradores no pueden tener reviews
        [Authorize]
        public async Task<ActionResult> Index() // Muestra las reviews del usuario logueado
        {
            // si el usuario es admin, se le redirige a la vista de detalles de la pelicula que esta editando
            if (User.IsInRole("Admin"))
                return Forbid();

            var userId = _userManager.GetUserId(User);
            var reviews = await _context.Reviews
                .Include(r => r.Pelicula)
                .Where(r => r.UsuarioId == userId)
                .ToListAsync();

            // en caso de que el usuario no tenga reviews, se devuelve un mensaje indicando que no tiene reviews
            if (reviews.Count == 0)
            {
                ViewBag.Mensaje = "No tienes reseñas realizadas.";
            }

            return View(reviews);
        }

        // GET: ReviewController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: ReviewController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ReviewController/Create
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ReviewCreateViewModel review)
        {
            try
            {
                review.UsuarioId = _userManager.GetUserId(User);

                //Validación de si ya existe una review del mismo usuario.
                var reviewExiste = _context.Reviews
                    .FirstOrDefault(r => r.PeliculaId == review.PeliculaId && r.UsuarioId == review.UsuarioId);
                if (reviewExiste != null)
                {
                    TempData["ReviewExiste"] = "Ya has realizado una reseña para esta película.";
                    return RedirectToAction("Details", "Home", new { id = review.PeliculaId });
                }
                //Fin validación.

                if (ModelState.IsValid)
                {
                    var nuevaReview = new Review
                    {
                        PeliculaId = review.PeliculaId,
                        UsuarioId = review.UsuarioId,
                        Rating = review.Rating,
                        Comentario = review.Comentario,
                        FechaReview = DateTime.Now
                    };
                    _context.Reviews.Add(nuevaReview);
                    _context.SaveChanges();
                    return RedirectToAction("Details", "Home", new { id = review.PeliculaId });
                }

                return View(review);
            }
            catch
            {
                return View(review);
            }
        }

        // GET: ReviewController/Edit/5
        [Authorize]
        public async Task<ActionResult> Edit(int id)
        {
            var review = _context.Reviews
                .Include(r => r.Pelicula) // Incluir la película para mostrar su título en la vista
                .FirstOrDefault(r => r.Id == id);
            if (review == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User); // Obtener el ID del usuario actual
            if (review.UsuarioId != user.Id && !await _userManager.IsInRoleAsync(user, "Admin")) // Verificar que el usuario es el autor de la review o un administrador
                return Forbid(); // Si no es el autor o un administrador, denegar el acceso

            var reviewViewModel = new ReviewCreateViewModel
            {
                Id = review.Id,
                PeliculaId = review.PeliculaId,
                UsuarioId = review.UsuarioId,
                Rating = review.Rating,
                Comentario = review.Comentario,
                PeliculaTitulo = review.Pelicula?.Titulo
            };

            return View(reviewViewModel);
        }

        // POST: ReviewController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(ReviewCreateViewModel review)
        {
            try
            {

                if (ModelState.IsValid)
                {
                    var reviewExistente = _context.Reviews.FirstOrDefault(r => r.Id == review.Id);
                    if (reviewExistente == null)
                        return NotFound();

                    var user = await _userManager.GetUserAsync(User); // Obtener el ID del usuario actual
                    if (review.UsuarioId != user.Id && !await _userManager.IsInRoleAsync(user, "Admin")) // Verificar que el usuario es el autor de la review o un administrador
                        return Forbid(); // Si no es el autor o un administrador, denegar el acceso

                    reviewExistente.Rating = review.Rating;
                    reviewExistente.Comentario = review.Comentario;
                    _context.Reviews.Update(reviewExistente);
                    _context.SaveChanges();
                    // Si es admin lo debe redirigir a la vista de detalles de la pelicula que esta editando
                    if (await _userManager.IsInRoleAsync(user, "Admin"))
                    {
                        return RedirectToAction("Details", "Home", new { id = review.PeliculaId });
                    }
                    else
                    {
                        return RedirectToAction("Index", "Review");
                    }
                }

                return View(review);
            }
            catch
            {
                return View(review);
            }
        }

        // GET: ReviewController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: ReviewController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
