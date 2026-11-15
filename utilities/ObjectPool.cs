using Godot;
using System.Collections.Generic;

public partial class ObjectPool : Node
{
	[Export] private PackedScene _scene;
	[Export] private int _preloadCount = 5;

	private readonly Stack<Node3D> _free = new();
	
	public override void _Ready()
	{
		for (int i = 0; i < _preloadCount; i++)
		{
			Release(Create());
		}
	}

	public Node3D Get()
	{
		var node = _free.Count > 0 ? _free.Pop() : Create();
		node.ProcessMode = Node.ProcessModeEnum.Inherit;
		node.Visible = true;
		return node;
	}

	public void Release(Node3D node)
	{
		if (node.ProcessMode == ProcessModeEnum.Disabled) return;
		node.ProcessMode = ProcessModeEnum.Disabled;
		node.Visible = false;
		_free.Push(node);
	}

	private Node3D Create()
	{
		var node = _scene.Instantiate<Node3D>();
		AddChild(node);
		return node;
	}
}
