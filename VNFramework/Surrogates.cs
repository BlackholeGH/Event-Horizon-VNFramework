﻿using System;
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
    public static class Surrogates
    {
        public sealed class RectangleSS : ISerializationSurrogate
        {
            public void GetObjectData(System.Object obj,
                                      SerializationInfo info, StreamingContext context)
            {
                Rectangle R = (Rectangle)obj;
                info.AddValue("X", R.X);
                info.AddValue("Y", R.Y);
                info.AddValue("Width", R.Width);
                info.AddValue("Height", R.Height);
            }
            public System.Object SetObjectData(System.Object obj,
                                               SerializationInfo info, StreamingContext context,
                                               ISurrogateSelector selector)
            {
                Rectangle R = (Rectangle)obj;
                R.X = (int)info.GetValue("X", typeof(int));
                R.Y = (int)info.GetValue("Y", typeof(int));
                R.Width = (int)info.GetValue("Width", typeof(int));
                R.Height = (int)info.GetValue("Height", typeof(int));
                obj = R;
                return obj;
            }
        }
        public sealed class Vector2SS : ISerializationSurrogate
        {
            public void GetObjectData(System.Object obj,
                                      SerializationInfo info, StreamingContext context)
            {
                Vector2 v2 = (Vector2)obj;
                info.AddValue("X", v2.X);
                info.AddValue("Y", v2.Y);
            }
            public System.Object SetObjectData(System.Object obj,
                                               SerializationInfo info, StreamingContext context,
                                               ISurrogateSelector selector)
            {
                Vector2 v2 = (Vector2)obj;
                v2.X = (float)info.GetValue("X", typeof(float));
                v2.Y = (float)info.GetValue("Y", typeof(float));
                obj = v2;
                return obj;
            }
        }
        public sealed class PointSS : ISerializationSurrogate
        {
            public void GetObjectData(System.Object obj,
                                      SerializationInfo info, StreamingContext context)
            {
                Point P = (Point)obj;
                info.AddValue("X", P.X);
                info.AddValue("Y", P.Y);
            }
            public System.Object SetObjectData(System.Object obj,
                                               SerializationInfo info, StreamingContext context,
                                               ISurrogateSelector selector)
            {
                Point P = (Point)obj;
                P.X = (int)info.GetValue("X", typeof(int));
                P.Y = (int)info.GetValue("Y", typeof(int));
                obj = P;
                return obj;
            }
        }
        public sealed class Texture2DSS : ISerializationSurrogate
        {
            public void GetObjectData(System.Object obj,
                                      SerializationInfo info, StreamingContext context)
            {
                Texture2D T = (Texture2D)obj;
                int[] Data = new int[T.Width * T.Height];
                T.GetData<int>(Data);
                info.AddValue("Data", Data);
                info.AddValue("Width", T.Width);
                info.AddValue("Height", T.Height);
            }
            public System.Object SetObjectData(System.Object obj,
                                               SerializationInfo info, StreamingContext context,
                                               ISurrogateSelector selector)
            {
                Texture2D T = (Texture2D)obj;
                int[] Data = (int[])info.GetValue("Data", typeof(int[]));
                int Width = (int)info.GetValue("Width", typeof(int));
                int Height = (int)info.GetValue("Height", typeof(int));
                T = new Texture2D(Shell.PubGD, Width, Height);
                T.SetData<int>(Data);
                obj = T;
                return obj;
            }
        }
        public sealed class ColorSS : ISerializationSurrogate
        {
            public void GetObjectData(System.Object obj,
                                      SerializationInfo info, StreamingContext context)
            {
                Color C = (Color)obj;
                info.AddValue("R", C.R);
                info.AddValue("G", C.G);
                info.AddValue("B", C.B);
                info.AddValue("A", C.A);
            }
            public System.Object SetObjectData(System.Object obj,
                                               SerializationInfo info, StreamingContext context,
                                               ISurrogateSelector selector)
            {
                Color C = (Color)obj;
                C.R = (byte)info.GetValue("R", typeof(byte));
                C.G = (byte)info.GetValue("G", typeof(byte));
                C.B = (byte)info.GetValue("B", typeof(byte));
                C.A = (byte)info.GetValue("A", typeof(byte));
                obj = C;
                return obj;
            }
        }
        public sealed class SpriteFontSS : ISerializationSurrogate
        {
            public void GetObjectData(System.Object obj,
                                      SerializationInfo info, StreamingContext context)
            {
            }
            public System.Object SetObjectData(System.Object obj,
                                               SerializationInfo info, StreamingContext context,
                                               ISurrogateSelector selector)
            {
                SpriteFont SF = (SpriteFont)obj;
                SF = Shell.Default;
                obj = SF;
                return obj;
            }
        }
    }
}