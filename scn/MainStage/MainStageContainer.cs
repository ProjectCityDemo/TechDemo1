using Godot;
using System;

public class MainStageContainer : HBoxContainer
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";
	
	private int Width = 0;
	private int Height = 0;
	private float Cost = 0;
	private int Flow = 0;

	private void PrepareStageRhythm()
	{
		var packedStageRhythm = GD.Load<PackedScene>("res://scn/StageRhythm.tscn");
		var stageRhythmScn = packedStageRhythm.Instance();
		AddChild(stageRhythmScn);
		var stageRhythm = GetNode<StageRhythm>("StageRhythm");
		
		
		if (Width != 1920 || Height != 1080) {
			stageRhythm.FitDimension(Width, Height);
		}
	}
	
	private void PrepareStageNetwork()
	{
		var packedStageNetwork = GD.Load<PackedScene>("res://scn/StageNetwork.tscn");
		var stageNetworkScn = packedStageNetwork.Instance();
		AddChild(stageNetworkScn);
		var stageNetwork = GetNode<StageNetwork>("StageNetwork");
		
		
		if (Width != 1920 || Height != 1080) {
			stageNetwork.FitDimension(Width, Height);
		}
		
		stageNetwork.Position = new Vector2(Width / 2, 0);

		Connect(nameof(ActivateNode), stageNetwork.RenderingNetwork, "ActivateNodeImpl");
	}
	
	private void PrepareHUD()
	{
		var HUD = GetNode<Node2D>("HUD");
		var CostLabel = GetNode<Label>("HUD/CostLabel");
		var FlowLabel = GetNode<Label>("HUD/FlowLabel");
		if (Width != 1920 || Height != 1080) {
			CostLabel.SetGlobalPosition(new Vector2(Width/2, 0));
			FlowLabel.SetGlobalPosition(new Vector2(Width/2, 50));
		}
		HUD.ZIndex = 1;
	}
	
	[Signal]
	public delegate void ActivateNode(int NodeID); // emit signal to StageNetwork

	public void ProcessCostConsume(float CostValue, int NodeID)
	{
		if (Cost > CostValue)
		{
			Cost -= CostValue;
			EmitSignal(nameof(ActivateNode), NodeID);
		}
	}

	public void ProcessFlowUpdate(int FlowValue)
	{
		Flow = FlowValue;
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var initialSize = GetNode<Viewport>("/root").Size;
		Width = (int) initialSize.x;
		Height = (int) initialSize.y;
		
		PrepareStageRhythm();
		PrepareStageNetwork();
		PrepareHUD();
	}

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta)
	{
		Cost += delta * 5;
		var CostLabel = GetNode<Label>("HUD/CostLabel");
		var FlowLabel = GetNode<Label>("HUD/FlowLabel");

		var displayCost = (int) Cost;
		CostLabel.Text = "Cost: " + displayCost.ToString();
		FlowLabel.Text = "Flow: " + Flow.ToString();
	}
}
