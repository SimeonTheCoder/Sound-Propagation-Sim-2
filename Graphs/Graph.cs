using System.Numerics;
using Utils;

namespace Graphs;

public class Graph
{
    public List<Vector2> nodes = new();
    public List<(int from, int to)> edges = new();

    public List<GraphPath> data = new();

    public int GetCollisionEdge(float theta, int lastCollision)
    {
        int collisionEdge = -1;
        float lastY = -999;

        for (int i = 0; i < edges.Count; i++)
        {
            Vector2 a = MathUtils.RotateVec(nodes[edges[i].from], theta);
            Vector2 b = MathUtils.RotateVec(nodes[edges[i].to], theta);

            if (b.X < a.X)
            {
                Vector2 temp = new(a.X, a.Y);

                a.X = b.X;
                a.Y = b.Y;

                b.X = temp.X;
                b.Y = temp.Y;
            }

            if (i != lastCollision && a.X <= 0 && b.X >= 0)
            {
                float t = (0f - a.X) / (b.X - a.X);
                float y = (1 - t) * a.Y + t * b.Y;

                if (y > lastY)
                {
                    collisionEdge = i;
                    lastY = y;
                }
            }
        }

        return collisionEdge;
    }

    public static Vector2 MirrorNodeAlongAxis(Graph graph, Vector2 vec, int selectedEdge)
    {
        if (selectedEdge == -1)
            return vec;

        Vector2 edgeStart = graph.nodes[graph.edges[selectedEdge].from];
        Vector2 edgeEnd = graph.nodes[graph.edges[selectedEdge].to];

        float dx = edgeEnd.X - edgeStart.X;
        float dy = edgeEnd.Y - edgeStart.Y;

        float theta = MathF.Atan2(dy, dx);

        float xo = edgeStart.X;
        float yo = edgeStart.Y;

        return MathUtils.DoAxisFlip(vec, theta * 180 / MathF.PI, xo, yo);
    }

    public static Graph MirrorGraphAlongAxis(Graph graph, int selectedEdge)
    {
        if (selectedEdge == -1)
            return graph;

        Vector2 edgeStart = graph.nodes[graph.edges[selectedEdge].from];
        Vector2 edgeEnd = graph.nodes[graph.edges[selectedEdge].to];

        float dx = edgeEnd.X - edgeStart.X;
        float dy = edgeEnd.Y - edgeStart.Y;

        float theta = MathF.Atan2(dy, dx);

        float xo = edgeStart.X;
        float yo = edgeStart.Y;

        return new()
        {
            nodes = graph
                .nodes.Select(n => MathUtils.DoAxisFlip(n, theta * 180 / MathF.PI, xo, yo))
                .ToList(),
            edges = graph.edges,
        };
    }

    public static GraphPath GeneratePath(Graph graph, int count, float theta)
    {
        Graph currGraph = graph;
        List<int> edges = [currGraph.GetCollisionEdge(theta, -1)];

        for (int i = 0; i < count - 1; i++)
        {
            currGraph = MirrorGraphAlongAxis(currGraph, edges[i]);
            edges.Add(currGraph.GetCollisionEdge(theta, edges.Count > 0 ? edges[i] : -1));

            if (edges[edges.Count - 1] == -1)
            {
                System.Console.WriteLine("ERR");
                currGraph.GetCollisionEdge(theta, edges[edges.Count - 2]);
            }
        }

        return new()
        {
            edges = edges,
            start = theta,
            end = theta
        };
    }

    public void GenerateData(int anglesCount, int reflectionsCount)
    {
        this.data = new();

        GraphPath lastPath = null!;
        float start = 0;

        for (int i = 0; i < anglesCount; i++)
        {
            float theta = 360f / anglesCount * i + 90;
            if (theta > 360f)
                theta -= 360f;

            GraphPath path = GeneratePath(this, reflectionsCount, theta);
            GraphPath currPath = path;

            if (lastPath != null && !currPath.Equals(lastPath))
            {
                data.Add(new()
                {
                    edges = currPath.edges,
                    start = start,
                    end = (360f / anglesCount * (i - 1) + 90f) % 360f
                });

                start = theta;
            }

            lastPath = currPath;

            if (i == anglesCount - 1)
            {
                data.Add(new(){
                    edges = currPath.edges,
                    start = start,
                    end = 90f
                });
            }
        }
    }

    public Vector2 TransformNodeWithGraph(Vector2 mic, GraphPath path, int count)
    {
        Vector2 copy = mic;
        Graph graphCopy = this;

        for (int i = 0; i < count; i++)
        {
            graphCopy = MirrorGraphAlongAxis(graphCopy, path.edges[i]);
            copy = MirrorNodeAlongAxis(graphCopy, copy, path.edges[i]);
        }

        return copy;
    }

    public bool IsValidPath(int pathIndex, Vector2 pos, int length)
    {
        Vector2 newMic = TransformNodeWithGraph(
            pos, data[pathIndex], length
        );

        float angle = MathF.Atan2(newMic.X, newMic.Y) / MathF.PI * 180f;
        if (angle < 0f) angle = angle + 360f;

        bool valid = angle >= data[pathIndex].start && angle <= data[pathIndex].end;
        if (valid) return true;

        int truePathIndex = -1;

        for(int i = 0; i < data.Count; i ++)
        {
            if (angle < data[i].start || angle > data[i].end) continue;
            truePathIndex = i;
            break;
        }

        if (truePathIndex == -1) return false;

        return data[pathIndex].Equals(data[truePathIndex], length);
    }
}
