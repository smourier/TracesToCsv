# TracesToCsv
An ASP.NET Core 9+ app that continuously digest simple traces sent from HTTP(S) clients and creates CSV files from them.

## Features
- Receives traces via HTTPS PUT requests
- Parses JSON payloads containing trace data, converts trace data into CSV format and saves CSV files to a specified directory
- Lightweight and easy to deploy
- Zero dependencies
- Supports a per-trace custom dictionary mapped to dynamic CSV columns

## Use cases
- Collecting traces from any type of clients in a centralized location
- Debugging and troubleshooting applications by examining trace data
- Analyzing trace data using spreadsheet software or other tools that support CSV format
 
## How to use
The application starts listening for incoming HTTP PUT requests on the specified port, default is https://localhost:7020.

Send trace data to application using HTTP PUT requests with JSON payloads.
The JSON payload format should be like this:
```json
{
    "timestamp": "2025-09-19T11:13:53.6160989+00:00", // required. client timestamp
    "level": "info",   // required. level of trace, can be "error" or 1, "warning" or 2, "info" or 3, "verbose" or 4
    "message": "blah", // optional. a message
    "values": {        // optional. a dictionary of values
        "some bool": true,
        "a value": 1234.5677,
        "dummy": "hello world",
        "etc.": "..."
    }
}

```
### Getting an API Key
To use a service you need an identifier ("id") that is a Guid. Once you have generated a Guid, just connect to 
https://localhost:7020/traces/&lt;id&gt, for example https://localhost:7020/traces/733eb60f-2ef1-4444-8660-93a64d6b8a41 and you will see this (click on *Show Help*):





### Categorization

### C# sample Code

```csharp
var apiKey = "..."; // see above to get the key
    
var client = new HttpClient(new HttpClientHandler
{
    // optional, if your server certificate is not valid
    ServerCertificateCustomValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true,
})
{
    DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher,
    BaseAddress = new Uri("https://127.0.0.1:7020/Traces/")
};

var trace = new
{
    level = "info",
    timestamp = DateTimeOffset.UtcNow,
    message = "Hello World",
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
var url = new Uri(apiKey, UriKind.Relative);
var response = await client.PutAsync(url, content);
if (!response.IsSuccessStatusCode)
{
    // dump error message on console
    var str = await response.Content.ReadAsStringAsync();
    Console.WriteLine(str);
    response.EnsureSuccessStatusCode();
}
