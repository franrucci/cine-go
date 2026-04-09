using cine_go_mvc.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace cine_go_mvc.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly SignInManager<Usuario> _signInManager;
        public UsuarioController(UserManager<Usuario> userManager, SignInManager<Usuario> signInManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string user)
        {
            return View();
        }

        public IActionResult Registro()
        {
            return View();
        }
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Registro(RegistroViewModel usuario)
        {
            if (ModelState.IsValid)
            {
                var nuevoUsuario = new Usuario
                {
                    UserName = usuario.Email,
                    Email = usuario.Email,
                    Nombre = usuario.Nombre,
                    Apellido = usuario.Apellido,
                    ImagenUrlPerfil = "default-profile.png"
                };
                var resultado = await _userManager.CreateAsync(nuevoUsuario, usuario.Clave);
                if (resultado.Succeeded) // si se creó el usuario correctamente, lo logueamos automáticamente
                {
                    await _signInManager.SignInAsync(nuevoUsuario, isPersistent: false); // isPersistent: false significa que la sesión no se mantendrá después de cerrar el navegador
                    return RedirectToAction("Index", "Home"); // redirigimos al usuario a la página principal después de registrarse y loguearse
                }
                else
                {
                    foreach (var error in resultado.Errors) // si hubo errores al crear el usuario, los agregamos al ModelState para mostrarlos en la vista
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            return View(usuario);
        }

        public IActionResult Logout()
        {
            return View();
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

    }
}
