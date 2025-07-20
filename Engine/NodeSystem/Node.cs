using ZombieSurvival.Engine.NodeSystem.Scene;

namespace ZombieSurvival.Engine.NodeSystem;

[SaveNode("engine.node")]
public class Node
{
    private Node? _Parent;
    public Node? Parent
    {
        get => _Parent;
        set
        {
            if (value == this)
            {
                throw new TreeException($"Can not parent to self");
            }

            if (value is not null)
            {
                bool isDescendant = IsDescendant(value),
                isAncestor = IsAncestor(value);
                if (isDescendant || isAncestor)
                {
                    throw new TreeException($"Circular Heiarchry attemped on {this}");
                }
            }

            OnParent(value);
            _Parent = value;
        }
    }
    [Export]
    public string Name { get; set; } = "";

    /// <summary>
    /// Will this Node be saved, when using <see cref="SceneHandler.SaveScene(Tree, string)"/>?
    /// </summary>
    public bool Archive = true;

    internal Guid _ID;
    public Guid ID => _ID;

    public static Tree GetTree()
    {
        return Tree.GetTree();
    }

    public TNode? FindFirstChild<TNode>(string name) where TNode : Node
    {
        foreach (Node node in GetTree().GetAllNodes())
        {
            bool nameMatch = node.Name == name,
            parentMatch = node.Parent == this;
            if (nameMatch && parentMatch && node is TNode tNode)
            {
                return tNode;
            }
        }
        return null;
    }

    public TNode? FindFirstChildOfType<TNode>() where TNode : Node
    {
        foreach (Node node in GetTree().GetAllNodes())
        {
            bool parentMatch = node.Parent == this;
            if (parentMatch && node is TNode tNode)
            {
                return tNode;
            }
        }
        return null;
    }

    public bool CanBeArchived()
    {
        var ancestors = GetAncestors();
        foreach (Node node in ancestors)
        {
            if (!node.Archive)
            {
                return false;
            }
        }
        return true;
    }

    public List<Node> GetChildren()
    {
        List<Node> nodes = [];
        foreach (Node node in GetTree().GetAllNodes())
        {
            if (node.Parent == this)
            {
                nodes.Add(node);
            }
        }
        return nodes;
    }

    public bool IsDescendant(Node other)
    {
        Node? current = Parent;
        while (current is not null)
        {
            if (other == current)
            {
                return true;
            }
            current = current.Parent;
        }
        return false;
    }

    public bool IsAncestor(Node other)
    {
        return IsDescendant(other);
    }

    public List<Node> GetDescendant()
    {
        List<Node> nodes = [];
        foreach (Node node in GetTree().GetAllNodes())
        {
            bool isDesendent = node.IsDescendant(this);
            if (isDesendent)
            {
                nodes.Add(node);
            }
        }
        return nodes;
    }

    public List<Node> GetAncestors()
    {
        List<Node> nodes = [];
        Node current = this;
        while (current.Parent is not null)
        {
            Node parent = current.Parent;
            nodes.Add(parent);
            current = parent;
        }
        return nodes;
    }

    public override string ToString()
    {
        return $"{GetType().Name} {Name}";
    }

    /// <summary>
    /// Runs every frame.
    /// </summary>
    /// <param name="delta">The time between the last frame and the second to last frame.</param>
    public virtual void Update(double delta) { }

    /// <summary>
    /// Runs at a fixed rate.
    /// </summary>
    /// <remarks>
    /// To get the time passed between calls use <seealso cref="Tree.FixedUpdateTime"/>.
    /// </remarks>
    public virtual void UpdateFixed() { }

    /// <summary>
    /// Runs before the node is registered.
    /// </summary>
    public virtual void Awake() { }

    /// <summary>
    /// Run after the node is registered.
    /// </summary>
    public virtual void Start() { }

    protected virtual void OnParent(Node? futureParent) { }

    public void Destroy()
    {
        Tree tree = GetTree();

        var desendents = GetDescendant();
        foreach (Node node in desendents)
        {
            node.Parent = null;
            tree.UnregisterNode(node);
        }
        tree.UnregisterNode(this);
    }

    #region Node Creation

    public static TNode NewDisabled<TNode>(Node? parent, string? name = null) where TNode : Node, new()
    {
        return new()
        {
            Parent = parent,
            Name = name ?? typeof(TNode).Name
        };
    }

    public static Node NewDisabled(Node? parent, string? name = null)
    {
        return NewDisabled<Node>(parent, name);
    }

    public static TNode New<TNode>(Node? parent = null, string? name = null) where TNode : Node, new()
    {
        TNode node = NewDisabled<TNode>(parent, name);

        node.Awake();
        node._ID = GetTree().RegisterNode(node);
        node.Start();

        return node;
    }

    public static Node New(Node? parent = null, string? name = null)
    {
        return New<Node>(parent, name);
    }

    #endregion

    ~Node()
    {
        GetTree().UnregisterNode(this);
    }
}