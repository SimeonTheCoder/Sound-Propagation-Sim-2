using System.Numerics;
using UiTabs;
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

        // theta = (90f - theta + 360f) % 360f;

        // for (int i = 0; i < edges.Count; i++)
        // {
        //     Vector2 from = nodes[edges[i].from];
        //     Vector2 to = nodes[edges[i].to];

        //     //You have to move the origin :'(
        //     float start = MathUtils.GetAngle(from);
        //     float end = MathUtils.GetAngle(to);

        //     System.Console.WriteLine($"i: {i}, {start:f1} - {end:f1}");

        //     bool collision =
        //         (start <= theta && end >= theta && start < end)
        //         || (end < start && (end >= theta || theta >= start));

        //     if (collision)
        //         return i;
        // }

        // return -1;
    }

    public static Vector2 MirrorNodeAlongAxis(Graph graph, Vector2 vec, int selectedEdge)
    {
        if (selectedEdge == -1)
            return vec;

        Vector2 edgeStart = graph.nodes[graph.edges[selectedEdge].from];
        Vector2 edgeEnd = graph.nodes[graph.edges[selectedEdge].to];

        float theta = MathUtils.GetAngle(edgeEnd - edgeStart);

        float xo = edgeStart.X;
        float yo = edgeStart.Y;

        return MathUtils.DoAxisFlip(vec, theta, xo, yo);
    }

    public static Graph MirrorGraphAlongAxis(Graph graph, int selectedEdge)
    {
        if (selectedEdge == -1)
            return graph;

        Vector2 edgeStart = graph.nodes[graph.edges[selectedEdge].from];
        Vector2 edgeEnd = graph.nodes[graph.edges[selectedEdge].to];

        float theta = MathUtils.GetAngle(edgeEnd - edgeStart);

        float xo = edgeStart.X;
        float yo = edgeStart.Y;

        return new Graph()
        {
            nodes = graph.nodes.Select(n => MathUtils.DoAxisFlip(n, theta, xo, yo)).ToList(),
            edges = graph.edges.Select(p => p).ToList(),
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
            end = theta,
        };
    }

    public void GenerateData(int anglesCount, int reflectionsCount)
    {
        this.data = new();

        GraphPath lastPath = null!;
        float start = 0;

        for (int i = 0; i < anglesCount; i++)
        {
            float theta = 360f / anglesCount * i + 0;
            if (theta > 360f)
                theta -= 360f;

            GraphPath path = GeneratePath(this, reflectionsCount, 90f - theta);
            GraphPath currPath = path;

            if (lastPath != null && !currPath.Equals(lastPath))
            {
                data.Add(
                    new()
                    {
                        edges = currPath.edges,
                        start = start,
                        end = (360f / anglesCount * (i - 1) + 0f) % 360f,
                    }
                );

                start = theta;
            }

            lastPath = currPath;

            if (i == anglesCount - 1)
            {
                data.Add(
                    new()
                    {
                        edges = currPath.edges,
                        start = start,
                        end = 0f,
                    }
                );
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
        Vector2 newMic = TransformNodeWithGraph(pos, data[pathIndex], length);
        float angle = MathUtils.GetAngle(new(newMic.X, newMic.Y));

        bool valid = angle >= data[pathIndex].start && angle <= data[pathIndex].end;
        if (valid)
            return true;

        int truePathIndex = -1;

        for (int i = 0; i < data.Count; i++)
        {
            if (angle < data[i].start || angle > data[i].end)
                continue;
            truePathIndex = i;
            break;
        }

        if (truePathIndex == -1)
            return false;

        return data[pathIndex].Equals(data[truePathIndex], length);
    }
}
