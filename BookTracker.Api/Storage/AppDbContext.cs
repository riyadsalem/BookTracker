using BookTracker.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace BookTracker.Api.Storage;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options)
{
    public DbSet<Book> Books => Set<Book>(); // Table in DB

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Book>(book =>
        {
            book.Property(b => b.Title)
                .HasConversion(
                    title => title.Value,
                    value => new BookTitle(value))
                .HasMaxLength(BookTitle.MaxLength);
            book.Property(b => b.Author)
            .HasConversion(author => author.Value, value => new AuthorName(value))
            // author => author.Value >> TO DB (SAVE)
            // value => new AuthorName(value) >> VAN DB (READ)
            .HasMaxLength(AuthorName.MaxLength); // Author nvarchar(100) IN DB 
        });
    }
}