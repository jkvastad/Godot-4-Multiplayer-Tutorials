using Godot;
using System;

public partial class Button2D : Area2D
{
#nullable disable   
    [Export]
    CollisionShape2D Shape2D;
    [Export]
    Button2DWrapper Wrapper;
#nullable enable
    public event Action? Pressed;
    public override void _Ready()
    {
        updateButtonHitBox();
        Wrapper.Resized += updateButtonHitBox;
    }

    private void updateButtonHitBox()
    {
        Shape2D.Shape.Set("size", Wrapper.Size);
        Position = new(Wrapper.Size.X / 2, Wrapper.Size.Y / 2); //Node2D is centered on origin
    }

    public void Press() { Pressed?.Invoke(); }
}
