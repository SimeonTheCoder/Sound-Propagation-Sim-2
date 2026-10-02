namespace Simulator;

public interface ISim
{
    public float[] WaveformL {get; set;}
    public float[] WaveformR {get; set;}

    public object Room {get; set;}

    public void Calculate();

    public void DrawControls();
}