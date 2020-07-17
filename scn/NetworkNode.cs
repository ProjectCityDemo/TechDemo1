using Godot;
using System;

public class NetworkNode : Node2D
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    public void SetTexture(Texture texture)
    {
        var sprite = GetNode<Sprite>("NodeSprite");
        sprite.Texture = texture;
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
