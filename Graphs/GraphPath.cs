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

    public bool Equals(GraphPath b, int steps=-1)
    {
        return steps == -1 ?
            this.edges.SequenceEqual(b.edges) :
            this.edges.Take(steps).SequenceEqual(b.edges.Take(steps));
    }
}