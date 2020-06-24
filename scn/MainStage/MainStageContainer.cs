using Godot;
using System;

public class MainStageContainer : HBoxContainer
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";
	
	private int w = 0;
	private int h = 0;
	private float cost = 0;
	
	private void PrepareStageRhythm()
	{
		var packedStageRhythm = GD.Load<PackedScene>("res://scn/StageRhythm.tscn");
		var StageRhythmScn = packedStageRhythm.Instance();
		AddChild(StageRhythmScn);
		var StageRhythm = GetNode<StageRhythm>("StageRhythm");
		
		
		if (w != 1920 || h != 1080) {
			StageRhythm.FitDimension(w, h);
		}
	}
	
	private void PrepareStageNetwork()
	{
		var packedStageNetwork = GD.Load<PackedScene>("res://scn/StageNetwork.tscn");
		var StageNetworkScn = packedStageNetwork.Instance();
		AddChild(StageNetworkScn);
		var StageNetwork = GetNode<StageNetwork>("StageNetwork");
		
		
		if (w != 1920 || h != 1080) {
			StageNetwork.FitDimension(w, h);
		}
		
		StageNetwork.Position = new Vector2(w / 2, 0);
	}
	
	private void PrepareHUD()
	{
		var HUD = GetNode<Node2D>("HUD");
		var CostLabel = GetNode<Label>("HUD/CostLabel");
		var FlowLabel = GetNode<Label>("HUD/FlowLabel");
		if (w != 1920 || h != 1080) {
			CostLabel.SetGlobalPosition(new Vector2(w/2, 0));
			FlowLabel.SetGlobalPosition(new Vector2(w/2, 50));
		}
		HUD.ZIndex = 1;
	}
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var initialSize = GetNode<Viewport>("/root").Size;
		w = (int) initialSize.x;
		h = (int) initialSize.y;
		
		PrepareStageRhythm();
		PrepareStageNetwork();
		PrepareHUD();
	}

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta)
	{
		cost += delta * 5;
		var CostLabel = GetNode<Label>("HUD/CostLabel");
		var displayCost = (int) cost;
		CostLabel.Text = "Cost: " + displayCost.ToString();
	}
}
