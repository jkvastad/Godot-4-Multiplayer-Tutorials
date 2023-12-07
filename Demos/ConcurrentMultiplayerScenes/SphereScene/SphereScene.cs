using Godot;

public partial class SphereScene : Node3D
{
    [Export]
    private Control _controlNodes;

    public override void _Ready()
    {
        VisibilityChanged += SyncControlVisibility;
    }

    public void SyncControlVisibility()
    {
        _controlNodes.Visible = Visible;
    }
}
