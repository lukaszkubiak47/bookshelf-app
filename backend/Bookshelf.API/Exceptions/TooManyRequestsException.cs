namespace Bookshelf.API.Exceptions
{
    public class TooManyRequestsException : BookshelfException
    {
        public override int StatusCode => 429;

        public TooManyRequestsException(string message) : base(message)
        {
        }
    }
}
