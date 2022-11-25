using CSCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace RhythmThingEditor.RhythmSystem
{
    public static class Utils
    {
        //create a beat value accurate to the games value system
        public static float GetCurrentBeat(float offset, float bpm, AudioTrack audioTrack)
        {
            //no accounting for bpm changes yet. Beast of a line from rhythmthing.
            //WHERE TF OFFSET HOLD ON

            double tempBeat = (((TimeConverterFactory.Instance.GetTimeConverterForSource(audioTrack.sampleSource).ToTimeSpan(audioTrack.sampleSource.WaveFormat, audioTrack.sampleSource.Position).TotalMilliseconds + (offset * 1000)) * ((float)(bpm) / 60000)));
            return (float)Math.Round(tempBeat, 2);

        }
        public static float GetBeatFromMs(float offset, float bpm, double ms)
        {
            double beat = (ms + (offset * 1000)) *((float)(bpm) / 60000);
            return (float)Math.Round(beat, 2);
        }
        public static double GetMsFromBeat(float offset, float bpm, float beat)
        {
            //aaa
            double ms = (beat / ((float)(bpm) / 60000)) - (offset * 1000);
            //AAA
            return ms;
        }
    }
}
