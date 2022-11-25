using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using CSCore;
using CSCore.Codecs;
using CSCore.MediaFoundation;
using CSCore.SoundOut;
using CSCore.Streams;
using RhythmThingEditor.RhythmSystem;
using RhythmThingEditor.Waveform;
using System.Threading.Tasks;
using RhythmThingEditor.Models;
using System.Diagnostics;
using System.Linq;
namespace RhythmThingEditor
{
    /// <summary>
    /// Interaction logic for mainEditor.xaml
    /// </summary>
    public partial class MainEditor : Window
    {

        private WaveformUIHelper waveformHelper;
        private string _chartInfoPath;
        private JsonChart jsonChart;
        public static RoutedCommand PlaybackShortcut = new RoutedCommand();
        public static Dictionary<float, Grid> arrowPlacements = new Dictionary<float, Grid>();
        public static Dictionary<float, Line> linePlacements = new Dictionary<float, Line>();
        public MainEditor(string chartInfoPath, JsonChart jsonChart)
        {
            this.Title = $"Editor ({jsonChart.chartAuthor}-{jsonChart.songName})";
            PlaybackShortcut.InputGestures.Add(new KeyGesture(Key.Space));
            _chartInfoPath = chartInfoPath;
            InitializeComponent();
            //temp
            waveformHelper = new WaveformUIHelper(chartInfoPath, jsonChart, Waveform);
            this.jsonChart = jsonChart;
            BeatSelect.SelectedIndex = 0;
            //load in existing notes
            jsonChart.notes.ForEach((NoteInfo note) =>
            {
                //this is a very performance intensive operation, but thats ok! it only happens once on boot
                PlaceableObject.ActAt(waveformHelper, Waveform, ButtonSpace, jsonChart, note.time, true);
                PlaceableObject go = PlaceableObject.placed.Find(x => x.Time == note.time);
                go.Switch((int)note.collumn);
            });

            txtSongAuthor.Text = jsonChart.songAuthor;
            txtSongTitle.Text = jsonChart.songName;
            txtChartAuthor.Text = jsonChart.chartAuthor;
            txtDifficulty.Text = jsonChart.difficulty.ToString();
            txtPreview.Text = jsonChart.preview.ToString();
            txtPreviewLength.Text = jsonChart.previewLength.ToString();
        }


        private void btnPauseClick(object sender, RoutedEventArgs e)
        {
            waveformHelper.PlaybackHandler.TogglePlayback();
            btnPause.Content = waveformHelper.PlaybackHandler.Playing ? "Pause Music" : "Play Music";
        }

        private void cnvWaveClick(object sender, MouseButtonEventArgs e)
        {
            Point click = Mouse.GetPosition(Waveform);
            double percent = click.Y / Waveform.Height;
            
            waveformHelper.PlaybackHandler.SetPositionPercent(percent);
            //basic for now. Will need to change with snapping probably? 

        }

        /*public void CreateArrowPlacement(float beat)
        {
            const int NUM_SPACES = 4;
            double canvasPos = playbackHandler.GetCanvasPosFromBeat(beat);

            Line newLine = new Line();
            Canvas canvas = Waveform;
            // X doesn't need to set, since SizeChanged event listener is triggered when generating the window
            newLine.Y1 = canvasPos;
            newLine.Y2 = canvasPos;
            newLine.SizeChanged += ResizeLineWidth;
            newLine.Stroke = System.Windows.Media.Brushes.Blue;
            newLine.StrokeThickness = 2;
            canvas.Children.Add(newLine);
            linePlacements.Add(beat, newLine);

            Grid outerGrid = new Grid();
            outerGrid.MinHeight = 80;
            outerGrid.Width = ButtonSpace.Width;
            outerGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(30) });
            outerGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(30) });
            Grid innerGrid = new Grid();
            innerGrid.SetValue(Grid.RowProperty, 1);
            TimeSpan time = playbackHandler.GetPositionFromPercent(playbackHandler.GetPercentFromBeat(beat));
            outerGrid.Children.Add(new TextBlock() { Text = time.ToString(), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center });
            outerGrid.Children.Add(innerGrid);
            for (int i = 0; i < NUM_SPACES; i++)
            {
                innerGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                Arrow newArrow = new Arrow(innerGrid, i, beat, jsonChart);
            }
            Canvas.SetTop(outerGrid, canvasPos-outerGrid.MinHeight/2);
            ButtonSpace.Children.Add(outerGrid);
            arrowPlacements.Add(beat, outerGrid);
        }*/
        private void cvnWave_RightClick(object sender, MouseButtonEventArgs e) => PlaceableObject.ActAt(waveformHelper, Waveform, ButtonSpace, jsonChart);
        /*
        {
            double clickPercent = playbackHandler.GetCanvasPosBeatSnapped(Mouse.GetPosition(Waveform).Y) / Waveform.Height;

            float beat = playbackHandler.GetBeatPercentWSnap(clickPercent);
            if(arrowPlacements.ContainsKey(beat))
            {
                //remove!!
                Arrow.activeNotes.FindAll(x => x.time == beat).ForEach((arrow) =>
                {
                    arrow.SetNote(false);
                });
                ButtonSpace.Children.Remove(arrowPlacements[beat]);
                arrowPlacements.Remove(beat);
                Waveform.Children.Remove(linePlacements[beat]);
                linePlacements.Remove(beat);
            } else
            {
                CreateArrowPlacement(beat);
            }

        }
        */

        private void RemoveArrowPlacement()
        {

        }


        private void ResizeLineWidth(object sender, RoutedEventArgs e)
        {
            Line myLine = (Line)sender;

            Double width = Double.IsNaN(myLine.Width) ? 2000 : myLine.Width;

            myLine.X1 = 0 - (width / 2);
            myLine.X2 = width / 2;
        }

        private void BeatSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox s = (ComboBox)sender;
            ComboBoxItem selection = (ComboBoxItem)s.SelectedItem;
            
            waveformHelper.PlaybackHandler.ChangeDivision((string)selection.Content);
        }

        private void chkbxSnapBeat_CheckChanged(object sender, RoutedEventArgs e)
        {
            CheckBox check = (CheckBox)sender;
            waveformHelper.PlaybackHandler.SnapToBeat = check.IsChecked ?? false;
        }

        private void btnHotLoad(object sender, RoutedEventArgs e)
        {
            jsonChart.GenerateFile(_chartInfoPath);
            ProcessStartInfo process = new ProcessStartInfo
            {
                FileName = EditorSettings.MainConfig.GameInstall,
                CreateNoWindow = false,
                UseShellExecute = true,
                Arguments = $"\"{_chartInfoPath}\""
            };
            Process.Start(process);
        }

        private void btnHotLoadTime(object sender, RoutedEventArgs e)
        {
            jsonChart.GenerateFile(_chartInfoPath);
            ProcessStartInfo process = new ProcessStartInfo
            {
                FileName = EditorSettings.MainConfig.GameInstall,
                CreateNoWindow = false,
                UseShellExecute = true,
                Arguments = $"\"{_chartInfoPath}\" {waveformHelper.PlaybackHandler.CurrentMs}"
            };
            Process.Start(process);
        }
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            jsonChart.songName = String.IsNullOrEmpty(txtSongTitle.Text) ? "Unknown" : txtSongTitle.Text;
            jsonChart.songAuthor = String.IsNullOrEmpty(txtSongAuthor.Text) ? "Unknown" : txtSongAuthor.Text;
            jsonChart.chartAuthor = String.IsNullOrEmpty(txtChartAuthor.Text) ? "Unknown" : txtChartAuthor.Text;
            jsonChart.preview = float.Parse(txtPreview.Text);
            jsonChart.previewLength = float.Parse(txtPreviewLength.Text);
            jsonChart.difficulty = int.Parse(txtDifficulty.Text);

            jsonChart.GenerateFile(_chartInfoPath);
            MessageBox.Show("Your level has been created at " + System.IO.Path.GetDirectoryName(_chartInfoPath));
        }

        private void PlaybackShortcut_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            waveformHelper.PlaybackHandler.TogglePlayback();
        }

        private void btnSaveInfo_Click(object sender, RoutedEventArgs e)
        {
            jsonChart.songName = txtSongTitle.Text;
            jsonChart.chartAuthor = txtChartAuthor.Text;
            jsonChart.difficulty = int.Parse(txtDifficulty.Text);
            jsonChart.songAuthor = txtSongAuthor.Text;
            this.Title = $"Editor ({jsonChart.chartAuthor}-{jsonChart.songName})";
        }
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            AudioManager.Destroy();

        }

        #region NumericTextBox
        private void NumericOnly(object sender, TextCompositionEventArgs e) => NumericTextBox.NumericOnly(sender, e);
        private void VerifyNumeric(object sender, RoutedEventArgs e) => NumericTextBox.VerifyNumeric((TextBox)sender);

        private void btnIncrease_Click(object sender, RoutedEventArgs e)
        {
            TextBox DIFFICULTY = txtDifficulty, PREVIEW = txtPreview, PREVIEW_LENGTH = txtPreviewLength;
            switch (((Button)sender).Tag)
            {
                case "difficulty":
                    NumericTextBox.VerifyNumeric(DIFFICULTY, 1);
                    break;
                case "preview":
                    NumericTextBox.VerifyNumeric(PREVIEW, 1);
                    break;
                case "previewLength":
                    NumericTextBox.VerifyNumeric(PREVIEW_LENGTH, 1);
                    break;
            }
        }

        private void btnDecrease_Click(object sender, RoutedEventArgs e)
        {
            TextBox DIFFICULTY = txtDifficulty, PREVIEW = txtPreview, PREVIEW_LENGTH = txtPreviewLength;
            switch (((Button)sender).Tag)
            {
                case "difficulty":
                    NumericTextBox.VerifyNumeric(DIFFICULTY, -1);
                    break;
                case "preview":
                    NumericTextBox.VerifyNumeric(PREVIEW, -1);
                    break;
                case "previewLength":
                    NumericTextBox.VerifyNumeric(PREVIEW_LENGTH, -1);
                    break;
            }
        }
        #endregion
    }
}
