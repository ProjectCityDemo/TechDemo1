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

	public float Flow;

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

	public EdgeInternal(float capacity, NodeInternal ref1, NodeInternal ref2) { this.Capacity = capacity; this.Refs = new Tuple<NodeInternal, NodeInternal>(ref1, ref2); }
}

public class NetworkInternal
{
	public List<NodeInternal> Nodes;
	public List<EdgeInternal> Edges;

	public float CalculateFlow()
	{
		var activeNodes = Nodes.Where(node => node.IsActive()).ToList();
		var activeEdges = Edges.Where(edge => edge.IsActive()).ToList();

		var sourceNodes = Nodes.Where(node => node.Type == NodeType.NODETYPE_SOURCE).ToList();
		var sinkNodes = Nodes.Where(node => node.Type == NodeType.NODETYPE_SINK).ToList();

		if (!sourceNodes[0].IsActive() || !sinkNodes[0].IsActive()) { return 0; }

        //for each optimization, the optimized flow that passes node "x" will only come from one edge
        List<int>[] graph;
		// graph: node "x" has a list "graph[x]", and "graph[x]" stores the index(es) of edge(s) in "edgesForCalculation" that starts with node "x"
        graph = new List<int>[activeNodes.Count()]; 
		// augment: node "x" has a float "augment[x]", and "augment[x]" stores the flow that passes node "x" during each optimization
        var augment = new float[activeNodes.Count()]; 

		// previous: node "x" has a int "previous[x]", and "previous[x]" stores the index of edge in "edgesForCalculation"
		// from which the flow "augment[x]" comes
        // in other words, node "x" will always be the outlet in edgesForCalculation[previous[x]] (it is an edge)
        var previous = new int[activeNodes.Count()]; 

        List<EdgeInternal> edgesForCalculation = new List<EdgeInternal>();
		
		for (int i = 0; i < activeEdges.Count(); i++)
		{
            //for each active edge (in out capacity flow), there will also be an edge (out in capacity flow) in "edgesForCalculation"
            edgesForCalculation.Add(activeEdges[i]);
			EdgeInternal tempEdge = new EdgeInternal(activeEdges[i].Capacity, activeEdges[i].Refs.Item2, activeEdges[i].Refs.Item1);
			edgesForCalculation.Add(tempEdge);
            // for origin edge (in out capacity flow), node "in" will be the inlet of "edgesForCalculation[2*i]", and "out" will be the inlet of "edgesForCalculation[2*i+1]"
            // this information will be stored in "graph"
            var index_in = activeNodes.IndexOf(activeEdges[i].Refs.Item1);
			var index_out = activeNodes.IndexOf(activeEdges[i].Refs.Item2);
			graph[index_in].Add(2*i);
			graph[index_out].Add(2*i+1);
		}
		
		float totalFlow = 0;
		while(true)
		{
			Array.Clear(augment, 0, augment.Length);
            List<int> travel = new List<int>(); // For each optimization, "travel" will store, up till now, the index(es) of the inlet node(s) in "activeNodes" of this optimization
			var sourceIndex = activeNodes.IndexOf(sourceNodes[0]);
			var sinkIndex = activeNodes.IndexOf(sinkNodes[0]);
			
			travel.Add(sourceIndex);// "source" is the first inlet node of each optimization
			augment[sourceIndex]=float.MaxValue;// for each optimization, the flow that passes "source" is always infinite
			
			while(travel.Any())
			{
				int tempIndex = travel[travel.Count()-1]; // inlet point that will be considered this time
				travel.RemoveAt(travel.Count()-1);
				for (int i = 0; i < graph[tempIndex].Count(); ++i) // consider all the edges in which point "tempIndex" is inlet
                {
					EdgeInternal tempEdge = edgesForCalculation[graph[tempIndex][i]]; // edge that will be considered this time
                    var tempOutIndex = activeNodes.IndexOf(tempEdge.Refs.Item2);
				
					if (augment[tempOutIndex] == 0 && tempEdge.Capacity > tempEdge.Flow) // "!augment[tempOutIndex]": the optimized augment of each node will only comes from one edge for each optimization
                    { 
						previous[tempOutIndex] = graph[tempIndex][i]; // "Graph[tempIndex][i]" is the index of edge from which the augment of point "tempOutIndex" comes from
                        augment[tempOutIndex] = (augment[tempIndex] < (tempEdge.Capacity - tempEdge.Flow) ? augment[tempIndex] : (tempEdge.Capacity - tempEdge.Flow));
                        // "augment[tempOutIndex]" = min{augment of the inlet node, (capacity-flow) of this edge}
                        travel.Add(tempOutIndex); // next time the node "tempOutIndex" will be the inlet that needs to be considered
                    }
				}
				if (augment[sinkIndex] == 0) break; // break this optimization if there is optimized augment in "sink"
			}
			if (augment[sinkIndex] == 0) break; // break the max flow calculation if after a thorough optimization, there is no extra augment that passes "sink"
			
			for (int i = sinkIndex; i != sourceIndex; i = activeNodes.IndexOf(edgesForCalculation[previous[i]].Refs.Item1)) // i will be each point that this time the augment passes through
			{
				edgesForCalculation[previous[i]].Flow += augment[sinkIndex]; // "edgesForCalculation[Previous[i]]" is the edge from where the augment of node "i" comes from
                edgesForCalculation[previous[i] ^ 1].Flow -= augment[sinkIndex]; // "edgesForCalculation[Previous[i] ^ 1]" is the opposite edge of the previous one
            }
			totalFlow += augment[sinkIndex];
		}
		
		return totalFlow;

	}

	public NetworkInternal(string JsonPath="stgdata/demo-r.json")
	{
		this.Nodes = new List<NodeInternal>();
		this.Edges = new List<EdgeInternal>();
		var jsonString = System.IO.File.ReadAllText(JsonPath);
		using (JsonDocument document = JsonDocument.Parse(jsonString))
		{
			JsonElement root = document.RootElement;
			JsonElement sources = root.GetProperty("sources");
			JsonElement sinks = root.GetProperty("sinks");
			JsonElement nodes = root.GetProperty("nodes");
			JsonElement edges = root.GetProperty("edges");

			var numSources = sources.GetArrayLength();
			var numSinks = sinks.GetArrayLength();
			var numNodes = nodes.GetArrayLength();

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
				var capacity = edge.GetProperty("capacity").GetInt32();
				var link = edge.GetProperty("link").EnumerateArray().ToArray();
				var type1 = link[0].GetProperty("type").GetString();
				var idx1 = link[0].GetProperty("index").GetInt32();
				var offset1 = 0;

				switch(type1)
				{
					case "source":
						offset1 = 0;
						break;
					case "sink":
						offset1 = numSources;
						break;
					case "node":
						offset1 = numSources + numSinks;
						break;
				}

				NodeInternal ref1 = this.Nodes[offset1 + idx1];

				var type2 = link[1].GetProperty("type").GetString();
				var idx2 = link[1].GetProperty("index").GetInt32();
				var offset2 = 0;

				switch(type2)
				{
					case "source":
						offset2 = 0;
						break;
					case "sink":
						offset2 = numSources;
						break;
					case "node":
						offset2 = numSources + numSinks;
						break;
				}

				NodeInternal ref2 = this.Nodes[offset2 + idx2];
				var newEdge = new EdgeInternal(capacity, ref1, ref2);
				this.Edges.Add(newEdge);
			}

		}
	}
}


public class Network : Node2D
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";

	private NetworkInternal InternalNetwork = new NetworkInternal();
	private PackedScene PackedNodeScene = GD.Load<PackedScene>("res://scn/NetworkNode.tscn");
	private PackedScene PackedEdgeScene = GD.Load<PackedScene>("res://scn/NetworkEdge.tscn");

	private Texture SourceNodeTexture = GD.Load<Texture>("res://texture/texture_node_source.png");
	private Texture SinkNodeTexture = GD.Load<Texture>("res://texture/texture_node_sink.png");
	private Texture ActiveNodeTexture = GD.Load<Texture>("res://texture/texture_node_active.png");
	private Texture InactiveNodeTexture = GD.Load<Texture>("res://texture/texture_node_inactive.png");
	private Texture ActiveEdgeTexture = GD.Load<Texture>("res://texture/texture_edge_active.png");
	private Texture InactiveEdgeTexture = GD.Load<Texture>("res://texture/texture_edge_inactive.png");
	private Texture SemiactiveEdgeTexture = GD.Load<Texture>("res://texture/texture_edge_semiactive.png");



	[Signal]
	public delegate void CostConsume(float CostValue, int NodeID); // emit NodeID to understand the NodeInternal to activate.

	public void ActivateNode(NodeInternal node)
	{
		if (node.IsActive()) return;
		var idx = InternalNetwork.Nodes.IndexOf(node);
		var cost = node.Cost;
		EmitSignal(nameof(CostConsume), cost, idx);
	}

	public void ActivateNodeImpl(int NodeID)
	{
		InternalNetwork.Nodes[NodeID].State = NodeState.NODE_ACTIVE;
	}


	private void RefreshTexture()
	{
		foreach (var edge in InternalNetwork.Edges)
		{
			var sceneEdge = GetNode<NetworkEdge>("SceneEdge_" + InternalNetwork.Edges.IndexOf(edge).ToString()); 

			if (edge.State == EdgeState.EDGE_ACTIVE)
			{
				sceneEdge.SetSpriteTexture(ActiveEdgeTexture);
			}
			else if (edge.State == EdgeState.EDGE_INACTIVE)
			{
				sceneEdge.SetSpriteTexture(InactiveEdgeTexture);
			}
			else
			{
				sceneEdge.SetSpriteTexture(SemiactiveEdgeTexture);
			}

		}

		foreach (var node in InternalNetwork.Nodes)
		{
			var sceneNode = GetNode<NetworkNode>("SceneNode_" + InternalNetwork.Nodes.IndexOf(node).ToString()); 

			if (node.Type == NodeType.NODETYPE_SOURCE)
			{
				sceneNode.SetTexture(SourceNodeTexture);
			}
			else if (node.Type == NodeType.NODETYPE_SINK)
			{
				sceneNode.SetTexture(SinkNodeTexture);
			}
			else
			{
				if (node.IsActive())
				{
					sceneNode.SetTexture(ActiveNodeTexture);
				}
				else
				{
					sceneNode.SetTexture(InactiveNodeTexture);
				}
			}

		}
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var mainStageContainer = GetNode<MainStageContainer>("../../../MainStageContainer");
		Connect(nameof(CostConsume), mainStageContainer, "ProcessCostConsume");

		var edgeWidth = 8;

		foreach (var edge in InternalNetwork.Edges)
		{
			var dx = edge.Refs.Item2.Coord.Item1 - edge.Refs.Item1.Coord.Item1;
			var dy = edge.Refs.Item2.Coord.Item2 - edge.Refs.Item1.Coord.Item2;
			var distance = (float)Math.Sqrt(dx * dx + dy * dy);
			var theta = (float)Math.Atan2(dy, dx);

			var anchorPoint = new Vector2((edge.Refs.Item2.Coord.Item1 + edge.Refs.Item1.Coord.Item1) / 2,
			 (edge.Refs.Item2.Coord.Item2 + edge.Refs.Item1.Coord.Item2) / 2);

			var sceneEdge = (NetworkEdge)PackedEdgeScene.Instance();
			sceneEdge.Name = "SceneEdge_" + InternalNetwork.Edges.IndexOf(edge).ToString();
			AddChild(sceneEdge);
			sceneEdge.SetSpriteTransform(edgeWidth, distance, -theta);
			sceneEdge.Position = anchorPoint;
		}

		foreach (var node in InternalNetwork.Nodes)
		{
			var sceneNode = (NetworkNode)PackedNodeScene.Instance();
			sceneNode.Name = "SceneNode_" + InternalNetwork.Nodes.IndexOf(node).ToString();
			AddChild(sceneNode);
			sceneNode.Position = new Vector2(node.Coord.Item1, node.Coord.Item2);
			sceneNode.Scale = new Vector2(0.4f, 0.4f);
		}

		RefreshTexture();

	}

	public override void _Draw()
	{
	}

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
