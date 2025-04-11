# worker-coop
Worker placement coop demo - players can place markers on 3D grids. Made in Godot using C#. Som key features:

- Late joining
- Disconnect/reconnect
- Swapping seats with other players
- Dynamically replicated values e.g. color choices
  - Updates with client disconnects, choice of player seat.
- Overlay menu
- Editor style debug camera
- Model View Control style RPC
- Basic Shaders

The purpose of this demo is to show a barebones multiplayer game using RPCs with MVC design in Godot using C#, something which i felt was missing when looking around the documentation. The closest thing i found is the [Multiplayer bomber demo] (https://github.com/godotengine/godot-demo-projects/tree/master/networking/multiplayer_bomber), but what i wanted was a MVC RPC C# demo. MVC and RPC for the flexibility, C# for the performance speed. Have a look around in the code, starting from Main, and hopefully some bits and pieces of the demo can be of use!

Coming straight from singleplayer games or "regular" programming, a multiplayer game is coded quite differently from a singleplayer game. The big difference is that in a singleplayer game the code is more direct - player input is directly handled and the game updates immediately. A multiplayer game is more reactive - player input is mostly about making requests to a server and passively listening for changes pushed by the server.

**On the design side**
The demo is modeled on an analogue board game:
- A board game is played over a game session
- A game session has a few seats by the table where players can sit down and play
- Game state consists of pieces on the table
- Session state consists of things around the table, such as:
  - who are the players?
  - who is sitting in which seat?
  - what are the players' names?
  - who ate all the chips?
  - etc.

**On the technical side**
The demo uses a Model View Control Network design, with:
- A Model class purely containing data.
- Viewers creating views representing the data - this is what the players sees on screen.
- Controllers defining what player input means at a given point in the game.
  - A player can click on a view, possibly raytracing to it for object picking and interacting with the game.
  - If a pause menu is open we don't want to accidentaly click through it to the game in the background.
- A dedicated Network node which handles all requests and updates between server and client.

The typical player loop then becomes:
- Player clicks on View on screen.
- Controller decides what this means - possibly object picking with resulting server request.
- Network node sends RPC request to server.
- Server updates model per request, pushes update to clients via Network.
- Clients receive update via Network, updating their Models.
- Client Models push changes to Views.

## Nuts, bolts and pitfalls
Making all of this work together i found some pieces of special note

### Serialisation
The work horse of multiplayer is [serialization](https://en.wikipedia.org/wiki/Serialization). In practice, this means taking an object on the server or client, turning it into a string (serialisation), passing it over the internet and then turning it back into an object from the string (deserialisation).

**Warning**
NEVER trust incoming strings. Always assume that the incoming string is from a spoofed client who is trying to delete your harddrive - or worse. This means that when deserializing we can never trust the sender in what they want us to deserialize into. Compare e.g.

```csharp
Type converterType = typeof(MyType);
return Activator.CreateInstance(converterType);
```
versus
```csharp
return Activator.CreateInstance(arbitraryTypeDecidedBySender);
```

In the first example an attacker can do no worse than the worst case scenario of MyType (assuming we do not include types in our game with methods to delete or scam the user). The attacker can do no worse than perhaps troll the players with strange numbers - no big deal comparatively. 

In the second example they can create any type available to the game, including all of .NET. This opens up possiblities for vulnerabilities in classes the developer have never even heard of. Stick with the first example and make sure there are no sufficently bad worst cases of string spoofing and the game is secure enough.
**End warning**

With that said, for serialisation/deserialisation i used the inbuilt [JSON](https://en.wikipedia.org/wiki/JSON) serializer "JsonSerializer" in C# found in [System.Text.Json](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/how-to). The reasons being:
- Json is quite readable to a human and I did not expect to run into network performance issues in such a simple demo as this.
- It is built in
- It can serialize some [basic (assumably safe) C# types](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/supported-types)
- It can [expand to custom types](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/converters-how-to). 
- There are also some neat features for [safely deserializing interface classes and child classes](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/polymorphism) (e.g. whitelisting which types belong to an interface/parent).

#### Making custom converters
Have a look at in the Subscribables folder, e.g. SubscribableBiDict. In that file there is a SubscribableBiDictConverterFactory which creates JsonConverters for the generic bidirectional dictionary. The factories must be added to the serialiser options, see the GameSessionNetwork main file.

#### Cheating
I made this demo as a stepping stone to a coop game, a setting where cheating is not really relevant - if one of your friends starts trying to trick the server they are ultra rich in game then it's more of a cyberprank than someone aimbotting in a competetive versus game. There are some checks against "cheating" (e.g. a client can request adding markers they do not have and the server will deny the request) but I have not put any thought into it in this demo.

### A single network node
Since RPCs require an exact identical node path an easy solution is to have a single node high up in the scene tree work as a sort of network dispatcher. A design with multiple network nodes (which may or may not be alive at all clients simultaneously) easily becomes brittle.

### Mixing 2D, 3D and Control (2D with benefits)
In godot there are three kinds of interactible nodes: 2D, 3D and Control. These receive input according to the [InputEvent](https://docs.godotengine.org/en/stable/tutorials/inputs/inputevent.html) order. This is all fine and good... until it isn't. What happens when you have a 3D game with a HUD, pause meny and a hand of cards? Already at the 2D + 3D stage there are problems such as "Click the 2D thing in front of the 3D thing" which are not handled by the standard InputEvent order (2D and 3D happen in parallel). To solve this I use generally use input polling in the physics process override, collecting input between physics ticks and letting the controler active for that tick handle the input. See e.g. WorkerGridViewer's GridViewerController, especially how it interacts with the session menu to go back and forth between control states. Basic InputEvent order is still useful, e.g. the PlayerSeatView class which only needs control nodes uses it unmodified.
