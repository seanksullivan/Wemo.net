[![Build Status](https://sullivan.visualstudio.com/Widget/_apis/build/status/seanksullivan.Wemo.net)](https://sullivan.visualstudio.com/Widget/_build/latest?definitionId=5)

# Wemo.net
.Net Standard 2.0 (C#) library providing easy, multi-platform communications with Wemo devices.

## Wemo Communications
Communicate with your locally-accessible Wemo plug - this will not work externally via the internet (of which would require access to the Wemo network).
### Usage
Compile the project, or acquire the latest nuget package and reference:
```
using WemoNet;
```
#### Instantiate the class
```csharp
var wemo = new Wemo();
```

#### Turn-on a Wemo Plug
```csharp
wemo.TurnOnWemoPlugAsync("http://192.168.1.5").GetAwaiter().GetResult();
```
#### Turn-off a Wemo Plug
```csharp
wemo.TurnOffWemoPlugAsync("http://192.168.1.5").GetAwaiter().GetResult();
```
#### Toggle on/off a Wemo Plug
```csharp
wemo.ToggleWemoPlugAsync("http://192.168.1.5").GetAwaiter().GetResult();
```

#### Verify communications example
```csharp
var onSuccess = await wemo.TurnOnWemoPlugAsync("http://192.168.1.5");
```
```csharp
var offSuccess = await wemo.TurnOffWemoPlugAsync("http://192.168.1.5");
```
```csharp
var success = wemo.TurnOnWemoPlugAsync("http://192.168.1.5").GetAwaiter().GetResult();
```

#### Search for Wemo devices within your local network (caution, this will consume 2-3 minutes)
```csharp
var listOfDevicesFound = await wemo.GetListOfLocalWemoDevicesAsync(192, 168, 10); // First 3 local IP address octets
```
