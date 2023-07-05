using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace VNFramework
{
    public partial class Shell : Game
    {
        private static object s_cWriteLockObj = new object();
        public static String PullInternalConsoleData
        {
            get
            {
                StringBuilder outputBuilder = new StringBuilder();
                for (int i = InternalLog.Count > 101 ? InternalLog.Count - 101 : 0; i < InternalLog.Count; i++)
                {
                    outputBuilder.Append("[I]");
                    foreach (object o in InternalLog[i])
                    {
                        if (o is Color)
                        {
                            Color textColour = (Color)o;
                            if (textColour == Color.White)
                            {
                                outputBuilder.Append("[F:SYSFONT]");
                            }
                            else
                            {
                                outputBuilder.Append(String.Format("[F:SYSFONT,C:{0}-{1}-{2}-{3}]", textColour.R, textColour.G, textColour.B, textColour.A));
                            }
                        }
                        else if (o is String)
                        {
                            outputBuilder.Append((String)o);
                        }
                    }
                    outputBuilder.Append("[N]");
                }
                outputBuilder.Remove(outputBuilder.Length - 3, 3);
                return outputBuilder.ToString();
            }
        }
        static String s_lastManualConsoleInput = "";
        public static String LastManualConsoleInput
        {
            get { return s_lastManualConsoleInput; }
        }
        public static void HandleConsoleInput(String input)
        {
            s_lastManualConsoleInput = input;
            SortedDictionary<int, Color> colours = new SortedDictionary<int, Color>();
            colours.Add(0, Color.LightGreen);
            WriteLine(input, colours);
            String[] commands = input.Split(' ');
            try
            {
                switch (commands[0].ToUpper())
                {
                    //Run ScriptProcessor commands as a forced script shift (by default, shift conditions are unchanged).
                    case "INSERT":
                        ScriptProcessor.ScriptSniffer foundSniffer = ScriptProcessor.SnifferSearch();
                        if (foundSniffer != null)
                        {
                            foundSniffer.ForceInsertScriptElement((input.Remove(0, input.IndexOf(' ') + 1)).Split(' '), false);
                        }
                        else { WriteLine("Cannot insert new script shift as a script is not running."); }
                        break;
                    //Activate a single script element.
                    case "ACTIVATE":
                        ScriptProcessor.ActivateScriptElement(input.Remove(0, input.IndexOf(' ') + 1));
                        break;
                    //Freshly load a new script.
                    case "LOAD":
                        WriteLine("Attempting to load script " + commands[1].ToUpper() + ".");
                        RunQueue.Add(new VoidDel(() => ButtonScripts.StartScript(commands[1].ToUpper(), true)));
                        break;
                    //Executes a function statement per the EntityFactory's inbuilt function parser.
                    case "DO":
                        RunQueue.Add(EntityFactory.AssembleVoidDelegate("do=" + input.Remove(0, input.IndexOf(' ') + 1)));
                        break;
                    //Executes a method specifier (instance return) per the EntityFactory's inbuilt function parser.
                    case "RUN":
                        RunQueue.Add(EntityFactory.AssembleVoidDelegate(input.Remove(0, input.IndexOf(' ') + 1)));
                        break;
                    //Fork to a new script from your current state. Equivalent to "do B|[Script name]".
                    case "FORK":
                        ScriptProcessor.ActivateScriptElement("B|" + commands[1].ToUpper());
                        break;
                    //Close the program.
                    case "QUIT":
                        WriteLine("Closing the VNF client...");
                        ExitOut = true;
                        break;
                    default:
                        WriteLine("Unrecognized command.");
                        break;
                }
            }
            catch (Exception e)
            {
                WriteLine(e.GetType().Name + ": " + e.Message);
            }
        }
        public static event Action<String> ConsoleWrittenTo;
        public static void WriteLine(String text)
        {
            WriteLine(text, null);
        }
        public static Boolean ConsoleWritesOverlay { get; set; }
        public static void WriteLine(String text, SortedDictionary<int, Color> colourArgs)
        {
            if (ConsoleWritesOverlay)
            {
                RequestDisplaySystemText(text);
            }
            String time = "(" + System.DateTime.Now.ToLongTimeString() + ")";
            if (s_hasConsole) { Console.WriteLine(time + " " + text); }
            text = text.Replace('[', '(').Replace(']', ')');
            ArrayList store = new ArrayList();
            store.Add(Color.Yellow);
            store.Add(time + " ");
            if (colourArgs != null)
            {
                Color rollingColour = Color.White;
                int lastI = 0;
                foreach (int i in colourArgs.Keys)
                {
                    if (i - lastI > 0)
                    {
                        String Seg = text.Substring(lastI, i - lastI);
                        store.Add(rollingColour);
                        store.Add(Seg);
                    }
                    rollingColour = colourArgs[i];
                    lastI = i;
                }
                if (lastI < text.Length)
                {
                    store.Add(rollingColour);
                    store.Add(text.Remove(0, lastI));
                }
            }
            else
            {
                store.Add(Color.White);
                store.Add(text);
            }
            object[] thisEntry = store.ToArray();
            InternalLog.Add(thisEntry);
            ConsoleWrittenTo?.Invoke(text);
            try
            {
                Monitor.Enter(s_cWriteLockObj);
                _lastLogLine = text;
            }
            finally { Monitor.Exit(s_cWriteLockObj); }
        }
        private static String _lastLogLine = "";
        public static String LastLogLine
        {
            get
            {
                String outString = "";
                try
                {
                    Monitor.Enter(s_cWriteLockObj);
                    outString = _lastLogLine;
                }
                finally { Monitor.Exit(s_cWriteLockObj); }
                return outString;
            }
        }
    }
}
