using CommandLine;

namespace FaceTrainer
{
    public class Options
    {
        [Option('k', "key", HelpText = "The subscription key of the service", Required = true)]
        public string SubscriptionKey { get; set; }

        [Option('r', "region", Default = "westeurope", HelpText = "Specify the region of the service", Required = false)]
        public string Region { get; set; }

        [Option('f', "folder", HelpText = "Specify the directory that contains person and images", Required = true)]
        public string Folder { get; set; }

        [Option('d', "delete", Default = false, HelpText = "Set to just delete the Face group", Required = false)]
        public bool Delete { get; set; }
    }
}
