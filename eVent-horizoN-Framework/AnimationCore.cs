using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using System.Runtime.Serialization;

namespace VNFramework
{
    /// <summary>
    /// Stores colour channel information as floating point data. Floating point equivalent of Microsoft.XNA.Framework.Color.
    /// </summary>
    [Serializable]
    public class ColourShift
    {
        public float R { get; set; }
        public float G { get; set; }
        public float B { get; set; }
        public float A { get; set; }
        public ColourShift(float iR, float iG, float iB, float iA)
        {
            R = iR;
            G = iG;
            B = iB;
            A = iA;
        }
        public static ColourShift operator +(ColourShift A, ColourShift B)
        {
            return new ColourShift(A.R + B.R, A.G + B.G, A.B + B.B, A.A + B.A);
        }
        public static ColourShift operator *(ColourShift A, float B)
        {
            return new ColourShift(A.R * B, A.G * B, A.B * B, A.A * B);
        }
        public static ColourShift Constrain(ColourShift Base)
        {
            float[] Vals = new float[] { Base.R, Base.G, Base.B, Base.A };
            for (int i = 0; i < 4; i++)
            {
                if (Vals[i] < 0f) { Vals[i] = 0f; }
                if (Vals[i] > 255f) { Vals[i] = 255f; }
            }
            return new ColourShift(Vals[0], Vals[1], Vals[2], Vals[3]);
        }
    }
    /// <summary>
    /// An animation object that contains information for frame-by-frame animations in multiple domains that can be played on a given WorldEntity object.
    /// </summary>
    [Serializable]
    public partial class Animation
    {
        private static ArrayList AnimationRegInternal = new ArrayList();
        public static void GlobalEndLoops()
        {
            foreach (Animation A in AnimationRegInternal) { A.Loop = false; }
        }
        public static void GlobalManualTrigger()
        {
            foreach (Animation A in AnimationRegInternal) { A.AutoTrigger = true; }
        }
        public static void GlobalManualTrigger(String Name)
        {
            foreach (Animation A in AnimationRegInternal) { if (A.AnimName == Name) { A.AutoTrigger = true; } }
        }
        public Boolean PlacedInPauseState { get; set; }
        private Boolean pTrigger = true;
        public Boolean AutoTrigger
        {
            get { return pTrigger; }
            set { pTrigger = value; }
        }
        public String AnimName { get; set; }
        public Boolean Loop { get; set; }
        Boolean pSpent = false;
        public int TimeElapsed { get { return StartTime > 0 ? Environment.TickCount - StartTime : 0; } }
        private int HungTime = -1;
        public void TimeHang()
        {
            HungTime = TimeElapsed;
        }
        public void UnHang()
        {
            if (HungTime != -1)
            {
                StartTime = Environment.TickCount - HungTime;
                HungTime = -1;
            }
        }
        public Boolean Spent
        {
            get { return pSpent; }
        }
        Boolean pStarted = false;
        public Boolean Started
        {
            get { return pStarted; }
        }
        Boolean pMove = false;
        Boolean pScale = false;
        Boolean pRotate = false;
        Boolean pColour = false;
        Boolean pFrames = false;
        public Boolean Move { get { return pMove; } }
        public Boolean Scale { get { return pScale; } }
        public Boolean Rotate { get { return pRotate; } }
        public Boolean Colour { get { return pColour; } }
        public Boolean Frames { get { return Frames; } }
        public void ReRegisterSelf()
        {
            if(!AnimationRegInternal.Contains(this)) { AnimationRegInternal.Add(this); }
        }
        public Animation(String Name)
        {
            AnimationRegInternal.Add(this);
            AnimName = Name;
            Loop = false;
            pTrigger = true;
        }
        ~Animation()
        {
            if (AnimationRegInternal.Contains(this)) { AnimationRegInternal.Remove(this); }
        }
        SortedList lastMovementFrames = new SortedList();
        SortedList lastRotationFrames = new SortedList();
        SortedList lastScaleFrames = new SortedList();
        SortedList lastColourFrames = new SortedList();
        public void WriteMovement(SortedList AnimationFrames)
        {
            MovementFrames = AnimationFrames;
            lastMovementFrames = (SortedList)MovementFrames.Clone();
            pMove = true;
        }
        public void WriteRotation(SortedList AnimationFrames)
        {
            RotationFrames = AnimationFrames;
            lastRotationFrames = (SortedList)RotationFrames.Clone();
            pRotate = true;
        }
        public void WriteScaling(SortedList AnimationFrames)
        {
            ScaleFrames = AnimationFrames;
            lastScaleFrames = (SortedList)ScaleFrames.Clone();
            pScale = true;
        }
        public void WriteColouring(SortedList AnimationFrames)
        {
            ColourFrames = AnimationFrames;
            lastColourFrames = (SortedList)ColourFrames.Clone();
            pColour = true;
        }
        public void WriteFrames(SortedList AnimationFrames)
        {
            FrameFrames = AnimationFrames;
            pFrames = true;
        }
        SortedList MovementFrames = new SortedList();
        SortedList RotationFrames = new SortedList();
        SortedList ScaleFrames = new SortedList();
        SortedList ColourFrames = new SortedList();
        SortedList FrameFrames = new SortedList();
        public void AutoWipe()
        {
            pSpent = true;
            MovementFrames = new SortedList();
            RotationFrames = new SortedList();
            ScaleFrames = new SortedList();
            ColourFrames = new SortedList();
            FrameFrames = new SortedList();
            if (AnimationRegInternal.Contains(this)) { AnimationRegInternal.Remove(this); }
        }
        public void AutoInvertScaling(Boolean InvertX, Boolean InvertY)
        {
            int[] MyScaleKeys = new int[ScaleFrames.Keys.Count];
            ScaleFrames.Keys.CopyTo(MyScaleKeys, 0);
            foreach (int i in MyScaleKeys)
            {
                if (InvertX) { ScaleFrames[i] = new Vector2(-((Vector2)ScaleFrames[i]).X, ((Vector2)ScaleFrames[i]).Y); }
                if (InvertY) { ScaleFrames[i] = new Vector2(((Vector2)ScaleFrames[i]).X, -((Vector2)ScaleFrames[i]).Y); }
            }
            lastScaleFrames = (SortedList)ScaleFrames.Clone();
        }
        protected Boolean cSpent = false;
        object GetItemAt(int Time, SortedList Frames)
        {
            Vector2 OutVector = new Vector2();
            float OutFloat = 0f;
            ColourShift OutColour = new ColourShift(0, 0, 0, 0);
            int OutMode = -1;
            if (Frames.Count > 0) { cSpent = false; }
            int[] K = new int[Frames.Count];
            Frames.Keys.CopyTo(K, 0);
            foreach (int T in K)
            {
                if (T <= Time)
                {
                    if (Frames[T] is Vector2)
                    {
                        OutVector += (Vector2)Frames[T];
                        OutMode = 0;
                    }
                    else if (Frames[T] is float)
                    {
                        OutFloat += (float)Frames[T];
                        OutMode = 1;
                    }
                    else if (Frames[T] is ColourShift)
                    {
                        OutColour += (ColourShift)Frames[T];
                        OutMode = 2;
                    }
                    Frames.Remove(T);
                }
            }
            object Out = null;
            switch (OutMode)
            {
                case 0:
                    Out = OutVector;
                    break;
                case 1:
                    Out = OutFloat;
                    break;
                case 2:
                    Out = OutColour;
                    break;
            }
            return Out;
        }
        int StartTime = 0;
        public Vector2 GetVector(SortedList List)
        {
            return GetVector(List, -1);
        }
        public Vector2 GetVector(SortedList List, int SetTime)
        {
            if (!pStarted)
            {
                pStarted = true;
                StartTime = Environment.TickCount;
            }
            object O;
            if (SetTime == -1) { O = GetItemAt(Environment.TickCount - StartTime, List); }
            else { O = GetItemAt(SetTime, List); }
            if (O != null) { return (Vector2)O; }
            return new Vector2();
        }
        public float GetRadians(SortedList List)
        {
            return GetRadians(List, -1);
        }
        public float GetRadians(SortedList List, int SetTime)
        {
            if (!pStarted)
            {
                pStarted = true;
                StartTime = Environment.TickCount;
            }
            object O;
            if (SetTime == -1) { O = GetItemAt(Environment.TickCount - StartTime, List); }
            else { O = GetItemAt(SetTime, List); }
            if (O != null) { return (float)O; }
            return new float();
        }
        public ColourShift GetColour(SortedList List)
        {
            return GetColour(List, -1);
        }
        public ColourShift GetColour(SortedList List, int SetTime)
        {
            if (!pStarted)
            {
                pStarted = true;
                StartTime = Environment.TickCount;
            }
            object O;
            if (SetTime == -1) { O = GetItemAt(Environment.TickCount - StartTime, List); }
            else { O = GetItemAt(SetTime, List); }
            if (O != null) { return (ColourShift)O; }
            return new ColourShift(0, 0, 0, 0);
        }
        public Point GetFrame(SortedList List)
        {
            return GetFrame(List, -1);
        }
        public Point GetFrame(SortedList List, int SetTime)
        {
            if (!pStarted)
            {
                pStarted = true;
                StartTime = Environment.TickCount;
            }
            if (SetTime == -1) { SetTime = Environment.TickCount - StartTime; }
            int[] K = new int[List.Count];
            List.Keys.CopyTo(K, 0);
            for (int i = K.Length - 1; i >= 0; i--)
            {
                if (K[i] <= SetTime)
                {
                    if (i < K.Length - 1) { cSpent = false; }
                    return (Point)List[K[i]];
                }
            }
            return new Point(-1, -1);
        }
        /// <summary>
        /// Creates a simple copy of this animation's frames and properties.
        /// </summary>
        /// <returns></returns>
        public Animation Clone()
        {
            Animation A = new Animation(AnimName);
            A.Loop = Loop;
            A.AutoTrigger = pTrigger;
            if (pMove) { A.WriteMovement((SortedList)lastMovementFrames.Clone()); }
            if (pScale) { A.WriteScaling((SortedList)lastScaleFrames.Clone()); }
            if (pColour) { A.WriteColouring((SortedList)lastColourFrames.Clone()); }
            if (pRotate) { A.WriteRotation((SortedList)lastRotationFrames.Clone()); }
            if (pFrames) { A.WriteFrames((SortedList)FrameFrames.Clone()); }
            return A;
        }
        /// <summary>
        /// Returns a deep copy of the Animation object's instance states.
        /// </summary>
        /// <returns>The cloned Animation object.</returns>
        public Animation Copy()
        {
            object[] Parameters = new object[] { AnimName, pSpent, pStarted, StartTime, PlacedInPauseState, HungTime, Loop, pMove, pScale, pRotate, pColour, pFrames, MovementFrames, lastMovementFrames, ScaleFrames, lastScaleFrames, RotationFrames, lastRotationFrames, ColourFrames, lastColourFrames, FrameFrames, pTrigger };
            Animation A = new Animation(Parameters);
            return A;
        }
        public Animation(object[] CopiedParamters)
        {
            AnimationRegInternal.Add(this);
            AnimName = (String)CopiedParamters[0];
            pSpent = (Boolean)CopiedParamters[1];
            pStarted = (Boolean)CopiedParamters[2];
            StartTime = (int)CopiedParamters[3];
            PlacedInPauseState = (Boolean)CopiedParamters[4];
            HungTime = (int)CopiedParamters[5];
            Loop = (Boolean)CopiedParamters[6];
            pMove = (Boolean)CopiedParamters[7];
            pScale = (Boolean)CopiedParamters[8];
            pRotate = (Boolean)CopiedParamters[9];
            pColour = (Boolean)CopiedParamters[10];
            pFrames = (Boolean)CopiedParamters[11];
            MovementFrames = (SortedList)((SortedList)CopiedParamters[12]).Clone();
            lastMovementFrames = (SortedList)((SortedList)CopiedParamters[13]).Clone();
            ScaleFrames = (SortedList)((SortedList)CopiedParamters[14]).Clone();
            lastScaleFrames = (SortedList)((SortedList)CopiedParamters[15]).Clone();
            RotationFrames = (SortedList)((SortedList)CopiedParamters[16]).Clone();
            lastRotationFrames = (SortedList)((SortedList)CopiedParamters[17]).Clone();
            ColourFrames = (SortedList)((SortedList)CopiedParamters[18]).Clone();
            lastColourFrames = (SortedList)((SortedList)CopiedParamters[19]).Clone();
            FrameFrames = (SortedList)((SortedList)CopiedParamters[20]).Clone();
            pTrigger = (Boolean)CopiedParamters[21];
        }
        /// <summary>
        /// Jumps an animated WorldEntity to the end of this Animation's cycle.
        /// </summary>
        /// <param name="Operand">The WorldEntity being animated by this operation.</param>
        public void Jump(WorldEntity Operand)
        {
            if (!pTrigger) { return; }
            int MaxTime = 0;
            foreach (int T in lastMovementFrames.Keys)
            {
                if (T > MaxTime) { MaxTime = T; }
            }
            foreach (int T in lastScaleFrames.Keys)
            {
                if (T > MaxTime) { MaxTime = T; }
            }
            foreach (int T in lastRotationFrames.Keys)
            {
                if (T > MaxTime) { MaxTime = T; }
            }
            foreach (int T in lastColourFrames.Keys)
            {
                if (T > MaxTime) { MaxTime = T; }
            }
            foreach (int T in FrameFrames.Keys)
            {
                if (T > MaxTime) { MaxTime = T; }
            }
            MaxTime++;
            if (pMove) { Operand.Move(GetVector(MovementFrames, MaxTime)); }
            if (pRotate) { Operand.Rotate(GetRadians(RotationFrames, MaxTime)); }
            if (pScale) { Operand.Scale(GetVector(ScaleFrames, MaxTime)); }
            if (pColour) { Operand.Colour(GetColour(ColourFrames, MaxTime)); }
            if (pFrames)
            {
                Point ACo = GetFrame(FrameFrames);
                if (ACo != new Point(-1, -1)) { Operand.SetAtlasFrame(ACo); }
            }
            if (!Loop)
            {
                pSpent = true;
                if (AnimationRegInternal.Contains(this)) { AnimationRegInternal.Remove(this); }
            }
            else
            {
                pStarted = false;
                MovementFrames = (SortedList)lastMovementFrames.Clone();
                RotationFrames = (SortedList)lastRotationFrames.Clone();
                ScaleFrames = (SortedList)lastScaleFrames.Clone();
                ColourFrames = (SortedList)lastColourFrames.Clone();
            }
            StartTime = Environment.TickCount;
        }
        /// <summary>
        /// Advances the animated WorldEntity through its animation cycle based on the environment time elapsed, per the stored animation frames.
        /// </summary>
        /// <param name="Operand">The WorldEntity being animated by this operation.</param>
        public void Step(WorldEntity Operand)
        {
            if (!pTrigger) { return; }
            cSpent = true;
            if (pMove) { Operand.Move(GetVector(MovementFrames)); }
            if (pRotate) { Operand.Rotate(GetRadians(RotationFrames)); }
            if (pScale) { Operand.Scale(GetVector(ScaleFrames)); }
            if (pColour) { Operand.Colour(GetColour(ColourFrames)); }
            if (pFrames)
            {
                Point ACo = GetFrame(FrameFrames);
                if (ACo != new Point(-1, -1)) { Operand.SetAtlasFrame(ACo); }
            }
            if (cSpent)
            {
                if (!Loop)
                {
                    pSpent = true;
                    if (AnimationRegInternal.Contains(this)) { AnimationRegInternal.Remove(this); }
                }
                else
                {
                    pStarted = false;
                    MovementFrames = (SortedList)lastMovementFrames.Clone();
                    RotationFrames = (SortedList)lastRotationFrames.Clone();
                    ScaleFrames = (SortedList)lastScaleFrames.Clone();
                    ColourFrames = (SortedList)lastColourFrames.Clone();
                }
            }
        }
        static public SortedList CreateVectorTween(Vector2 Shift, int Time, int FrameLength)
        {
            if (FrameLength <= 0) { FrameLength = 1; }
            if (Time < 0) { Time = 0; }
            SortedList Construct = new SortedList();
            if (Time == 0) { Construct.Add(0, Shift); }
            else
            {
                Construct.Add(0, new Vector2());
                for (int i = FrameLength; i <= Time; i += FrameLength)
                {
                    Construct.Add(i, Shift / ((float)Time / (float)FrameLength));
                }
            }
            return Construct;
        }
        static public SortedList CreateFloatTween(float Shift, int Time, int FrameLength)
        {
            if (FrameLength <= 0) { FrameLength = 1; }
            if (Time < 0) { Time = 0; }
            SortedList Construct = new SortedList();
            if (Time == 0) { Construct.Add(0, Shift); }
            else
            {
                Construct.Add(0, 0f);
                for (int i = FrameLength; i <= Time; i += FrameLength)
                {
                    Construct.Add(i, Shift / ((float)Time / (float)FrameLength));
                }
            }
            return Construct;
        }
        static public SortedList CreateColourTween(ColourShift Shift, int Time, int FrameLength)
        {
            if (FrameLength <= 0) { FrameLength = 1; }
            if (Time < 0) { Time = 0; }
            SortedList Construct = new SortedList();
            if (Time == 0) { Construct.Add(0, Shift); }
            else
            {
                Construct.Add(0, new ColourShift(0, 0, 0, 0));
                for (int i = FrameLength; i <= Time; i += FrameLength)
                {
                    Construct.Add(i, new ColourShift(Shift.R / ((float)Time / (float)FrameLength), Shift.G / ((float)Time / (float)FrameLength), Shift.B / ((float)Time / (float)FrameLength), Shift.A / ((float)Time / (float)FrameLength)));
                }
            }
            return Construct;
        }
        static public SortedList MergeFrames(SortedList A, SortedList B)
        {
            SortedList Construct = new SortedList();
            int LFrame = 0;
            foreach (int K in A.Keys)
            {
                Construct.Add(K, A[K]);
                LFrame = K;
            }
            foreach (int K in B.Keys)
            {
                if (Construct.ContainsKey(K + LFrame)) { continue; }
                Construct.Add(K + LFrame, B[K]);
            }
            return Construct;
        }
    }
}
