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
        public static void UIBoxClick()
        {
            if (Shell.AllowEnter) { Shell.DoNextShifter = true; }
        }
        public static void RunScriptButton(String ScriptName)
        {
            ScriptProcessor.PastStates.Clear();
            ScriptProcessor.ActivateScriptElement("B|" + ScriptName);
        }
        public static void SetGlobalWorldStateButton(String NewGWS)
        {
            Shell.GlobalWorldState = NewGWS;
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
                    if (E.OverlayUtility) { continue; }
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
                        E.Move(new Vector2((640 - (Shell.Default.MeasureString(NT).X / 2f)) - E.Position.X, 0));
                    }
                }
                if(PageNumber > 1 && !Prev)
                {
                    Button PrevB = new Button("PREVBUTTON_LOADSCREEN", new Vector2(340, 625), (TAtlasInfo)Shell.AtlasDirectory["PREVBUTTON"], 0.99f);
                    //Note: These anonymous button methods cannot be serialized.
                    PrevB.ButtonPressFunction += new VoidDel(delegate ()
                    {
                        PageNumber--;
                        RefreshLoadPage();
                    });
                    Shell.UpdateQueue.Add(PrevB);
                    Shell.RenderQueue.Add(PrevB);
                }
                if (PageNumber < MaxPage && !Next)
                {
                    Button NextB = new Button("NEXTBUTTON_LOADSCREEN", new Vector2(940, 625), (TAtlasInfo)Shell.AtlasDirectory["NEXTBUTTON"], 0.99f);
                    NextB.ButtonPressFunction += new VoidDel(delegate ()
                    {
                        PageNumber++;
                        RefreshLoadPage();
                    });
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
                        LoadButtons[i] = new Button("LOADSAVEATPOSITION_" + i, ButtonLocations[i], ThisButton, 0.98f);
                        ((Button)LoadButtons[i]).SubscribeToEvent(WorldEntity.EventNames.ButtonPressFunction, typeof(ButtonScripts).GetMethod("TriggerSpecLoad"), null);
                        SaveAccess.Add(LoadButtons[i].EntityID, TheseSaves[i]);

                        String SaveDate = (String)SaveAttributes["#TIME"];
                        long ThenTicks = Convert.ToInt64(SaveDate);
                        DateTime Then = DateTime.FromBinary(ThenTicks);
                        String Time = Then.ToShortDateString() + " " + Then.ToShortTimeString();
                        TextEntity TimeText = new TextEntity("TIMETEXT_" + i, Time, ButtonLocations[i] - new Vector2(Shell.Default.MeasureString(Time).X / 2f, -110), 0.98f);
                        TimeText.TypeWrite = false;
                        Dates.Add(TimeText);

                        Button MyDeleteButton = new Button("DELETESAVEATPOSITION_" + i, ButtonLocations[i], (TAtlasInfo)Shell.AtlasDirectory["DELETESAVEBUTTON"], 0.981f);
                        MyDeleteButton.SubscribeToEvent(WorldEntity.EventNames.ButtonPressFunction, typeof(ButtonScripts).GetMethod("TriggerSpecDelete"), null);
                        MyDeleteButton.SubscribeToEvent(WorldEntity.EventNames.ButtonHoverFunction, typeof(ButtonScripts).GetMethod("TriggerSpecDeleteHover"), null);
                        MyDeleteButton.SubscribeToEvent(WorldEntity.EventNames.ButtonHoverReleaseFunction, typeof(ButtonScripts).GetMethod("TriggerSpecDeleteUnHover"), null);
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
        //TODO: These Spec functions are silly now that we have the proper event system and could pass in the ID of the caller. Change this up.
        public static void TriggerSpecDeleteHover()
        {
            foreach (WorldEntity E in Shell.UpdateQueue)
            {
                if (E.OverlayUtility) { continue; }
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
                if (E.OverlayUtility) { continue; }
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
                if (E.OverlayUtility) { continue; }
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
                if (E.OverlayUtility) { continue; }
                if (E is Button && ((Button)E).ViableClick)
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
                if (E.OverlayUtility) { continue; }
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
            Button Yes = new Button("BUTTON_DELETE_YES", new Vector2(530, 530), (TAtlasInfo)Shell.AtlasDirectory["YESBUTTON"], 0.9951f);
            Yes.SubscribeToEvent(WorldEntity.EventNames.ButtonPressFunction, typeof(ButtonScripts).GetMethod("DeleteActual"), new object[] { ThumbPath, SavePath });
            Shell.UpdateQueue.Add(Yes);
            Shell.RenderQueue.Add(Yes);
            Button No = new Button("BUTTON_DELETE_NO", new Vector2(750, 530), (TAtlasInfo)Shell.AtlasDirectory["NOBUTTON"], 0.9951f);
            No.SubscribeToEvent(WorldEntity.EventNames.ButtonPressFunction, typeof(ButtonScripts).GetMethod("UnDelete"), null);
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
            Button Back = new Button("BUTTON_DELETE_BACK", new Vector2(640, 360), (TAtlasInfo)Shell.AtlasDirectory["BACKBUTTON"], 0.9961f);
            Back.SubscribeToEvent(WorldEntity.EventNames.ButtonPressFunction, typeof(ButtonScripts).GetMethod("UnDelete"), null);
            Shell.UpdateQueue.Add(Back);
            Shell.RenderQueue.Add(Back);
        }
        public static void UnDelete()
        {
            foreach (WorldEntity E in Shell.UpdateQueue)
            {
                if (E.OverlayUtility) { continue; }
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
            float EStartY = E.Position.Y;
            ArrayList ScrollTextures = new ArrayList();
            int UpShift = 0;
            while (UpShift < ScrollDimensions.Y)
            {
                int RealScrollY = (int)ScrollDimensions.Y - UpShift;
                if (RealScrollY > 2000) { RealScrollY = 2000; }
                E.QuickMoveTo(new Vector2(E.Position.X, EStartY - UpShift));
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
        public static Texture2D CreateDynamicTextCheckbox(String text, Vector2 dimensions, Vector2 textBuffer, Color highlightColour, Color unhighlightColour, Color backgroundColour, Color checkedColor)
        {
            Texture2D A = CreateCustomButton(text, dimensions, textBuffer, highlightColour, unhighlightColour, backgroundColour);
            Texture2D B = CreateCustomButton(text, dimensions, textBuffer, highlightColour, checkedColor, backgroundColour);
            Texture2D Out = VNFUtils.CombineTextures(Shell.DefaultShell, new Point(A.Width, A.Height + B.Height), A, A.Bounds, new Vector2(), new Vector2(1, 1), B, B.Bounds, new Vector2(0, A.Height), new Vector2(1, 1));
            return Out;
        }
        public static Texture2D CreateDynamicCustomButton(String Text, float Width)
        {
            return CreateCustomButton(Text, new Vector2(Width, -1), new Vector2(15, 15), new Color(138, 0, 255, 255), new Color(70, 70, 70, 255), new Color(129, 129, 129, 200));
        }
        public static Texture2D CreateCustomButton(String text, Vector2 buttonDimensions, Vector2 buttonTextBuffer, Color highlightColour, Color unhighlightColour, Color backgroundColour)
        {
            TextEntity customText = new TextEntity("CUSTOM_BUTTON_TEXT", "", buttonTextBuffer, 1);
            customText.TypeWrite = false;
            customText.BufferLength = (int)(buttonDimensions.X + 10 - (buttonTextBuffer.X * 2));
            customText.Text = "[C:" + unhighlightColour.R + "-" + unhighlightColour.G + "-" + unhighlightColour.B + "-255]" + text;
            if (buttonDimensions.Y == -1)
            {
                buttonDimensions.Y = customText.VerticalLength(true) + 20;
            }
            RenderTarget2D centerPane = new RenderTarget2D(Shell.PubGD, (int)buttonDimensions.X, (int)buttonDimensions.Y, false,
                Shell.PubGD.PresentationParameters.BackBufferFormat,
                DepthFormat.Depth24);
            Shell.PubGD.SetRenderTarget(centerPane);
            Shell.PubGD.Clear(new Color((int)backgroundColour.R, (int)backgroundColour.G, (int)backgroundColour.B, 255));
            RenderTarget2D highlightPane = new RenderTarget2D(Shell.PubGD, (int)buttonDimensions.X + 10, (int)buttonDimensions.Y + 10, false,
                Shell.PubGD.PresentationParameters.BackBufferFormat,
                DepthFormat.Depth24);
            Shell.PubGD.SetRenderTarget(highlightPane);
            Shell.PubGD.Clear(highlightColour);
            RenderTarget2D Output = new RenderTarget2D(Shell.PubGD, (int)(buttonDimensions.X + 10)*2, (int)buttonDimensions.Y + 10, false,
                Shell.PubGD.PresentationParameters.BackBufferFormat,
                DepthFormat.Depth24);
            Shell.PubGD.SetRenderTarget(Output);
            Shell.PubGD.Clear(unhighlightColour);
            SpriteBatch spriteBatch = new SpriteBatch(Shell.PubGD);
            spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
            spriteBatch.Draw(highlightPane, new Rectangle((int)buttonDimensions.X + 10, 0, (int)buttonDimensions.X + 10, (int)buttonDimensions.Y + 10), Color.White);
            spriteBatch.Draw(centerPane, new Rectangle(5, 5, (int)buttonDimensions.X, (int)buttonDimensions.Y), Color.White);
            spriteBatch.Draw(centerPane, new Rectangle((int)buttonDimensions.X + 15, 5, (int)buttonDimensions.X, (int)buttonDimensions.Y), Color.White);
            customText.Draw(spriteBatch);
            customText.Text = "[C:" + highlightColour.R + "-" + highlightColour.G + "-" + highlightColour.B + "-255]" + text;
            customText.Move(new Vector2(buttonDimensions.X + 10, 0));
            customText.Draw(spriteBatch);
            spriteBatch.End();
            Shell.PubGD.SetRenderTarget(null);
            Texture2D Out = VNFUtils.GetFromRT(Output);
            Color[] OutModify = new Color[Out.Width * Out.Height];
            Out.GetData<Color>(OutModify);
            Color Prev = backgroundColour;
            Color New = new Color((int)backgroundColour.R, (int)backgroundColour.G, (int)backgroundColour.B, (int)backgroundColour.A);
            for (int i = 0; i < OutModify.Length; i++)
            {
                if(OutModify[i] == Prev) { OutModify[i] = New; }
            }
            Out.SetData<Color>(OutModify);
            return Out;
        }
        public static Button GetQuickButton(String text)
        {
            return GetQuickButton(text, 600);
        }
        public static Button GetQuickButton(String text, int width)
        {
            TAtlasInfo NewAtlas = new TAtlasInfo();
            NewAtlas.Atlas = CreateDynamicCustomButton(text, width);
            NewAtlas.DivDimensions = new Point(2, 1);
            Button NewB = new Button("BUTTON_CUSTOM_" + text.ToUpper(), new Vector2(), NewAtlas, 0.91f);
            return NewB;
        }
        public static void OpenAndConstructConsole()
        {
            WorldEntity ConsoleBacking = new WorldEntity("CONSOLE_BACKING_UI", new Vector2(), (TAtlasInfo)Shell.AtlasDirectory["CONSOLEPANE"], 0.9989f);
            ConsoleBacking.OverlayUtility = true;
            ConsoleBacking.CameraImmune = true;
            VerticalScrollPane ConsoleWindow = new VerticalScrollPane("CONSOLE_SCROLLPANE", new Vector2(1262, 35), (TAtlasInfo)Shell.AtlasDirectory["CONSOLESCROLLBAR"], 0.999f, new Point(1243, 265), Color.Black);
            ConsoleWindow.SetAsTextPane(Shell.PullInternalConsoleData, 100);
            ConsoleWindow.JumpTo(1f);
            ConsoleWindow.MyBehaviours.Add(new Behaviours.ConsoleReaderBehaviour());
            ConsoleWindow.OverlayUtility = true;
            ConsoleWindow.CameraImmune = true;
            MonitoringTextInputField ConsoleField = new MonitoringTextInputField("CONSOLE_TEXTINPUT", "", new Vector2(30, 277), 0.999f);
            ConsoleField.BufferLength = 1150;
            ConsoleField.OverlayUtility = true;
            ConsoleField.CameraImmune = true;
            Button ConsoleButton = new Button("CONSOLE_ENTER_BUTTON", new Vector2(1212, 274), (TAtlasInfo)Shell.AtlasDirectory["CONSOLEENTERBUTTON"], 0.999f);
            ConsoleButton.OverlayUtility = true;
            ConsoleButton.CenterOrigin = false;
            ConsoleButton.CameraImmune = true;
            ConsoleField.SubscribeToEvent(ConsoleButton, WorldEntity.EventNames.ButtonPressFunction, typeof(MonitoringTextInputField).GetMethod("ManualSendEnterSignal"), null);
            ConsoleField.TextEnteredFunction += new VoidDel(delegate ()
            {
                Shell.HandleConsoleInput(ConsoleField.LastSentText);
            });
            Shell.UpdateQueue.Add(ConsoleBacking);
            Shell.RenderQueue.Add(ConsoleBacking);
            Shell.UpdateQueue.Add(ConsoleWindow);
            Shell.RenderQueue.Add(ConsoleWindow);
            Shell.UpdateQueue.Add(ConsoleButton);
            Shell.RenderQueue.Add(ConsoleButton);
            Shell.UpdateQueue.Add(ConsoleField);
            Shell.RenderQueue.Add(ConsoleField);
        }
        public static void CloseConsole()
        {
            foreach (WorldEntity E in Shell.UpdateQueue)
            {
                if (E is VerticalScrollPane && E.Name == "CONSOLE_SCROLLPANE" && !Shell.DeleteQueue.Contains(E))
                {
                    Shell.DeleteQueue.Add(E);
                }
                else if (E is Button && E.Name == "CONSOLE_ENTER_BUTTON" && !Shell.DeleteQueue.Contains(E)) { Shell.DeleteQueue.Add(E); }
                else if (E is MonitoringTextInputField && E.Name == "CONSOLE_TEXTINPUT" && !Shell.DeleteQueue.Contains(E)) { Shell.DeleteQueue.Add(E); }
                else if (E.Name == "CONSOLE_BACKING_UI" && !Shell.DeleteQueue.Contains(E)) { Shell.DeleteQueue.Add(E); }
            }
        }
        public static void OpenArchive()
        {
            ScriptProcessor.AllowScriptShift = false;
            Shell.AllowEnter = false;
            foreach (WorldEntity E in Shell.UpdateQueue)
            {
                if (E.OverlayUtility) { continue; }
                if (E is TextEntity) { E.Drawable = false; }
                if (E is Button) { ((Button)E).Enabled = false; }
            }
            VerticalScrollPane Archive = new VerticalScrollPane("ARCHIVE_SCROLLBAR", new Vector2(1080, 95), (TAtlasInfo)Shell.AtlasDirectory["SCROLLBAR"], 0.98f, new Point(1000, 600), Color.Gray);
            Archive.SetAsTextPane(ScriptProcessor.PullArchiveText(), 0);
            Archive.JumpTo(1f);
            Shell.UpdateQueue.Add(Archive);
            Shell.RenderQueue.Add(Archive);
            Button Back = new Button("BACKBUTTON_ARCHIVE", new Vector2(1180, 625), (TAtlasInfo)Shell.AtlasDirectory["BACKBUTTON"], 1f);
            Back.SubscribeToEvent(WorldEntity.EventNames.ButtonPressFunction, typeof(ButtonScripts).GetMethod("CloseArchive"), null);
            Shell.UpdateQueue.Add(Back);
            Shell.RenderQueue.Add(Back);
        }
        public static void CloseArchive()
        {
            foreach (WorldEntity E in Shell.UpdateQueue)
            {
                if(E.OverlayUtility) { continue; }
                if (E is VerticalScrollPane && E.Name == "ARCHIVE_SCROLLBAR" && !Shell.DeleteQueue.Contains(E))
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
        public static void HideUI(Boolean includeHideButton)
        {
            UIHideEnabled = true;
            //ScriptProcessor.AllowScriptShift = false;
            Shell.AllowEnter = false;
            foreach (WorldEntity worldEntity in Shell.UpdateQueue)
            {
                if (worldEntity.OverlayUtility) { continue; }
                else if (worldEntity.IsUIElement)
                {
                    if (!(worldEntity is Button && worldEntity.Name == "BUTTON_HIDE_UI") || includeHideButton)
                    {
                        worldEntity.Drawable = false;
                        worldEntity.SuppressClickable = true;
                    }
                    if (worldEntity is Button && (worldEntity.Name != "BUTTON_HIDE_UI" || includeHideButton))
                    {
                        ((Button)worldEntity).Enabled = false;
                    }
                    if (worldEntity is ITextInputReceiver)
                    {
                        ((ITextInputReceiver)worldEntity).Active = false;
                    }
                    if (worldEntity is ScrollBar)
                    {
                        ((ScrollBar)worldEntity).Enabled = false;
                    }
                    if (worldEntity is VerticalScrollPane)
                    {
                        ((VerticalScrollPane)worldEntity).Enabled = false;
                    }
                }
            }
        }
        public static void UnHideUI()
        {
            foreach (WorldEntity worldEntity in Shell.UpdateQueue)
            {
                if (worldEntity.IsUIElement)
                {
                    worldEntity.Drawable = true;
                    worldEntity.SuppressClickable = false;
                    if (worldEntity is Button)
                    {
                        ((Button)worldEntity).Enabled = true;
                    }
                    if (worldEntity is ITextInputReceiver && !(worldEntity is ToggleableTextInputField))
                    {
                        ((ITextInputReceiver)worldEntity).Active = true;
                    }
                    if (worldEntity is ScrollBar)
                    {
                        ((ScrollBar)worldEntity).Enabled = true;
                    }
                    if (worldEntity is VerticalScrollPane)
                    {
                        ((VerticalScrollPane)worldEntity).Enabled = true;
                        worldEntity.Drawable = true;
                    }
                }
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
                S.ForceInsertScriptElement(new object[] { "C|GWS:CONTINUE", "T||You can't leave the area right now!" }, true);
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
                Button B = ButtonScripts.GetQuickButton("Travel to the SOURCE, home of the Mystic Sofia.");
                B.ButtonPressFunction += new VoidDel(delegate ()
                {
                    Shell.GlobalWorldState = "EXITING";
                    CloseNavScreen(true);
                    ((ScriptProcessor.ScriptSniffer)ScriptProcessor.SnifferSearch()).ScriptThrowTarget = Sofia.NavigateToMystic();
                    ScriptProcessor.SearchAndThrowForExit();
                }
                );
                B.QuickMoveTo(ButtonLocs[i]);
                Shell.UpdateQueue.Add(B);
                Shell.RenderQueue.Add(B);
                i++;
            }
            if (!(Sofia.GetContextualLocation() == "KING"))
            {
                Button B = ButtonScripts.GetQuickButton("Travel to the CASTLE of the King Sofia.");
                B.ButtonPressFunction += new VoidDel(delegate ()
                {
                    Shell.GlobalWorldState = "EXITING";
                    CloseNavScreen(true);
                    ((ScriptProcessor.ScriptSniffer)ScriptProcessor.SnifferSearch()).ScriptThrowTarget = Sofia.NavigateToKing();
                    ScriptProcessor.SearchAndThrowForExit();
                }
                );
                B.QuickMoveTo(ButtonLocs[i]);
                Shell.UpdateQueue.Add(B);
                Shell.RenderQueue.Add(B);
                i++;
            }
            if (!(Sofia.GetContextualLocation() == "CROOKED"))
            {
                Button B = ButtonScripts.GetQuickButton("Travel to the hills and caves of the BADLANDS.");
                B.ButtonPressFunction += new VoidDel(delegate ()
                {
                    Shell.GlobalWorldState = "EXITING";
                    CloseNavScreen(true);
                    ((ScriptProcessor.ScriptSniffer)ScriptProcessor.SnifferSearch()).ScriptThrowTarget = Sofia.NavigateToCrooked();
                    ScriptProcessor.SearchAndThrowForExit();
                }
                );
                B.QuickMoveTo(ButtonLocs[i]);
                Shell.UpdateQueue.Add(B);
                Shell.RenderQueue.Add(B);
                i++;
            }
            Button Back = new Button("BACKBUTTON_NAVSCREEN", new Vector2(1110, 610), (TAtlasInfo)Shell.AtlasDirectory["BACKBUTTON"], 0.98f);
            Back.SubscribeToEvent(WorldEntity.EventNames.ButtonPressFunction, typeof(ButtonScripts).GetMethod("CloseNavScreen", new Type[0]), null);
            Shell.UpdateQueue.Add(Back);
            Shell.RenderQueue.Add(Back);
        }
        public static void CloseNavScreen()
        {
            CloseNavScreen(false);
        }
        public static void CloseNavScreen(Boolean hideCButtons)
        {
            foreach (WorldEntity worldEntity in Shell.UpdateQueue)
            {
                if (worldEntity.OverlayUtility) { continue; }
                if (worldEntity.IsUIElement)
                {
                    worldEntity.Drawable = true;
                    worldEntity.SuppressClickable = false;
                }
                if (worldEntity is Button && worldEntity.Drawable && !Shell.DeleteQueue.Contains(worldEntity)) { Shell.DeleteQueue.Add(worldEntity); }
                else if (worldEntity is Button) {
                    if (!hideCButtons || !worldEntity.Name.Contains("BUTTON_CUSTOM_"))
                    {
                        ((Button)worldEntity).Enabled = true;
                        worldEntity.Drawable = true;
                    }
                }
                if (worldEntity is ITextInputReceiver && !(worldEntity is ToggleableTextInputField))
                {
                    ((ITextInputReceiver)worldEntity).Active = true;
                }
                if (worldEntity is ScrollBar)
                {
                    ((ScrollBar)worldEntity).Enabled = true;
                    worldEntity.Drawable = true;
                }
                if (worldEntity is TextEntity) { worldEntity.Drawable = true; }
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
            foreach (WorldEntity worldEntity in Shell.UpdateQueue)
            {
                if (worldEntity.OverlayUtility) { continue; }
                //if (E is TextEntity) { E.Drawable = false; }
                if (worldEntity is Button) { ((Button)worldEntity).Enabled = false; }
                if (worldEntity is ITextInputReceiver) { ((ITextInputReceiver)worldEntity).Active = false; }
                if (worldEntity is MonitoringTextInputField) { ((MonitoringTextInputField)worldEntity).Enabled = false; }
                if (worldEntity is ScrollBar) { ((ScrollBar)worldEntity).Enabled = false; }
                if (worldEntity is VerticalScrollPane) { ((VerticalScrollPane)worldEntity).Enabled = false; }
            }
            WorldEntity pane = new WorldEntity("PAUSE_PANE", new Vector2(640, 360), (TAtlasInfo)Shell.AtlasDirectory["PAUSEMENUPANE"], 0.97f);
            pane.CenterOrigin = true;
            pane.CameraImmune = true;
            pane.ColourValue = new Color(200, 200, 200, 150);
            Shell.UpdateQueue.Add(pane);
            Shell.RenderQueue.Add(pane);
            Button back = new Button("BUTTON_PAUSE_RETURN", new Vector2(640, 180), (TAtlasInfo)Shell.AtlasDirectory["PAUSERETURNBUTTON"], 0.98f);
            back.SubscribeToEvent(WorldEntity.EventNames.ButtonPressFunction, typeof(ButtonScripts).GetMethod("Unpause"), null);
            back.CameraImmune = true;
            Shell.UpdateQueue.Add(back);
            Shell.RenderQueue.Add(back);
            Button saveButton = new Button("BUTTON_PAUSE_SAVE", new Vector2(640, 270), (TAtlasInfo)Shell.AtlasDirectory["PAUSESAVEBUTTON"], 0.98f);
            saveButton.SubscribeToEvent(WorldEntity.EventNames.ButtonPressFunction, typeof(ButtonScripts).GetMethod("Save"), null);
            saveButton.CameraImmune = true;
            Shell.UpdateQueue.Add(saveButton);
            Shell.RenderQueue.Add(saveButton);
            Button settings = new Button("BUTTON_PAUSE_SETTINGS", new Vector2(640, 360), (TAtlasInfo)Shell.AtlasDirectory["PAUSESETTINGSBUTTON"], 0.98f);
            settings.SubscribeToEvent(WorldEntity.EventNames.ButtonPressFunction, typeof(ButtonScripts).GetMethod("ShowSettings"), null);
            settings.CameraImmune = true;
            Shell.UpdateQueue.Add(settings);
            Shell.RenderQueue.Add(settings);
            Button mainMenu = new Button("BUTTON_PAUSE_MAINMENU", new Vector2(640, 450), (TAtlasInfo)Shell.AtlasDirectory["PAUSEMAINMENUBUTTON"], 0.98f);
            mainMenu.SubscribeToEvent(WorldEntity.EventNames.ButtonPressFunction, typeof(ButtonScripts).GetMethod("BackToMainMenu"), null);
            mainMenu.CameraImmune = true;
            Shell.UpdateQueue.Add(mainMenu);
            Shell.RenderQueue.Add(mainMenu);
            Button quitButton = new Button("BUTTON_PAUSE_QUIT", new Vector2(640, 540), (TAtlasInfo)Shell.AtlasDirectory["PAUSEQUITBUTTON"], 0.98f);
            quitButton.SubscribeToEvent(WorldEntity.EventNames.ButtonPressFunction, typeof(ButtonScripts).GetMethod("Quit"), null);
            quitButton.CameraImmune = true;
            Shell.UpdateQueue.Add(quitButton);
            Shell.RenderQueue.Add(quitButton);
        }
        public static void Unpause()
        {
            foreach (WorldEntity worldEntity in Shell.UpdateQueue)
            {
                if (worldEntity.OverlayUtility) { continue; }
                if (worldEntity.Name == "PAUSE_PANE" && !Shell.DeleteQueue.Contains(worldEntity)) { Shell.DeleteQueue.Add(worldEntity); }
                if (worldEntity is Button && worldEntity.Name == "BUTTON_PAUSE_RETURN" && !Shell.DeleteQueue.Contains(worldEntity)) { Shell.DeleteQueue.Add(worldEntity); }
                else if (worldEntity is Button && worldEntity.Name == "BUTTON_PAUSE_SAVE" && !Shell.DeleteQueue.Contains(worldEntity)) { Shell.DeleteQueue.Add(worldEntity); }
                else if (worldEntity is Button && worldEntity.Name == "BUTTON_PAUSE_SETTINGS" && !Shell.DeleteQueue.Contains(worldEntity)) { Shell.DeleteQueue.Add(worldEntity); }
                else if (worldEntity is Button && worldEntity.Name == "BUTTON_PAUSE_MAINMENU" && !Shell.DeleteQueue.Contains(worldEntity)) { Shell.DeleteQueue.Add(worldEntity); }
                else if (worldEntity is Button && worldEntity.Name == "BUTTON_PAUSE_QUIT" && !Shell.DeleteQueue.Contains(worldEntity)) { Shell.DeleteQueue.Add(worldEntity); }
                else if (worldEntity is Button) { ((Button)worldEntity).Enabled = true; }
                if (worldEntity is MonitoringTextInputField) { ((MonitoringTextInputField)worldEntity).Enabled = true; }
                if (worldEntity is ScrollBar) { ((ScrollBar)worldEntity).Enabled = true; }
                if (worldEntity is VerticalScrollPane) { ((VerticalScrollPane)worldEntity).Enabled = true; }
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
            Pane.CameraImmune = true;
            Shell.UpdateQueue.Add(Pane);
            Shell.RenderQueue.Add(Pane);
            Button Yes = new Button("BUTTON_EXIT_YES", new Vector2(560, 380), (TAtlasInfo)Shell.AtlasDirectory["QUITYESBUTTON"], 1f);
            Yes.ButtonPressFunction += new VoidDel(delegate () { Shell.ExitOut = true; });
            Yes.CameraImmune = true;
            Shell.UpdateQueue.Add(Yes);
            Shell.RenderQueue.Add(Yes);
            Button No = new Button("BUTTON_EXIT_NO", new Vector2(700, 380), (TAtlasInfo)Shell.AtlasDirectory["QUITNOBUTTON"], 1f);
            No.SubscribeToEvent(WorldEntity.EventNames.ButtonPressFunction, typeof(ButtonScripts).GetMethod("Unquit"), null);
            No.CameraImmune = true;
            Shell.UpdateQueue.Add(No);
            Shell.RenderQueue.Add(No);
        }
        public static void Unquit()
        {
            foreach (WorldEntity E in Shell.UpdateQueue)
            {
                if (E.OverlayUtility) { continue; }
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
                if (ScriptProcessor.PastStates.Peek() is null)
                {
                    Shell.WriteLine("Could not rollback script state, as the previous state was null (did not save correctly).");
                    return;
                }
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
        public static void StartTutorial()
        {
            MediaPlayer.Stop();
            SpoonsTrip = true;
            Shell.RunQueue = new List<VoidDel>();
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
        private static Boolean s_spoonsTrip = true;
        public static Boolean SpoonsTrip
        {
            get { return s_spoonsTrip; }
            set
            {
                s_spoonsTrip = value;
                Shell.UpdateFlag("SPOONSTRIP", s_spoonsTrip);
            }
        }
        public static void OpenUSWMainMenu()
        {
            Shell.BackdropColour = Color.Black;
            Shell.AutoCamera.AutoSnapToOnResetEntityName = "";
            Shell.AutoCamera.RecenterPosition = Shell.Resolution / 2;
            Shell.AutoCamera.RecenterCamera();
            ScriptProcessor.AssertGameRunningWithoutScript = false;
            Shell.GlobalVoid = new VoidDel(() => { StartScript("USW_MAIN_MENU_CONSTRUCTOR", false); });
        }
        public static void OpenMainMenu()
        {
            Shell.BackdropColour = Color.Black;
            Shell.AutoCamera.AutoSnapToOnResetEntityName = "";
            Shell.AutoCamera.RecenterPosition = Shell.Resolution / 2;
            Shell.AutoCamera.RecenterCamera(); //Can still be moved as a sticker after this, add something to bump it
            ScriptProcessor.AssertGameRunningWithoutScript = false;
            Shell.GlobalVoid = new VoidDel(() => { StartScript("MAIN_MENU_CONSTRUCTOR", false); });
        }
        public static void StartTest()
        {
            MediaPlayer.Stop();
            SpoonsTrip = true;
            Shell.RunQueue = new List<VoidDel>();
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
        public static void StartScript(String ScriptName, Boolean startAsFresh)
        {
            if (startAsFresh)
            {
                SpoonsTrip = true;
                MediaPlayer.Stop();
                Sofia.ParticleFire.Cease = true;
                Shell.RunQueue = new List<VoidDel>();
                foreach (WorldEntity E in Shell.UpdateQueue)
                {
                    if (E.OverlayUtility) { continue; }
                    if (!Shell.DeleteQueue.Contains(E)) { Shell.DeleteQueue.Add(E); }
                }
                foreach (WorldEntity E in Shell.RenderQueue)
                {
                    if (E.OverlayUtility) { continue; }
                    if (!Shell.DeleteQueue.Contains(E)) { Shell.DeleteQueue.Add(E); }
                }
                ScriptProcessor.PastStates.Clear();
            }
            else
            {
                ScriptProcessor.ScriptSniffer curSniffer = ScriptProcessor.SnifferSearch();
                if(curSniffer != null && !Shell.DeleteQueue.Contains(curSniffer))
                {
                    Shell.DeleteQueue.Add(curSniffer);
                }
            }
            Shell.UpdateQueue.Add(new ScriptProcessor.ScriptSniffer(ScriptName + "_SNIFFER", ScriptProcessor.RetrieveScriptByName(ScriptName), ScriptName));
        }
        public static void BackToMainMenu()
        {
            foreach (WorldEntity E in Shell.UpdateQueue)
            {
                if (E.OverlayUtility) { continue; }
                if (!Shell.DeleteQueue.Contains(E) && !(E is Sofia.UpwardParticle) && !(E is Sofia.Transient)) { Shell.DeleteQueue.Add(E); }
            }
            foreach (WorldEntity E in Shell.RenderQueue)
            {
                if (E.OverlayUtility) { continue; }
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
                if (E.OverlayUtility) { continue; }
                if (!Shell.DeleteQueue.Contains(E) && E is Button) { Shell.DeleteQueue.Add(E); }
            }
            String[] Saves = LoadManager.FetchSaves();
            LoadManager.MaxPage = (int)Math.Ceiling((Saves.Length / 6f));
            if(LoadManager.MaxPage < 1) { LoadManager.MaxPage = 1; }
            LoadManager.PageNumber = LoadManager.MaxPage;
            Button Back = new Button("BACKBUTTON_LOADSCREEN", new Vector2(1180, 625), (TAtlasInfo)Shell.AtlasDirectory["BACKBUTTON"], 0.99f);
            Back.SubscribeToEvent(WorldEntity.EventNames.ButtonPressFunction, typeof(ButtonScripts).GetMethod("BackToMainMenu"), null);
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
                if (E.OverlayUtility) { continue; }
                if (!Shell.DeleteQueue.Contains(E) && E is Button) { Shell.DeleteQueue.Add(E); }
            }
            String CreditString = "This application is running the eVent horizoN Framework, lovingly hand-coded by Blackhole.\n\nAdditional programming also by Blackhole.\nScripting by Blackhole.\nUI design Blackhole.\n\nWritten using the MonoGame framework (www.monogame.net).";
            //String Ardata = "Ardata Carmia is a young Alternian troll living in Outglut during the time period of Hiveswap and Hiveswap Friendsim. Ardata was first revealed during the Hiveswap Troll Call event alongside Marvus Xoloto. Along with him, she is one of the few trolls whose sign is unknown; it is not visible on any of her sprites and was obscured by her cape on her Troll Call card. Ardata later went on to be one of two trolls to be featured in Hiveswap Friendship Simulator: Volume One after its release on April 13th, 2018, alongside her fellow Troll Call troll, Diemen Xicali.\n\nA high - ranking cerulean - blood, Ardata maintains a significant following on social media by playing up her sinister personality, torturing captives in her basement on camera, and utilizing them as slaves with the aid of her mind control abilities.\n\nArdata's Troll Call card described her as \"bloodthirsty on main\" (later revealed to be very accurate), \"probably Vriska\" and \"fresh to death sentence\".\n\n";
            Texture2D[] SB = CreateDynamicScroll(CreditString, 1000);
            ScrollBar Credits = new ScrollBar("CREDIT_SCROLLBAR", new Vector2(1080, 95), (TAtlasInfo)Shell.AtlasDirectory["SCROLLBAR"], 0.98f, SB, 600);
            Shell.UpdateQueue.Add(Credits);
            Shell.RenderQueue.Add(Credits);
            Button Back = new Button("BACKBUTTON_CREDITS", new Vector2(1180, 625), (TAtlasInfo)Shell.AtlasDirectory["BACKBUTTON"], 1f);
            Back.SubscribeToEvent(WorldEntity.EventNames.ButtonPressFunction, typeof(ButtonScripts).GetMethod("BackToMainMenu"), null);
            Shell.UpdateQueue.Add(Back);
            Shell.RenderQueue.Add(Back);
        }
        public static void ShowSettings()
        {
            if (!Paused)
            {
                foreach (WorldEntity E in Shell.UpdateQueue)
                {
                    if (E.OverlayUtility) { continue; }
                    if (!Shell.DeleteQueue.Contains(E) && E is Button) { Shell.DeleteQueue.Add(E); }
                }
            }
            else
            {
                foreach (WorldEntity E in Shell.UpdateQueue)
                {
                    if (E.OverlayUtility) { continue; }
                    if (E is Button) { ((Button)E).Enabled = false; }
                    if (E is ScrollBar) { ((ScrollBar)E).Enabled = false; }
                    if (E is Slider) { ((Slider)E).Enabled = false; }
                }
            }
            WorldEntity SettingsPane = new WorldEntity("PANE_SETTINGS", new Vector2(), (TAtlasInfo)Shell.AtlasDirectory["SETTINGSPANE"], 0.99f);
            SettingsPane.CameraImmune = true;
            Shell.UpdateQueue.Add(SettingsPane);
            Shell.RenderQueue.Add(SettingsPane);
            Checkbox Fullscreen = new Checkbox("CHECKBOX_SETTINGS_FULLSCREEN", new Vector2(140, 200), (TAtlasInfo)Shell.AtlasDirectory["CHECKBOX"], 0.991f, Shell.QueryFullscreen());
            Fullscreen.CameraImmune = true;
            Fullscreen.ButtonPressFunction += new VoidDel(delegate ()
                {
                    if (Shell.CaptureFullscreen.Toggle) { if (!Shell.QueryFullscreen()) { Shell.ToggleFullscreen(); } }
                    else { if (Shell.QueryFullscreen()) { Shell.ToggleFullscreen(); } }
                });
            Shell.CaptureFullscreen = Fullscreen;
            Shell.UpdateQueue.Add(Fullscreen);
            Shell.RenderQueue.Add(Fullscreen);
            TextEntity FSLabel = new TextEntity("LABEL_SETTINGS_FULLSCREEN", "Run in fullscreen", new Vector2(175, 185), 0.991f);
            FSLabel.CameraImmune = true;
            FSLabel.TypeWrite = false;
            Shell.UpdateQueue.Add(FSLabel);
            Shell.RenderQueue.Add(FSLabel);
            TextEntity VLabel = new TextEntity("LABEL_SETTINGS_VOLUME", "Volume control", new Vector2(135, 235), 0.991f);
            VLabel.CameraImmune = true;
            VLabel.TypeWrite = false;
            Shell.UpdateQueue.Add(VLabel);
            Shell.RenderQueue.Add(VLabel);
            WorldEntity VolumeBar = new WorldEntity("BAR_SETTINGS_VOLUME", new Vector2(135, 285), (TAtlasInfo)Shell.AtlasDirectory["SLIDERBAR"], 0.9905f);
            VolumeBar.CameraImmune = true;
            Shell.UpdateQueue.Add(VolumeBar);
            Shell.RenderQueue.Add(VolumeBar);
            Slider Volume = new Slider("SLIDER_SETTINGS_VOLUME", new Vector2(140, 295), (TAtlasInfo)Shell.AtlasDirectory["SLIDERKNOB"], 0.991f, new Vector2(140, 295), new Vector2(630, 295), Shell.GlobalVolume);
            Volume.CameraImmune = true;
            if (Shell.Mute)
            {
                Volume.ForceState(0f);
                Volume.Enabled = false;
            }
            Shell.CaptureVolume = Volume;
            Shell.UpdateQueue.Add(Volume);
            Shell.RenderQueue.Add(Volume);
            Checkbox Mute = new Checkbox("CHECKBOX_SETTINGS_MUTE", new Vector2(140, 355), (TAtlasInfo)Shell.AtlasDirectory["CHECKBOX"], 0.991f, Shell.Mute);
            Mute.CameraImmune = true;
            Mute.ButtonPressFunction += new VoidDel(delegate ()
                {
                    if (Shell.CaptureMute.Toggle) { Shell.Mute = true; }
                    else { Shell.Mute = false; }
                });
            Shell.CaptureMute = Mute;
            Shell.UpdateQueue.Add(Mute);
            Shell.RenderQueue.Add(Mute);
            TextEntity MLabel = new TextEntity("LABEL_SETTINGS_MUTE", "Mute audio", new Vector2(175, 340), 0.991f);
            MLabel.CameraImmune = true;
            MLabel.TypeWrite = false;
            Shell.UpdateQueue.Add(MLabel);
            Shell.RenderQueue.Add(MLabel);
            TextEntity TRateLabel = new TextEntity("LABEL_SETTINGS_TEXTRATE", "Text speed", new Vector2(135, 400), 0.991f);
            TRateLabel.CameraImmune = true;
            TRateLabel.TypeWrite = false;
            Shell.UpdateQueue.Add(TRateLabel);
            Shell.RenderQueue.Add(TRateLabel);
            WorldEntity TextBar = new WorldEntity("BAR_SETTINGS_TEXTRATE", new Vector2(135, 450), (TAtlasInfo)Shell.AtlasDirectory["SLIDERBAR"], 0.9905f);
            TextBar.CameraImmune = true;
            Shell.UpdateQueue.Add(TextBar);
            Shell.RenderQueue.Add(TextBar);
            Slider Textrate = new Slider("SLIDER_SETTINGS_TEXTRATE", new Vector2(140, 460), (TAtlasInfo)Shell.AtlasDirectory["SLIDERKNOB"], 0.991f, new Vector2(140, 460), new Vector2(630, 460), TextEntity.GetSliderValueFromTicks(TextEntity.TickWriteInterval));
            Textrate.CameraImmune = true;
            Shell.CaptureTextrate = Textrate;
            Shell.UpdateQueue.Add(Textrate);
            Shell.RenderQueue.Add(Textrate);
            TextEntity DynamicTextrate = new TextEntity("DYNAMIC_SETTINGS_TEXTRATE_DISPLAY", TextEntity.TickWriteInterval + " milliseconds", new Vector2(350, 400), 0.991f);
            DynamicTextrate.CameraImmune = true;
            DynamicTextrate.TypeWrite = false;
            Shell.CaptureRateDisplay = DynamicTextrate;
            Shell.UpdateQueue.Add(DynamicTextrate);
            Shell.RenderQueue.Add(DynamicTextrate);
            Checkbox SSSave = new Checkbox("CHECKBOX_SETTINGS_SAVES", new Vector2(140, 520), (TAtlasInfo)Shell.AtlasDirectory["CHECKBOX"], 0.991f, SaveLoadModule.ApplicableSaveType == "ScriptStem");
            SSSave.ButtonPressFunction += new VoidDel(delegate ()
            {
                if (Shell.CaptureSaveType.Toggle) { SaveLoadModule.ApplicableSaveType = "ScriptStem"; }
                else { SaveLoadModule.ApplicableSaveType = "FullySerializedBinary"; }
            });
            SSSave.CameraImmune = true;
            Shell.CaptureSaveType = SSSave;
            Shell.UpdateQueue.Add(SSSave);
            Shell.RenderQueue.Add(SSSave);
            TextEntity SLabel = new TextEntity("LABEL_SETTINGS_SAVES", "Enable simple saves", new Vector2(175, 505), 0.991f);
            SLabel.CameraImmune = true;
            SLabel.TypeWrite = false;
            Shell.UpdateQueue.Add(SLabel);
            Shell.RenderQueue.Add(SLabel);
            TextEntity SLabel2 = new TextEntity("LABEL_SETTINGS_SAVES2", "[C:220-100-255-255]Simple saves ensure version compatibility and take up less space, but may not work with unsupported scripts.", new Vector2(660, 450), 0.991f);
            SLabel2.CameraImmune = true;
            SLabel2.BufferLength = 500;
            SLabel2.TypeWrite = false;
            Shell.UpdateQueue.Add(SLabel2);
            Shell.RenderQueue.Add(SLabel2);
            if (!Paused)
            {
                Button Back = new Button("BACKBUTTON_SETTINGS", new Vector2(1110, 610), (TAtlasInfo)Shell.AtlasDirectory["BACKBUTTON"], 1f);
                Back.CameraImmune = true;
                Back.SubscribeToEvent(WorldEntity.EventNames.ButtonPressFunction, typeof(ButtonScripts).GetMethod("BackToMainMenu"), null);
                Shell.UpdateQueue.Add(Back);
                Shell.RenderQueue.Add(Back);
            }
            else
            {
                Button Back = new Button("BACKBUTTON_SETTINGS", new Vector2(1110, 610), (TAtlasInfo)Shell.AtlasDirectory["BACKBUTTON"], 1f);
                Back.CameraImmune = true;
                Back.SubscribeToEvent(WorldEntity.EventNames.ButtonPressFunction, typeof(ButtonScripts).GetMethod("UnSettings"), null);
                Shell.UpdateQueue.Add(Back);
                Shell.RenderQueue.Add(Back);
            }
            Button Restore = new Button("BUTTON_SETTINGS_RESTORE", new Vector2(280, 620), (TAtlasInfo)Shell.AtlasDirectory["RESTOREBUTTON"], 1f);
            Restore.CameraImmune = true;
            Restore.SubscribeToEvent(WorldEntity.EventNames.ButtonPressFunction, typeof(Shell).GetMethod("DefaultSettings"), null);
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
                if (E.OverlayUtility) { continue; }
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
            //Update to report invalid save
            Shell.PlaySoundInstant("UT_SAVE");
            int SaveResult = SaveLoadModule.WriteSave(Thumb);
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
            SaveWrittenPane.CameraImmune = true;
            SaveWrittenPane.CenterOrigin = true;
            Shell.UpdateQueue.Add(SaveWrittenPane);
            Shell.RenderQueue.Add(SaveWrittenPane);
            Button Back = new Button("BUTTON_SAVE_BACK", new Vector2(640, 360), (TAtlasInfo)Shell.AtlasDirectory["BACKBUTTON"], 0.991f);
            Back.CameraImmune = true;
            Back.SubscribeToEvent(WorldEntity.EventNames.ButtonPressFunction, typeof(ButtonScripts).GetMethod("UnSave"), null);
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
            SavePane.CameraImmune = true;
            SavePane.CenterOrigin = true;
            Shell.UpdateQueue.Add(SavePane);
            Shell.RenderQueue.Add(SavePane);
            TAtlasInfo ThisThumb = (TAtlasInfo)Shell.AtlasDirectory["THUMBBLANK"];
            ThisThumb.Atlas = SaveThumb;
            WorldEntity Thumb = new WorldEntity("THUMB_SAVE", new Vector2(480, 233), ThisThumb, 0.991f);
            Thumb.CameraImmune = true;
            Shell.UpdateQueue.Add(Thumb);
            Shell.RenderQueue.Add(Thumb);
            DateTime Now = DateTime.Now;
            String Time = Now.ToShortDateString() + " " + Now.ToShortTimeString();
            TextEntity TimeText = new TextEntity("TIMETEXT_NEWSAVE", Time, new Vector2(640, 323) - new Vector2(Shell.Default.MeasureString(Time).X / 2f, -110), 0.991f);
            TimeText.CameraImmune = true;
            TimeText.TypeWrite = false;
            Shell.UpdateQueue.Add(TimeText);
            Shell.RenderQueue.Add(TimeText);
            Button Yes = new Button("BUTTON_SAVE_YES", new Vector2(530, 530), (TAtlasInfo)Shell.AtlasDirectory["YESBUTTON"], 0.991f);
            Yes.CameraImmune = true;
            Yes.SubscribeToEvent(WorldEntity.EventNames.ButtonPressFunction, typeof(ButtonScripts).GetMethod("SaveActual"), new object[] { SaveThumb });
            Shell.UpdateQueue.Add(Yes);
            Shell.RenderQueue.Add(Yes);
            Button No = new Button("BUTTON_SAVE_NO", new Vector2(750, 530), (TAtlasInfo)Shell.AtlasDirectory["NOBUTTON"], 0.991f);
            No.CameraImmune = true;
            No.SubscribeToEvent(WorldEntity.EventNames.ButtonPressFunction, typeof(ButtonScripts).GetMethod("UnSave"), null);
            Shell.UpdateQueue.Add(No);
            Shell.RenderQueue.Add(No);
        }
        public static void UnSave()
        {
            foreach (WorldEntity E in Shell.UpdateQueue)
            {
                if (E.OverlayUtility) { continue; }
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
                Add.SubscribeToEvent(WorldEntity.EventNames.EntityClickFunction, typeof(ButtonScripts).GetMethod("UIBoxClick"), null);
                Add.IsUIElement = true;
                Shell.UpdateQueue.Add(Add);
                Shell.RenderQueue.Add(Add);
            }
            if (Shell.GetEntityByName("BUTTON_ARCHIVE") == null)
            {
                Button Archive = new Button("BUTTON_ARCHIVE", new Vector2(70, 510), (TAtlasInfo)Shell.AtlasDirectory["ARCHIVEBUTTON"], 0.95f);
                Archive.SubscribeToEvent(WorldEntity.EventNames.ButtonPressFunction, typeof(ButtonScripts).GetMethod("OpenArchive"), null);
                Archive.IsUIElement = true;
                Shell.UpdateQueue.Add(Archive);
                Shell.RenderQueue.Add(Archive);
            }
            if (Shell.GetEntityByName("BUTTON_SKIP") == null)
            {
                Button Skip = new Button("BUTTON_SKIP", new Vector2(70, 557), (TAtlasInfo)Shell.AtlasDirectory["SKIPBUTTON"], 0.95f);
                Skip.SubscribeToEvent(WorldEntity.EventNames.ButtonPressFunction, typeof(ButtonScripts).GetMethod("Skip"), null);
                Skip.IsUIElement = true;
                Shell.UpdateQueue.Add(Skip);
                Shell.RenderQueue.Add(Skip);
            }
            if (Shell.GetEntityByName("BUTTON_PAUSEMENU") == null)
            {
                Button PauseB = new Button("BUTTON_PAUSEMENU", new Vector2(70, 604), (TAtlasInfo)Shell.AtlasDirectory["PAUSEMENUBUTTON"], 0.95f);
                PauseB.SubscribeToEvent(WorldEntity.EventNames.ButtonPressFunction, typeof(ButtonScripts).GetMethod("Pause"), null);
                PauseB.IsUIElement = true;
                Shell.UpdateQueue.Add(PauseB);
                Shell.RenderQueue.Add(PauseB);
            }
            if (Shell.GetEntityByName("BUTTON_ROLLBACK") == null)
            {
                Button Return = new Button("BUTTON_ROLLBACK", new Vector2(70, 651), (TAtlasInfo)Shell.AtlasDirectory["RETURNBUTTON"], 0.95f);
                Return.SubscribeToEvent(WorldEntity.EventNames.ButtonPressFunction, typeof(ButtonScripts).GetMethod("ScriptRollback"), null);
                Return.IsUIElement = true;
                Shell.UpdateQueue.Add(Return);
                Shell.RenderQueue.Add(Return);
            }
            if (Shell.GetEntityByName("BUTTON_HIDE_UI") == null)
            {
                Checkbox HideUI = new Checkbox("BUTTON_HIDE_UI", new Vector2(1205, 650), (TAtlasInfo)Shell.AtlasDirectory["EYECHECKBOX"], 0.95f, false);
                HideUI.CenterOrigin = false;
                HideUI.SubscribeToEvent(WorldEntity.EventNames.ButtonPressFunction, typeof(ButtonScripts).GetMethod("RefreshUIHideState"), null);
                HideUI.IsUIElement = true;
                Shell.UpdateQueue.Add(HideUI);
                Shell.RenderQueue.Add(HideUI);
            }
        }
        public static void Skip()
        {
            foreach (WorldEntity E in Shell.UpdateQueue)
            {
                if (E.OverlayUtility) { continue; }
                if (E is ScriptProcessor.ScriptSniffer)
                {
                    ScriptProcessor.ScriptSniffer S = (ScriptProcessor.ScriptSniffer)E;
                    if(S.Skipping) { S.CeaseSkipping(); }
                    else { S.Skip(); }
                }
            }
        }
    }
}