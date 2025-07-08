namespace ZombieSurvival.Engine.NodeSystem;

public class Node : IDisposable
{
    private Node? _Parent;
    public Node? Parent
    {
        get { return _Parent; }
        set
        {
            if (value == this)
            {
                throw new TreeException($"Circular Heiarchry attemped on {this}");
            }
            _Parent = value;
        }
    }
    public string Name = "";

    private Guid _ID;
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

    public IEnumerable<Node> GetChildren()
    {
        foreach (Node node in GetTree().GetAllNodes())
        {
            if (node.Parent == this)
            {
                yield return node;
            }
        }
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
    /// Runs before the node is registered.
    /// </summary>
    public virtual void Awake() { }

    /// <summary>
    /// Run after the node is registered.
    /// </summary>
    public virtual void Start()
    {
        Console.WriteLine($"Started {this}");
    }

    public void Dispose()
    {
        GetTree().UnregisterNode(this);
    }

    public static TNode New<TNode>(Node? parent = null, string? name = null) where TNode : Node, new()
    {
        TNode node = new()
        {
            Parent = parent,
            Name = name ?? typeof(TNode).Name
        };

        node.Awake();
        node._ID = GetTree().RegisterNode(node);
        node.Start();

        return node;
    }

    public static Node New(Node? parent = null, string? name = null)
    {
        return New<Node>(parent, name);
    }

    ~Node()
    {
        Console.WriteLine("Bye!");

        GetTree().UnregisterNode(this);
    }
}