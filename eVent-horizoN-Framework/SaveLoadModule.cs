using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace VNFramework
{
    public static class SaveLoadModule
    {
        public class SaveLoadException : Exception
        {
            public SaveLoadException(String Arg) : base(Arg)
            { }
        }
        static public void PullOrInitPersistentState()
        {
            FileInfo PSArchive = new FileInfo(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Blackhole Media Systems\\Event Horizon Framework\\persistence\\settingsstates.eha");
            if(PSArchive.Exists)
            {
                BinaryReader Reader = new BinaryReader(PSArchive.OpenRead());
                String State = Reader.ReadString();
                while(State.Contains("<"))
                {
                    State = State.Remove(0, State.IndexOf("<") + 1);
                    String CurrentAttr = State.Remove(State.IndexOf(">"));
                    State = State.Remove(0, State.IndexOf(">") + 1);
                    String[] Attr = CurrentAttr.Split('=');
                    switch(Attr[0])
                    {
                        case "globalvolume":
                            float NV = Convert.ToSingle(Attr[1]);
                            Shell.GlobalVolume = NV;
                            break;
                        case "globalmute":
                            if(Attr[1] == "true") { Shell.Mute = true; }
                            else if(Attr[1] == "false") { Shell.Mute = false; }
                            break;
                        case "tickwriteinterval":
                            int TR = Convert.ToInt32(Attr[1]);
                            TextEntity.TickWriteInterval = TR;
                            break;
                        case "applicablesavetype":
                            ApplicableSaveType = Attr[1];
                            break;
                        case "isfullscreen":
                            if (Attr[1] == "true" && Shell.QueryFullscreen() == false) { Shell.ToggleFullscreen(); }
                            else if (Attr[1] == "false" && Shell.QueryFullscreen() == true) { Shell.ToggleFullscreen(); }
                            break;
                    }
                }
            }
            else { WritePersistentState(); }
        }
        static public void WriteFinals()
        {
            WritePersistentState();
            WriteSessionLog();
        }
        static public void WriteSessionLog()
        {
            using (StreamWriter Writer = new StreamWriter(new FileStream(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Blackhole Media Systems\\Event Horizon Framework\\Previous Session Log.txt", FileMode.Create, FileAccess.Write)))
            {
                foreach (object[] o in Shell.InternalLog)
                {
                    foreach (object RealObj in o)
                    {
                        if (RealObj is String) { Writer.Write(RealObj); }
                    }
                    Writer.Write("\r\n");
                }
                Writer.Close();
            }
        }
        static public void WritePersistentState()
        {
            String StateRecord = "Event Horizon Framework States Archive (Event Horizon Archive File) \nFile last updated at " + System.DateTime.Now.ToShortTimeString() + " " + System.DateTime.Now.ToShortDateString() + " \n"
                + "<globalvolume=" + Shell.GlobalVolume + "> \n"
                + "<globalmute=" + Shell.Mute.ToString().ToLower() + ">\n"
                + "<tickwriteinterval=" + TextEntity.TickWriteInterval + ">\n"
                + "<applicablesavetype=" + ApplicableSaveType + "> \n"
                + "<isfullscreen=" + Shell.QueryFullscreen().ToString().ToLower() + "> \n";
            BinaryWriter Writer = new BinaryWriter(new FileStream(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Blackhole Media Systems\\Event Horizon Framework\\persistence\\settingsstates.eha", FileMode.Create));
            Writer.Write(StateRecord);
            Writer.Close();
        }
        static public void InitializeAppFolders()
        {
            String LocalDataStore = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            DirectoryInfo Dir = new DirectoryInfo(Path.Combine(LocalDataStore, "Blackhole Media Systems"));
            if (!Dir.Exists) { Dir.Create(); }
            Dir = new DirectoryInfo(Path.Combine(Dir.FullName, "Event Horizon Framework"));
            if (!Dir.Exists) { Dir.Create(); }
            String EHFFolder = Dir.FullName;
            Dir = new DirectoryInfo(Path.Combine(EHFFolder, "persistence"));
            if (!Dir.Exists) { Dir.Create(); }
            Dir = new DirectoryInfo(Path.Combine(EHFFolder, "savedata"));
            if (!Dir.Exists) { Dir.Create(); }
            String SaveDatFolder = Dir.FullName;
            Dir = new DirectoryInfo(Path.Combine(SaveDatFolder, "saves"));
            if (!Dir.Exists) { Dir.Create(); }
            Dir = new DirectoryInfo(Path.Combine(SaveDatFolder, "thumbs"));
            if (!Dir.Exists) { Dir.Create(); }
            AutoRenumberSaveFiles();
            /*Dir = new DirectoryInfo(Path.Combine(EHFFolder, "appdata"));
            if (!Dir.Exists) { Dir.Create(); }
            String AppDatFolder = Dir.FullName;
            Dir = new DirectoryInfo(Path.Combine(AppDatFolder, "scripts"));
            if (!Dir.Exists) { Dir.Create(); }
            Dir = new DirectoryInfo(Path.Combine(AppDatFolder, "appmanifests"));
            if (!Dir.Exists) { Dir.Create(); }*/
        }
        static public void AutoRenumberSaveFiles()
        {
            try
            {
                String SaveDirectoryPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Blackhole Media Systems\\Event Horizon Framework\\savedata\\saves";
                DirectoryInfo SaveFolder = new DirectoryInfo(SaveDirectoryPath);
                FileInfo[] Saves = SaveFolder.GetFiles();
                if(Saves.Length == 0) { return; }
                Array.Sort<FileInfo>(Saves, ((a, b) => a.CreationTime.CompareTo(b.CreationTime)));
                int i = 1;
                foreach(FileInfo S in Saves)
                {
                    String TrueName = S.Name.Remove(0, S.Name.IndexOf('_') + 1);
                    S.MoveTo(SaveDirectoryPath + "\\temp_" + i + "_" + TrueName);
                    i++;
                }
                SaveFolder.Refresh();
                Saves = SaveFolder.GetFiles();
                foreach (FileInfo S in Saves)
                {
                    String TrueName = S.Name.Remove(0, S.Name.IndexOf('_') + 1);
                    S.MoveTo(SaveDirectoryPath + "\\" + TrueName);
                }
            }
            catch(Exception E) { throw (new SaveLoadException("Error attempting to renumber files in the saves directory: \n" + E.Message + "\n" + E.StackTrace)); }
        }
        static public Texture2D PopulateLoadSlot(String ImagePath)
        {
            System.Drawing.Image img = System.Drawing.Image.FromFile(ImagePath);
            MemoryStream ms = new MemoryStream();
            img.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            Texture2D Thumb = Texture2D.FromStream(Shell.PubGD, ms);
            ms.Close();
            Texture2D ButtonBase = ((TAtlasInfo)Shell.AtlasDirectory["SAVESLOT"]).Atlas;
            SpriteBatch spriteBatch = new SpriteBatch(Shell.PubGD);
            RenderTarget2D OutButton = new RenderTarget2D(Shell.PubGD, 1020, 200, false,
                Shell.PubGD.PresentationParameters.BackBufferFormat,
                DepthFormat.Depth24);
            Shell.PubGD.SetRenderTarget(OutButton);
            Shell.PubGD.Clear(Color.Transparent);
            spriteBatch = new SpriteBatch(Shell.PubGD);
            spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
            spriteBatch.Draw(ButtonBase, new Rectangle(0, 0, 1020, 200), Color.White);
            spriteBatch.Draw(Thumb, new Rectangle(10, 10, 320, 180), Color.White);
            spriteBatch.Draw(Thumb, new Rectangle(350, 10, 320, 180), Color.White);
            spriteBatch.Draw(Thumb, new Rectangle(690, 10, 320, 180), Color.White);
            spriteBatch.End();
            return OutButton;
        }
        static public Texture2D GenerateSaveThumb()
        {
            String[] ExcludeEnts = new String[] { "PAUSE_PANE", "BUTTON_PAUSE_RETURN", "BUTTON_PAUSE_SAVE", "BUTTON_PAUSE_SETTINGS", "BUTTON_PAUSE_MAINMENU", "BUTTON_PAUSE_QUIT" };
            RenderTarget2D ThumbPane = new RenderTarget2D(Shell.PubGD, 1280, 720, false,
                Shell.PubGD.PresentationParameters.BackBufferFormat,
                DepthFormat.Depth24);
            Shell.PubGD.SetRenderTarget(ThumbPane);
            Shell.PubGD.Clear(Color.Transparent);
            SpriteBatch spriteBatch = new SpriteBatch(Shell.PubGD);
            spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
            foreach (WorldEntity E in Shell.RenderQueue)
            {
                if (E.Drawable && !ExcludeEnts.Contains(E.Name)) { E.Draw(spriteBatch); }
            }
            spriteBatch.End();
            RenderTarget2D RealThumb = new RenderTarget2D(Shell.PubGD, 320, 180, false,
                Shell.PubGD.PresentationParameters.BackBufferFormat,
                DepthFormat.Depth24);
            Shell.PubGD.SetRenderTarget(RealThumb);
            Shell.PubGD.Clear(Color.Black);
            spriteBatch = new SpriteBatch(Shell.PubGD);
            spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
            spriteBatch.Draw(ThumbPane, new Rectangle(0, 0, 320, 180), Color.White);
            spriteBatch.End();
            return RealThumb;
        }
        static public int WriteSave()
        {
            return WriteSave(null);
        }
        static public int WriteSave(Texture2D Thumb)
        {
            String SaveDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Blackhole Media Systems\\Event Horizon Framework\\savedata\\saves";
            String ThumbDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Blackhole Media Systems\\Event Horizon Framework\\savedata\\thumbs";
            if(Thumb is null) { Thumb = GenerateSaveThumb(); }
            Texture2D ThumbData = Thumb;
            String SName = "UNKNOWN";
            foreach (WorldEntity E in Shell.UpdateQueue)
            {
                if (E is ScriptProcessor.ScriptSniffer)
                {
                    SName = E.Name.Replace("_SNIFFER", "");
                    break;
                }
            }
            DirectoryInfo SaveFolder = new DirectoryInfo(SaveDirectory);
            int CurSaveNum = SaveFolder.GetFiles().Length;
            String SaveName =  (CurSaveNum + 1) + "_" + SName;
            String ThumbName = SaveName + "-" + System.DateTime.Now.Ticks + ".jpg";
            String Path = ThumbDirectory + "\\" + ThumbName;
            String SaveData = GenerateSave(ThumbName, ApplicableSaveType);
            if(SaveData == null) { return 1; }
            FileStream Out = new FileStream(Path, FileMode.CreateNew, FileAccess.ReadWrite);
            ThumbData.SaveAsJpeg(Out, 320, 180);
            Out.Close();
            BinaryWriter Writer = new BinaryWriter(new FileStream(SaveDirectory + "\\" + SaveName + ".ehs", FileMode.Create));
            Writer.Write(SaveData);
            Writer.Close();
            return 0;
        }
        public static String ApplicableSaveType { get; set; }
        public static String RecordApplicableFlags()
        {
            String Out = "";
            String ASE = "TRUE";
            if(!ScriptProcessor.AllowScriptExit) { ASE = "FALSE"; }
            Out += "|ALLOWSCRIPTEXIT-" + ASE + "|";
            return Out;
        }
        static String GenerateSave(String ThumbName, String Type)
        {
            String Save = "EVENT HORIZON ENGINE SAVE FILE\n"
                + "#VERSION=" + Shell.FrameworkVersion + "\n"
                + "#TIME=" + System.DateTime.Now.ToBinary() + "\n"
                + "#THUMB=" + ThumbName + "\n"
                + "#SAVETYPE=" + Type + "\n"
                + "#FLAGS=" + RecordApplicableFlags() + "\n";
            if (Type == "ScriptStem")
            {
                ScriptProcessor.ScriptSniffer CurrentSniffer = null;
                Boolean Found = false;
                foreach (WorldEntity E in Shell.UpdateQueue)
                {
                    if (E is ScriptProcessor.ScriptSniffer)
                    {
                        CurrentSniffer = (ScriptProcessor.ScriptSniffer)E;
                        Found = true;
                        break;
                    }
                }
                if (!Found) { throw new SaveLoadException("Error when saving the game - no active script!"); }
                Save += "#SCRIPTNAME=" + CurrentSniffer.Name.Replace("_SNIFFER", "") + "\n";
                Save += "#SCRIPTSHIFTINDEX=" + CurrentSniffer.Index + "\n";
                Save += "#CONDITIONALS=";
                switch (CurrentSniffer.Name.Replace("_SNIFFER", "").ToUpper())
                {
                    default:
                        Save += "DEFAULTUI";
                        break;
                    case "VS_MAIN_INTRO":
                        Save += "NULL";
                        break;
                }
            }
            else if(Type == "FullySerializedBinary")
            {
                if(ScriptProcessor.PastStates.Count == 0)
                {
                    Shell.WriteLine("Could not generate save file: No valid RecallableState stored.");
                    return null;
                }
                RecallableState? State = (RecallableState?)ScriptProcessor.PastStates.Peek();
                if(State is null)
                {
                    Stack StatesClone = (Stack)ScriptProcessor.PastStates.Clone();
                    StatesClone.Pop();
                    while (State is null)
                    {
                        if(StatesClone.Count == 0)
                        {
                            Shell.WriteLine("Could not generate save file: No valid RecallableState stored.");
                            return null;
                        }
                        State = (RecallableState?)StatesClone.Pop();
                    }
                }
                IFormatter SerFormatter = new BinaryFormatter();
                ArrayList Streams = new ArrayList();
                MemoryStream EntityStream = new MemoryStream();
                SerFormatter.Serialize(EntityStream, State);
                EntityStream.Close();
                byte[] Bin = EntityStream.ToArray();
                String BinString = Convert.ToBase64String(Bin);
                Save += "#DATASTREAM=" + BinString + "&ENDDATASTREAM";
            }
            Save += "\n";
            Save += "END";
            return Save;
        }
        public static void Load(String Save)
        {
            String SH = "";
            if(Save.Contains("#DATASTREAM="))
            {
                SH = Save.Remove(0, Save.IndexOf("#DATASTREAM=") + 12);
                SH = SH.Remove(SH.IndexOf("&ENDDATASTREAM"));
                Save = Save.Replace(SH, "");
            }
            String[] SaveAttributes = Save.Split('\n');
            Hashtable AttrIndex = new Hashtable();
            foreach(String A in SaveAttributes)
            {
                if(A.Contains('='))
                {
                    AttrIndex.Add(A.Split('=')[0], A.Split('=')[1]);
                }
            }
            if(AttrIndex.ContainsKey("#DATASTREAM")) { AttrIndex["#DATASTREAM"] = SH; }
            if(AttrIndex.ContainsKey("#SAVETYPE"))
            {
                switch(AttrIndex["#SAVETYPE"])
                {
                    case "ScriptStem":
                        LoadScriptStemSave(AttrIndex);
                        break;
                    case "FullySerializedBinary":
                        LoadFSBSave(AttrIndex);
                        break;
                    default:
                        throw new SaveLoadException("Error parsing save file: Unknown save format was specified.");
                }
            }
            else { throw new SaveLoadException("Error parsing save file: File format not specified."); }
            if(AttrIndex.ContainsKey("#FLAGS"))
            {
                String FlagRestore = (String)AttrIndex["#FLAGS"];
                String[] FlagList = FlagRestore.Split('|');
                foreach(String F in FlagList)
                {
                    if(F.Length > 0 && F.Contains('-'))
                    {
                        String[] RealFlag = F.Split('-');
                        switch(RealFlag[0])
                        {
                            case "KINGFLAG":
                                Sofia.KingFlag = Convert.ToByte(RealFlag[1]);
                                break;
                            case "CROOKEDFLAG":
                                Sofia.CrookedFlag = Convert.ToByte(RealFlag[1]);
                                break;
                            case "MYSTICFLAG":
                                Sofia.MysticFlag = Convert.ToByte(RealFlag[1]);
                                break;
                            case "ALLOWSCRIPTEXIT":
                                ScriptProcessor.AllowScriptExit = (RealFlag[1] == "FALSE" ? false : true);
                                break;
                        }
                    }
                    else { continue; }
                }
            }
        }
        static void LoadScriptStemSave(Hashtable AttrIndex)
        {
            String ScriptName = (String)AttrIndex["#SCRIPTNAME"];
            int SIndex = Convert.ToInt32((String)AttrIndex["#SCRIPTSHIFTINDEX"]);
            String[] Conditionals = ((String)AttrIndex["#CONDITIONALS"]).Split(',');
            Shell.UpdateQueue = new List<WorldEntity>();
            Shell.RenderQueue = new List<WorldEntity>();
            foreach (String C in Conditionals)
            {
                switch(C)
                {
                    case "DEFAULTUI":
                        ButtonScripts.InitDefaultUI();
                        break;
                }
            }
            ScriptProcessor.ScriptSniffer LoadSniffer = new ScriptProcessor.ScriptSniffer(ScriptName.ToUpper() + "_SNIFFER", ScriptProcessor.RetrieveScriptByName(ScriptName), ScriptName, true);
            LoadSniffer.SetLoadBreaker(SIndex);
            Shell.UpdateQueue.Add(LoadSniffer);
        }
        static void LoadFSBSave(Hashtable AttrIndex)
        {
            String Dat = (String)AttrIndex["#DATASTREAM"];
            byte[] SBytes = Convert.FromBase64String(Dat);
            IFormatter SerFormatter = new BinaryFormatter();
            MemoryStream DecodeStream = new MemoryStream(SBytes);
            RecallableState R = (RecallableState)SerFormatter.Deserialize(DecodeStream);
            DecodeStream.Close();
            Shell.DeserializeState(R, true);
            Shell.HoldRender = false;
        }
    }
}
