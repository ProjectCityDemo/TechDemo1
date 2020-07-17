using Godot;
using System;

public class NetworkNode : Node2D
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    public int NodeID;

    [Signal]
    public delegate void Activate(int NodeID);

    public void ButtonPressed()
    {
        EmitSignal(nameof(Activate), NodeID);
    }


    public void SetTexture(Texture texture)
    {
        var sprite = GetNode<Sprite>("NodeSprite");
        sprite.Texture = texture;
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        var network = GetNode<Network>("..");
        Connect(nameof(Activate), network, "ActivateNode");
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
