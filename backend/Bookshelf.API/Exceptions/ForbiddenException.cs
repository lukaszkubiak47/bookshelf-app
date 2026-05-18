namespace Bookshelf.API.Exceptions
{
    public class ForbiddenException : BookshelfException
    {
        public override int StatusCode => 403;

        public ForbiddenException(string message) : base(message)
        {
        }
    }
}