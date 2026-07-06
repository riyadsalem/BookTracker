using BookTracker.Api.Storage.Books;
namespace BookTracker.Api.Application.Books.DeleteBook;

public class DeleteBookCommandHandler(IBookRepository bookRepository) : IHandler
{
    public async Task<bool> Execute(int id) => await bookRepository.DeleteAsync(id);
}