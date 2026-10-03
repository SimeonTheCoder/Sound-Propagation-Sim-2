using System.Numerics;
using ImGuiNET;
using Raylib_cs;
using rlImGui_cs;
using Simulator;
using UiTabs;
using Utils;
using static Shared;

public class Program
{
    static void Main(string[] args)
    {
        InitShared();

        ISim[] simulations = [new RectSimulation(), new ConvexSimulation()];
        SimRef = simulations[0];

        IUi[] tabs = [new SingleParticleUi(), new ConvexRoomUi(), new RecordUi()];
        foreach (var tab in tabs) tab.Init();

        Zoom = 1000f;
        Pan = new(0f, 0f);

        int selectedTab = 0;

        Raylib.SetConfigFlags(ConfigFlags.ResizableWindow);

        Raylib.InitWindow(1920, 1080, "Test");
        Raylib.InitAudioDevice();
        Raylib.SetTargetFPS(120);

        rlImGui.Setup(true);

        Vector2 mouseDragStart = new(0f, 0f);
        bool mouseDragging = false;

        bool firstOpen = true;

        while(!Raylib.WindowShouldClose())
        {
            foreach (var tab in tabs) tab.Update();

            Zoom *= Raylib.GetMouseWheelMoveV().Y * 0.1f + 1;

            Vector2 mousePos = Raylib.GetMousePosition();

            if (Raylib.IsMouseButtonDown(MouseButton.Middle))
            {
                if (!mouseDragging)
                {
                    mouseDragStart = RenderingUtils.ClipToXY(((int) mousePos.X, (int) mousePos.Y));
                    mouseDragging = true;
                }
                else
                {
                    Vector2 curr = RenderingUtils.ClipToXY(((int) mousePos.X, (int) mousePos.Y));
                    Vector2 offset = curr - mouseDragStart;

                    Pan += offset;
                }
            }
            else
            {
                mouseDragging = false;
            }

            Raylib.BeginDrawing();

            for (int i = 0; i < tabs.Length; i ++)
            {
                if (i != selectedTab) continue;
                tabs[i].Draw();
            }

            rlImGui.Begin();

            if (firstOpen)
            {
                ImGui.SetNextWindowPos(new(Raylib.GetScreenWidth() * 22 / 30, Raylib.GetScreenHeight() / 20));
                ImGui.SetNextWindowSize(new(Raylib.GetScreenWidth() / 4, Raylib.GetScreenHeight() / 12));
            }

            ImGui.Begin("Controls");

            ImGui.InputFloat2("Source Position", ref SourcePos);
            ImGui.InputFloat2("Listener Position", ref ListenerPos);

            ImGui.End();

            if (firstOpen)
            {
                ImGui.SetNextWindowPos(new(Raylib.GetScreenWidth() * 22 / 30, Raylib.GetScreenHeight() / 6.7f));
                ImGui.SetNextWindowSize(new(Raylib.GetScreenWidth() / 4, Raylib.GetScreenHeight() / 12));
            }

            ImGui.Begin("Simulation Options");

            if (ImGui.BeginCombo("simulator", "Select Simulator"))
            {
                foreach (ISim sim in simulations)
                {
                    if (ImGui.Selectable(sim.GetType().Name))
                        SimRef = sim;
                }

                ImGui.EndCombo();
            }

            SimRef.DrawControls();

            ImGui.End();

            if (firstOpen)
            {
                ImGui.SetNextWindowPos(new(Raylib.GetScreenWidth() * 22 / 30, Raylib.GetScreenHeight() * 2 / 8));
                ImGui.SetNextWindowSize(new(Raylib.GetScreenWidth() / 4, Raylib.GetScreenHeight() / 2));
            }

            ImGui.Begin("Visualisation");

            ImGui.BeginTabBar("tab_bar");

            for (int i = 0; i < tabs.Length; i ++)
            {
                bool result = tabs[i].DrawUi();
                
                if (!result) continue;

                if (selectedTab != i) tabs[i].Init();
                selectedTab = i;
            }

            ImGui.EndTabBar();

            ImGui.End();

            rlImGui.End();
            Raylib.EndDrawing();

            firstOpen = false;
        }

        Raylib.CloseAudioDevice();
        Raylib.CloseWindow();
    }
}