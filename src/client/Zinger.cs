// LogicWorld.ClientCode.Singer
using System.Collections;
using System.Collections.Generic;
using CSharpSynth.Synthesis;
using JimmysUnityUtilities;
using LogicWorld.Audio;
using LogicWorld.ClientCode;
using LogicWorld.ClientCode.Decorations;
using LogicWorld.Interfaces;
using LogicWorld.References;
using LogicWorld.Rendering.Chunks;
using LogicWorld.Rendering.Components;
using LogicWorld.SharedCode;
using TMPro;
using UnityEngine;

namespace ZingerMod.Client {

    /// The Zinger is like a Singer but with a little more zing
    public class Zinger : ComponentClientCode<LogicWorld.ClientCode.Singer.IData>
    {
        protected static readonly HashSet<string> FORBIDDEN_INSTRUMENTS = new HashSet<string>(
            new string[] {
                // Definitely don't play this instrument under any circumstances
                "Tung.Applause"
            }
        );

        public const string DEFAULT_INSTRUMENT = "Tung.Waves.Sine";
        public const int DEFAULT_VOLUME = 75;
        // the bend pins are effectively the least significant note bits
        public const int BEND_PIN_COUNT = 2;
        public const int NOTE_PIN_COUNT = 6;
        public const byte DEFAULT_BASE_NOTE = 40;

        
        protected static Color24 InactiveColor = new Color24(40, 110, 88);
        protected static Color24 ActiveColor = new Color24(40, 160, 125);

        protected Synthesizer Synthesizer = new Synthesizer();

        protected TextMeshPro TextMesh;

        protected MusicComponentSoundPlayer SoundPlayer;


        private string _instrumentId = DEFAULT_INSTRUMENT;
        private bool _currentlyPlaying = false;
        private byte _prevNote = 0;
        private float _prevBend = 0f;

        protected string FormatNoteAndPitchBendText(byte note, float pitchBend) {
            return $"{(float)note + pitchBend:0.00}";
        }

        protected (byte, float) GetNoteAndPitchBend() {
            // skip the enable pin
            int startIndex = 1; 
            // endPin used as exclusive end index
            int endPin = startIndex + BEND_PIN_COUNT;

            int pitchBendQuarterSteps = 0;
            for(int i = startIndex; i < endPin; i++) {
                if (GetInputState(i)) {
                    pitchBendQuarterSteps |= 1 << (i-startIndex);
                }
            }

            // read the note pins,
            // starting after the last pitch bend pin
            startIndex += BEND_PIN_COUNT;
            endPin += NOTE_PIN_COUNT;

            int note = 0;
            for(int i = startIndex; i < endPin; i++) {
                if (GetInputState(i)) {
                    note |= 1 << (i-startIndex);
                }
            }

            // Data.Note is used as the base note
            return ((byte)(Data.Note + note), ((float)pitchBendQuarterSteps) * 0.25f);
        }

        protected override void FrameUpdate()
        {
            bool enabled = GetInputState(0);
            if (!enabled) {
                if (_currentlyPlaying) {
                    StopTheMusic();
                }
                return;
            }

            var (note, pitchBend) = GetNoteAndPitchBend();

            TextMesh.text = FormatNoteAndPitchBendText(note, pitchBend);

            if (!_currentlyPlaying) {
                StartTheMusic();
            }

            SetNote(note);
            SetPitchBend(pitchBend);
        }

        protected override void DataUpdate()
        {
            string desiredInstrumentId = Data.InstrumentTextID;
            if (_instrumentId != desiredInstrumentId) {
                // we sure would not want to hear any applause
                _instrumentId = Zinger.FORBIDDEN_INSTRUMENTS.Contains(desiredInstrumentId)
                    ? DEFAULT_INSTRUMENT
                    : desiredInstrumentId;
            }
            QueueFrameUpdate();
        }

        // returns true if the pitch bend actually changed
        private bool SetPitchBend(float pitchBend) {
            if (pitchBend != _prevBend) {
                Synthesizer.SetPitchBend(0, pitchBend);
                _prevBend = pitchBend;
                return true;
            }
            return false;
        }

        // returns true if the note actually changed
        private bool SetNote(byte note) {
            if (_prevNote != note) {
                Synthesizer.SoundOff(_prevNote, 0);
                Synthesizer.SoundOn(Data.InstrumentTextID, note, Data.Velocity, 0);
                _prevNote = note;
                return true;
            }
            return false;
        }

        private void StartTheMusic()
        {
            SoundPlayer.EnableAudio();
            _currentlyPlaying = true;
            SetBlockColor(Zinger.ActiveColor);
        }

        private void StopTheMusic()
        {
            TextMesh.text = "OFF";
            SoundPlayer.DisableAudio();
            Synthesizer.SetPitchBend(0, 0);
            Synthesizer.SoundOffAll();
            _currentlyPlaying = false;
            _prevNote = 0;
            _prevBend = 0;
            SetBlockColor(Zinger.InactiveColor);
        }

        protected override IList<IDecoration> GenerateDecorations()
        {
            GameObject gameObject = Object.Instantiate(Prefabs.ComponentDecorations.MusicComponentSoundPlayer);
            SoundPlayer = gameObject.GetComponent<MusicComponentSoundPlayer>();
            SoundPlayer.SetSynth(Synthesizer);
            GameObject gameObject2 = Object.Instantiate(Prefabs.ComponentDecorations.SingerNoteLabel);
            TextMesh = gameObject2.GetComponent<TextMeshPro>();
            TextMesh.text = "...";
            return new Decoration[]
            {
                new Decoration
                {
                    LocalPosition = new Vector3(0f, 0.8f, 0f),
                    DecorationObject = gameObject
                },
                new Decoration
                {
                    LocalPosition = new Vector3(0f, 0.25f, 0.175f),
                    LocalRotation = Quaternion.Euler(0f, 180f, 0f),
                    DecorationObject = gameObject2
                }
            };
        }

        protected override void SetDataDefaultValues()
        {
            Data.InstrumentTextID = DEFAULT_INSTRUMENT;
            Data.Velocity = DEFAULT_VOLUME;
            Data.Note = DEFAULT_BASE_NOTE;
        }
    }
}
