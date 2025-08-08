namespace ZombieSurvival.Engine.NodeSystem;

[Serializable]
public class TagException : Exception
{
    public TagException() { }
    public TagException(string message) : base(message) { }
    public TagException(string message, Exception inner) : base(message, inner) { }
}

/// <summary>
/// Used to quickly query nodes.
/// </summary>
public static class TagSystem
{
    internal static readonly Dictionary<string, List<Node>> Tags = [];

    /// <summary>
    /// Get all nodes that have the <paramref name="tag"/>.
    /// </summary>
    /// <param name="tag">The tag name</param>
    /// <returns>Array of nodes</returns>
    /// <exception cref="TagException"></exception>
    public static Node[] GetTagged(string tag)
    {
        if (!Tags.TryGetValue(tag, out List<Node>? value))
        {
            throw new TagException($"Tag {tag} does not exist");
        }

        return [.. value];
    }

    /// <summary>
    /// Checks if a <paramref name="tag"/> is valid.
    /// </summary>
    /// <param name="tag">The tag name</param>
    /// <returns><c>true</c>, if the <paramref name="tag"/> is valid.</returns>
    public static bool IsATag(string tag)
    {
        return Tags.ContainsKey(tag);
    }

    /// <summary>
    /// Does the <paramref name="node"/> has a <paramref name="tag"/>.
    /// </summary>
    /// <param name="node">The node checked</param>
    /// <param name="tag">The tag name</param>
    /// <returns><c>true</c>, if the <paramref name="tag"/> is valid.</returns>
    public static bool HasTag(Node node, string tag)
    {
        Node[] tagged = GetTagged(tag);

        return tagged.Contains(node);
    }

    /// <summary>
    /// Adds the <paramref name="tag"/> to the <paramref name="node"/>.
    /// </summary>
    /// <param name="node">The node to add</param>
    /// <param name="tag">The tag name</param>
    public static void AddTag(Node node, string tag)
    {
        bool isTag = IsATag(tag);

        if (isTag)
        {
            Tags[tag].Add(node);
            node._Tags.Add(tag);
        }
        else
        {
            Tags[tag] = [];
            Tags[tag].Add(node);
            node._Tags.Add(tag);
        }
    }

    /// <summary>
    /// Removes the <paramref name="tag"/> from the <paramref name="node"/>.
    /// </summary>
    /// <param name="node">The node that the tag is removed.</param>
    /// <param name="tag">The tag name</param>
    /// <returns><c>true</c>, if the <paramref name="tag"/> did exist on the tag.</returns>
    public static bool RemoveTag(Node node, string tag)
    {
        bool isATag = IsATag(tag);
        if (isATag)
        {
            return false;
        }

        node._Tags.Remove(tag);

        bool wasATag = Tags[tag].Remove(node);

        return wasATag;
    }

    /// <summary>
    /// Gets the tags on the <paramref name="node"/>.
    /// </summary>
    /// <param name="node">The node</param>
    /// <returns>The tags attached to the <paramref name="node"/>.</returns>
    public static string[] GetTags(Node node)
    {
        return [.. node._Tags];
    }

    /// <summary>
    /// Gets all the tags registered.
    /// </summary>
    /// <returns>All of the tags registered.</returns>
    public static string[] GetAllTags()
    {
        return [.. Tags.Keys];
    }
}