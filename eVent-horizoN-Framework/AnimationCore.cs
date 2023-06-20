using Microsoft.Xna.Framework;
using Nvidia.TextureTools;
using SharpDX;
using System;
using System.Collections;
using System.Collections.Generic;

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
        public static ColourShift operator +(ColourShift a, ColourShift b)
        {
            return new ColourShift(a.R + b.R, a.G + b.G, a.B + b.B, a.A + b.A);
        }
        public static ColourShift operator *(ColourShift a, float b)
        {
            return new ColourShift(a.R * b, a.G * b, a.B * b, a.A * b);
        }
        public static ColourShift Constrain(ColourShift @base)
        {
            float[] vals = new float[] { @base.R, @base.G, @base.B, @base.A };
            for (int i = 0; i < 4; i++)
            {
                if (vals[i] < 0f) { vals[i] = 0f; }
                if (vals[i] > 255f) { vals[i] = 255f; }
            }
            return new ColourShift(vals[0], vals[1], vals[2], vals[3]);
        }
        public Microsoft.Xna.Framework.Color AsColour()
        {
            return new Microsoft.Xna.Framework.Color(R, G, B, A);
        }
    }
    /// <summary>
    /// An animation object that contains information for frame-by-frame animations in multiple domains that can be played on a given WorldEntity object.
    /// </summary>
    [Serializable]
    public partial class Animation
    {
        private static List<Animation> _animationRegInternal = new List<Animation>();
        public static void GlobalEndLoops()
        {
            foreach (Animation anim in _animationRegInternal) { anim.Loop = false; }
        }
        public static void GlobalManualTrigger()
        {
            foreach (Animation anim in _animationRegInternal) { anim.AutoTrigger = true; }
        }
        public static void GlobalManualTrigger(String Name)
        {
            foreach (Animation anim in _animationRegInternal) { if (anim.AnimName == Name) { anim.AutoTrigger = true; } }
        }
        public Boolean PlacedInPauseState { get; set; }
        private Boolean _trigger = true;
        public Boolean AutoTrigger
        {
            get { return _trigger; }
            set { _trigger = value; }
        }
        public String AnimName { get; set; }
        public Boolean Loop { get; set; }
        Boolean _spent = false;
        public int TimeElapsed { get { return StartTime > 0 ? Environment.TickCount - StartTime : 0; } }
        private int _hungTime = -1;
        public void TimeHang()
        {
            _hungTime = TimeElapsed;
        }
        public void UnHang()
        {
            if (_hungTime != -1)
            {
                StartTime = Environment.TickCount - _hungTime;
                _hungTime = -1;
            }
        }
        public Boolean Spent
        {
            get { return _spent; }
        }
        Boolean _started = false;
        public Boolean Started
        {
            get { return _started; }
        }
        Boolean _move = false;
        Boolean _scale = false;
        Boolean _rotate = false;
        Boolean _colour = false;
        Boolean _frames = false;
        public Boolean Move { get { return _move; } }
        public Boolean Scale { get { return _scale; } }
        public Boolean Rotate { get { return _rotate; } }
        public Boolean Colour { get { return _colour; } }
        public Boolean Frames { get { return Frames; } }
        public void ReRegisterSelf()
        {
            if(!_animationRegInternal.Contains(this)) { _animationRegInternal.Add(this); }
        }
        public Animation(String name)
        {
            _animationRegInternal.Add(this);
            AnimName = name;
            Loop = false;
            _trigger = true;
        }
        ~Animation()
        {
            if (_animationRegInternal.Contains(this)) { _animationRegInternal.Remove(this); }
        }
        SortedList<int,Vector2> lastMovementFrames = new SortedList<int, Vector2>();
        SortedList<int,float> lastRotationFrames = new SortedList<int, float>();
        SortedList<int, Vector2> lastScaleFrames = new SortedList<int, Vector2>();
        SortedList<int, ColourShift> lastColourFrames = new SortedList<int, ColourShift>();
        public void WriteMovement(SortedList<int, Vector2> animationFrames)
        {
            MovementFrames = animationFrames;
            lastMovementFrames = new SortedList<int, Vector2>(MovementFrames);
            _move = true;
        }
        public void WriteRotation(SortedList<int, float> animationFrames)
        {
            RotationFrames = animationFrames;
            lastRotationFrames = new SortedList<int, float>(RotationFrames);
            _rotate = true;
        }
        public void WriteScaling(SortedList<int, Vector2> animationFrames)
        {
            ScaleFrames = animationFrames;
            lastScaleFrames = new SortedList<int, Vector2>(ScaleFrames);
            _scale = true;
        }
        public void WriteColouring(SortedList<int, ColourShift> animationFrames)
        {
            ColourFrames = animationFrames;
            lastColourFrames = new SortedList<int, ColourShift>(ColourFrames);
            _colour = true;
        }
        public void WriteFrames(SortedList<int, Point> animationFrames)
        {
            FrameFrames = animationFrames;
            _frames = true;
        }
        SortedList<int, Vector2> MovementFrames = new SortedList<int, Vector2>();
        SortedList<int, float> RotationFrames = new SortedList<int, float>();
        SortedList<int, Vector2> ScaleFrames = new SortedList<int, Vector2>();
        SortedList<int, ColourShift> ColourFrames = new SortedList<int, ColourShift>();
        SortedList<int, Point> FrameFrames = new SortedList<int, Point>();
        public void AutoWipe()
        {
            _spent = true;
            MovementFrames = new SortedList<int, Vector2>();
            RotationFrames = new SortedList<int, float>();
            ScaleFrames = new SortedList<int, Vector2>();
            ColourFrames = new SortedList<int, ColourShift>();
            FrameFrames = new SortedList<int, Point>();
            if (_animationRegInternal.Contains(this)) { _animationRegInternal.Remove(this); }
        }
        public void AutoInvertScaling(Boolean invertX, Boolean invertY)
        {
            int[] mScaleKeys = new int[ScaleFrames.Keys.Count];
            ScaleFrames.Keys.CopyTo(mScaleKeys, 0);
            foreach (int i in mScaleKeys)
            {
                if (invertX) { ScaleFrames[i] = new Vector2(-((Vector2)ScaleFrames[i]).X, ((Vector2)ScaleFrames[i]).Y); }
                if (invertY) { ScaleFrames[i] = new Vector2(((Vector2)ScaleFrames[i]).X, -((Vector2)ScaleFrames[i]).Y); }
            }
            lastScaleFrames = new SortedList<int, Vector2>(ScaleFrames);
        }
        private Boolean _cSpent = false;
        object GetItemAt<T>(int time, SortedList<int, T> frames) where T : notnull
        {
            T output = default(T);
            if (output is null)
            {
                if (typeof(T) == typeof(Vector2))
                {
                    output = (T)(object)new Vector2();
                }
                else if (typeof(T) == typeof(ColourShift))
                {
                    output = (T)(object)new ColourShift(0, 0, 0, 0);
                }
            }
            if (frames.Count > 0) { _cSpent = false; }
            int[] keyR = new int[frames.Count];
            frames.Keys.CopyTo(keyR, 0);
            foreach (int t in keyR)
            {
                if (t <= time)
                {
                    if (typeof(T) == typeof(Vector2))
                    {
                        output = (T)(object)((Vector2)(object)output + (Vector2)(object)frames[t]);
                    }
                    else if(typeof(T) == typeof(float))
                    {
                        output = (T)(object)((float)(object)output + (float)(object)frames[t]);
                    }
                    else if(typeof(T) == typeof(ColourShift))
                    {
                        output = (T)(object)((ColourShift)(object)output + (ColourShift)(object)frames[t]);
                    }
                    frames.Remove(t);
                }
            }
            return output;
        }
        int StartTime = 0;
        public Vector2 GetVector(SortedList<int, Vector2> list)
        {
            return GetVector(list, -1);
        }
        public Vector2 GetVector(SortedList<int, Vector2> list, int setTime)
        {
            if (!_started)
            {
                _started = true;
                StartTime = Environment.TickCount;
            }
            object obj;
            if (setTime == -1) { obj = GetItemAt<Vector2>(Environment.TickCount - StartTime, list); }
            else { obj = GetItemAt<Vector2>(setTime, list); }
            if (obj != null) { return (Vector2)obj; }
            return new Vector2();
        }
        public float GetRadians(SortedList<int, float> list)
        {
            return GetRadians(list, -1);
        }
        public float GetRadians(SortedList<int, float> list, int setTime)
        {
            if (!_started)
            {
                _started = true;
                StartTime = Environment.TickCount;
            }
            object obj;
            if (setTime == -1) { obj = GetItemAt<float>(Environment.TickCount - StartTime, list); }
            else { obj = GetItemAt<float>(setTime, list); }
            if (obj != null) { return (float)obj; }
            return new float();
        }
        public ColourShift GetColour(SortedList<int, ColourShift> list)
        {
            return GetColour(list, -1);
        }
        public ColourShift GetColour(SortedList<int, ColourShift> list, int setTime)
        {
            if (!_started)
            {
                _started = true;
                StartTime = Environment.TickCount;
            }
            object obj;
            if (setTime == -1) { obj = GetItemAt<ColourShift>(Environment.TickCount - StartTime, list); }
            else { obj = GetItemAt<ColourShift>(setTime, list); }
            if (obj != null) { return (ColourShift)obj; }
            return new ColourShift(0, 0, 0, 0);
        }
        public Point GetFrame(SortedList<int, Point> list)
        {
            return GetFrame(list, -1);
        }
        public Point GetFrame(SortedList<int, Point> list, int setTime)
        {
            if (!_started)
            {
                _started = true;
                StartTime = Environment.TickCount;
            }
            if (setTime == -1) { setTime = Environment.TickCount - StartTime; }
            int[] keyR = new int[list.Count];
            list.Keys.CopyTo(keyR, 0);
            for (int i = keyR.Length - 1; i >= 0; i--)
            {
                if (keyR[i] <= setTime)
                {
                    if (i < keyR.Length - 1) { _cSpent = false; }
                    return (Point)list[keyR[i]];
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
            Animation anim = new Animation(AnimName);
            anim.Loop = Loop;
            anim.AutoTrigger = _trigger;
            if (_move) { anim.WriteMovement(new SortedList<int, Vector2>(lastMovementFrames)); }
            if (_scale) { anim.WriteScaling(new SortedList<int, Vector2>(lastScaleFrames)); }
            if (_colour) { anim.WriteColouring(new SortedList<int, ColourShift>(lastColourFrames)); }
            if (_rotate) { anim.WriteRotation(new SortedList<int, float>(lastRotationFrames)); }
            if (_frames) { anim.WriteFrames(new SortedList<int, Point>(FrameFrames)); }
            return anim;
        }
        /// <summary>
        /// Returns a deep copy of the Animation object's instance states.
        /// </summary>
        /// <returns>The cloned Animation object.</returns>
        public Animation Copy()
        {
            object[] parameters = new object[] { AnimName, _spent, _started, StartTime, PlacedInPauseState, _hungTime, Loop, _move, _scale, _rotate, _colour, _frames, MovementFrames, lastMovementFrames, ScaleFrames, lastScaleFrames, RotationFrames, lastRotationFrames, ColourFrames, lastColourFrames, FrameFrames, _trigger };
            Animation anim = new Animation(parameters);
            return anim;
        }
        public Animation(object[] copiedParamters)
        {
            _animationRegInternal.Add(this);
            AnimName = (String)copiedParamters[0];
            _spent = (Boolean)copiedParamters[1];
            _started = (Boolean)copiedParamters[2];
            StartTime = (int)copiedParamters[3];
            PlacedInPauseState = (Boolean)copiedParamters[4];
            _hungTime = (int)copiedParamters[5];
            Loop = (Boolean)copiedParamters[6];
            _move = (Boolean)copiedParamters[7];
            _scale = (Boolean)copiedParamters[8];
            _rotate = (Boolean)copiedParamters[9];
            _colour = (Boolean)copiedParamters[10];
            _frames = (Boolean)copiedParamters[11];
            MovementFrames = new SortedList<int, Vector2>((SortedList<int, Vector2>)copiedParamters[12]);
            lastMovementFrames = new SortedList<int, Vector2>((SortedList<int, Vector2>)copiedParamters[13]);
            ScaleFrames = new SortedList<int, Vector2>((SortedList<int, Vector2>)copiedParamters[14]);
            lastScaleFrames = new SortedList<int, Vector2>((SortedList<int, Vector2>)copiedParamters[15]);
            RotationFrames = new SortedList<int, float>((SortedList<int, float>)copiedParamters[16]);
            lastRotationFrames = new SortedList<int, float>((SortedList<int, float>)copiedParamters[17]);
            ColourFrames = new SortedList<int, ColourShift>((SortedList<int, ColourShift>)copiedParamters[18]);
            lastColourFrames = new SortedList<int, ColourShift>((SortedList<int, ColourShift>)copiedParamters[19]);
            FrameFrames = new SortedList<int, Point>((SortedList<int, Point>)copiedParamters[20]);
            _trigger = (Boolean)copiedParamters[21];
        }
        /// <summary>
        /// Jumps an animated WorldEntity to the end of this Animation's cycle.
        /// </summary>
        /// <param name="operand">The WorldEntity being animated by this operation.</param>
        public void Jump(WorldEntity operand)
        {
            if (!_trigger) { return; }
            int maxTime = 0;
            foreach (int t in lastMovementFrames.Keys)
            {
                if (t > maxTime) { maxTime = t; }
            }
            foreach (int t in lastScaleFrames.Keys)
            {
                if (t > maxTime) { maxTime = t; }
            }
            foreach (int t in lastRotationFrames.Keys)
            {
                if (t > maxTime) { maxTime = t; }
            }
            foreach (int t in lastColourFrames.Keys)
            {
                if (t > maxTime) { maxTime = t; }
            }
            foreach (int t in FrameFrames.Keys)
            {
                if (t > maxTime) { maxTime = t; }
            }
            maxTime++;
            if (_move) { operand.Move(GetVector(MovementFrames, maxTime)); }
            if (_rotate) { operand.Rotate(GetRadians(RotationFrames, maxTime)); }
            if (_scale) { operand.Scale(GetVector(ScaleFrames, maxTime)); }
            if (_colour) { operand.Colour(GetColour(ColourFrames, maxTime)); }
            if (_frames)
            {
                Point aCo = GetFrame(FrameFrames);
                if (aCo != new Point(-1, -1)) { operand.SetAtlasFrame(aCo); }
            }
            if (!Loop)
            {
                _spent = true;
                if (_animationRegInternal.Contains(this)) { _animationRegInternal.Remove(this); }
            }
            else
            {
                _started = false;
                MovementFrames = new SortedList<int, Vector2>(lastMovementFrames);
                RotationFrames = new SortedList<int, float>(lastRotationFrames);
                ScaleFrames = new SortedList<int, Vector2>(lastScaleFrames);
                ColourFrames = new SortedList<int, ColourShift>(lastColourFrames);
            }
            StartTime = Environment.TickCount;
        }
        /// <summary>
        /// Advances the animated WorldEntity through its animation cycle based on the environment time elapsed, per the stored animation frames.
        /// </summary>
        /// <param name="operand">The WorldEntity being animated by this operation.</param>
        public void Step(WorldEntity operand)
        {
            if (!_trigger) { return; }
            _cSpent = true;
            if (_move) { operand.Move(GetVector(MovementFrames)); }
            if (_rotate) { operand.Rotate(GetRadians(RotationFrames)); }
            if (_scale) { operand.Scale(GetVector(ScaleFrames)); }
            if (_colour) { operand.Colour(GetColour(ColourFrames)); }
            if (_frames)
            {
                Point aCo = GetFrame(FrameFrames);
                if (aCo != new Point(-1, -1)) { operand.SetAtlasFrame(aCo); }
            }
            if (_cSpent)
            {
                if (!Loop)
                {
                    _spent = true;
                    if (_animationRegInternal.Contains(this)) { _animationRegInternal.Remove(this); }
                }
                else
                {
                    _started = false;
                    MovementFrames = new SortedList<int, Vector2>(lastMovementFrames);
                    RotationFrames = new SortedList<int, float>(lastRotationFrames);
                    ScaleFrames = new SortedList<int, Vector2>(lastScaleFrames);
                    ColourFrames = new SortedList<int, ColourShift>(lastColourFrames);
                }
            }
        }
        static public SortedList<int, Vector2> CreateVectorTween(Vector2 shift, int time, int frameLength)
        {
            if (frameLength <= 0) { frameLength = 1; }
            if (time < 0) { time = 0; }
            SortedList<int, Vector2> Construct = new SortedList<int, Vector2>();
            if (time == 0) { Construct.Add(0, shift); }
            else
            {
                Construct.Add(0, new Vector2());
                for (int i = frameLength; i <= time; i += frameLength)
                {
                    Construct.Add(i, shift / ((float)time / (float)frameLength));
                }
            }
            return Construct;
        }
        static public SortedList<int, float> CreateFloatTween(float shift, int time, int frameLength)
        {
            if (frameLength <= 0) { frameLength = 1; }
            if (time < 0) { time = 0; }
            SortedList<int, float> construct = new SortedList<int, float>();
            if (time == 0) { construct.Add(0, shift); }
            else
            {
                construct.Add(0, 0f);
                for (int i = frameLength; i <= time; i += frameLength)
                {
                    construct.Add(i, shift / ((float)time / (float)frameLength));
                }
            }
            return construct;
        }
        static public SortedList<int, ColourShift> CreateColourTween(ColourShift shift, int time, int frameLength)
        {
            if (frameLength <= 0) { frameLength = 1; }
            if (time < 0) { time = 0; }
            SortedList<int, ColourShift> construct = new SortedList<int, ColourShift>();
            if (time == 0) { construct.Add(0, shift); }
            else
            {
                construct.Add(0, new ColourShift(0, 0, 0, 0));
                for (int i = frameLength; i <= time; i += frameLength)
                {
                    construct.Add(i, new ColourShift(shift.R / ((float)time / (float)frameLength), shift.G / ((float)time / (float)frameLength), shift.B / ((float)time / (float)frameLength), shift.A / ((float)time / (float)frameLength)));
                }
            }
            return construct;
        }
        static public SortedList<int, T> MergeFrames<T>(SortedList<int, T> a, SortedList<int, T> b)
        {
            SortedList<int, T> construct = new SortedList<int, T>();
            int lFrame = 0;
            foreach (int key in a.Keys)
            {
                construct.Add(key, a[key]);
                lFrame = key;
            }
            foreach (int key in b.Keys)
            {
                if (construct.ContainsKey(key + lFrame)) { continue; }
                construct.Add(key + lFrame, b[key]);
            }
            return construct;
        }
    }
}
