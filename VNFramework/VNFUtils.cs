﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Collections;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace VNFramework
{
    //Texture atlas info type
    [Serializable]
    public struct TAtlasInfo
    {
        [field: NonSerialized]
        private Texture2D pAtlas;
        public Texture2D Atlas
        {
            get
            {
                return pAtlas;
            }
            set
            {
                pAtlas = value;
                pSourceRect = pAtlas.Bounds;
            }
        }
        private Rectangle pSourceRect;
        public Rectangle SourceRect
        {
            get
            {
                return pSourceRect;
            }
        }
        public void SetManualSR(Rectangle Rect)
        {
            pSourceRect = Rect;
        }
        public Point DivDimensions;
        public Point FrameSize()
        {
            return new Point(SourceRect.Width / DivDimensions.X, SourceRect.Height / DivDimensions.Y);
        }
        public Hashtable FrameLookup;
        public String ReferenceHash;
    }
    public delegate void VoidDel();
    public static class VNFUtils
    {
        public static Point ConvertVector(Vector2 V)
        {
            return new Point((int)V.X, (int)V.Y);
        }
        public static Vector2 ConvertPoint(Point P)
        {
            return new Vector2(P.X, P.Y);
        }
        public static Point PointMultiply(Point P, Vector2 V)
        {
            return new Point((int)(V.X * P.X), (int)(V.Y * P.Y));
        }
        public static Point PointMultiply(Point P, Point P2)
        {
            return new Point((int)(P2.X * P.X), (int)(P2.Y * P.Y));
        }
        public static double GetLinearDistance(Vector2 A, Vector2 B)
        {
            return Math.Sqrt((double)((B.X - A.X) * (B.X - A.X)) + ((B.Y - A.Y) * (B.Y - A.Y)));
        }
        //Function to extract texture objects defined by rectangles from a larger spritesheet
        public static Texture2D ExtractTexture(Shell MyShell, Texture2D Sheet, Rectangle Source)
        {
            return ExtractTexture(MyShell, Sheet, Source, new Vector2(1, 1));
        }
        public static Texture2D ExtractTexture(Shell MyShell, Texture2D Sheet, Rectangle Source, Vector2 Scaling)
        {
            RenderTarget2D Output = new RenderTarget2D(MyShell.GraphicsDevice, (int)(Source.Width * Scaling.X), (int)(Source.Height * Scaling.Y), false,
                MyShell.GraphicsDevice.PresentationParameters.BackBufferFormat,
                DepthFormat.Depth24);
            MyShell.GraphicsDevice.SetRenderTarget(Output);
            MyShell.GraphicsDevice.Clear(Color.Transparent);
            MyShell.spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
            MyShell.spriteBatch.Draw(Sheet, new Rectangle(new Point(0, 0), new Point((int)(Source.Width * Scaling.X), (int)(Source.Height * Scaling.Y))), Source, Color.White);
            MyShell.spriteBatch.End();
            MyShell.GraphicsDevice.SetRenderTarget(null);
            Color[] texdata = new Color[Output.Width * Output.Height];
            Output.GetData(texdata);
            Texture2D NOut = new Texture2D(MyShell.GraphicsDevice, Output.Width, Output.Height);
            NOut.SetData(texdata);
            return NOut;
        }
        public static Texture2D CombineTextures(Shell MyShell, Point DestinationDims, Texture2D TextureA, Rectangle SourceA, Vector2 PositionA, Vector2 ScalingA, Texture2D TextureB, Rectangle SourceB, Vector2 PositionB, Vector2 ScalingB)
        {
            RenderTarget2D Output = new RenderTarget2D(MyShell.GraphicsDevice, DestinationDims.X, DestinationDims.Y, false,
                MyShell.GraphicsDevice.PresentationParameters.BackBufferFormat,
                DepthFormat.Depth24);
            MyShell.GraphicsDevice.SetRenderTarget(Output);
            MyShell.GraphicsDevice.Clear(Color.Transparent);
            MyShell.spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
            MyShell.spriteBatch.Draw(TextureA, new Rectangle(new Point((int)PositionA.X, (int)PositionA.Y), new Point((int)(SourceA.Width * ScalingA.X), (int)(SourceA.Height * ScalingA.Y))), SourceA, Color.White);
            MyShell.spriteBatch.Draw(TextureB, new Rectangle(new Point((int)PositionB.X, (int)PositionB.Y), new Point((int)(SourceB.Width * ScalingB.X), (int)(SourceB.Height * ScalingB.Y))), SourceB, Color.White);
            MyShell.spriteBatch.End();
            MyShell.GraphicsDevice.SetRenderTarget(null);
            Color[] texdata = new Color[Output.Width * Output.Height];
            Output.GetData(texdata);
            Texture2D NOut = new Texture2D(MyShell.GraphicsDevice, Output.Width, Output.Height);
            NOut.SetData(texdata);
            return NOut;
        }
        public static Texture2D GetNovelTextureOfColour(Shell MyShell, Color Colour, Point Dims)
        {
            RenderTarget2D Output = new RenderTarget2D(MyShell.GraphicsDevice, Dims.X, Dims.Y, false,
                MyShell.GraphicsDevice.PresentationParameters.BackBufferFormat,
                DepthFormat.Depth24);
            MyShell.GraphicsDevice.SetRenderTarget(Output);
            MyShell.GraphicsDevice.Clear(Colour);
            MyShell.GraphicsDevice.SetRenderTarget(null);
            Color[] texdata = new Color[Output.Width * Output.Height];
            Output.GetData(texdata);
            Texture2D NOut = new Texture2D(MyShell.GraphicsDevice, Output.Width, Output.Height);
            NOut.SetData(texdata);
            return NOut;
        }
        public static Texture2D GetFromRT(RenderTarget2D In)
        {
            Texture2D Out = new Texture2D(Shell.PubGD, In.Width, In.Height);
            Color[] texdata = new Color[Out.Width * Out.Height];
            In.GetData(texdata);
            Out.SetData(texdata);
            return Out;
        }
        public static class Strings
        {
            public static String ReplaceExclosed(String Input, String Find, String Replace, char Encloser)
            {
                Boolean Exclosed = true;
                for (int i = 0; i < Input.Length; i++)
                {
                    if (Input[i] == Encloser) { Exclosed = !Exclosed; }
                    if (Input[i] == Find[0] && Exclosed)
                    {
                        for (int ii = 0; ii < Find.Length; ii++)
                        {
                            if ((i + ii >= Input.Length) || !(Find[ii] == Input[i + ii])) { break; }
                            if (ii == Find.Length - 1)
                            {
                                Input = Input.Remove(i) + Replace + Input.Remove(0, i + ii + 1);
                                i += Replace.Length - 1;
                            }
                        }
                    }
                }
                return Input;
            }
            public static int IndexOfExclosed(String Input, String ContainsString, char Encloser)
            {
                Boolean Exclosed = true;
                for (int i = 0; i < Input.Length; i++)
                {
                    if (Input[i] == Encloser) { Exclosed = !Exclosed; }
                    if (Input[i] == ContainsString[0] && Exclosed)
                    {
                        for (int ii = 0; ii < ContainsString.Length; ii++)
                        {
                            if ((i + ii >= Input.Length) || !(ContainsString[ii] == Input[i + ii])) { break; }
                            if (ii == ContainsString.Length - 1) { return i; }
                        }
                    }
                }
                return -1;
            }
            public static Boolean ContainsExclosed(String Input, char ContainsChar, char Encloser)
            {
                Boolean Exclosed = true;
                foreach (char C in Input)
                {
                    if (C == Encloser) { Exclosed = !Exclosed; }
                    if (C == ContainsChar && Exclosed) { return true; }
                }
                return false;
            }
            public static String RemoveExclosed(String Input, char RemChar, char Encloser)
            {
                String Output = "";
                Boolean Remove = true;
                foreach (char C in Input)
                {
                    if (C == Encloser) { Remove = !Remove; }
                    if (C == RemChar && Remove) { continue; }
                    else { Output += C; }
                }
                return Output;
            }
            public static String[] SplitAtExclosed(String Input, char SplitChar, char EncloseCommence, char EncloseCease)
            {
                return SplitAtExclosed(Input, SplitChar, EncloseCommence, EncloseCease, '\0');
            }
            public static String[] SplitAtExclosed(String Input, char SplitChar, char EncloseCommence, char EncloseCease, char HigherLevel)
            {
                ArrayList Splits = new ArrayList();
                String Current = "";
                Boolean Split = true;
                Boolean HLevelEnc = false;
                foreach (char C in Input)
                {
                    if (!HLevelEnc)
                    {
                        if (HigherLevel != '\0' && C == HigherLevel)
                        {
                            HLevelEnc = true;
                            Split = false;
                        }
                        else
                        {
                            if (C == EncloseCommence && Split) { Split = false; }
                            else if (C == EncloseCease && !Split) { Split = true; }
                        }
                    }
                    else if (HigherLevel != '\0' && C == HigherLevel)
                    {
                        HLevelEnc = false;
                        Split = true;
                    }
                    if (C == SplitChar && Split)
                    {
                        Splits.Add(Current);
                        Current = "";
                    }
                    else { Current += C; }
                }
                Splits.Add(Current);
                return Splits.ToArray().Select(o => (String)o).ToArray();
            }
            public static String[] SplitAtExclosed(String Input, char SplitChar, char Encloser)
            {
                ArrayList Splits = new ArrayList();
                String Current = "";
                Boolean Split = true;
                foreach (char C in Input)
                {
                    if (C == Encloser) { Split = !Split; }
                    if (C == SplitChar && Split)
                    {
                        Splits.Add(Current);
                        Current = "";
                    }
                    else { Current += C; }
                }
                Splits.Add(Current);
                return Splits.ToArray().Select(o => (String)o).ToArray();
            }
            /// <summary>
            /// Splits a string into an array when the delimiter is not within an enclosing sequence. In this version, enclosers are tiered.
            /// </summary>
            /// <param name="Input">The string to split.</param>
            /// <param name="SplitChar">The char to split at.</param>
            /// <param name="Enclosers">The enclosing characters in order of priority.</param>
            /// <returns></returns>
            public static String[] SplitAtExclosed(String Input, char SplitChar, char[] Enclosers)
            {
                ArrayList Splits = new ArrayList();
                String Current = "";
                Boolean[] Enclosed = new Boolean[Enclosers.Length];
                for (int i = 0; i < Enclosed.Length; i++)
                {
                    Enclosed[i] = false;
                }
                foreach (char C in Input)
                {
                    int Set = -1;
                    for (int i = 0; i < Enclosers.Length; i++)
                    {
                        if (Set >= 0) { Enclosed[i] = Enclosed[Set]; }
                        else if (C == Enclosers[i])
                        {
                            Enclosed[i] = !Enclosed[i];
                            Set = i;
                        }
                    }
                    Boolean SplitNow = true;
                    foreach (Boolean B in Enclosed)
                    {
                        if (B)
                        {
                            SplitNow = false;
                            break;
                        }
                    }
                    if (C == SplitChar && SplitNow)
                    {
                        Splits.Add(Current);
                        Current = "";
                    }
                    else { Current += C; }
                }
                if (Current.Length > 0) { Splits.Add(Current); }
                return Splits.ToArray().Select(o => (String)o).ToArray();
            }
        }
    }
}