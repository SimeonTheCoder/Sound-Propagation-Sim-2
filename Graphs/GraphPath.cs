namespace Graphs;

public class GraphPath
{
    public List<int> edges = new();
    public float start;
    public float end;

    public int Length
    {
        get
        {
            return edges.Count;
        }
    }

    public bool Equals(GraphPath b)
    {
        return this.edges.SequenceEqual(b.edges);
    }
}