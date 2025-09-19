using System.Collections.Generic;
using System.Threading;

namespace TracesToCsv.Cli;

internal class Program
{
    static async Task Main()
    {
        var client = new HttpClient(new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true,
        })
        {
            BaseAddress = new Uri("https://127.0.0.1:7020/Traces/")
        };

        var ids = new ConcurrentDictionary<Guid, string>();
        for (var i = 1; i <= 10; i++)
        {
            var id = new Guid($"00000000-0000-0000-0000-{i:D12}");

            var key = await client.GetStringAsync($"{id}/key");
            Console.WriteLine(key);

            ids[id] = key;
            Console.WriteLine($"Registered {id} with key {key}");
        }

        var index = 0;
        var tasks = new List<Task>();
        for (var i = 0; i < 100; i++)
        {
            foreach (var kv in ids)
            {
                tasks.Add(Task.Run(async () =>
                {
                    var idx = Interlocked.Increment(ref index);
                    // get key
                    var trace = new
                    {
                        level = "info",
                        timestamp = DateTimeOffset.UtcNow,
                        message = $"{kv.Key} #{idx}",
                        values = new Dictionary<string, object>()
                        {
                            { "bool", true },
                            { "single", 1234.5678f},
                            { "double", 1234.5678},
                            { "timespan", TimeSpan.FromDays(1234.5679)},
                            { "decimal", 1234.5678m }
                        }
                    };

                    var content = JsonContent.Create(trace);
                    var response = await client.PutAsync($"{Uri.EscapeDataString(kv.Value)}/{DateTime.Now:t}", content);
                    if (!response.IsSuccessStatusCode)
                    {
                        var str = await response.Content.ReadAsStringAsync();
                        Console.WriteLine(str);
                        response.EnsureSuccessStatusCode();
                    }
                }));
            }
        }

        Console.WriteLine($"tasks: {tasks.Count}");
        Task.WaitAll(tasks);
    }
}
