using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using Polly;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FaceTrainer
{
    public class Trainer
    {
        public static async Task TrainAsync(Options options)
        {
            var faceClient = new FaceClient(new ApiKeyServiceClientCredentials(options.SubscriptionKey))
            {
                Endpoint = $"https://{options.Region}.api.cognitive.microsoft.com"
            };

            var fullFolder = Path.GetFullPath(options.Folder);
            var groupName = Path.GetFileName(fullFolder).ToLower();

            try
            {
                if (options.Delete)
                {
                    Console.WriteLine($"Deleting group \"{groupName}\"...");
                    try
                    {
                        await faceClient.PersonGroup.DeleteAsync(groupName);
                        Console.WriteLine($"Group \"{groupName}\" successfully deleted.");
                    }
                    catch
                    {
                    }

                    return;
                }

                if (!Directory.Exists(fullFolder))
                {
                    Console.WriteLine($"Error: folder \"{fullFolder}\" does not exist.");
                    Console.WriteLine(string.Empty);
                    return;
                }

                try
                {
                    // Test whether the group already exists.
                    Console.WriteLine($"Checking if \"{groupName}\" already exists...");
                    await faceClient.PersonGroup.GetAsync(groupName);

                    Console.WriteLine($"Group \"{groupName}\" exists. Deleting...");
                    await faceClient.PersonGroup.DeleteAsync(groupName);
                }
                catch (Exception ex) when ((ex as APIErrorException)?.Body.Error.Code == "PersonGroupNotFound")
                {
                    Console.WriteLine($"Group {groupName} did not exist.");
                }

                var policy = Policy
                            .Handle<Exception>(ex => (ex.GetBaseException() as APIErrorException)?.Body.Error.Code == "RateLimitExceeded")
                            .WaitAndRetryForeverAsync(
                            retryAttempt => TimeSpan.FromSeconds(Math.Pow(3, retryAttempt)),
                            (exception, timeSpan) =>
                            {
                                Console.WriteLine("Rate limit exceeded. Throttling...");
                            });

                Console.WriteLine($"Creating group \"{groupName}\"...");
                await faceClient.PersonGroup.CreateAsync(groupName, groupName, groupName);

                Console.WriteLine("Preparing faces for identification, detecting faces in chosen folder...");
                foreach (var dir in Directory.EnumerateDirectories(fullFolder).Where(f => !Path.GetFileName(f).StartsWith("!")))
                {
                    var tag = Path.GetFileName(dir);
                    var p = new Person { Name = tag };

                    Console.WriteLine($"\nCreating person \"{p.Name}\"...");

                    await policy.ExecuteAsync(async () =>
                    {
                        p.PersonId = (await faceClient.PersonGroupPerson.CreateAsync(groupName, p.Name)).PersonId;
                    });

                    foreach (var file in Directory.EnumerateFiles(dir, "*.*", SearchOption.AllDirectories)
                               .Where(s => s.EndsWith(".jpg", StringComparison.InvariantCultureIgnoreCase) || s.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase) || s.EndsWith(".bmp", StringComparison.InvariantCultureIgnoreCase) || s.EndsWith(".gif", StringComparison.InvariantCultureIgnoreCase)))
                    {
                        var faceTag = Path.GetFileName(file);
                        Console.WriteLine($"Adding face \"{faceTag}\" to person \"{p.Name}\"...");

                        await policy.ExecuteAsync(async () =>
                        {
                            // Update person face on server side.
                            using var stream = File.OpenRead(file);
                            await faceClient.PersonGroupPerson.AddFaceFromStreamAsync(groupName, p.PersonId, stream, p.Name);
                        });
                    }
                }

                Console.WriteLine($"\nTraining group \"{groupName}\"...");

                await policy.ExecuteAsync(async () =>
                {
                    await faceClient.PersonGroup.TrainAsync(groupName);
                });

                await policy.ExecuteAsync(async () =>
                {
                    // Wait until train completed.
                    while (true)
                    {
                        await Task.Delay(1000);
                        var status = await faceClient.PersonGroup.GetTrainingStatusAsync(groupName);
                        Console.WriteLine($"Group \"{groupName}\" training process is {status.Status}.");

                        if (status.Status != TrainingStatusType.Running)
                        {
                            Console.WriteLine(status.Message);
                            break;
                        }
                    }
                });

                Console.WriteLine("Training completed.\n");
            }

            catch (Exception ex) when (ex.GetBaseException() is APIErrorException error)
            {
                Console.WriteLine($"\nError: {error.Body.Error.Message}.\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nUnexpected error: {ex.GetBaseException()?.Message}.\n");
            }
        }
    }
}
