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
using Tileset = VNFramework.GraphicsTools.TileRenderer.Tileset;

namespace VNFramework
{
    public static class ManifestReader
    {
        public class ManifestReaderException : VNFUtils.EventHorizonException
        {
            public ManifestReaderException(String arg) : base(arg)
            { }
        }
        public static Dictionary<string, string> ReadManifestFile(String fileName)
        {
            Shell.WriteLine("Reading from manifest: " + fileName);
            if (fileName.EndsWith(".ehm")) { fileName = fileName.Remove(fileName.Length - 4); }
            DirectoryInfo manifestDir = new DirectoryInfo("Content\\Data\\appdata\\appmanifests");
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
            Dictionary<object, Tileset> tilesetDirectory = new Dictionary<object, Tileset>();
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
                    case "TILESET":
                        dirIndex = EntityFactory.ParseRealData(entrySegment[1]);
                        tilesetDirectory.Add(dirIndex, ParseTileset(entrySegment, atlasDirectory));
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
            return new object[] { metaDirectory, scriptDirectory, fontDirectory, sfxDirectory, songDirectory, atlasDirectory, tilesetDirectory };
        }
        private static Tileset ParseTileset(String[] entrySegment, Dictionary<object, TAtlasInfo> atlasDirectory)
        {
            Tileset tileset = new Tileset();
            object dirIndex = EntityFactory.ParseRealData(entrySegment[1]);
            if (!(dirIndex is String || dirIndex is string)) { throw new ManifestReaderException("Manifest is invalid: Tileset must be String indexed at point of load."); }
            for (int i = 2; i < entrySegment.Length; i++)
            {
                float x = 1;
                float y = 1;
                if (entrySegment[i].ToUpper().StartsWith("TEX_ATL:"))
                {
                    object atlIndex = EntityFactory.ParseRealData(entrySegment[i].Remove(0, 8));
                    if(atlasDirectory.ContainsKey(atlIndex))
                    {
                        tileset.TileAtlas = atlasDirectory[atlIndex];
                    }
                    else
                    {
                        throw new ManifestReaderException("Manifest is invalid: Invalid texture identifier when building Tileset " + dirIndex.ToString() + ".");
                    }
                }
                else if (entrySegment[i].ToUpper().StartsWith("TILE_OFFSET:"))
                {
                    String offsets = entrySegment[i].Remove(0, 12).Replace("#", "").Replace('[', '#').Replace(']', '#');
                    String[] offsetsXY = VNFUtils.Strings.SplitAtExclosed(offsets, ',', '#');
                    if(offsetsXY.Length != 2) { throw new ManifestReaderException("Manifest is invalid: Tiling offsets must be defined as a two variable sequence."); }
                    Vector2[][] trueOffsets = new Vector2[2][];
                    try
                    {
                        for(int j = 0; j < 2; j++)
                        {
                            String[] vectors = offsetsXY[j].Split(':');
                            trueOffsets[j] = new Vector2[vectors.Length];
                            int k = 0;
                            foreach (String vector in vectors)
                            {
                                String[] offset = vector.Split(',');
                                x = Convert.ToSingle(offset[0]);
                                y = Convert.ToSingle(offset[1]);
                                trueOffsets[j][k] = new Vector2(x, y);
                                k++;
                            }
                        }
                    }
                    catch (FormatException) { throw new ManifestReaderException("Manifest is invalid: Tileset offsets are in an incorrect format."); }
                    tileset.TileOffset = trueOffsets;
                }
                else if (entrySegment[i].ToUpper().StartsWith("TILE_ORIGIN:"))
                {
                    String[] origin = entrySegment[i].Remove(0, 12).Split(',');
                    try
                    {
                        x = Convert.ToSingle(origin[0]);
                        y = Convert.ToSingle(origin[1]);
                    }
                    catch (FormatException) { throw new ManifestReaderException("Manifest is invalid: Tileset origin is in an incorrect format."); }
                    tileset.TileOrigin = new Vector2(x, y);
                }
                else if (entrySegment[i].ToUpper().StartsWith("TILE_SCALING:"))
                {
                    String[] scale = entrySegment[i].Remove(0, 13).Split(',');
                    try
                    {
                        x = Convert.ToSingle(scale[0]);
                        y = Convert.ToSingle(scale[1]);
                    }
                    catch (FormatException) { throw new ManifestReaderException("Manifest is invalid: Tileset scaling is in an incorrect format."); }
                    tileset.TileOrigin = new Vector2(x, y);
                }
                else if (entrySegment[i].ToUpper().StartsWith("TILE_TINT:"))
                {
                    String[] col = entrySegment[i].Remove(0, 10).Split(',');
                    int[] colourChannels = new int[4];
                    try
                    {
                        for(int ii = 0; ii < colourChannels.Length; ii++)
                        {
                            colourChannels[ii] = Convert.ToInt32(col[ii]);
                        }
                    }
                    catch (FormatException) { throw new ManifestReaderException("Manifest is invalid: Tileset tint is in an incorrect format."); }
                    tileset.Tint = new Color(colourChannels[0], colourChannels[1], colourChannels[2], colourChannels[3]);
                }
                else if (entrySegment[i].ToUpper().StartsWith("TILE_BG:"))
                {
                    String[] col = entrySegment[i].Remove(0, 8).Split(',');
                    int[] colourChannels = new int[4];
                    try
                    {
                        for (int ii = 0; ii < colourChannels.Length; ii++)
                        {
                            colourChannels[ii] = Convert.ToInt32(col[ii]);
                        }
                    }
                    catch (FormatException) { throw new ManifestReaderException("Manifest is invalid: Tileset background colour is in an incorrect format."); }
                    tileset.Background = new Color(colourChannels[0], colourChannels[1], colourChannels[2], colourChannels[3]);
                }
                else if (entrySegment[i].ToUpper().StartsWith("TL:"))
                {
                    String[] tlParams = entrySegment[i].Remove(0, 3).Split(':');
                    int tileIndex = 0;
                    int x2 = 0;
                    int y2 = 0;
                    try
                    {
                        tileIndex = Convert.ToInt32(tlParams[0]);
                        String[] tileDivs = tlParams[1].Split(',');
                        x2 = Convert.ToInt32(tileDivs[0]);
                        y2 = Convert.ToInt32(tileDivs[1]);
                    }
                    catch (FormatException) { throw new ManifestReaderException("Manifest is invalid: Tile index mappings are in an incorrect format."); }
                    tileset.TileLookup.Add(tileIndex, new Point(x2, y2));
                }
            }
            if (tileset.TileAtlas is null) { throw new ManifestReaderException("Manifest is invalid: Tileset " + dirIndex.ToString() + " did not specify a texture atlas."); }
            if (tileset.TileLookup.Count == 0) { throw new ManifestReaderException("Manifest is invalid: Tileset " + dirIndex.ToString() + " has no tile index mappings."); }
            return tileset;
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
            DirectoryInfo scriptArchive = new DirectoryInfo("Content\\Data\\appdata\\scripts");
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
