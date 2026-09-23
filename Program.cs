using System.Numerics;
using ImGuiNET;
using Raylib_cs;
using rlImGui_cs;
using UiTabs;
using static Shared;

public class Program
{
    static void Main(string[] args)
    {
        InitShared();

        IUi[] tabs = [new SingleParticleUi(), new SimUi()];
        foreach (var tab in tabs) tab.Init();

        int selectedTab = 0;

        Raylib.SetConfigFlags(ConfigFlags.ResizableWindow);

        Raylib.InitWindow(1920, 1080, "Test");
        Raylib.SetTargetFPS(120);

        rlImGui.Setup(true);

        while(!Raylib.WindowShouldClose())
        {
            foreach (var tab in tabs) tab.Update();

            Zoom *= Raylib.GetMouseWheelMoveV().Y * 0.1f + 1;

            Raylib.BeginDrawing();

            for (int i = 0; i < tabs.Length; i ++)
            {
                if (i != selectedTab) continue;
                tabs[i].Draw();
            }

            rlImGui.Begin();

            ImGui.Begin("Controls");

            ImGui.InputFloat2("Room Dimensions", ref SimRef.RoomDimensions);

            ImGui.InputFloat2("Source Position", ref SourcePos);
            ImGui.InputFloat2("Listener Position", ref ListenerPos);

            ImGui.Separator();

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
        }

        Raylib.CloseWindow();
    }
}