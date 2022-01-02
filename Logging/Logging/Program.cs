using System.Threading.Tasks;

namespace Logging
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var startup = new Startup();
            await startup.Run();
        }
    }
}
