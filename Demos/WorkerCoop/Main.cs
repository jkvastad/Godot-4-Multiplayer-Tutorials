using System.Linq;
using Client;
using Godot;
using Server;

/// <summary>
/// The games top node (apart from SceneTrees Root node). 
/// Used primarily to branch into server or client. 
/// Singleton is used to e.g. restart client after disconnect
/// Example debug launch arguments: (600 000 gives 10 minutes of debug pause for break line inspection)
/// start server seats=2 timeout=600000
/// client name=Godette timeout=600000
/// Easy way to run a demo is to use the Godot editors Debug Run Instances and set up desired clients and a server there
/// </summary>
public partial class Main : Node
{
    public static Main Singleton { get; private set; } = null!;
    public override void _Ready()
    {
        Singleton = this;

        string[] args = OS.GetCmdlineArgs();
        if (args.Contains("server"))
            StartServer();
        else if (args.Contains("client"))
        {
            new InputBindings(); // Run the static constructor, initializing input bindings.
            StartClient();
        }
    }
    private void StartServer()
    {
        ServerLobby serverLobby = GD.Load<PackedScene>("res://Server/server_lobby.tscn").Instantiate<ServerLobby>();
        string[] args = OS.GetCmdlineArgs();

        var nameArg = args.Where(arg => arg.Contains("seats="));
        if (nameArg.Count() == 1)
        {
            string seats = nameArg.Select(arg => arg.Split("=").Last()).First();
            serverLobby.PlayerSeats.Text = seats;
        }

        var startArg = args.Single(arg => arg == "start");
        if (startArg == "start")
            serverLobby.StartServer = true;

        var timeoutArg = args.Where(arg => arg.Contains("timeout="));
        if (timeoutArg.Count() == 1)
        {
            int msTimeout = timeoutArg.Select(arg => arg.Split("=").Last()).First().ToInt();
            serverLobby.TimeoutMS = msTimeout;
        }

        CallDeferred(MethodName.AddChild, serverLobby); // CallDeferred to avoid problems of adding children to a node mid ready method
    }

    public void StartClient()
    {
        ClientLobby clientLobby = GD.Load<PackedScene>("res://Client/client_lobby.tscn").Instantiate<ClientLobby>();
        string[] args = OS.GetCmdlineArgs();

        var nameArg = args.Where(arg => arg.Contains("name="));
        if (nameArg.Count() == 1)
        {
            string playerName = nameArg.Select(arg => arg.Split("=").Last()).First();
            clientLobby.PlayerName.Text = playerName;
        }

        var timeoutArg = args.Where(arg => arg.Contains("timeout="));
        if (timeoutArg.Count() == 1)
        {
            int msTimeout = timeoutArg.Select(arg => arg.Split("=").Last()).First().ToInt();
            clientLobby.TimeoutMS = msTimeout;
        }

        CallDeferred(MethodName.AddChild, clientLobby); // CallDeferred to avoid problems of adding children to a node mid ready method
    }
}