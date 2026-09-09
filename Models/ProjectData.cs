namespace PulseForgeStudio.Models;

public sealed class ProjectData
{
    public string Name { get; set; } = "Untitled Project";
    public int Bpm { get; set; } = 128;
    public List<TrackData> Tracks { get; set; } = [];
    public List<PianoNote> Notes { get; set; } = [];
}

public sealed class TrackData
{
    public string Name { get; set; } = "Track";
    public bool[] Steps { get; set; } = new bool[16];
    public double Volume { get; set; } = 0.75;
    public double Pan { get; set; }
    public bool Muted { get; set; }
}

public sealed class PianoNote
{
    public int Step { get; set; }
    public int Pitch { get; set; }
    public int Length { get; set; } = 1;
    public double Velocity { get; set; } = 0.8;
}
