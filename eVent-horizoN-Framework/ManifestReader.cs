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
            public ManifestReaderException(String arg) : base(arg)
            { }
        }
        public static Dictionary<string, string> ReadManifestFile(String fileName)
        {
            Shell.WriteLine("Reading from manifest: " + fileName);
            if (fileName.EndsWith(".ehm")) { fileName = fileName.Remove(fileName.Length - 4); }
            DirectoryInfo manifestDir = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Blackhole Media Systems\\Event Horizon Framework\\appdata\\appmanifests");
            if (manifestDir.Exists)
            {
                String manifest = "";
                foreach (FileInfo F in manifestDir.EnumerateFiles())
                {
                    String FName = F.Name;
                    if (FName.EndsWith(".ehm")) { FName = FName.Remove(FName.Length - 4); }
                    if (FName.ToUpper() == fileName.ToUpper())
                    {
                        StreamReader Reader = new StreamReader(F.OpenRead());
                        String Line = "";
                        while((Line = Reader.ReadLine()) != null)
                        {
                            if(Line.Length == 0 || Line.StartsWith("//")) { continue; }
                            else
                            {
                                manifest += Line;
                            }
                        }
                        Reader.Close();
                        break;
                    }
                }
                if(manifest.Length == 0)
                {
                    Shell.WriteLine("Could not read manifest file!");
                    return null;
                }
                else
                {
                    manifest = manifest.Replace("\n", "");
                    manifest = VNFUtils.Strings.RemoveExclosed(manifest, ' ', '\"');
                    manifest = manifest.Replace("\r", "");
                    manifest = manifest.Replace("\n", "");
                    Dictionary<string, string> manifestae = new Dictionary<string, string>();
                    int manifestIndex = VNFUtils.Strings.IndexOfExclosed(manifest, "manifest", '\"');
                    if (manifestIndex > 0) { manifest = manifest.Remove(0, manifestIndex); }
                    while (manifestIndex > 0)
                    {
                        manifest = manifest.Remove(0, manifest.IndexOf('\"') + 1);
                        String manifestName = manifest.Remove(manifest.IndexOf('\"'));
                        manifest = manifest.Remove(0, manifest.IndexOf('\"') + 1);
                        manifest = manifest.Remove(0, manifest.IndexOf(':') + 1);
                        manifestIndex = VNFUtils.Strings.IndexOfExclosed(manifest, "manifest", '\"');
                        if(manifestIndex > 0)
                        {
                            Shell.WriteLine("Added manifest " + manifestName + ".");
                            manifestae.Add(manifestName, manifest.Remove(manifestIndex));
                        }
                        else
                        {
                            Shell.WriteLine("Added manifest " + manifestName + ".");
                            manifestae.Add(manifestName, manifest);
                        }
                    }
                    return manifestae;
                }
            }
            else { return null; }
        }
        public static object[] ParseManifest(String manifest, Shell myShell)
        {
            Shell.WriteLine("Parsing manifest contents.");
            Dictionary<object, object> metaDirectory = new Dictionary<object, object>();
            List<string> scriptDirectory = new List<string>();
            Dictionary<object, SpriteFont> fontDirectory = new Dictionary<object, SpriteFont>();
            Dictionary<object, SoundEffect> sfxDirectory = new Dictionary<object, SoundEffect>();
            Dictionary<object, Song> songDirectory = new Dictionary<object, Song>();
            Dictionary<object, Texture2D> stemAtlasTemps = new Dictionary<object, Texture2D>();
            Dictionary<object, TAtlasInfo> atlasDirectory = new Dictionary<object, TAtlasInfo>();
            String[] manifestEntries = VNFUtils.Strings.SplitAtExclosed(manifest, ';', '\"');
            int readProg = 0;
            foreach(String entry in manifestEntries)
            {
                String[] entrySegment = VNFUtils.Strings.SplitAtExclosed(entry, '|', '\"');
                object dirIndex;
                String contentFilePath;
                String strContent = "";
                switch (entrySegment[0].ToUpper())
                {
                    case "COMMENCER_SCRIPT":
                        strContent = (String)EntityFactory.ParseRealData(entrySegment[1]);
                        Shell.WriteLine("Manifested application will commence at script " + strContent + ".");
                        metaDirectory.Add("startatscript", strContent);
                        break;
                    case "TREAT_COMMENCER_AS_UNIQUE_INTRO":
                        Boolean UseUnique = (Boolean)EntityFactory.ParseRealData(entrySegment[1]);
                        Shell.WriteLine("Use unique intro sniffer? " + UseUnique);
                        metaDirectory.Add("useunique", UseUnique);
                        break;
                    case "DEFAULT_RESOLUTION":
                        Vector2 res = (Vector2)EntityFactory.ParseRealData(entrySegment[1]);
                        Shell.WriteLine("Default resolution set to: X: " + res.X + ", Y: " + res.Y + ".");
                        metaDirectory.Add("defaultresolution", res);
                        break;
                    case "SCRIPT":
                        strContent = (String)EntityFactory.ParseRealData(entrySegment[1]);
                        scriptDirectory.Add(strContent);
                        break;
                    case "FONT":
                        dirIndex = EntityFactory.ParseRealData(entrySegment[1]);
                        contentFilePath = entrySegment[2].Trim('\"');
                        fontDirectory.Add(dirIndex, myShell.Content.Load<SpriteFont>(contentFilePath));
                        Shell.WriteLine("Loaded font " + contentFilePath + " to " + dirIndex.ToString() + ".");
                        break;
                    case "SOUND":
                        dirIndex = EntityFactory.ParseRealData(entrySegment[1]);
                        contentFilePath = entrySegment[2].Trim('\"');
                        sfxDirectory.Add(dirIndex, myShell.Content.Load<SoundEffect>(contentFilePath));
                        Shell.WriteLine("Loaded sound effect " + contentFilePath + " to " + dirIndex.ToString() + ".");
                        break;
                    case "MUSIC":
                        dirIndex = EntityFactory.ParseRealData(entrySegment[1]);
                        contentFilePath = entrySegment[2].Trim('\"');
                        songDirectory.Add(dirIndex, myShell.Content.Load<Song>(contentFilePath));
                        Shell.WriteLine("Loaded song " + contentFilePath + " to " + dirIndex.ToString() + ".");
                        break;
                    case "ATLAS":
                        dirIndex = EntityFactory.ParseRealData(entrySegment[1]);
                        contentFilePath = entrySegment[2].Trim('\"');
                        stemAtlasTemps.Add(dirIndex, myShell.Content.Load<Texture2D>(contentFilePath));
                        Shell.WriteLine("Loaded texture atlas " + contentFilePath + " to " + dirIndex.ToString() + ".");
                        break;
                    case "TEXTURE":
                        dirIndex = EntityFactory.ParseRealData(entrySegment[1]);
                        atlasDirectory.Add(dirIndex, ParseTextureAtlas(entrySegment, stemAtlasTemps, myShell));
                        break;
                }
                readProg++;
                try
                {
                    Monitor.Enter(myShell.LPLockObj);
                    myShell.LoadPercentage = (float)0.95 * ((float)readProg / manifestEntries.Length);
                }
                finally { Monitor.Exit(myShell.LPLockObj); }
            }
            Shell.WriteLine("Finished reading manifest.");
            return new object[] { metaDirectory, scriptDirectory, fontDirectory, sfxDirectory, songDirectory, atlasDirectory };
        }
        private static TAtlasInfo ParseTextureAtlas(String[] entrySegment, Dictionary<object, Texture2D> stemAtlasTemps, Shell myShell)
        {
            TAtlasInfo texAssembly = new TAtlasInfo();
            object dirIndex = EntityFactory.ParseRealData(entrySegment[1]);
            if (!(dirIndex is String || dirIndex is string)) { throw new ManifestReaderException("Manifest is invalid: Texture atlases must be String indexed at point of load."); }
            texAssembly.ReferenceHash = (String)dirIndex;
            Texture2D current = null;
            if (entrySegment[2].ToUpper().StartsWith("ATL:"))
            {
                String[] atlParams = VNFUtils.Strings.SplitAtExclosed(entrySegment[2].Remove(0, 4), ',', '\"');
                object atlasStemIndex = EntityFactory.ParseRealData(atlParams[0]);
                if (stemAtlasTemps.ContainsKey(atlasStemIndex))
                {
                    Texture2D Stem = (Texture2D)stemAtlasTemps[atlasStemIndex];
                    int[] rectDims = new int[4];
                    try
                    {
                        for (int i = 1; i <= 4; i++)
                        {
                            rectDims[i - 1] = Convert.ToInt32(atlParams[i]);
                        }
                    }
                    catch (FormatException) { throw new ManifestReaderException("Manifest is invalid: Texture extraction bounds are in an incorrect format."); }
                    Task<Texture2D> newGTask = new Task<Texture2D>(() =>
                    {
                        return VNFUtils.ExtractTexture(myShell, Stem, new Rectangle(rectDims[0], rectDims[1], rectDims[2], rectDims[3]));
                    });
                    try
                    {
                        Monitor.Enter(myShell.LoadGraphicsQueue);
                        myShell.LoadGraphicsQueue.Enqueue(newGTask);
                    }
                    finally { Monitor.Exit(myShell.LoadGraphicsQueue); }
                    newGTask.Wait();
                    current = newGTask.GetAwaiter().GetResult();
                    newGTask.Dispose();
                    Shell.WriteLine("Extracted texture from atlas " + atlasStemIndex.ToString() + " to " + dirIndex.ToString() + ".");
                }
                else { throw new ManifestReaderException("Manifest is invalid: Attempted to derive texture from an index that did not correspond to a loaded atlas sheet."); }
            }
            else
            {
                String contentFilePath = entrySegment[2].Trim('\"');
                current = myShell.Content.Load<Texture2D>(contentFilePath);
                Shell.WriteLine("Loaded texture " + contentFilePath + " to " + dirIndex.ToString() + ".");
            }
            texAssembly.Atlas = current;
            for (int i = 3; i < entrySegment.Length; i++)
            {
                if (entrySegment[i].ToUpper().StartsWith("DIV:"))
                {
                    String[] divisions = entrySegment[i].Remove(0, 4).Split(',');
                    int x = 1;
                    int y = 1;
                    try
                    {
                        x = Convert.ToInt32(divisions[0]);
                        y = Convert.ToInt32(divisions[1]);
                    }
                    catch (FormatException) { throw new ManifestReaderException("Manifest is invalid: Texture division sizes are in an incorrect format."); }
                    texAssembly.DivDimensions = new Point(x, y);
                }
                else if (entrySegment[i].ToUpper().StartsWith("FL:"))
                {
                    if(texAssembly.FrameLookup == null) { texAssembly.FrameLookup = new Hashtable(); }
                    String[] flParams = entrySegment[i].Remove(0, 3).Split(':');
                    object frameKey = EntityFactory.ParseRealData(flParams[0]);
                    String[] frameDivs = flParams[1].Split(',');
                    int x;
                    int y;
                    try
                    {
                        x = Convert.ToInt32(frameDivs[0]);
                        y = Convert.ToInt32(frameDivs[1]);
                    }
                    catch (FormatException) { throw new ManifestReaderException("Manifest is invalid: Texture division sizes are in an incorrect format."); }
                    texAssembly.FrameLookup.Add(frameKey, new Point(x, y));
                }
            }
            return texAssembly;
        }
        public static Boolean IngestScriptFile(String filename)
        {
            if (filename.EndsWith(".esa")) { filename = filename.Remove(filename.Length - 4); }
            DirectoryInfo scriptArchive = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Blackhole Media Systems\\Event Horizon Framework\\appdata\\scripts");
            if (scriptArchive.Exists)
            {
                foreach (FileInfo fileInfo in scriptArchive.EnumerateFiles())
                {
                    String fileName = fileInfo.Name;
                    if (fileName.EndsWith(".esa")) { fileName = fileName.Remove(fileName.Length - 4); }
                    if (fileName.ToUpper() == filename.ToUpper())
                    {
                        StreamReader reader = new StreamReader(fileInfo.OpenRead());
                        String scriptContent = reader.ReadToEnd();
                        reader.Close();
                        Dictionary<string, object[]> extractedScripts = new Dictionary<string, object[]>();
                        try
                        {
                            extractedScripts = ScriptProcessor.ExtractEventScriptArchive(scriptContent);
                        }
                        catch(Exception e)
                        {
                            throw new ManifestReaderException("Could not extract EventScriptArchive file \"" + fileName + "\". Check for a malformed script! Error: " + e.Message + e.StackTrace);
                        }
                        foreach (String key in extractedScripts.Keys)
                        {
                            ScriptProcessor.ScriptCache.Add(key, extractedScripts[key]);
                            Shell.WriteLine("Added script " + key + " to cache.");
                        }
                        return true;
                    }
                }
                return false;
            }
            else { return false; }
        }
    }
}
