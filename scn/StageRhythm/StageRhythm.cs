using Godot;
using System;

public class StageRhythm : Node2D
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
	}
	
	public void FitDimension(int dev_w, int dev_h)
	{
		var BGRect = GetNode<ColorRect>("BGRect");
		var HitHintLine = GetNode<Line2D>("HitHintLine");
		
		BGRect.SetSize(new Vector2(dev_w / 2, dev_h));
		
		int HitLineOffset = dev_h / 6;
		
		HitHintLine.SetPointPosition(0, new Vector2(0, dev_h - HitLineOffset));
		HitHintLine.SetPointPosition(1, new Vector2(dev_w / 2, dev_h - HitLineOffset));
		
	}
	
	
//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
