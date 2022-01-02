using System;
using System.Threading.Tasks;

namespace Logging.Services.Abstractions
{
    public interface ILogger : IAsyncDisposable
    {
        event Func<string, Task> OnBackedUp;
        Task LogInfo<T>(T message);
        Task LogError<T>(T message);
    }
}
