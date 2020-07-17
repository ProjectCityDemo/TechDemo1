using Godot;
using System;

public class NetworkEdge : Node2D
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.

    public void SetSpriteTexture(Texture texture)
    {
        var sprite = GetNode<Sprite>("EdgeSprite");
        sprite.Texture = texture;
    }

    public void SetSpriteTransform(float width, float height, float theta)
    {
        var sprite = GetNode<Sprite>("EdgeSprite");
        var transform = new Transform2D(0, new Vector2(0, 0));
        transform.Scale = new Vector2(width / 100, height / 100);
        transform.Rotation = theta;        
        sprite.Transform = transform;
    }

    public override void _Ready()
    {
        
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
