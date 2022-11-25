using CSCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace RhythmThingEditor.Waveform
{

    public static class WaveformData
    {
        public const int NUMBEROFPOINTS = 20000;

        public static long Length;
        //NOTE! A LARGE PART OF THIS CLASS WAS TAKEN FROM THE CSCORE SAMPLES, DESPITE BEING MODIFIED AFTER. I CAN NOT TAKE CREDIT FOR THE LOGIC USED TO OBTAIN THE DATA 
        public static float[][] GetData(ISampleSource sampleSource)
        {

           
                int channels = sampleSource.WaveFormat.Channels;
                int blockSize = (int)(sampleSource.Length / channels / NUMBEROFPOINTS);
                WaveformDataChannel[] waveformDataChannels = new WaveformDataChannel[channels];
                for (var i = 0; i < channels; i++)
                {
                    waveformDataChannels[i] = new WaveformDataChannel(blockSize);
                }

                float[] buffer = new float[sampleSource.WaveFormat.BlockAlign * 5];
                int sampleCount = 0;

                var flag = true;
                while (flag)
                {
                    int samplesToRead = buffer.Length;
                    int read = sampleSource.Read(buffer, 0, samplesToRead);
                    for (int i = 0; i < read; i += channels)
                    {
                        for (int n = 0; n < channels; n++)
                        {
                            waveformDataChannels[n].AddSample(buffer[i + n]);
                            sampleCount++;
                        }
                    }

                    if (read == 0)
                        flag = false;
                }

                foreach (WaveformDataChannel waveformDataChannel in waveformDataChannels)
                {
                    waveformDataChannel.Finish();
                }

                Length = sampleCount;


                return waveformDataChannels.Select(x => x.GetData()).ToArray();

        }


        private class WaveformDataChannel
        {
            private readonly int _blockSize;
            private readonly List<float> _maxData = new List<float>();

            private readonly List<float> _minData = new List<float>();
            private readonly SampleAnalyzer _sampleAnalyzer = new SampleAnalyzer();

            public WaveformDataChannel(int blockSize)
            {
                _blockSize = blockSize;
            }

            public void AddSample(float sample)
            {
                _sampleAnalyzer.AddSample(sample);
                if (_sampleAnalyzer.Counter >= _blockSize)
                {
                    _minData.Add(_sampleAnalyzer.AvgMin);
                    _maxData.Add(_sampleAnalyzer.AvgMax);

                    _sampleAnalyzer.Reset();
                }
            }

            public void Finish()
            {
                _minData.Add(_sampleAnalyzer.AvgMin);
                _minData.Add(_sampleAnalyzer.AvgMax);

                _sampleAnalyzer.Reset();
            }

            public float[] GetData()
            {
                _maxData.AddRange(_minData);
                float[] data = _maxData.ToArray();

                Single z = 1 / data.Average(x => Math.Abs(x));
                z /= 2;
                for (int i = 0; i < data.Length; i++)
                {
                    data[i] *= z;
                    data[i] = Math.Min(1.5f, Math.Max(-1.5f, data[i]));
                }
                return data;
            }

            private class SampleAnalyzer
            {
                private readonly List<float> _neg = new List<float>();
                private readonly List<float> _pos = new List<float>();

                public float Min { get; private set; }

                public float Max { get; private set; }

                public float AvgMin
                {
                    get { return _neg.Any() ? _neg.Average() : 0; }
                }

                public float AvgMax
                {
                    get { return _pos.Any() ? _pos.Average() : 0; }
                }

                public int Counter { get; private set; }

                public void AddSample(float sample)
                {
                    Min = Math.Min(Min, sample);
                    Max = Math.Max(Max, sample);

                    if (sample < 0)
                        _neg.Add(sample);
                    else if (sample > 0)
                        _pos.Add(sample);

                    Counter++;
                }

                public void Reset()
                {
                    Counter = 0;
                    Min = 0;
                    Max = 0;
                    _neg.Clear();
                    _pos.Clear();
                }
            }
        }
    }

}
