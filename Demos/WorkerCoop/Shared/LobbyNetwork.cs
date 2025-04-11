
using Client;
using Godot;
using Server;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class LobbyNetwork : Node
{
#nullable disable
    [Export]
    ServerLobby ServerLobby { get; set; }
    [Export]
    ClientLobby ClientLobby { get; set; }
#nullable enable

    #region Server-side RPCs    

    #endregion

    #region Client-side RPCs            
    #endregion
}
