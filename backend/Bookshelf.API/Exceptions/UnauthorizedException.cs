namespace Bookshelf.API.Exceptions
{
    public class UnauthorizedException : BookshelfException
    {
        public override int StatusCode => 401;

        public UnauthorizedException(string message) : base(message)
        {
        }
    }
}