using Godot;
using static GodotUtils.GlobalConstants;

public partial class GameSession : Node3D
{
    private Node3D _sphereScene;
    private Node3D _cubeScene;    

    public override void _Ready()
    {
        _sphereScene = GD.Load<PackedScene>(SPHERE_SCENE_PATH).Instantiate<Node3D>();
        _cubeScene = GD.Load<PackedScene>(CUBE_SCENE_PATH).Instantiate<Node3D>();

        AddChild(_cubeScene);
        AddChild(_sphereScene);
        _sphereScene.Visible = false;
    }

    public void SwapScene()
    {
        _cubeScene.Visible = !_cubeScene.Visible;
        _sphereScene.Visible = !_sphereScene.Visible;
    }
}
