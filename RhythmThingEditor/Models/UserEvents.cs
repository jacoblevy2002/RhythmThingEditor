using System;
using System.Collections.Generic;
using System.Text;
using RhythmThingEditor.RhythmSystem;

namespace RhythmThingEditor.Models
{
    abstract class UserEvents
    {
        protected bool state;
        protected float time;
        protected JsonChart json;
        public static List<UserEvents> activeEvents = new List<UserEvents>();

        public UserEvents(float time, JsonChart saveTo)
        {
            this.time = time;
            json = saveTo;
        }

        public abstract void SwitchState();

        public abstract object ToInfo();
        public bool State { get => state; }
        public float Time { get => time; }
    }
}
