using cine_go_mvc.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace cine_go_mvc.Data
{
    public class CineDbContext : IdentityDbContext<Usuario>
    {
        public CineDbContext(DbContextOptions<CineDbContext> options) : base(options)
        {
            
        }
        public DbSet<Pelicula> Peliculas { get; set; }
        public DbSet<Genero> Generos { get; set; }
        public DbSet<Plataforma> Plataformas { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Favorito> Favoritos { get; set; }
    }
}
