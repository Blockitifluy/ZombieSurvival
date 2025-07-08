using System.Diagnostics.CodeAnalysis;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace ZombieSurvival.Engine;

public static class Input
{
    public static event EventHandler<KeyboardKeyEventArgs>? OnKeyDown;
    public static event EventHandler<KeyboardKeyEventArgs>? OnKeyUp;

    [AllowNull]
    public static KeyboardState KeyboardState { get; set; }

    [AllowNull]
    public static MouseState MouseState { get; set; }

    internal static void SendKeyDown(object sender, KeyboardKeyEventArgs e)
    {
        OnKeyDown?.Invoke(sender, e);
    }

    internal static void SendKeyUp(object sender, KeyboardKeyEventArgs e)
    {
        OnKeyUp?.Invoke(sender, e);
    }
}
