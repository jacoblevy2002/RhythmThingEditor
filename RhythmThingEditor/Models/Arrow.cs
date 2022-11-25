using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using System.Windows.Controls;
using RhythmThingEditor.RhythmSystem;

namespace RhythmThingEditor.Models
{
    class Arrow : UserEvents
    {
        
        private readonly collumn direction;
        

        public Arrow(int num, float time, JsonChart owner) : base(time, owner)
        {
            direction = (collumn)num;
            state = false;
            activeEvents.Add(this); //to ensure we know one has been placed at this beat already;
        }
        
        public override void SwitchState()
        {
            NoteInfo note = (NoteInfo)this.ToInfo();
            if(!state)
            {
                if(!json.notes.Contains(note))
                    json.AddNotes((NoteInfo)ToInfo());
            } else
            {
                json.RemoveNotes((NoteInfo)ToInfo());
            }
            state = !state;
        }


        public override object ToInfo() => new NoteInfo() { time = this.time, collumn = direction };
    }
}
