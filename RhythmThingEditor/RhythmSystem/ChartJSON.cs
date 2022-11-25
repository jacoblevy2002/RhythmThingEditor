using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using RhythmThingEditor.Models;

namespace RhythmThingEditor.RhythmSystem
{

    public struct JsonChart
    {
        private string sPath;
        private float beats;
        private float off;
        private string sName;
        private string sAuthor;
        private string cAuthor;
        private float teaser;
        private float teaserLength;
        private List<NoteInfo> noteArr;
        private List<EventInfo> eventArr;
        private int diff;
        //private videoInfo video;
        //private string script;

        public JsonChart(string path, float bpm, float offset)
        {
            noteArr = new List<NoteInfo>();
            eventArr = new List<EventInfo>();

            sPath = path;
            beats = bpm;
            off = offset;
            sName = "";
            sAuthor = "";
            cAuthor = "";
            teaser = 0.0f;
            teaserLength = 0.0f;
            diff = 0;
        }

        public void AddNotes(NoteInfo note)
        {
            
            noteArr.Add(note);
        }
        public void RemoveNotes(NoteInfo note) => noteArr.Remove(note);

        public bool GenerateFile(string path)
        {
            try
            {
                var settings = new Newtonsoft.Json.JsonSerializerSettings();
                settings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                File.WriteAllText(path, Newtonsoft.Json.JsonConvert.SerializeObject(this));
                
                return true;
            }
            catch { return false; }
        }

        // Properties use camelCase instead of PascalCase so that they are saved in the JSON correctly. Newtonsoft pulls direct Properties names
        public string songPath { get => sPath; set => sPath = value; }
        public float bpm { get => beats; set => beats = value >= 1 ? value : 1; }
        public float offset { get => off; set => off = value >= 0 ? value : 0; }
        public string songName { get => sName; set => sName = value; }
        public string songAuthor { get => sAuthor; set => sAuthor = value; }
        public string chartAuthor { get => cAuthor; set => cAuthor = value; }
        public float preview { get => teaser; set => teaser = value >= 0 ? value : 0; }
        public float previewLength { get => teaserLength; set => teaserLength = value >= 0 ? value : 0; }
        public int difficulty { get => diff; set => diff = value >= 0 && value <= 5 ? value : 0; }
        public List<NoteInfo> notes { get => noteArr; set => noteArr = value; }
        public List<EventInfo> events { get => eventArr; set => eventArr = value; }
    }

    public struct videoInfo
    {
        public int framerate;
        public string videoPath;
        public int frames;
        public int[] startPoint;

    }
    public enum collumn
    {
        Left,
        Down,
        Up,
        Right
    }
    public struct NoteInfo
    {
        public float time;
        public collumn collumn;
    }

    public struct EventInfo
    {
        //these will be dealt with accordingly depending on the event
        public float time;
        public string name;
        public string data;

    }
}
