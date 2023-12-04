# HelloCSharp
C# implementation of Hello Network. Things of note:

* [Signals](https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/c_sharp_signals.html) are now connected via [C# events](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/events/how-to-subscribe-to-and-unsubscribe-from-events):

```csharp
Multiplayer.PeerConnected +=
  (peer_id) => GD.Print($"{Multiplayer.GetUniqueId()} now connected to {peer_id}");
```

* In this implementation I use [export attributes]([https://learn.microsoft.com/en-us/dotnet/csharp/advanced-topics/reflection-and-attributes/#using-attributes](https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/c_sharp_exports.html)) to avoid coding node paths. This is not a C# exclusive feature, but can be done in Hello Network as well with e.g.

```gdscript
@export var exported_line_edit: LineEdit
```
