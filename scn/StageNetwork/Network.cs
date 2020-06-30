// Internal Implementation of the Network Node
// Remark that the Network is always rendered in resolution 960*1080
// It would be scaled preserving the aspect ratio for different devices


using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

public enum NodeType
{
	NODETYPE_NORMAL = 0,
	NODETYPE_SOURCE = 1,
	NODETYPE_SINK = 2
}

public enum NodeState
{
	NODE_INACTIVE = 0,
	NODE_ACTIVE = 1
}

public enum EdgeState
{
		EDGE_INACTIVE = 0,
		EDGE_SEMIACTIVE = 1, // for rendering purpose only
		EDGE_ACTIVE = 2
}

public class NodeInternal
{
	public float Cost;

	public NodeState State;

	public NodeType Type;

	public Tuple<int, int> Coord;

	public bool IsActive() { return State == NodeState.NODE_ACTIVE; }
	public NodeInternal(int Cost, NodeState State, NodeType Type, int x, int y)
	{ this.Cost = Cost; this.State = State; this.Type = Type; this.Coord = new Tuple<int, int>(x,y); }
}

public class EdgeInternal
{
	public float Capacity;

	public Tuple<NodeInternal, NodeInternal> Refs;

	private EdgeState _State()
	{
		if (Refs.Item1.IsActive() && Refs.Item2.IsActive())
		{
			return EdgeState.EDGE_ACTIVE;
		}
		else if (!Refs.Item1.IsActive() && !Refs.Item2.IsActive())
		{
			return EdgeState.EDGE_INACTIVE;
		}
		else
		{
			return EdgeState.EDGE_SEMIACTIVE;
		}
	}

	public EdgeState State { get=>_State(); }
	
	public bool IsActive() { return State == EdgeState.EDGE_ACTIVE; }
}

public class NetworkInternal
{
	public List<NodeInternal> Nodes;
	public List<EdgeInternal> Edges;

	public float CalculateFlow()
	{
		var activeNodes = Nodes.Where(node => node.IsActive()).ToList();
		var activeEdges = Edges.Where(edge => edge.IsActive()).ToList();
		//TODO
		return 0;
	}

	public NetworkInternal(string JsonPath="res://stgdata/demo-r.json")
	{
		this.Nodes = new List<NodeInternal>();
		var jsonString = System.IO.File.ReadAllText(JsonPath);
		using (JsonDocument document = JsonDocument.Parse(jsonString))
		{
			JsonElement root = document.RootElement;
			JsonElement sources = root.GetProperty("sources");
			JsonElement sinks = root.GetProperty("sinks");
			JsonElement nodes = root.GetProperty("nodes");
			JsonElement edges = root.GetProperty("edges");

			foreach (JsonElement source in sources.EnumerateArray())
			{
				var x = source.GetProperty("x").GetInt32();
				var y = source.GetProperty("y").GetInt32();
				var cost = source.GetProperty("cost").GetInt32();
				var active = source.GetProperty("active").GetBoolean();
				var state = active ? NodeState.NODE_ACTIVE : NodeState.NODE_INACTIVE;
				this.Nodes.Add(new NodeInternal(cost, state, NodeType.NODETYPE_SOURCE, x, y));
			}

			foreach (JsonElement sink in sinks.EnumerateArray())
			{
				var x = sink.GetProperty("x").GetInt32();
				var y = sink.GetProperty("y").GetInt32();
				var cost = sink.GetProperty("cost").GetInt32();
				var active = sink.GetProperty("active").GetBoolean();
				var state = active ? NodeState.NODE_ACTIVE : NodeState.NODE_INACTIVE;
				this.Nodes.Add(new NodeInternal(cost, state, NodeType.NODETYPE_SINK, x, y));
			}

			foreach (JsonElement node in nodes.EnumerateArray())
			{
				var x = node.GetProperty("x").GetInt32();
				var y = node.GetProperty("y").GetInt32();
				var cost = node.GetProperty("cost").GetInt32();
				var active = node.GetProperty("active").GetBoolean();
				var state = active ? NodeState.NODE_ACTIVE : NodeState.NODE_INACTIVE;
				this.Nodes.Add(new NodeInternal(cost, state, NodeType.NODETYPE_NORMAL, x, y));
			}

			foreach (JsonElement edge in edges.EnumerateArray())
			{
				//困了，明天写，咕咕咕
			}

		}
		//TODO: Construct Network from JSON
	}
}


public class Network : Node2D
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";

	private NetworkInternal InternalNetwork;


	[Signal]
	public delegate void CostConsume(float CostValue, int NodeID); // emit NodeID to understand the NodeInternal to activate.

	public void ActivateNode(int NodeID)
	{

	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// TODO: load stage data from JSON
	}

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
