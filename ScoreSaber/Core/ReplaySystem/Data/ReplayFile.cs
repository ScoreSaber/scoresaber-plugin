#region

using System;
using System.Collections.Generic;
using UnityEngine;

#endregion

#pragma warning disable IDE1006 // Naming Styles
namespace ScoreSaber.Core.ReplaySystem.Data {
    internal class ReplayFile {
        internal List<ComboEvent> comboKeyframes;
        internal List<EnergyEvent> energyKeyframes;
        internal List<HeightEvent> heightKeyframes;
        internal Metadata metadata;
        internal List<MultiplierEvent> multiplierKeyframes;
        internal List<NoteEvent> noteKeyframes;
        internal List<VRPoseGroup> poseKeyframes;
        internal List<ScoreEvent> scoreKeyframes;

        internal ReplayFile() {
            poseKeyframes = new List<VRPoseGroup>();
            heightKeyframes = new List<HeightEvent>();
            noteKeyframes = new List<NoteEvent>();
            scoreKeyframes = new List<ScoreEvent>();
            comboKeyframes = new List<ComboEvent>();
            multiplierKeyframes = new List<MultiplierEvent>();
            energyKeyframes = new List<EnergyEvent>();
        }
    }

    internal struct Metadata {
        internal string Version;
        internal string LevelID;
        internal int Difficulty;
        internal string Characteristic;
        internal string Environment;
        internal string[] Modifiers;
        internal float NoteSpawnOffset;
        internal bool LeftHanded;
        internal float InitialHeight;
        internal float RoomRotation;
        internal VRPosition RoomCenter;
        internal float FailTime;
    }

    internal struct ScoreEvent {
        public int Score;
        public float Time;
    }

    internal struct ComboEvent {
        internal int Combo;
        internal float Time;
    }

    internal struct NoteEvent {
        internal NoteID NoteID;
        internal NoteEventType EventType;
        internal VRPosition CutPoint;
        internal VRPosition CutNormal;
        internal VRPosition SaberDirection;
        internal int SaberType;
        internal bool DirectionOK;
        internal float SaberSpeed;
        internal float CutAngle;
        internal float CutDistanceToCenter;
        internal float CutDirectionDeviation;
        internal float BeforeCutRating;
        internal float AfterCutRating;
        internal float Time;
        internal float UnityTimescale;
        internal float TimeSyncTimescale;
    }

    internal enum NoteEventType {
        None,
        GoodCut,
        BadCut,
        Miss,
        Bomb
    }

    internal struct NoteID : IEquatable<NoteID> {
        internal float Time;
        internal int LineLayer;
        internal int LineIndex;
        internal int ColorType;
        internal int CutDirection;

        public static bool operator ==(NoteID a, NoteID b) {
            return Mathf.Approximately(a.Time, b.Time) && a.LineIndex == b.LineIndex && a.LineLayer == b.LineLayer &&
                   a.ColorType == b.ColorType && a.CutDirection == b.CutDirection;
        }

        public static bool operator !=(NoteID a, NoteID b) {
            return !(a == b);
        }

        public override int GetHashCode() {
            return Time.GetHashCode() ^ LineLayer ^ LineIndex;
        }

        public override bool Equals(object obj) {
            return Equals((NoteID)obj);
        }

        public bool Equals(NoteID other) {
            return this == other;
        }
    }

    internal struct EnergyEvent {
        internal float Energy;
        internal float Time;
    }

    internal struct HeightEvent {
        internal float Height;
        internal float Time;
    }

    internal struct MultiplierEvent {
        internal int Multiplier;
        internal float NextMultiplierProgress;
        internal float Time;
    }

    internal struct VRPoseGroup {
        internal VRPose Head;
        internal VRPose Left;
        internal VRPose Right;
        internal int FPS;
        internal float Time;
    }

    internal struct VRPose {
        internal VRPosition Position;
        internal VRRotation Rotation;
    }

    internal struct VRPosition {
        internal float X;
        internal float Y;
        internal float Z;

        internal static VRPosition None() {
            return new VRPosition { X = 0, Y = 0, Z = 0 };
        }
    }

    internal struct VRRotation {
        internal float X;
        internal float Y;
        internal float Z;
        internal float W;
    }
}