using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace VNFramework
{
    public static class ManifestReader
    {
        public class ManifestReaderException : Exception
        {
            public ManifestReaderException(String Arg) : base(Arg)
            { }
        }
        public static Hashtable ReadManifestFile(String Filename)
        {
            Shell.WriteLine("Reading from manifest: " + Filename);
            if (Filename.EndsWith(".ehm")) { Filename = Filename.Remove(Filename.Length - 4); }
            DirectoryInfo ManifestDir = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Blackhole Media Systems\\Event Horizon Framework\\appdata\\appmanifests");
            if (ManifestDir.Exists)
            {
                String Manifest = "";
                foreach (FileInfo F in ManifestDir.EnumerateFiles())
                {
                    String FName = F.Name;
                    if (FName.EndsWith(".ehm")) { FName = FName.Remove(FName.Length - 4); }
                    if (FName.ToUpper() == Filename.ToUpper())
                    {
                        StreamReader Reader = new StreamReader(F.OpenRead());
                        String Line = "";
                        while((Line = Reader.ReadLine()) != null)
                        {
                            if(Line.Length == 0 || Line.StartsWith("//")) { continue; }
                            else
                            {
                                Manifest += Line;
                            }
                        }
                        Reader.Close();
                        break;
                    }
                }
                if(Manifest.Length == 0)
                {
                    Shell.WriteLine("Could not read manifest file!");
                    return null;
                }
                else
                {
                    Manifest = Manifest.Replace("\n", "");
                    Manifest = VNFUtils.Strings.RemoveExclosed(Manifest, ' ', '\"');
                    Manifest = Manifest.Replace("\r", "");
                    Manifest = Manifest.Replace("\n", "");
                    Hashtable Manifestae = new Hashtable();
                    int MIndex = VNFUtils.Strings.IndexOfExclosed(Manifest, "manifest", '\"');
                    if (MIndex > 0) { Manifest = Manifest.Remove(0, MIndex); }
                    while (MIndex > 0)
                    {
                        Manifest = Manifest.Remove(0, Manifest.IndexOf('\"') + 1);
                        String MName = Manifest.Remove(Manifest.IndexOf('\"'));
                        Manifest = Manifest.Remove(0, Manifest.IndexOf('\"') + 1);
                        Manifest = Manifest.Remove(0, Manifest.IndexOf(':') + 1);
                        MIndex = VNFUtils.Strings.IndexOfExclosed(Manifest, "manifest", '\"');
                        if(MIndex > 0)
                        {
                            Shell.WriteLine("Added manifest " + MName + ".");
                            Manifestae.Add(MName, Manifest.Remove(MIndex));
                        }
                        else
                        {
                            Shell.WriteLine("Added manifest " + MName + ".");
                            Manifestae.Add(MName, Manifest);
                        }
                    }
                    return Manifestae;
                }
            }
            else { return null; }
        }
        public static Hashtable[] ParseManifest(String Manifest, Shell MyShell)
        {
            Shell.WriteLine("Parsing manifest contents.");
            Hashtable MetaDirectory = new Hashtable();
            Hashtable ScriptDirectory = new Hashtable();
            Hashtable FontDirectory = new Hashtable();
            Hashtable SFXDirectory = new Hashtable();
            Hashtable SongDirectory = new Hashtable();
            Hashtable StemAtlasTemps = new Hashtable();
            Hashtable AtlasDirectory = new Hashtable();
            String[] MEntries = VNFUtils.Strings.SplitAtExclosed(Manifest, ';', '\"');
            int ReadProg = 0;
            foreach(String Entry in MEntries)
            {
                String[] EntrySegment = VNFUtils.Strings.SplitAtExclosed(Entry, '|', '\"');
                object DirIndex;
                String ContentFilePath;
                String StrContent = "";
                switch (EntrySegment[0].ToUpper())
                {
                    case "COMMENCER_SCRIPT":
                        StrContent = (String)EntityFactory.ParseRealData(EntrySegment[1]);
                        Shell.WriteLine("Manifested application will commence at script " + StrContent + ".");
                        MetaDirectory.Add("startatscript", StrContent);
                        break;
                    case "TREAT_COMMENCER_AS_UNIQUE_INTRO":
                        Boolean UseUnique = (Boolean)EntityFactory.ParseRealData(EntrySegment[1]);
                        Shell.WriteLine("Use unique intro sniffer? " + UseUnique);
                        MetaDirectory.Add("useunique", UseUnique);
                        break;
                    case "SCRIPT":
                        StrContent = (String)EntityFactory.ParseRealData(EntrySegment[1]);
                        ScriptDirectory.Add(StrContent, StrContent);
                        break;
                    case "FONT":
                        DirIndex = EntityFactory.ParseRealData(EntrySegment[1]);
                        ContentFilePath = EntrySegment[2].Trim('\"');
                        FontDirectory.Add(DirIndex, MyShell.Content.Load<SpriteFont>(ContentFilePath));
                        Shell.WriteLine("Loaded font " + ContentFilePath + " to " + DirIndex.ToString() + ".");
                        break;
                    case "SOUND":
                        DirIndex = EntityFactory.ParseRealData(EntrySegment[1]);
                        ContentFilePath = EntrySegment[2].Trim('\"');
                        SFXDirectory.Add(DirIndex, MyShell.Content.Load<SoundEffect>(ContentFilePath));
                        Shell.WriteLine("Loaded sound effect " + ContentFilePath + " to " + DirIndex.ToString() + ".");
                        break;
                    case "MUSIC":
                        DirIndex = EntityFactory.ParseRealData(EntrySegment[1]);
                        ContentFilePath = EntrySegment[2].Trim('\"');
                        SongDirectory.Add(DirIndex, MyShell.Content.Load<Song>(ContentFilePath));
                        Shell.WriteLine("Loaded song " + ContentFilePath + " to " + DirIndex.ToString() + ".");
                        break;
                    case "ATLAS":
                        DirIndex = EntityFactory.ParseRealData(EntrySegment[1]);
                        ContentFilePath = EntrySegment[2].Trim('\"');
                        StemAtlasTemps.Add(DirIndex, MyShell.Content.Load<Texture2D>(ContentFilePath));
                        Shell.WriteLine("Loaded texture atlas " + ContentFilePath + " to " + DirIndex.ToString() + ".");
                        break;
                    case "TEXTURE":
                        DirIndex = EntityFactory.ParseRealData(EntrySegment[1]);
                        AtlasDirectory.Add(DirIndex, ParseTextureAtlas(EntrySegment, StemAtlasTemps, MyShell));
                        break;
                }
                ReadProg++;
                try
                {
                    Monitor.Enter(MyShell.LPLockObj);
                    MyShell.LoadPercentage = (float)0.99 * ((float)ReadProg / MEntries.Length);
                }
                finally { Monitor.Exit(MyShell.LPLockObj); }
            }
            Shell.WriteLine("Finished reading manifest.");
            return new Hashtable[] { MetaDirectory, ScriptDirectory, FontDirectory, SFXDirectory, SongDirectory, AtlasDirectory };
        }
        private static TAtlasInfo ParseTextureAtlas(String[] EntrySegment, Hashtable StemAtlasTemps, Shell MyShell)
        {
            TAtlasInfo TexAssembly = new TAtlasInfo();
            object DirIndex = EntityFactory.ParseRealData(EntrySegment[1]);
            if (!(DirIndex is String || DirIndex is string)) { throw new ManifestReaderException("Manifest is invalid: Texture atlases must be String indexed at point of load."); }
            TexAssembly.ReferenceHash = (String)DirIndex;
            Texture2D Current = null;
            if (EntrySegment[2].ToUpper().StartsWith("ATL:"))
            {
                String[] ATLParams = VNFUtils.Strings.SplitAtExclosed(EntrySegment[2].Remove(0, 4), ',', '\"');
                object AtlasStemIndex = EntityFactory.ParseRealData(ATLParams[0]);
                if (StemAtlasTemps.ContainsKey(AtlasStemIndex))
                {
                    Texture2D Stem = (Texture2D)StemAtlasTemps[AtlasStemIndex];
                    int[] RectDims = new int[4];
                    try
                    {
                        for (int i = 1; i <= 4; i++)
                        {
                            RectDims[i - 1] = Convert.ToInt32(ATLParams[i]);
                        }
                    }
                    catch (FormatException E) { throw new ManifestReaderException("Manifest is invalid: Texture extraction bounds are in an incorrect format."); }
                    Task<Texture2D> NewGTask = new Task<Texture2D>(() =>
                    {
                        return VNFUtils.ExtractTexture(MyShell, Stem, new Rectangle(RectDims[0], RectDims[1], RectDims[2], RectDims[3]));
                    });
                    try
                    {
                        Monitor.Enter(MyShell.LoadGraphicsQueue);
                        MyShell.LoadGraphicsQueue.Enqueue(NewGTask);
                    }
                    finally { Monitor.Exit(MyShell.LoadGraphicsQueue); }
                    NewGTask.Wait();
                    Current = NewGTask.GetAwaiter().GetResult();
                    NewGTask.Dispose();
                    Shell.WriteLine("Extracted texture from atlas " + AtlasStemIndex.ToString() + " to " + DirIndex.ToString() + ".");
                }
                else { throw new ManifestReaderException("Manifest is invalid: Attempted to derive texture from an index that did not correspond to a loaded atlas sheet."); }
            }
            else
            {
                String ContentFilePath = EntrySegment[2].Trim('\"');
                Current = MyShell.Content.Load<Texture2D>(ContentFilePath);
                Shell.WriteLine("Loaded texture " + ContentFilePath + " to " + DirIndex.ToString() + ".");
            }
            TexAssembly.Atlas = Current;
            for (int i = 3; i < EntrySegment.Length; i++)
            {
                if (EntrySegment[i].ToUpper().StartsWith("DIV:"))
                {
                    String[] Divs = EntrySegment[i].Remove(0, 4).Split(',');
                    int X = 1;
                    int Y = 1;
                    try
                    {
                        X = Convert.ToInt32(Divs[0]);
                        Y = Convert.ToInt32(Divs[1]);
                    }
                    catch (FormatException E) { throw new ManifestReaderException("Manifest is invalid: Texture division sizes are in an incorrect format."); }
                    TexAssembly.DivDimensions = new Point(X, Y);
                }
                else if (EntrySegment[i].ToUpper().StartsWith("FL:"))
                {
                    if(TexAssembly.FrameLookup == null) { TexAssembly.FrameLookup = new Hashtable(); }
                    String[] FLParams = EntrySegment[i].Remove(0, 3).Split(':');
                    object FrameKey = EntityFactory.ParseRealData(FLParams[0]);
                    String[] FrameDivs = FLParams[1].Split(',');
                    int X;
                    int Y;
                    try
                    {
                        X = Convert.ToInt32(FrameDivs[0]);
                        Y = Convert.ToInt32(FrameDivs[1]);
                    }
                    catch (FormatException E) { throw new ManifestReaderException("Manifest is invalid: Texture division sizes are in an incorrect format."); }
                    TexAssembly.FrameLookup.Add(FrameKey, new Point(X, Y));
                }
            }
            return TexAssembly;
        }
        public static Boolean IngestScriptFile(String Filename)
        {
            if (Filename.EndsWith(".esa")) { Filename = Filename.Remove(Filename.Length - 4); }
            DirectoryInfo ScriptArchive = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Blackhole Media Systems\\Event Horizon Framework\\appdata\\scripts");
            if (ScriptArchive.Exists)
            {
                foreach (FileInfo F in ScriptArchive.EnumerateFiles())
                {
                    String FName = F.Name;
                    if (FName.EndsWith(".esa")) { FName = FName.Remove(FName.Length - 4); }
                    if (FName.ToUpper() == Filename.ToUpper())
                    {
                        StreamReader Reader = new StreamReader(F.OpenRead());
                        String ScriptContent = Reader.ReadToEnd();
                        Hashtable ExtractedScripts = ScriptProcessor.ExtractEventScriptArchive(ScriptContent);
                        foreach (String Key in ExtractedScripts.Keys)
                        {
                            ScriptProcessor.ScriptCache.Add(Key, ExtractedScripts[Key]);
                        }
                        Reader.Close();
                        return true;
                    }
                }
                return false;
            }
            else { return false; }
        }
    }
}
