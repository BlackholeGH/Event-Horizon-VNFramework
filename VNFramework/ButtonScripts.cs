using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Collections;
using System.Text.RegularExpressions;

namespace VNFramework
{
    public static class ButtonScripts
    {
        public static VoidDel DelegateFetch(String index)
        {
            Hashtable DelegateTable = new Hashtable();
            DelegateTable.Add("uiboxclick", new VoidDel(delegate ()
            {
                if (Shell.AllowEnter) { Shell.DoNextShifter = true; }
            }));
            DelegateTable.Add("openarchive", new VoidDel(delegate ()
            {
                ButtonScripts.OpenArchive();
            }));
            DelegateTable.Add("skip", new VoidDel(delegate ()
            {
                ButtonScripts.Skip();
            }));
            DelegateTable.Add("scriptrollback", new VoidDel(delegate ()
            {
                ButtonScripts.ScriptRollback();
            }));
            DelegateTable.Add("pause", new VoidDel(delegate ()
            {
                ButtonScripts.Pause();
            }));
            DelegateTable.Add("backtomainmenu", new VoidDel(delegate ()
            {
                ButtonScripts.BackToMainMenu();
            }));
            DelegateTable.Add("opennavscreen", new VoidDel(delegate ()
            {
                ButtonScripts.OpenNavScreen();
            }));
            DelegateTable.Add("refreshuihidestate", new VoidDel(delegate ()
            {
                ButtonScripts.RefreshUIHideState();
            }));
            DelegateTable.Add("navigatetomystic", new VoidDel(delegate ()
            {
                Sofia.NavigateToMystic();
            }));
            DelegateTable.Add("navigatetoking", new VoidDel(delegate ()
            {
                Sofia.NavigateToKing();
            }));
            DelegateTable.Add("navigatetocrooked", new VoidDel(delegate ()
            {
                Sofia.NavigateToCrooked();
            }));
            if (index.Contains("runscript_"))
            {
                ScriptProcessor.ScriptSniffer S = ScriptProcessor.SnifferSearch();
                if (S != null) { S.CeaseSkipping(); }
                return new VoidDel(delegate ()
                {
                    ScriptProcessor.PastStates.Clear();
                    ScriptProcessor.ActivateScriptElement("B|" + index.Replace("runscript_", ""));
                });
            }
            else if (index.Contains("setgws_"))
            {
                return new VoidDel(delegate ()
                {
                    Shell.GlobalWorldState = index.Replace("setgws_", "");
                });
            }
            else
            {
                return (VoidDel)DelegateTable[index];
            }
        }
        public static Boolean Paused = false;
        public static Boolean Navigating = false;
        public static class LoadManager
        {
            public static int PageNumber { get; set; }
            public static int MaxPage { get; set; }
            public static String SavePathByIndex(int Index)
            {
                String SaveDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Blackhole Media Systems\\Event Horizon Framework\\savedata\\saves";
                DirectoryInfo SaveDir = new DirectoryInfo(SaveDirectory);
                SortedList SortSaves = new SortedList();
                foreach(FileInfo F in SaveDir.GetFiles())
                {
                    SortSaves.Add(Convert.ToInt32(F.Name.Split('_')[0]), F.FullName);
                }
                int i = 0;
                foreach (int K in SortSaves.Keys)
                {
                    if(i == Index) { return (String)SortSaves[K]; }
                    i++;
                }
                return null;
            }
            public static String[] FetchSaves()
            {
                SortedList SortSaves = new SortedList();
                ArrayList Saves = new ArrayList();
                SaveLoadModule.AutoRenumberSaveFiles();
                String SaveDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Blackhole Media Systems\\Event Horizon Framework\\savedata\\saves";
                DirectoryInfo SaveDir = new DirectoryInfo(SaveDirectory);
                foreach(FileInfo F in SaveDir.GetFiles())
                {
                    BinaryReader Reader = new BinaryReader(F.OpenRead());
                    String Save = Reader.ReadString();
                    SortSaves.Add(Convert.ToInt32(F.Name.Split('_')[0]), Save);
                    Reader.Close();
                }
                foreach(int K in SortSaves.Keys)
                {
                    Saves.Add(SortSaves[K]);
                }
                return Saves.ToArray().Select(x => (String)x).ToArray();
            }
            public static void RefreshSaveLoadPages()
            {
                String[] Saves = LoadManager.FetchSaves();
                LoadManager.MaxPage = (int)Math.Ceiling((Saves.Length / 6f));
                if (LoadManager.MaxPage < 1) { LoadManager.MaxPage = 1; }
                if (LoadManager.PageNumber > LoadManager.MaxPage) { LoadManager.PageNumber = LoadManager.MaxPage; }
            }
            public static Hashtable SaveAccess { get; set; }
            public static Hashtable DeleteAccess { get; set; }
            public static Hashtable DeletePathAccess { get; set; }
            public static void RefreshLoadPage()
            {
                Vector2[] ButtonLocations = new Vector2[6] { new Vector2(280, 150), new Vector2(640, 150), new Vector2(1000, 150), new Vector2(280, 410), new Vector2(640, 410), new Vector2(1000, 410) };
                Boolean Next = false;
                Boolean Prev = false;
                foreach (WorldEntity E in Shell.UpdateQueue)
                {
                    if (!Shell.DeleteQueue.Contains(E) && E.Name != "BACKBUTTON_LOADSCREEN" && E.Name != "TEXT_PAGENUM" && E.Name != "BACKDROP_MAIN")
                    {
                        if(!(E.Name == "PREVBUTTON_LOADSCREEN" && PageNumber > 1) && !(E.Name == "NEXTBUTTON_LOADSCREEN" && PageNumber < MaxPage)) { Shell.DeleteQueue.Add(E); }
                    }
                    if (E.Name == "PREVBUTTON_LOADSCREEN") { Prev = true; }
                    if (E.Name == "NEXTBUTTON_LOADSCREEN") { Next = true; }
                    //TextEntity PNum = new TextEntity("TEXT_PAGENUM", "[C:0-255-0-255]Page 1 of 1", new Vector2(640 - (Shell.Default.MeasureString("Page 1 of 1").X / 2f), 615), 1f);
                    if (E.Name == "TEXT_PAGENUM" && E is TextEntity)
                    {
                        String NT = "Page " + PageNumber + " of " + MaxPage;
                        ((TextEntity)E).Text = "[C:138-0-255-255]" + NT;
                        E.Move(new Vector2((640 - (Shell.Default.MeasureString(NT).X / 2f)) - E.DrawCoords.X, 0));
                    }
                }
                if(PageNumber > 1 && !Prev)
                {
                    Button PrevB = new Button("PREVBUTTON_LOADSCREEN", new Vector2(340, 625), (TAtlasInfo)Shell.AtlasDirectory["PREVBUTTON"], 0.99f, new VoidDel(delegate ()
                    {
                        PageNumber--;
                        RefreshLoadPage();
                    }));
                    Shell.UpdateQueue.Add(PrevB);
                    Shell.RenderQueue.Add(PrevB);
                }
                if (PageNumber < MaxPage && !Next)
                {
                    Button NextB = new Button("NEXTBUTTON_LOADSCREEN", new Vector2(940, 625), (TAtlasInfo)Shell.AtlasDirectory["NEXTBUTTON"], 0.99f, new VoidDel(delegate ()
                    {
                        PageNumber++;
                        RefreshLoadPage();
                    }));
                    Shell.UpdateQueue.Add(NextB);
                    Shell.RenderQueue.Add(NextB);
                }
                String ThumbDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Blackhole Media Systems\\Event Horizon Framework\\savedata\\thumbs";
                String[] MFS = FetchSaves();
                String[] TheseSaves = new String[6];
                DeletePathAccess = new Hashtable();
                int ii = 0;
                for(int i = ((PageNumber-1) * 6); i < MFS.Length; i++)
                {
                    TheseSaves[ii] = MFS[i];
                    ii++;
                    if(ii == 6) { break; }
                }
                WorldEntity[] LoadButtons = new WorldEntity[6];
                ArrayList Dates = new ArrayList();
                ArrayList ContextualDeleters = new ArrayList();
                SaveAccess = new Hashtable();
                DeleteAccess = new Hashtable();
                for (int i = 0; i < 6; i++)
                {
                    if (TheseSaves[i] != null)
                    {
                        String TempTS = TheseSaves[i];
                        if (TempTS.Contains("#DATASTREAM="))
                        {
                            String SH = TempTS.Remove(0, TempTS.IndexOf("#DATASTREAM=") + 12);
                            SH = SH.Remove(SH.IndexOf("&ENDDATASTREAM"));
                            TempTS = TempTS.Replace(SH, "");
                        }
                        String[] Save = TempTS.Split('\n');
                        Hashtable SaveAttributes = new Hashtable();
                        foreach (String S in Save)
                        {
                            if (S.Contains("#DATASTREAM")) { continue; }
                            if (S.Contains('=')) { SaveAttributes.Add(S.Split('=')[0], S.Split('=')[1]); }
                        }
                        String ThumbPath = ThumbDirectory + "\\" + (String)SaveAttributes["#THUMB"];
                        TAtlasInfo ThisButton = new TAtlasInfo();
                        ThisButton.Atlas = SaveLoadModule.PopulateLoadSlot(ThumbPath);
                        ThisButton.DivDimensions = new Point(3, 1);
                        LoadButtons[i] = new Button("LOADSAVEATPOSITION_" + i, ButtonLocations[i], ThisButton, 0.98f, new VoidDel(delegate() { ButtonScripts.TriggerSpecLoad(); }));
                        SaveAccess.Add(LoadButtons[i].EntityID, TheseSaves[i]);

                        String SaveDate = (String)SaveAttributes["#TIME"];
                        long ThenTicks = Convert.ToInt64(SaveDate);
                        DateTime Then = DateTime.FromBinary(ThenTicks);
                        String Time = Then.ToShortDateString() + " " + Then.ToShortTimeString();
                        TextEntity TimeText = new TextEntity("TIMETEXT_" + i, Time, ButtonLocations[i] - new Vector2(Shell.Default.MeasureString(Time).X / 2f, -110), 0.98f);
                        TimeText.TypeWrite = false;
                        Dates.Add(TimeText);

                        Button MyDeleteButton = new Button("DELETESAVEATPOSITION_" + i, ButtonLocations[i], (TAtlasInfo)Shell.AtlasDirectory["DELETESAVEBUTTON"], 0.981f, TriggerSpecDelete, TriggerSpecDeleteHover, TriggerSpecDeleteUnHover);
                        DeleteAccess.Add(MyDeleteButton.EntityID, LoadButtons[i].EntityID);
                        String[] DeletePaths = new String[] { ThumbPath, SavePathByIndex(((PageNumber - 1) * 6) + i), Time };
                        DeletePathAccess.Add(MyDeleteButton.EntityID, DeletePaths);
                        ContextualDeleters.Add(MyDeleteButton);
                    }
                    else
                    {
                        LoadButtons[i] = new WorldEntity("EMPTY_LOADSAVEATPOSITION_" + i, ButtonLocations[i], (TAtlasInfo)Shell.AtlasDirectory["SAVESLOT"], 0.98f);
                        LoadButtons[i].CenterOrigin = true;
                    }
                }
                foreach(WorldEntity E in LoadButtons)
                {
                    Shell.UpdateQueue.Add(E);
                    Shell.RenderQueue.Add(E);
                }
                foreach (WorldEntity E in ContextualDeleters)
                {
                    Shell.UpdateQueue.Add(E);
                    Shell.RenderQueue.Add(E);
                }
                foreach (WorldEntity E in Dates)
                {
                    Shell.UpdateQueue.Add(E);
                    Shell.RenderQueue.Add(E);
                }
            }
        }
        public static void TriggerSpecDeleteHover()
        {
            foreach (WorldEntity E in Shell.UpdateQueue)
            {
                if (LoadManager.DeleteAccess.ContainsKey(E.EntityID) && ((Button)E).ViableClick)
                {
                    Button TargetLoadButton = (Button)Shell.GetEntityByID((ulong)LoadManager.DeleteAccess[E.EntityID]);
                    TargetLoadButton.Enabled = false;
                    TargetLoadButton.AutoUpdateFrameState = false;
                    TargetLoadButton.SetAtlasFrame(new Point(2, 0));
                }
            }
        }
        public static void TriggerSpecDeleteUnHover()
        {
            foreach (WorldEntity E in Shell.UpdateQueue)
            {
                if (LoadManager.DeleteAccess.ContainsKey(E.EntityID) && !((Button)E).ViableClick && ((Button)E).HoverActive)
                {
                    Button TargetLoadButton = (Button)Shell.GetEntityByID((ulong)LoadManager.DeleteAccess[E.EntityID]);
                    TargetLoadButton.Enabled = true;
                    TargetLoadButton.AutoUpdateFrameState = true;
                }
            }
        }
        public static void TriggerSpecDelete()
        {
            foreach (WorldEntity E in Shell.UpdateQueue)
            {
                if (LoadManager.DeleteAccess.ContainsKey(E.EntityID) && ((Button)E).ViableClick)
                {
                    Button TargetLoadButton = (Button)Shell.GetEntityByID((ulong)LoadManager.DeleteAccess[E.EntityID]);
                    if (TargetLoadButton.Enabled == false)
                    {
                        String[] Paths = (String[])LoadManager.DeletePathAccess[E.EntityID];
                        Shell.RunQueue.Add(new VoidDel(delegate () { Delete(TargetLoadButton.Atlas, Paths[0], Paths[1], Paths[2]); }));
                    }
                }
            }
        }
        public static void TriggerSpecLoad()
        {
            MediaPlayer.Stop();
            ScriptProcessor.SongCom = "";
            ScriptProcessor.PastStates.Clear();
            SpoonsTrip = true;
            foreach (WorldEntity E in Shell.UpdateQueue)
            {
                if(E is Button && ((Button)E).ViableClick)
                {
                    ((Button)E).Enabled = false;
                    foreach (WorldEntity E2 in Shell.UpdateQueue)
                    {
                        if (!Shell.DeleteQueue.Contains(E2)) { Shell.DeleteQueue.Add(E2); }
                    }
                    foreach (WorldEntity E2 in Shell.RenderQueue)
                    {
                        if (!Shell.DeleteQueue.Contains(E2)) { Shell.DeleteQueue.Add(E2); }
                    }
                    Shell.HoldRender = true;
                    SaveLoadModule.Load((String)LoadManager.SaveAccess[E.EntityID]);
                    break;
                }
            }
        }
        public static void Delete(TAtlasInfo ThumbAtlas, String ThumbPath, String SavePath, String TimeText)
        {
            foreach (WorldEntity E in Shell.UpdateQueue)
            {
                if (E is Button) { ((Button)E).Enabled = false; }
                if (E is ScrollBar) { ((ScrollBar)E).Enabled = false; }
                if (E is Slider) { ((Slider)E).Enabled = false; }
            }
            WorldEntity SavePane = new WorldEntity("PANE_DELETE", new Vector2(640, 360), (TAtlasInfo)Shell.AtlasDirectory["DELETEPANE"], 0.995f);
            SavePane.CenterOrigin = true;
            Shell.UpdateQueue.Add(SavePane);
            Shell.RenderQueue.Add(SavePane);
            WorldEntity ThumbObj = new WorldEntity("THUMB_DELETE", new Vector2(470, 223), ThumbAtlas, 0.9951f);
            Shell.UpdateQueue.Add(ThumbObj);
            Shell.RenderQueue.Add(ThumbObj);
            TextEntity TimeTextDelete = new TextEntity("TIMETEXT_DELETESAVE", TimeText, new Vector2(640, 323) - new Vector2(Shell.Default.MeasureString(TimeText).X / 2f, -110), 0.9951f);
            TimeTextDelete.TypeWrite = false;
            Shell.UpdateQueue.Add(TimeTextDelete);
            Shell.RenderQueue.Add(TimeTextDelete);
            Button Yes = new Button("BUTTON_DELETE_YES", new Vector2(530, 530), (TAtlasInfo)Shell.AtlasDirectory["YESBUTTON"], 0.9951f, new VoidDel(delegate () { DeleteActual(ThumbPath, SavePath); }));
            Shell.UpdateQueue.Add(Yes);
            Shell.RenderQueue.Add(Yes);
            Button No = new Button("BUTTON_DELETE_NO", new Vector2(750, 530), (TAtlasInfo)Shell.AtlasDirectory["NOBUTTON"], 0.9951f, new VoidDel(delegate () { UnDelete(); }));
            Shell.UpdateQueue.Add(No);
            Shell.RenderQueue.Add(No);
        }
        public static void DeleteActual(String ThumbPath, String SavePath)
        {
            FileInfo Thumb = new FileInfo(ThumbPath);
            FileInfo Save = new FileInfo(SavePath);
            Thumb.Delete();
            Save.Delete();
            LoadManager.RefreshSaveLoadPages();
            LoadManager.RefreshLoadPage();
            foreach (WorldEntity E in Shell.UpdateQueue)
            {
                if (E.Name == "PANE_DELETE" && !Shell.DeleteQueue.Contains(E)) { Shell.DeleteQueue.Add(E); }
                else if (E.Name == "THUMB_DELETE" && !Shell.DeleteQueue.Contains(E)) { Shell.DeleteQueue.Add(E); }
                if (E is TextEntity && E.Name == "TIMETEXT_DELETESAVE" && !Shell.DeleteQueue.Contains(E)) { Shell.DeleteQueue.Add(E); }
                if (E is Button)
                {
                    if (E.Name == "BUTTON_DELETE_YES" && !Shell.DeleteQueue.Contains(E)) { Shell.DeleteQueue.Add(E); }
                    else if (E.Name == "BUTTON_DELETE_NO" && !Shell.DeleteQueue.Contains(E)) { Shell.DeleteQueue.Add(E); }
                    else { ((Button)E).Enabled = false; }
                }
            }
            WorldEntity SaveWrittenPane = new WorldEntity("PANE_SAVEDELETED", new Vector2(640, 300), (TAtlasInfo)Shell.AtlasDirectory["SAVEDELETEDPANE"], 0.996f);
            SaveWrittenPane.CenterOrigin = true;
            Shell.UpdateQueue.Add(SaveWrittenPane);
            Shell.RenderQueue.Add(SaveWrittenPane);
            Button Back = new Button("BUTTON_DELETE_BACK", new Vector2(640, 360), (TAtlasInfo)Shell.AtlasDirectory["BACKBUTTON"], 0.9961f, new VoidDel(delegate () { UnDelete(); }));
            Shell.UpdateQueue.Add(Back);
            Shell.RenderQueue.Add(Back);
        }
        public static void UnDelete()
        {
            foreach (WorldEntity E in Shell.UpdateQueue)
            {
                if (E.Name == "PANE_DELETE" && !Shell.DeleteQueue.Contains(E)) { Shell.DeleteQueue.Add(E); }
                else if (E.Name == "PANE_SAVEDELETED" && !Shell.DeleteQueue.Contains(E)) { Shell.DeleteQueue.Add(E); }
                else if (E.Name == "THUMB_DELETE" && !Shell.DeleteQueue.Contains(E)) { Shell.DeleteQueue.Add(E); }
                if (E is TextEntity && E.Name == "TIMETEXT_DELETESAVE" && !Shell.DeleteQueue.Contains(E)) { Shell.DeleteQueue.Add(E); }
                if (E is Button)
                {
                    if (E.Name == "BUTTON_DELETE_YES" && !Shell.DeleteQueue.Contains(E)) { Shell.DeleteQueue.Add(E); }
                    else if (E.Name == "BUTTON_DELETE_NO" && !Shell.DeleteQueue.Contains(E)) { Shell.DeleteQueue.Add(E); }
                    else if (E.Name == "BUTTON_DELETE_BACK" && !Shell.DeleteQueue.Contains(E)) { Shell.DeleteQueue.Add(E); }
                    else
                    {
                        ((Button)E).Enabled = true;
                    }
                }
            }
        }
        public static Texture2D[] CreateDynamicScroll(String Text, float Width)
        {
            return CreateScrollSequence(Text, new Vector2(Width, -1));
        }
        static Texture2D[] CreateSequenceWithEntity(WorldEntity E, Vector2 ScrollDimensions)
        {
            float EStartY = E.DrawCoords.Y;
            ArrayList ScrollTextures = new ArrayList();
            int UpShift = 0;
            while (UpShift < ScrollDimensions.Y)
            {
                int RealScrollY = (int)ScrollDimensions.Y - UpShift;
                if (RealScrollY > 2000) { RealScrollY = 2000; }
                E.QuickMoveTo(new Vector2(E.DrawCoords.X, EStartY - UpShift));
                RenderTarget2D Output = new RenderTarget2D(Shell.PubGD, (int)ScrollDimensions.X, RealScrollY, false,
                Shell.PubGD.PresentationParameters.BackBufferFormat,
                DepthFormat.Depth24);
                Shell.PubGD.SetRenderTarget(Output);
                Shell.PubGD.Clear(Color.Purple);
                SpriteBatch spriteBatch = new SpriteBatch(Shell.PubGD);
                spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
                E.Draw(spriteBatch);
                spriteBatch.End();
                Shell.PubGD.SetRenderTarget(null);
                ScrollTextures.Add(Output);
                UpShift += 2000;
            }
            return ScrollTextures.ToArray().Select(x => (Texture2D)x).ToArray();
        }
        public static Texture2D[] CreateScrollSequenceFromTexture(Texture2D Scroll, Vector2 ScrollDimensions)
        {
            TAtlasInfo EA = new TAtlasInfo();
            EA.Atlas = Scroll;
            EA.DivDimensions = new Point(1, 1);
            WorldEntity E = new WorldEntity("SCROLL_DRAW_ENTITY", new Vector2(0, 0), EA, 1);
            if (ScrollDimensions.Y == -1)
            {
                ScrollDimensions.Y = E.Atlas.Atlas.Height;
            }
            return CreateSequenceWithEntity(E, ScrollDimensions);
        }
        public static Texture2D[] CreateScrollSequence(String Text, Vector2 ScrollDimensions)
        {
            TextEntity SText = new TextEntity("SCROLL_TEXT", "", new Vector2(20, 20), 1);
            SText.TypeWrite = false;
            SText.BufferLength = (int)ScrollDimensions.X - 40;
            SText.Text = Text;
            if (ScrollDimensions.Y == -1)
            {
                ScrollDimensions.Y = SText.VerticalLength(true) + 40;
            }
            return CreateSequenceWithEntity(SText, ScrollDimensions);
        }
        public static Texture2D CreateScroll(String Text, Vector2 ScrollDimensions)
        {
            TextEntity SText = new TextEntity("SCROLL_TEXT", "", new Vector2(20, 20), 1);
            SText.TypeWrite = false;
            SText.BufferLength = (int)ScrollDimensions.X - 40;
            SText.Text = Text;
            if(ScrollDimensions.Y == -1)
            {
                ScrollDimensions.Y = SText.VerticalLength(true) + 40;
            }
            RenderTarget2D Output = new RenderTarget2D(Shell.PubGD, (int)ScrollDimensions.X, (int)ScrollDimensions.Y, false,
                Shell.PubGD.PresentationParameters.BackBufferFormat,
                DepthFormat.Depth24);
            Shell.PubGD.SetRenderTarget(Output);
            Shell.PubGD.Clear(Color.Purple);
            SpriteBatch spriteBatch = new SpriteBatch(Shell.PubGD);
            spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
            SText.Draw(spriteBatch);
            spriteBatch.End();
            Shell.PubGD.SetRenderTarget(null);
            return (Texture2D)Output;
        }
        public static Texture2D CreateDynamicTextCheckbox(String Text, float Width)
        {
            Texture2D A = CreateDynamicCustomButton(Text, Width);
            Texture2D B = CreateCustomButton(Text, new Vector2(Width, -1), new Color(138, 0, 255, 255), new Color(255, 255, 255, 255), new Color(70, 70, 70, 255));
            Texture2D Out = VNFUtils.CombineTextures(Shell.DefaultShell, new Point(A.Width, A.Height + B.Height), A, A.Bounds, new Vector2(), new Vector2(1, 1), B, B.Bounds, new Vector2(0, A.Height), new Vector2(1, 1));
            return Out;
        }
        public static Texture2D CreateDynamicCustomButton(String Text, float Width)
        {
            return CreateCustomButton(Text, new Vector2(Width, -1), new Color(138, 0, 255, 255), new Color(129, 129, 129, 255), new Color(70, 70, 70, 255));
        }
        public static Texture2D CreateCustomButton(String Text, Vector2 ButtonDimensions, Color ActiveColour, Color BackgroundColour, Color EdgeColour)
        {
            TextEntity SText = new TextEntity("SCROLL_TEXT", "", new Vector2(20, 15), 1);
            SText.TypeWrite = false;
            SText.BufferLength = (int)ButtonDimensions.X - 40;
            SText.Text = "[C:70-70-70-255]" + Text;
            if (ButtonDimensions.Y == -1)
            {
                ButtonDimensions.Y = SText.VerticalLength(true) + 20;
            }
            RenderTarget2D MiddlePane = new RenderTarget2D(Shell.PubGD, (int)ButtonDimensions.X, (int)ButtonDimensions.Y, false,
                Shell.PubGD.PresentationParameters.BackBufferFormat,
                DepthFormat.Depth24);
            Shell.PubGD.SetRenderTarget(MiddlePane);
            Shell.PubGD.Clear(BackgroundColour);
            RenderTarget2D PurplePane = new RenderTarget2D(Shell.PubGD, (int)ButtonDimensions.X + 10, (int)ButtonDimensions.Y + 10, false,
                Shell.PubGD.PresentationParameters.BackBufferFormat,
                DepthFormat.Depth24);
            Shell.PubGD.SetRenderTarget(PurplePane);
            Shell.PubGD.Clear(ActiveColour);
            RenderTarget2D Output = new RenderTarget2D(Shell.PubGD, (int)(ButtonDimensions.X + 10)*2, (int)ButtonDimensions.Y + 10, false,
                Shell.PubGD.PresentationParameters.BackBufferFormat,
                DepthFormat.Depth24);
            Shell.PubGD.SetRenderTarget(Output);
            Shell.PubGD.Clear(EdgeColour);
            SpriteBatch spriteBatch = new SpriteBatch(Shell.PubGD);
            spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
            spriteBatch.Draw(PurplePane, new Rectangle((int)ButtonDimensions.X + 10, 0, (int)ButtonDimensions.X + 10, (int)ButtonDimensions.Y + 10), Color.White);
            spriteBatch.Draw(MiddlePane, new Rectangle(5, 5, (int)ButtonDimensions.X, (int)ButtonDimensions.Y), Color.White);
            spriteBatch.Draw(MiddlePane, new Rectangle((int)ButtonDimensions.X + 15, 5, (int)ButtonDimensions.X, (int)ButtonDimensions.Y), Color.White);
            SText.Draw(spriteBatch);
            SText.Text = "[C:138-0-255-255]" + Text;
            SText.Move(new Vector2(ButtonDimensions.X + 10, 0));
            SText.Draw(spriteBatch);
            spriteBatch.End();
            Shell.PubGD.SetRenderTarget(null);
            Texture2D Out = VNFUtils.GetFromRT(Output);
            Color[] OutModify = new Color[Out.Width * Out.Height];
            Out.GetData<Color>(OutModify);
            Color Prev = BackgroundColour;
            Color New = new Color((int)BackgroundColour.R, (int)BackgroundColour.G, (int)BackgroundColour.B, (int)200);
            for (int i = 0; i < OutModify.Length; i++)
            {
                if(OutModify[i] == Prev) { OutModify[i] = New; }
            }
            Out.SetData<Color>(OutModify);
            return Out;
        }
        public static Button GetQuickButton(String Text, VoidDel Function)
        {
            TAtlasInfo NewAtlas = new TAtlasInfo();
            NewAtlas.Atlas = CreateDynamicCustomButton(Text, 600);
            NewAtlas.DivDimensions = new Point(2, 1);
            Button NewB = new Button("BUTTON_CUSTOM_" + Text.ToUpper(), new Vector2(), NewAtlas, 0.91f, Function);
            return NewB;
        }
        public static void OpenArchive()
        {
            ScriptProcessor.AllowScriptShift = false;
            Shell.AllowEnter = false;
            foreach (WorldEntity E in Shell.UpdateQueue)
            {
                if (E is TextEntity) { E.Drawable = false; }
                if (E is Button) { ((Button)E).Enabled = false; }
            }
            Texture2D[] SB = ScriptProcessor.PullArchive();
            ScrollBar Archive = new ScrollBar("ARCHIVE_SCROLLBAR", new Vector2(1080, 95), (TAtlasInfo)Shell.AtlasDirectory["SCROLLBAR"], 0.98f, SB, 600);
            Archive.JumpTo(1f);
            Shell.UpdateQueue.Add(Archive);
            Shell.RenderQueue.Add(Archive);
            Button Back = new Button("BACKBUTTON_ARCHIVE", new Vector2(1180, 625), (TAtlasInfo)Shell.AtlasDirectory["BACKBUTTON"], 1f, new VoidDel(delegate () { CloseArchive(); }));
            Shell.UpdateQueue.Add(Back);
            Shell.RenderQueue.Add(Back);
        }
        public static void CloseArchive()
        {
            foreach (WorldEntity E in Shell.UpdateQueue)
            {
                if (E is ScrollBar && !Shell.DeleteQueue.Contains(E))
                {
                    Shell.DeleteQueue.Add(E);
                }
                if (E is Button && E.Name == "BACKBUTTON_ARCHIVE" && !Shell.DeleteQueue.Contains(E)) { Shell.DeleteQueue.Add(E); }
                else if (E is Button) { ((Button)E).Enabled = true; }
                if (E is TextEntity) { E.Drawable = true; }
            }
            Shell.AllowEnter = true;
            ScriptProcessor.AllowScriptShift = true;
        }
        public static Boolean UIHideEnabled { get; set; }
        public static void HideUI()
        {
            HideUI(false);
        }
        public static void HideUI(Boolean IncludeHideButton)
        {
            UIHideEnabled = true;
            //ScriptProcessor.AllowScriptShift = false;
            Shell.AllowEnter = false;
            foreach (WorldEntity E in Shell.UpdateQueue)
            {
                if (E.Name == "UIBOX" || E.Name == "NAMELABELBACKING")
                {
                    E.Drawable = false;
                    E.SuppressClickable = true;
                }
                if (E is TextEntity) { E.Drawable = false; }
                if (E is Button && (E.Name != "BUTTON_HIDE_UI" || IncludeHideButton))
                {
                    ((Button)E).Enabled = false;
                    E.Drawable = false;
                }
                if (E is ScrollBar)
                {
                    ((ScrollBar)E).Enabled = false;
                    E.Drawable = false;
                }
            }
        }
        public static void UnHideUI()
        {
            foreach (WorldEntity E in Shell.UpdateQueue)
            {
                if (E.Name == "UIBOX" || E.Name == "NAMELABELBACKING")
                {
                    E.Drawable = true;
                    E.SuppressClickable = false;
                }
                if (E is Button)
                {
                    ((Button)E).Enabled = true;
                    E.Drawable = true;
                }
                if (E is ScrollBar)
                {
                    ((ScrollBar)E).Enabled = true;
                    E.Drawable = true;
                }
                if (E is TextEntity) { E.Drawable = true; }
            }
            Shell.AllowEnter = true;
            //ScriptProcessor.AllowScriptShift = true;
            UIHideEnabled = false;
        }
        public static void OpenNavScreen()
        {
            if (Navigating) { return; }
            if (!ScriptProcessor.AllowScriptExit)
            {
                ScriptProcessor.ScriptSniffer S = ScriptProcessor.SnifferSearch();
                S.ForceInsertScriptElement(new object[] { "C|GWS:CONTINUE", "T||You can't leave the area right now!" });
                return;
            }
            Navigating = true;
            ScriptProcessor.AllowScriptShift = false;
            Shell.AllowEnter = false;
            HideUI(true);
            Vector2[] ButtonLocs = new Vector2[] { new Vector2(400, 200), new Vector2(880, 400) };
            int i = 0;
            if (!(Sofia.GetContextualLocation() == "MYSTIC"))
            {
                Button B = ButtonScripts.GetQuickButton("Travel to the SOURCE, home of the Mystic Sofia.", new VoidDel(delegate ()
                {
                    Shell.GlobalWorldState = "EXITING";
                    CloseNavScreen(true);
                    ((ScriptProcessor.ScriptSniffer)ScriptProcessor.SnifferSearch()).ScriptThrowTarget = Sofia.NavigateToMystic();
                    ScriptProcessor.SearchAndThrowForExit();
                }
                ));
                B.QuickMoveTo(ButtonLocs[i]);
                Shell.UpdateQueue.Add(B);
                Shell.RenderQueue.Add(B);
                i++;
            }
            if (!(Sofia.GetContextualLocation() == "KING"))
            {
                Button B = ButtonScripts.GetQuickButton("Travel to the CASTLE of the King Sofia.", new VoidDel(delegate ()
                {
                    Shell.GlobalWorldState = "EXITING";
                    CloseNavScreen(true);
                    ((ScriptProcessor.ScriptSniffer)ScriptProcessor.SnifferSearch()).ScriptThrowTarget = Sofia.NavigateToKing();
                    ScriptProcessor.SearchAndThrowForExit();
                }
                ));
                B.QuickMoveTo(ButtonLocs[i]);
                Shell.UpdateQueue.Add(B);
                Shell.RenderQueue.Add(B);
                i++;
            }
            if (!(Sofia.GetContextualLocation() == "CROOKED"))
            {
                Button B = ButtonScripts.GetQuickButton("Travel to the hills and caves of the BADLANDS.", new VoidDel(delegate ()
                {
                    Shell.GlobalWorldState = "EXITING";
                    CloseNavScreen(true);
                    ((ScriptProcessor.ScriptSniffer)ScriptProcessor.SnifferSearch()).ScriptThrowTarget = Sofia.NavigateToCrooked();
                    ScriptProcessor.SearchAndThrowForExit();
                }
                ));
                B.QuickMoveTo(ButtonLocs[i]);
                Shell.UpdateQueue.Add(B);
                Shell.RenderQueue.Add(B);
                i++;
            }
            Button Back = new Button("BACKBUTTON_NAVSCREEN", new Vector2(1110, 610), (TAtlasInfo)Shell.AtlasDirectory["BACKBUTTON"], 0.98f, new VoidDel(delegate () { CloseNavScreen(); }));
            Shell.UpdateQueue.Add(Back);
            Shell.RenderQueue.Add(Back);
        }
        public static void CloseNavScreen()
        {
            CloseNavScreen(false);
        }
        public static void CloseNavScreen(Boolean HideCButtons)
        {
            foreach (WorldEntity E in Shell.UpdateQueue)
            {
                if (E.Name == "UIBOX" || E.Name == "NAMELABELBACKING")
                {
                    E.Drawable = true;
                    E.SuppressClickable = false;
                }
                if (E is Button && E.Drawable && !Shell.DeleteQueue.Contains(E)) { Shell.DeleteQueue.Add(E); }
                else if (E is Button) {
                    if (!HideCButtons || !E.Name.Contains("BUTTON_CUSTOM_"))
                    {
                        ((Button)E).Enabled = true;
                        E.Drawable = true;
                    }
                }
                if (E is ScrollBar)
                {
                    ((ScrollBar)E).Enabled = true;
                    E.Drawable = true;
                }
                if (E is TextEntity) { E.Drawable = true; }
            }
            ScriptProcessor.AllowScriptShift = true;
            Shell.AllowEnter = true;
            UIHideEnabled = false;
            Navigating = false;
        }
        public static void Pause()
        {
            if(Paused) { return; }
            if(Navigating) { CloseNavScreen(); }
            Paused = true;
            ScriptProcessor.AllowScriptShift = false;
            Shell.AllowEnter = false;
            foreach (WorldEntity E in Shell.UpdateQueue)
            {
                //if (E is TextEntity) { E.Drawable = false; }
                if (E is Button) { ((Button)E).Enabled = false; }
                if (E is ScrollBar) { ((ScrollBar)E).Enabled = false; }
            }
            WorldEntity Pane = new WorldEntity("PAUSE_PANE", new Vector2(640, 360), (TAtlasInfo)Shell.AtlasDirectory["PAUSEMENUPANE"], 0.97f);
            Pane.CenterOrigin = true;
            Pane.ColourValue = new Color(200, 200, 200, 150);
            Shell.UpdateQueue.Add(Pane);
            Shell.RenderQueue.Add(Pane);
            Button Back = new Button("BUTTON_PAUSE_RETURN", new Vector2(640, 180), (TAtlasInfo)Shell.AtlasDirectory["PAUSERETURNBUTTON"], 0.98f, new VoidDel(delegate () { Unpause(); }));
            Shell.UpdateQueue.Add(Back);
            Shell.RenderQueue.Add(Back);
            Button SaveB = new Button("BUTTON_PAUSE_SAVE", new Vector2(640, 270), (TAtlasInfo)Shell.AtlasDirectory["PAUSESAVEBUTTON"], 0.98f, new VoidDel(delegate () { Save(); }));
            Shell.UpdateQueue.Add(SaveB);
            Shell.RenderQueue.Add(SaveB);
            Button Settings = new Button("BUTTON_PAUSE_SETTINGS", new Vector2(640, 360), (TAtlasInfo)Shell.AtlasDirectory["PAUSESETTINGSBUTTON"], 0.98f, new VoidDel(delegate () { ShowSettings(); }));
            Shell.UpdateQueue.Add(Settings);
            Shell.RenderQueue.Add(Settings);
            Button Main = new Button("BUTTON_PAUSE_MAINMENU", new Vector2(640, 450), (TAtlasInfo)Shell.AtlasDirectory["PAUSEMAINMENUBUTTON"], 0.98f, new VoidDel(delegate () { BackToMainMenu(); }));
            Shell.UpdateQueue.Add(Main);
            Shell.RenderQueue.Add(Main);
            Button QuitB = new Button("BUTTON_PAUSE_QUIT", new Vector2(640, 540), (TAtlasInfo)Shell.AtlasDirectory["PAUSEQUITBUTTON"], 0.98f, new VoidDel(delegate () { Quit(); }));
            Shell.UpdateQueue.Add(QuitB);
            Shell.RenderQueue.Add(QuitB);
        }
        public static void Unpause()
        {
            foreach (WorldEntity E in Shell.UpdateQueue)
            {
                if(E.Name == "PAUSE_PANE" && !Shell.DeleteQueue.Contains(E)) { Shell.DeleteQueue.Add(E); }
                if (E is Button && E.Name == "BUTTON_PAUSE_RETURN" && !Shell.DeleteQueue.Contains(E)) { Shell.DeleteQueue.Add(E); }
                else if (E is Button && E.Name == "BUTTON_PAUSE_SAVE" && !Shell.DeleteQueue.Contains(E)) { Shell.DeleteQueue.Add(E); }
                else if (E is Button && E.Name == "BUTTON_PAUSE_SETTINGS" && !Shell.DeleteQueue.Contains(E)) { Shell.DeleteQueue.Add(E); }
                else if (E is Button && E.Name == "BUTTON_PAUSE_MAINMENU" && !Shell.DeleteQueue.Contains(E)) { Shell.DeleteQueue.Add(E); }
                else if (E is Button && E.Name == "BUTTON_PAUSE_QUIT" && !Shell.DeleteQueue.Contains(E)) { Shell.DeleteQueue.Add(E); }
                else if (E is Button) { ((Button)E).Enabled = true; }
                if (E is ScrollBar) { ((ScrollBar)E).Enabled = true; }
                //if (E is TextEntity) { E.Drawable = true; }
            }
            Shell.AllowEnter = true;
            ScriptProcessor.AllowScriptShift = true;
            Paused = false;
        }
        public static void Quit()
        {
            ScriptProcessor.AllowScriptShift = false;
            Shell.AllowEnter = false;
            foreach (WorldEntity E in Shell.UpdateQueue)
            {
                if (E is Button) { ((Button)E).Enabled = false; }
                if (E is ScrollBar) { ((ScrollBar)E).Enabled = false; }
                if (E is Slider) { ((Slider)E).Enabled = false; }
            }
            WorldEntity Pane = new WorldEntity("EXIT_PANE", new Vector2(640, 360), (TAtlasInfo)Shell.AtlasDirectory["EXITPANE"], 0.995f);
            Pane.CenterOrigin = true;
            Shell.UpdateQueue.Add(Pane);
            Shell.RenderQueue.Add(Pane);
            Button Yes = new Button("BUTTON_EXIT_YES", new Vector2(560, 380), (TAtlasInfo)Shell.AtlasDirectory["QUITYESBUTTON"], 1f, new VoidDel(delegate () { Shell.ExitOut = true; }));
            Shell.UpdateQueue.Add(Yes);
            Shell.RenderQueue.Add(Yes);
            Button No = new Button("BUTTON_EXIT_NO", new Vector2(700, 380), (TAtlasInfo)Shell.AtlasDirectory["QUITNOBUTTON"], 1f, new VoidDel(delegate () { Unquit(); }));
            Shell.UpdateQueue.Add(No);
            Shell.RenderQueue.Add(No);
        }
        public static void Unquit()
        {
            foreach (WorldEntity E in Shell.UpdateQueue)
            {
                if (E.Name == "EXIT_PANE" && !Shell.DeleteQueue.Contains(E)) { Shell.DeleteQueue.Add(E); }
                if (E is Button && E.Name == "BUTTON_EXIT_YES" && !Shell.DeleteQueue.Contains(E)) { Shell.DeleteQueue.Add(E); }
                else if (E is Button && E.Name == "BUTTON_EXIT_NO" && !Shell.DeleteQueue.Contains(E)) { Shell.DeleteQueue.Add(E); }
                if (!Paused)
                {
                    if (E is Button) { ((Button)E).Enabled = true; }
                    if (E is ScrollBar) { ((ScrollBar)E).Enabled = true; }
                    if (E is Slider) { ((Slider)E).Enabled = true; }
                }
                else
                {
                    if (E.Name == "BUTTON_PAUSE_RETURN" || E.Name == "BUTTON_PAUSE_SAVE" || E.Name == "BUTTON_PAUSE_SETTINGS" || E.Name == "BUTTON_PAUSE_MAINMENU" || E.Name == "BUTTON_PAUSE_QUIT")
                    {
                        ((Button)E).Enabled = true;
                    }
                }
            }
            if(!Paused)
            {
                Shell.AllowEnter = true;
                ScriptProcessor.AllowScriptShift = true;
            }
        }
        public static void ScriptRollback()
        {
            if (ScriptProcessor.ActiveGame())
            {
                if (ScriptProcessor.PastStates.Count > 1)
                {
                    ScriptProcessor.RollbackArchive();
                    Shell.GlobalWorldState = "PERFORMED SCRIPT ROLLBACK...";
                }
                if (ScriptProcessor.PastStates.Count > 1)
                {
                    ScriptProcessor.PastStates.Pop();
                }
                else if (ScriptProcessor.PastStates.Count == 0) { return; }
                Shell.GlobalVoid = new VoidDel(delegate ()
                {
                    Shell.DeserializeState((RecallableState)ScriptProcessor.PastStates.Peek(), false);
                });
            }
        }
        public static void OpenMatMutMenu()
        {
            if (SpoonsTrip)
            {
                MediaPlayer.Stop();
                WorldEntity Black = new WorldEntity("BLACK", new Vector2(0, 0), (TAtlasInfo)Shell.AtlasDirectory["BLACK"], 0.9601f);
                Black.TransientAnimation = true;
                Black.AnimationQueue.Add(Animation.Retrieve("FADEOUTRAPID"));
                Shell.UpdateQueue.Add(Black);
                Shell.RenderQueue.Add(Black);
                SpoonsTrip = false;
            }
            Shell.GlobalWorldState = "MAIN MENU OPENED";
            WorldEntity MainMenuBackdrop = new WorldEntity("BACKDROP_MAIN", new Vector2(), (TAtlasInfo)Shell.AtlasDirectory["MATMUTBG"], 0);
            Shell.UpdateQueue.Add(MainMenuBackdrop);
            Shell.RenderQueue.Add(MainMenuBackdrop);
            Button Button = new Button("BUTTON_MAIN_SETTINGS", new Vector2(944, 500), (TAtlasInfo)Shell.AtlasDirectory["SETTINGSBUTTON"], 0.5f, delegate () { ShowSettings(); });
            Button.CenterOrigin = false;
            Shell.UpdateQueue.Add(Button);
            Shell.RenderQueue.Add(Button);
            Button = new Button("BUTTON_MAIN_QUIT", new Vector2(1010, 635), (TAtlasInfo)Shell.AtlasDirectory["QUITBUTTON"], 0.5f, delegate () { Quit(); });
            Button.CenterOrigin = false;
            Shell.UpdateQueue.Add(Button);
            Shell.RenderQueue.Add(Button);
            TextEntity MatmutTitle = new TextEntity("MatmutTitle", "[C:0-255-0-255]Matmut CyberEx Securimax Prime", new Vector2(640f - Shell.Default.MeasureString("Matmut CyberEx Securimax Prime").X / 2f, 50), 0.8f);
            MatmutTitle.TypeWrite = false;
            Shell.UpdateQueue.Add(MatmutTitle);
            Shell.RenderQueue.Add(MatmutTitle);
            DropMenu TestDM = new DropMenu("TEST_DROPMENU", new Vector2(100, 150), 0.9f, 500, "Security Level", new String[] { "Basic (Free) Protection", "Medium Protection", "High Protection", "Extreme Meme Protection" }, false, new VoidDel(delegate () { }));
            TestDM.CenterOrigin = false;
            Shell.UpdateQueue.Add(TestDM);
            Shell.RenderQueue.Add(TestDM);
            TAtlasInfo TutorialButton = new TAtlasInfo();
            TutorialButton.Atlas = ButtonScripts.CreateDynamicCustomButton("Launch educational experience", 500);
            TutorialButton.DivDimensions = new Point(2, 1);
            Button CommenceTutorial = new Button("BUTTON_MAKE_TUTORIAL", new Vector2(700, 150), TutorialButton, 0.55f, new VoidDel(delegate() { StartTutorial(); }));
            CommenceTutorial.CenterOrigin = false;
            Shell.UpdateQueue.Add(CommenceTutorial);
            Shell.RenderQueue.Add(CommenceTutorial);
        }
        public static void StartTutorial()
        {
            MediaPlayer.Stop();
            SpoonsTrip = true;
            Shell.RunQueue = new ArrayList();
            foreach (WorldEntity E in Shell.UpdateQueue)
            {
                if (!Shell.DeleteQueue.Contains(E)) { Shell.DeleteQueue.Add(E); }
            }
            foreach (WorldEntity E in Shell.RenderQueue)
            {
                if (!Shell.DeleteQueue.Contains(E)) { Shell.DeleteQueue.Add(E); }
            }
            ScriptProcessor.PastStates.Clear();
            Shell.UpdateQueue.Add(new ScriptProcessor.ScriptSniffer("MATMUT_TUTORIAL_SNIFFER", ScriptProcessor.RetrieveScriptByName("MATMUT_TUTORIAL"), "MATMUT_TUTORIAL"));
        }
        public static Boolean SpoonsTrip = true;
        public static void OpenMainMenu()
        {
            Shell.GlobalWorldState = "MAIN MENU OPENED";
            if (SpoonsTrip)
            {
                MediaPlayer.Play((Song)Shell.SongDirectory["MEDLEY"]);
                MediaPlayer.IsRepeating = true;
                WorldEntity Black = new WorldEntity("BLACK", new Vector2(0, 0), (TAtlasInfo)Shell.AtlasDirectory["BLACK"], 0.9601f);
                Black.TransientAnimation = true;
                Black.AnimationQueue.Add(Animation.Retrieve("FADEOUTRAPID"));
                Shell.UpdateQueue.Add(Black);
                Shell.RenderQueue.Add(Black);
                SpoonsTrip = false;
            }
            WorldEntity MainMenuBackdrop = new WorldEntity("BACKDROP_MAIN", new Vector2(), (TAtlasInfo)Shell.AtlasDirectory["STARBG"], 0);
            Shell.UpdateQueue.Add(MainMenuBackdrop);
            Shell.RenderQueue.Add(MainMenuBackdrop);
            Button Button = new Button("BUTTON_MAIN_PLAY", new Vector2(86, 500), (TAtlasInfo)Shell.AtlasDirectory["PLAYBUTTON"], 0.5f, delegate () { ButtonScripts.StartMain(); });
            Button.CenterOrigin = false;
            Shell.UpdateQueue.Add(Button);
            Shell.RenderQueue.Add(Button);
            Button = new Button("BUTTON_MAIN_LOAD", new Vector2(372, 500), (TAtlasInfo)Shell.AtlasDirectory["LOADBUTTON"], 0.5f, delegate () { ButtonScripts.LoadSaveMenu(); });
            Button.CenterOrigin = false;
            Shell.UpdateQueue.Add(Button);
            Shell.RenderQueue.Add(Button);
            Button = new Button("BUTTON_MAIN_CREDITS", new Vector2(658, 500), (TAtlasInfo)Shell.AtlasDirectory["CREDITSBUTTON"], 0.5f, delegate () { ButtonScripts.ShowCredits(); });
            Button.CenterOrigin = false;
            Shell.UpdateQueue.Add(Button);
            Shell.RenderQueue.Add(Button);
            Button = new Button("BUTTON_MAIN_SETTINGS", new Vector2(944, 500), (TAtlasInfo)Shell.AtlasDirectory["SETTINGSBUTTON"], 0.5f, delegate () { ShowSettings(); });
            Button.CenterOrigin = false;
            Shell.UpdateQueue.Add(Button);
            Shell.RenderQueue.Add(Button);
            Button = new Button("BUTTON_MAIN_QUIT", new Vector2(1010, 635), (TAtlasInfo)Shell.AtlasDirectory["QUITBUTTON"], 0.5f, delegate () { Quit(); });
            Button.CenterOrigin = false;
            Shell.UpdateQueue.Add(Button);
            Shell.RenderQueue.Add(Button);
            WorldEntity Title = new WorldEntity("TITLE_MAIN", new Vector2(640, 200), (TAtlasInfo)Shell.AtlasDirectory["SOFIAWORLD"], 0.5f);
            Title.CenterOrigin = true;
            Animation A = Animation.Retrieve("WIGGLE");
            A.Loop = true;
            Title.AnimationQueue.Add(A);
            Shell.UpdateQueue.Add(Title);
            Shell.RenderQueue.Add(Title);
            WorldEntity Fire = new Sofia.ParticleFire("PARTICLEFIRE_MAIN");
            Shell.UpdateQueue.Add(Fire);
            /*Sofia.BigSofia TB = new Sofia.BigSofia("BIGSOFIA", new Vector2(640, 360), (TAtlasInfo)Shell.AtlasDirectory["BIGSOFIA"], 0.8f, new ArrayList(new String[] { "FLOATING", "GLOW", "SPEW1", "SHIFTER" }));
            TB.CenterOrigin = true;
            Shell.UpdateQueue.Add(TB);
            Shell.RenderQueue.Add(TB);*/
        }
        public static void StartTest()
        {
            MediaPlayer.Stop();
            SpoonsTrip = true;
            Shell.RunQueue = new ArrayList();
            foreach (WorldEntity E in Shell.UpdateQueue)
            {
                if(!Shell.DeleteQueue.Contains(E)) { Shell.DeleteQueue.Add(E); }
            }
            foreach (WorldEntity E in Shell.RenderQueue)
            {
                if (!Shell.DeleteQueue.Contains(E)) { Shell.DeleteQueue.Add(E); }
            }
            Shell.UpdateQueue.Add(new ScriptProcessor.ScriptSniffer("TEST_SNIFFER", ScriptProcessor.RetrieveScriptByName("TEST"), "TEST"));
        }
        public static void StartMain()
        {
            MediaPlayer.Stop();
            SpoonsTrip = true;
            Sofia.ParticleFire.Cease = true;
            Shell.RunQueue = new ArrayList();
            foreach (WorldEntity E in Shell.UpdateQueue)
            {
                if (!Shell.DeleteQueue.Contains(E)) { Shell.DeleteQueue.Add(E); }
            }
            foreach (WorldEntity E in Shell.RenderQueue)
            {
                if (!Shell.DeleteQueue.Contains(E)) { Shell.DeleteQueue.Add(E); }
            }
            ScriptProcessor.PastStates.Clear();
            Shell.UpdateQueue.Add(new ScriptProcessor.ScriptSniffer("SOFIA_MAIN_INTRO_SNIFFER", ScriptProcessor.RetrieveScriptByName("SOFIA_MAIN_INTRO"), "SOFIA_MAIN_INTRO"));
        }
        public static void BackToMainMenu()
        {
            foreach (WorldEntity E in Shell.UpdateQueue)
            {
                if (!Shell.DeleteQueue.Contains(E) && !(E is Sofia.UpwardParticle) && !(E is Sofia.Transient)) { Shell.DeleteQueue.Add(E); }
            }
            foreach (WorldEntity E in Shell.RenderQueue)
            {
                if (!Shell.DeleteQueue.Contains(E) && !(E is Sofia.UpwardParticle) && !(E is Sofia.Transient)) { Shell.DeleteQueue.Add(E); }
            }
            ScriptProcessor.WipeArchive();
            ScriptProcessor.PastStates.Clear();
            Shell.CaptureFullscreen = null;
            Shell.AllowEnter = true;
            Paused = false;
            Shell.ResetFlags();
            OpenMainMenu();
        }
        public static void LoadSaveMenu()
        {
            foreach (WorldEntity E in Shell.UpdateQueue)
            {
                if (!Shell.DeleteQueue.Contains(E) && E is Button) { Shell.DeleteQueue.Add(E); }
            }
            String[] Saves = LoadManager.FetchSaves();
            LoadManager.MaxPage = (int)Math.Ceiling((Saves.Length / 6f));
            if(LoadManager.MaxPage < 1) { LoadManager.MaxPage = 1; }
            LoadManager.PageNumber = LoadManager.MaxPage;
            Button Back = new Button("BACKBUTTON_LOADSCREEN", new Vector2(1180, 625), (TAtlasInfo)Shell.AtlasDirectory["BACKBUTTON"], 0.99f, new VoidDel(delegate () { BackToMainMenu(); }));
            Shell.UpdateQueue.Add(Back);
            Shell.RenderQueue.Add(Back);
            TextEntity PNum = new TextEntity("TEXT_PAGENUM", "[C:138-0-255-255]Page 1 of 1", new Vector2(640 - (Shell.Default.MeasureString("Page 1 of 1").X / 2f), 615), 0.99f);
            PNum.TypeWrite = false;
            Shell.UpdateQueue.Add(PNum);
            Shell.RenderQueue.Add(PNum);
            LoadManager.RefreshLoadPage();
        }
        public static void ShowCredits()
        {
            foreach (WorldEntity E in Shell.UpdateQueue)
            {
                if (!Shell.DeleteQueue.Contains(E) && E is Button) { Shell.DeleteQueue.Add(E); }
            }
            String CreditString = "This application is running the Event Horizon visual novel engine, lovingly hand-coded by Blackhole.\n\nAdditional programming also by Blackhole.\nScripting by Blackhole.\nUI design Blackhole.\n\nWritten using the MonoGame framework (www.monogame.net).";
            //String Ardata = "Ardata Carmia is a young Alternian troll living in Outglut during the time period of Hiveswap and Hiveswap Friendsim. Ardata was first revealed during the Hiveswap Troll Call event alongside Marvus Xoloto. Along with him, she is one of the few trolls whose sign is unknown; it is not visible on any of her sprites and was obscured by her cape on her Troll Call card. Ardata later went on to be one of two trolls to be featured in Hiveswap Friendship Simulator: Volume One after its release on April 13th, 2018, alongside her fellow Troll Call troll, Diemen Xicali.\n\nA high - ranking cerulean - blood, Ardata maintains a significant following on social media by playing up her sinister personality, torturing captives in her basement on camera, and utilizing them as slaves with the aid of her mind control abilities.\n\nArdata's Troll Call card described her as \"bloodthirsty on main\" (later revealed to be very accurate), \"probably Vriska\" and \"fresh to death sentence\".\n\n";
            Texture2D[] SB = CreateDynamicScroll(CreditString, 1000);
            ScrollBar Credits = new ScrollBar("CREDIT_SCROLLBAR", new Vector2(1080, 95), (TAtlasInfo)Shell.AtlasDirectory["SCROLLBAR"], 0.98f, SB, 600);
            Shell.UpdateQueue.Add(Credits);
            Shell.RenderQueue.Add(Credits);
            Button Back = new Button("BACKBUTTON_CREDITS", new Vector2(1180, 625), (TAtlasInfo)Shell.AtlasDirectory["BACKBUTTON"], 1f, new VoidDel(delegate () { BackToMainMenu(); }));
            Shell.UpdateQueue.Add(Back);
            Shell.RenderQueue.Add(Back);
        }
        public static void ShowSettings()
        {
            if (!Paused)
            {
                foreach (WorldEntity E in Shell.UpdateQueue)
                {
                    if (!Shell.DeleteQueue.Contains(E) && E is Button) { Shell.DeleteQueue.Add(E); }
                }
            }
            else
            {
                foreach (WorldEntity E in Shell.UpdateQueue)
                {
                    if (E is Button) { ((Button)E).Enabled = false; }
                    if (E is ScrollBar) { ((ScrollBar)E).Enabled = false; }
                    if (E is Slider) { ((Slider)E).Enabled = false; }
                }
            }
            WorldEntity SettingsPane = new WorldEntity("PANE_SETTINGS", new Vector2(), (TAtlasInfo)Shell.AtlasDirectory["SETTINGSPANE"], 0.99f);
            Shell.UpdateQueue.Add(SettingsPane);
            Shell.RenderQueue.Add(SettingsPane);
            Checkbox Fullscreen = new Checkbox("CHECKBOX_SETTINGS_FULLSCREEN", new Vector2(140, 200), (TAtlasInfo)Shell.AtlasDirectory["CHECKBOX"], 0.991f, Shell.QueryFullscreen(), new VoidDel(delegate ()
                {
                    if (Shell.CaptureFullscreen.Toggle) { if (!Shell.QueryFullscreen()) { Shell.ToggleFullscreen(); } }
                    else { if (Shell.QueryFullscreen()) { Shell.ToggleFullscreen(); } }
                }));
            Shell.CaptureFullscreen = Fullscreen;
            Shell.UpdateQueue.Add(Fullscreen);
            Shell.RenderQueue.Add(Fullscreen);
            TextEntity FSLabel = new TextEntity("LABEL_SETTINGS_FULLSCREEN", "Run in fullscreen", new Vector2(175, 185), 0.991f);
            FSLabel.TypeWrite = false;
            Shell.UpdateQueue.Add(FSLabel);
            Shell.RenderQueue.Add(FSLabel);
            TextEntity VLabel = new TextEntity("LABEL_SETTINGS_VOLUME", "Volume control", new Vector2(135, 235), 0.991f);
            VLabel.TypeWrite = false;
            Shell.UpdateQueue.Add(VLabel);
            Shell.RenderQueue.Add(VLabel);
            WorldEntity VolumeBar = new WorldEntity("BAR_SETTINGS_VOLUME", new Vector2(135, 285), (TAtlasInfo)Shell.AtlasDirectory["SLIDERBAR"], 0.9905f);
            Shell.UpdateQueue.Add(VolumeBar);
            Shell.RenderQueue.Add(VolumeBar);
            Slider Volume = new Slider("SLIDER_SETTINGS_VOLUME", new Vector2(140, 295), (TAtlasInfo)Shell.AtlasDirectory["SLIDERKNOB"], 0.991f, new Vector2(140, 295), new Vector2(630, 295), Shell.GlobalVolume);
            if (Shell.Mute)
            {
                Volume.ForceState(0f);
                Volume.Enabled = false;
            }
            Shell.CaptureVolume = Volume;
            Shell.UpdateQueue.Add(Volume);
            Shell.RenderQueue.Add(Volume);
            Checkbox Mute = new Checkbox("CHECKBOX_SETTINGS_MUTE", new Vector2(140, 355), (TAtlasInfo)Shell.AtlasDirectory["CHECKBOX"], 0.991f, Shell.Mute, new VoidDel(delegate ()
                {
                    if (Shell.CaptureMute.Toggle) { Shell.Mute = true; }
                    else { Shell.Mute = false; }
                }));
            Shell.CaptureMute = Mute;
            Shell.UpdateQueue.Add(Mute);
            Shell.RenderQueue.Add(Mute);
            TextEntity MLabel = new TextEntity("LABEL_SETTINGS_MUTE", "Mute audio", new Vector2(175, 340), 0.991f);
            MLabel.TypeWrite = false;
            Shell.UpdateQueue.Add(MLabel);
            Shell.RenderQueue.Add(MLabel);
            TextEntity TRateLabel = new TextEntity("LABEL_SETTINGS_TEXTRATE", "Text speed", new Vector2(135, 400), 0.991f);
            TRateLabel.TypeWrite = false;
            Shell.UpdateQueue.Add(TRateLabel);
            Shell.RenderQueue.Add(TRateLabel);
            WorldEntity TextBar = new WorldEntity("BAR_SETTINGS_TEXTRATE", new Vector2(135, 450), (TAtlasInfo)Shell.AtlasDirectory["SLIDERBAR"], 0.9905f);
            Shell.UpdateQueue.Add(TextBar);
            Shell.RenderQueue.Add(TextBar);
            Slider Textrate = new Slider("SLIDER_SETTINGS_TEXTRATE", new Vector2(140, 460), (TAtlasInfo)Shell.AtlasDirectory["SLIDERKNOB"], 0.991f, new Vector2(140, 460), new Vector2(630, 460), TextEntity.GetSliderValueFromTicks(TextEntity.TickWriteInterval));
            Shell.CaptureTextrate = Textrate;
            Shell.UpdateQueue.Add(Textrate);
            Shell.RenderQueue.Add(Textrate);
            TextEntity DynamicTextrate = new TextEntity("DYNAMIC_SETTINGS_TEXTRATE_DISPLAY", TextEntity.TickWriteInterval + " milliseconds", new Vector2(350, 400), 0.991f);
            DynamicTextrate.TypeWrite = false;
            Shell.CaptureRateDisplay = DynamicTextrate;
            Shell.UpdateQueue.Add(DynamicTextrate);
            Shell.RenderQueue.Add(DynamicTextrate);
            Checkbox SSSave = new Checkbox("CHECKBOX_SETTINGS_SAVES", new Vector2(140, 520), (TAtlasInfo)Shell.AtlasDirectory["CHECKBOX"], 0.991f, SaveLoadModule.ApplicableSaveType == "ScriptStem", new VoidDel(delegate ()
            {
                if (Shell.CaptureSaveType.Toggle) { SaveLoadModule.ApplicableSaveType = "ScriptStem"; }
                else { SaveLoadModule.ApplicableSaveType = "FullySerializedBinary"; }
            }));
            Shell.CaptureSaveType = SSSave;
            Shell.UpdateQueue.Add(SSSave);
            Shell.RenderQueue.Add(SSSave);
            TextEntity SLabel = new TextEntity("LABEL_SETTINGS_SAVES", "Enable simple saves", new Vector2(175, 505), 0.991f);
            SLabel.TypeWrite = false;
            Shell.UpdateQueue.Add(SLabel);
            Shell.RenderQueue.Add(SLabel);
            TextEntity SLabel2 = new TextEntity("LABEL_SETTINGS_SAVES2", "[C:220-100-255-255]Simple saves ensure version compatibility and take up less space, but may not work with unsupported scripts.", new Vector2(660, 450), 0.991f);
            SLabel2.BufferLength = 500;
            SLabel2.Text = SLabel2.Text;
            SLabel2.TypeWrite = false;
            Shell.UpdateQueue.Add(SLabel2);
            Shell.RenderQueue.Add(SLabel2);
            if (!Paused)
            {
                Button Back = new Button("BACKBUTTON_SETTINGS", new Vector2(1110, 610), (TAtlasInfo)Shell.AtlasDirectory["BACKBUTTON"], 1f, new VoidDel(delegate () { BackToMainMenu(); }));
                Shell.UpdateQueue.Add(Back);
                Shell.RenderQueue.Add(Back);
            }
            else
            {
                Button Back = new Button("BACKBUTTON_SETTINGS", new Vector2(1110, 610), (TAtlasInfo)Shell.AtlasDirectory["BACKBUTTON"], 1f, new VoidDel(delegate () { UnSettings(); }));
                Shell.UpdateQueue.Add(Back);
                Shell.RenderQueue.Add(Back);
            }
            Button Restore = new Button("BUTTON_SETTINGS_RESTORE", new Vector2(280, 620), (TAtlasInfo)Shell.AtlasDirectory["RESTOREBUTTON"], 1f, new VoidDel(delegate () { Shell.DefaultSettings(); }));
            Shell.UpdateQueue.Add(Restore);
            Shell.RenderQueue.Add(Restore);
        }
        public static void UnSettings()
        {
            SaveLoadModule.WritePersistentState();
            Shell.CaptureFullscreen = null;
            Shell.CaptureMute = null;
            Shell.CaptureVolume = null;
            Shell.CaptureSaveType = null;
            Shell.CaptureTextrate = null;
            Shell.CaptureRateDisplay = null;
            foreach (WorldEntity E in Shell.UpdateQueue)
            {
                if (E.Name == "PANE_SETTINGS" && !Shell.DeleteQueue.Contains(E)) { Shell.DeleteQueue.Add(E); }
                else if (E.Name == "BAR_SETTINGS_VOLUME" && !Shell.DeleteQueue.Contains(E)) { Shell.DeleteQueue.Add(E); }
                else if (E.Name == "BAR_SETTINGS_TEXTRATE" && !Shell.DeleteQueue.Contains(E)) { Shell.DeleteQueue.Add(E); }
                if (E is Checkbox && E.Name == "CHECKBOX_SETTINGS_FULLSCREEN" && !Shell.DeleteQueue.Contains(E)) { Shell.DeleteQueue.Add(E); }
                else if (E is Checkbox && E.Name == "CHECKBOX_SETTINGS_MUTE" && !Shell.DeleteQueue.Contains(E)) { Shell.DeleteQueue.Add(E); }
                else if (E is Checkbox && E.Name == "CHECKBOX_SETTINGS_SAVES" && !Shell.DeleteQueue.Contains(E)) { Shell.DeleteQueue.Add(E); }
                if (E is Slider && E.Name == "SLIDER_SETTINGS_VOLUME" && !Shell.DeleteQueue.Contains(E)) { Shell.DeleteQueue.Add(E); }
                else if (E is Slider && E.Name == "SLIDER_SETTINGS_TEXTRATE" && !Shell.DeleteQueue.Contains(E)) { Shell.DeleteQueue.Add(E); }
                if (E is TextEntity && E.Name == "LABEL_SETTINGS_FULLSCREEN" && !Shell.DeleteQueue.Contains(E)) { Shell.DeleteQueue.Add(E); }
                else if (E is TextEntity && E.Name == "LABEL_SETTINGS_MUTE" && !Shell.DeleteQueue.Contains(E)) { Shell.DeleteQueue.Add(E); }
                else if (E is TextEntity && E.Name == "LABEL_SETTINGS_VOLUME" && !Shell.DeleteQueue.Contains(E)) { Shell.DeleteQueue.Add(E); }
                else if (E is TextEntity && E.Name == "LABEL_SETTINGS_SAVES" && !Shell.DeleteQueue.Contains(E)) { Shell.DeleteQueue.Add(E); }
                else if (E is TextEntity && E.Name == "LABEL_SETTINGS_SAVES2" && !Shell.DeleteQueue.Contains(E)) { Shell.DeleteQueue.Add(E); }
                else if (E is TextEntity && E.Name == "LABEL_SETTINGS_TEXTRATE" && !Shell.DeleteQueue.Contains(E)) { Shell.DeleteQueue.Add(E); }
                else if (E is TextEntity && E.Name == "DYNAMIC_SETTINGS_TEXTRATE_DISPLAY" && !Shell.DeleteQueue.Contains(E)) { Shell.DeleteQueue.Add(E); }
                if (E is Button)
                {
                    if (E.Name == "BACKBUTTON_SETTINGS" && !Shell.DeleteQueue.Contains(E)) { Shell.DeleteQueue.Add(E); }
                    else if (E.Name == "BUTTON_SETTINGS_RESTORE" && !Shell.DeleteQueue.Contains(E)) { Shell.DeleteQueue.Add(E); }
                    else if (E.Name == "BUTTON_PAUSE_RETURN" || E.Name == "BUTTON_PAUSE_SAVE" || E.Name == "BUTTON_PAUSE_SETTINGS" || E.Name == "BUTTON_PAUSE_MAINMENU" || E.Name == "BUTTON_PAUSE_QUIT")
                    {
                        ((Button)E).Enabled = true;
                    }
                }
            }
        }
        public static void SaveActual(Texture2D Thumb)
        {
            Shell.PlaySoundInstant("UT_SAVE");
            SaveLoadModule.WriteSave(Thumb);
            foreach (WorldEntity E in Shell.UpdateQueue)
            {
                if (E.Name == "PANE_SAVE" && !Shell.DeleteQueue.Contains(E)) { Shell.DeleteQueue.Add(E); }
                else if (E.Name == "THUMB_SAVE" && !Shell.DeleteQueue.Contains(E)) { Shell.DeleteQueue.Add(E); }
                if (E is TextEntity && E.Name == "TIMETEXT_NEWSAVE" && !Shell.DeleteQueue.Contains(E)) { Shell.DeleteQueue.Add(E); }
                if (E is Button)
                {
                    if (E.Name == "BUTTON_SAVE_YES" && !Shell.DeleteQueue.Contains(E)) { Shell.DeleteQueue.Add(E); }
                    else if (E.Name == "BUTTON_SAVE_NO" && !Shell.DeleteQueue.Contains(E)) { Shell.DeleteQueue.Add(E); }
                }
            }
            WorldEntity SaveWrittenPane = new WorldEntity("PANE_SAVEWRITTEN", new Vector2(640, 300), (TAtlasInfo)Shell.AtlasDirectory["SAVEWRITTENPANE"], 0.99f);
            SaveWrittenPane.CenterOrigin = true;
            Shell.UpdateQueue.Add(SaveWrittenPane);
            Shell.RenderQueue.Add(SaveWrittenPane);
            Button Back = new Button("BUTTON_SAVE_BACK", new Vector2(640, 360), (TAtlasInfo)Shell.AtlasDirectory["BACKBUTTON"], 0.991f, new VoidDel(delegate () { UnSave(); }));
            Shell.UpdateQueue.Add(Back);
            Shell.RenderQueue.Add(Back);
        }
        public static void Save()
        {
            foreach (WorldEntity E in Shell.UpdateQueue)
            {
                if (E is Button) { ((Button)E).Enabled = false; }
                if (E is ScrollBar) { ((ScrollBar)E).Enabled = false; }
                if (E is Slider) { ((Slider)E).Enabled = false; }
            }
            Texture2D SaveThumb = SaveLoadModule.GenerateSaveThumb();
            WorldEntity SavePane = new WorldEntity("PANE_SAVE", new Vector2(640, 360), (TAtlasInfo)Shell.AtlasDirectory["SAVEPANE"], 0.99f);
            SavePane.CenterOrigin = true;
            Shell.UpdateQueue.Add(SavePane);
            Shell.RenderQueue.Add(SavePane);
            TAtlasInfo ThisThumb = (TAtlasInfo)Shell.AtlasDirectory["THUMBBLANK"];
            ThisThumb.Atlas = SaveThumb;
            WorldEntity Thumb = new WorldEntity("THUMB_SAVE", new Vector2(480, 233), ThisThumb, 0.991f);
            Shell.UpdateQueue.Add(Thumb);
            Shell.RenderQueue.Add(Thumb);
            DateTime Now = DateTime.Now;
            String Time = Now.ToShortDateString() + " " + Now.ToShortTimeString();
            TextEntity TimeText = new TextEntity("TIMETEXT_NEWSAVE", Time, new Vector2(640, 323) - new Vector2(Shell.Default.MeasureString(Time).X / 2f, -110), 0.991f);
            TimeText.TypeWrite = false;
            Shell.UpdateQueue.Add(TimeText);
            Shell.RenderQueue.Add(TimeText);
            Button Yes = new Button("BUTTON_SAVE_YES", new Vector2(530, 530), (TAtlasInfo)Shell.AtlasDirectory["YESBUTTON"], 0.991f, new VoidDel(delegate () { SaveActual(SaveThumb); }));
            Shell.UpdateQueue.Add(Yes);
            Shell.RenderQueue.Add(Yes);
            Button No = new Button("BUTTON_SAVE_NO", new Vector2(750, 530), (TAtlasInfo)Shell.AtlasDirectory["NOBUTTON"], 0.991f, new VoidDel(delegate () { UnSave(); }));
            Shell.UpdateQueue.Add(No);
            Shell.RenderQueue.Add(No);
        }
        public static void UnSave()
        {
            foreach (WorldEntity E in Shell.UpdateQueue)
            {
                if (E.Name == "PANE_SAVE" && !Shell.DeleteQueue.Contains(E)) { Shell.DeleteQueue.Add(E); }
                else if (E.Name == "PANE_SAVEWRITTEN" && !Shell.DeleteQueue.Contains(E)) { Shell.DeleteQueue.Add(E); }
                else if (E.Name == "THUMB_SAVE" && !Shell.DeleteQueue.Contains(E)) { Shell.DeleteQueue.Add(E); }
                if(E is TextEntity && E.Name == "TIMETEXT_NEWSAVE" && !Shell.DeleteQueue.Contains(E)) { Shell.DeleteQueue.Add(E); }
                if (E is Button)
                {
                    if (E.Name == "BUTTON_SAVE_YES" && !Shell.DeleteQueue.Contains(E)) { Shell.DeleteQueue.Add(E); }
                    else if (E.Name == "BUTTON_SAVE_NO" && !Shell.DeleteQueue.Contains(E)) { Shell.DeleteQueue.Add(E); }
                    else if (E.Name == "BUTTON_SAVE_BACK" && !Shell.DeleteQueue.Contains(E)) { Shell.DeleteQueue.Add(E); }
                    else if (E.Name == "BUTTON_PAUSE_RETURN" || E.Name == "BUTTON_PAUSE_SAVE" || E.Name == "BUTTON_PAUSE_SETTINGS" || E.Name == "BUTTON_PAUSE_MAINMENU" || E.Name == "BUTTON_PAUSE_QUIT")
                    {
                        ((Button)E).Enabled = true;
                    }
                }
            }
        }
        public static void RefreshUIHideState()
        {
            if(Paused) { return; }
            if (UIHideEnabled) { UnHideUI(); }
            else { ButtonScripts.HideUI(); }
            WorldEntity HUICB = Shell.GetEntityByName("BUTTON_HIDE_UI");
            if(!(HUICB == null || !(HUICB is Checkbox)))
            {
                Checkbox C = (Checkbox)HUICB;
                if (C.Toggle != UIHideEnabled) { C.ForceState(UIHideEnabled); }
            }
            else { Shell.WriteLine("Internal warning: The UI view state is being toggled while the toggle UI element is not present."); }
        }
        public static String[] DefaultUINames = { "UIBOX", "BUTTON_ARCHIVE", "BUTTON_SKIP", "BUTTON_PAUSEMENU", "BUTTON_ROLLBACK", "BUTTON_HIDE_UI", "NAMELABELBACKING" };
        public static void InitDefaultUI()
        {
            if (Shell.GetEntityByName("UIBOX") == null)
            {
                WorldEntity Add = new WorldEntity("UIBOX", new Vector2(100, 470), (TAtlasInfo)Shell.AtlasDirectory["UIBOX"], 0.9f);
                Add.ColourValue = new Color(200, 200, 200, 255);
                Add.GiveClickFunction(DelegateFetch("uiboxclick"));
                Add.MLCRecord = new String[] { "uiboxclick" };
                Shell.UpdateQueue.Add(Add);
                Shell.RenderQueue.Add(Add);
            }
            if (Shell.GetEntityByName("BUTTON_ARCHIVE") == null)
            {
                Button Archive = new Button("BUTTON_ARCHIVE", new Vector2(70, 510), (TAtlasInfo)Shell.AtlasDirectory["ARCHIVEBUTTON"], 0.95f, DelegateFetch("openarchive"));
                Archive.MLCRecord = new String[] { "openarchive" };
                Shell.UpdateQueue.Add(Archive);
                Shell.RenderQueue.Add(Archive);
            }
            if (Shell.GetEntityByName("BUTTON_SKIP") == null)
            {
                Button Skip = new Button("BUTTON_SKIP", new Vector2(70, 557), (TAtlasInfo)Shell.AtlasDirectory["SKIPBUTTON"], 0.95f, DelegateFetch("skip"));
                Skip.MLCRecord = new String[] { "skip" };
                Shell.UpdateQueue.Add(Skip);
                Shell.RenderQueue.Add(Skip);
            }
            if (Shell.GetEntityByName("BUTTON_PAUSEMENU") == null)
            {
                Button PauseB = new Button("BUTTON_PAUSEMENU", new Vector2(70, 604), (TAtlasInfo)Shell.AtlasDirectory["PAUSEMENUBUTTON"], 0.95f, DelegateFetch("pause"));
                PauseB.MLCRecord = new String[] { "pause" };
                Shell.UpdateQueue.Add(PauseB);
                Shell.RenderQueue.Add(PauseB);
            }
            if (Shell.GetEntityByName("BUTTON_ROLLBACK") == null)
            {
                Button Return = new Button("BUTTON_ROLLBACK", new Vector2(70, 651), (TAtlasInfo)Shell.AtlasDirectory["RETURNBUTTON"], 0.95f, DelegateFetch("scriptrollback"));
                Return.MLCRecord = new String[] { "scriptrollback" };
                Shell.UpdateQueue.Add(Return);
                Shell.RenderQueue.Add(Return);
            }
            if (Shell.GetEntityByName("BUTTON_HIDE_UI") == null)
            {
                Checkbox HideUI = new Checkbox("BUTTON_HIDE_UI", new Vector2(1205, 650), (TAtlasInfo)Shell.AtlasDirectory["EYECHECKBOX"], 0.95f, false, DelegateFetch("refreshuihidestate"));
                HideUI.CenterOrigin = false;
                HideUI.MLCRecord = new String[] { "refreshuihidestate" };
                Shell.UpdateQueue.Add(HideUI);
                Shell.RenderQueue.Add(HideUI);
            }
        }
        public static void Skip()
        {
            foreach (WorldEntity E in Shell.UpdateQueue)
            {
                if(E is ScriptProcessor.ScriptSniffer)
                {
                    ScriptProcessor.ScriptSniffer S = (ScriptProcessor.ScriptSniffer)E;
                    if(S.Skipping) { S.CeaseSkipping(); }
                    else { S.Skip(); }
                }
            }
        }
    }
}