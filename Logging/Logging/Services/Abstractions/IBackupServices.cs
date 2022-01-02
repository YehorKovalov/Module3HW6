using System.Threading.Tasks;

namespace Logging.Services.Abstractions
{
    public interface IBackupServices
    {
        Task Create(string report);
    }
}