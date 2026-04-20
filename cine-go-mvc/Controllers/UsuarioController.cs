using cine_go_mvc.Models;
using cine_go_mvc.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace cine_go_mvc.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly SignInManager<Usuario> _signInManager;
        private readonly ImagenStorage _imagenStorage;
        //private readonly IEmailService _emailService;
        public UsuarioController(UserManager<Usuario> userManager, SignInManager<Usuario> signInManager, ImagenStorage imagenStorage/*, IEmailService emailService*/)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _imagenStorage = imagenStorage;
            //_emailService = emailService;
        }
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel usuario)
        {
            if (ModelState.IsValid)
            {
                var resultado = await _signInManager.PasswordSignInAsync(usuario.Email, usuario.Clave, usuario.Recordarme, lockoutOnFailure: false);
                if (resultado.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Inicio de sesión inválido");
                }
            }
            return View(usuario);
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
                    ImagenUrlPerfil = "/images/default-avatar.png"
                };
                var resultado = await _userManager.CreateAsync(nuevoUsuario, usuario.Clave);
                if (resultado.Succeeded) // si se creó el usuario correctamente, lo logueamos automáticamente
                {
                    await _signInManager.SignInAsync(nuevoUsuario, isPersistent: false); // isPersistent: false significa que la sesión no se mantendrá después de cerrar el navegador
                    //await _emailService.SendAsync(nuevoUsuario.Email, "Bienvenido a CineGo!", $"Hola {nuevoUsuario.Nombre}, gracias por registrarte en CineGo. ¡Disfruta de tu experiencia cinematográfica!"); // enviamos un email de bienvenida al nuevo usuario
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
            _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> MiPerfil()
        {
            var usuarioActual = await _userManager.GetUserAsync(User);

            var usuarioVM = new MiPerfilViewModel
            {
                Nombre = usuarioActual.Nombre,
                Apellido = usuarioActual.Apellido,
                Email = usuarioActual.Email,
                ImagenUrlPerfil = usuarioActual.ImagenUrlPerfil
            };

            return View(usuarioVM);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MiPerfil(MiPerfilViewModel usuarioVM)
        {
            if (ModelState.IsValid)
            {
                var usuarioActual = await _userManager.GetUserAsync(User);

                try
                {
                    if (usuarioVM.ImagenPerfil is not null && usuarioVM.ImagenPerfil.Length > 0)
                    {
                        // opcional: borrar la anterior (si no es placeholder)
                        if (!string.IsNullOrWhiteSpace(usuarioActual.ImagenUrlPerfil))
                            await _imagenStorage.DeleteAsync(usuarioActual.ImagenUrlPerfil);

                        var nuevaRuta = await _imagenStorage.SaveAsync(usuarioActual.Id, usuarioVM.ImagenPerfil);
                        usuarioActual.ImagenUrlPerfil = nuevaRuta;
                        usuarioVM.ImagenUrlPerfil = nuevaRuta;
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                    return View(usuarioVM);
                }

                usuarioActual.Nombre = usuarioVM.Nombre;
                usuarioActual.Apellido = usuarioVM.Apellido;

                var resultado = await _userManager.UpdateAsync(usuarioActual);

                if (resultado.Succeeded)
                {
                    ViewBag.Mensaje = "Perfil actualizado con éxito.";
                    return View(usuarioVM);
                }
                else
                {
                    foreach (var error in resultado.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            return View(usuarioVM);
        }
    }
}
