namespace Task3.Bll.CustomExceptions;

public class OrderException : Exception
{
    public OrderException(string message)
        : base(message) { }

    public OrderException(string message, Exception innerException)
        : base(message, innerException) { }
}