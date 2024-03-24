using Microsoft.Xna.Framework;
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
using System.Reflection;

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
        public class EventHorizonException : Exception
        {
            public EventHorizonException(String arg) : base(arg)
            { }
        }
        public static Dictionary<String, String> TypeAliasLookup()
        {
            Dictionary<String, String> Out = new Dictionary<string, string>();
            Out.Add("bool", "Boolean");
            Out.Add("byte", "Byte");
            Out.Add("sbyte", "SByte");
            Out.Add("char", "Char");
            Out.Add("decimal", "Decimal");
            Out.Add("double", "Double");
            Out.Add("float", "Single");
            Out.Add("int", "Int32");
            Out.Add("uint", "UInt32");
            Out.Add("long", "Int64");
            Out.Add("ulong", "UInt64");
            Out.Add("short", "Int16");
            Out.Add("ushort", "UInt16");
            Out.Add("object", "Object");
            Out.Add("string", "String");
            Out.Add("dynamic", "Object");
            return Out;
        }
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
            MyShell.ShellSpriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
            MyShell.ShellSpriteBatch.Draw(Sheet, new Rectangle(new Point(0, 0), new Point((int)(Source.Width * Scaling.X), (int)(Source.Height * Scaling.Y))), Source, Color.White);
            MyShell.ShellSpriteBatch.End();
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
            MyShell.ShellSpriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
            MyShell.ShellSpriteBatch.Draw(TextureA, new Rectangle(new Point((int)PositionA.X, (int)PositionA.Y), new Point((int)(SourceA.Width * ScalingA.X), (int)(SourceA.Height * ScalingA.Y))), SourceA, Color.White);
            MyShell.ShellSpriteBatch.Draw(TextureB, new Rectangle(new Point((int)PositionB.X, (int)PositionB.Y), new Point((int)(SourceB.Width * ScalingB.X), (int)(SourceB.Height * ScalingB.Y))), SourceB, Color.White);
            MyShell.ShellSpriteBatch.End();
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
        public static Boolean IsNumeric(Object T)
        {
            return (T is Int16 || T is Int32 || T is Int64 || T is float || T is double || T is Decimal);
        }
        public static object MultiAdd(object A, object B)
        {
            if(A is String || B is String)
            {
                return A.ToString() + B.ToString();
            }
            else if(IsNumeric(A) && IsNumeric(B))
            {
                Decimal Result = Convert.ToDecimal(A) + Convert.ToDecimal(B);
                if (A is Decimal || B is Decimal) { return Result; }
                else if (A is double || B is double) { return (double)Result; }
                else if (A is float || B is float) { return (float)Result; }
                else { return (int)Result; }
            }
            else { return "[Undefined addition operation]"; }
        }
        public static object MultiSubtract(object A, object B)
        {
            if (IsNumeric(A) && IsNumeric(B))
            {
                Decimal Result = Convert.ToDecimal(A) - Convert.ToDecimal(B);
                if (A is Decimal || B is Decimal) { return Result; }
                else if (A is double || B is double) { return (double)Result; }
                else if (A is float || B is float) { return (float)Result; }
                else { return (int)Result; }
            }
            else { return "[Undefined subtraction operation]"; }
        }
        public static object MultiMultiply(object A, object B)
        {
            if (IsNumeric(A) && IsNumeric(B))
            {
                Decimal Result = Convert.ToDecimal(A) * Convert.ToDecimal(B);
                if (A is Decimal || B is Decimal) { return Result; }
                else if (A is double || B is double) { return (double)Result; }
                else if (A is float || B is float) { return (float)Result; }
                else { return (int)Result; }
            }
            else { return "[Undefined multiplication operation]"; }
        }
        public static object MultiDivide(object A, object B)
        {
            if (IsNumeric(A) && IsNumeric(B))
            {
                Decimal Result = Convert.ToDecimal(A) / Convert.ToDecimal(B);
                if (A is Decimal || B is Decimal) { return Result; }
                else if (A is double || B is double) { return (double)Result; }
                else if (A is float || B is float) { return (float)Result; }
                else { return (int)Result; }
            }
            else { return "[Undefined division operation]"; }
        }
        public static Type TypeOfNameString(String TypeName, Boolean BroadSearch)
        {
            TypeName = TypeName.Replace("\\+", "+");
            if (BroadSearch)
            {
                ArrayList TheseTypes = new ArrayList();
                Assembly[] RawAssemblies = AppDomain.CurrentDomain.GetAssemblies();
                ArrayList OrderedAssemblies = new ArrayList();
                OrderedAssemblies.Add(Assembly.GetExecutingAssembly());
                OrderedAssemblies.Add(typeof(Shell).Assembly);
                OrderedAssemblies.Add(typeof(Game).Assembly);
                foreach (Assembly A in RawAssemblies)
                {
                    if (!OrderedAssemblies.Contains(A)) { OrderedAssemblies.Add(A); }
                }
                foreach (Assembly ThisAssembly in OrderedAssemblies)
                {
                    foreach (Type AssType in ThisAssembly.GetTypes())
                    {
                        TheseTypes.Add(AssType);
                    }
                }
                foreach (Type T in TheseTypes)
                {
                    if (T.Name.ToUpper() == TypeName.ToUpper())
                    {
                        TypeName = T.AssemblyQualifiedName;
                        break;
                    }
                }
            }
            return Type.GetType(TypeName, false, false);
        }
        public static class SysProperties
        {
            public static String DayOfWeek
            {
                get
                {
                    return System.DateTime.Now.DayOfWeek.ToString();
                }
            }
            public static String Time
            {
                get
                {
                    return System.DateTime.Now.ToString("hh:mm tt");
                }
            }
        }
        public static class Strings
        {
            public static String Upperise(String In)
            {
                return In.ToUpper();
            }
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
            public static String ReplaceExclosedOutestTier(String Input, String Find, String Replace, char Encloser)
            {
                int First = Input.IndexOf(Encloser);
                int Last = Input.LastIndexOf(Encloser);
                if (First == Last) { return Input.Replace(Find, Replace); }
                else
                {
                    for (int i = 0; i < Input.Length; i++)
                    {
                        if (Input[i] == Find[0] && (i < First || i > Last))
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
            }
            public static Boolean ContainsExclosedFromNestingChars(String Input, char ContainsChar, char EncloseCommence, char EncloseCease)
            {
                return ContainsExclosedFromNestingChars(Input, ContainsChar, new char[] { EncloseCommence }, new char[] { EncloseCease });
            }
            public static Boolean ContainsExclosedFromNestingChars(String Input, char ContainsChar, char[] EncloseCommence, char[] EncloseCease)
            {
                int level = 0;
                foreach (char c in Input)
                {
                    if (EncloseCommence.Contains(c)) { level++; }
                    if (EncloseCease.Contains(c) && level > 0) { level--; }
                    if (c == ContainsChar && level == 0) { return true; }
                }
                return false;
            }
            public static String[] SplitAtExclosedFromNestingChars(String Input, char SplitChar, char EncloseCommence, char EncloseCease)
            {
                return SplitAtExclosedFromNestingChars(Input, SplitChar, new char[] { EncloseCommence }, new char[] { EncloseCease });
            }
            public static String[] SplitAtExclosedFromNestingChars(String Input, char SplitChar, char[] EncloseCommence, char[] EncloseCease)
            {
                ArrayList Splits = new ArrayList();
                StringBuilder Current = new StringBuilder();
                int level = 0;
                foreach (char c in Input)
                {
                    if (EncloseCommence.Contains(c)) { level++; }
                    if (EncloseCease.Contains(c) && level > 0) { level--; }
                    if (c == SplitChar && level == 0)
                    {
                        Splits.Add(Current.ToString());
                        Current = new StringBuilder();
                    }
                    else { Current.Append(c); }
                }
                Splits.Add(Current.ToString());
                return Splits.ToArray().Select(o => (String)o).ToArray();
            }
            /// <summary>
            /// This String replacer method will replace all instances of an occurrence of a substring in a String with another, if said occurrence is enclosed within one set of characters, but not also enclosed within another.
            /// </summary>
            /// <param name="Input">An input String.</param>
            /// <param name="Find">The substring to find.</param>
            /// <param name="Replace">The string to replace with.</param>
            /// <param name="ValidEncloser">All replacing should be enclosed between two of this parameter.</param>
            /// <param name="InvalidEncloser">No replacing can take place between two of this parameter.</param>
            /// <returns></returns>
            public static String ReplaceEnclosedExclosed(String Input, String Find, String Replace, char ValidEncloser, char InvalidEncloser)
            {
                Boolean ValidEnclosed = false;
                Boolean InvalidEnclosed = false;
                for (int i = 0; i < Input.Length; i++)
                {
                    Char C = Input[i];
                    if (!InvalidEnclosed)
                    {
                        if (InvalidEncloser != '\0' && C == InvalidEncloser)
                        {
                            InvalidEnclosed = true;
                        }
                        else
                        {
                            if (C == ValidEncloser && !ValidEnclosed) { ValidEnclosed = true; }
                            else if (C == ValidEncloser && ValidEnclosed) { ValidEnclosed = false; }
                        }
                    }
                    else if (InvalidEncloser != '\0' && C == InvalidEncloser)
                    {
                        InvalidEnclosed = false;
                    }
                    if (Input[i] == Find[0] && ValidEnclosed && !InvalidEnclosed)
                    {
                        for(int ii = 0; ii < Find.Length; ii++)
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
            public static int IndexOfExclosed(String Input, String ContainsString, char EncloseCommence, char EncloseCease)
            {
                return IndexOfExclosed(Input, ContainsString, EncloseCommence, EncloseCease, '\0');
            }
            public static int IndexOfExclosed(String Input, String ContainsString, char EncloseCommence, char EncloseCease, char HigherLevel)
            {
                Boolean Exclosed = true;
                Boolean HLevelEnc = false;
                int i = -1;
                foreach (char C in Input)
                {
                    i++;
                    if (!HLevelEnc)
                    {
                        if (HigherLevel != '\0' && C == HigherLevel)
                        {
                            HLevelEnc = true;
                            Exclosed = false;
                        }
                        else
                        {
                            if (C == EncloseCommence && Exclosed) { Exclosed = false; }
                            else if (C == EncloseCease && !Exclosed) { Exclosed = true; }
                        }
                    }
                    else if (HigherLevel != '\0' && C == HigherLevel)
                    {
                        HLevelEnc = false;
                        Exclosed = true;
                    }
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
            public static Boolean ContainsExclosed(String Input, String ContainsString, char Encloser)
            {
                return IndexOfExclosed(Input, ContainsString, Encloser) != -1;
            }
            public static Boolean ContainsExclosed(String Input, String ContainsString, char EncloseCommence, char EncloseCease)
            {
                return ContainsExclosed(Input, ContainsString, EncloseCommence, EncloseCease, '\0');
            }
            public static Boolean ContainsExclosed(String Input, String ContainsString, char EncloseCommence, char EncloseCease, char HigherLevel)
            {
                return IndexOfExclosed(Input, ContainsString, EncloseCommence, EncloseCease, HigherLevel) != -1;
            }
            public static Boolean ContainsExclosed(String Input, char ContainsChar, char EncloseCommence, char EncloseCease)
            {
                return ContainsExclosed(Input, ContainsChar, EncloseCommence, EncloseCease, '\0');
            }
            public static Boolean ContainsExclosed(String Input, char ContainsChar, char EncloseCommence, char EncloseCease, char HigherLevel)
            {
                Boolean Exclosed = true;
                Boolean HLevelEnc = false;
                foreach (char C in Input)
                {
                    if (!HLevelEnc)
                    {
                        if (HigherLevel != '\0' && C == HigherLevel)
                        {
                            HLevelEnc = true;
                            Exclosed = false;
                        }
                        else
                        {
                            if (C == EncloseCommence && Exclosed) { Exclosed = false; }
                            else if (C == EncloseCease && !Exclosed) { Exclosed = true; }
                        }
                    }
                    else if (HigherLevel != '\0' && C == HigherLevel)
                    {
                        HLevelEnc = false;
                        Exclosed = true;
                    }
                    if (C == ContainsChar && Exclosed)
                    {
                        return true;
                    }
                }
                return false;
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
                StringBuilder Output = new StringBuilder();
                Boolean Remove = true;
                foreach (char C in Input)
                {
                    if (C == Encloser) { Remove = !Remove; }
                    if (C == RemChar && Remove) { continue; }
                    else { Output.Append(C); }
                }
                return Output.ToString();
            }
            public static String[] SplitAtExclosed(String Input, char SplitChar, char EncloseCommence, char EncloseCease)
            {
                return SplitAtExclosed(Input, SplitChar, new char[] { EncloseCommence }, new char[] { EncloseCease }, '\0');
            }
            public static String[] SplitAtExclosed(String Input, char SplitChar, char EncloseCommence, char EncloseCease, char HigherLevel)
            {
                return SplitAtExclosed(Input, SplitChar, new char[] { EncloseCommence }, new char[] { EncloseCease }, HigherLevel);
            }
            public static String[] SplitAtExclosed(String Input, char SplitChar, char[] EncloseCommence, char[] EncloseCease, char HigherLevel)
            {
                ArrayList Splits = new ArrayList();
                StringBuilder Current = new StringBuilder();
                Boolean Split = true;
                Boolean HLevelEnc = false;
                foreach (char C in Input)
                {
                    if (!HLevelEnc)
                    {
                        if (HigherLevel != '\0' && C == HigherLevel)
                        {
                            HLevelEnc = true;
                        }
                        else
                        {
                            if (EncloseCommence.Contains(C) && Split) { Split = false; }
                            else if (EncloseCease.Contains(C) && !Split) { Split = true; }
                        }
                    }
                    else if (HigherLevel != '\0' && C == HigherLevel)
                    {
                        HLevelEnc = false;
                    }
                    if (C == SplitChar && Split && !HLevelEnc)
                    {
                        Splits.Add(Current.ToString());
                        Current = new StringBuilder();
                    }
                    else { Current.Append(C); }
                }
                Splits.Add(Current.ToString());
                return Splits.ToArray().Select(o => (String)o).ToArray();
            }
            public static String[] SplitAtExclosed(String Input, char SplitChar, char Encloser)
            {
                ArrayList Splits = new ArrayList();
                StringBuilder Current = new StringBuilder();
                Boolean Split = true;
                foreach (char C in Input)
                {
                    if (C == Encloser) { Split = !Split; }
                    if (C == SplitChar && Split)
                    {
                        Splits.Add(Current.ToString());
                        Current = new StringBuilder();
                    }
                    else { Current.Append(C); }
                }
                Splits.Add(Current.ToString());
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
                StringBuilder Current = new StringBuilder();
                Boolean[] Enclosed = new Boolean[Enclosers.Length];
                for (int i = 0; i < Enclosed.Length; i++)
                {
                    Enclosed[i] = false;
                }
                foreach (char C in Input)
                {
                    for (int i = 0; i < Enclosers.Length; i++)
                    {
                        if (C == Enclosers[i])
                        {
                            Enclosed[i] = !Enclosed[i];
                        }
                        if(Enclosed[i]) { break; }
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
                        Splits.Add(Current.ToString());
                        Current = new StringBuilder();
                    }
                    else { Current.Append(C); }
                }
                if (Current.Length > 0) { Splits.Add(Current.ToString()); }
                return Splits.ToArray().Select(o => (String)o).ToArray();
            }
        }
    }
}
