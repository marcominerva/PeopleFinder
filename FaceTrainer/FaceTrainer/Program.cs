using CommandLine;
using System.Threading.Tasks;

namespace FaceTrainer
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var result = Parser.Default.ParseArguments<Options>(args);
            await result.MapResult(async options =>
                {
                    await Trainer.TrainAsync(options);
                },
                errors => Task.CompletedTask);
        }
    }
}
