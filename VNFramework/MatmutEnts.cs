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
    }
}
