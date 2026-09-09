using PulseForgeStudio.Models;

namespace PulseForgeStudio.Audio;

public static class SynthEngine
{
    public const int SampleRate = 44100;

    public static byte[] Render(ProjectData project, int bars = 4)
    {
        double beat = 60.0 / Math.Clamp(project.Bpm, 60, 200);
        double stepSeconds = beat / 4.0;
        int totalSteps = bars * 16;
        int samples = (int)(totalSteps * stepSeconds * SampleRate);
        float[] left = new float[samples];
        float[] right = new float[samples];

        for (int s = 0; s < totalSteps; s++)
        {
            int patternStep = s % 16;
            int offset = (int)(s * stepSeconds * SampleRate);
            for (int tr = 0; tr < project.Tracks.Count; tr++)
            {
                var track = project.Tracks[tr];
                if (!track.Muted && track.Steps[patternStep])
                    AddVoice(left, right, offset, tr, track.Volume, track.Pan);
            }
        }

        foreach (var note in project.Notes)
        {
            for (int bar = 0; bar < bars; bar++)
            {
                int offset = (int)((bar * 16 + note.Step) * stepSeconds * SampleRate);
                double freq = 440.0 * Math.Pow(2, (note.Pitch - 69) / 12.0);
                AddLead(left, right, offset, freq, note.Length * stepSeconds, note.Velocity);
            }
        }

        return WaveWriter.Encode(left, right, SampleRate);
    }

    private static void AddVoice(float[] l, float[] r, int start, int type, double volume, double pan)
    {
        double duration = type switch { 0 => .34, 1 => .20, 2 => .09, 3 => .42, _ => .28 };
        int count = Math.Min((int)(duration * SampleRate), l.Length - start);
        if (count <= 0) return;
        var random = new Random(991 + start + type);
        for (int i = 0; i < count; i++)
        {
            double t = i / (double)SampleRate;
            double sample = type switch
            {
                0 => Math.Sin(2 * Math.PI * (115 - 70 * t / duration) * t) * Math.Exp(-t * 12),
                1 => (random.NextDouble() * 2 - 1) * Math.Exp(-t * 24) + .25 * Math.Sin(2 * Math.PI * 190 * t) * Math.Exp(-t * 15),
                2 => (random.NextDouble() * 2 - 1) * Math.Exp(-t * 52),
                3 => Math.Tanh(2.2 * Math.Sin(2 * Math.PI * 55 * t)) * Math.Exp(-t * 5),
                _ => Math.Sin(2 * Math.PI * 330 * t) * Math.Exp(-t * 8)
            };
            Mix(l, r, start + i, sample * volume * .65, pan);
        }
    }

    private static void AddLead(float[] l, float[] r, int start, double frequency, double duration, double velocity)
    {
        int count = Math.Min((int)(duration * SampleRate), l.Length - start);
        for (int i = 0; i < count; i++)
        {
            double t = i / (double)SampleRate;
            double env = Math.Min(1, t / .015) * Math.Min(1, (duration - t) / .08);
            double saw = 0;
            foreach (double detune in new[] { .992, .997, 1.0, 1.004, 1.009 })
                saw += 2 * ((t * frequency * detune) % 1.0) - 1;
            Mix(l, r, start + i, saw / 5 * env * velocity * .22, 0);
        }
    }

    private static void Mix(float[] l, float[] r, int index, double sample, double pan)
    {
        if (index < 0 || index >= l.Length) return;
        l[index] += (float)(sample * Math.Sqrt((1 - pan) * .5));
        r[index] += (float)(sample * Math.Sqrt((1 + pan) * .5));
    }
}

public static class WaveWriter
{
    public static byte[] Encode(float[] left, float[] right, int sampleRate)
    {
        using var ms = new MemoryStream(); using var w = new BinaryWriter(ms);
        int dataSize = left.Length * 4;
        w.Write("RIFF"u8.ToArray()); w.Write(36 + dataSize); w.Write("WAVE"u8.ToArray());
        w.Write("fmt "u8.ToArray()); w.Write(16); w.Write((short)1); w.Write((short)2);
        w.Write(sampleRate); w.Write(sampleRate * 4); w.Write((short)4); w.Write((short)16);
        w.Write("data"u8.ToArray()); w.Write(dataSize);
        for (int i = 0; i < left.Length; i++)
        {
            w.Write((short)(Math.Clamp(Math.Tanh(left[i]), -1, 1) * short.MaxValue));
            w.Write((short)(Math.Clamp(Math.Tanh(right[i]), -1, 1) * short.MaxValue));
        }
        return ms.ToArray();
    }
}
