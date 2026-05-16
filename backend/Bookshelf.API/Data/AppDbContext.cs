using Microsoft.EntityFrameworkCore;
using Bookshelf.API.Models.Entities;

namespace Bookshelf.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Tabele w bazie danych
        public DbSet<User> Users { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<UserBook> UserBooks { get; set; }
        public DbSet<TransactionRequest> TransactionRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // === INDEKSY UNIKALNE ===
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<Book>()
                .HasIndex(b => b.GoogleBooksId)
                .IsUnique()
                .HasFilter("[GoogleBooksId] IS NOT NULL");

            // === RELACJE I KASKADOWOŚĆ ===

            // User -> UserBook (Restrict — nie kasuj usera jeśli ma oferty)
            modelBuilder.Entity<UserBook>()
                .HasOne(ub => ub.User)
                .WithMany(u => u.UserBooks)
                .HasForeignKey(ub => ub.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Book -> UserBook (Restrict)
            modelBuilder.Entity<UserBook>()
                .HasOne(ub => ub.Book)
                .WithMany(b => b.UserBooks)
                .HasForeignKey(ub => ub.BookId)
                .OnDelete(DeleteBehavior.Restrict);

            // Genre -> Book (SetNull — usunięcie gatunku nie kasuje książek)
            modelBuilder.Entity<Book>()
                .HasOne(b => b.Genre)
                .WithMany(g => g.Books)
                .HasForeignKey(b => b.GenreId)
                .OnDelete(DeleteBehavior.SetNull);

            // User -> TransactionRequest (Restrict)
            modelBuilder.Entity<TransactionRequest>()
                .HasOne(tr => tr.Requester)
                .WithMany(u => u.TransactionRequests)
                .HasForeignKey(tr => tr.RequesterId)
                .OnDelete(DeleteBehavior.Restrict);

            // UserBook -> TransactionRequest (Cascade — jedyny cascade)
            modelBuilder.Entity<TransactionRequest>()
                .HasOne(tr => tr.UserBook)
                .WithMany(ub => ub.TransactionRequests)
                .HasForeignKey(tr => tr.UserBookId)
                .OnDelete(DeleteBehavior.Cascade);

            // === SEED GATUNKÓW ===
            modelBuilder.Entity<Genre>().HasData(
                new Genre { Id = Guid.Parse("a1111111-1111-1111-1111-111111111111"), Name = "Fantasy" },
                new Genre { Id = Guid.Parse("a2222222-2222-2222-2222-222222222222"), Name = "Kryminał" },
                new Genre { Id = Guid.Parse("a3333333-3333-3333-3333-333333333333"), Name = "Romans" },
                new Genre { Id = Guid.Parse("a4444444-4444-4444-4444-444444444444"), Name = "Sci-Fi" },
                new Genre { Id = Guid.Parse("a5555555-5555-5555-5555-555555555555"), Name = "Literatura piękna" },
                new Genre { Id = Guid.Parse("a6666666-6666-6666-6666-666666666666"), Name = "Biografia" },
                new Genre { Id = Guid.Parse("a7777777-7777-7777-7777-777777777777"), Name = "Poradniki" },
                new Genre { Id = Guid.Parse("a8888888-8888-8888-8888-888888888888"), Name = "Historia" },
                new Genre { Id = Guid.Parse("a9999999-9999-9999-9999-999999999999"), Name = "Horror" },
                new Genre { Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), Name = "Inne" }
            );
        }
    }
}