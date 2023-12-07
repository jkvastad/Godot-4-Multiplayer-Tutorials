using Godot;

public partial class Sphere : MeshInstance3D
{

    [Export]
    public Color sphereColor
    {
        get => _sphereColor;
        set
        {
            _sphereColor = value;
            _material.AlbedoColor = value;
        }
    }
    private Color _sphereColor = new Color(1, 0, 0);
    [Export]
    private MultiplayerSynchronizer _synchronizer;

    private SceneReplicationConfig _sceneReplicationConfig = new SceneReplicationConfig();
    private StandardMaterial3D _material;

    public override void _Ready()
    {
        _material = GetActiveMaterial(0) as StandardMaterial3D;
        _material.AlbedoColor = sphereColor;

        _sceneReplicationConfig.AddProperty($"{GetPath()}:{nameof(sphereColor)}");
        _synchronizer.ReplicationConfig = _sceneReplicationConfig;
    }

    public void RequestSphereColor(Color color)
    {
        RpcId(1, MethodName.ServerSphereColor, color);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void ServerSphereColor(Color color)
    {
        if (!Multiplayer.IsServer()) return;
        sphereColor = color;
    }

    private void SetSphereColor(Color color)
    {
        sphereColor = color;
    }
}
