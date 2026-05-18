namespace Bookshelf.API.Exceptions
{
    public class BadRequestException : BookshelfException
    {
        public override int StatusCode => 400;

        public BadRequestException(string message) : base(message)
        {
        }
    }
}