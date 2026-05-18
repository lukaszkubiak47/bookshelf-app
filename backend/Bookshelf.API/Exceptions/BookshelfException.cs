namespace Bookshelf.API.Exceptions
{
    /// <summary>
    /// Bazowa klasa wyjątków
    /// Każdy specyficzny wyjątek dziedziczy z niej i ma przypisany kod HTTP.
    /// </summary>
    public abstract class BookshelfException : Exception
    {
        public abstract int StatusCode { get; }

        protected BookshelfException(string message) : base(message)
        {
        }
    }
}