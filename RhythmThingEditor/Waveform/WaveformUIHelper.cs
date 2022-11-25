using CSCore;
using RhythmThingEditor.RhythmSystem;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace RhythmThingEditor.Waveform
{
    public class WaveformUIHelper
    {

        const float OUTREACH = 0.75f;
        const int WAVEREACH = 40;
        const float CANVASWIDTH = 500; //how to do dynamically??

        private List<Line> BeatMeasures = new List<Line>();
        private Line _trackingLine;
        private SongPlaybackHandler _playbackHandler;

        public SongPlaybackHandler PlaybackHandler { get { return _playbackHandler; } }
        private Canvas _waveCanvas;
        public WaveformUIHelper(string chartInfoPath, RhythmSystem.JsonChart jsonChart, Canvas waveCanvas )
        {
            _waveCanvas = waveCanvas;
            SongPlaybackHandler.PlaybackHandlerLoad += PlaybackLoaded;
            SongPlaybackHandler.BeatDivisionChanges += DrawBeatMeasures;
            _playbackHandler = new SongPlaybackHandler(chartInfoPath, jsonChart);
        }

        private void PlaybackLoaded(object sender, PlaybackHandlerLoadArgs e)
        {
            DrawWave(e.AudioTrack.sampleSource, _waveCanvas);
            CreateTrackingLine(e.AudioTrack, _waveCanvas);
        }

        public double GetCanvasPosBeatSnapped(double Ypos)
        {
            double percent = Ypos / _waveCanvas.Height;
            percent = PlaybackHandler.GetPercentBeatSnapped(percent);
            return percent * _waveCanvas.Height;
        }
        public double GetCanvasPosFromBeat(float beat)
        {
            double percent = PlaybackHandler.GetPercentFromBeat(beat);
            return percent * _waveCanvas.Height;
        }
        private void DrawBeatMeasures(object sender, BeatDivisionEventArgs beatDivision)
        {
            float division = beatDivision.Division;
            //lets try just drawing a line on each beat rn
            float lastBeat = Utils.GetBeatFromMs(PlaybackHandler.Offset, PlaybackHandler.BaseBPM, PlaybackHandler.TotalSongMs);
            float beatDiv = 1 / division;

            int lineIndex = 0;

            for (float i = 0; i < lastBeat; i += beatDiv)
            {
                //draw line 

                if (lineIndex + 1 > BeatMeasures.Count)
                {
                    Line temp = new Line();
                    temp.Stroke = System.Windows.Media.Brushes.Gray;
                    temp.StrokeThickness = 2;
                    temp.Width = 2000;
                    temp.SizeChanged += ResizeLineWidth;
                    _waveCanvas.Children.Add(temp);
                    BeatMeasures.Add(temp);

                }
                Line line = BeatMeasures[lineIndex];
                double current = Utils.GetMsFromBeat(PlaybackHandler.Offset, PlaybackHandler.BaseBPM, i);
                line.Y1 = GetPointOnCanvFromMs(current);
                line.Y2 = GetPointOnCanvFromMs(current);
                line.Visibility = Visibility.Visible;
                lineIndex++;

            }
            Console.WriteLine("A");
            for (int i = lineIndex; i < BeatMeasures.Count; i++)
            {
                BeatMeasures[i].Visibility = Visibility.Collapsed;
            }

            Console.WriteLine();
        }

        private static void DrawWave(ISampleSource waveSource, Canvas waveCanvas)
        {
            const int WAVEMULT = 2;
            int channels = waveSource.WaveFormat.Channels;
            float[][] data = WaveformData.GetData(waveSource);
            int dataLen = data[0].Length / 2;
            Line myLine = new Line();
            Polygon polygon = new Polygon();
            polygon.Stroke = System.Windows.Media.Brushes.Green;
            polygon.Fill = System.Windows.Media.Brushes.DarkGreen;
            polygon.StrokeThickness = 4;
            waveCanvas.Width = CANVASWIDTH;
            double mid = CANVASWIDTH / 2;
            double multInc = mid * OUTREACH;

            waveCanvas.Height = dataLen;
            PointCollection polyPoints = new PointCollection();
            for (int i = 0; i < dataLen; i++)
            {

                double waveVal = (data[0][i] * multInc);
                Point point = new Point(waveVal + mid, i);
                polyPoints.Add(point);
            }
            for (int i = dataLen - 1; i > 0; i--)
            {
                double waveVal = (data[0][i] * multInc);

                Point point = new Point((mid) - (waveVal), i);
                polyPoints.Add(point);
            }
            polygon.Points = polyPoints;
            waveCanvas.Children.Add(polygon);
            
        }

        private void CreateTrackingLine(AudioTrack track, Canvas waveCanvas)
        {
            //add the line tracking 
            _trackingLine = new Line();
            // X doesn't need to set, since SizeChanged event listener is triggered when generating the window
            _trackingLine.Y1 = 20;
            _trackingLine.Y2 = 20;
            _trackingLine.SizeChanged += ResizeLineWidth;
            _trackingLine.Stroke = System.Windows.Media.Brushes.Red;
            _trackingLine.StrokeThickness = 2;
            waveCanvas.Children.Add(_trackingLine);
            //hook the line movement to the rendering update.
            CompositionTarget.Rendering += UpdateLine;


        }

        private double GetPointOnCanvFromMs(double ms)
        {
            return (ms / PlaybackHandler.TotalSongMs) * _waveCanvas.Height;
        }

        private void UpdateLine(object sender, EventArgs e)
        {
            //change the Y pos of the line to be accurate to the time in the song(hopefully this is perfectly accurate.)
            
            double point = PlaybackHandler.CurrentMs / PlaybackHandler.TotalSongMs;
            _trackingLine.Y1 = _waveCanvas.Height * point;
            _trackingLine.Y2 = _trackingLine.Y1;
        }

        private void ResizeLineWidth(object sender, RoutedEventArgs e)
        {
            Line myLine = (Line)sender;

            Double width = Double.IsNaN(myLine.Width) ? 2000 : myLine.Width;

            myLine.X1 = 0 - (width / 2);
            myLine.X2 = width / 2;
        }
    }
}
