namespace Bookshelf.API.Exceptions
{
    public class ConflictException : BookshelfException
    {
        public override int StatusCode => 409;

        public ConflictException(string message) : base(message)
        {
        }
    }
}