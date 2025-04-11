using Godot;
using System.Collections.Generic;

// Editor style debug camera - hold right mouse to look around
public partial class DebugCamera : Camera3D
{
    const float piBy2 = Mathf.Pi / 2;
    bool _isLooking = false;
    float _mouseSensitivity = 0.005f;
    float _cameraSpeed = 10;
    private float _rotationX = 0f;
    private float _rotationY = 0f;      

    Dictionary<Key, float> _isKeyPressed = new()
    {
        { Key.Q, 0},
        { Key.W, 0},
        { Key.E, 0},
        { Key.A, 0},
        { Key.S, 0},
        { Key.D, 0}
    };

    public override void _Process(double delta)
    {
        if (_isLooking)
        {
            Vector3 direction = new Vector3(
                        _isKeyPressed[Key.D] - _isKeyPressed[Key.A],
                        _isKeyPressed[Key.E] - _isKeyPressed[Key.Q],
                        _isKeyPressed[Key.S] - _isKeyPressed[Key.W]
                        );
            direction = direction.Normalized();
            Translate(direction * (float)delta * _cameraSpeed);
        }
    }

    public override void _Ready()
    {
        KeepAspect = KeepAspectEnum.Width;     
        _rotationX = Rotation.X;
        _rotationY = Rotation.Y;        
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton)
        {
            if (_isLooking && mouseButton.ButtonIndex == MouseButton.WheelUp)
            {
                _cameraSpeed += 1;
            }
            if (_isLooking && mouseButton.ButtonIndex == MouseButton.WheelDown)
            {
                if (_cameraSpeed > 1) _cameraSpeed -= 1;
            }
            if (mouseButton.Pressed && mouseButton.ButtonIndex == MouseButton.Right)
            {
                Input.MouseMode = Input.MouseModeEnum.Captured;
                _isLooking = true;
            }
            if (!mouseButton.Pressed && mouseButton.ButtonIndex == MouseButton.Right)
            {
                Input.MouseMode = Input.MouseModeEnum.Visible;
                _isLooking = false;
            }
        }

        if (@event is InputEventKey keyEvent)
        {
            if (keyEvent.Pressed && _isKeyPressed.ContainsKey(keyEvent.Keycode))
            {
                _isKeyPressed[keyEvent.Keycode] = 1;
            }
            if (!keyEvent.Pressed && _isKeyPressed.ContainsKey(keyEvent.Keycode))
            {
                _isKeyPressed[keyEvent.Keycode] = 0;
            }
        }

        if (@event is InputEventMouseMotion mouseMotion && _isLooking)
        {
            // modify accumulated mouse rotation            
            _rotationX += mouseMotion.Relative.X * _mouseSensitivity;
            _rotationY = Mathf.Clamp(_rotationY + mouseMotion.Relative.Y * _mouseSensitivity, -piBy2, piBy2);

            // reset rotation
            Transform3D transform = Transform;
            transform.Basis = Basis.Identity;
            Transform = transform;

            RotateObjectLocal(Vector3.Up, -_rotationX); // first rotate about Y
            RotateObjectLocal(Vector3.Right, -_rotationY); // then rotate about X
        }

    }
}