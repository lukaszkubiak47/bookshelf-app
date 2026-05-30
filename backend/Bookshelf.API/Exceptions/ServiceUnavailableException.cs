namespace Bookshelf.API.Exceptions
{
    public class ServiceUnavailableException : BookshelfException
    {
        public override int StatusCode => 503;

        public ServiceUnavailableException(string message) : base(message)
        {
        }
    }
}
