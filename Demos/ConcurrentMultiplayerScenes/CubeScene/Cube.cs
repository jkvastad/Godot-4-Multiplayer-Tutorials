using Godot;

public partial class Cube : MeshInstance3D
{
    [Export]
    public Color cubeColor
    {
        get => _cubeColor;
        set
        {
            _cubeColor = value;
            _material.AlbedoColor = value;
        }
    }
    private Color _cubeColor = new Color(1, 0, 0);
    [Export]
    private MultiplayerSynchronizer _synchronizer;

    private SceneReplicationConfig _sceneReplicationConfig = new SceneReplicationConfig();
    private StandardMaterial3D _material;

    public override void _Ready()
    {
        _material = GetActiveMaterial(0) as StandardMaterial3D;
        _material.AlbedoColor = cubeColor;

        _sceneReplicationConfig.AddProperty($"{GetPath()}:{nameof(cubeColor)}");
        _synchronizer.ReplicationConfig = _sceneReplicationConfig;        
    }

    public void RequestCubeColor(Color color)
    {
        RpcId(1, MethodName.ServerCubeColor, color);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void ServerCubeColor(Color color)
    {
        if (!Multiplayer.IsServer()) return;
        cubeColor = color;
    }

    private void SetCubeColor(Color color)
    {
        cubeColor = color;
    }
}