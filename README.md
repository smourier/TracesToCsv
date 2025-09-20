# TracesToCsv
An ASP.NET Core 9+ application that continuously ingests lightweight trace payloads sent over HTTP(S) and converts them into downloadable CSV files.

## Features
- Accepts traces via HTTPS PUT requests
- Reads JSON trace payloads, converts them to CSV rows, and stores them in a server-side folder
- Simple hierarchical categorization of traces using URL path segments
- Minimal footprint: publish and run
- No third-party dependencies
- Supports per-trace dynamic key/value pairs mapped to expandable CSV columns

## Use cases
- Centralized collection of diagnostic or telemetry traces from diverse clients
- Application troubleshooting by inspecting structured trace history
- Offline analysis in spreadsheets or any CSV-compatible tooling

## How to use
The app listens for HTTPS PUT requests on the configured port (default: https://localhost:7020).

Send trace data as JSON in the following shape:
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
Generate a Guid to serve as the trace identifier (the "id"). This guid is yours and yours only. Visit  
`https://localhost:7020/traces/<id>` (example: https://localhost:7020/traces/733eb60f2ef14444866093a64d6b8a41) and open *Show Help*.

<img width="907" height="125" alt="Show Help" src="https://github.com/user-attachments/assets/9cba8871-482b-466a-b6a7-4a0a96ac3ba7" />

The displayed API Key (example: `iElBVk8qlQd5h3nmXdbu8DNLGUH3AlJHHPAiNGb0eLvaM9Roxf7s556hUNzX4nk6`) is what clients use to submit traces associated with that Guid.  
> Treat the Guid more carefully than the API Key: possession of the Guid allows browsing of the related CSV output. Avoid embedding the Guid directly in distributed binaries or sharing it publicly.

### Server configuration
Configure a server-side password in `appsettings.json`:

```json
{
  "Traces": {
    "Password": "put_your_password_here"
  }
}
```
This password is used to derive API Keys from their Guids. Changing it later invalidates previously issued API Keys, so pick a value early and keep it stable.

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
```
Here is the result when connecting to the server:

<img width="874" height="352" alt="CSV Traces" src="https://github.com/user-attachments/assets/f50d0377-d9c8-484c-81cf-71072cd6e3dc" />

### Categorization
You can categorize traces by appending path segments: `https://localhost:7020/traces/<id>/cat/cat2`. The server places the resulting CSV into a matching subdirectory structure.

Example client call:

```csharp
var response = await client.PutAsync(url + "/cat/cat2", content);
```

Result: the UI understands `cat` and `cat2` in url like nested "folders".

<img width="886" height="383" alt="Categorization" src="https://github.com/user-attachments/assets/a1bafcf5-85a1-4c04-942b-ed430aeb2efa" />

