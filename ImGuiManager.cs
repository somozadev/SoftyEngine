using System.Numerics;
using ImGuiNET;
using SFML.Graphics;

namespace Softine;

public class ImGuiManager
{
    public ImGuiManager(RenderWindow window)
    {
        // ImGui.CreateContext();
        // ImGui.NewFrame();
        //
        // ImGui.StyleColorsDark();
        //
        // ImGuiIOPtr io = ImGui.GetIO();
        // io.Fonts.AddFontDefault();
        // io.DisplaySize = new Vector2(100,100);
    }

    public void NewFrame()
    {
        ImGui.NewFrame();
    }

    public void Render()
    {
        ImGui.Render();
        RenderImGui();
    }

    private static void RenderImGui()
    {
    }

    public void AddSlider()
    {
        ImGui.Begin("Control Panel");
        float someValue = 0.0f;
        ImGui.SliderFloat("Slider", ref someValue, 0.0f, 100.0f);
        ImGui.End();
    }
}