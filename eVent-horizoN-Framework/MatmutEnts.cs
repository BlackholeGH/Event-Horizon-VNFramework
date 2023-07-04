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
using System.Collections;
using System.Text.RegularExpressions;

namespace VNFramework
{
    class MatmutEnts
    {
        public class MatmutMonitor : WorldEntity
        {
            public static Hashtable DataRecord = new Hashtable();
            public MatmutMonitor(String Name) : base(Name, new Vector2(), null, 0f)
            {

            }
            public static String GetResultString()
            {
                String PrefSocMed = "No answer chosen.";
                if (DataRecord.ContainsKey("PREF_SOCIALMEDIA")) { PrefSocMed = (String)DataRecord["PREF_SOCIALMEDIA"]; }
                String InternetUse = "No answer chosen.";
                if (DataRecord.ContainsKey("FREQ_INTERNET")) { InternetUse = (String)DataRecord["FREQ_INTERNET"]; }
                int CorrectAnswers = 0;
                if ((String)DataRecord["DATA_THEFT_RATE"] == "SECOND") { CorrectAnswers++; }
                if ((String)DataRecord["PERCENT_LEAKED"] == "42") { CorrectAnswers++; }
                if ((String)DataRecord["CRIMINAL"] == "HACKER") { CorrectAnswers++; }
                String Results = "Your results:\n\nYour preferred social media site: " + PrefSocMed + "[N][N]" +
                    "You use the internet: " + InternetUse + "[N][N]Quiz Results![N][N]" +
                    "Quiz Question 1: " + ((String)DataRecord["DATA_THEFT_RATE"] == "SECOND" ? "[C:0-255-0-255]Correct" : "[C:255-0-0-255]Incorrect") + "[N]" +
                    "Quiz Question 2: " + ((String)DataRecord["PERCENT_LEAKED"] == "42" ? "[C:0-255-0-255]Correct" : "[C:255-0-0-255]Incorrect") + "[N]" +
                    "Quiz Question 3: " + ((String)DataRecord["CRIMINAL"] == "HACKER" ? "[C:0-255-0-255]Correct" : "[C:255-0-0-255]Incorrect") + "[N]" +
                    "[N]Total score: [C:0-0-255-255]" + CorrectAnswers + "/3[N,C:WHITE]Percentage score: [C:0-0-255-255]" + (int)((CorrectAnswers / 3f) * 100) + "%";
                return Results;
            }
        }
        /*public static void OpenMatMutMenu()
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
        }*/
    }
}
