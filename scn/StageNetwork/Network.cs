// Internal Implementation of the Network Node
// Remark that the Network is always rendered in resolution 960*1080
// It would be scaled preserving the aspect ratio for different devices


using Godot;
using System;

public enum NodeState
{
    NODE_INACTIVE = 0,
    NODE_ACTIVE = 1
}

public enum EdgeState
{
        EDGE_INACTIVE = 0,
        EDGE_SEMIACTIVE = 1,
        EDGE_ACTIVE = 2
}

public class NodeInternal
{
    public int x { set; get; }
    public int y { set; get; }
    public NodeState state = NodeState.NODE_INACTIVE;

    public bool IsSource = false;
    public bool IsSink = false;

    public NodeInternal(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public NodeInternal() {}
}

public class EdgeInternal
{

    public float capacity = 10;
    public EdgeState state = EdgeState.EDGE_INACTIVE;

    private NodeInternal a;
    private NodeInternal b;

    public EdgeInternal(NodeInternal a, NodeInternal b)
    {
        this.a = a;
        this.b = b;
    }
}

public class NetworkInternal
{
    public NodeInternal[] nodes;
    public EdgeInternal[] edges;

    public float CalculateFlow()
    {
        //TODO
        return 0;
    }
}


public class Network : Node2D
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    private NetworkInternal InternalNetwork;


    [Signal]
    public delegate void CostConsume(int CostValue, int NodeID); // emit NodeID to understand the NodeInternal to activate.

    public void ActivateNode(int NodeID)
    {

    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        // TODO: load stage data from JSON
        InternalNetwork.nodes = new NodeInternal[2] {new NodeInternal(160, 800), new NodeInternal(800, 160)};
        InternalNetwork.edges = new EdgeInternal[1] {new EdgeInternal(InternalNetwork.nodes[0], InternalNetwork.nodes[1])};
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
