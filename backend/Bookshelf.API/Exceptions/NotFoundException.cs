namespace Bookshelf.API.Exceptions
{
    public class NotFoundException : BookshelfException
    {
        public override int StatusCode => 404;

        public NotFoundException(string message) : base(message)
        {
        }
    }
}