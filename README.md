# SignalWrapper

A simple C# client library for interacting with [signal-cli-rest-api](https://github.com/bbernhard/signal-cli-rest-api). API documentation available [here](https://bbernhard.github.io/signal-cli-rest-api/).

## Installation

### Prerequisites
- .NET 10.0 or later
- Signal Messenger REST API running

### Dependencies
- RestSharp (>=113.0.0) for HTTP requests
- Newtonsoft.Json (>=13.0.4) for JSON serialization

## Quick Start
Open [http://localhost:8080/v1/qrcodelink?device_name=signal-api](http://localhost:8080/v1/qrcodelink?device_name=signal-api) in your browser and link your device. Afterwards call the REST API endpoint and send a test message:
```csharp
using SignalWrapper;

// Initialize the client
var client = new SignalApiClient("http://localhost:8080");
var about = await client.GetAboutAsync();
Console.WriteLine($"Signal API Version: {about.Version}");

// Send a message
var message = new SendMessage
{
    Number = "+1234567890",
    Recipients = new List<string> { "+0987654321" },
    Message = "Hello from SignalWrapper!"
};
var response = await client.SendMessageAsync(message);
Console.WriteLine($"Message sent at: {response.Timestamp}");
```

## License
Licensed under [WTFPL](LICENSE).