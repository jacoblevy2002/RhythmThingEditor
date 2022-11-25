using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using RhythmThingEditor.RhythmSystem;
using System.Windows.Shapes;
using RhythmThingEditor.Waveform;
using System.Windows;

namespace RhythmThingEditor.Models
{
    class PlaceableObject
    {
        private readonly int NUM_SPACES;
        private const int NUM_ARROWS = 4, NUM_EVENTS = 0, IMAGE_HEIGHT = 30, GRID_HEIGHT = 80;
        public static Dictionary<float, Grid> eventPlacements = new Dictionary<float, Grid>();
        public static Dictionary<float, Line> linePlacements = new Dictionary<float, Line>();
        public static List<PlaceableObject> placed = new List<PlaceableObject>();
        private Line newLine;
        private Canvas parent, waveform;
        private double canvasPos;
        private Grid outer, inner;
        private WaveformUIHelper waveformHelper;
        private float time;
        private JsonChart json;
        private const string IMAGES_LOCATION = "../Sprites/";
        private static string[] arrowImages = new string[] { "LeftArrow.bmp", "DownArrow.bmp", "UpArrow.bmp", "RightArrow.bmp" };
        private Image[] images;
        private UserEvents[] userEvents;

        private PlaceableObject(float songBeat, WaveformUIHelper waveHelper, Canvas wave, Canvas boundary, JsonChart myJSON)
        {
            NUM_SPACES = NUM_ARROWS + NUM_EVENTS; // atm pretty much irrelevant, but put in place to aid in future changes
            time = songBeat;
            waveformHelper = waveHelper;
            waveform = wave;
            parent = boundary;
            json = myJSON;
            canvasPos = waveformHelper.GetCanvasPosFromBeat(time);
            newLine = new Line()
            {
                Y1 = canvasPos,
                Y2 = canvasPos,
                Stroke = System.Windows.Media.Brushes.Blue,
                StrokeThickness = 2
            };
            newLine.SizeChanged += ResizeLineWidth;
            images = new Image[NUM_SPACES];
            userEvents = new UserEvents[NUM_SPACES];
            placed.Add(this);
            CreateArrowPlacement();
        }

        private void imgPlaceable_Click(object sender, MouseButtonEventArgs e) => SwitchState((Image)sender);

        private void SwitchState(Image toChange, int i = -1)
        {
            i = i >= 0 ? i : Array.IndexOf(images, toChange);
            UserEvents eventToChange = userEvents[i];
            changeSource(!eventToChange.State ? IMAGES_LOCATION + "Selected" + arrowImages[i] : IMAGES_LOCATION + arrowImages[i], toChange);
            eventToChange.SwitchState();
        }
        private void changeSource(string source, Image image) => image.Source = new BitmapImage(new Uri(source, UriKind.Relative));

        private void CreateArrowPlacement()
        {
            waveform.Children.Add(newLine);
            linePlacements.Add(time, newLine);

            outer = new Grid()
            {
                MinHeight = GRID_HEIGHT,
                Width = parent.Width
            };
            outer.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(outer.MinHeight - IMAGE_HEIGHT) });
            outer.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(IMAGE_HEIGHT) });
            inner = new Grid() { Height = IMAGE_HEIGHT };
            inner.SetValue(Grid.RowProperty, 1);
            TimeSpan displayTime = waveformHelper.PlaybackHandler.GetPositionFromPercent(waveformHelper.PlaybackHandler.GetPercentFromBeat(time));
            outer.Children.Add(new TextBlock() { Text = displayTime.ToString(), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center });
            outer.Children.Add(inner);
            for (int i = 0; i < NUM_ARROWS; i++)
            {
                inner.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                userEvents[i] = new Arrow(i, time, json);
                images[i] = new Image() { Height = inner.Height };
                images[i].SetValue(Grid.ColumnProperty, i);
                images[i].SetValue(Grid.RowProperty, 1);
                changeSource(IMAGES_LOCATION + arrowImages[i], images[i]);
                images[i].MouseLeftButtonDown += imgPlaceable_Click;
                inner.Children.Add(images[i]);
            }
            Canvas.SetTop(outer, canvasPos - outer.MinHeight / 2);
            parent.Children.Add(outer);
            eventPlacements.Add(time, outer);
        }

        private void ResizeLineWidth(object sender, RoutedEventArgs e)
        {
            Line myLine = (Line)sender;

            Double width = Double.IsNaN(myLine.Width) ? 2000 : myLine.Width;

            myLine.X1 = 0 - (width / 2);
            myLine.X2 = width / 2;
        }

        public void Switch(int column) => SwitchState(images[column], column);

        public static void ActAt(WaveformUIHelper waveHelper, Canvas waveform, Canvas boundary, JsonChart myJSON, float beat = -1, bool onlyAdd = false)
        {
            if (beat == -1)
            {
                double clickPercent = waveHelper.GetCanvasPosBeatSnapped(Mouse.GetPosition(waveform).Y) / waveform.Height;
                beat = waveHelper.PlaybackHandler.GetBeatPercentWSnap(clickPercent);
            }

            if (eventPlacements.ContainsKey(beat))
            {
                if (onlyAdd)
                    return;

                if (!Remove(boundary, beat, waveform))
                    MessageBox.Show("Unknown error removing the beat. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                PlaceableObject newGrid = new PlaceableObject(beat, waveHelper, waveform, boundary, myJSON);
            }
        }



        private static bool Remove(Canvas parent, float beat, Canvas waveform)
        {
            try
            {
                UserEvents.activeEvents.FindAll(x => x.Time == beat).ForEach((selectedEvent) =>
                {
                    selectedEvent.SwitchState();
                });
                parent.Children.Remove(eventPlacements[beat]);
                eventPlacements.Remove(beat);
                waveform.Children.Remove(linePlacements[beat]);
                linePlacements.Remove(beat);
                return true;
            } catch
            {
                return false;
            }
        }

        public float Time { get => time; }
    }
}
