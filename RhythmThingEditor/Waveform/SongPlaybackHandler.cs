using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using CSCore;
using CSCore.Codecs;
using CSCore.Streams;
using RhythmThingEditor.RhythmSystem;
using RhythmThingEditor.Waveform;
namespace RhythmThingEditor.Waveform
{

    //responsible for-wave form. line. playing and pausing song. getting time. getting time in beats.
    public class SongPlaybackHandler
    {


        private AudioTrack _audioTrack;
        private bool _playing;
        private bool _snapToBeat;
        private double _totalSongMs;
        public bool SnapToBeat { get {return _snapToBeat; } set {_snapToBeat = value;} }

        private JsonChart _jsonChart;

        private float beatDivision;
        //maybe need setters? if we wanna sync in editor. good for now
        public float BaseBPM { get { return _jsonChart.bpm; } }
        public float Offset { get { return _jsonChart.offset; } }
        public string SongName { get { return _jsonChart.songName; } }
        public string SongAuthor { get { return _jsonChart.songAuthor; } }
        public string ChartAuthor { get { return _jsonChart.chartAuthor; } }
        public int ChartDifficulty { get { return _jsonChart.difficulty; } }
        public bool Playing { get { return _playing; } }
        public AudioTrack MainAudioTrack { get { return _audioTrack; } }
        public double TotalSongMs { get { return _totalSongMs; } }

        public static event EventHandler<BeatDivisionEventArgs> BeatDivisionChanges;
        public static event EventHandler<PlaybackHandlerLoadArgs> PlaybackHandlerLoad;
        public float CurrentBeat 
        {
            get
            {
                //this will have to change later if we plan on implementing variable bpm
                return Utils.GetCurrentBeat(Offset, BaseBPM, _audioTrack);
            } 
        }
        public double CurrentMs
        {
            get
            {
                return _audioTrack.sampleSource.GetPosition().TotalMilliseconds;
            }
        }


        public SongPlaybackHandler(string chartInfoPath, JsonChart jsonChart)
        {

            _jsonChart = jsonChart;

            string songPath = chartInfoPath;
            songPath = System.IO.Path.Combine(System.IO.Directory.GetParent(chartInfoPath).ToString(), jsonChart.songPath);
            VolumeSource volumeSource;
            //make it ourselves first so we can generate the waveform from the ISampleSource.
            ISampleSource sampleSource = CodecFactory.Instance.GetCodec(songPath).ChangeSampleRate(AudioManager.sampleRate).ToStereo().ToSampleSource().AppendSource(x => new VolumeSource(x), out volumeSource);

            _audioTrack = new AudioTrack("mainTrack", sampleSource, volumeSource);
            _totalSongMs = _audioTrack.sampleSource.GetLength().TotalMilliseconds;

            PlaybackHandlerLoad(this, new PlaybackHandlerLoadArgs(_audioTrack));
            //DrawWave(sampleSource, waveCanvas);
            //instantiate the line
            //CreateTrackingLine(_audioTrack, waveCanvas);
            _audioTrack.sampleSource.Position = 0;
            

        }

        
        public void TogglePlayback()
        {
            if (_playing)
            {
                Pause();
            } else
            {
                Play();
            }
        }

        public void Pause()
        {
            if(_playing)
            {
                AudioManager.Instance.removeTrack(_audioTrack);
                _playing = false;
            } 
            //otherwise, dont do anything.
        }
        public void Play()
        {
            if(!_playing)
            {
                //if we're done, seek to the start
                if(_audioTrack.sampleSource.Position >= _audioTrack.sampleSource.Length)
                {
                    _audioTrack.sampleSource.Position = 0;
                }
                _playing = true;
                AudioManager.Instance.addTrack(_audioTrack);
            }
        }

        /// <summary>
        /// Sets the song progress to a spot from 0-1
        /// </summary>
        /// <param name="percent">spot from 0-1</param>
        public void SetPositionPercent(double percent)
        {
            if (percent > 1 || 0 > percent) throw new ArgumentOutOfRangeException("percent", "Value must be between 0 and 1 inclusive.");
            if(SnapToBeat)
            {
                percent = GetPercentBeatSnapped(percent);
            }
            _audioTrack.sampleSource.SetPosition(_audioTrack.sampleSource.GetLength() * percent);
            
        }

        public double GetPercentBeatSnapped(double percent)
        {
            if(SnapToBeat)
            {
                double msVal = _totalSongMs * percent;
                float curBeat = Utils.GetBeatFromMs(Offset, BaseBPM, msVal);
                //scuffed solution lol
                curBeat *= beatDivision;
                curBeat = MathF.Round(curBeat);
                curBeat /= beatDivision;
                msVal = Utils.GetMsFromBeat(Offset, BaseBPM, curBeat);
                return msVal / _totalSongMs;

            } else
            {
                return percent;
            }
        }
        public TimeSpan GetPositionFromPercent(double percent) => _audioTrack.sampleSource.GetLength() * percent;

        public float GetBeatPercentWSnap(double percent)
        {
            if(SnapToBeat)
            {
                percent = GetPercentBeatSnapped(percent);
            }
            double msVal = _totalSongMs * percent;
            float curBeat = Utils.GetBeatFromMs(Offset, BaseBPM, msVal);
            return curBeat;
        }
        public double GetPercentFromBeat(float beat)
        {
            double msBeat = Utils.GetMsFromBeat(Offset, BaseBPM, beat);
            double percent = msBeat / _totalSongMs;
            return percent;
        }
        public void ChangeDivision(string division)
        {
            //this is not safe!!!!
            //..buuuut we know what we're getting so we can let it happen :3
            float divi = float.Parse(division.Split("/")[1]);
            beatDivision = divi;
            BeatDivisionChanges(this, new BeatDivisionEventArgs(divi));
        }

        #region DECOUPLE


        #endregion
    }
    public class BeatDivisionEventArgs : EventArgs
    {
        public float Division { get; set; }
        public BeatDivisionEventArgs(float div)
        {
            Division = div;
        }
    }
    public class PlaybackHandlerLoadArgs : EventArgs
    {
        public AudioTrack AudioTrack { get; set; }
        public PlaybackHandlerLoadArgs(AudioTrack audioTrack)
        {
            AudioTrack = audioTrack;
        }
    }
}
