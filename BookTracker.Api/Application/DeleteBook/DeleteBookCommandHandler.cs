using BookTracker.Api.Storage;

namespace BookTracker.Api.Application.DeleteBook;

public class DeleteBookCommandHandler(IBookRepository bookRepository)
{
    public async Task<bool> Execute(int id) => await bookRepository.DeleteAsync(id);
}