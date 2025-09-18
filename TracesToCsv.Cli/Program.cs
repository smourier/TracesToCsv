namespace TracesToCsv.Cli;

internal class Program
{
    static async Task Main()
    {
        var client = new HttpClient(new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true,
        });

        client.BaseAddress = new Uri("https://127.0.0.1:7020/traces/");
        var id = Guid.NewGuid();


        // get key
        var key = await client.GetStringAsync($"{id}/key");
        Console.WriteLine(key);

        for (var i = 0; i < 10; i++)
        {
            var trace = new
            {
                version = 1,
                level = "info",
                timestamp = DateTimeOffset.UtcNow,
            };
            var content = JsonContent.Create(trace);
            var response = await client.PutAsync($"{WebUtility.UrlEncode(key)}", content);

            var str = await response.Content.ReadAsStringAsync();
            Console.WriteLine(str);
            response.EnsureSuccessStatusCode();
        }
    }
}
