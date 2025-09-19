using System.Collections.Generic;
using System.Threading;

namespace TracesToCsv.Cli;

internal class Program
{
    static async Task Main()
    {
        var client = new HttpClient(new HttpClientHandler
        {
            MaxConnectionsPerServer = int.MaxValue,
            ServerCertificateCustomValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true,
        })
        {
            DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher,
            BaseAddress = new Uri("https://127.0.0.1:7020/Traces/")
            //BaseAddress = new Uri("https://tracer-dngtc6cccjghguh0.centralus-01.azurewebsites.net/traces/")
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
                    var url = new Uri($"{kv.Value}/{DateTime.Now:t}", UriKind.Relative);
                    var response = await client.PutAsync(url, content);
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
