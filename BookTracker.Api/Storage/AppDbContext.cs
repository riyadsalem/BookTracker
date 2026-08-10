using BookTracker.Api.Domain.Books;
using BookTracker.Api.Domain.Members;
using Microsoft.EntityFrameworkCore;

namespace BookTracker.Api.Storage;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options)
{
    public DbSet<Book> Books => Set<Book>(); // Table in DB
    public DbSet<Member> Members => Set<Member>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Book Table
        modelBuilder.Entity<Book>(book =>
        {
            book.Property(b => b.Title)
                .HasConversion(title => title.Value, value => new BookTitle(value))
                .HasMaxLength(BookTitle.MaxLength);
            book.Property(b => b.Author)
            .HasConversion(author => author.Value, value => new AuthorName(value))
            // author => author.Value >> TO DB (SAVE)
            // value => new AuthorName(value) >> VAN DB (READ)5
            .HasMaxLength(AuthorName.MaxLength); // Author nvarchar(100) IN DB 

            book.Property(b => b.Year)
            .HasConversion(year => year.Value, value => new PublicationYear(value));

            book.Property(b => b.Version).IsConcurrencyToken();
            // Optimistic Concurrency Control
            // Throw (DbUpdateConcurrencyException)
            // IsConcurrencyToken >> In SQL (AND Version = 1111)
        });

        // Member Table
        modelBuilder.Entity<Member>(member =>
        {
            member.Property(m => m.Name)
            .HasConversion(
                name => name.Value,
                value => new MemberName(value))
            .HasMaxLength(MemberName.MaxLength);


            member.Property(m => m.Email)
            .HasConversion(
                email => email.Value,
                value => new MemberEmail(value))
            .HasMaxLength(MemberEmail.MaxLength);

            member.HasIndex(current => current.Email).IsUnique(); // DB Validation 

            member.Property(current => current.Role)
                .HasConversion<string>()
                .HasMaxLength(50);
        });
    }
}