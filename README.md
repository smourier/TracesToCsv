# TracesToCsv
An ASP.NET Core 9+ app that continuously digest simple traces sent from HTTP(S) clients and creates CSV files from them.

## Features
- Receives traces via HTTPS PUT requests
- Parses JSON payloads containing trace data, converts trace data into CSV format and saves CSV files to a specified directory
- Lightweight and easy to deploy (just run Publish)
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
`https://localhost:7020/traces/<id>`, for example https://localhost:7020/traces/733eb60f2ef14444866093a64d6b8a41 and you will see this (click on *Show Help*):

<img width="907" height="125" alt="Show Help" src="https://github.com/user-attachments/assets/9cba8871-482b-466a-b6a7-4a0a96ac3ba7" />

So, as we can see, the API Key for this id is `iElBVk8qlQd5h3nmXdbu8DNLGUH3AlJHHPAiNGb0eLvaM9Roxf7s556hUNzX4nk6` this is how we can send traces to the server and they will be associated with the id above.
> The API Key is not super secret, the id (the Guid) is more since this is the one that allows anyone to see related CSV files. So you shouldn't communicate the id to anyone nor embed it in your code.

### Server configuration
You can change the appsettings.json with a password of your choice:

```json
{
  "Traces": {
    "Password": "put_your_password_here"
  }
}
```
This password is a server-side only thing that is used to build all API Keys from guids. If you change it all API keys will change, so once you've defined it you shouldn't change it.

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

### Categorization
TracesToCsv has a feature that allows you to categorize traces using relative path. So for example if you PUT the traces to `https://localhost:7020/traces/<id>/cat1/cat2` then the CSV file that will contain this trace will be put in a sub directory:
