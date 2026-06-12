using System;
using Godot;

public static class GodotUiExtensions
{
    public static void SetVisibility(this CanvasItem node, bool visible) =>
        node.Visible = visible;

    public static void Connect(this TextureButton button, Action handler) =>
        button.Pressed += handler;
}
