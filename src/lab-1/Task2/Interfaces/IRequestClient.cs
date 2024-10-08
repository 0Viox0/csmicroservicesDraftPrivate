using Task2.Models;

namespace Task2.Interfaces;

public interface IRequestClient
{
    Task<ResponseModel> SendAsync(RequestModel request, CancellationToken cancellationToken);
}