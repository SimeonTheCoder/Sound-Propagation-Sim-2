using System.Numerics;
using ImGuiNET;
using Raylib_cs;
using Utils;
using static Shared;

namespace UiTabs;

public class SingleParticleUi : IUi
{
    private bool spawnOtherWorlds = false;
    private bool drawTrails = false;
    private bool drawGuide = false;
    private bool drawLabels = false;

    private bool drawVelocities = false;
 
    private float timeScale = 0f;
 
    private int aimX, aimY;

    private Vector2 initialVelocity = Vector2.Normalize(new(1.0f, 0.3f));
 
    private List<Vector2> positions = new();
    private List<Vector2> velocities = new();
    private List<Vector2> boundariesX = new();
    private List<Vector2> boundariesY = new();

    private void SpawnParticles()
    {
        positions = new();
        velocities = new();

        boundariesX = new();
        boundariesY = new();

        for (int i = 0; i < 10_000; i ++)
        {
            float vx = MathF.Cos(i / 10000f * 2 * MathF.PI + 0.4f);
            float vy = MathF.Sin(i / 10000f * 2 * MathF.PI + 0.4f);

            positions.Add(new(SourcePos.X, SourcePos.Y));
            velocities.Add(new(vx, vy));

            boundariesX.Add(new(0, SimRef.RoomDimensions.X));
            boundariesY.Add(new(0, SimRef.RoomDimensions.Y));
        }
    }

    public void Init()
    {
        Zoom = 1000f;
        timeScale = 0f;
        Vector2 initialVelocity = Vector2.Normalize(new(1.0f, 0.3f));

        positions = new();
        velocities = new();

        boundariesX = new();
        boundariesY = new();

        positions = [new(SourcePos.X, SourcePos.Y)];
        velocities = [new(initialVelocity.X, initialVelocity.Y)];
        boundariesX = [new(0, SimRef.RoomDimensions.X)];
        boundariesY = [new(0, SimRef.RoomDimensions.Y)];
    }

    public void Update()
    {
        float dt = Raylib.GetFrameTime() * timeScale;

        int posCount = positions.Count;

        for (int i = 0; i < posCount; i ++)
        {
            Vector2 prevPos = new(positions[i].X, positions[i].Y);
            positions[i] += velocities[i] * dt;

            if (positions[i].X < boundariesX[i].X || positions[i].X > boundariesX[i].Y)
            {
                if (spawnOtherWorlds)
                {
                    positions.Add(new(positions[i].X, positions[i].Y));
                    velocities.Add(new(velocities[i].X, velocities[i].Y));

                    float dir = ((positions[i].X < boundariesX[i].X) ? -1 : 1) * SimRef.RoomDimensions.X;

                    boundariesX.Add(new(boundariesX[i].X + dir, boundariesX[i].Y + dir));
                    boundariesY.Add(new(boundariesY[i].X, boundariesY[i].Y));
                }

                velocities[i] = new(-velocities[i].X, velocities[i].Y);
                positions[i] = prevPos;
            }

            if (positions[i].Y < boundariesY[i].X || positions[i].Y > boundariesY[i].Y)
            {
                if (spawnOtherWorlds)
                {
                    positions.Add(new(positions[i].X, positions[i].Y));
                    velocities.Add(new(velocities[i].X, velocities[i].Y));

                    float dir = ((positions[i].Y < boundariesY[i].X) ? -1 : 1) * Shared.SimRef.RoomDimensions.Y;

                    boundariesX.Add(new(boundariesX[i].X, boundariesX[i].Y));
                    boundariesY.Add(new(boundariesY[i].X + dir, boundariesY[i].Y + dir));
                }

                velocities[i] = new(velocities[i].X, -velocities[i].Y);
                positions[i] = prevPos;
            }
        }
    }

    public void Draw()
    {
        if(!drawTrails) Raylib.ClearBackground(Color.Black);

        for (int i = -10; i < 10; i ++)
        {
            for (int j = -10; j < 10; j ++)
            {
                if(drawGuide)
                {
                    RenderingUtils.Rect(
                        MathUtils.GetReflectedCoords(ListenerPos, j, i, SimRef.RoomDimensions),
                        new(4, 4),
                        Color.Red
                    );
                }

                if (!boundariesX.Any(b => b.X == j)) continue;
                if (!boundariesY.Any(b => b.X == i)) continue;

                (int x, int y) textPos = RenderingUtils.TransformCoords(new(j + 0.01f, i - 0.01f));
                if(drawLabels) Raylib.DrawText($"({j},{i - 1})", textPos.x, textPos.y, (int) (0.03f * Zoom), Color.Gray);

                Color c = (i == 0 && j == 0) ? Color.Yellow : Color.Gray;

                RenderingUtils.Line(new(j, i), new(j + SimRef.RoomDimensions.X, i), c);
                RenderingUtils.Line(new(j, i), new(j, i + SimRef.RoomDimensions.Y), c);
                RenderingUtils.Line(new(j, i + SimRef.RoomDimensions.Y), new(j + SimRef.RoomDimensions.X, i + SimRef.RoomDimensions.Y), c);
                RenderingUtils.Line(new(j + SimRef.RoomDimensions.X, i), new(j + SimRef.RoomDimensions.X, i + SimRef.RoomDimensions.Y), c);
            }
        }

        for (int i = 0; i < positions.Count; i ++)
        {
            RenderingUtils.Rect(positions[i], new(4, 4), Color.Blue);
            if(drawVelocities) RenderingUtils.Line(positions[i], positions[i] + velocities[i] * 0.1f, Color.Blue);
        }

        if (drawGuide) RenderingUtils.Line(SourcePos - initialVelocity * 10, SourcePos + initialVelocity * 10, Color.Pink);
    }

    public bool DrawUi()
    {
        if (!ImGui.BeginTabItem("Particle Reflection Visualization")) return false;

        ImGui.Checkbox("Spawn other worlds", ref spawnOtherWorlds);
        if (ImGui.Button("Spawn all")) SpawnParticles();

        ImGui.Separator();

        ImGui.SliderFloat("Zoom", ref Zoom, 25, 2000);
        
        ImGui.Checkbox("Draw trails", ref drawTrails);
        ImGui.Checkbox("Draw velocity vectors", ref drawVelocities);
        ImGui.Checkbox("Draw guide", ref drawGuide);
        ImGui.Checkbox("Labels", ref drawLabels);

        ImGui.SliderFloat("Speed", ref timeScale, 0f, 1f);

        if (ImGui.Button("Play")) timeScale = 1f;
        ImGui.SameLine();
        if (ImGui.Button("Pause")) timeScale = 0f;

        ImGui.SliderInt("Aim X", ref aimX, -10, 10);
        ImGui.SliderInt("Aim Y", ref aimY, -10, 10);

        if (ImGui.Button("Aim"))
        {
            Vector2 targetPos = MathUtils.GetReflectedCoords(SourcePos, aimX, aimY, SimRef.RoomDimensions);
            
            velocities[0] = Vector2.Normalize(targetPos - positions[0]);
            initialVelocity = new(velocities[0].X, velocities[0].Y);
        }

        ImGui.SameLine();

        if (ImGui.Button("Reset"))
        {
            timeScale = 0f;

            positions = [new(SourcePos.X, SourcePos.Y)];
            velocities = [new(initialVelocity.X, initialVelocity.Y)];
            boundariesX = [new(0, SimRef.RoomDimensions.X)];
            boundariesY = [new(0, SimRef.RoomDimensions.Y)];
        }

        ImGui.EndTabItem();
        return true;
    }
}