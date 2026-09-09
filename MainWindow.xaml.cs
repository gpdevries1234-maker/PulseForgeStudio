using Microsoft.Win32;
using PulseForgeStudio.Audio;
using PulseForgeStudio.Models;
using System.IO;
using System.Media;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace PulseForgeStudio;

public partial class MainWindow : Window
{
    private ProjectData _project = CreateDemo();
    private SoundPlayer? _player;
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromMilliseconds(50) };
    private DateTime _playStart;
    private bool _playing;
    private readonly string[] _colors = ["#258BFF", "#9B6CFF", "#34D7D5", "#4DDB7C", "#FFC94A", "#A574FF", "#ED5A9D", "#6CA7D9"];

    public MainWindow()
    {
        InitializeComponent();
        _timer.Tick += Timer_Tick;
        PreviewKeyDown += (_, e) => { if (e.Key == Key.Space) { TogglePlayback(); e.Handled = true; } };
        RefreshAll();
    }

    private static ProjectData CreateDemo()
    {
        var p = new ProjectData { Name = "Untitled Project", Bpm = 128 };
        string[] names = ["Kick", "Snare", "Hat", "Bass", "Lead", "Chords", "Vox", "FX"];
        foreach (string name in names) p.Tracks.Add(new TrackData { Name = name });
        foreach (int i in new[] { 0, 4, 8, 12 }) p.Tracks[0].Steps[i] = true;
        foreach (int i in new[] { 4, 12 }) p.Tracks[1].Steps[i] = true;
        for (int i = 2; i < 16; i += 2) p.Tracks[2].Steps[i] = true;
        foreach (int i in new[] { 0, 3, 6, 10, 14 }) p.Tracks[3].Steps[i] = true;
        int[] pitches = [60, 62, 64, 67, 69, 67, 64, 62, 60, 64, 67, 72, 69, 67, 64, 62];
        for (int i = 0; i < 16; i++) p.Notes.Add(new PianoNote { Step = i, Pitch = pitches[i], Velocity = .75 });
        return p;
    }

    private void RefreshAll()
    {
        ProjectTitle.Text = _project.Name; BpmBox.Text = _project.Bpm.ToString();
        BuildSequencer(); BuildMixer(); DrawPiano();
    }

    private void BuildSequencer()
    {
        SequencerPanel.Children.Clear();
        for (int ti = 0; ti < _project.Tracks.Count; ti++)
        {
            int trackIndex = ti; var track = _project.Tracks[ti];
            var row = new Grid { Height = 49, Margin = new Thickness(0, 2, 0, 2) };
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110) });
            for (int i = 0; i < 16; i++) row.ColumnDefinitions.Add(new ColumnDefinition());
            var label = new TextBlock { Text = $"{ti + 1}   {track.Name}", VerticalAlignment = VerticalAlignment.Center, FontWeight = FontWeights.SemiBold, Foreground = Brush(_colors[ti]) };
            row.Children.Add(label);
            for (int s = 0; s < 16; s++)
            {
                int step = s;
                var b = new Button { Tag = (trackIndex, step), Margin = new Thickness(s % 4 == 0 ? 5 : 2, 7, 2, 7), Padding = new Thickness(0), Background = track.Steps[s] ? Brush(_colors[ti]) : Brush("#293241"), BorderBrush = Brush(s % 4 == 0 ? "#566174" : "#323C4B") };
                b.Click += (_, _) => { track.Steps[step] = !track.Steps[step]; BuildSequencer(); };
                Grid.SetColumn(b, s + 1); row.Children.Add(b);
            }
            SequencerPanel.Children.Add(row);
        }
    }

    private void BuildMixer()
    {
        MixerPanel.Children.Clear();
        for (int i = 0; i < _project.Tracks.Count; i++)
        {
            int index = i; var tr = _project.Tracks[i];
            var grid = new Grid { Height = 63, Margin = new Thickness(2) };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(65) });
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(44) });
            var name = new TextBlock { Text = tr.Name, VerticalAlignment = VerticalAlignment.Center, Foreground = Brush(_colors[i]), FontWeight = FontWeights.SemiBold };
            var slider = new Slider { Minimum = 0, Maximum = 1, Value = tr.Volume, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(6, 0, 8, 0) };
            slider.ValueChanged += (_, _) => tr.Volume = slider.Value;
            var mute = new Button { Content = tr.Muted ? "ON" : "M", Background = tr.Muted ? Brush("#ED5A9D") : Brush("#293241"), Padding = new Thickness(3) };
            mute.Click += (_, _) => { tr.Muted = !tr.Muted; BuildMixer(); };
            Grid.SetColumn(slider, 1); Grid.SetColumn(mute, 2); grid.Children.Add(name); grid.Children.Add(slider); grid.Children.Add(mute);
            MixerPanel.Children.Add(grid);
        }
        var master = new TextBlock { Text = "MASTER  ▮▮▮▮▮▮▯▯", Foreground = Brush("#34D7D5"), FontFamily = new FontFamily("Consolas"), FontSize = 15, Margin = new Thickness(8, 18, 8, 8) };
        MixerPanel.Children.Add(master);
    }

    private void DrawPiano()
    {
        if (PianoCanvas.ActualWidth < 1 || PianoCanvas.ActualHeight < 1) return;
        PianoCanvas.Children.Clear(); double w = PianoCanvas.ActualWidth, h = PianoCanvas.ActualHeight; double cw = w / 16, ch = h / 24;
        for (int i = 0; i <= 16; i++) PianoCanvas.Children.Add(Line(i * cw, 0, i * cw, h, i % 4 == 0 ? "#455064" : "#27303D"));
        for (int i = 0; i <= 24; i++) PianoCanvas.Children.Add(Line(0, i * ch, w, i * ch, "#27303D"));
        foreach (var note in _project.Notes)
        {
            int row = 83 - note.Pitch; if (row < 0 || row >= 24) continue;
            var rect = new Rectangle { Width = cw * note.Length - 3, Height = ch - 3, Fill = Brush("#FFC94A"), RadiusX = 2, RadiusY = 2, Stroke = Brush("#FFE39B") };
            Canvas.SetLeft(rect, note.Step * cw + 1); Canvas.SetTop(rect, row * ch + 1); PianoCanvas.Children.Add(rect);
        }
    }

    private void Piano_Click(object sender, MouseButtonEventArgs e)
    {
        var p = e.GetPosition(PianoCanvas); int step = Math.Clamp((int)(p.X / (PianoCanvas.ActualWidth / 16)), 0, 15); int row = Math.Clamp((int)(p.Y / (PianoCanvas.ActualHeight / 24)), 0, 23); int pitch = 83 - row;
        var existing = _project.Notes.FirstOrDefault(n => n.Step == step && n.Pitch == pitch);
        if (existing is null) _project.Notes.Add(new PianoNote { Step = step, Pitch = pitch }); else _project.Notes.Remove(existing);
        DrawPiano();
    }

    private void TogglePlayback()
    {
        if (_playing) StopPlayback(); else StartPlayback();
    }

    private void StartPlayback()
    {
        StopPlayback(); byte[] wav = SynthEngine.Render(_project); _player = new SoundPlayer(new MemoryStream(wav)); _player.PlayLooping();
        _playing = true; _playStart = DateTime.Now; _timer.Start(); PlayButton.Content = "❚❚  PAUSE"; StatusText.Content = "Playing";
    }

    private void StopPlayback()
    {
        _player?.Stop(); _player?.Dispose(); _player = null; _playing = false; _timer.Stop(); PlayButton.Content = "▶  PLAY"; TimeText.Text = "00:00.000"; ProgressBar.Width = 0; StatusText.Content = "Ready";
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        double length = 16 * (60.0 / _project.Bpm); double sec = (DateTime.Now - _playStart).TotalSeconds % length;
        TimeText.Text = TimeSpan.FromSeconds(sec).ToString(@"mm\:ss\.fff"); ProgressBar.Width = 170 * sec / length;
    }

    private static Brush Brush(string hex) => new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
    private static Line Line(double x1, double y1, double x2, double y2, string color) => new() { X1 = x1, Y1 = y1, X2 = x2, Y2 = y2, Stroke = Brush(color), StrokeThickness = 1 };
    private void Piano_SizeChanged(object sender, SizeChangedEventArgs e) => DrawPiano();
    private void Play_Click(object sender, RoutedEventArgs e) => TogglePlayback();
    private void Stop_Click(object sender, RoutedEventArgs e) => StopPlayback();
    private void Record_Click(object sender, RoutedEventArgs e) => MessageBox.Show("Audio-opname wordt voorbereid voor versie 1.1. In dit MVP kun je beats programmeren, noten tekenen, mixen en WAV exporteren.", "PulseForge");
    private void Bpm_Changed(object sender, RoutedEventArgs e) { if (int.TryParse(BpmBox.Text, out int bpm)) _project.Bpm = Math.Clamp(bpm, 60, 200); BpmBox.Text = _project.Bpm.ToString(); }
    private void New_Click(object sender, RoutedEventArgs e) { StopPlayback(); _project = CreateDemo(); _project.Name = "Untitled Project"; foreach (var t in _project.Tracks) Array.Fill(t.Steps, false); _project.Notes.Clear(); RefreshAll(); }
    private void Demo_Click(object sender, RoutedEventArgs e) { StopPlayback(); _project = CreateDemo(); RefreshAll(); }
    private void Clear_Click(object sender, RoutedEventArgs e) { foreach (var t in _project.Tracks) Array.Fill(t.Steps, false); _project.Notes.Clear(); RefreshAll(); }
    private void Exit_Click(object sender, RoutedEventArgs e) => Close();
    private void About_Click(object sender, RoutedEventArgs e) => MessageBox.Show("PulseForge Studio 1.0\nNative Windows music production MVP\n\nOriginal software — not affiliated with FL Studio or Image-Line.", "About");

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        var d = new SaveFileDialog { Filter = "PulseForge Project (*.pulse)|*.pulse", FileName = _project.Name + ".pulse" };
        if (d.ShowDialog() == true) { _project.Name = Path.GetFileNameWithoutExtension(d.FileName); File.WriteAllText(d.FileName, JsonSerializer.Serialize(_project, new JsonSerializerOptions { WriteIndented = true })); RefreshAll(); StatusText.Content = "Project saved"; }
    }

    private void Open_Click(object sender, RoutedEventArgs e)
    {
        var d = new OpenFileDialog { Filter = "PulseForge Project (*.pulse)|*.pulse" };
        if (d.ShowDialog() == true) { var loaded = JsonSerializer.Deserialize<ProjectData>(File.ReadAllText(d.FileName)); if (loaded is not null) { StopPlayback(); _project = loaded; RefreshAll(); StatusText.Content = "Project opened"; } }
    }

    private void Export_Click(object sender, RoutedEventArgs e)
    {
        var d = new SaveFileDialog { Filter = "Wave audio (*.wav)|*.wav", FileName = _project.Name + ".wav" };
        if (d.ShowDialog() == true) { File.WriteAllBytes(d.FileName, SynthEngine.Render(_project, 8)); StatusText.Content = "WAV exported"; MessageBox.Show("Je mix is geëxporteerd als WAV.", "PulseForge"); }
    }
}
