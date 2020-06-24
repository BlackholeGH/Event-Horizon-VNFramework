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
    public static partial class ScriptProcessor
    {
        public static readonly object[] ReceiveNav = new object[] { "C|GWS:CONTINUE", "T||You can use this map button to travel to different locations in ULTRASOFIAWORLD.", new VoidDel(delegate()
            {
                Shell.RunQueue.Add(new VoidDel(delegate()
                {
                    if(Shell.GetEntityByName("BUTTON_NAVSCREEN") == null)
                    {
                        Button Map = new Button("BUTTON_NAVSCREEN", new Vector2(75, 458), (TAtlasInfo)Shell.AtlasDirectory["MAPBUTTON"], 0.95f, ButtonScripts.DelegateFetch("opennavscreen"));
                        Map.MLCRecord = new String[] { "opennavscreen" };
                        Shell.UpdateQueue.Add(Map);
                        Shell.RenderQueue.Add(Map);
                    }
                    WorldEntity Add = new WorldEntity("NAVBUTTON_APPEARFLASHER", new Vector2(75, 458), (TAtlasInfo)Shell.AtlasDirectory["MAPBUTTON"], 0.96f);
                    Add.CenterOrigin = true;
                    Add.AnimationQueue.Add(Animation.Retrieve("SLOWOSCILLATE"));
                    Add.TransientAnimation = true;
                    Shell.UpdateQueue.Add(Add);
                    Shell.RenderQueue.Add(Add);
                }));
            })
            };
        public static readonly object[] ScriptFadeInOut = new object[]
        {
            new object[] { "C|TIME:1600", "T||", new VoidDel(delegate()
            {
                Shell.RunQueue.Add(new VoidDel(delegate()
                {
                    WorldEntity Add = new WorldEntity("BLACK", new Vector2(0, 0), (TAtlasInfo)Shell.AtlasDirectory["BLACK"], 0.9601f);
                    Add.ColourValue = new Color(0,0,0,0);
                    Add.AnimationQueue.Add(Animation.Retrieve("FADEIN"));
                    Shell.UpdateQueue.Add(Add);
                    Shell.RenderQueue.Add(Add);
                }));
            })
            },
            new object[] { "M|#NULL", "C|TIME:0", new VoidDel(delegate()
            {
                ClearNonUIEntities();
                foreach(WorldEntity E in Shell.UpdateQueue)
                {
                    if(E.Name == "BLACK")
                    {
                        E.AnimationQueue.Add(Animation.Retrieve("FADEOUT"));
                        E.TransientAnimation = true;
                        break;
                    }
                }
            })
            },
            new object[] { "B|#SCRIPTTHROWTARGET" }
        };
        public static readonly object[] InitMysticBasic = new object[] { "C|GWS:CONTINUE", "T||You locate the entrance to the [R]SOURCE[C:WHITE] cave, and venture inside...", new VoidDel(delegate()
            {
                Shell.RunQueue.Add(new VoidDel(delegate()
                {
                    WorldEntity Add = new WorldEntity("SOURCECAVEBG", new Vector2(-75, -180), (TAtlasInfo)Shell.AtlasDirectory["SOURCECAVEBG"], 0.05f);
                    Add.ColourValue = new Color(0,0,0,0);
                    Add.AnimationQueue.Add(Animation.Retrieve("FADEIN"));
                    Shell.UpdateQueue.Add(Add);
                    Shell.RenderQueue.Add(Add);
                    Add = new WorldEntity("SOFIA", new Vector2(350, 405), (TAtlasInfo)Shell.AtlasDirectory["SOFIASPRITES"], 0.5f);
                    Add.ColourValue = new Color(0,0,0,0);
                    Add.CenterOrigin = true;
                    Add.SetAtlasFrame(new Point(0, 2));
                    Add.Scale(new Vector2(-0.06f, -0.06f));
                    Shell.UpdateQueue.Add(Add);
                    Shell.RenderQueue.Add(Add);
                    Sofia.SourceGlow S = new Sofia.SourceGlow("SOURCE", new Vector2(-75, -180), (TAtlasInfo)Shell.AtlasDirectory["SOURCE"], 0.06f);
                    S.ColourValue = new Color(0,0,0,0);
                    S.AnimationQueue.Add(Animation.Retrieve("FADEIN"));
                    Shell.UpdateQueue.Add(S);
                    Shell.RenderQueue.Add(S);
                    Add = new WorldEntity("MYSTIC SOFIA", new Vector2(930, 405), (TAtlasInfo)Shell.AtlasDirectory["MYSTICSOFIA"], 0.48f);
                    Add.ColourValue = new Color(0,0,0,0);
                    Add.CenterOrigin = true;
                    Add.SetAtlasFrame(new Point(0, 0));
                    Add.Scale(new Vector2(-0.06f, -0.06f));
                    Shell.UpdateQueue.Add(Add);
                    Shell.RenderQueue.Add(Add);
                }));
            })
        };
        public static readonly object[] InitCrookedBasic = new object[] { "C|GWS:CONTINUE", "T||You return to badland caves, and manage to locate the Crooked Sofia's business establishment.", new VoidDel(delegate()
            {
                Shell.RunQueue.Add(new VoidDel(delegate()
                {
                    WorldEntity Add = new WorldEntity("CROOKEDCAVEBG", new Vector2(0, -160), (TAtlasInfo)Shell.AtlasDirectory["CROOKEDCAVEBG"], 0.05f);
                    Add.ColourValue = new Color(0,0,0,0);
                    Add.AnimationQueue.Add(Animation.Retrieve("FADEIN"));
                    Shell.UpdateQueue.Add(Add);
                    Shell.RenderQueue.Add(Add);
                    Add = new WorldEntity("SOFIA", new Vector2(350, 405), (TAtlasInfo)Shell.AtlasDirectory["SOFIASPRITES"], 0.5f);
                    Add.ColourValue = new Color(0,0,0,0);
                    Add.AnimationQueue.Add(Animation.Retrieve("FADEIN"));
                    Add.CenterOrigin = true;
                    Add.SetAtlasFrame(new Point(0, 2));
                    Add.Scale(new Vector2(-0.06f, -0.06f));
                    Shell.UpdateQueue.Add(Add);
                    Shell.RenderQueue.Add(Add);
                    Add = new WorldEntity("CRIME SHACK", new Vector2(850, 300), (TAtlasInfo)Shell.AtlasDirectory["CRIMESHACK"], 0.46f);
                    Add.ColourValue = new Color(0,0,0,0);
                    Add.AnimationQueue.Add(Animation.Retrieve("FADEIN"));
                    Add.CenterOrigin = true;
                    Add.Scale(new Vector2(-0.2f, -0.2f));
                    Shell.RenderQueue.Add(Add);
                    Shell.UpdateQueue.Add(Add);
                    Add = new WorldEntity("CROOKED SOFIA", new Vector2(830, 405), (TAtlasInfo)Shell.AtlasDirectory["CROOKEDSOFIA"], 0.48f);
                    Add.ColourValue = new Color(0,0,0,0);
                    Add.CenterOrigin = true;
                    Add.SetAtlasFrame(new Point(0, 0));
                    Add.Scale(new Vector2(-0.06f, -0.06f));
                    Shell.UpdateQueue.Add(Add);
                    Shell.RenderQueue.Add(Add);
                }));
            })
        };
        public static readonly object[] InitKingExtBasic = new object[] { "C|GWS:CONTINUE", "T||You make your way to the King Sofia's castle.", new VoidDel(delegate()
            {
                Shell.RunQueue.Add(new VoidDel(delegate()
                {
                    WorldEntity Add = new WorldEntity("CASTLEEXTBG", new Vector2(0, 0), (TAtlasInfo)Shell.AtlasDirectory["CASTLEEXTBG"], 0.05f);
                    Add.ColourValue = new Color(0,0,0,0);
                    Add.AnimationQueue.Add(Animation.Retrieve("FADEIN"));
                    Shell.UpdateQueue.Add(Add);
                    Shell.RenderQueue.Add(Add);
                    Add = new WorldEntity("SOFIA", new Vector2(350, 405), (TAtlasInfo)Shell.AtlasDirectory["SOFIASPRITES"], 0.5f);
                    Add.ColourValue = new Color(0,0,0,0);
                    Add.AnimationQueue.Add(Animation.Retrieve("FADEIN"));
                    Add.CenterOrigin = true;
                    Add.SetAtlasFrame(new Point(1, 0));
                    Add.Scale(new Vector2(-0.06f, -0.06f));
                    Shell.UpdateQueue.Add(Add);
                    Shell.RenderQueue.Add(Add);
                }));
            })
        };
        public static readonly object[] InitKingIntBasic = new object[] { "C|GWS:CONTINUE", "T||You are waved through the castle entranceway into its throne room.", new VoidDel(delegate()
            {
                Shell.RunQueue.Add(new VoidDel(delegate()
                {
                    WorldEntity Add = new WorldEntity("CASTLEINTBG", new Vector2(0, 0), (TAtlasInfo)Shell.AtlasDirectory["CASTLEINTBG"], 0.05f);
                    Add.ColourValue = new Color(0,0,0,0);
                    Add.AnimationQueue.Add(Animation.Retrieve("FADEIN"));
                    Shell.UpdateQueue.Add(Add);
                    Shell.RenderQueue.Add(Add);
                    Add = new WorldEntity("SOFIA", new Vector2(-350, 405), (TAtlasInfo)Shell.AtlasDirectory["SOFIASPRITES"], 0.5f);
                    Add.ColourValue = new Color(0,0,0,0);
                    Add.AnimationQueue.Add(Animation.Retrieve("FADEIN"));
                    Add.CenterOrigin = true;
                    Add.SetAtlasFrame(new Point(1, 0));
                    Add.Scale(new Vector2(-0.06f, -0.06f));
                    Shell.UpdateQueue.Add(Add);
                    Shell.RenderQueue.Add(Add);
                    Add = new WorldEntity("KING SOFIA", new Vector2(930, 405), (TAtlasInfo)Shell.AtlasDirectory["KINGSOFIA"], 0.5f);
                    Add.ColourValue = new Color(0,0,0,0);
                    Add.CenterOrigin = true;
                    Add.SetAtlasFrame(new Point(1, 1));
                    Add.Scale(new Vector2(-0.06f, -0.06f));
                    Shell.UpdateQueue.Add(Add);
                    Shell.RenderQueue.Add(Add);
                }));
            })
        };
        public static Hashtable ScriptCache { get; set; }
        public static String[] ScriptIndex()
        {
            String[] SIndex = new String[]
            {
                "EXIT_DEFAULT",
                "EXIT_KING_PRIMARY",
                "EXIT_CROOKED_PRIMARY_FINALIZE",
                "TEST",
                "PATH_1",
                "PATH_2",
                "INTRO_PRELOAD",
                "VS_MAIN_INTRO",
                "SOFIA_MAIN_INTRO", //No nav
                "SOFIA_DRAW", //No nav
                "SOFIA_VENTURE", //No nav
                "SOFIA_WHATTHEHELL", //No nav
                "SOFIA_KING_PRIMARY", //Custom
                "SOFIA_CROOKED_PRIMARY", //No nav
                "SOFIA_CROOKED_PRIMARY_NOSHAKE", //No nav
                "SOFIA_CROOKED_PRIMARY_SHAKE", //No nav
                "SOFIA_CROOKED_PRIMARY_FINALIZE", //Custom
                "SOFIA_MYSTIC_NO_EDICT", // -Proper-
                "SOFIA_MYSTIC_EXPLANATION",
                "SOFIA_MYSTIC_STORY",
                "SOFIA_MYSTIC_AFTER_STORY",
                "SOFIA_MYSTIC_AFTER_STORY_SECONDARY",
                "SOFIA_MYSTIC_AFTER_STORY_REPEAT",
                "SOFIA_MYSTIC_AFTER_STORY_NO_REPEAT",
                "SOFIA_MYSTIC_EXPLANATION_RETURN",
                "SOFIA_MYSTIC_ESSENCE_QUEST",
                "SOFIA_MYSTIC_NO_ESSENCE", // -Proper-
                "SOFIA_MYSTIC_ESSENCE_PRIOR",
                "SOFIA_MYSTIC_POSTPONE", // -Proper-
                "SOFIA_MYSTIC_ESSENCE_RETURN_SUCCESS",
                "SOFIA_MYSTIC_ESSENCE_RETURN_FAILURE",
                "SOFIA_MYSTIC_FAILURE_KING_ADVICE", // -Proper-
                "SOFIA_MYSTIC_FAILURE_CROOKED_RECALL", // -Proper-
                "SOFIA_MYSTIC_FAILURE_NO_ADVICE", // -Proper-
                "SOFIA_MYSTIC_PREFINAL_RETURN",
                "SOFIA_MYSTIC_FINAL", //No nav
                "SOFIA_KING_SECONDARY",
                "SOFIA_KING_SECONDARY_INTERIOR",
                "SOFIA_KING_SECONDARY_FINALIZE", // -Proper-
                "SOFIA_KING_RETURN_POST_GOLEM_PRE_INTERIOR",
                "SOFIA_KING_RETURN_NO_EDICT",
                "SOFIA_KING_RETURN_CLUELESS", // -Proper-
                "SOFIA_KING_RETURN_NO_ESSENCE",
                "SOFIA_KING_RETURN_ESSENCE_SOLUTION",
                "SOFIA_KING_RETURN_NO_ESSENSE_NO_CROOKED", // -Proper-
                "SOFIA_KING_RETURN_NO_ESSENSE_YES_CROOKED", // -Proper-
                "SOFIA_KING_RETURN_NO_ESSENCE_RETURN",
                "SOFIA_KING_RETURN_DURING_ESSENCE_SEEK", // -Proper-
                "SOFIA_KING_RETURN_NO_ESSENSE_RETURN_WITH_ESSENCE",
                "SOFIA_KING_RETURN_ESSENCE_SEEK_SUCCESSFUL",
                "SOFIA_KING_SUCCESSFUL_ESSENCE_RETRIEVE_END", // -Proper-
                "SOFIA_KING_FINAL", // -Proper-
                "SOFIA_CROOKED_SECONDARY_DOESNT_KNOW_MYSTIC",
                "SOFIA_CROOKED_SECONDARY_KNOWS_MYSTIC",
                "SOFIA_CROOKED_SECONDARY_BODY", // -Proper-
                "SOFIA_CROOKED_SECONDARY_WANT_ESSENCE",
                "SOFIA_CROOKED_SECONDARY_FINALIZE",
                "SOFIA_CROOKED_RETURN_PARANOID", // -Proper-
                "SOFIA_CROOKED_RETURN_CLUELESS", // -Proper-
                "SOFIA_CROOKED_RETURN_NO_ESSENCE",
                "SOFIA_CROOKED_RETURN_NO_ESSENCE_PARANOID",
                "SOFIA_CROOKED_RETURN_HELPFUL_FINALIZE", // -Proper-
                "SOFIA_CROOKED_RETURN_NO_ESSENCE_MISSION_KNOWLEDGE",
                "SOFIA_CROOKED_RETURN_NO_KNOWLEDGE",
                "SOFIA_CROOKED_FINAL", // -Proper-
                "SOFIA_EPILOGUE_SCENES", //No nav
                "SOFIA_CREDITS" //No nav
            };
            return SIndex;
        }
        public static object[] RetrieveScriptByName(String S)
        {
            if(ScriptCache.ContainsKey(S.ToUpper())) { return (object[])ScriptCache[S.ToUpper()]; }
            object[] Script = new object[0];
            switch (S.ToUpper())
            {
                case "EXIT_DEFAULT":
                    Script = new object[]
                    {
                        new object[] { "C|GWS:CONTINUE", "T||Actually, you decide you've had enough of this place for now." },
                        new object[] { "C|GWS:CONTINUE", "T||Time for an impromptu departure!" },
                        ScriptFadeInOut[0],
                        ScriptFadeInOut[1],
                        ScriptFadeInOut[2],
                    };
                    break;
                case "EXIT_DEFAULT_PROPER":
                    Script = new object[]
                    {
                        new object[] { "C|GWS:CONTINUE", "T||After consulting the map, you make your exit and head towards your next destination." },
                        ScriptFadeInOut[0],
                        ScriptFadeInOut[1],
                        ScriptFadeInOut[2],
                    };
                    break;
                case "EXIT_DEFAULT_MYSTIC":
                    Script = new object[]
                    {
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "T|Sofia|Hey, you know, this has been fun and all, but I've suddenly remembered that I have somewhere else that I need to be!" },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|UNIMPRESSED", "T|Mystic Sofia|What? Child, I still had things to impart-" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|LAUGHING", "T|Sofia|Sorry, bye!" },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|Make sure you return now! We are counting on you!" },
                        ScriptFadeInOut[0],
                        ScriptFadeInOut[1],
                        ScriptFadeInOut[2],
                    };
                    break;
                case "EXIT_DEFAULT_KING":
                    Script = new object[]
                    {
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "T|Sofia|Hey, um, guys?" },
                        new object[] { "C|GWS:CONTINUE", "T|Cool Sofia|Hmm?" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|LAUGHING", "T|Sofia|I think I need to be going to do something else for a bit!" },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|UNIMPRESSED", "T|King Sofia|[F:KING]Oh ho? But, young one, I have not yet given you leave to depart my royal presence!" },
                        new object[] { "C|GWS:CONTINUE", "F|COOL SOFIA|HAPPY", "T|Cool Sofia|Yeah, like, rude yo. We were having a conversation!" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|GRINNING", "T|Sofia|Sorry, goodbyeeee!" },
                        ScriptFadeInOut[0],
                        ScriptFadeInOut[1],
                        ScriptFadeInOut[2],
                    };
                    break;
                case "EXIT_DEFAULT_CROOKED":
                    Script = new object[]
                    {
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "T|Sofia|Yo, hold up a sec." },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|Hmm? [T:400]What's up?" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Sorry to olly out on you... Buuuuut I think I gotta head off for a bit now." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|GRINNING", "T|Crooked Sofia|Hey now, no big deal, hear?" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|HAPPY", "T|Crooked Sofia|I ain't bein' the law now, after all." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Fair enough! [T:200]See you around then." },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|Be seein' you, Sis." },
                        ScriptFadeInOut[0],
                        ScriptFadeInOut[1],
                        ScriptFadeInOut[2],
                    };
                    break;
                case "EXIT_KING_PRIMARY":
                    Script = new object[]
                    {
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|I guess I'll be on my way, then." },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|HAPPY", "T|King Sofia|[F:KING]Fare ye well on your quest." },
                        new object[] { "C|GWS:CONTINUE", "F|COOL SOFIA|HAPPY", "T|Cool Sofia|See you around, yo. B)" },
                        ScriptFadeInOut[0],
                        ScriptFadeInOut[1],
                        ScriptFadeInOut[2],
                    };
                    break;
                case "EXIT_CROOKED_PRIMARY_FINALIZE":
                    Script = new object[]
                    {
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Goodbye." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|UNIMPRESSED", "T|Sofia|Be seeing you around. [T:500]Maybe." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|GRINNING", "T|Crooked Sofia|I'll be seeing ya, sis." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|HAPPY", "T|Crooked Sofia|Always good to be making interesting new acquaintances." },
                        ScriptFadeInOut[0],
                        ScriptFadeInOut[1],
                        ScriptFadeInOut[2],
                    };
                    break;
                case "TEST":
                    Script = new object[]
                    {
                        new object[] { "C|TIME:2000", "T|Test client|This is the first section of text.", new VoidDel(delegate()
                        {
                            /*Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                WorldEntity Add = new WorldEntity("UIBOX", new Vector2(100, 470), (TAtlasInfo)Shell.AtlasDirectory["UIBOX"], 0.9f);
                                Add.ColourValue = new Color(255,255,255,150);
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                                Add = new WorldEntity("TESTBG", new Vector2(0, 0), (TAtlasInfo)Shell.AtlasDirectory["TESTBG"], 0);
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                            }));*/
                        })
                        },
                        new object[] { "C|TIME:2000", "T|Test client|This is the second section of text." },
                        new object[] { "C|TIME:1000" },
                        new object[] { "C|GWS:BUTTON_TEST_PRESSED", "T|Instructor|Press the button.", new VoidDel(delegate()
                        {
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                Button B = new Button("TEST_BUTTON_SCRIPT", new Vector2(400, 200), Shell.ButtonAtlas, 0.1f, ButtonScripts.DelegateFetch("setgws_BUTTON_TEST_PRESSED") );
                                B.MLCRecord = new String[] { "setgws_BUTTON_TEST_PRESSED" };
                                Shell.UpdateQueue.Add(B);
                                Shell.RenderQueue.Add(B);
                                WorldEntity W = new WorldEntity("TEST_OSCILLATOR", new Vector2(800, 200), Shell.ButtonAtlas, 0.1f);
                                W.CenterOrigin = true;
                                W.AnimationQueue.Add(Animation.Retrieve("01OSCILLATE"));
                                Shell.UpdateQueue.Add(W);
                                Shell.RenderQueue.Add(W);
                            }));
                        })
                        },
                        new object[] { "C|TIME:1000", "T||You pressed it!", "S|YAY", "A|TEST_BUTTON_SCRIPT||2,500,10|||loop" },
                        new object[] { "C|TIME:2000", "T||[R]You pressed it!", "A|TEST_BUTTON_SCRIPT|bounce_1" },
                        new object[] { "C|GWS:BUTTON_TEST_PRESSED", "T||Now press this button.", "D|TEST_BUTTON_SCRIPT", new VoidDel(delegate()
                        {
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                Shell.GlobalWorldState = "null";
                                Button B = new Button("TEST_BUTTON_SCRIPT_2", new Vector2(600, 200), Shell.ButtonAtlas, 0.1f, ButtonScripts.DelegateFetch("setgws_BUTTON_TEST_PRESSED") );
                                B.MLCRecord = new String[] { "setgws_BUTTON_TEST_PRESSED" };
                                Shell.UpdateQueue.Add(B);
                                Shell.RenderQueue.Add(B);
                            }));
                        })
                        },
                        new object[] { "C|TIME:2000", "T||You pressed it!", "S|YAY", "A|TEST_BUTTON_SCRIPT_2|bounce_2" },
                        new object[] { "C|TIME:1000", "T||Now you will get two choices..." },
                        new object[] { "C|GWS:impos", "T||[R]Choose wisely...", new VoidDel(delegate()
                        {
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                Button B = new Button("PATH_1_BUTTON", new Vector2(400, 200), Shell.ButtonAtlas, 0.1f, ButtonScripts.DelegateFetch("runscript_PATH_1"));
                                B.MLCRecord = new String[] { "runscript_PATH_1" };
                                Button B2 = new Button("PATH_2_BUTTON", new Vector2(600, 200), Shell.ButtonAtlas, 0.1f, ButtonScripts.DelegateFetch("runscript_PATH_2"));
                                B2.MLCRecord = new String[] { "runscript_PATH_2" };
                                Shell.UpdateQueue.Add(B);
                                Shell.RenderQueue.Add(B);
                                Shell.UpdateQueue.Add(B2);
                                Shell.RenderQueue.Add(B2);
                            }));
                        })
                        }
                    };
                    break;
                case "PATH_1":
                    Script = new object[]
                    {
                        new object[] { "C|TIME:2000", "T||[R]Path 1 was chosen." },
                        new object[] { "B" }
                    };
                    break;
                case "PATH_2":
                    Script = new object[]
                    {
                        new object[] { "C|TIME:2000", "T||Path 2 was chosen." },
                        new object[] { "B" }
                    };
                    break;
                case "INTRO_PRELOAD":
                    Script = new object[]
                    {
                        new object[] { "S|BHLOGO", "C|TIME:1600:ORSKIP", new VoidDel(delegate()
                        {
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                WorldEntity Add = new WorldEntity("BMS", new Vector2(0, 0), (TAtlasInfo)Shell.AtlasDirectory["BMS"], 0f);
                                Add.GiveClickFunction(new VoidDel(delegate () { if (Shell.AllowEnter) { Shell.DoNextShifter = true; } }));
                                Add.ColourValue = new Color(0,0,0,0);
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                            }));
                        })
                        },
                        new object[] { "C|TIME:3000:ORSKIP", "A|BMS|FADEIN" },
                        new object[] { "C|TIME:1500:ORSKIP", "A|BMS|FADEOUT" },
                        new object[] { "C|TIME:0:ORSKIP", new VoidDel(delegate()
                        {
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                WorldEntity Add = new WorldEntity("PRESENTING", new Vector2(0, 0), (TAtlasInfo)Shell.AtlasDirectory["PRESENTING"], 0f);
                                Add.GiveClickFunction(new VoidDel(delegate () { if (Shell.AllowEnter) { Shell.DoNextShifter = true; } }));
                                Add.ColourValue = new Color(0,0,0,0);
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                            }));
                        })
                        },
                        new object[] { "C|TIME:3000:ORSKIP", "A|PRESENTING|FADEIN" },
                        new object[] { "C|TIME:1500:ORSKIP", "A|PRESENTING|FADEOUT" },
                        new object[] { "D|BMS", "D|PRESENTING", "S|#CLOSEALL", new VoidDel(delegate()
                        {
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                ButtonScripts.OpenMainMenu();
                            }));
                        }),
                        "B" }
                    };
                    break;
                case "INTRO_MATMUT":
                    Script = new object[]
                    {
                        new object[] { "C|TIME:1600:ORSKIP", new VoidDel(delegate()
                        {
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                WorldEntity Add = new WorldEntity("MATMUTLOGO", new Vector2(0, 0), (TAtlasInfo)Shell.AtlasDirectory["MATMUTLOGO"], 0f);
                                Add.GiveClickFunction(new VoidDel(delegate () { if (Shell.AllowEnter) { Shell.DoNextShifter = true; } }));
                                Add.ColourValue = new Color(0,0,0,0);
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                            }));
                        })
                        },
                        new object[] { "C|TIME:3000:ORSKIP", "A|MATMUTLOGO|FADEIN" },
                        new object[] { "C|TIME:1500:ORSKIP", "A|MATMUTLOGO|FADEOUT" },
                        new object[] { "D|MATMUTLOGO", "S|#CLOSEALL", new VoidDel(delegate()
                        {
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                ButtonScripts.OpenMatMutMenu();
                            }));
                        }),
                        "B" }
                    };
                    break;
                case "MATMUT_TUTORIAL":
                    Script = new object[]
                    {
                        new object[] { "C|TIME:0", "M|ORDINARY|TRUE", new VoidDel(delegate() { Shell.RunQueue.Add(new VoidDel(ButtonScripts.InitDefaultUI)); }) },
                        new object[] { "C|GWS:NEXT", "T|Tutorial Master|Welcome to your security tutorial session.", new VoidDel(delegate()
                        {
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                WorldEntity Add = new WorldEntity("TESTBG", new Vector2(0, 0), (TAtlasInfo)Shell.AtlasDirectory["TESTBG"], 0);
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                                Button Next = new Button("QUIZ_BUTTON_NEXT", new Vector2(1100, 400), (TAtlasInfo)Shell.AtlasDirectory["NEXTBUTTON"], 0.55f, ButtonScripts.DelegateFetch("setgws_NEXT") );
                                Next.MLCRecord = new String[] { "setgws_NEXT" };
                                Shell.UpdateQueue.Add(Next);
                                Shell.RenderQueue.Add(Next);
                            }));
                        })
                        },
                        new object[] { "C|GWS:NEXT", "T|Tutorial Master|This program will walk you through some basic data security questions." },
                        new object[] { "C|GWS:NEXT", "T|Tutorial Master|What social media website do you prefer?", new VoidDel(delegate()
                        {
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                DropMenu SM = new DropMenu("SOCIALMEDIA_DROPMENU", new Vector2(390, 30), 0.9f, 500, "Please choose an option.", new String[] { "Facebook", "Twitter", "Instagram", "Snapchat" }, false, new VoidDel(delegate () { }));
                                SM.CenterOrigin = false;
                                SM.AssignMenuClickFuncs(new VoidDel(delegate()
                                {
                                    if(!MatmutEnts.MatmutMonitor.DataRecord.ContainsKey("PREF_SOCIALMEDIA")) { MatmutEnts.MatmutMonitor.DataRecord.Add("PREF_SOCIALMEDIA", SM.OutputText); }
                                    else { MatmutEnts.MatmutMonitor.DataRecord["PREF_SOCIALMEDIA"] = SM.OutputText; }
                                }));
                                Shell.UpdateQueue.Add(SM);
                                Shell.RenderQueue.Add(SM);
                            }));
                        })
                        },
                        new object[] { "C|GWS:NEXT", "D|SOCIALMEDIA_DROPMENU", "T|Tutorial Master|Thank you for answering!" },
                        new object[] { "C|GWS:NEXT", "T|Tutorial Master|How often do you use the internet?", new VoidDel(delegate()
                        {
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                DropMenu SM = new DropMenu("INTERNET_FREQ_DROPMENU", new Vector2(390, 30), 0.9f, 500, "Please choose an option.", new String[] { "Daily", "Several times weekly", "Once a week", "Less often" }, false, new VoidDel(delegate () { }));
                                SM.CenterOrigin = false;
                                SM.AssignMenuClickFuncs(new VoidDel(delegate()
                                {
                                    if(!MatmutEnts.MatmutMonitor.DataRecord.ContainsKey("FREQ_INTERNET")) { MatmutEnts.MatmutMonitor.DataRecord.Add("FREQ_INTERNET", SM.OutputText); }
                                    else { MatmutEnts.MatmutMonitor.DataRecord["FREQ_INTERNET"] = SM.OutputText; }
                                }));
                                Shell.UpdateQueue.Add(SM);
                                Shell.RenderQueue.Add(SM);
                            }));
                        })
                        },
                        new object[] { "C|GWS:NEXT", "D|INTERNET_FREQ_DROPMENU", "T|Tutorial Master|Thank you for answering!" },
                        new object[] { "C|GWS:NEXT", "M|BATTLE|TRUE", "D|QUIZ_BUTTON_NEXT", "T|Tutorial Master|Quiz question![N][N]What is the typical rate of instances of data security theft?", new VoidDel(delegate()
                        {
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                Button B = ButtonScripts.GetQuickButton("65 instances per second", new VoidDel(delegate() {
                                    if(!MatmutEnts.MatmutMonitor.DataRecord.ContainsKey("DATA_THEFT_RATE")) { MatmutEnts.MatmutMonitor.DataRecord.Add("DATA_THEFT_RATE", "SECOND"); }
                                    else { MatmutEnts.MatmutMonitor.DataRecord["DATA_THEFT_RATE"] = "SECOND"; }
                                    Shell.GlobalWorldState = "NEXT";
                                }));
                                B.QuickMoveTo(new Vector2(640, 100));
                                Button B2 = ButtonScripts.GetQuickButton("65 instances per hour", new VoidDel(delegate() {
                                    if(!MatmutEnts.MatmutMonitor.DataRecord.ContainsKey("DATA_THEFT_RATE")) { MatmutEnts.MatmutMonitor.DataRecord.Add("DATA_THEFT_RATE", "HOUR"); }
                                    else { MatmutEnts.MatmutMonitor.DataRecord["DATA_THEFT_RATE"] = "HOUR"; }
                                    Shell.GlobalWorldState = "NEXT";
                                }));
                                B2.QuickMoveTo(new Vector2(640, 180));
                                Button B3 = ButtonScripts.GetQuickButton("65 instances per week", new VoidDel(delegate() {
                                    if(!MatmutEnts.MatmutMonitor.DataRecord.ContainsKey("DATA_THEFT_RATE")) { MatmutEnts.MatmutMonitor.DataRecord.Add("DATA_THEFT_RATE", "WEEK"); }
                                    else { MatmutEnts.MatmutMonitor.DataRecord["DATA_THEFT_RATE"] = "WEEK"; }
                                    Shell.GlobalWorldState = "NEXT";
                                }));
                                B3.QuickMoveTo(new Vector2(640, 260));
                                Shell.UpdateQueue.Add(B);
                                Shell.RenderQueue.Add(B);
                                Shell.UpdateQueue.Add(B2);
                                Shell.RenderQueue.Add(B2);
                                Shell.UpdateQueue.Add(B3);
                                Shell.RenderQueue.Add(B3);
                            }));
                        })
                        },
                        new object[] { "C|GWS:NEXT", "D|#CBUTTONS", "T|Tutorial Master|Thank you for answering!", new VoidDel(delegate()
                        {
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                Button Next = new Button("QUIZ_BUTTON_NEXT", new Vector2(1100, 400), (TAtlasInfo)Shell.AtlasDirectory["NEXTBUTTON"], 0.55f, ButtonScripts.DelegateFetch("setgws_NEXT") );
                                Next.MLCRecord = new String[] { "setgws_NEXT" };
                                Shell.UpdateQueue.Add(Next);
                                Shell.RenderQueue.Add(Next);
                            }));
                        })
                        },
                        new object[] { "C|GWS:NEXT", "D|QUIZ_BUTTON_NEXT", "T|Tutorial Master|Quiz question![N][N]What percentage of people have had data leaked?", new VoidDel(delegate()
                        {
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                Button B = ButtonScripts.GetQuickButton("20%", new VoidDel(delegate() {
                                    if(!MatmutEnts.MatmutMonitor.DataRecord.ContainsKey("PERCENT_LEAKED")) { MatmutEnts.MatmutMonitor.DataRecord.Add("PERCENT_LEAKED", "20"); }
                                    else { MatmutEnts.MatmutMonitor.DataRecord["PERCENT_LEAKED"] = "20"; }
                                    Shell.GlobalWorldState = "NEXT";
                                }));
                                B.QuickMoveTo(new Vector2(640, 100));
                                Button B2 = ButtonScripts.GetQuickButton("35%", new VoidDel(delegate() {
                                    if(!MatmutEnts.MatmutMonitor.DataRecord.ContainsKey("PERCENT_LEAKED")) { MatmutEnts.MatmutMonitor.DataRecord.Add("PERCENT_LEAKED", "35"); }
                                    else { MatmutEnts.MatmutMonitor.DataRecord["PERCENT_LEAKED"] = "35"; }
                                    Shell.GlobalWorldState = "NEXT";
                                }));
                                B2.QuickMoveTo(new Vector2(640, 180));
                                Button B3 = ButtonScripts.GetQuickButton("42%", new VoidDel(delegate() {
                                    if(!MatmutEnts.MatmutMonitor.DataRecord.ContainsKey("PERCENT_LEAKED")) { MatmutEnts.MatmutMonitor.DataRecord.Add("PERCENT_LEAKED", "42"); }
                                    else { MatmutEnts.MatmutMonitor.DataRecord["PERCENT_LEAKED"] = "42"; }
                                    Shell.GlobalWorldState = "NEXT";
                                }));
                                B3.QuickMoveTo(new Vector2(640, 260));
                                Button B4 = ButtonScripts.GetQuickButton("58%", new VoidDel(delegate() {
                                    if(!MatmutEnts.MatmutMonitor.DataRecord.ContainsKey("PERCENT_LEAKED")) { MatmutEnts.MatmutMonitor.DataRecord.Add("PERCENT_LEAKED", "58"); }
                                    else { MatmutEnts.MatmutMonitor.DataRecord["PERCENT_LEAKED"] = "58"; }
                                    Shell.GlobalWorldState = "NEXT";
                                }));
                                B4.QuickMoveTo(new Vector2(640, 340));
                                Shell.UpdateQueue.Add(B);
                                Shell.RenderQueue.Add(B);
                                Shell.UpdateQueue.Add(B2);
                                Shell.RenderQueue.Add(B2);
                                Shell.UpdateQueue.Add(B3);
                                Shell.RenderQueue.Add(B3);
                                Shell.UpdateQueue.Add(B4);
                                Shell.RenderQueue.Add(B4);
                            }));
                        })
                        },
                        new object[] { "C|GWS:NEXT", "D|#CBUTTONS", "T|Tutorial Master|Thank you for answering!", new VoidDel(delegate()
                        {
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                Button Next = new Button("QUIZ_BUTTON_NEXT", new Vector2(1100, 400), (TAtlasInfo)Shell.AtlasDirectory["NEXTBUTTON"], 0.55f, ButtonScripts.DelegateFetch("setgws_NEXT") );
                                Next.MLCRecord = new String[] { "setgws_NEXT" };
                                Shell.UpdateQueue.Add(Next);
                                Shell.RenderQueue.Add(Next);
                            }));
                        })
                        },
                        new object[] { "C|GWS:NEXT", "D|QUIZ_BUTTON_NEXT", "T|Tutorial Master|Quiz question![N][N]Which of these criminals would steal your personal data?", new VoidDel(delegate()
                        {
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                Button B = new Button("BUTTON_CRIME_PIRATE", new Vector2(300, 200), (TAtlasInfo)Shell.AtlasDirectory["PIRATE_IMAGEBUTTON"], 0.6f, new VoidDel(delegate() {
                                    if(!MatmutEnts.MatmutMonitor.DataRecord.ContainsKey("CRIMINAL")) { MatmutEnts.MatmutMonitor.DataRecord.Add("CRIMINAL", "PIRATE"); }
                                    else { MatmutEnts.MatmutMonitor.DataRecord["CRIMINAL"] = "PIRATE"; }
                                    Shell.GlobalWorldState = "NEXT";
                                }));
                                Button B2 = new Button("BUTTON_CRIME_THIEF", new Vector2(640, 200), (TAtlasInfo)Shell.AtlasDirectory["THIEF_IMAGEBUTTON"], 0.6f, new VoidDel(delegate() {
                                    if(!MatmutEnts.MatmutMonitor.DataRecord.ContainsKey("CRIMINAL")) { MatmutEnts.MatmutMonitor.DataRecord.Add("CRIMINAL", "THIEF"); }
                                    else { MatmutEnts.MatmutMonitor.DataRecord["CRIMINAL"] = "THIEF"; }
                                    Shell.GlobalWorldState = "NEXT";
                                }));
                                Button B3 = new Button("BUTTON_CRIME_HACKER", new Vector2(980, 200), (TAtlasInfo)Shell.AtlasDirectory["HACKER_IMAGEBUTTON"], 0.6f, new VoidDel(delegate() {
                                    if(!MatmutEnts.MatmutMonitor.DataRecord.ContainsKey("CRIMINAL")) { MatmutEnts.MatmutMonitor.DataRecord.Add("CRIMINAL", "HACKER"); }
                                    else { MatmutEnts.MatmutMonitor.DataRecord["CRIMINAL"] = "HACKER"; }
                                    Shell.GlobalWorldState = "NEXT";
                                }));
                                Shell.UpdateQueue.Add(B);
                                Shell.RenderQueue.Add(B);
                                Shell.UpdateQueue.Add(B2);
                                Shell.RenderQueue.Add(B2);
                                Shell.UpdateQueue.Add(B3);
                                Shell.RenderQueue.Add(B3);
                            }));
                        })
                        },
                        new object[] { "C|GWS:NEXT", "D|BUTTON_CRIME_PIRATE", "D|BUTTON_CRIME_THIEF", "D|BUTTON_CRIME_HACKER", "T|Tutorial Master|Thank you for answering!", new VoidDel(delegate()
                        {
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                Button Next = new Button("QUIZ_BUTTON_NEXT", new Vector2(1100, 400), (TAtlasInfo)Shell.AtlasDirectory["NEXTBUTTON"], 0.55f, ButtonScripts.DelegateFetch("setgws_NEXT") );
                                Next.MLCRecord = new String[] { "setgws_NEXT" };
                                Shell.UpdateQueue.Add(Next);
                                Shell.RenderQueue.Add(Next);
                            }));
                        })
                        },
                        new object[] { "C|GWS:NEXT", "D|UIBOX", "D|QUIZ_BUTTON_NEXT", "T||" , new VoidDel(delegate()
                        {
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                Button Next = new Button("QUIZ_BUTTON_NEXT", new Vector2(200, 530), (TAtlasInfo)Shell.AtlasDirectory["BACKBUTTON"], 0.55f, ButtonScripts.DelegateFetch("setgws_NEXT") );
                                Next.MLCRecord = new String[] { "setgws_NEXT" };
                                Shell.UpdateQueue.Add(Next);
                                Shell.RenderQueue.Add(Next);
                                TextEntity T = new TextEntity("TextResults", MatmutEnts.MatmutMonitor.GetResultString(), new Vector2(50, 20), 0.7f);
                                T.BufferLength = 1200;
                                Shell.UpdateQueue.Add(T);
                                Shell.RenderQueue.Add(T);
                            }));
                        })
                        }
                    };
                    break;
                case "VS_MAIN_INTRO":
                    Script = new object[]
                    {
                        new object[] { "C|GWS:CONTINUE", "T|Vinny|Urgh...", new VoidDel(delegate() { Shell.RunQueue.Add(new VoidDel(ButtonScripts.InitDefaultUI)); })},
                        new object[] { "C|GWS:CONTINUE", "T|Vinny|What... what is this?" },
                        new object[] { "C|GWS:CONTINUE", "T|Vinny|Where am I?" },
                        new object[] { "C|GWS:CONTINUE", "T|Vinny|Could this be..." },
                        new object[] { "C|GWS:CONTINUE", "T|Vinny|No." },
                        new object[] { "C|GWS:CONTINUE", "T|Vinny|FUCK, NO!" },
                        new object[] { "C|GWS:CONTINUE", "T|Vinny|Anything but this!" },
                        new object[] { "C|GWS:CONTINUE", "T|Vinny|Could it be that... chat has gotten me to play..." },
                        new object[] { "C|GWS:CONTINUE", "T|Vinny|A VISUAL NOVEL?!" },
                        new object[] { "C|TIME:0", "T|???|HELLO THERE", new VoidDel(delegate()
                        {
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                WorldEntity Add = new WorldEntity("TESTBG", new Vector2(0, 0), (TAtlasInfo)Shell.AtlasDirectory["TESTBG"], 0.05f);
                                Add.ColourValue = new Color(0,0,0,0);
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                            }));
                        })
                        },
                        new object[] { "C|GWS:CONTINUE", "A|TESTBG|FADEIN" },
                        new object[] { "C|GWS:CONTINUE", "T|???|IT SEEMS THAT YOU ARE THE NEW STUDENT..." },
                        new object[] { "C|GWS:CONTINUE", "T|???|AT THE WORLD REKNOWNED [R]VINE HIGH!" },
                        new object[] { "C|GWS:CONTINUE", "T|Vinny|Yeah... no. No thank you." },
                        new object[] { "C|GWS:CONTINUE", "T|???|CHIN UP NOW! WE ARE WORLD REKNOWNED AFTER ALL." },
                        new object[] { "C|GWS:CONTINUE", "T|???|WELL." },
                        new object[] { "C|GWS:CONTINUE", "T|???|WHEN I SAY THAT WE ARE WORLD REKNOWNED..." },
                        new object[] { "C|GWS:CONTINUE", "T|???|I FEEL. IT IS LESS FOR OUR PRESTIGIOUS EDUCATIONAL OFFERINGS." },
                        new object[] { "C|GWS:CONTINUE", "T|???|BUT MORE FOR THE SERVICE WE PROVIDE!" },
                        new object[] { "C|GWS:CONTINUE", "T|???|BY TAKING IN ALL THE STUDENTS WHO FLUNKED OUT OF BETTER, LESS TRASH-FILLED ANIME SCHOOLS." },
                        new object[] { "C|GWS:CONTINUE", "T|???|BUT WORRY NOT, MR. \"VINE SAUCE\"." },
                        new object[] { "C|GWS:CONTINUE", "T|???|(that is your name right)" },
                        new object[] { "C|GWS:CONTINUE", "T|Vinny|Uhh..." },
                        new object[] { "C|GWS:CONTINUE", "T|Vinny|I guess?" },
                        new object[] { "C|GWS:CONTINUE", "T|???|(i mean it's what it says on my clipboard here)" },
                        new object[] { "C|GWS:CONTINUE", "T|???|(but really, what sort of name is Vine Sauce)" },
                        new object[] { "C|GWS:CONTINUE", "T|???|((it sounds dumb))" },
                        new object[] { "C|GWS:CONTINUE", "T|???|(wait, i hope he didn't hear me say that)" },
                        new object[] { "C|GWS:CONTINUE", "T|???|(fuck he's looking at me funny)" },
                        new object[] { "C|GWS:CONTINUE", "T|???|ANYWAY!" },
                        new object[] { "C|GWS:CONTINUE", "T|???|LET US NOT PISS AROUND OUT HERE ANY LONGER." },
                        new object[] { "C|GWS:CONTINUE", "T|???|YOU NEED TO JOIN YOUR FIRST CLASS!" },
                        new object[] { "C|GWS:CONTINUE", "T|???|AND GUESS WHAT. YOUR TEACHERS ARE NEW TOO! YOU'LL FEEL RIGHT AT HOME." },
                        new object[] { "C|GWS:CONTINUE", "T|???|AND YES, I DID SAY TEACHER*S*." },
                        new object[] { "C|GWS:CONTINUE", "T|???|WHAT CAN I SAY. THEY ONLY AGREED TO COME ON BOARD IF WE HIRED THEM BOTH." },
                        new object[] { "C|GWS:CONTINUE", "T|???|BUT I'M SURE THEY MAKE UP FOR IN TEACHING ABILITY WHAT THEY COST US IN PAY." },
                        new object[] { "C|GWS:CONTINUE", "T|???|(and we really needed to fill that vacancy... :/)" },
                        new object[] { "B|TEST" }
                    };
                    break;
                case "SOFIA_KING_PRIMARY":
                    Script = new object[]
                    {
                        new object[] { "C|GWS:CONTINUE", "D|#CBUTTONS", "F|SOFIA|DOWNCAST", "T|Sofia|Okay, sure. I guess I'm just rolling with this at this point." },
                        new object[] { "C|GWS:CONTINUE",  "F|COOL SOFIA|HAPPY", "T|Cool Sofia|Wonderful! Follow me, it's just this way." },
                        new object[] { "C|TIME:1000:ORSKIP", "T|Cool Sofia|It should only take about an hour." },
                        new object[] { "C|TIME:600:ORSKIP", "F|SOFIA|UNIMPRESSED", "A|COOL SOFIA|750=0,600,20||||" },
                        new object[] { "C|TIME:1200:ORSKIP", "A|SOFIA|1200=0,1000,20||||", "D|COOL SOFIA" },
                        new object[] { "C|GWS:CONTINUE", "D|SOFIA", "T||You find yourself travelling onwards into the murky highlands of ULTRASOFIAWORLD.", "A|SOFIAWORLDBACKDROP|FADEOUTLONG" },
                        new object[] { "C|GWS:CONTINUE", "T||After some time, you begin to make out structures in the distance..." },
                        new object[] { "C|TIME:3000:ORSKIP", "D|SOFIAWORLDBACKDROP", "T||A ragged castle slowly reveals itself.", new VoidDel(delegate()
                        {
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                WorldEntity Add = new WorldEntity("CASTLEEXTBG", new Vector2(0, 0), (TAtlasInfo)Shell.AtlasDirectory["CASTLEEXTBG"], 0.05f);
                                Add.ColourValue = new Color(0,0,0,0);
                                Add.AnimationQueue.Add(Animation.Retrieve("FADEINLONG"));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                                Add = new WorldEntity("SOFIA", new Vector2(-700, 405), (TAtlasInfo)Shell.AtlasDirectory["SOFIASPRITES"], 0.5f);
                                Add.CenterOrigin = true;
                                Add.SetAtlasFrame(new Point(1, 0));
                                Add.Scale(new Vector2(-0.06f, -0.06f));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                                Add = new WorldEntity("COOL SOFIA", new Vector2(-450, 405), (TAtlasInfo)Shell.AtlasDirectory["COOLSOFIA"], 0.46f);
                                Add.CenterOrigin = true;
                                Add.ManualHorizontalFlip = true;
                                Add.SetAtlasFrame(new Point(1, 0));
                                Add.Scale(new Vector2(-0.12f, -0.12f));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                            }));
                        })
                        },
                        new object[] { "C|TIME:400:ORSKIP", "F|SOFIA|DOWNCAST", "F|COOL SOFIA|HAPPY", "T|Sofia|Okay, is this the place?", "A|COOL SOFIA|1000=0,900,20||||" },
                        new object[] { "C|GWS:CONTINUE", "A|SOFIA|1000=0,900,20||||" },
                        new object[] { "C|GWS:CONTINUE", "T|Cool Sofia|More or less." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "T|Sofia|Huh. Pretty big castl-", new VoidDel(delegate()
                        {
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                WorldEntity Add = new WorldEntity("GOLEM SOFIA", new Vector2(1430, 405), (TAtlasInfo)Shell.AtlasDirectory["GOLEMSOFIA"], 0.5f);
                                Add.CenterOrigin = true;
                                Add.SetAtlasFrame(new Point(1, 0));
                                Add.Scale(new Vector2(-0.06f, -0.06f));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                            }));
                        })
                        },
                        new object[] { "C|GWS:CONTINUE", "A|GOLEM SOFIA|-500=0,500,20||||", "F|SOFIA|UNIMPRESSED", "F|COOL SOFIA|LAUGHING", "T|Golem Sofia|HALT!!!" },
                        new object[] { "C|GWS:CONTINUE", "F|COOL SOFIA|GRINNING", "T|Golem Sofia|WHO APPROACHES THE DOMAIN OF THE KING SOFIA?" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|You again!" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|We already told you that she's Cool Sofia, and I'm with her!" },
                        new object[] { "C|TIME:500:ORSKIP", "T|Golem Sofia|I DON'T-" },
                        new object[] { "C|GWS:CONTINUE", "T|Cool Sofia|Shh, cool it. She's a *different* Golem." },
                        new object[] { "C|GWS:CONTINUE", "F|COOL SOFIA|HAPPY", "F|SOFIA|WORRIED", "T|Sofia|Huh. [T:500]There's more than one?" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|DOWNCAST", "T|Cool Sofia|Hell yeah there's more than one. They wouldn't be very useful if we only had one of them, would they?" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|I, uh..." },
                        new object[] { "C|GWS:CONTINUE", "F|COOL SOFIA|GRINNING", "T|Cool Sofia|So, yeah, it's me, Cool Sofia, here to see the King Sofia. Remember our pure essence problem? Well, look what I have for you! Special delivery. B)" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|UNIMPRESSED", "T||Cool Sofia sort of gestures at you dramatically." },
                        new object[] { "C|GWS:CONTINUE", "F|COOL SOFIA|HAPPY", "F|GOLEM SOFIA|GRINNING", "T|Golem Sofia|SOUNDS VALID. YOU MAY PASS." },
                        new object[] { "C|GWS:CONTINUE", "T|Golem Sofia|MAY THE BLESSINGS OF THE KING SOFIA BE UPON YOU." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "T|Cool Sofia|Cool, let's go." },
                        new object[] { "C|TIME:1500:ORSKIP", "A|GOLEM SOFIA|FADEOUT", "A|CASTLEEXTBG|FADEOUT", "T||" },
                        new object[] { "C|GWS:CONTINUE", "M|KING|TRUE", "D|CASTLEEXTBG", "D|GOLEM SOFIA", "T||You enter the castle. The interior is opulent, but subdued. A strange, out of place feeling comes over you.", new VoidDel(delegate()
                        {
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                WorldEntity Add = new WorldEntity("CASTLEINTBG", new Vector2(0, 0), (TAtlasInfo)Shell.AtlasDirectory["CASTLEINTBG"], 0.05f);
                                Add.ColourValue = new Color(0,0,0,0);
                                Add.AnimationQueue.Add(Animation.Retrieve("FADEIN"));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                            }));
                        })
                        },
                        new object[] { "C|GWS:CONTINUE", "T||You hear sounds of running water. Is it raining outside?" },
                        new object[] { "C|GWS:CONTINUE", "T||You're so busy looking around at the room and its arching windows that you almost don't spot the person sitting in the chair by the wall until she gets up and walks towards you.", new VoidDel(delegate()
                        {
                            Sofia.KingFlag = 1;
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                WorldEntity Add = new WorldEntity("KING SOFIA", new Vector2(1530, 405), (TAtlasInfo)Shell.AtlasDirectory["KINGSOFIA"], 0.5f);
                                Add.CenterOrigin = true;
                                Add.SetAtlasFrame(new Point(1, 1));
                                Add.Scale(new Vector2(-0.06f, -0.06f));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                            }));
                        })
                        },
                        new object[] { "C|GWS:CONTINUE", "A|KING SOFIA|-600=0,1000,20||||", "F|SOFIA|WORRIED", "F|COOL SOFIA|EXCITED", "T|King Sofia|[F:KING]Who goes there! Who enters the domain of the King Sofia???" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|UNIMPRESSED", "F|COOL SOFIA|DOWNCAST", "T|Sofia|Do the, uh, Infinity Stones in her eyes grant her the King power, or are they just there for decoration?" },
                        new object[] { "C|TIME:1000:ORSKIP", "F|COOL SOFIA|WORRIED", "F|KING SOFIA|UNIMPRESSED", "T|Cool Sofia|Shh, no, don't say that-" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|DOWNCAST", "F|KING SOFIA|JUDGING", "T|King Sofia|[F:KING]SILENCE! The King Sofia is not here to be mocked!" },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|CONSIDERING", "T|King Sofia|[F:KING]State the meaning for your presence in my court, else my Golems will escort you out of my castle and into the gutter!" },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|THINKING", "F|COOL SOFIA|HAPPY", "T|Cool Sofia|My most humble apologies, Your Excellency." },
                        new object[] { "C|GWS:CONTINUE", "F|COOL SOFIA|LAUGHING", "T|Cool Sofia|My uncouth companion here unfortunately doesn't yet know the ways of our world." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "F|COOL SOFIA|GRINNING", "T|Cool Sofia|But she'll come around." },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|CONSIDERING", "T|King Sofia|[F:KING]Why so? She is a Sofia, is she not? What is her designation?" },
                        new object[] { "C|GWS:CONTINUE", "F|COOL SOFIA|HAPPY", "T|King Sofia|[F:KING]I don't recognize her eyes. Most bizarre." },
                        new object[] { "C|GWS:CONTINUE", "T|Cool Sofia|That is the reason for my presence, Sire." },
                        new object[] { "C|GWS:CONTINUE", "F|COOL SOFIA|EXCITED", "T|Cool Sofia|Remember my petition regarding a potential new source of [R]SOFIA ESSENCE[C:WHITE]?" },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|DOWNCAST", "T|King Sofia|[F:KING]Yes, I told you to forget it. Extracting a SOFIA from another world? A ridiculous notion." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|UNIMPRESSED", "F|COOL SOFIA|LAUGHING", "F|KING SOFIA|THINKING", "T|King Sofia|[F:KING]But wait, you aren't saying, that this is..." },
                        new object[] { "C|GWS:CONTINUE", "T|Cool Sofia|You are not mistaken, my Lord! A pure Sofia, a *generic* Sofia! Unique in her world! Brought here to bring light once more to ULTRASOFIAWORLD!" },
                        new object[] { "C|GWS:CONTINUE", "T|King Sofia|..." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "F|COOL SOFIA|WORRIED", "F|KING SOFIA|JUDGING", "T|King Sofia|[F:KING]RIDICULOUS! PREPOSTEROUS! BLASPHEMOUS! INSANE!" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|UNIMPRESSED", "F|COOL SOFIA|GRINNING", "F|KING SOFIA|EXCITED", "T|King Sofia|[F:KING]...I LOVE IT!!!", new VoidDel(delegate() { Sofia.KingFlag = 2; }) },
                        new object[] { "C|GWS:CONTINUE", "F|COOL SOFIA|EXCITED", "T|Cool Sofia|Wonderful!" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "F|KING SOFIA|HAPPY", "T|Sofia|Uh. Yay?" },
                        new object[] { "C|GWS:CONTINUE", "F|COOL SOFIA|GRINNING", "T|King Sofia|[F:KING]Yay indeed, my young friend!" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|UNIMPRESSED", "F|KING SOFIA|LAUGHING", "T|King Sofia|[F:KING]Hah! What am I saying, we're all the same age by definition!" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "F|KING SOFIA|HAPPY", "T|King Sofia|[F:KING]But what does it matter! We must proceed at once with THE PROCEDURE." },
                        new object[] { "C|GWS:CONTINUE", "T|Cool Sofia|Much agreed, Sire. There is no time to waste." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Whoa there! Hold up, what procedure!" },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|LAUGHING", "T|Sofia|That sounds highly ominous and I am very afraid all of a sudden." },
                        new object[] { "C|GWS:CONTINUE", "T|Cool Sofia|Don't worry, we're not going to chop your head off or anything. B)" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Somehow that doesn't reassure me all that much!" },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|HAPPY", "T|King Sofia|[F:KING]No need to worry, no need to worry!" },
                        new object[] { "C|GWS:CONTINUE", "T|King Sofia|[F:KING]The PROCEDURE is more of a ritual than anything else. A process to inject some of your Sofia [R,F:KING]ESSENCE[C:255-255-255-255,F:KING] into the [R,F:KING]SOURCE[C:255-255-255-255,F:KING]." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "T|Sofia|The source?" },
                        new object[] { "C|GWS:CONTINUE", "F|COOL SOFIA|HAPPY", "T|Cool Sofia|The [R]SOURCE[C:WHITE] is an opening into the underlying energy continuum upon which ULTRASOFIAWORLD is built." },
                        new object[] { "C|GWS:CONTINUE", "T|Cool Sofia|It's an open vein of power, the power being the [R]ESSENCE[C:WHITE]. Placing some of your essence into it should kickstart a reaction that will reverse the decay that our world has been experiencing." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|CONSIDERING", "T|Sofia|Giving up some of my essence..." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Will that hurt me at all?" },
                        new object[] { "C|GWS:CONTINUE", "T|King Sofia|[F:KING]It shouldn't do, no." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "T|Sofia|Well, maybe I trust you. But only because you're me." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|GRINNING", "T|Sofia|Also, because I like your floaty crowny thing." },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|LAUGHING", "T|King Sofia|[F:KING]Thanks!" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|LAUGHING", "F|COOL SOFIA|HAPPY", "T|Cool Sofia|Because you're a pure Sofia, your essence should just regenerate on its own." },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|EXCITED", "T|King Sofia|[F:KING]Yes, but the Mystic Sofia will be able to tell you more." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "T|Sofia|Mystic Sofia, huh?" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|HAPPY", "T|King Sofia|[F:KING]She's the one who you will be going to see. The devotee who guards the [R,F:KING]SOURCE[C:255-255-255-255,F:KING]." },
                        new object[] { "C|GWS:CONTINUE", "T|King Sofia|[F:KING]She will oversee the essence injection process." },
                        new object[] { "C|GWS:CONTINUE", "F|COOL SOFIA|THINKING", "T|Cool Sofia|We'll need some documentation to make sure she'll see us, though." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|UNIMPRESSED", "T|Sofia|You know, this bizarre Sofia dimension has a strange amount of bureaucracy." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|HAPPY", "T|King Sofia|[F:KING]Quite. But worry not, I can provide the relevant documentation." },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|LAUGHING", "T||The King Sofia hands you a [C:138-0-255-255]ROYAL EDICT[C:WHITE].", new VoidDel(delegate() { Sofia.KingFlag = 3; }) },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|EXCITED", "T|King Sofia|[F:KING]This should get you into the Mystic's sanctum." },
                        new object[] { "C|GWS:CONTINUE", "T|Cool Sofia|You should probably get going right away." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "T|Sofia|Wait, are you not coming?" },
                        new object[] { "C|GWS:CONTINUE", "F|COOL SOFIA|LAUGHING", "T|Cool Sofia|Maybe I'll come along for the finale, but I have a few things I need to take care of first." },
                        new object[] { "C|GWS:CONTINUE", "T|Cool Sofia|You can't be monopolizing all of my time, yo. B)" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|DOWNCAST", "F|COOL SOFIA|HAPPY", "T|Sofia|Uh, okay. [T:500]Um, how do I get there then?" },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|THINKING", "T|King Sofia|[F:KING]Oh yes, of course!" },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|GRINNING", "T|King Sofia|[F:KING]Take this map. It should allow you to navigate to the sanctum of the Mystic Sofia." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|HAPPY", "T||The King Sofia hands you a [C:138-0-255-255]MAP[C:WHITE]." },
                        ReceiveNav,
                        new object[] { "C|GWS:CONTINUE", "F|COOL SOFIA|LAUGHING", "T|Cool Sofia|Well, no time to waste." },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|LAUGHING", "T|King Sofia|[F:KING]Yes! Fare you well, young SOFIA!" },
                        new object[] { "C|GWS:impos_mapnavigate", "T||Use the [C:138-0-255-255]MAP[C:WHITE] to navigate to your next destination." },
                    };
                    break;
                case "SOFIA_CROOKED_PRIMARY":
                    Script = new object[]
                    {
                        new object[] { "C|GWS:CONTINUE", "D|#CBUTTONS", "F|SOFIA|WORRIED", "T|Sofia|Yeah, uh, no." },
                        new object[] { "C|GWS:CONTINUE", "F|COOL SOFIA|UNIMPRESSED", "T|Cool Sofia|E- Excuse me?" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Yeah, I mean. As much as you look like me, I don't know you." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|And..." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|UNIMPRESSED", "T|Sofia|Honestly? So far you've done nothing but weird me out pretty much this entire time." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|\"Untainted essence\"? \"Alternate dimensions?\" \"King Sofias\" and \"Golem Sofias\"? [T:300]Not for me, thanks." },
                        new object[] { "C|TIME:400:ORSKIP", "F|COOL SOFIA|WORRIED", "T|Cool Sofia|But-!" },
                        new object[] { "C|TIME:1200:ORSKIP", "F|SOFIA|LAUGHING", "A|SOFIA|-600=0,1000,20||||", "T|Sofia|See you later, nerd!" },
                        new object[] { "C|GWS:CONTINUE", "T|Cool Sofia|Wait!" },
                        new object[] { "C|GWS:CONTINUE", "F|COOL SOFIA|DOWNCAST", "T|Cool Sofia|Oh. Well shit. So much for that." },
                        new object[] { "C|TIME:2200:ORSKIP", "F|COOL SOFIA|UNIMPRESSED", "A|COOL SOFIA|1000=0,3000,20||||", "T|Cool Sofia|Guess I better go report this to the [C:138-0-255-255]King[C:WHITE]." },
                        new object[] { "C|GWS:CONTINUE", "D|SOFIA", "T||Abandoning Cool Sofia, you wander to and fro amongst the shimmering, iridescent hills of ULTRASOFIAWORLD.", "A|SOFIAWORLDBACKDROP|FADEOUTLONG", "A|COOL SOFIA|FADEOUTLONG" },
                        new object[] { "C|GWS:CONTINUE", "T||You're... not sure how you're going to get back home now that you've given the person who brought you here the cold shoulder, but you're just kind of winging it at this point." },
                        new object[] { "C|GWS:CONTINUE", "D|COOL SOFIA", "D|SOFIAWORLDBACKDROP", "T||And hey! It's not like making rash, spur-of-the-moment decisions has ever gotten you into trouble before! I mean, it's not like you got here by deciding to reach out and touch a weirdly glowing portal in a room beneath your hous- Oh wait." },
                        new object[] { "C|GWS:CONTINUE", "T||Huh. Crap, you guess." },
                        new object[] { "C|TIME:2000:ORSKIP", "M|#NULL", "T||Regardless, you trudge on through the lowlands, until eventually you see a CAVE.", new VoidDel(delegate()
                        {
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                WorldEntity Add = new WorldEntity("CROOKEDCAVEBG", new Vector2(0, -160), (TAtlasInfo)Shell.AtlasDirectory["CROOKEDCAVEBG"], 0.05f);
                                Add.ColourValue = new Color(0,0,0,0);
                                Add.AnimationQueue.Add(Animation.Retrieve("FADEINLONG"));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                                Add = new WorldEntity("SOFIA", new Vector2(350, 405), (TAtlasInfo)Shell.AtlasDirectory["SOFIASPRITES"], 0.5f);
                                Add.ColourValue = new Color(0,0,0,0);
                                Add.CenterOrigin = true;
                                Add.SetAtlasFrame(new Point(0, 2));
                                Add.Scale(new Vector2(-0.06f, -0.06f));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                                Add = new WorldEntity("CRIME SHACK", new Vector2(850, 300), (TAtlasInfo)Shell.AtlasDirectory["CRIMESHACK"], 0.46f);
                                Add.ColourValue = new Color(0,0,0,0);
                                Add.CenterOrigin = true;
                                Add.Scale(new Vector2(-0.2f, -0.2f));
                                Shell.RenderQueue.Add(Add);
                                Shell.UpdateQueue.Add(Add);
                                Add = new WorldEntity("CROOKED SOFIA", new Vector2(1430, 405), (TAtlasInfo)Shell.AtlasDirectory["CROOKEDSOFIA"], 0.48f);
                                Add.CenterOrigin = true;
                                Add.SetAtlasFrame(new Point(0, 0));
                                Add.Scale(new Vector2(-0.06f, -0.06f));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                            }));
                        })
                        },
                        new object[] { "C|GWS:CONTINUE", "A|SOFIA|FADEIN" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Well, it's a cave." },
                        new object[] { "C|GWS:CONTINUE", "A|CRIME SHACK|FADEINLONG", "T|Sofia|At least there couldn't possibly be anything weird or shady in heeeroooooh hell." },
                        new object[] { "M|CRIMINAL|TRUE", "C|GWS:CONTINUE", "T|???|Hey.", new VoidDel(delegate() { Sofia.CrookedFlag = 1; }) },
                        new object[] { "C|GWS:CONTINUE", "T|???|Psst. [T:300]Hey kid." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|UNIMPRESSED", "A|CROOKED SOFIA|-600=0,1500,20||||", "T|Crooked Sofia|Wanna buy some [R]ESSENCE[C:WHITE]?", new VoidDel(delegate() { Sofia.CrookedFlag = 2; }) },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Oh no. Who are you then, Pirate Sofia?" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|CONSIDERING", "T|Sofia|(...double Pirate Sofia?)" },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|HAPPY", "T|Crooked Sofia|Me? Oh, I'm nobody important, Sister." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "F|CROOKED SOFIA|LAUGHING", "T|Crooked Sofia|Just another honest tradeswoman tryin'tuh drum up some business in these trying times." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|HAPPY", "T|Crooked Sofia|In the mood for some [R]ESSENCE[C:WHITE]? I have good stuff. Mine's the best you'll find on this side of ULTRASOFIAWORLD." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "T|Crooked Sofia|Really hits the spot if you know what I mean, Sister. Top quality [R]ESSENCE[C:WHITE]." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|I, uh. Um." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "F|CROOKED SOFIA|LAUGHING", "T|Sofia|Are you trying to... sell me drugs?" },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|Hah hah, hells no, Sister! Do you take me for some kinda unscrupulous businesser?" },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|EXCITED", "T|Crooked Sofia|Not to knock on you if that's your groove, 'coursewise." },
                        new object[] { "C|TIME:500:ORSKIP", "T|Sofia|Uh-" },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|HAPPY", "T|Crooked Sofia|Nah, what I have is all kinds of wicked better. Can't shine a light to any DRUGS you could find topside." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|DOWNCAST", "T|Crooked Sofia|Pure distilled [R]SOFIA ESSENCE[C:WHITE], fresh as twilight, as straight from the [R]SOURCE[C:WHITE]." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "F|CROOKED SOFIA|LAUGHING", "T|Crooked Sofia|Has a real kick to it." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|HAPPY", "T|Sofia|Yeah, you know, actually, I think I was just hearing about that." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "T|Sofia|I thought it was in short supply or something?" },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|Maybe for some people, but I have connections in high *and* low places, hear." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|EXCITED", "T|Crooked Sofia|CROOKED SOFIA has hookups for *all* the best stuff, Sister." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|HAPPY", "T|Crooked Sofia|Trust me. I'm a businesswoman." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|UNIMPRESSED", "T||She puts out her hand for you to shake." },
                        new object[] { "C|GWS:impos", new VoidDel(delegate()
                        {
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                Button B = ButtonScripts.GetQuickButton("Shake it.", ButtonScripts.DelegateFetch("runscript_SOFIA_CROOKED_PRIMARY_SHAKE"));
                                B.MLCRecord = new String[] { "runscript_SOFIA_CROOKED_PRIMARY_SHAKE" };
                                B.QuickMoveTo(new Vector2(640, 200));
                                Button B2 = ButtonScripts.GetQuickButton("Do not.", ButtonScripts.DelegateFetch("runscript_SOFIA_CROOKED_PRIMARY_NOSHAKE"));
                                B2.MLCRecord = new String[] { "runscript_SOFIA_CROOKED_PRIMARY_NOSHAKE" };
                                B2.QuickMoveTo(new Vector2(640, 320));
                                Shell.UpdateQueue.Add(B);
                                Shell.RenderQueue.Add(B);
                                Shell.UpdateQueue.Add(B2);
                                Shell.RenderQueue.Add(B2);
                            }));
                        })
                        }
                    };
                    break;
                case "SOFIA_CROOKED_PRIMARY_NOSHAKE":
                    Script = new object[]
                    {
                        new object[] { "C|GWS:CONTINUE", "D|#CBUTTONS", "F|SOFIA|WORRIED", "T|Sofia|No thanks." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|DOWNCAST", "T|Crooked Sofia|Aw, sis, can't you give a chance at trusting me?" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|I don't know you? [TIME:300]And I think you probably just tried to sell me drugs? [TIME:300]And your stall here literally says \"crime shack\" on it?" },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|UNIMPRESSED", "T|Crooked Sofia|Well maybe that's just how I roll my style, yo." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Well, maybe I still just don't trust you." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|DOWNCAST", "T|Crooked Sofia|..." },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|Aw, hell. Well now I reckon I've gone'n made a bad impression for myself." },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|Well hey." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|EXCITED", "T|Crooked Sofia|I ain't gonna go pretending that I'm 100% straight and narrow, but I'm not all that bad!" },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|HAPPY", "F|SOFIA|THINKING", "T|Crooked Sofia|How about I go make a better portrayal of myself for you." },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|Now, I sense you're new around these parts." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|LAUGHING", "F|SOFIA|UNIMPRESSED", "T|Sofia|How could you tell?" },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|What can I say? I'm perceptive. Perceptive *and* helpful for folks such as yourself, hear." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|HAPPY", "F|SOFIA|THINKING", "T|Crooked Sofia|So hows about I give you a lowdown on some of these parts as a bit of a warmup gift?" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Okay, I'm listening." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|LAUGHING", "T|Crooked Sofia|Best choice you ever made, sister." },
                        new object[] { "B|SOFIA_CROOKED_PRIMARY_FINALIZE" }
                    };
                    break;
                case "SOFIA_CROOKED_PRIMARY_SHAKE":
                    Script = new object[]
                    {
                        new object[] { "C|GWS:CONTINUE", "D|#CBUTTONS", "F|SOFIA|HAPPY", "T|Sofia|You know what, sure, what the hell." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|GRINNING", "T||You shake her hand." },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|Hell yeah! Always love to meet a potential new customer." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|EXCITED", "F|SOFIA|CONSIDERING", "T|Sofia|What can I say, something about your straight up, absolutely-definitely-shady demeanour must have charmed me." },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|So how about some of that [R]ESSENCE[C:WHITE], huh? Can I hook you up?" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "T|Sofia|I mean, I still don't really know what essence is?" },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|LAUGHING", "F|SOFIA|DOWNCAST", "T|Sofia|Or, um. Anything about what I would do with it?" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|HAPPY", "T|Crooked Sofia|Oh, don't you be worrying about that, sister. When you feel it, you'll know." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|GRINNING", "T|Crooked Sofia|So what do you say?" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "T|Sofia|Well..." },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|You know what? As you've been so nice and friendwise, how about I give you a little taster, on the house." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|EXCITED", "T|Crooked Sofia|A little essence instillation for yourself. A booster, little pickmeup." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|HAPPY", "T|Crooked Sofia|It won't do ya any harm at all. Promise." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "T|Sofia|Uh..." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Well, sure. Okay, I guess that'd be fine." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|EXCITED", "T|Crooked Sofia|Excellent choice! Now hold still." },
                        new object[] { "C|TIME:2000:ORSKIP", "F|CROOKED SOFIA|DOWNCAST", "T||" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "T|Sofia|Uh, I don't feel anythin-" },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|GRINNING", "F|SOFIA|JUDGING", "T|Sofia|[R]Whoa.", new VoidDel(delegate()
                        {
                            Sofia.CrookedFlag = 3;
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                WorldEntity Add = new Sofia.EssenseGlow("ESSENCEGLOW", new Vector2(0, 0), (TAtlasInfo)Shell.AtlasDirectory["ESSENCEGLOW"], 0.6f);
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                            }));
                        })
                        },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|Told ya." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|[R]I feel... glowing." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|[R]Like I'm..." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|[R]Somehow, even more Sofia-like than I was before. Huh!" },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|Hah! Welcome to the wonders of [R]SOFIA ESSENCE[C:WHITE]. ;)" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|[R]I can see why they said this world is wanting for more of it..." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Wait, it's fading...", new VoidDel(delegate()
                        {
                            Sofia.EssenseGlow EG = (Sofia.EssenseGlow)Shell.GetEntityByName("ESSENCEGLOW");
                            EG.AnimationQueue.Add(Animation.Retrieve("fadeoutcolourpreserve"));
                            EG.TransientAnimation = true;
                        })
                        },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "D|ESSENCEGLOW|IFPRESENT", "T|Sofia|Huh. What happened?" },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|HAPPY", "T|Crooked Sofia|Good things can't be lasting forever. The initial effects fade quickwise." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "F|CROOKED SOFIA|EXCITED", "T|Crooked Sofia|But that essence is part of you now." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|I see..." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|CONSIDERING", "F|CROOKED SOFIA|LAUGHING", "T|Sofia|Well, that certainly was something." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|HAPPY", "T|Crooked Sofia|I bet you it was, Sis." },
                        new object[] { "B|SOFIA_CROOKED_PRIMARY_FINALIZE" }
                    };
                    break;
                case "SOFIA_CROOKED_PRIMARY_FINALIZE":
                    Script = new object[]
                    {
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|EXCITED", "T|Crooked Sofia|You won't regret wandering into my part of town. Let me be your intrepid guide to the underbelly of society." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|HAPPY", "T|Crooked Sofia|See, folks like me are all the more in demand these days, what with the little shortage on [R]ESSENCE[C:WHITE] that you are so astutely informed abouts, now seems." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|DOWNCAST", "T|Crooked Sofia|So those who are able to scrape a little bit off the top, such as, ahem, yours truly, we can do good business for those in need." },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|And there are few more acquainted than me with the real good stuff. Maybe not even the [C:138-0-255-255]MYSTIC SOFIA[C:WHITE] herself can top my scoping of the [R]TRUE SOFIA ESSENCE[C:WHITE]." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|GRINNING", "T|Crooked Sofia|That's how I can tell you're not from around these parts, Sis. In fact, I have an inkling..." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|LAUGHING", "F|SOFIA|CONSIDERING", "T|Crooked Sofia|That this may not be your true world, even. ;)" },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|HAPPY", "T|Sofia|I- how did you know that? From my, uh, own \"essence\"?" },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|You look like you have something of an all hells unique flavour, babe. Almost the essence I'm used to but also... not." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|EXCITED", "T|Crooked Sofia|You have an interdimensional tang. It's not something I see all that oftentimes." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "T|Crooked Sofia|Let's say you piqued my interest when you up and walked into my establishment here." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Yeah, the \"Cool Sofia\" was saying something about that." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|CONSIDERING", "T|Sofia|She was saying something about how my essence is special..." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|JUDGING", "T|Sofia|That she wanted to use it to rejuvinate this world's essence or something?", new VoidDel(delegate() { Sofia.CrookedKnowledgeFlag = 1; }) },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|Whoa! That's quite the pronouncment there, sisterino." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "T|Crooked Sofia|I hope you're gonna be pursuing that particular line of doifying rightawaywise, hear." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Uh, what? You want me to get involved with this crazy essense bullshit?" },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|CONSIDERING", "T|Crooked Sofia|You betcha, rightly." },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|You saying there's a new [R]ESSENCE SOURCE[C:WHITE] somewhere inside that self of yours?" },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|EXCITED", "T|Crooked Sofia|You gotta crack it open right now immediately!" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "T|Sofia|But... I thought you were profiting from the shortfall? What-" },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|You think they'll ease up regulation after all these years of keeping it locked up? And besides, even I'm running dry these days. There more essence there is, the more there is for me to steal!" },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|GRINNING", "T|Crooked Sofia|Uh, that is to say... Acquire through innovative business practices." },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|But, anyway, I think you'll be doing a true righteous thing pulling this little world of ours out of the proverbial gutter, hear." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|THINKING", "T|Crooked Sofia|So, when are you meeting up with this Cool Sofia, you say?" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "T|Sofia|Um..." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|UNIMPRESSED", "T|Sofia|I mean I... Actually... Sort of ran away from her?" },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|Aw, hell, well we best rectify that immediately!" },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|THINKING", "T|Crooked Sofia|Do you know where she is?" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|DOWNCAST", "T|Sofia|I think she said something about seeing a King? Like, a King Sofia or something?" },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|CONSIDERING", "T|Crooked Sofia|You'll want the castle then. Hmm, not like I can go near there muchwise these days. Hmm..." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "T|Crooked Sofia|I have an idea. Here, take this map." },
                        new object[] { "C|GWS:CONTINUE", "T||The Crooked Sofia hands you a [C:138-0-255-255]MAP[C:WHITE].", new VoidDel(delegate()
                        {
                            Sofia.ParanoidFlag = 2;
                            if(Sofia.CrookedFlag == 3) { Sofia.CrookedFlag = 4; }
                        })
                        },
                        ReceiveNav,
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "T|Crooked Sofia|This should lead you to the CASTLE. Just don't let them know I sent you, coursewise. ;)" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "T|Sofia|Thanks, I guess?" },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|EXCITED", "T|Crooked Sofia|Great! Well then, off you pop now." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|W- wait, you want me to go right now?" },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|GRINNING", "T|Crooked Sofia|No time like the present, sister! The map should show you where to go." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|HAPPY", "T|Crooked Sofia|No need for thanks. You can say so later." },
                        new object[] { "C|GWS:CONTINUE", "T||The Crooked Sofia sweeps you a dramatic bow." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Uh, yeah. Thanks." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|GRINNING", "T|Crooked Sofia|I said no need, hear!" },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|HAPPY", "T|Crooked Sofia|But I'll be seeing you. ;)" },
                        new object[] { "C|GWS:impos_mapnavigate", "T||Use the [C:138-0-255-255]MAP[C:WHITE] to navigate to your next destination." },
                    };
                    break;
                case "SOFIA_MYSTIC_NO_EDICT":
                    Script = new object[]
                    {
                        new object[] { "C|GWS:CONTINUE", "T||The map leads you towards a place relatively nearby labelled only as the [R]SOURCE[C:WHITE]. You're not particularly sure what this is, but as it's the only place labelled in big fancy glowing letters, you figure it's probably worth checking out while you're here." },
                        new object[] { "C|GWS:CONTINUE", "T||You arrive and discover... what seems to be a cave entrance? What is it with this place and caves?" },
                        new object[] { "C|GWS:CONTINUE", "T||You don't mind, though. You freaking love caves." },
                        new object[] { "C|GWS:CONTINUE", "T||This one is glowing slightly, though. [T:300]So that's different.", new VoidDel(delegate()
                        {
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {

                                WorldEntity Add = new WorldEntity("SOURCECAVEBG", new Vector2(-75, -180), (TAtlasInfo)Shell.AtlasDirectory["SOURCECAVEBG"], 0.05f);
                                Add.ColourValue = new Color(0,0,0,0);
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                                Add = new WorldEntity("SOFIA", new Vector2(350, 405), (TAtlasInfo)Shell.AtlasDirectory["SOFIASPRITES"], 0.5f);
                                Add.ColourValue = new Color(0,0,0,0);
                                Add.CenterOrigin = true;
                                Add.SetAtlasFrame(new Point(0, 2));
                                Add.Scale(new Vector2(-0.06f, -0.06f));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                                Sofia.SourceGlow S2 = new Sofia.SourceGlow("SOURCE", new Vector2(-75, -180), (TAtlasInfo)Shell.AtlasDirectory["SOURCE"], 0.06f);
                                S2.ColourValue = new Color(0,0,0,0);
                                Shell.UpdateQueue.Add(S2);
                                Shell.RenderQueue.Add(S2);
                                Add = new WorldEntity("MYSTIC SOFIA", new Vector2(1530, 405), (TAtlasInfo)Shell.AtlasDirectory["MYSTICSOFIA"], 0.48f);
                                Add.CenterOrigin = true;
                                Add.SetAtlasFrame(new Point(1, 1));
                                Add.Scale(new Vector2(-0.06f, -0.06f));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                            }));
                        })
                        },
                        new object[] { "C|GWS:CONTINUE", "A|SOFIA|FADEIN", "T|Sofia|Um..." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|CONSIDERING", "T|Sofia|He- hewwo?" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|EXCITED", "T|Sofia|Is anyone here? HEWWO?" },
                        new object[] { "C|GWS:CONTINUE", "T||You walk further in." },
                        new object[] { "C|TIME:1000:ORSKIP", "F|SOFIA|JUDGING", "T|Sofia|Holy-!" },
                        new object[] { "C|GWS:CONTINUE", "M|SOURCE|TRUE", "A|SOURCECAVEBG|FADEINLONG", "A|SOURCE|FADEINLONG" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|LAUGHING", "T|Sofia|What- What is...?" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "T|???|Go away." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Huh?" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Hello? Is somebody there?" },
                        new object[] { "C|GWS:CONTINUE", "T|???|You have not been invited here. Go away." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Um-" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|JUDGING", "A|MYSTIC SOFIA|-600=0,500,20||||", "T|Mystic Sofia|I SAID GO AWAY!" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "T|Sofia|Christ! You have no e-[T:400] I mean, I'm leaving! I'm leaving!" },
                        new object[] { "C|GWS:CONTINUE", "A|SOURCECAVEBG|FADEOUT", "A|SOURCE|FADEOUT", "A|MYSTIC SOFIA|FADEOUT", "T|Sofia|..." },
                        new object[] { "C|GWS:CONTINUE", "T||Yeah, you won't be going back in there any time soon." },
                        new object[] { "C|GWS:CONTINUE", "T||At least, not without assurance you can get past the crazy eyeless lady." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|DOWNCAST", "T||Probably best to head somewhere else for now.[T:300] Wasn't there somewhere you were supposed to be?" },
                        new object[] { "C|GWS:impos_mapnavigate", "T||Use the [C:138-0-255-255]MAP[C:WHITE] to navigate to your next destination." },
                    };
                    break;
                case "SOFIA_MYSTIC_EXPLANATION":
                    Script = new object[]
                    {
                        new object[] { "C|GWS:CONTINUE", "M|#NULL", "T||The way to the [R]SOURCE[C:WHITE] is not hard to find.", new VoidDel(delegate()
                        {
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {

                                WorldEntity Add = new WorldEntity("SOURCECAVEBG", new Vector2(-75, -180), (TAtlasInfo)Shell.AtlasDirectory["SOURCECAVEBG"], 0.05f);
                                Add.ColourValue = new Color(0,0,0,0);
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                                Add = new WorldEntity("SOFIA", new Vector2(350, 405), (TAtlasInfo)Shell.AtlasDirectory["SOFIASPRITES"], 0.5f);
                                Add.ColourValue = new Color(0,0,0,0);
                                Add.CenterOrigin = true;
                                Add.SetAtlasFrame(new Point(0, 2));
                                Add.Scale(new Vector2(-0.06f, -0.06f));
                                Add.AnimationQueue.Add(Animation.Retrieve("FADEIN"));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                                Sofia.SourceGlow S2 = new Sofia.SourceGlow("SOURCE", new Vector2(-75, -180), (TAtlasInfo)Shell.AtlasDirectory["SOURCE"], 0.06f);
                                S2.ColourValue = new Color(0,0,0,0);
                                Shell.UpdateQueue.Add(S2);
                                Shell.RenderQueue.Add(S2);
                                Add = new WorldEntity("MYSTIC SOFIA", new Vector2(1530, 405), (TAtlasInfo)Shell.AtlasDirectory["MYSTICSOFIA"], 0.48f);
                                Add.CenterOrigin = true;
                                Add.SetAtlasFrame(new Point(2, 1));
                                Add.Scale(new Vector2(-0.06f, -0.06f));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                            }));
                        })
                        },
                        new object[] { "C|GWS:CONTINUE", "T||In fact, it's marked on your map in big glowing letters. Fitting, for a place that is seemingly so important." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "T||(You do wonder how they managed to do the glowing letters on paper, though)." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|DOWNCAST", "T||You've been sort of going along with all this, but you haven't really thought too much about what it means that you might literally be going to save a world, though." },
                        new object[] { "C|GWS:CONTINUE", "T||It's a hard concept to grasp. But the sight of this glowing cave entrance is definitely starting to hammer it home." },
                        new object[] { "C|GWS:CONTINUE", "M|SOURCE|TRUE", "F|SOFIA|HAPPY", "T|Sofia|Oh my God.", "A|SOURCECAVEBG|FADEINLONG", "A|SOURCE|FADEINLONG" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|EXCITED", "T|Sofia|This place is incredible!" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Hello! Are you here?" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|I'm here to find the Mystic Sofia!" },
                        new object[] { "C|TIME:1000:ORSKIP", "T|Mystic Sofia|I see you." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Hello? Where are you?" },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|You are one sent by the King." },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|I can feel her mark upon you." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "T|Sofia|The... edict? [T:300]Yes, I've got that." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "T|Sofia|She told me to bring it to you!" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|And, well, myself." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "T|Sofia|Mostly to bring myself." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "T|Sofia|Apparently I can help!" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|I can help with the essence problem!" },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|Is that so?" },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|Well then." },
                        new object[] { "C|TIME:1500:ORSKIP", "T|Mystic Sofia|I guess I better take a better look at you.", "A|MYSTIC SOFIA|-600=0,2000,20||||" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|JUDGING" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Whoa! Whoa, hello!" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "T|Sofia|You- you haven't..." },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|I have noticed my eyes, yes." },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|THINKING", "T|Mystic Sofia|But even so, they are not even as strange as yours." },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|DOWNCAST", "T|Mystic Sofia|You are not from here, are you?" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|No. No, I'm not." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|I'm not from here at all." },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|Yes." },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|THINKING", "T|Mystic Sofia|I see, I see..." },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|This is..." },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|GRINNING", "T|Mystic Sofia|This is wonderful.", new VoidDel(delegate() { Sofia.MysticFlag = 1; }) },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|I..." },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|LAUGHING", "T|Mystic Sofia|[R]Traveller from another world, I see you." },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|[R]I see you and I welcome you, on the behalf of ULTRASOFIAWORLD." },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|EXCITED", "T|Mystic Sofia|[R]Surely your presence here is truly a machination of fate." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|UNIMPRESSED", "T|Sofia|Well, maybe fate, and also the Cool Sofia." },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|HAPPY", "T|Mystic Sofia|Quite." },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|EXCITED", "T|Mystic Sofia|[R]This is the first time that new ESSENCE has come to ULTRASOFIAWORLD in living memory." },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|DOWNCAST", "T|Mystic Sofia|[R]That is saying a lot for us." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "T|Mystic Sofia|[R]But I cannot understate the magnitude of your arrival." },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|HAPPY", "T|Mystic Sofia|[R]SOFIA." },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|[R]You are going to save us all." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Well, look. I'm glad!" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|That sounds really great for you guys! [T:400]And I do want to help!" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "T|Sofia|But, look, listen." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "T|Sofia|You have to understand, I don't really know anything about all this!" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Where this place is- WHAT this place is." },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|THINKING", "T|Mystic Sofia|This is [C:PURPLE]ULTRASOFIAWORLD[C:WHITE]." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|I know that!" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|But what I don't know is what that means!" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Look, I want to help you guys. But I don't know what it means to have SOFIA ESSENCE. I don't know what THE PROCESS means." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|DOWNCAST", "T|Sofia|And, frankly, that's making me a little nervous. [T:300]Especially with the pressure that's being put on me here..." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "T|Sofia|I don't really know what I'm getting into!" },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|DOWNCAST", "T|Mystic Sofia|I see." },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|Before you proceed, you wish for an explanation." },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|THINKING", "T|Mystic Sofia|You wish to truly know what [C:PURPLE]ULTRASOFIAWORLD[C:WHITE] is, and what role you must play to save it." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Yes!" },
                        new object[] { "C|TIME:1000:ORSKIP", "F|MYSTIC SOFIA|JUDGING", "T|Mystic Sofia|[R]Very well." },
                        new object[] { "C|TIME:2500", "M|#NULL", new VoidDel(delegate()
                        {
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                WorldEntity Add = new WorldEntity("FADEBLACK", new Vector2(0, 0), (TAtlasInfo)Shell.AtlasDirectory["BLACK"], 0.8f);
                                Add.ColourValue = new Color(0,0,0,0);
                                Animation A = new Animation("fadetoblack");
                                A.WriteColouring(Animation.CreateColourTween(new ColourShift(255,255,255,255), 2000, 20));
                                Add.AnimationQueue.Add(A);
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                            }));
                        })
                        },
                        new object[] { "D|FADEBLACK", "D|SOFIA", "D|MYSTIC SOFIA", "D|SOURCECAVEBG", "D|SOURCE", "B|SOFIA_MYSTIC_STORY" }
                    };
                    break;
                case "SOFIA_MYSTIC_STORY":
                    Script = new object[]
                    {
                        new object[] { "C|TIME:6000", "M|LEGEND|FALSE", "T|Mystic Sofia|I'm going to tell you a story.", new VoidDel(delegate()
                        {
                            if(Sofia.StoryFlag == 0) { Sofia.StoryFlag = 1; }
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                WorldEntity Add = new WorldEntity("STORY1BASE", new Vector2(0, -120), (TAtlasInfo)Shell.AtlasDirectory["MYSTICSTORY1_BASE"], 0.1f);
                                Add.ColourValue = new Color(0,0,0,0);
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                                Add = new WorldEntity("STORY1LIGHTNING", new Vector2(0, -120), (TAtlasInfo)Shell.AtlasDirectory["MYSTICSTORY1_BOLTS"], 0.2f);
                                Add.ColourValue = new Color(0,0,0,0);
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                                Add = new WorldEntity("STORY1FLASH", new Vector2(0, 0), (TAtlasInfo)Shell.AtlasDirectory["BGFLASHER"], 0.25f);
                                Add.ColourValue = new Color(0,0,0,0);
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                                Add = new WorldEntity("STORY2", new Vector2(0, 0), (TAtlasInfo)Shell.AtlasDirectory["MYSTICSTORY2"], 0.3f);
                                Add.ColourValue = new Color(0,0,0,0);
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                                Add = new WorldEntity("STORY3", new Vector2(-120, -60), (TAtlasInfo)Shell.AtlasDirectory["MYSTICSTORY3"], 0.4f);
                                Add.ColourValue = new Color(0,0,0,0);
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                                Add = new WorldEntity("STORY4", new Vector2(0, 0), (TAtlasInfo)Shell.AtlasDirectory["MYSTICSTORY4"], 0.5f);
                                Add.ColourValue = new Color(0,0,0,0);
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                                Add = new WorldEntity("STORY5", new Vector2(-720, 0), (TAtlasInfo)Shell.AtlasDirectory["MYSTICSTORY5"], 0.6f);
                                Add.ColourValue = new Color(0,0,0,0);
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                                Add = new WorldEntity("STORY6", new Vector2(640, 360), (TAtlasInfo)Shell.AtlasDirectory["MYSTICSTORY6"], 0.7f);
                                Add.ColourValue = new Color(0,0,0,0);
                                Add.CenterOrigin = true;
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                                Add = new WorldEntity("STORY7", new Vector2(640, 360), (TAtlasInfo)Shell.AtlasDirectory["MYSTICSTORY7"], 0.8f);
                                Add.ColourValue = new Color(0,0,0,0);
                                Add.CenterOrigin = true;
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                            }));
                        })
                        },
                        new object[] { "C|TIME:6000", "T|Mystic Sofia|It is a story of one world born from many." },
                        new object[] { "C|TIME:6000", "T|Mystic Sofia|It is a story of fate spanning the multiverse." },
                        new object[] { "C|TIME:3000", "T|Mystic Sofia|This is the story..." },
                        new object[] { "C|TIME:3000", "T|Mystic Sofia|Of [R]ULTRASOFIAWORLD[C:WHITE]." },
                        new object[] { "C|TIME:10000", "T||Eons ago, our world was devoid of flesh, and yet unformed.", "A|STORY1BASE|FADEINMED", "A|STORY1BASE|0=120,12000,20||||", "A|STORY1LIGHTNING|0=120,12000,20||||", "A|STORY1LIGHTNING|FLASH2|LOOP", "A|STORY1FLASH|FLASH1|LOOP"  },
                        new object[] { "C|TIME:2000", "A|STORY1BASE|FADEOUTMED" },
                        new object[] { "C|TIME:10000", "D|STORY1BASE", "D|STORY1LIGHTNING", "D|STORY1FLASH", "T||Then, a mysterious power fell from the stars. [T:400]This was the power of [R]SOFIA[C:WHITE].", "A|STORY2|FADEINMED", "A|STORY2|0=-720,12000,20||||" },
                        new object[] { "C|TIME:2000", "A|STORY2|FADEOUTMED" },
                        new object[] { "C|TIME:6000", "D|STORY2", "T||This power came from other worlds, seeded before our own. [T:400]Across the multiverse, each extant instance of [R]SOFIA[C:WHITE] allowed it to grow yet more in strength.", "A|STORY3|FADEINMED", "A|STORY3|120=0,12000,20||||" },
                        new object[] { "C|TIME:4000", "T||[C:PURPLE]SOFIAs[C:WHITE] from other worlds. [C:PURPLE]SOFIAs[C:WHITE] like you. Together, their [R]ESSENCE[C:WHITE] was able to manifest in our dimension." },
                        new object[] { "C|TIME:2000", "A|STORY3|FADEOUTMED" },
                        new object[] { "C|TIME:10000", "D|STORY3", "T||As the undiluted [R]SOFIA ESSENCE[C:WHITE] collided with our nascent realm, it formed into physical vessels of the SOFIA. [C:PURPLE]US[C:WHITE].", "A|STORY4|FADEINMED", "A|STORY4|-720=0,12000,20||||" },
                        new object[] { "C|TIME:2000", "A|STORY4|FADEOUTMED" },
                        new object[] { "C|TIME:10000", "D|STORY4", "T||In this manner, [C:PURPLE]ULTRASOFIAWORLD[C:WHITE] came to be.", "A|STORY5|FADEINMED", "A|STORY5|720=0,12000,20||||" },
                        new object[] { "C|TIME:2000", "A|STORY5|FADEOUTMED" },
                        new object[] { "C|TIME:5000", "D|STORY5", "T||But in recent times...", "A|STORY6|FADEINMED", "A|STORY6|||-0.09=-0.13,12000,20||", "A|STORY7|||-0.09=-0.13,12000,20||" },
                        new object[] { "C|TIME:2500", "T||The remaining power of [R]SOFIA ESSENCE[C:WHITE] has begun to wane...", "A|STORY7|FADEINMED" },
                        new object[] { "C|TIME:2500", "D|STORY6" },
                        new object[] { "C|TIME:4000", "A|STORY7|FADEOUTMED" },
                        new object[] { "C|GWS:impos", "D|STORY7", new VoidDel(delegate()
                        {
                            if(Sofia.StoryFlag == 1) { ActivateScriptElement("B|SOFIA_MYSTIC_AFTER_STORY"); }
                            else if(Sofia.StoryFlag == 2) { ActivateScriptElement("B|SOFIA_MYSTIC_AFTER_STORY_SECONDARY"); }
                        })
                        }
                    };
                    break;
                case "SOFIA_MYSTIC_AFTER_STORY":
                    Script = new object[]
                    {
                        new object[] { "C|TIME:2000:ORSKIP", "T||", new VoidDel(delegate()
                        {
                            Sofia.StoryFlag = 2;
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {

                                WorldEntity Add = new WorldEntity("FADEBLACK", new Vector2(0, 0), (TAtlasInfo)Shell.AtlasDirectory["BLACK"], 0.8f);
                                Animation A = new Animation("fadetoblack");
                                A.WriteColouring(Animation.CreateColourTween(new ColourShift(-255,-255,-255,-255), 2000, 20));
                                Add.AnimationQueue.Add(A);
                                 Add.TransientAnimation = true;
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                                Add = new WorldEntity("SOURCECAVEBG", new Vector2(-75, -180), (TAtlasInfo)Shell.AtlasDirectory["SOURCECAVEBG"], 0.05f);
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                                Add = new WorldEntity("SOFIA", new Vector2(350, 405), (TAtlasInfo)Shell.AtlasDirectory["SOFIASPRITES"], 0.5f);
                                Add.CenterOrigin = true;
                                Add.SetAtlasFrame(new Point(1, 1));
                                Add.Scale(new Vector2(-0.06f, -0.06f));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                                Sofia.SourceGlow S2 = new Sofia.SourceGlow("SOURCE", new Vector2(-75, -180), (TAtlasInfo)Shell.AtlasDirectory["SOURCE"], 0.06f);
                                Shell.UpdateQueue.Add(S2);
                                Shell.RenderQueue.Add(S2);
                                Add = new WorldEntity("MYSTIC SOFIA", new Vector2(930, 405), (TAtlasInfo)Shell.AtlasDirectory["MYSTICSOFIA"], 0.48f);
                                Add.CenterOrigin = true;
                                Add.SetAtlasFrame(new Point(2, 1));
                                Add.Scale(new Vector2(-0.06f, -0.06f));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                            }));
                        })
                        },
                        new object[] { "C|GWS:CONTINUE", "D|FADEBLACK|IFPRESENT", "M|SOURCE|TRUE", "T|Sofia|Whoa." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "T|Sofia|How did you get all those pictures to appear in my head like that?" },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|There's a knack to it." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|CONSIDERING", "T|Sofia|Hmmmm........" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "T|Sofia|Sounds suspicious, but I'll accept it." },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|Regardless, I hope the exposition was illuminating." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "T|Sofia|It was... sort of confusing, to be honest." },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|I can tell you it again, if you'd like." },
                        new object[] { "C|GWS:impos", "F|SOFIA|CONSIDERING", "T|Sofia|Ehhh...", new VoidDel(delegate()
                        {
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                Button B = ButtonScripts.GetQuickButton("Listen to the Mystic Sofia's story again.", ButtonScripts.DelegateFetch("runscript_SOFIA_MYSTIC_AFTER_STORY_REPEAT"));
                                B.MLCRecord = new String[] { "runscript_SOFIA_MYSTIC_AFTER_STORY_REPEAT" };
                                B.QuickMoveTo(new Vector2(640, 200));
                                Button B2 = ButtonScripts.GetQuickButton("Don't do that.", ButtonScripts.DelegateFetch("runscript_SOFIA_MYSTIC_AFTER_STORY_NO_REPEAT"));
                                B2.MLCRecord = new String[] { "runscript_SOFIA_MYSTIC_AFTER_STORY_NO_REPEAT" };
                                B2.QuickMoveTo(new Vector2(640, 320));
                                Shell.UpdateQueue.Add(B);
                                Shell.RenderQueue.Add(B);
                                Shell.UpdateQueue.Add(B2);
                                Shell.RenderQueue.Add(B2);
                            }));
                        })
                        }
                    };
                    break;
                case "SOFIA_MYSTIC_AFTER_STORY_SECONDARY":
                    Script = new object[]
                    {
                        new object[] { "C|TIME:2000:ORSKIP", "T||", new VoidDel(delegate()
                        {
                            Sofia.StoryFlag = 2;
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {

                                WorldEntity Add = new WorldEntity("FADEBLACK", new Vector2(0, 0), (TAtlasInfo)Shell.AtlasDirectory["BLACK"], 0.8f);
                                Animation A = new Animation("fadetoblack");
                                A.WriteColouring(Animation.CreateColourTween(new ColourShift(-255,-255,-255,-255), 2000, 20));
                                Add.AnimationQueue.Add(A);
                                 Add.TransientAnimation = true;
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                                Add = new WorldEntity("SOURCECAVEBG", new Vector2(-75, -180), (TAtlasInfo)Shell.AtlasDirectory["SOURCECAVEBG"], 0.05f);
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                                Add = new WorldEntity("SOFIA", new Vector2(350, 405), (TAtlasInfo)Shell.AtlasDirectory["SOFIASPRITES"], 0.5f);
                                Add.CenterOrigin = true;
                                Add.SetAtlasFrame(new Point(1, 1));
                                Add.Scale(new Vector2(-0.06f, -0.06f));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                                Sofia.SourceGlow S2 = new Sofia.SourceGlow("SOURCE", new Vector2(-75, -180), (TAtlasInfo)Shell.AtlasDirectory["SOURCE"], 0.06f);
                                Shell.UpdateQueue.Add(S2);
                                Shell.RenderQueue.Add(S2);
                                Add = new WorldEntity("MYSTIC SOFIA", new Vector2(930, 405), (TAtlasInfo)Shell.AtlasDirectory["MYSTICSOFIA"], 0.48f);
                                Add.CenterOrigin = true;
                                Add.SetAtlasFrame(new Point(2, 1));
                                Add.Scale(new Vector2(-0.06f, -0.06f));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                            }));
                        })
                        },
                        new object[] { "C|GWS:CONTINUE", "D|FADEBLACK|IFPRESENT", "M|SOURCE|TRUE", "F|SOFIA|CONSIDERING", "T|Mystic Sofia|There you are." },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|LAUGHING", "T|Mystic Sofia|Want to hear it another time, child?" },
                        new object[] { "C|GWS:impos", "F|SOFIA|THINKING", "T|Sofia|Uhh...", new VoidDel(delegate()
                        {
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                Button B = ButtonScripts.GetQuickButton("Listen to the Mystic Sofia's story again, again.", ButtonScripts.DelegateFetch("runscript_SOFIA_MYSTIC_AFTER_STORY_REPEAT"));
                                B.MLCRecord = new String[] { "runscript_SOFIA_MYSTIC_AFTER_STORY_REPEAT" };
                                B.QuickMoveTo(new Vector2(640, 200));
                                Button B2 = ButtonScripts.GetQuickButton("Please no, not another time.", ButtonScripts.DelegateFetch("runscript_SOFIA_MYSTIC_AFTER_STORY_NO_REPEAT"));
                                B2.MLCRecord = new String[] { "runscript_SOFIA_MYSTIC_AFTER_STORY_NO_REPEAT" };
                                B2.QuickMoveTo(new Vector2(640, 320));
                                Shell.UpdateQueue.Add(B);
                                Shell.RenderQueue.Add(B);
                                Shell.UpdateQueue.Add(B2);
                                Shell.RenderQueue.Add(B2);
                            }));
                        })
                        }
                    };
                    break;
                case "SOFIA_MYSTIC_AFTER_STORY_REPEAT":
                    Script = new object[]
                    {
                        new object[] { "C|GWS:CONTINUE", "D|#CBUTTONS", "T|Sofia|You know what? Sure." },
                        new object[] { "C|TIME:1000:ORSKIP", "F|MYSTIC SOFIA|JUDGING", "T|Mystic Sofia|[R]Okay then." },
                        new object[] { "C|TIME:2500", "M|#NULL", new VoidDel(delegate()
                        {
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                WorldEntity Add = new WorldEntity("FADEBLACK", new Vector2(0, 0), (TAtlasInfo)Shell.AtlasDirectory["BLACK"], 0.8f);
                                Add.ColourValue = new Color(0,0,0,0);
                                Animation A = new Animation("fadetoblack");
                                A.WriteColouring(Animation.CreateColourTween(new ColourShift(255,255,255,255), 2000, 20));
                                Add.AnimationQueue.Add(A);
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                            }));
                        })
                        },
                        new object[] { "D|FADEBLACK", "D|SOFIA", "D|MYSTIC SOFIA", "D|SOURCECAVEBG", "D|SOURCE", "B|SOFIA_MYSTIC_STORY" }
                    };
                    break;
                case "SOFIA_MYSTIC_AFTER_STORY_NO_REPEAT":
                    Script = new object[]
                    {
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|HAPPY", "D|#CBUTTONS", "T|Sofia|Nah, I'm good." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "T|Sofia|I think I sort of understand what this place is a little better now?" },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|THINKING", "T|Mystic Sofia|Let me recap in more basic terms for the sake of certainty." },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|HAPPY", "T|Mystic Sofia|Essentially, myself, my fellows and this entire world originally formed from [R]SOFIA ESSENCE[C:WHITE]." },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|We have existed in this state for many thousands of years. But the essence that we were created from is finite." },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|THINKING", "T|Mystic Sofia|It can't last forever, and we are starting to run out." },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|EXCITED", "T|Mystic Sofia|That's where you come in." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Right." },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|HAPPY", "T|Mystic Sofia|Because you are an original, pure Sofia - a physical Sofia, unlike the essence-born of our world..." },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|Your essence is self-replenishing and essentially unlimited." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|HAPPY", "T|Mystic Sofia|If we were to transfer a portion of [C:PURPLE]YOUR ESSENCE[C:WHITE] to the [R]SOURCE[C:WHITE], the concentrated central vein of essence that sustains our world..." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|The glowing rainbow line up there?" },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|Yes, that." },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|THINKING", "T|Mystic Sofia|If I can transfer some of your essence into that, then, according to my theories, as best as I can tell..." },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|EXCITED", "T|Mystic Sofia|The [R]SOURCE[C:WHITE] should rejuvinate! Your [C:PURPLE]ESSENCE[C:WHITE] will cause the source to replenish itself." },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|It should be enough to sustain [C:PURPLE]ULTRASOFIAWORLD[C:WHITE] for yet more thousands of years." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|GRINNING", "T|Sofia|I never knew my SOFIANESS would be such a highly prized quality!" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|EXCITED", "F|MYSTIC SOFIA|HAPPY", "T|Sofia|If it does no harm to me, then I'm totally down." },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|It won't. The process should ultimately be harmless, and your own essence will replenish itself immediately." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Well then, let's get going!" },
                        new object[] { "C|TIME:1500:ORSKIP", "F|SOFIA|GRINNING", "T|Sofia|I can't wait to pump my Sofia juice into your cave rainbow. :D" },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|LAUGHING" },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|You won't find any hesitation on my part." },
                        new object[] { "C|TIME:1500:ORSKIP", "F|MYSTIC SOFIA|HAPPY", "T|Mystic Sofia|But I'm afraid there's a slight catch before we could possibly start that process." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Oh?" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|...now I'm slightly worried." },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|It's nothing to particularly worry about, I assure you." },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|DOWNCAST", "T|Mystic Sofia|Merely a roadblock." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|A roadblock, you say..." },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|Yes." },
                        new object[] { "B|SOFIA_MYSTIC_ESSENCE_QUEST" }
                    };
                    break;
                case "SOFIA_MYSTIC_EXPLANATION_RETURN":
                    Script = new object[]
                    {
                        InitMysticBasic,
                        new object[] { "C|GWS:CONTINUE", "T||This time, you stroll right in, though. You've got this now. You saunter into that rainbow cave like it's no one's business, yo." },
                        new object[] { "C|GWS:CONTINUE", "A|SOFIA|FADEIN", "T||As you return to the [R]SOURCE[C:WHITE] cavern, you cast about for the Mystic Sofia. This time she is not so difficult to spot." },
                        new object[] { "C|GWS:CONTINUE", "T||She is standing in the center of the cavern, under the [R]SOURCE[C:WHITE] itself." },
                        new object[] { "C|GWS:CONTINUE", "T||She could almost be meditating, if it weren't for the slightly irritable expression on her face." },
                        new object[] { "C|GWS:CONTINUE", "M|SOURCE|TRUE", "A|MYSTIC SOFIA|FADEIN", "T|Sofia|Hello?" },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|Ah, you return." },
                        new object[] { "C|TIME:1000:ORSKIP", "T|Sofia|Yes, I-" },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|UNIMPRESSED", "T|Mystic Sofia|You left so abruptly! In the middle of an important exposition no less." },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|Do you not realize how important this is? [T:400]To our world? [T:400]Our future?" },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|JUDGING", "T|Mystic Sofia|All of [C:PURPLE]ULTRASOFIAWORLD[C:WHITE] is depending on what we do here this day!" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Look okay, I'm sorry!" },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|DOWNCAST", "T|Sofia|But I'm here now, right? And I think I understand what's going down." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|HAPPY", "T|Sofia|This place needs essence, I have that essence, I give some of it to you and the world is all saved. Right?" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Can't be that much more to it!" },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|UNIMPRESSED", "T|Mystic Sofia|..." },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|I see that you are not one for beautifully conveyed exposition and backstory." },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|DOWNCAST", "T|Mystic Sofia|A shame. [T:300]Maybe be sure to hear it all next time you're around." },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|JUDGING", "T|Mystic Sofia|But what *is* important is that you left before I could tell you the most important thing!" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "T|Sofia|Oh?" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Wait, what? What did I miss?" },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|This is why you listen instead of running off, child!" },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|DOWNCAST", "T|Mystic Sofia|Now, let me explain." },
                        new object[] { "B|SOFIA_MYSTIC_ESSENCE_QUEST" }
                    };
                    break;
                case "SOFIA_MYSTIC_ESSENCE_QUEST":
                    Script = new object[]
                    {
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|THINKING", "T|Mystic Sofia|You see, the process to transfer your [C:PURPLE]ESSENCE[C:WHITE] into the [R]SOURCE[C:WHITE]..." },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|That cannot be done with your essence alone." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|UNIMPRESSED", "T|Sofia|What! [T:300]Then what was the point of all this if we can't get it to work!" },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|JUDGING", "T|Mystic Sofia|Calm now, child! [T:200]All is not lost!" },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|THINKING", "T|Mystic Sofia|You see, the [R]SOURCE[C:WHITE] is like a door to the [R]PURE SOFIA ESSENCE SPIRIT[C:WHITE]." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|...Right." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|DOWNCAST", "T|Mystic Sofia|In order to transfer your essence it must first be [C:PURPLE]OPENED[C:WHITE]. And the only thing that can open it is another source of [R]ESSENCE[C:WHITE]." },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|Not your own essence. Being otherworldly it will be too alien to act as the key itself. [T:400]Nor, the sort of essence I can channel here..." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|UNIMPRESSED", "T|Mystic Sofia|...for that is but more of the breach's own essence, too alike for it to respond to." },
                        new object[] { "C|TIME:1500:ORSKIP", "F|MYSTIC SOFIA|HAPPY", "T|Mystic Sofia|But wait! There is a solution!" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|There is?" },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|JUDGING", "T|Mystic Sofia|Yes. You must venture out once more into this land, and seek out essence from [C:PURPLE]ESLEWHERE OTHER THAN THE [R]SOURCE[C:WHITE].", new VoidDel(delegate()
                        {
                            Sofia.MysticFlag = 2;
                            if(Sofia.CrookedFlag < 2) { Sofia.CrookedFlag = 5; }
                            else { Sofia.CrookedFlag = 3; }
                        })
                        },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "F|MYSTIC SOFIA|HAPPY", "T|Mystic Sofia|If sufficiently divorced from the [R]SOURCE[C:WHITE] by time and distance, that should be fir for the purpose." },
                        new object[] { "C|GWS:impos", new VoidDel(delegate()
                        {
                            if(Sofia.CrookedFlag == 4) { ActivateScriptElement("B|SOFIA_MYSTIC_ESSENCE_PRIOR"); }
                            else { ActivateScriptElement("B|SOFIA_MYSTIC_NO_ESSENCE"); }
                        })
                        }
                    };
                    break;
                case "SOFIA_MYSTIC_NO_ESSENCE":
                    Script = new object[]
                    {
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|What! Where am I supposed to go to find that!" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|And why do I have to do all of this?!" },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|DOWNCAST", "T|Mystic Sofia|As for where you might find such an alternative source, I cannot say." },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|I seldom leave this place and the true [R]SOURCE[C:WHITE] itself. [T:400]I am disconnected from much of this world, even as through the [R]SOURCE[C:WHITE] I reach out across all of it." },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|However, I believe that you are acquainted with the King Sofia. Potentially she will be able to help, if you are to seek out essence sources in her [C:PURPLE]KINGDOM[C:WHITE]." },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|WORRIED", "T|Mystic Sofia|But as for why [R]YOU[C:WHITE] must be the one to bear this burden..." },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|I can say only that I sense that fate has rested itself on your shoulders. You bear a critical role in our quest for salvation." },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|DOWNCAST", "T|Mystic Sofia|[R]The essences must mingle within your being and your being must become one with our world." },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|[R]In this form of union, this form of energy, you will be linked to all things SOFIA, and you too shall become SOURCE." },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|[R]The burden of this world's fate is one that only you can now bear. None else could wield the fates such." },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|[R]Not I. Nor any of the others you have met. Only you have this ability, traveller from another world." },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|[R]Our hopes rest upon you. Whether you will in time fulfill this burden and destiny, only you can know." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|I..." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|DOWNCAST", "T|Sofia|Okay." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "T|Sofia|I think..." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|I can do this." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|EXCITED", "T|Sofia|I can do this!" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|HAPPY", "T|Sofia|I mean, of course I can. [T:300]I'm awesome." },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|GRINNING", "T|Mystic Sofia|That is truly wonderful to hear!" },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|You must go, then, as soon as you can, and return just as quick." },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|Seek out another source of [R]SOFIA ESSENCE[C:WHITE]! Perhaps asking the [C:PURPLE]KING SOFIA[C:WHITE] would be a good place to start." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|EXCITED", "T|Sofia|Okay! I'll do my best!" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|HAPPY", "T|Sofia|(It's time to save the world!)" },
                        new object[] { "C|GWS:impos_mapnavigate", "T||Use the [C:138-0-255-255]MAP[C:WHITE] to navigate to your next destination." },
                    };
                    break;
                case "SOFIA_MYSTIC_ESSENCE_PRIOR":
                    Script = new object[]
                    {
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|CONSIDERING", "T|Mystic Sofia|But..." },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|..." },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|Wait." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "F|MYSTIC SOFIA|JUDGING", "T|Mystic Sofia|How is this possible...!" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|What? [T:200]What is is?" },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|CONSIDERING", "T|Mystic Sofia|Your... Your aura..." },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|JUDGING", "T|Mystic Sofia|You... ALREADY HAVE another source of [R]ESSENCE[C:WHITE] in your system!", new VoidDel(delegate() { Sofia.MysticFlag = 3; }) },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|I have?" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|EXCITED", "T|Sofia|Oh, wait, yes! I have!" },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|THINKING", "T|Mystic Sofia|How did this come to be, child?" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|HAPPY", "T|Sofia|Oh, right! [T:200]Well..." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Before I came here I went to this other cave, where there was this other Sofia." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "T|Sofia|She spoke with a weird accent and was running this strange sort of little stall..." },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|UNIMPRESSED", "T|Sofia|I'm fairly sure she was like some sort of drug dealer... Only she was selling essence?" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|CONSIDERING", "T|Sofia|Illegal, black market ESSENCE!" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|HAPPY", "T|Sofia|Anyway, she offered me some and I took it because like, what the hell am I right?" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|EXCITED", "T|Sofia|And now, here I am!" },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|I can't say I particularly associate your choices of association. You've barely been here a day and you're already in deep with the underground markets, it seems." },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|THINKING", "T|Mystic Sofia|And yet..." },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|HAPPY", "T|Mystic Sofia|By all accounts, it should serve our purpose." },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|GRINNING", "T|Mystic Sofia|Truly this must be the work of fate!" },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|You have managed to shortcut past your big quest!" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|LAUGHING", "T|Sofia|Cool! [T:300]I'm a big fan of breaking rules! :D" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|CONSIDERING", "T|Sofia|Or, at the very least, taking highly convenient shortcuts." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "T|Sofia|Maybe I'll have to check it out the next time around, though, if I haven't already." },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|What?" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|LAUGHING", "T|Sofia|Nevermind!" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|EXCITED", "F|MYSTIC SOFIA|HAPPY", "T|Mystic Sofia|Okay then!" },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|There's nothing else to wait on, then. Do you want to help me undertake the PROCESS to infuse your [R]ESSENCE[C:WHITE] save [C:PURPLE]ULTRASOFIAWORLD[C:WHITE]?" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|GRINNING", "T|Sofia|Like I said, I'm happy to try to help!" },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|Once we've started the process, we won't be able to stop. Is there anything you want to take care of first before we begin?" },
                        new object[] { "C|GWS:impos", "T|Sofia|Hmm...", new VoidDel(delegate()
                        {
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                Button B = ButtonScripts.GetQuickButton("Start the process right now.", ButtonScripts.DelegateFetch("runscript_SOFIA_MYSTIC_FINAL"));
                                B.MLCRecord = new String[] { "runscript_SOFIA_MYSTIC_FINAL" };
                                B.QuickMoveTo(new Vector2(640, 200));
                                Button B2 = ButtonScripts.GetQuickButton("There are some other things I want to do first.", ButtonScripts.DelegateFetch("runscript_SOFIA_MYSTIC_POSTPONE"));
                                B2.MLCRecord = new String[] { "runscript_SOFIA_MYSTIC_POSTPONE" };
                                B2.QuickMoveTo(new Vector2(640, 320));
                                Shell.UpdateQueue.Add(B);
                                Shell.RenderQueue.Add(B);
                                Shell.UpdateQueue.Add(B2);
                                Shell.RenderQueue.Add(B2);
                            }));
                        })
                        }
                    };
                    break;
                case "SOFIA_MYSTIC_POSTPONE":
                    Script = new object[]
                    {
                        new object[] { "C|GWS:CONTINUE", "D|#CBUTTONS", "T|Sofia|Actually, I think there are few things I want to do before we start, if that's okay with you." },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|DOWNCAST", "T|Mystic Sofia|Very well." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "F|MYSTIC SOFIA|JUDGING", "T|Mystic Sofia|Make haste back, though, as all of [C:PURPLE]ULTRASOFIAWORLD[C:WHITE] depends on what we do here this day!" },
                        new object[] { "C|TIME:1500:ORSKIP", "F|MYSTIC SOFIA|HAPPY", "T|Mystic Sofia|So be quick, alright?" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|GRINNING" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Okay, I will!" },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|Fare you well, child." },
                        new object[] { "C|GWS:impos_mapnavigate", "T||Use the [C:138-0-255-255]MAP[C:WHITE] to navigate to your next destination." },
                    };
                    break;
                case "SOFIA_MYSTIC_ESSENCE_RETURN_SUCCESS":
                    Script = new object[]
                    {
                        InitMysticBasic,
                        new object[] { "C|GWS:CONTINUE", "M|SOURCE|TRUE", "F|MYSTIC SOFIA|DOWNCAST", "A|SOFIA|FADEIN", "A|MYSTIC SOFIA|600=0,0,0|||255=255=255=255,30,30|", "F|SOFIA|GRINNING", "T||You feel like the multihued shine of the [R]SOURCE[C:WHITE] compliments the lingering glow of your newly acquired ESSENCE effectively, on a level that is both literal and metaphorical." },
                        new object[] { "C|GWS:CONTINUE", "A|MYSTIC SOFIA|-600=0,2000,20||||", "T|Mystic Sofia|Ah, welcome back." },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|THINKING", "T|Mystic Sofia|How are you progressing with locating a new [R]ESSENCE[C:WHITE] source for the procedure?" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|EXCITED", "T|Sofia|Well, I think you'll find that I have some good news, yo!" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|LAUGHING", "T|Sofia|And it's not just that I'm unbanned from Brighton Aquarium!" },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|You were banned from Brighton Aquarium?" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|UNIMPRESSED", "T|Sofia|Don't worry about it." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "T|Sofia|They just don't understand that you can't give a fish too much love." },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|Very true." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|HAPPY", "T|Sofia|Right!" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|But hey, that isn't what I'm here to tell you!" },
                        new object[] { "C|TIME:1500:ORSKIP", "F|SOFIA|EXCITED", "T|Sofia|I did it.", new VoidDel(delegate() { Sofia.MysticFlag = 3; }) },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|HAPPY" },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|You... you did it?" },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|GRINNING", "T|Mystic Sofia|You managed to find another source of essence?" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Found it and retrieved it, yo!" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|LAUGHING", "T|Sofia|Behold my amazeballs." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|HAPPY", "T|Mystic Sofia|Incredible!" },
                        new object[] { "C|TIME:1000:ORSKIP", "F|MYSTIC SOFIA|EXCITED", "T|Mystic Sofia|Let me take a look at you..." },
                        new object[] { "C|GWS:CONTINUE", "A|MYSTIC SOFIA|-300=0,1000,20||||" },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|THINKING", "T|Mystic Sofia|Yes..." },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|CONSIDERING", "T|Mystic Sofia|...yes..." },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|EXCITED", "A|MYSTIC SOFIA|300=0,1000,20||||", "T|Mystic Sofia|...yes!!!" },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|HAPPY", "T|Mystic Sofia|My child, your aura is glowing!" },
                        new object[] { "C|TIME:1000:ORSKIP", "F|MYSTIC SOFIA|LAUGHING", "T|Mystic Sofia|And not just because you got unbanned from Brighton Aquarium." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|LAUGHING" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|EXCITED", "T|Sofia|Although I'm sure that helped!" },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|Oh, for sure." },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|HAPPY", "T|Mystic Sofia|But otherwise, yes! [T:300]With the [R]ESSENCE[C:WHITE] that has been infused in you, I am confident that we will be able to proceed." },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|Where did you manage to find it?" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|UNIMPRESSED", "T|Sofia|I'm... not sure you want to know." },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|Oh?" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Let's just say some strange characters were involved, and leave it at that." },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|LAUGHING", "T|Mystic Sofia|Ah, very well, child." },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|HAPPY", "F|SOFIA|HAPPY", "T|Mystic Sofia|It matters not to me how you achieved our aims. [T:300]The fate of our very world was at stake." },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|DOWNCAST", "T|Mystic Sofia|[C:PURPLE]ULTRASOFIAWORLD[C:WHITE] will forever be in your utmost debt for what you have already achieved for us." },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|EXCITED", "T|Mystic Sofia|Now all that is left is to perform the procedure itself, and restore the [R]ESSENCE[C:WHITE] to this world once and for all." },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|GRINNING", "T|Mystic Sofia|The time is nigh at last!" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|LAUGHING", "T|Sofia|Hell yeah!" },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|HAPPY", "T|Mystic Sofia|Would you be ready to proceed right away?" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|EXCITED", "T|Sofia|Like I said, I'm happy to help!" },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|That's good!" },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|THINKING", "T|Mystic Sofia|But once we commence [C:PURPLE]THE PROCEDURE[C:WHITE], we won't be able to stop while it's ongoing." },
                        new object[] { "C|TIME:800:ORSKIP", "F|MYSTIC SOFIA|HAPPY", "T|Mystic Sofia|So, is there anything else you need to see to before we start, or shall we go ahead now?" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING" },
                        new object[] { "C|GWS:impos", "T|Sofia|Hmm...", new VoidDel(delegate()
                        {
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                Button B = ButtonScripts.GetQuickButton("Start the process right now.", ButtonScripts.DelegateFetch("runscript_SOFIA_MYSTIC_FINAL"));
                                B.MLCRecord = new String[] { "runscript_SOFIA_MYSTIC_FINAL" };
                                B.QuickMoveTo(new Vector2(640, 200));
                                Button B2 = ButtonScripts.GetQuickButton("There are some other things I want to do first.", ButtonScripts.DelegateFetch("runscript_SOFIA_MYSTIC_POSTPONE"));
                                B2.MLCRecord = new String[] { "runscript_SOFIA_MYSTIC_POSTPONE" };
                                B2.QuickMoveTo(new Vector2(640, 320));
                                Shell.UpdateQueue.Add(B);
                                Shell.RenderQueue.Add(B);
                                Shell.UpdateQueue.Add(B2);
                                Shell.RenderQueue.Add(B2);
                            }));
                        })
                        }
                    };
                    break;
                case "SOFIA_MYSTIC_ESSENCE_RETURN_FAILURE":
                    Script = new object[]
                    {
                        InitMysticBasic,
                        new object[] { "C|GWS:CONTINUE", "M|SOURCE|TRUE", "A|SOFIA|FADEIN", "F|MYSTIC SOFIA|DOWNCAST", "A|MYSTIC SOFIA|600=0,0,0|||255=255=255=255,30,30|", "F|SOFIA|GRINNING", "T||The look of this place never ceases to amaze you. [T:400]Whoa! [T:300]Whoever came up with this place must have had some serious artistic vision, yo." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "T||Or, perhaps probably more accurately, way too much free time on their hands to invent strange rainbow [C:PURPLE]SOFIA[C:WHITE] rooms." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|HAPPY", "A|MYSTIC SOFIA|-600=0,2000,20||||", "T|Mystic Sofia|Ah, welcome back." },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|THINKING", "T|Mystic Sofia|How are you progressing with locating a new [R]ESSENCE[C:WHITE] source for the procedure?" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|DOWNCAST", "T|Sofia|Not quite yet." },
                        new object[] { "C|GWS:impos", new VoidDel(delegate()
                        {
                            if(Sofia.KingFlag != 5)
                            {
                                if(Sofia.CrookedFlag != 0) { ActivateScriptElement("B|SOFIA_MYSTIC_FAILURE_CROOKED_RECALL"); }
                                else { ActivateScriptElement("B|SOFIA_MYSTIC_FAILURE_KING_ADVICE"); }
                            }
                            else { ActivateScriptElement("B|SOFIA_MYSTIC_FAILURE_NO_ADVICE"); }
                        })
                        }
                    };
                    break;
                case "SOFIA_MYSTIC_FAILURE_KING_ADVICE":
                    Script = new object[]
                    {
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|Ah, that is understandable." },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|DOWNCAST", "T|Mystic Sofia|It is a difficult task that has been placed upon your shoulders." },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|THINKING", "T|Mystic Sofia|In [C:PURPLE]ULTRASOFIAWORLD[C:WHITE], while all of our world is steeped in [R]ESSENCE[C:WHITE] in a diluted form..." },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|[R]ESSENCE[C:WHITE] is rarely found in its pure form outside of the [R]SOURCE[C:WHITE], and from within us, born of the essence." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "F|MYSTIC SOFIA|DOWNCAST", "T|Mystic Sofia|But attempting to extract it from one of us [C:PURPLE]SOFIAS[C:WHITE] would surely destroy she who underwent such an operation." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|DOWNCAST", "F|MYSTIC SOFIA|THINKING", "T|Mystic Sofia|That said, I have heard rumours of other ways that concentrated SOFIA ESSENCE can be acquired." },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|It would likely be worthwhile to talk through that more with the KING SOFIA." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Yes, I've been meaning to talk to her more." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|HAPPY", "T|Sofia|Thank you for your help as well, though." },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|HAPPY", "T|Mystic Sofia|It is my duty, but moreso it is my pleasure." },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|Come back to me once you have made more progress. [T:400]Remember, we must hurry!" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|I know. I'll be as speedy as I can, yo." },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|DOWNCAST", "T|Mystic Sofia|Good luck." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|EXCITED", "T|Sofia|See you soon!" },
                        new object[] { "C|GWS:impos_mapnavigate", "T||Use the [C:138-0-255-255]MAP[C:WHITE] to navigate to your next destination." }
                    };
                    break;
                case "SOFIA_MYSTIC_FAILURE_CROOKED_RECALL":
                    Script = new object[]
                    {
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|Ah, that is understandable." },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|DOWNCAST", "T|Mystic Sofia|It is a difficult task that has been placed upon your shoulders." },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|THINKING", "T|Mystic Sofia|In [C:PURPLE]ULTRASOFIAWORLD[C:WHITE], while all of our world is steeped in [R]ESSENCE[C:WHITE] in a diluted form..." },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|[R]ESSENCE[C:WHITE] is rarely found in its pure form outside of the [R]SOURCE[C:WHITE], and from within us, born of the essence." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "F|MYSTIC SOFIA|DOWNCAST", "T|Mystic Sofia|But attempting to extract it from one of us [C:PURPLE]SOFIAS[C:WHITE] would surely destroy she who underwent such an operation." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|DOWNCAST", "F|MYSTIC SOFIA|THINKING", "T|Mystic Sofia|That said, I have heard rumours of other ways that concentrated SOFIA ESSENCE can be acquired." },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|It would likely be worthwhile to talk through that more with the KING SOFIA." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Yes, I've been meaning to talk to her more." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "T|Sofia|Also, I have some ideas about where I could maybe get some more essence." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|There was someone I met earlier..." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|HAPPY", "T|Sofia|Anyway, it might be a lead." },
                        new object[] { "C|TIME:1500:ORSKIP", "F|MYSTIC SOFIA|HAPPY", "T|Mystic Sofia|That is good to hear. [T:300]No stone must be left unturned while the fate of this world hangs in the balance!" },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|WORRIED" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|DOWNCAST", "T|Sofia|I know. I understand that this is important, and I want to help." },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|HAPPY", "T|Mystic Sofia|It warms my heart to hear that that's the case." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|GRINNING", "T|Sofia|I'm all about that SERIOUS BUSINESS!" },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|Good. Hopefully you'll find something soon." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|EXCITED", "T|Sofia|I'll do my best!" },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|Wonderful! Come back to me once you have made more progress. [T:400]Remember, we must hurry!" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|I know. I'll be as speedy as I can, yo." },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|DOWNCAST", "T|Mystic Sofia|Good luck." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|EXCITED", "T|Sofia|See you soon!" },
                        new object[] { "C|GWS:impos_mapnavigate", "T||Use the [C:138-0-255-255]MAP[C:WHITE] to navigate to your next destination." }
                    };
                    break;
                case "SOFIA_MYSTIC_FAILURE_NO_ADVICE":
                    Script = new object[]
                    {
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|EXCITED", "T|Sofia|I do have leads, though!" },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|HAPPY", "F|SOFIA|HAPPY", "T|Sofia|I talked to the King Sofia and the Cool Sofia..." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|UNIMPRESSED", "T|Sofia|...God, I still can't believe those names." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "T|Sofia|Or... titles?" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|EXCITED", "T|Sofia|But anyway, they gave me an idea of where to look!" },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|GRINNING", "T|Mystic Sofia|Wonderful! You are doing well." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|HAPPY", "T|Mystic Sofia|I am not sure that I can give further advice beyond what they will likely have already told you. [R]ESSENCE[C:WHITE] is hard to come by in its pure form outside of the [R]SOURCE[C:WHITE], and, well, outside of [C:PURPLE]US[C:WHITE]." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Yeah. That's along the lines of what they told me." },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|EXCITED", "F|SOFIA|GRINNING", "T|Sofia|But I reckon we've found a solution!" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|HAPPY", "T|Sofia|The lead they gave me was an, uh, person..." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|UNIMPRESSED", "T|Sofia|Who I'm... not sure I'm gonna be able to vouch for personally..." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|GRINNING", "T|Sofia|But they could be the answer to finding some more essence!" },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|HAPPY", "T|Mystic Sofia|Any path to victory is suitable for our ends, the stakes being what they are." },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|And I can vouch for the insight of both of those two, for any faults they may have." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|HAPPY", "T|Mystic Sofia|I recommend you seek out this person at once." },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|DOWNCAST", "T|Mystic Sofia|And hurry, child. Remember that our world is at stake!" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|I know. I'll be as speedy as I can, yo." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|I now that I've checked back in I'll get right onto it." },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|HAPPY", "T|Mystic Sofia|Good. Farewell, and good luck." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|EXCITED", "T|Sofia|Thank you! [T:300]See you soon!" },
                        new object[] { "C|GWS:impos_mapnavigate", "T||Use the [C:138-0-255-255]MAP[C:WHITE] to navigate to your next destination." },
                    };
                    break;
                case "SOFIA_MYSTIC_PREFINAL_RETURN":
                    Script = new object[]
                    {
                        InitMysticBasic,
                        new object[] { "C|GWS:CONTINUE", "M|SOURCE|TRUE", "F|MYSTIC SOFIA|EXCITED", "F|SOFIA|HAPPY", "A|SOFIA|FADEIN", "T||As you re-enter the cavern, you notice that the Mystic Sofia appears to have been making preparations." },
                        new object[] { "C|GWS:CONTINUE", "T||She is sitting in the center of the cavern floor, apparently meditating. You imagine as you enter that her eyes would be closed; if she had them, that is." },
                        new object[] { "C|GWS:CONTINUE", "T||Elsewhere, you notice that in a few places, bizarre, arcane-looking symbols have been daubed onto the bald stone of the cave. Some are of a dizzying complexity and beauty, all formed in a strange, glittering substance you can't quite identify." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|EXCITED", "T||More than that, though, you can *feel* the change. There is an electric crackling in the air, as if the entire cavern is waiting with bated breath for what is about to happen." },
                        new object[] { "C|GWS:CONTINUE", "T||And hey, maybe it is? This place is weird, yo. Semi-sentient caves would be the least surprising thing you've seen today." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "T||And besides, you've always suspected that caves are at least a little bit sentient." },
                        new object[] { "C|TIME:1500:ORSKIP", "A|MYSTIC SOFIA|FADEIN", "T|Mystic Sofia|You return." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|GRINNING" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|I do!" },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|HAPPY", "T|Mystic Sofia|It is good to see you again. Feel the aura of this place? It awaits its salvation!" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|EXCITED", "T|Mystic Sofia|Are you doing well? Are you still prepared to proceed?" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Well, I'm pretty sure that I have everything we need still." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|LAUGHING", "T|Sofia|Mostly, well, me, I guess. Heh." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|HAPPY", "T|Sofia|Is there anything else?" },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|No. [T:300]Now that you are infused with the correct ESSENCES, I can being [C:PURPLE]THE PROCEDURE[C:WHITE] at any time." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|EXCITED", "T|Sofia|Rad! Almost time for action. ;D" },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|LAUGHING", "T|Mystic Sofia|Quite!" },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|HAPPY", "T|Mystic Sofia|Are you ready to get started, then?" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "T|Mystic Sofia|Remember, once we begin, we won't be able to stop until the PROCEDURE is complete. So if there is anything at all that you need to do beforehand, you will need to do it now, before we begin." },
                        new object[] { "C|GWS:impos", "T|Sofia|Hmm...", new VoidDel(delegate()
                        {
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                Button B = ButtonScripts.GetQuickButton("Start the process right now.", ButtonScripts.DelegateFetch("runscript_SOFIA_MYSTIC_FINAL"));
                                B.MLCRecord = new String[] { "runscript_SOFIA_MYSTIC_FINAL" };
                                B.QuickMoveTo(new Vector2(640, 200));
                                Button B2 = ButtonScripts.GetQuickButton("There are some other things I want to do first.", ButtonScripts.DelegateFetch("runscript_SOFIA_MYSTIC_POSTPONE"));
                                B2.MLCRecord = new String[] { "runscript_SOFIA_MYSTIC_POSTPONE" };
                                B2.QuickMoveTo(new Vector2(640, 320));
                                Shell.UpdateQueue.Add(B);
                                Shell.RenderQueue.Add(B);
                                Shell.UpdateQueue.Add(B2);
                                Shell.RenderQueue.Add(B2);
                            }));
                        })
                        }
                    };
                    break;
                case "SOFIA_MYSTIC_FINAL":
                    Script = new object[]
                    {
                        new object[] { "C|GWS:CONTINUE", "D|#CBUTTONS", "D|BUTTON_NAVSCREEN|IFPRESENT", "F|SOFIA|DOWNCAST", "T|Sofia|I think that I'm ready to start.", new VoidDel(delegate()
                        {
                            Sofia.CrookedFlag = 6;
                            Sofia.KingFlag = 6;
                            Sofia.MysticFlag = 4;
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                WorldEntity Add = new WorldEntity("NAVBUTTON_FADEOUTER", new Vector2(75, 458), (TAtlasInfo)Shell.AtlasDirectory["MAPBUTTON"], 0.96f);
                                Add.CenterOrigin = true;
                                Add.AnimationQueue.Add(Animation.Retrieve("FADEOUT"));
                                Add.TransientAnimation = true;
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                            }));
                        })
                        },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|HAPPY", "T|Sofia|Lay that SOFIA MAGIC on me." },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|EXCITED", "T|Mystic Sofia|Wonderful!" },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|GRINNING", "T|Mystic Sofia|The salvation that we have sought is nigh!" },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|EXCITED", "T|Mystic Sofia|I must prepare the room for what is to come." },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|DOWNCAST", "T|Mystic Sofia|[R]OH ESSENCE OF THE SOFIA..." },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|[R]...SOURCE OF ALL WE ARE..." },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|[R]...AND ALL THAT WE MAY BE!" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "T||An eerie feeling begins to descend upon you." , new VoidDel(delegate()
                        {
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                WorldEntity R = new WorldEntity("SOURCE_RUNES_EFFECT_TRANSIENT", new Vector2(-75, -180), (TAtlasInfo)Shell.AtlasDirectory["RUNEGLOW"], 0.055f);
                                R.AnimationQueue.Add(Animation.Retrieve("FADEINOUTLONG"));
                                R.TransientAnimation = true;
                                R.ColourValue =  new Color(0,0,0,0);
                                Shell.UpdateQueue.Add(R);
                                Shell.RenderQueue.Add(R);
                            }));
                        })
                        },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Um-" },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|JUDGING", "T|Mystic Sofia|[R]I BESEECH THEE, OH ESSENCE OF THE SOURCE OF OUR WORLD...!" },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|[R]OPEN YOURSELF TO ME, AND AS YOUR NATURE ALLOWS..." },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|[R]...ACCEPT THAT WE OFFER UNTO YOU THIS DAY!" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "T||Something seems to rumble in the distance." , new VoidDel(delegate()
                        {
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                WorldEntity R = new WorldEntity("SOURCE_RUNES_EFFECT_TRANSIENT_2", new Vector2(-75, -180), (TAtlasInfo)Shell.AtlasDirectory["RUNEGLOW"], 0.055f);
                                R.AnimationQueue.Add(Animation.Retrieve("FADEINOUTLONG"));
                                R.TransientAnimation = true;
                                R.ColourValue =  new Color(0,0,0,0);
                                Shell.UpdateQueue.Add(R);
                                Shell.RenderQueue.Add(R);
                            }));
                        })
                        },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Hey, uh, are you sure that this is safe?" },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|HAPPY", "T|Mystic Sofia|Of course it is, child." },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|I have not yet even begun the true process of transferring essence. All that I am doing now is preparing the [R]SOURCE[C:WHITE] for the offering that we will give it." },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|THINKING", "T|Mystic Sofia|Putting a little grease in the mechanism, so to speak." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|DOWNCAST", "T|Sofia|Well, okay, if you say so." },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|HAPPY", "T|Mystic Sofia|Let me continue, then." },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|DOWNCAST", "T|Mystic Sofia|[R]TODAY, WE RESTORE GLORY TO [C:PURPLE]ULTRASOFIAWORLD[R]." },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|JUDGING", "T|Mystic Sofia|[R]GLORY THAT IN OUR HUBRIS WE TOOK FROM THEE." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|(...?)" },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|[R]THIS DAY, NEW ESSENCES SHALL RUN FORTH ACROSS THESE DARKLING PLAINS..." },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|[R]...ULTRASOFIAWORLD SHALL BE RESTORED ANEW!" },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|[R]NOW AND FOR ALL TIME!" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|JUDGING", "S|DEEPBOOM", "T||" , new VoidDel(delegate()
                        {
                            foreach(WorldEntity E in Shell.RenderQueue)
                            {
                                if(ButtonScripts.DefaultUINames.Contains(E.Name) || E is TextEntity) { continue; }
                                E.AnimationQueue.Add(Animation.Retrieve("SHAKEQUAKE"));
                            }
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                Sofia.RuneGlow R = new Sofia.RuneGlow("SOURCE_RUNES_EFFECT_PERSISTENT", new Vector2(-75, -180), (TAtlasInfo)Shell.AtlasDirectory["RUNEGLOW"], 0.055f);
                                R.ColourValue =  new Color(0,0,0,0);
                                Shell.UpdateQueue.Add(R);
                                Shell.RenderQueue.Add(R);
                                WorldEntity A = new WorldEntity("BIG_SOURCE_GLOW", new Vector2(-75, -180), (TAtlasInfo)Shell.AtlasDirectory["SPLASHGLOW"], 0.079f);
                                A.ColourValue =  new Color(0,0,0,0);
                                A.AnimationQueue.Add(Animation.Retrieve("FADEINLONG"));
                                Shell.UpdateQueue.Add(A);
                                Shell.RenderQueue.Add(A);
                                WorldEntity Add = new WorldEntity("KING SOFIA", new Vector2(1580, 405), (TAtlasInfo)Shell.AtlasDirectory["KINGSOFIA"], 0.5f);
                                Add.CenterOrigin = true;
                                Add.SetAtlasFrame(new Point(0, 1));
                                Add.Scale(new Vector2(-0.06f, -0.06f));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                                Add = new WorldEntity("COOL SOFIA", new Vector2(1430, 405), (TAtlasInfo)Shell.AtlasDirectory["COOLSOFIA"], 0.46f);
                                Add.CenterOrigin = true;
                                Add.SetAtlasFrame(new Point(3, 1));
                                Add.Scale(new Vector2(-0.12f, -0.12f));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                            }));
                        })
                        },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "T|Sofia|Are you... [T:400]quite sure that this is safe?" },
                        new object[] { "C|TIME:1500:ORSKIP", "F|MYSTIC SOFIA|THINKING", "T|Cool Sofia|Well hey now babe, if it wasn't safe then I sure wouldn't be here, would I?" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|HAPPY" },
                        new object[] { "C|TIME:2000:ORSKIP", new VoidDel(delegate()
                        {
                            Animation MysticMovement = new Animation("mysticmovement");
                            SortedList A = Animation.CreateVectorTween(new Vector2(-530, 0), 800, 20);
                            MysticMovement.WriteMovement(A);
                            A = Animation.CreateVectorTween(new Vector2(-0.06f, -0.06f), 800, 20);
                            SortedList B = Animation.CreateVectorTween(new Vector2(-1.76f, 0), 20, 20);
                            A = Animation.MergeFrames(A, B);
                            MysticMovement.WriteScaling(A);
                            Animation SofiaMovement = new Animation("sofiamovement");
                            A = Animation.CreateVectorTween(new Vector2(-100, 0), 200, 20);
                            SofiaMovement.WriteMovement(A);
                            Animation CoolMovement = new Animation("coolmovement");
                            A = Animation.CreateVectorTween(new Vector2(0, 0), 800, 20);
                            B = Animation.CreateVectorTween(new Vector2(-550, 0), 800, 20);
                            A = Animation.MergeFrames(A, B);
                            CoolMovement.WriteMovement(A);
                            Animation KingMovement = new Animation("kingmovement");
                            A = Animation.CreateVectorTween(new Vector2(0, 0), 1000, 20);
                            B = Animation.CreateVectorTween(new Vector2(-550, 0), 800, 20);
                            A = Animation.MergeFrames(A, B);
                            KingMovement.WriteMovement(A);
                            Shell.GetEntityByName("MYSTIC SOFIA").AnimationQueue.Add(MysticMovement);
                            Shell.GetEntityByName("SOFIA").AnimationQueue.Add(SofiaMovement);
                            Shell.GetEntityByName("COOL SOFIA").AnimationQueue.Add(CoolMovement);
                            Shell.GetEntityByName("KING SOFIA").AnimationQueue.Add(KingMovement);
                        })
                        },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Yooo!" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|GRINNING", "T|Sofia|You made it!" },
                        new object[] { "C|GWS:CONTINUE", "F|COOL SOFIA|HAPPY", "T|Cool Sofia|Well, if I missed the finale, then that would be decidedly uncool. B)" },
                        new object[] { "C|GWS:CONTINUE", "F|COOL SOFIA|LAUGHING", "T|Cool Sofia|'specially seeing the role I played in making this all come to be." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|UNIMPRESSED", "T|Sofia|Hey now, don't give yourself too much credit for kidnapping me." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|DOWNCAST", "T|Sofia|But hey, at least you're both here, I suppose." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|GRINNING", "T|Sofia|We can all witness the awesome!" },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|JUDGING", "T|King Sofia|[F:KING]Indeed!" },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|THINKING", "T|King Sofia|[F:KING]This ESSENCE crisis has been ongoing for generations." },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|EXCITED", "T|King Sofia|[F:KING]Now that a solution is finally at hand, I least of all can afford to not be present to witness what may be the salvation of our entire kingdom!" },
                        new object[] { "C|GWS:CONTINUE", "F|COOL SOFIA|EXCITED", "F|KING SOFIA|JUDGING", "T|King Sofia|[F:KING]It would not be proper!" },
                        new object[] { "C|GWS:CONTINUE", "T|Cool Sofia|Calm down now, Sire, we're here now." },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|HAPPY", "F|COOL SOFIA|THINKING", "T|Cool Sofia|And just in time, so it seems. [T:300]Things haven't started yet, have they?" },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|HAPPY", "T|Mystic Sofia|Not quite yet, no. I have merely laid the groundwork." },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|JUDGING", "T|Mystic Sofia|The soft loam that is the [R]SOURCE[C:WHITE] has been gently tamped down into a fertile recepticle for our new friend's offering." },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|HAPPY", "F|COOL SOFIA|LAUGHING", "F|SOFIA|UNIMPRESSED", "T|Sofia|I don't like how you phrased that, but then that goes for pretty much everything you say." },
                        new object[] { "C|GWS:CONTINUE", "T|Cool Sofia|Don't I know it, darling." },
                        new object[] { "C|GWS:CONTINUE", "F|COOL SOFIA|EXCITED", "T|Cool Sofia|And hey, don't look so glum! [T:400]Remember, you can go home after this. Never have to see us again." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|HAPPY", "T|Sofia|Huh! [T:400]I guess you're right!" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "T|Sofia|Somehow, I think I actually forgot about that." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|...huh." },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|GRINNING", "T|King Sofia|[F:KING]Not to interrupt, but - well, I am the King." },
                        new object[] { "C|GWS:CONTINUE", "T|King Sofia|[F:KING]But, furthermore-! I believe that we have business to attend to!" },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|THINKING", "T|Mystic Sofia|Yes, I think perhaps that it is best that we continue. [T:200]The [R]ESSENCE[C:WHITE] flow seems turbulent today." },
                        new object[] { "C|GWS:CONTINUE", "F|COOL SOFIA|HAPPY", "T|Cool Sofia|Yeah, uh, what was that big bang back there?" },
                        new object[] { "C|GWS:CONTINUE", "F|COOL SOFIA|WORRIED", "F|SOFIA|WORRIED", "T|Mystic Sofia|Best not to worry about it. [T:300]Shall we continue?" },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|EXCITED", "T|King Sofia|[F:KING]If all is prepared, I say we proceed ahead!" },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|GRINNING", "T|Mystic Sofia|Excellent!" },
                        new object[] { "C|GWS:CONTINUE", "M|#NULL", "T|Mystic Sofia|After all...", "A|MYSTIC SOFIA|150=0,500,20||||" },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|WORRIED","F|MYSTIC SOFIA|JUDGING", "T|Mystic Sofia|[R,F:MACABRE]WE HAVE DELAYED FOR LONG ENOUGH!", new VoidDel(delegate()
                        {
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                WorldEntity Add = new Sofia.EssenseGlow("ESSENCEGLOW", new Vector2(0, 0), (TAtlasInfo)Shell.AtlasDirectory["ESSENCEGLOW"], 0.49f, "MYSTIC SOFIA");
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                            }));
                        })
                        },
                        new object[] { "C|GWS:CONTINUE", "M|DEEP1|TRUE", "T|Mystic Sofia|[R,F:MACABRE]POWERS THAT BE." },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|[R,F:MACABRE]THAT WHICH WAS SUNDERED SHALL BE MADE ANEW." },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|[R,F:MACABRE]LIGHT OF THE SOURCE COME FORTH, SOURCE OF OUR ESSENCE." },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|[R,F:MACABRE]ONE HAS BEEN BROUGHT BEFORE THEE..." },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|[R,F:MACABRE]THROUGH WHICH THOU SHALT BE NEWLY REMADE." },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|[R,F:MACABRE]BEHOLD!" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Um... I..." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|JUDGING", "T|Sofia|Whooooaoao[R]AAAAAAAAAAAAAAAAA!" },
                        new object[] { "C|GWS:CONTINUE", "M|DEEP2|TRUE", "A|SOFIA|FLOATER|LOOP", "A|SOFIA|0=50,1000,20||||", "S|DEEPBOOM", "F|KING SOFIA|THINKING", "T|Sofia|[R]...", new VoidDel(delegate()
                        {
                            foreach(WorldEntity E in Shell.RenderQueue)
                            {
                                if(ButtonScripts.DefaultUINames.Contains(E.Name) || E is TextEntity) { continue; }
                                E.AnimationQueue.Add(Animation.Retrieve("SHAKEQUAKE"));
                            }
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                WorldEntity Add = new Sofia.EssenseGlow("ESSENCEGLOW_2", new Vector2(0, 0), (TAtlasInfo)Shell.AtlasDirectory["ESSENCEGLOW"], 0.51f, "SOFIA");
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                            }));
                        })
                        },
                        new object[] { "C|GWS:CONTINUE", "T|Cool Sofia|Hey, are you sure that this is safe? [T:300]This seems a little intense." },
                        new object[] { "C|GWS:CONTINUE", "T|Cool Sofia|What's all that noise, yo? Is this meant to happen?" },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|[R,F:MACABRE]The time is over for such menial concerns." },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|[R,F:MACABRE]SHE IS PRIMED." },
                        new object[] { "C|GWS:CONTINUE", "T|King Sofia|[F:KING]Um, yes, I can see..." },
                        new object[] { "C|GWS:CONTINUE", "T|King Sofia|[F:KING]But is this really... [T:400,F:KING]So to speak..." },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|CONSIDERING", "T|King Sofia|[F:KING]...Standard operating procedure?" },
                        new object[] { "C|GWS:CONTINUE", "T|Cool Sofia|Dude, I don't think anyone's ever done this before. [T:300]Least of all us." },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|THINKING", "T|King Sofia|[F:KING]Hmm." },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|WORRIED", "T|King Sofia|[F:KING]Well, yes, of course..." },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|[R,F:MACABRE]Enough of this." },
                        new object[] { "C|TIME:1200:ORSKIP", "T|King Sofia|[F:KING]Well, hold on now. [T:300,F:KING]I am the K-" },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|JUDGING", "T|Mystic Sofia|[R,F:MACABRE]ENOUGH OF THIS!" },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|UNIMPRESSED", "T|Mystic Sofia|[R,F:MACABRE]THE POWER IS FLOWING. I CAN FEEL THE ESSENCE WITHIN ME." },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|[R,F:MACABRE]IT IS WITHIN US. IT IS WITHIN..." },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|[R,F:MACABRE]HER." },
                        new object[] { "C|GWS:CONTINUE", "M|DEEP3|TRUE", "S|DEEPBOOM", "T|Mystic Sofia|[R,F:MACABRE]ESSENCE OF THE SOFIA, COME FORTH TO ME!", new VoidDel(delegate()
                        {
                            foreach(WorldEntity E in Shell.RenderQueue)
                            {
                                if(ButtonScripts.DefaultUINames.Contains(E.Name) || E is TextEntity) { continue; }
                                E.AnimationQueue.Add(Animation.Retrieve("SHAKEQUAKE"));
                            }
                        })
                        },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|WORRIED", "T|King Sofia|[F:KING]My word-!" },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|[R,F:MACABRE]By the power within me..." },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|[R,F:MACABRE]Her ESSENCE shall be DRAWN FORTH." },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|[R,F:MACABRE]I CALL IT NOW... I CALL FORTH TO THE ESSENCE OF..." },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|JUDGING", "F|COOL SOFIA|JUDGING", "T|Mystic Sofia|[R,F:MACABRE]SOFIA!", "S|DEEPBOOM", new VoidDel(delegate()
                        {
                            foreach(WorldEntity E in Shell.RenderQueue)
                            {
                                if(ButtonScripts.DefaultUINames.Contains(E.Name) || E is TextEntity) { continue; }
                                E.AnimationQueue.Add(Animation.Retrieve("SHAKEQUAKE"));
                            }
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                WorldEntity Add = new Sofia.SofiaBoomer("BOOMER_OBJECT", 3, 500);
                                Shell.UpdateQueue.Add(Add);
                            }));
                        })
                        },
                        new object[] { "C|GWS:CONTINUE", "F|COOL SOFIA|WORRIED", "T|Cool Sofia|Dude, what the shit is happening???" },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|WORRIED", "T|Mystic Sofia|[R,F:MACABRE]NOW LET THIS POWER... [T:500,R,F:MACABRE]TO THE SOURCE BE INSTILLED." },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|[R,F:MACABRE]TO THE SOURCE... [T:300,R,F:MACABRE]BE INSTILLED.", "S|DEEPBOOM", new VoidDel(delegate()
                        {
                            foreach(WorldEntity E in Shell.RenderQueue)
                            {
                                if(ButtonScripts.DefaultUINames.Contains(E.Name) || E is TextEntity) { continue; }
                                E.AnimationQueue.Add(Animation.Retrieve("SHAKEQUAKE"));
                            }
                        })
                        },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|[R,F:MACABRE]TO THE SOURCE... [T:300,R,F:MACABRE]BE INSTILLED!", "S|DEEPBOOM", new VoidDel(delegate()
                        {
                            foreach(WorldEntity E in Shell.RenderQueue)
                            {
                                if(ButtonScripts.DefaultUINames.Contains(E.Name) || E is TextEntity) { continue; }
                                E.AnimationQueue.Add(Animation.Retrieve("SHAKEQUAKE"));
                            }
                        })
                        },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|[R,F:MACABRE]TO THE SOURCE BE INSTILLED!", "F|KING SOFIA|JUDGING", "F|COOL SOFIA|JUDGING", "M|#NULL", "S|DEEPBOOM", new VoidDel(delegate()
                        {
                            WorldEntity OurSofia = Shell.GetEntityByName("SOFIA");
                            OurSofia.AnimationQueue.Clear();
                            Animation FreshTween = new Animation("sofia_return_tween");
                            SortedList TweenFrames = Animation.CreateVectorTween(new Vector2(0, 405 - OurSofia.DrawCoords.Y), 1000, 20);
                            FreshTween.WriteMovement(TweenFrames);
                            OurSofia.AnimationQueue.Add(FreshTween);
                            foreach(WorldEntity E in Shell.RenderQueue)
                            {
                                if(ButtonScripts.DefaultUINames.Contains(E.Name) || E is TextEntity) { continue; }
                                E.AnimationQueue.Add(Animation.Retrieve("SHAKEQUAKE"));
                            }
                            Sofia.EssenseGlow EG = (Sofia.EssenseGlow)Shell.GetEntityByName("ESSENCEGLOW_2");
                            EG.AnimationQueue.Add(Animation.Retrieve("fadeoutcolourpreserve"));
                            EG.TransientAnimation = true;
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                WorldEntity Add = new Sofia.SofiaBoomer("BOOMER_OBJECT", 10, 500);
                                Shell.UpdateQueue.Add(Add);
                            }));
                        })
                        },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|DOWNCAST", "T|Sofia|..." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Did..." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|...did it work?" },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|UNIMPRESSED", "F|COOL SOFIA|UNIMPRESSED", "T|Mystic Sofia|[R]...", new VoidDel(delegate()
                        {
                            Sofia.EssenseGlow EG = (Sofia.EssenseGlow)Shell.GetEntityByName("ESSENCEGLOW");
                            EG.AnimationQueue.Add(Animation.Retrieve("fadeoutcolourpreserve"));
                            EG.TransientAnimation = true;
                        })
                        },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|GRINNING", "T|Mystic Sofia|...well, I believe-!" },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|WORRIED", "T|Mystic Sofia|..." },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|Wait." },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|Something's wrong." },
                        new object[] { "C|GWS:CONTINUE", "S|DEEPBOOM", "M|GODHEAD|TRUE", new VoidDel(delegate()
                        {
                            Animation A = Animation.Retrieve("LASTINGQUAKE");
                            A.Loop = true;
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                WorldEntity Add = new Sofia.BigSofia("BIG SOFIA", new Vector2(600, 300), (TAtlasInfo)Shell.AtlasDirectory["BIGSOFIA"], 0.4f, new ArrayList());
                                Add.CenterOrigin = true;
                                Add.ColourValue = new Color(0,0,0,0);
                                Animation B = new Animation("bigsofiafadein");
                                B.WriteColouring(Animation.CreateColourTween(new ColourShift(50f,50f,50f,50f), 500, 20));
                                Add.AnimationQueue.Add(B);
                                Add.AnimationQueue.Add(A.Clone());
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                            }));
                            foreach(WorldEntity E in Shell.RenderQueue)
                            {
                                if(ButtonScripts.DefaultUINames.Contains(E.Name) || E is TextEntity) { continue; }
                                E.AnimationQueue.Add(A.Clone());
                            }
                        })
                        },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "T|Sofia|What? What's happening?" },
                        new object[] { "C|GWS:CONTINUE", "F|COOL SOFIA|DOWNCAST", "T|Cool Sofia|What the hell did you do?" },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|The essence field feels unstable. [T:300]I- I've never..." },
                        new object[] { "C|GWS:CONTINUE", "T|Cool Sofia|Well perhaps now's the time to shut off the flow then!" },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|DOWNCAST", "T|Mystic Sofia|I did- I- It's not-" },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|WORRIED", "T|Mystic Sofia|It's not- It's not shutting down!" },
                        new object[] { "C|GWS:CONTINUE", "A|BIG SOFIA||||50=50=50=50,500,20|", "T|Mystic Sofia|It's getting stronger!" },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|CONSIDERING", "T|King Sofia|[F:KING]Bloody hell, what is *that*?" },
                        new object[] { "C|GWS:CONTINUE", "T|Cool Sofia|It's a damn earthquake, obviously!" },
                        new object[] { "C|GWS:CONTINUE", "T|King Sofia|[F:KING]No, no, not that- THAT." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|I see it too! [T:300]Something's coming!" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Over there by the source- behind you!" },
                        new object[] { "C|TIME:1000:ORSKIP", "A|BIG SOFIA|50=0,1000,20|||50=50=50=50,500,20|", "A|SOFIA|-100=0,500,20||||", "A|MYSTIC SOFIA|-250=0,500,20||||", "A|KING SOFIA|100=0,500,20||||", "A|COOL SOFIA|100=0,500,20||||", "F|MYSTIC SOFIA|JUDGING", "F|KING SOFIA|JUDGING", "F|COOL SOFIA|JUDGING", "F|SOFIA|JUDGING", new VoidDel(delegate()
                        {
                            ((Sofia.BigSofia)Shell.GetEntityByName("BIG SOFIA")).States.Add("FLOATING");
                        })
                        },
                        new object[] { "C|GWS:CONTINUE", "T|Cool Sofia|What the hell?" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|What is it!?" },
                        new object[] { "C|GWS:CONTINUE", "T|Cool Sofia|I don't know!" },
                        new object[] { "C|GWS:CONTINUE", "T|Cool Sofia|Mystic Sofia???" },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|WORRIED", "T|Mystic Sofia|I- I've never seen-" },
                        new object[] { "C|TIME:1000:ORSKIP", "T|Mystic Sofia|This is unprecedented! The [R]ESSENCE[C:WHITE] I sense is phenomenal-" },
                        new object[] { "C|GWS:CONTINUE", "A|BIG SOFIA||||50=50=50=50,500,20|", "F|MYSTIC SOFIA|JUDGING", "T|Mystic Sofia|Ah!", "S|DEEPBOOM", new VoidDel(delegate()
                        {
                            foreach(WorldEntity E in Shell.RenderQueue)
                            {
                                if(ButtonScripts.DefaultUINames.Contains(E.Name) || E is TextEntity) { continue; }
                                E.AnimationQueue.Add(Animation.Retrieve("SHAKEQUAKE"));
                            }
                        })
                        },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|WORRIED", "T|Mystic Sofia|I think tapping into the essence stream is strengthening it!" },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|UNIMPRESSED", "T|King Sofia|[F:KING]Bloody hell, Lady, don't do it then!" },
                        new object[] { "C|GWS:CONTINUE", "T|King Sofia|[F:KING]I thought this was your... [T:300,F:KING]so to speak..." },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|JUDGING", "T|King Sofia|[F:KING]Domain of wisdom!" },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|WORRIED", "T|King Sofia|[F:KING]Damn that thing is... Big." },
                        new object[] { "C|GWS:CONTINUE", "A|BIG SOFIA||||55=55=55=55,500,20|", "F|SOFIA|WORRIED", "T|Sofia|I think it's getting stronger! [T:300]Look at it!" },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|My- My word!" },
                        new object[] { "C|GWS:CONTINUE", "F|COOL SOFIA|JUDGING", "T|Cool Sofia|It... it looks..." },
                        new object[] { "C|GWS:CONTINUE", "T|Cool Sofia|It almost looks like-! [T:500]Like-!!!" },
                        new object[] { "C|TIME:0:ORSKIP", "F|BIG SOFIA|0|0", "M|#NULL||INSTANT", "F|KING SOFIA|JUDGING", "F|COOL SOFIA|JUDGING", "F|SOFIA|JUDGING", "F|MYSTIC SOFIA|JUDGING", new VoidDel(delegate()
                        {
                            Animation.GlobalEndLoops();
                            foreach(WorldEntity E in Shell.RenderQueue)
                            {
                                foreach(Animation A in E.AnimationQueue)
                                {
                                    if(A.AnimName == "lastingquake" || A.AnimName == "shakequake") { A.Jump(E); }
                                }
                            }
                        })
                        },
                        new object[] { "C|GWS:CONTINUE", "T||" },
                        new object[] { "C|GWS:CONTINUE", "T|Big Sofia|..." },
                        new object[] { "C|GWS:CONTINUE", "S|SOFIA_I", "T|Big Sofia|I..." },
                        new object[] { "C|GWS:CONTINUE", "S|SOFIA_AM", "T|Big Sofia|Am..." },
                        new object[] { "C|GWS:CONTINUE", "S|SOFIA_SOFIA", "T|Big Sofia|Sofia." },
                        new object[] { "C|GWS:CONTINUE", "S|SOFIA_HOH", "M|BATTLE|TRUE", "T|Big Sofia|[R]HOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOH!", "S|DEEPBOOM", new VoidDel(delegate()
                        {
                            ArrayList NewStates = new ArrayList(new String[] { "FLOATING", "GLOW", "SHIFTER", "SPEW1" });
                            Sofia.BigSofia B = ((Sofia.BigSofia)Shell.GetEntityByName("BIG SOFIA"));
                            B.States = NewStates;
                            B.SetAtlasFrame(new Point());
                            Animation A = Animation.Retrieve("LASTINGQUAKE");
                            A.Loop = true;
                            foreach(WorldEntity E in Shell.RenderQueue)
                            {
                                if(ButtonScripts.DefaultUINames.Contains(E.Name) || E is TextEntity || E is Sofia.BigSofia) { continue; }
                                E.AnimationQueue.Add(A.Clone());
                            }
                        })
                        },
                        new object[] { "C|GWS:CONTINUE", "T|King Sofia|[F:KING]By the nine royal oaths!" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Aaaaaah!!!" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Another- why is it always another me!" },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|It's an [C:PURPLE]avatar of the true source[C:WHITE]! I can't contain it!" },
                        new object[] { "C|GWS:CONTINUE", "T|King Sofia|[F:KING]We- we need to get out!" },
                        new object[] { "C|GWS:CONTINUE", "T|King Sofia|[F:KING]Heaven's above, but there's no time!" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|This whole place is coming apart!" },
                        new object[] { "C|GWS:CONTINUE", "S|SOFIA_HOH", "T|Big Sofia|[R]HOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOH!", "S|DEEPBOOM", new VoidDel(delegate()
                        {
                            Sofia.BigSofia B = ((Sofia.BigSofia)Shell.GetEntityByName("BIG SOFIA"));
                            B.States.Remove("SPEW1");
                            B.States.Add("SPEW2");
                            foreach(WorldEntity E in Shell.RenderQueue)
                            {
                                if(ButtonScripts.DefaultUINames.Contains(E.Name) || E is TextEntity) { continue; }
                                E.AnimationQueue.Add(Animation.Retrieve("SHAKEQUAKE"));
                            }
                        })
                        },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|It's power! We've given it too much power-!" },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|WORRIED", "T|Mystic Sofia|We- we-" },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|DOWNCAST", "T|Mystic Sofia|It's come for us." },
                        new object[] { "C|GWS:CONTINUE", "F|MYSTIC SOFIA|WORRIED", "F|COOL SOFIA|WORRIED", "T|Cool Sofia|Why is this happening? [T:300]What does it want?" },
                        new object[] { "C|GWS:CONTINUE", "T|Cool Sofia|Mystic Sofia, what did you DO?!" },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|I- I'm...!" },
                        new object[] { "C|TIME:3000:ORSKIP", "M|#NULL", "F|MYSTIC SOFIA|DOWNCAST", "S|#CLOSEALL", "T|Mystic Sofia|I'm sorry.", new VoidDel(delegate()
                        {
                            Animation.GlobalEndLoops();
                            foreach(WorldEntity E in Shell.RenderQueue)
                            {
                                foreach(Animation A in E.AnimationQueue)
                                {
                                    if(A.AnimName == "lastingquake" || A.AnimName == "shakequake") { A.Jump(E); }
                                }
                            }
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                WorldEntity Add = new WorldEntity("WHITE-SHEET", new Vector2(0, 0), (TAtlasInfo)Shell.AtlasDirectory["BGFLASHER"], 0.89f);
                                Add.ColourValue = new Color(0,0,0,0);
                                Add.AnimationQueue.Add(Animation.Retrieve("FADEINLONG"));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                            }));
                        })
                        },
                        new object[] { "C|GWS:CONTINUE", "M|QUINTESSENCE|TRUE", "T|Mystic Sofia|It was because of us." },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|The doom of our world... [T:400]It was of our own making." },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|Long ago, we of [C:PURPLE]ULTRASOFIAWORLD[C:WHITE] lived in true harmony with the power of [C:PURPLE]SOFIA ESSENCE[C:WHITE]." },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|But, in our greed, we pulled [C:PURPLE]ESSENCE[C:WHITE] from the [C:PURPLE]SOURCE[C:WHITE] for our own selfish needs." },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|It was this that threw our world out of balance and caused the decline of [C:PURPLE]ULTRASOFIAWORLD[C:WHITE]." },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|And it was I... [T:300]I, who should be our highest sage..." },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|I who allowed this to occur." },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|Through the [C:PURPLE]ESSENCE[C:WHITE] of a [C:PURPLE]PURE SOFIA[C:WHITE] I thought we could restore balance to the world." },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|But now, its glory restored, [C:PURPLE]ULTRASOFIAWORLD[C:WHITE] seeks to take revenge on those beings that harmed her at her very [C:PURPLE]HEART[C:WHITE]." },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|Her avatar will spew doom upon this world, from the very jaws of our victory." },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|I am sorry." },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|Sofia, who came from another world..." },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|A pure Sofia among us [C:PURPLE]SHADES[C:WHITE]..." },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|You are the only one who is blameless in our folly, child." },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|Can you forgive us?" },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|Might you still be our only hope? [T:300]Your essence still remains pure." },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|This world... [T:300]is yours now." },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|Can you... Save Our Flesh-In-Animation?" },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|That's what SOFIA stands for, by the way." },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|..." },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|Okay, no it doesn't. [T:500]That was a stretch. [T:500]I'm sorry." },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|Anyway, goodbye, now." },
                        new object[] { "C|GWS:CONTINUE", "T|Mystic Sofia|We shall see where the cards will fall.", new VoidDel(delegate()
                        {
                            Animation A = Animation.Retrieve("LASTINGQUAKE");
                            A.Loop = true;
                            foreach(WorldEntity E in Shell.RenderQueue)
                            {
                                if(ButtonScripts.DefaultUINames.Contains(E.Name) || E is TextEntity || E is Sofia.BigSofia || E.Name == "WHITE-SHEET") { continue; }
                                E.AnimationQueue.Add(A.Clone());
                            }
                        })
                        },
                        new object[] { "C|TIME:3000:ORSKIP", "M|#NULL", "A|WHITE-SHEET|FADEOUTLONG", "F|MYSTIC SOFIA|DOWNCAST", "F|SOFIA|UNIMPRESSED", "F|COOL SOFIA|JUDGING", "F|KING SOFIA|JUDGING", "T||" },
                        new object[] { "C|GWS:CONTINUE", "T|Cool Sofia|!!!" },
                        new object[] { "C|GWS:CONTINUE", "T|Cool Sofia|GET DOWN!!!" },
                        new object[] { "C|TIME:3000", "M|EPILOGUE|FALSE|INSTANT", "A|WHITE-SHEET|FADEINLONG", "F|MYSTIC SOFIA|JUDGING", "F|SOFIA|JUDGING", "T|Sofia|...!", "S|DEEPBOOM", new VoidDel(delegate()
                        {
                            foreach(WorldEntity E in Shell.RenderQueue)
                            {
                                if(ButtonScripts.DefaultUINames.Contains(E.Name) || E is TextEntity || E.Name == "WHITE-SHEET") { continue; }
                                E.AnimationQueue.Add(Animation.Retrieve("SHAKEQUAKE"));
                            }
                        })
                        },
                        new object[] { "D|SOFIA", "D|MYSTIC SOFIA", "D|KING SOFIA", "D|COOL SOFIA", "D|BIG SOFIA", "D|SOURCE_RUNES_EFFECT_PERSISTENT", "D|BIG_SOURCE_GLOW", "D|SOURCE", "D|SOURCECAVEBG", "B|SOFIA_EPILOGUE_SCENES" }
                    };
                    break;
                case "SOFIA_KING_SECONDARY":
                    Script = new object[]
                    {
                        new object[] { "C|GWS:CONTINUE", "T||So, you guess you need to save the world or something?" , new VoidDel(delegate()
                        {
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                WorldEntity Add = new WorldEntity("CASTLEEXTBG", new Vector2(0, 0), (TAtlasInfo)Shell.AtlasDirectory["CASTLEEXTBG"], 0.05f);
                                Add.ColourValue = new Color(0,0,0,0);
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                                Add = new WorldEntity("SOFIA", new Vector2(350, 405), (TAtlasInfo)Shell.AtlasDirectory["SOFIASPRITES"], 0.5f);
                                Add.ColourValue = new Color(0,0,0,0);
                                Add.AnimationQueue.Add(Animation.Retrieve("FADEIN"));
                                Add.CenterOrigin = true;
                                Add.SetAtlasFrame(new Point(0, 2));
                                Add.Scale(new Vector2(-0.06f, -0.06f));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                                Add = new WorldEntity("GOLEM SOFIA", new Vector2(1430, 405), (TAtlasInfo)Shell.AtlasDirectory["GOLEMSOFIA"], 0.5f);
                                Add.CenterOrigin = true;
                                Add.SetAtlasFrame(new Point(1, 0));
                                Add.Scale(new Vector2(-0.06f, -0.06f));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                            }));
                        })
                        },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|UNIMPRESSED", "T||Sorry, you meant that you need to save ULTRASOFIAWORLD." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "T||Which is still something that exists for some reason." },
                        new object[] { "C|GWS:CONTINUE", "T||Either way, you probably shouldn't have ditched that Cool Sofia person." },
                        new object[] { "C|GWS:CONTINUE", "T||Thankfully, this castle place is marked on the map. She's probably there, and even if she's not, then this King person should be, right?" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|DOWNCAST", "T||It's a longer walk than you expected, but eventually you find yourself making progress. Finally, a ragged castle starts to materialize in the distance." },
                        new object[] { "C|GWS:CONTINUE", "M|DARKLING|TRUE", "A|CASTLEEXTBG|FADEINLONG", "T|Sofia|This must be the place..." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|HAPPY", "T|Sofia|It certainly looks appropriately, um. Castle-like." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "T|Sofia|I wonder how I get in?" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Do I just-" },
                        new object[] { "C|GWS:CONTINUE", "A|GOLEM SOFIA|-500=0,500,20||||", "F|SOFIA|UNIMPRESSED", "T|Golem Sofia|HALT!!!" },
                        new object[] { "C|GWS:CONTINUE", "T|Golem Sofia|WHO APPROACHES THE DOMAIN OF THE KING SOFIA?" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|You again!" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|I already told you that I'm allowed to be here!" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "T|Sofia|Or, well, I mean, I guess that Cool Sofia did." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|UNIMPRESSED", "T|Sofia|But still!" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Cool Sofia said I needed to come here and see the King! You know her, right?" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|DOWNCAST", "T|Sofia|I should see her too, if she's here." },
                        new object[] { "C|GWS:CONTINUE", "T|Golem Sofia|I, UH." },
                        new object[] { "C|GWS:CONTINUE", "F|GOLEM SOFIA|PLACID", "T|Golem Sofia|I HAVE ABSOLUTELY NO IDEA WHAT YOU'RE TALKING ABOUT." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "T|Golem Sofia|I'VE NEVER SEEN YOU BEFORE." },
                        new object[] { "C|GWS:CONTINUE", "F|GOLEM SOFIA|STARE", "T|Golem Sofia|WHO EVEN ARE YOU?!" },
                        new object[] { "C|GWS:CONTINUE", "T|Golem Sofia|STATE YOUR SECTOR AND DESIGNATION." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Seriously?" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|I only met you like an hour or two ago. I can't be that hard to remember!" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|CONSIDERING", "T|Sofia|Even if everyone here does look exactly like me..." },
                        new object[] { "C|GWS:CONTINUE", "F|GOLEM SOFIA|PLACID", "T|Golem Sofia|HMM." },
                        new object[] { "C|GWS:CONTINUE", "T|Golem Sofia|I THINK I UNDERSTAND." },
                        new object[] { "C|GWS:CONTINUE", "T|Golem Sofia|YOU MET ONE OF MY COUNTERPARTS." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "T|Sofia|One of your... counterparts?" },
                        new object[] { "C|GWS:CONTINUE", "T|Golem Sofia|YES. [T:300]THERE ARE MANY DIFFERENT GOLEM SOFIAS THAT EXIST TO SERVE THE NEEDS OF THIS WORLD." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "T|Golem Sofia|WE ALL LOOK ALIKE, BUT FILL DIFFERENT ROLES. MINE IS TO GUARD THIS CASTLE." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Huh, I see..." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "T|Sofia|...sorry, I guess? For getting you confused?" },
                        new object[] { "C|GWS:CONTINUE", "T|Golem Sofia|..." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "T|Sofia|Sooooo..." },
                        new object[] { "C|GWS:CONTINUE", "F|GOLEM SOFIA|STARE", "F|SOFIA|GRINNING", "T|Sofia|...can I go in?" },
                        new object[] { "C|TIME:1100:ORSKIP", "T|Golem Sofia|WITHOUT DESIGNATION? [T:500]NO." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|DOWNCAST" },
                        new object[] { "C|GWS:CONTINUE", "T|Golem Sofia|I STILL DON'T EVEN KNOW WHO YOU ARE." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "T|Sofia|Oh, come on now! [T:500]This is important!" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|The, uh, Cool Sofia told me to come here! She said it couldn't wait!" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|UNIMPRESSED", "T|Sofia|(I mean yes I did ditch her immediately afterwards but I REGRET THAT NOW)" },
                        new object[] { "C|GWS:CONTINUE", "F|GOLEM SOFIA|PLACID", "F|SOFIA|WORRIED", "T|Sofia|Is Cool Sofia here? Maybe she could vouch for me!" },
                        new object[] { "C|TIME:2000:ORSKIP", "T|Golem Sofia|I WILL.[T:300].[T:300]. [T:300]GO AND SEE." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|GRINNING" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Thank you!!" },
                        new object[] { "C|GWS:CONTINUE", "T|Golem Sofia|ONE MOMENT.", "A|GOLEM SOFIA|700=0,1500,20||||" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|DOWNCAST", "T|Sofia|." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|..." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "T|Sofia|Uh..." },
                        new object[] { "C|GWS:CONTINUE", "A|GOLEM SOFIA|-700=0,1000,20||||", "F|SOFIA|EXCITED", "T|Golem Sofia|YOU MAY ENTER.", new VoidDel(delegate() { Sofia.KingFlag = 1; }) },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Great! [T:300]Wonderful! [T:300]Finally!" },
                        new object[] { "C|GWS:CONTINUE", "A|SOFIA|1500=0,1500,20||||", "T|Golem Sofia|Catch you later robo-broski!" },
                        new object[] { "C|TIME:1500:ORSKIP", "A|GOLEM SOFIA|FADEOUT", "A|CASTLEEXTBG|FADEOUT", "T||" },
                        new object[] { "D|CASTLEEXTBG", "D|GOLEM SOFIA", "D|SOFIA", "B|SOFIA_KING_SECONDARY_INTERIOR" }
                        };
                    break;
                case "SOFIA_KING_SECONDARY_INTERIOR":
                    Script = new object[]
                    {
                        new object[] { "C|GWS:CONTINUE", "M|KING|TRUE", "T||You enter the castle. The interior is opulent, but subdued. A strange, out of place feeling comes over you.", new VoidDel(delegate()
                        {
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                WorldEntity Add = new WorldEntity("CASTLEINTBG", new Vector2(0, 0), (TAtlasInfo)Shell.AtlasDirectory["CASTLEINTBG"], 0.05f);
                                Add.ColourValue = new Color(0,0,0,0);
                                Add.AnimationQueue.Add(Animation.Retrieve("FADEIN"));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                                Add = new WorldEntity("SOFIA", new Vector2(350, 405), (TAtlasInfo)Shell.AtlasDirectory["SOFIASPRITES"], 0.5f);
                                Add.ColourValue = new Color(0,0,0,0);
                                Add.AnimationQueue.Add(Animation.Retrieve("FADEIN"));
                                Add.CenterOrigin = true;
                                Add.SetAtlasFrame(new Point(1, 2));
                                Add.Scale(new Vector2(-0.06f, -0.06f));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                            }));
                        })
                        },
                        new object[] { "C|GWS:CONTINUE", "T||You hear sounds of running water. Is it raining outside?" },
                        new object[] { "C|GWS:CONTINUE", "T||You're so busy looking around at the room and its arching windows that you almost don't spot the two people standing by the wall until they turn to face you." },
                        new object[] { "C|GWS:CONTINUE", "T||One of them is the Cool Sofia. The other, well...", new VoidDel(delegate()
                        {
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                WorldEntity Add = new WorldEntity("KING SOFIA", new Vector2(1530, 405), (TAtlasInfo)Shell.AtlasDirectory["KINGSOFIA"], 0.5f);
                                Add.CenterOrigin = true;
                                Add.SetAtlasFrame(new Point(1, 1));
                                Add.Scale(new Vector2(-0.06f, -0.06f));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                                Add = new WorldEntity("COOL SOFIA", new Vector2(1730, 405), (TAtlasInfo)Shell.AtlasDirectory["COOLSOFIA"], 0.46f);
                                Add.CenterOrigin = true;
                                Add.SetAtlasFrame(new Point(1, 0));
                                Add.Scale(new Vector2(-0.12f, -0.12f));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                            }));
                        })
                        },
                        new object[] { "C|GWS:CONTINUE", "A|KING SOFIA|-600=0,1100,20||||", "A|COOL SOFIA|-1010=0,1700,20||||", "F|SOFIA|UNIMPRESSED", "T|King Sofia|[F:KING]Who goes there! Who enters the domain of the King Sofia???" },
                        new object[] { "C|GWS:CONTINUE", "T|Cool Sofia|Ah. Here she is again. This is the one I told you about, Sire." },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|CONSIDERING", "T|King Sofia|[F:KING]Aha! Is it indeed?" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "T|Sofia|Yeah. Hi." },
                        new object[] { "C|GWS:CONTINUE", "T|Cool Sofia|Hi yourself, broski." },
                        new object[] { "C|GWS:CONTINUE", "T|Cool Sofia|Where did you go?" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Well, for one, I went and found this cave, and-" },
                        new object[] { "C|GWS:CONTINUE", "T|Cool Sofia|Actually, I don't really care. But just tell me, yo." },
                        new object[] { "C|GWS:CONTINUE", "F|COOL SOFIA|UNIMPRESSED", "T|Cool Sofia|Are you about to run off on me again?" },
                        new object[] { "C|GWS:CONTINUE", "T|King Sofia|[F:KING]Ho-hum!" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Hey- hey now, come on!" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|You kidnap me, yank me into your weird Sofia dimension..." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|You can't seriously expect me to just have gone along with everything you said!" },
                        new object[] { "C|GWS:CONTINUE", "T|Cool Sofia|Worlds were at stake, babe." },
                        new object[] { "C|TIME:600:ORSKIP", "F|COOL SOFIA|HAPPY", "T|Cool Sofia|Just next time, how about you give me some warning before you wander off into the badlands to leave our world to die?" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|UNIMPRESSED" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Now wait just one moment, you stuck up asswipe of a wannabe me-" },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|JUDGING", "F|SOFIA|WORRIED", "F|COOL SOFIA|DOWNCAST", "T|King Sofia|[F:KING]ORDER! ORDER IN MY COURT!" },
                        new object[] { "C|GWS:CONTINUE", "T|King Sofia|[F:KING]Perhaps if you would let me get a word in edgeways-!" },
                        new object[] { "C|GWS:CONTINUE", "T|Cool Sofia|As you wish, Sire." },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|UNIMPRESSED", "T|King Sofia|[F:KING]What did I just say?" },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|THINKING", "T|King Sofia|[F:KING]Regardless. Let us calm down a moment. I think this little hullabaloo can become water under the bridge." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|DOWNCAST", "T|King Sofia|[F:KING]She's here *now*, is she not? [T:300]No harm is done, and we can continue despite the delay.", new VoidDel(delegate() { Sofia.KingFlag = 2; }) },
                        new object[] { "C|GWS:CONTINUE", "T|King Sofia|[F:KING]The Cool Sofia and I were just discussing preparations for THE PROCEDURE, my young friend." },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|LAUGHING", "T|King Sofia|[F:KING]Hah! What am I saying, we're all the same age by definition!" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "F|KING SOFIA|HAPPY", "T|Sofia|Hold up, what procedure?" },
                        new object[] { "C|GWS:CONTINUE", "F|COOL SOFIA|GRINNING", "T|Sofia|That sounds highly ominous and I am very afraid all of a sudden." },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|CONSIDERING", "T|King Sofia|[F:KING]Ah! Well, you see, if the Cool Sofia here is to be believed, you are an original Sofia!" },
                        new object[] { "C|GWS:CONTINUE", "F|COOL SOFIA|HAPPY", "T|Cool Sofia|She is indeed. A pure Sofia, a *generic* Sofia. Unique in her world!" },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|GRINNING", "F|COOL SOFIA|EXCITED", "T|Cool Sofia|She has the power to bring light once more to ULTRASOFIAWORLD!" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|UNIMPRESSED", "F|COOL SOFIA|HAPPY", "T|Sofia|So I've heard." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "T|Sofia|But that's still not telling me what this, uh, PROCEDURE is." },
                        new object[] { "B|SOFIA_KING_SECONDARY_FINALIZE" }
                        };
                    break;
                case "SOFIA_KING_SECONDARY_FINALIZE":
                    Script = new object[]
                    {
                        new object[] { "C|GWS:CONTINUE", "T|Cool Sofia|Don't worry, we're not going to chop your head off or anything. B)" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "T|Sofia|Somehow that doesn't reassure me all that much!" },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|HAPPY", "T|King Sofia|[F:KING]No need to worry, no need to worry!" },
                        new object[] { "C|GWS:CONTINUE", "T|King Sofia|[F:KING]The PROCEDURE is more of a ritual than anything else. A process to inject some of your Sofia [R,F:KING]ESSENCE[C:255-255-255-255,F:KING] into the [R,F:KING]SOURCE[C:255-255-255-255,F:KING]." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "T|Sofia|The source?" },
                        new object[] { "C|GWS:CONTINUE", "F|COOL SOFIA|HAPPY", "T|Cool Sofia|The [R]SOURCE[C:WHITE] is an opening into the underlying energy continuum upon which ULTRASOFIAWORLD is built." },
                        new object[] { "C|GWS:CONTINUE", "T|Cool Sofia|It's an open vein of power, the power being the [R]ESSENCE[C:WHITE]. Placing some of your essence into it should kickstart a reaction that will reverse the decay that our world has been experiencing." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|CONSIDERING", "T|Sofia|Giving up some of my essence..." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Will that hurt me at all?" },
                        new object[] { "C|GWS:CONTINUE", "T|King Sofia|[F:KING]It shouldn't do, no." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "T|Sofia|Well, maybe I trust you. But only because you're me." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|GRINNING", "T|Sofia|Also, because I like your floaty crowny thing." },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|LAUGHING", "T|King Sofia|[F:KING]Thanks!" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|LAUGHING", "F|COOL SOFIA|HAPPY", "T|Cool Sofia|Because you're a pure Sofia, your essence should just regenerate on its own." },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|EXCITED", "T|King Sofia|[F:KING]Yes, but the Mystic Sofia will be able to tell you more." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "T|Sofia|Mystic Sofia, huh?" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|HAPPY", "T|King Sofia|[F:KING]She's the one who you will be going to see. The devotee who guards the [R,F:KING]SOURCE[C:255-255-255-255,F:KING]." },
                        new object[] { "C|GWS:CONTINUE", "T|King Sofia|[F:KING]She will oversee the essence injection process." },
                        new object[] { "C|GWS:CONTINUE", "F|COOL SOFIA|THINKING", "T|Cool Sofia|We'll need some documentation to make sure she'll see us, though." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|UNIMPRESSED", "T|Sofia|You know, this bizarre Sofia dimension has a strange amount of bureaucracy." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|HAPPY", "T|King Sofia|Quite. But worry not, I can provide the relevant documentation." },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|LAUGHING", "T||The King Sofia hands you a [C:138-0-255-255]ROYAL EDICT[C:WHITE].", new VoidDel(delegate() { Sofia.KingFlag = 3; }) },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|EXCITED", "T|King Sofia|[F:KING]This should get you into the Mystic's sanctum." },
                        new object[] { "C|GWS:CONTINUE", "T|Cool Sofia|You should probably get going right away." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "T|Sofia|Wait, are you not coming?" },
                        new object[] { "C|GWS:CONTINUE", "F|COOL SOFIA|LAUGHING", "T|Cool Sofia|Maybe I'll come along for the finale, but I have a few things I need to take care of first." },
                        new object[] { "C|GWS:CONTINUE", "F|COOL SOFIA|UNIMPRESSED", "T|Cool Sofia|Just please don't skip town again this time." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|DOWNCAST", "F|COOL SOFIA|HAPPY", "T|Sofia|I'll... okay. Geeze." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "T|Sofia|So, how do I get to this place?" },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|THINKING", "T|King Sofia|[F:KING]Oh yes, of course!" },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|GRINNING", "T|King Sofia|[F:KING]I see you have a map! Let me see... yes! The sanctum of the Mystic Sofia at the [R,F:KING]SOURCE[C:WHITE,F:KING] is marked." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|HAPPY", "T|King Sofia|[F:KING]You can simply navigate your way there using it!" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Ah, yeah. Yeah, I think I can do that." },
                        new object[] { "C|GWS:CONTINUE", "F|COOL SOFIA|LAUGHING", "T|Cool Sofia|Well, no time to waste." },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|LAUGHING", "T|King Sofia|[F:KING]Yes! Fare you well, young SOFIA!" },
                        new object[] { "C|GWS:impos_mapnavigate", "T||Use the [C:138-0-255-255]MAP[C:WHITE] to navigate to your next destination." },
                    };
                    break;
                case "SOFIA_KING_RETURN_POST_GOLEM_PRE_INTERIOR":
                    Script = new object[]
                    {
                        InitKingExtBasic,
                        new object[] { "C|GWS:CONTINUE", "T||This time the Golem waves you right on through." },
                        new object[] { "C|TIME:1500:ORSKIP", "A|SOFIA|FADEOUT", "A|CASTLEEXTBG|FADEOUT", "T||You head on inside." },
                        new object[] { "D|CASTLEEXTBG", "D|SOFIA", "B|SOFIA_KING_SECONDARY_INTERIOR" }
                    };
                    break;
                case "SOFIA_KING_RETURN_NO_EDICT":
                    Script = new object[]
                    {
                        InitKingExtBasic,
                        new object[] { "C|GWS:CONTINUE", "T||This time the Golem waves you right on through." },
                        new object[] { "C|TIME:1500:ORSKIP", "A|SOFIA|FADEOUT", "A|CASTLEEXTBG|FADEOUT", "T||You head on inside." },
                        new object[] { "C|TIME:0:ORSKIP", "D|CASTLEEXTBG", "A|SOFIA|FADEIN", "M|KING|TRUE", "F|SOFIA|WORRIED", new VoidDel(delegate()
                        {
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                WorldEntity Add = new WorldEntity("CASTLEINTBG", new Vector2(0, 0), (TAtlasInfo)Shell.AtlasDirectory["CASTLEINTBG"], 0.05f);
                                Add.ColourValue = new Color(0,0,0,0);
                                Add.AnimationQueue.Add(Animation.Retrieve("FADEIN"));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                                Add = new WorldEntity("KING SOFIA", new Vector2(930, 405), (TAtlasInfo)Shell.AtlasDirectory["KINGSOFIA"], 0.5f);
                                Add.ColourValue = new Color(0,0,0,0);
                                Add.AnimationQueue.Add(Animation.Retrieve("FADEIN"));
                                Add.CenterOrigin = true;
                                Add.SetAtlasFrame(new Point(1, 1));
                                Add.Scale(new Vector2(-0.06f, -0.06f));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                                Add = new WorldEntity("COOL SOFIA", new Vector2(720, 405), (TAtlasInfo)Shell.AtlasDirectory["COOLSOFIA"], 0.46f);
                                Add.ColourValue = new Color(0,0,0,0);
                                Add.AnimationQueue.Add(Animation.Retrieve("FADEIN"));
                                Add.CenterOrigin = true;
                                Add.SetAtlasFrame(new Point(3, 0));
                                Add.Scale(new Vector2(-0.12f, -0.12f));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                            }));
                        })
                        },
                        new object[] { "C|GWS:CONTINUE", "T|King Sofia|[F:KING]Ah, she returns!" },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|EXCITED", "T|Cool Sofia|Oh, Lord, wow." },
                        new object[] { "C|GWS:CONTINUE", "F|COOL SOFIA|JUDGING", "T|Cool Sofia|Why did you run off *again*?!" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|LAUGHING", "F|COOL SOFIA|UNIMPRESSED", "T|Sofia|Hey, well maybe I don't conform to your whims!" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "T|Sofia|I'm a free spirit, roaming to and fro..." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|CONSIDERING", "T|Sofia|Discovering what whacky possibilities this world has in store for me..." },
                        new object[] { "C|GWS:CONTINUE", "F|COOL SOFIA|HAPPY", "T|Cool Sofia|Yes, but don't you realize how difficult you're making this?" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|GRINNING", "T|Sofia|Hey, I already said that I don't care what you think!" },
                        new object[] { "C|GWS:CONTINUE", "T|Cool Sofia|Nah, hun." },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|UNIMPRESSED", "F|COOL SOFIA|UNIMPRESSED", "F|SOFIA|UNIMPRESSED", "T|Cool Sofia|I mean, for the person programming all this?" },
                        new object[] { "C|GWS:CONTINUE", "T|Cool Sofia|Your reckless use of the [C:138-0-255-255]MAP[C:WHITE] function... your jumping to and fro in the middle of story beats..." },
                        new object[] { "C|GWS:CONTINUE", "F|COOL SOFIA|JUDGING", "T|Cool Sofia|Your wild ramblings across branching progression pathways! Madness!" },
                        new object[] { "C|GWS:CONTINUE", "F|COOL SOFIA|UNIMPRESSED", "T|Cool Sofia|Do you realize how much contextual dialogue I had to write to account for all of these unlikely eventualities where you leave in the middle of a scene? All because of your cursed propensity for taking a non-obvious path through this carefully crafted narrative!" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|GRINNING", "T|Sofia|Hey, screw you! I'm not the one who decided to add the ability for me to leave in the middle of a scene! To be honest, that sounds like probably a bad design decision in retrospect!" },
                        new object[] { "C|GWS:CONTINUE", "F|COOL SOFIA|THINKING", "T|Cool Sofia|..." },
                        new object[] { "C|GWS:CONTINUE", "T|Cool Sofia|Touche." },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|THINKING", "T|King Sofia|[F:KING]...What on Ultrasofiaworld are you two talking about?" },
                        new object[] { "C|GWS:CONTINUE", "T|Cool Sofia|I..." },
                        new object[] { "C|GWS:CONTINUE", "F|COOL SOFIA|CONSIDERING", "T|Cool Sofia|...actually have no idea." },
                        new object[] { "C|GWS:CONTINUE", "F|COOL SOFIA|EXCITED", "T|Cool Sofia|Whatever. B)" },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|JUDGING", "T|King Sofia|[F:KING]Perhaps we should return to our topic of discussion before our young newcomer here left." },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|HAPPY", "T|Cool Sofia|Yes, where were we..." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "T|Sofia|Uh... I think you were telling me what I needed to do to help you guys save the world?" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|UNIMPRESSED", "T|Sofia|Honestly it's hard to keep up with what's going on at this point." },
                        new object[] { "C|GWS:CONTINUE", "F|COOL SOFIA|HAPPY", "T|Cool Sofia|That sounds about right." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "F|KING SOFIA|GRINNING", "T|King Sofia|[F:KING]Indeed!" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Yeah, well, I'm still not entirely sure how or why that's supposed to work." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Like, why me, again?" },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|CONSIDERING", "T|King Sofia|[F:KING]Ah! Well, you see, if the Cool Sofia here is to be believed, you are an original Sofia!" },
                        new object[] { "C|GWS:CONTINUE", "F|COOL SOFIA|HAPPY", "T|Cool Sofia|She is indeed. A pure Sofia, a *generic* Sofia. Unique in her world!" },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|GRINNING", "F|COOL SOFIA|EXCITED", "T|Cool Sofia|She has the power to bring light once more to ULTRASOFIAWORLD!" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|UNIMPRESSED", "F|COOL SOFIA|HAPPY", "T|Sofia|So I've heard." },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|CONSIDERING", "T|King Sofia|[F:KING]Well you see, your unique origin and ontological purity grants your [R,F:KING]SOFIA ESSENCE[C:255-255-255-255,F:KING] a unique quality and scope." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "T|King Sofia|[F:KING]It is this that could be used to restore life to my Kingdom!" },
                        new object[] { "C|GWS:CONTINUE", "T|King Sofia|[F:KING]But this is no mere talk. Our world lessens with every day, and we must make haste!" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "F|KING SOFIA|HAPPY", "T|King Sofia|[F:KING]We must proceed at once with THE PROCEDURE." },
                        new object[] { "C|GWS:CONTINUE", "T|Cool Sofia|Much agreed, Sire. There is no time to waste." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Whoa there! Hold up, what procedure!" },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|LAUGHING", "T|Sofia|That sounds highly ominous and I am very afraid all of a sudden." },
                        new object[] { "B|SOFIA_KING_SECONDARY_FINALIZE" }
                    };
                    break;
                case "SOFIA_KING_RETURN_CLUELESS":
                    Script = new object[]
                    {
                        InitKingExtBasic,
                        new object[] { "C|GWS:CONTINUE", "T||This time the Golem waves you right on through." },
                        new object[] { "C|TIME:1500:ORSKIP", "A|SOFIA|FADEOUT", "A|CASTLEEXTBG|FADEOUT", "T||You head on inside." },
                        new object[] { "C|TIME:0:ORSKIP","D|CASTLEEXTBG" },
                        new object[] { "C|GWS:CONTINUE", "A|SOFIA|FADEIN", "M|KING|TRUE", "F|SOFIA|DOWNCAST", "T||The castle's intimidating walls and stained glass windows loom over you.", new VoidDel(delegate()
                        {
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                WorldEntity Add = new WorldEntity("CASTLEINTBG", new Vector2(0, 0), (TAtlasInfo)Shell.AtlasDirectory["CASTLEINTBG"], 0.05f);
                                Add.ColourValue = new Color(0,0,0,0);
                                Add.AnimationQueue.Add(Animation.Retrieve("FADEIN"));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                            }));
                        })
                        },
                        new object[] { "C|GWS:CONTINUE", "T||The Cool Sofia and the King Sofia are still in the throne room. As you enter, they look up eagerly.", new VoidDel(delegate()
                        {
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                WorldEntity Add = new WorldEntity("KING SOFIA", new Vector2(1530, 405), (TAtlasInfo)Shell.AtlasDirectory["KINGSOFIA"], 0.5f);
                                Add.CenterOrigin = true;
                                Add.SetAtlasFrame(new Point(0, 1));
                                Add.Scale(new Vector2(-0.06f, -0.06f));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                                Add = new WorldEntity("COOL SOFIA", new Vector2(1730, 405), (TAtlasInfo)Shell.AtlasDirectory["COOLSOFIA"], 0.46f);
                                Add.CenterOrigin = true;
                                Add.SetAtlasFrame(new Point(1, 0));
                                Add.Scale(new Vector2(-0.12f, -0.12f));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                            }));
                        })
                        },
                        new object[] { "C|GWS:CONTINUE", "A|KING SOFIA|-600=0,1100,20||||", "A|COOL SOFIA|-1010=0,1700,20||||", "T|King Sofia|[F:KING]You return!" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "T|King Sofia|[F:KING]Are we saved? Is my land restored at last???" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Um..." },
                        new object[] { "C|GWS:CONTINUE", "F|COOL SOFIA|UNIMPRESSED", "F|KING SOFIA|UNIMPRESSED", "T|Sofia|...no?" },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|JUDGING", "T|King Sofia|[F:KING]Whyever not!" },
                        new object[] { "C|GWS:CONTINUE", "T|King Sofia|[F:KING]Did you talk to the [C:138-0-255-255,F:KING]MYSTIC SOFIA[C:255-255-255-25,F:KING] and ask her what is required to complete [C:138-0-255-255,F:KING]THE PROCEDURE[C:255-255-255-255,F:KING]???" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Not... really?" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|(Also hey did you know that your text actually gets super hard to read when you do... that...... okayi'llbequiet)" },
                        new object[] { "C|GWS:CONTINUE", "T|Cool Sofia|..." },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|DOWNCAST", "T|King Sofia|[F:KING]Well then..." },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|JUDGING", "T|King Sofia|[R,F:KING]WHY DON'T YOU HURRY UP AND GET TO IT???" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Okay! Okay! I'm going!!!" },
                        new object[] { "C|GWS:CONTINUE", "T||You really should probably go and find out what the MYSTIC SOFIA needs." },
                        new object[] { "C|GWS:impos_mapnavigate", "T||Use the [C:138-0-255-255]MAP[C:WHITE] to navigate to your next destination." },
                    };
                    break;
                case "SOFIA_KING_RETURN_NO_ESSENCE":
                    Script = new object[]
                    {
                        InitKingExtBasic,
                        new object[] { "C|GWS:CONTINUE", "T||This time the Golem waves you right on through." },
                        new object[] { "C|TIME:1500:ORSKIP", "A|SOFIA|FADEOUT", "A|CASTLEEXTBG|FADEOUT", "T||You head on inside." },
                        new object[] { "C|TIME:0:ORSKIP","D|CASTLEEXTBG" },
                        new object[] { "C|GWS:CONTINUE", "A|SOFIA|FADEIN", "M|KING|TRUE", "F|SOFIA|DOWNCAST", "T||The castle's intimidating walls and stained glass windows loom over you.", new VoidDel(delegate()
                        {
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                WorldEntity Add = new WorldEntity("CASTLEINTBG", new Vector2(0, 0), (TAtlasInfo)Shell.AtlasDirectory["CASTLEINTBG"], 0.05f);
                                Add.ColourValue = new Color(0,0,0,0);
                                Add.AnimationQueue.Add(Animation.Retrieve("FADEIN"));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                            }));
                        })
                        },
                        new object[] { "C|GWS:CONTINUE", "T||The Cool Sofia and the King Sofia are still in the throne room. As you enter, they look up eagerly.", new VoidDel(delegate()
                        {
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                WorldEntity Add = new WorldEntity("KING SOFIA", new Vector2(1530, 405), (TAtlasInfo)Shell.AtlasDirectory["KINGSOFIA"], 0.5f);
                                Add.CenterOrigin = true;
                                Add.SetAtlasFrame(new Point(0, 1));
                                Add.Scale(new Vector2(-0.06f, -0.06f));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                                Add = new WorldEntity("COOL SOFIA", new Vector2(1730, 405), (TAtlasInfo)Shell.AtlasDirectory["COOLSOFIA"], 0.46f);
                                Add.CenterOrigin = true;
                                Add.SetAtlasFrame(new Point(1, 0));
                                Add.Scale(new Vector2(-0.12f, -0.12f));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                            }));
                        })
                        },
                        new object[] { "C|GWS:CONTINUE", "A|KING SOFIA|-600=0,1100,20||||", "A|COOL SOFIA|-1010=0,1700,20||||", "T|King Sofia|[F:KING]Ah, she returns from the her quest!" },
                        new object[] { "C|GWS:CONTINUE", "T|King Sofia|[F:KING]Did you rendezvous with the Mystic Sofia?" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "T|Sofia|I did, but we've hit a slight problem." },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|CONSIDERING", "T|King Sofia|[F:KING]Oh? How so?" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|The Mystic said we need more essence." },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|UNIMPRESSED", "T|King Sofia|[F:KING]Well, yes. That's sort of the point." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "T|Sofia|No, as in, we need more, uh, input essence to perform the PROCEDURE. Like, in addition to mine?", new VoidDel(delegate() { Sofia.KingFlag = 4; }) },
                        new object[] { "C|GWS:CONTINUE", "F|COOL SOFIA|THINKING", "T|Cool Sofia|Really." },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|JUDGING", "T|King Sofia|[F:KING]Hrmph. Cool Sofia, what is the meaning of this?" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|DOWNCAST", "T|King Sofia|[F:KING]You assured me that this SOFIA would be sufficient. You were quite certain when you petitioned me!" },
                        new object[] { "C|GWS:CONTINUE", "T|Cool Sofia|It is strange..." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|UNIMPRESSED", "F|COOL SOFIA|EXCITED", "T|Cool Sofia|But hey, we couldn't know until we got her here, and it ain't my fault if our girl here don't got the juice." },
                        new object[] { "C|GWS:CONTINUE", "F|COOL SOFIA|HAPPY", "T|Cool Sofia|No offence, Sis." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Yeah, no, that was still rude. [T:300]But you don't need to worry so much." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "F|KING SOFIA|CONSIDERING", "T|Sofia|She said it wouldn't take much. Apparently I have enough, like, pure [R]ESSENCE[C:WHITE]?" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|But she needs some local essence from this world to catalyse the reaction." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "T|Sofia|There was something about... opening a door? [T:300]It was weird, but I think I got the gist." },
                        new object[] { "C|GWS:CONTINUE", "F|COOL SOFIA|THINKING", "F|SOFIA|GRINNING", "T|Sofia|And I'm sure we can manage to find a solution that, right?" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|HAPPY", "T|Sofia|...right?" },
                        new object[] { "C|GWS:CONTINUE", "T|King Sofia|[F:KING]Hmm. Could she not extract some essence from the [R,F:KING]SOURCE[C:255-255-255-255,F:KING] for you to use?" },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|DOWNCAST", "F|SOFIA|WORRIED", "T|Sofia|I... don't think so? She said it couldn't come from the [R]SOURCE[C:WHITE] itself directly..." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|...why are you two frowning?" },
                        new object[] { "C|GWS:CONTINUE", "T|Cool Sofia|Hmm. This could be difficult." },
                        new object[] { "C|GWS:CONTINUE", "T|King Sofia|[F:KING]It is hard to obtain [R,F:KING]ESSENCE[C:255-255-255-255,F:KING] external to the [R,F:KING]SOURCE[C:255-255-255-255,F:KING]." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|But... you're the King!" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|UNIMPRESSED", "T|Sofia|What's the point in being King and having like, a magical floating crown and, uh, spooky chaos emerald eyes if you don't have a vault full of secret but useful mystic treasures???" },
                        new object[] { "C|GWS:CONTINUE", "F|COOL SOFIA|DOWNCAST", "T|King Sofia|[F:KING]The land is lean. We no longer keep external stockpiles of the [R,F:KING]SOFIA ESSENCE[C:255-255-255-255,F:KING]." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "T|Sofia|Surely there's something we can do, yo! I've already put more than enough on the line for this." },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|CONSIDERING", "T|King Sofia|[F:KING]Well, of course, we all have our own internal [R,F:KING]ESSENCE[C:255-255-255-255,F:KING]..." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|EXCITED", "T|Sofia|Oh, great! That could-" },
                        new object[] { "C|TIME:2000:ORSKIP", "F|KING SOFIA|THINKING", "T|King Sofia|[F:KING]...But it is not of the same strength as yours. Extracting it from a host would result in severe damage, or even their death." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|DOWNCAST" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|-oh." },
                        new object[] { "C|GWS:CONTINUE", "T|Cool Sofia|Don't go easy on her. You and I both know what would happen if we really tried, Sire." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "F|COOL SOFIA|LAUGHING", "T|Cool Sofia|Whatever Sofia we used - she'd disintegrate into a thousand tiny pieces, dude." },
                        new object[] { "C|GWS:CONTINUE", "F|COOL SOFIA|THINKING", "F|KING SOFIA|CONSIDERING", "T|King Sofia|[F:KING]Of course, if a sacrifice is required..." },
                        new object[] { "C|GWS:CONTINUE", "A|SOFIA|SHAKEMINOR", "T|Sofia|Hey, no way!" },
                        new object[] { "C|GWS:CONTINUE", "T|Cool Sofia|Hey, Cool it. B)" },
                        new object[] { "C|GWS:CONTINUE", "T|Cool Sofia|You're both in luck, because..." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "F|KING SOFIA|EXCITED", "F|COOL SOFIA|GRINNING", "T|Cool Sofia|...I have an idea." },
                        new object[] { "C|GWS:CONTINUE", "T|King Sofia|[F:KING]Oh? You do?" },
                        new object[] { "B|SOFIA_KING_RETURN_ESSENCE_SOLUTION" },
                    };
                    break;
                case "SOFIA_KING_RETURN_ESSENCE_SOLUTION":
                    Script = new object[]
                    {
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "T|Sofia|I'm listening. But if this involves anyone being sacrificed..." },
                        new object[] { "C|GWS:CONTINUE", "F|COOL SOFIA|HAPPY", "T|Cool Sofia|Hey, give me some credit." },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|CONSIDERING", "T|Cool Sofia|So, we need some regular [R]SOFIA ESSENCE[C:WHITE], from this world, right?" },
                        new object[] { "C|GWS:CONTINUE", "T|Cool Sofia|And it can't be straight from the [R]SOURCE[C:WHITE]. So that means from another Sofia, or, otherwise, essence that *was* extracted from the source, but has been out of it for long enough that it's had a chance to differentiate." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "T|Sofia|Yeah, I think that's pretty much exactly what she said." },
                        new object[] { "C|GWS:CONTINUE", "F|COOL SOFIA|GRINNING", "T|Cool Sofia|So what we need is an essence stockpile. *Any* essence stockpile." },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|UNIMPRESSED", "T|King Sofia|[F:KING]Well, yes. But there are none." },
                        new object[] { "C|GWS:CONTINUE", "T|King Sofia|[F:KING]I feel like I just said that." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "F|COOL SOFIA|HAPPY", "T|Cool Sofia|Well, yes. There are none *here*, or in the Royal Collection..." },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|CONSIDERING", "F|COOL SOFIA|EXCITED", "T|Cool Sofia|But I think I might know [C:138-0-255-255]ANOTHER PLACE[C:WHITE] where there could be some essence stockpiled.", new VoidDel(delegate() { Sofia.KingFlag = 5; }) },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|JUDGING", "T|King Sofia|[F:KING]What?! This is the first I've heard of this." },
                        new object[] { "C|GWS:CONTINUE", "T|King Sofia|[F:KING]How could there be other essence stockpiles? I know of none!" },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|UNIMPRESSED", "F|COOL SOFIA|HAPPY", "T|Cool Sofia|I may have... not mentioned this previously." },
                        new object[] { "C|GWS:CONTINUE", "F|COOL SOFIA|GRINNING", "T|Cool Sofia|(No sense ratting on your sources to Royalty, yo)." },
                        new object[] { "C|GWS:CONTINUE", "F|COOL SOFIA|HAPPY", "T|Cool Sofia|But I have heard word that somebody might have had access to and been stocking [R]ESSENCE[C:WHITE] below the board, for sale." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "T|Cool Sofia|They're apparently based in a cave somewhere out in the [C:138-0-255-255]BADLANDS[C:WHITE]." },
                        new object[] { "C|GWS:CONTINUE", "T|King Sofia|[F:KING]I should have you drawn and quartered for not having told me of this. Hoarding black market essence, in these times... this is highest treason!" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "F|KING SOFIA|CONSIDERING", "T|King Sofia|[F:KING]If it were not the answer to our conundrum..." },
                        new object[] { "C|GWS:CONTINUE", "F|COOL SOFIA|LAUGHING", "T|Cool Sofia|A solution's a solution, yo." },
                        new object[] { "C|GWS:CONTINUE", "F|COOL SOFIA|HAPPY", "T|Cool Sofia|To be honest, I'm not sure if this mysterious illegal [R]ESSENCE[C:WHITE] really exists." },
                        new object[] { "C|GWS:CONTINUE", "T|Cool Sofia|But if it does..." },
                        new object[] { "C|GWS:CONTINUE", "T|King Sofia|[F:KING]...it would indeed be the answer to our needs." },
                        new object[] { new VoidDel(delegate()
                        {
                            if (Sofia.CrookedFlag == 0 || Sofia.CrookedFlag == 5) { ScriptProcessor.ActivateScriptElement("B|SOFIA_KING_RETURN_NO_ESSENSE_NO_CROOKED"); }
                            else { ScriptProcessor.ActivateScriptElement("B|SOFIA_KING_RETURN_NO_ESSENSE_YES_CROOKED"); }
                        })
                        },
                    };
                    break;
                case "SOFIA_KING_RETURN_NO_ESSENSE_NO_CROOKED":
                    Script = new object[]
                    {
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "F|KING SOFIA|JUDGING", "T|King Sofia|[F:KING]SO BE IT!" },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|EXCITED", "T|King Sofia|[F:KING]Young one! Or- ah, whatever. You there! Pure Sofia, brave of heart, fresh, new Sofia!" },
                        new object[] { "C|GWS:CONTINUE", "F|COOL SOFIA|LAUGHING", "T|King Sofia|[F:KING]You must travel to the [C:138-0-255-255,F:KING]BADLANDS[C:WHITE,F:KING] and seek out this fabled wealth of [R,F:KING]ESSENCE[C:WHITE,F:KING]!" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Hey, wait, why do I always have to do these things on my own!" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|The Badlands? They sound... bad! What gives!" },
                        new object[] { "C|GWS:CONTINUE", "T|King Sofia|[F:KING]You will need to be there in person to receive the new essence!" },
                        new object[] { "C|GWS:CONTINUE", "F|COOL SOFIA|HAPPY", "T|Cool Sofia|And we can't go with you because, hey, we're busy, babe. B)" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Busy with what??" },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|LAUGHING", "F|COOL SOFIA|LAUGHING", "T|Cool Sofia|Wouldn't you like to know." },
                        new object[] { "C|TIME:2200:ORSKIP", "T|Sofia|But can't you send someone with me? Like maybe-" },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|JUDGING", "T|King Sofia|[F:KING]Enough! Enough of this!" },
                        new object[] { "C|GWS:CONTINUE", "F|COOL SOFIA|HAPPY", "T|King Sofia|[F:KING]The [R,F:KING]PURE SOFIA[C:WHITE,F:KING] shall embark upon the quest. It is decreed!" },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|EXCITED", "T|King Sofia|[F:KING]The Badlands should be marked upon your [C:138-0-255-255,F:KING]MAP[C:WHITE,F:KING]." },
                        new object[] { "C|GWS:CONTINUE", "T|Cool Sofia|Good luck, yo. You're doing a great thing for all of us." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|UNIMPRESSD", "T|Sofia|............" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Okay, fine. I'll go." },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|GRINNING", "T|King Sofia|[F:KING]Wonderful!" },
                        new object[] { "C|GWS:CONTINUE", "T|Cool Sofia|Wishing you the best, Sis. ;)" },
                        new object[] { "C|GWS:impos_mapnavigate", "T||Use the [C:138-0-255-255]MAP[C:WHITE] to navigate to your next destination." }
                    };
                    break;
                case "SOFIA_KING_RETURN_NO_ESSENSE_YES_CROOKED":
                    Script = new object[]
                    {
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Huh." },
                        new object[] { "C|GWS:CONTINUE", "F|COOL SOFIA|WORRIED", "T|Sofia|I... think I might already know something about this." },
                        new object[] { "C|GWS:CONTINUE", "T|Cool Sofia|Wait, really?" },
                        new object[] { "C|GWS:CONTINUE", "T|Cool Sofia|That sort of seems unlikely, but, go on." },
                        new object[] { "C|GWS:CONTINUE", "F|COOL SOFIA|DOWNCAST", "T|Sofia|Earlier I was walking around, and I found this sort of cave." },
                        new object[] { "C|GWS:CONTINUE", "F|COOL SOFIA|UNIMPRESSED", "T|Sofia|There was this person in there. A Sofia. She had two eyepatches? Spoke kinda like a vaguely cockney goblin?" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "T|Sofia|Cockney? Cock-e-ney? How do you even pronounce that?" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|CONSIDERING", "T|Sofia|She said she was selling essence, so maybe that was her?" },
                        new object[] { "C|GWS:CONTINUE", "T|Cool Sofia|Yep. [T:300]That'd be her." },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|JUDGING", "T|King Sofia|[F:KING]Young one!" },
                        new object[] { "C|GWS:CONTINUE", "F|COOL SOFIA|LAUGHING", "F|SOFIA|WORRIED", "T|King Sofia|[F:KING]Know you not better than to consort with unscrupulous cockneys?!" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|UNIMPRESSED", "T|Sofia|What can I say. I'm a cockney botherer." },
                        new object[] { "C|GWS:CONTINUE", "F|COOL SOFIA|HAPPY", "T|King Sofia|[F:KING]Despicable!" },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|GRINNING", "T|King Sofia|[F:KING]But also, great!" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "T|King Sofia|[F:KING]You have already built up a rapport!" },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|THINKING", "T|King Sofia|[F:KING]Young one- Or, I should say, same-aged-one!" },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|CONSIDERING", "T|King Sofia|[F:KING]You shall embark upon a new quest to find your cockeney friend once more, and retrieve the [R,F:KING]ESSENCE[C:WHITE,F:KING] she bears!" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|It's \"cockney\"." },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|EXCITED", "T|King Sofia|[F:KING]Precisely!" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Why can't you two just come as well?" },
                        new object[] { "C|GWS:CONTINUE", "F|COOL SOFIA|HAPPY", "T|Cool Sofia|Because, hey, we're busy, babe. B)" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Busy with what??" },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|LAUGHING", "F|COOL SOFIA|LAUGHING", "T|Cool Sofia|Wouldn't you like to know." },
                        new object[] { "C|TIME:2200:ORSKIP", "T|Sofia|But can't you send someone with me? Like maybe-" },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|JUDGING", "T|King Sofia|[F:KING]Enough! Enough of this!" },
                        new object[] { "C|GWS:CONTINUE", "T|King Sofia|[F:KING]For one, only you have that precious cockney rapport!" },
                        new object[] { "C|GWS:CONTINUE", "T|Cool Sofia|And crooks don't generally respond well to authority figures, yo." },
                        new object[] { "C|GWS:CONTINUE", "F|COOL SOFIA|HAPPY", "T|King Sofia|[F:KING]The [R,F:KING]PURE SOFIA[C:WHITE,F:KING] shall embark upon the quest. It is decreed!" },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|EXCITED", "T|King Sofia|[F:KING]The Badlands should be marked upon your [C:138-0-255-255,F:KING]MAP[C:WHITE,F:KING]." },
                        new object[] { "C|GWS:CONTINUE", "T|Cool Sofia|Good luck, yo. You're doing a great thing for all of us." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|UNIMPRESSD", "T|Sofia|............" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Okay, fine. I'll go." },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|GRINNING", "T|King Sofia|[F:KING]Wonderful!" },
                        new object[] { "C|GWS:CONTINUE", "T|Cool Sofia|Wishing you the best, Sis. ;)" },
                        new object[] { "C|GWS:impos_mapnavigate", "T||Use the [C:138-0-255-255]MAP[C:WHITE] to navigate to your next destination." }
                    };
                    break;
                case "SOFIA_KING_RETURN_NO_ESSENCE_RETURN":
                    Script = new object[]
                    {
                        InitKingExtBasic,
                        new object[] { "C|GWS:CONTINUE", "T||This time the Golem waves you right on through." },
                        new object[] { "C|TIME:1500:ORSKIP", "A|SOFIA|FADEOUT", "A|CASTLEEXTBG|FADEOUT", "T||You head on inside." },
                        new object[] { "C|TIME:0:ORSKIP","D|CASTLEEXTBG" },
                        new object[] { "C|GWS:CONTINUE", "A|SOFIA|FADEIN", "M|KING|TRUE", "F|SOFIA|DOWNCAST", "T||Castle walls. Stained glass windows. Whatever, you know the drill pretty well by this point.", new VoidDel(delegate()
                        {
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                WorldEntity Add = new WorldEntity("CASTLEINTBG", new Vector2(0, 0), (TAtlasInfo)Shell.AtlasDirectory["CASTLEINTBG"], 0.05f);
                                Add.ColourValue = new Color(0,0,0,0);
                                Add.AnimationQueue.Add(Animation.Retrieve("FADEIN"));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                            }));
                        })
                        },
                        new object[] { "C|GWS:CONTINUE", "T||Heck, you're almost an expert. Pink, oily castles? Clones of yourself with gems embedded in their faces? Poorly composed harpsicord music?" },
                        new object[] { "C|GWS:CONTINUE", "T||All old news. You breeze right on by." },
                        new object[] { "C|GWS:CONTINUE", "T||The Cool Sofia and the King Sofia are still in the throne room. As you enter, they look up eagerly.", new VoidDel(delegate()
                        {
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                WorldEntity Add = new WorldEntity("KING SOFIA", new Vector2(1530, 405), (TAtlasInfo)Shell.AtlasDirectory["KINGSOFIA"], 0.5f);
                                Add.CenterOrigin = true;
                                Add.SetAtlasFrame(new Point(1, 1));
                                Add.Scale(new Vector2(-0.06f, -0.06f));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                                Add = new WorldEntity("COOL SOFIA", new Vector2(1730, 405), (TAtlasInfo)Shell.AtlasDirectory["COOLSOFIA"], 0.46f);
                                Add.CenterOrigin = true;
                                Add.SetAtlasFrame(new Point(3, 0));
                                Add.Scale(new Vector2(-0.12f, -0.12f));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                            }));
                        })
                        },
                        new object[] { "C|GWS:CONTINUE", "A|KING SOFIA|-600=0,1100,20||||", "A|COOL SOFIA|-1010=0,1700,20||||", "T|King Sofia|[F:KING]You're back!" },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|WORRIED", "T|King Sofia|[F:KING]...where did you go?" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|UNIMPRESSED", "T|Cool Sofia|Something tells me that running off at impromptu moments is a running theme in this one's personal narrative." },
                        new object[] { "C|GWS:CONTINUE", "T|Cool Sofia|If not so much in this timeline, then certainly even moreso in others." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|HAPPY", "T|Sofia|Hey, give me a break!" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|I am what's known as a free spirit." },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|CONSIDERING" , "T|King Sofia|[F:KING]Well, while you were out spreading your wings, did you come up with a way to get that extra [R]ESSENCE[C:WHITE] that you needed?" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "T|Sofia|Um, not exactly... But..." },
                        new object[] { "C|GWS:CONTINUE",  "F|KING SOFIA|HAPPY", "T|Cool Sofia|But don't worry, because while you were running about..." },
                        new object[] { "C|GWS:CONTINUE", "F|COOL SOFIA|GRINNING", "T|Cool Sofia|...I, being the genius that I am, did!" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "T|Sofia|Huh, wait, really? You did?" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|DOWNCAST", "T|Sofia|I guess I didn't give you enough credit after all." },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|EXCITED", "T|King Sofia|[F:KING]Indeed! She hasn't even told me yet, and I'm on the edge of my seat waiting to hear her solution to our little conundrum." },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|THINKING", "F|SOFIA|WORRIED", "T|King Sofia|[F:KING]My idea was to ritually sacrifice you and hope that that would be enough..." },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|LAUGHING", "F|COOL SOFIA|LAUGHING", "T|King Sofia|[F:KING]But we're not sure that that would even work! Hah!" },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|HAPPY", "F|COOL SOFIA|HAPPY", "F|SOFIA|UNIMPRESSED", "T|Sofia|..." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|...I guess I'll have to reserve judgement on this other new idea, then." },
                        new object[] { "C|GWS:CONTINUE", "T|King Sofia|Worry not, worry not! Take it away, Cool Sofia." },
                        new object[] { "C|GWS:CONTINUE", "F|COOL SOFIA|GRINNING", "T|Cool Sofia|Okay, assuming you're both going to stay to hear it." },
                        new object[] { "B|SOFIA_KING_RETURN_ESSENCE_SOLUTION" }
                    };
                    break;
                case "SOFIA_KING_RETURN_DURING_ESSENCE_SEEK":
                    Script = new object[]
                    {
                        InitKingExtBasic,
                        new object[] { "C|GWS:CONTINUE", "T||This time the Golem waves you right on through." },
                        new object[] { "C|TIME:1500:ORSKIP", "A|SOFIA|FADEOUT", "A|CASTLEEXTBG|FADEOUT", "T||You head on inside." },
                        new object[] { "C|TIME:0:ORSKIP","D|CASTLEEXTBG" },
                        new object[] { "C|GWS:CONTINUE", "A|SOFIA|FADEIN", "M|KING|TRUE", "F|SOFIA|DOWNCAST", "T||Things seem pretty much the same as the last time you were here, which is probably because they absolutely are. It's not even been an hour or so yet.", new VoidDel(delegate()
                        {
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                WorldEntity Add = new WorldEntity("CASTLEINTBG", new Vector2(0, 0), (TAtlasInfo)Shell.AtlasDirectory["CASTLEINTBG"], 0.05f);
                                Add.ColourValue = new Color(0,0,0,0);
                                Add.AnimationQueue.Add(Animation.Retrieve("FADEIN"));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                            }));
                        })
                        },
                        new object[] { "C|GWS:CONTINUE", "T||You walk up to the other two Sofias, who are once more waiting in the throne room.", new VoidDel(delegate()
                        {
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                WorldEntity Add = new WorldEntity("KING SOFIA", new Vector2(1530, 405), (TAtlasInfo)Shell.AtlasDirectory["KINGSOFIA"], 0.5f);
                                Add.CenterOrigin = true;
                                Add.SetAtlasFrame(new Point(0, 0));
                                Add.Scale(new Vector2(-0.06f, -0.06f));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                                Add = new WorldEntity("COOL SOFIA", new Vector2(1730, 405), (TAtlasInfo)Shell.AtlasDirectory["COOLSOFIA"], 0.46f);
                                Add.CenterOrigin = true;
                                Add.SetAtlasFrame(new Point(1, 0));
                                Add.Scale(new Vector2(-0.12f, -0.12f));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                            }));
                        })
                        },
                        new object[] { "C|GWS:CONTINUE", "A|KING SOFIA|-600=0,1100,20||||", "A|COOL SOFIA|-1010=0,1700,20||||", "T|Cool Sofia|Heya babe." },
                        new object[] { "C|GWS:CONTINUE", "T|King Sofia|[F:KING]Well met again, my friend!" },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|THINKING", "T|King Sofia|[F:KING]Any luck on thy quest?" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "T|Sofia|Not so much yet, no." },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|DOWNCAST", "T|King Sofia|[F:KING]Well, I didn't think it would be easy." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Are you sure I can get, uh, [R]ESSENCE[C:WHITE] from this person?" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|UNIMPRESSED", "T|Sofia|I'm not sure that she sounds particularly trustworthy." },
                        new object[] { "C|GWS:CONTINUE", "F|COOL SOFIA|GRINNING", "T|Cool Sofia|Oh, she absolutely isn't." },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|HAPPY", "F|COOL SOFIA|HAPPY", "T|Cool Sofia|But my sources are, and they say that she can back up her claims." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "T|Cool Sofia|The essence is there, Sis. You just have to figure out a way to get it from her." },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|EXCITED", "T|King Sofia|[F:KING]Indeed! I have faith in you! As I do all people who I've just met an hour or so ago!" },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|CONSIDERING", "T|King Sofia|[F:KING](People may say I'm overly trusting, but I prefer \"enthusiastic\")." },
                        new object[] { "C|GWS:CONTINUE", "T|Cool Sofia|Quite." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "T|Sofia|Okay..." },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|EXCITED", "F|SOFIA|EXCITED", "T|Sofia|I'll head out again, then." },
                        new object[] { "C|GWS:CONTINUE", "T|Cool Sofia|Yes. Remember, if you navigate to the badlands with your [C:138-0-255-255]MAP[C:WHITE], then you should be able to find her." },
                        new object[] { "C|GWS:CONTINUE", "T|Cool Sofia|When you get there, try and convince her to give you some [R]ESSENCE[C:WHITE]." },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|GRINNING", "T|King Sofia|[F:KING]Good luck!" },
                        new object[] { "C|GWS:impos_mapnavigate", "T||Use the [C:138-0-255-255]MAP[C:WHITE] to navigate to your next destination." },
                    };
                    break;
                case "SOFIA_KING_RETURN_NO_ESSENSE_RETURN_WITH_ESSENCE":
                    Script = new object[]
                    {
                        InitKingExtBasic,
                        new object[] { "C|GWS:CONTINUE", "T||This time the Golem waves you right on through." },
                        new object[] { "C|TIME:1500:ORSKIP", "A|SOFIA|FADEOUT", "A|CASTLEEXTBG|FADEOUT", "T||You head on inside." },
                        new object[] { "C|TIME:0:ORSKIP","D|CASTLEEXTBG" },
                        new object[] { "C|GWS:CONTINUE", "A|SOFIA|FADEIN", "M|KING|TRUE", "F|SOFIA|DOWNCAST", "T||Castle walls. Stained glass windows. Whatever, you know the drill pretty well by this point.", new VoidDel(delegate()
                        {
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                WorldEntity Add = new WorldEntity("CASTLEINTBG", new Vector2(0, 0), (TAtlasInfo)Shell.AtlasDirectory["CASTLEINTBG"], 0.05f);
                                Add.ColourValue = new Color(0,0,0,0);
                                Add.AnimationQueue.Add(Animation.Retrieve("FADEIN"));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                            }));
                        })
                        },
                        new object[] { "C|GWS:CONTINUE", "T||Heck, you're almost an expert. Pink, oily castles? Clones of yourself with gems embedded in their faces? Poorly composed harpsicord music?" },
                        new object[] { "C|GWS:CONTINUE", "T||All old news. You breeze right on by." },
                        new object[] { "C|GWS:CONTINUE", "T||The Cool Sofia and the King Sofia are still in the throne room. As you enter, they look up eagerly.", new VoidDel(delegate()
                        {
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                WorldEntity Add = new WorldEntity("KING SOFIA", new Vector2(1530, 405), (TAtlasInfo)Shell.AtlasDirectory["KINGSOFIA"], 0.5f);
                                Add.CenterOrigin = true;
                                Add.SetAtlasFrame(new Point(1, 1));
                                Add.Scale(new Vector2(-0.06f, -0.06f));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                                Add = new WorldEntity("COOL SOFIA", new Vector2(1730, 405), (TAtlasInfo)Shell.AtlasDirectory["COOLSOFIA"], 0.46f);
                                Add.CenterOrigin = true;
                                Add.SetAtlasFrame(new Point(3, 0));
                                Add.Scale(new Vector2(-0.12f, -0.12f));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                            }));
                        })
                        },
                        new object[] { "C|GWS:CONTINUE", "A|KING SOFIA|-600=0,1100,20||||", "A|COOL SOFIA|-1010=0,1700,20||||", "T|King Sofia|[F:KING]You're back!" },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|WORRIED", "T|King Sofia|[F:KING]...where did you go?" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|UNIMPRESSED", "T|Cool Sofia|Something tells me that running off at impromptu moments is a running theme in this one's personal narrative." },
                        new object[] { "C|GWS:CONTINUE", "T|Cool Sofia|If not so much in this timeline, then certainly even moreso in others." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|HAPPY", "T|Sofia|Hey, give me a break!" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|I am what's known as a free spirit." },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|CONSIDERING" , "T|King Sofia|[F:KING]Well, while you were out spreading your wings, did you come up with a way to get that extra [R]ESSENCE[C:WHITE] that you needed?" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|GRINNING", "T|Sofia|Well, speaking of that..." },
                        new object[] { "C|TIME:400:ORSKIP", "F|SOFIA|JUDGING", "T|Sofia|[R]Behold, bruh.", new VoidDel(delegate()
                        {
                            Sofia.KingFlag = 6;
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                WorldEntity Add = new Sofia.EssenseGlow("ESSENCEGLOW", new Vector2(0, 0), (TAtlasInfo)Shell.AtlasDirectory["ESSENCEGLOW"], 0.6f);
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                            }));
                        })
                        },
                        new object[] { "C|TIME:0", "F|KING SOFIA|JUDGING", "F|COOL SOFIA|UNIMPRESSED" },
                        new object[] { "C|GWS:CONTINUE", "T|King Sofia|[F:KING]Whoa." },
                        new object[] { "C|GWS:CONTINUE", "T|King Sofia|[F:KING]Thou art... based!" },
                        new object[] { "C|GWS:CONTINUE", "T|Cool Sofia|...what." },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|EXCITED", "T|King Sofia|[F:KING]This means... you have found success?!" },
                        new object[] { "C|GWS:CONTINUE", "T|Cool Sofia|...WHAT." },
                        new object[] { "C|GWS:CONTINUE", "T|Cool Sofia|But, how?" },
                        new object[] { "C|GWS:CONTINUE", "T|Cool Sofia|You just wandered off in the middle of our discussion! Before I could tell you my idea!" },
                        new object[] { "C|GWS:CONTINUE", "T|Cool Sofia|I had an idea!" },
                        new object[] { "C|GWS:CONTINUE", "F|COOL SOFIA|DOWNCAST", "T|Cool Sofia|...I was going to give a cool monologue about how great I was and everything." },
                        new object[] { "C|GWS:CONTINUE", "F|COOL SOFIA|JUDGING", "T|Cool Sofia|And you just managed it on your own???" },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|THINKING", "T|King Sofia|[F:KING]I guess you underestimated her." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|[R]Hell yeah. Don't you underestimate my awesome." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|[R]I'm like, totes amazeballs." },
                        new object[] { "C|GWS:CONTINUE", "F|COOL SOFIA|UNIMPRESSED", "T|Cool Sofia|...[T:400]I hate that you said that. But. I do have to hand it to you." },
                        new object[] { "C|GWS:CONTINUE", "F|COOL SOFIA|DOWNCAST", "T|Cool Sofia|Well done." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Hell yeah!", new VoidDel(delegate()
                        {
                            Sofia.EssenseGlow EG = (Sofia.EssenseGlow)Shell.GetEntityByName("ESSENCEGLOW");
                            EG.AnimationQueue.Add(Animation.Retrieve("fadeoutcolourpreserve"));
                            EG.TransientAnimation = true;
                        })
                        },
                        new object[] { "C|GWS:CONTINUE", "D|ESSENCEGLOW|IFPRESENT", "F|COOL SOFIA|THINKING", "F|SOFIA|GRINNING", "T|Cool Sofia|...How did you manage it?" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|EXCITED", "T|Sofia|..." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|GRINNING", "T|Sofia|My methods are my own." },
                        new object[] { "C|GWS:CONTINUE", "F|COOL SOFIA|HAPPY", "T|Cool Sofia|...Fair enough." },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|EXCITED", "T|King Sofia|Forget all that. Let us focus on this great news!" },
                        new object[] { "B|SOFIA_KING_SUCCESSFUL_ESSENCE_RETRIEVE_END" }
                    };
                    break;
                case "SOFIA_KING_RETURN_ESSENCE_SEEK_SUCCESSFUL":
                    Script = new object[]
                    {
                        InitKingExtBasic,
                        new object[] { "C|GWS:CONTINUE", "T||This time the Golem waves you right on through." },
                        new object[] { "C|TIME:1500:ORSKIP", "A|SOFIA|FADEOUT", "A|CASTLEEXTBG|FADEOUT", "T||You head on inside." },
                        new object[] { "C|TIME:0:ORSKIP","D|CASTLEEXTBG" },
                        new object[] { "C|GWS:CONTINUE", "A|SOFIA|FADEIN", "M|KING|TRUE", "F|SOFIA|EXCITED", "T||Things seem pretty much the same as the last time you were here, which is probably because they absolutely are. It's not even been an hour or so yet.", new VoidDel(delegate()
                        {
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                WorldEntity Add = new WorldEntity("CASTLEINTBG", new Vector2(0, 0), (TAtlasInfo)Shell.AtlasDirectory["CASTLEINTBG"], 0.05f);
                                Add.ColourValue = new Color(0,0,0,0);
                                Add.AnimationQueue.Add(Animation.Retrieve("FADEIN"));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                            }));
                        })
                        },
                        new object[] { "C|GWS:CONTINUE", "T||But this time something is different! You! You're flushed with success, and also with a little bit of residual high from when the Crooked Sofia laid that essence on you." },
                        new object[] { "C|GWS:CONTINUE", "T||That stuff is some wicked shit, yo." },
                        new object[] { "C|GWS:CONTINUE", "T||You walk up to the other two Sofias, who are once more waiting in the throne room.", new VoidDel(delegate()
                        {
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                WorldEntity Add = new WorldEntity("KING SOFIA", new Vector2(1530, 405), (TAtlasInfo)Shell.AtlasDirectory["KINGSOFIA"], 0.5f);
                                Add.CenterOrigin = true;
                                Add.SetAtlasFrame(new Point(0, 0));
                                Add.Scale(new Vector2(-0.06f, -0.06f));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                                Add = new WorldEntity("COOL SOFIA", new Vector2(1730, 405), (TAtlasInfo)Shell.AtlasDirectory["COOLSOFIA"], 0.46f);
                                Add.CenterOrigin = true;
                                Add.SetAtlasFrame(new Point(1, 0));
                                Add.Scale(new Vector2(-0.12f, -0.12f));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                            }));
                        })
                        },
                        new object[] { "C|GWS:CONTINUE", "A|KING SOFIA|-600=0,1100,20||||", "A|COOL SOFIA|-1010=0,1700,20||||", "T|Cool Sofia|Heya babe." },
                        new object[] { "C|GWS:CONTINUE", "T|King Sofia|[F:KING]Well met again, my friend!" },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|THINKING", "T|King Sofia|[F:KING]Any luck on thy quest?" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Oh, you betcha!" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|GRINNING", "T|Sofia|Rejoice, clones one and two, because I am so ready to save your asses." },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|EXCITED", "T|King Sofia|[F:KING]You managed to get more [R,F:KING]ESSENCE[C:WHITE,F:KING]?" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|LAUGHING", "T|Sofia|Yep!", new VoidDel(delegate() { Sofia.KingFlag = 6; }) },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|EXCITED", "T|Sofia|I'm totes amazeballs, I know." },
                        new object[] { "B|SOFIA_KING_SUCCESSFUL_ESSENCE_RETRIEVE_END" }
                    };
                    break;
                case "SOFIA_KING_SUCCESSFUL_ESSENCE_RETRIEVE_END":
                    Script = new object[]
                    {
                        new object[] { "C|GWS:CONTINUE", "T|King Sofia|[F:KING]This is wonderful! Finally, all of [C:138-0-255-255]ULTRASOFIAWORLD[C:WHITE] is in with a shot at being saved!" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|GRINNING", "T|Sofia|I'm only about 60% sure that I understood what you just said, but hell yeah!" },
                        new object[] { "C|GWS:CONTINUE", "F|COOL SOFIA|EXCITED", "T|Cool Sofia|Hey, well done bro. I almost believed you had it in you." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|UNIMPRESSED", "T|Sofia|Hey, watch it. My jury's still out on my feelings about you, but I'm fairly sure that you're *not* totes amazeballs." },
                        new object[] { "C|GWS:CONTINUE", "F|COOL SOFIA|HAPPY", "F|SOFIA|HAPPY", "T|Cool Sofia|Well, amazeballs aside, there's no time to lose!" },
                        new object[] { "C|GWS:CONTINUE", "F|COOL SOFIA|THINKING", "T|Cool Sofia|You need to hurry up and return to the [C:138-0-255-255]MYSIC SOFIA[C:WHITE] before the fabric of [C:138-0-255-255]ULTRASOFIAWORLD[C:WHITE] degrades any more." },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|THINKING", "T|King Sofia|[F:KING]Yes, and that might cause things to get even *more* weird." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|UNIMPRESSED", "T|Sofia|...Somehow, I can't imagine it." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|EXCITED", "T|Sofia|But okay. Are you coming? Surely you want to, like, see the world saved? Or whatever it is we've been doing for the past god-knows-how-long now." },
                        new object[] { "C|GWS:CONTINUE", "F|COOL SOFIA|HAPPY", "T|Cool Sofia|I'll be heading along later. I have a few things I want to clear up here first." },
                        new object[] { "C|TIME:800:ORSKIP", "T|King Sofia|[F:KING]Yes, I have been instructing her in how to play five-dimensional checkers." },
                        new object[] { "C|GWS:CONTINUE", "F|COOL SOFIA|UNIMPRESSED" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|LAUGHING", "T|Cool Sofia|(Shh, don't tell her that!)" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|EXCITED", "F|COOL SOFIA|HAPPY", "T|Cool Sofia|Ahem. But, yes. Don't wait around to save the world on my account." },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|EXCITED", "T|King Sofia|[F:KING]You must head back to the Mystic Sofia right away!" },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|JUDGING", "T|King Sofia|[F:KING]In fact, I decree it as King. SAVE MY LAND, MY LEIGE!" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|LAUGHING", "T|Sofia|You're not actually my King, and I still can't really understand what you're saying when you do that, but..." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|...okay!" },
                        new object[] { "C|GWS:impos_mapnavigate", "T||Use the [C:138-0-255-255]MAP[C:WHITE] to navigate to your next destination." },
                    };
                    break;
                case "SOFIA_KING_FINAL":
                    Script = new object[]
                    {
                        InitKingExtBasic,
                        new object[] { "C|GWS:CONTINUE", "T||This time the Golem waves you right on through." },
                        new object[] { "C|TIME:1500:ORSKIP", "A|SOFIA|FADEOUT", "A|CASTLEEXTBG|FADEOUT", "T||You head on inside." },
                        new object[] { "C|TIME:0:ORSKIP","D|CASTLEEXTBG" },
                        new object[] { "C|GWS:CONTINUE", "A|SOFIA|FADEIN", "M|KING|TRUE", "F|SOFIA|DOWNCAST", "T||The two Sofias you have come to know are still inside.", new VoidDel(delegate()
                        {
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                WorldEntity Add = new WorldEntity("CASTLEINTBG", new Vector2(0, 0), (TAtlasInfo)Shell.AtlasDirectory["CASTLEINTBG"], 0.05f);
                                Add.ColourValue = new Color(0,0,0,0);
                                Add.AnimationQueue.Add(Animation.Retrieve("FADEIN"));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                                Add = new WorldEntity("KING SOFIA", new Vector2(1530, 405), (TAtlasInfo)Shell.AtlasDirectory["KINGSOFIA"], 0.5f);
                                Add.CenterOrigin = true;
                                Add.SetAtlasFrame(new Point(0, 1));
                                Add.Scale(new Vector2(-0.06f, -0.06f));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                                Add = new WorldEntity("COOL SOFIA", new Vector2(1730, 405), (TAtlasInfo)Shell.AtlasDirectory["COOLSOFIA"], 0.46f);
                                Add.CenterOrigin = true;
                                Add.SetAtlasFrame(new Point(1, 0));
                                Add.Scale(new Vector2(-0.12f, -0.12f));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                            }));
                        })
                        },
                        new object[] { "C|GWS:CONTINUE", "A|KING SOFIA|-600=0,1100,20||||", "A|COOL SOFIA|-1010=0,1700,20||||", "T|King Sofia|[F:KING]Hello again!" },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|CONSIDERING", "T|King Sofia|[F:KING]Is everything prepared to save my Kingdom?" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|GRINNING", "T|Sofia|Yep! I have everything we need." },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|JUDGING", "T|King Sofia|[F:KING]Then make haste! Hurry to the Mystic Sofia at the [R,F:KING]SOURCE[C:WHITE,F:KING]!" },
                        new object[] { "C|GWS:CONTINUE", "F|KING SOFIA|EXCITED", "F|COOL SOFIA|EXCITED", "T|Cool Sofia|Yeah, sis. There's no time to lose!" },
                        new object[] { "C|GWS:CONTINUE", "F|COOL SOFIA|HAPPY", "T|Cool Sofia|Head over to the Mystic Sofia. We'll catch up with you soon!" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Got it! On my way." },
                        new object[] { "C|GWS:impos_mapnavigate", "T||Use the [C:138-0-255-255]MAP[C:WHITE] to navigate to your next destination." },
                    };
                    break;
                case "SOFIA_CROOKED_SECONDARY_DOESNT_KNOW_MYSTIC":
                    Script = new object[]
                    {
                        new object[] { "C|GWS:CONTINUE", "M|#NULL", "T||So, you have to find a Mystic, huh?", new VoidDel(delegate()
                        {
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                WorldEntity Add = new WorldEntity("SOFIA", new Vector2(350, 405), (TAtlasInfo)Shell.AtlasDirectory["SOFIASPRITES"], 0.5f);
                                Add.ColourValue = new Color(0,0,0,0);
                                Add.CenterOrigin = true;
                                Add.SetAtlasFrame(new Point(2, 0));
                                Add.AnimationQueue.Add(Animation.Retrieve("FADEIN"));
                                Add.Scale(new Vector2(-0.06f, -0.06f));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                                Add = new WorldEntity("SOFIAWORLDBACKDROP", new Vector2(-600, 0), (TAtlasInfo)Shell.AtlasDirectory["SWBG"], 0.05f);
                                Add.ColourValue = new Color(0,0,0,0);
                                Add.AnimationQueue.Add(Animation.Retrieve("FADEIN"));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                            }));
                        })
                        },
                        new object[] { "C|GWS:CONTINUE", "T||You've sometimes fancied yourself as a bit of a Mystic. Inscrutible. Arcane. Mysterious." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "T||Sort of like this map you're having a bit of trouble reading." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|GRINNING", "T||Honestly, you're not sure where you're going, but this path seems to be going *somewhere*, so you guess you'll stick with it and hope that it is in fact the right way." },
                        new object[] { "C|GWS:CONTINUE", "A|SOFIAWORLDBACKDROP|FADEOUT", "T||After half an hour or so you find yourself descending into some low hills." },
                        new object[] { "C|GWS:CONTINUE", "T||You round a corner, and suddenly a wide cave entrance appears in front of you.", new VoidDel(delegate()
                        {
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                WorldEntity Add = new WorldEntity("CROOKEDCAVEBG", new Vector2(0, -160), (TAtlasInfo)Shell.AtlasDirectory["CROOKEDCAVEBG"], 0.05f);
                                Add.ColourValue = new Color(0,0,0,0);
                                Add.AnimationQueue.Add(Animation.Retrieve("FADEINLONG"));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                                Add = new WorldEntity("CRIME SHACK", new Vector2(850, 300), (TAtlasInfo)Shell.AtlasDirectory["CRIMESHACK"], 0.46f);
                                Add.ColourValue = new Color(0,0,0,0);
                                Add.CenterOrigin = true;
                                Add.Scale(new Vector2(-0.2f, -0.2f));
                                Shell.RenderQueue.Add(Add);
                                Shell.UpdateQueue.Add(Add);
                                Add = new WorldEntity("CROOKED SOFIA", new Vector2(1430, 405), (TAtlasInfo)Shell.AtlasDirectory["CROOKEDSOFIA"], 0.48f);
                                Add.CenterOrigin = true;
                                Add.SetAtlasFrame(new Point(3, 1));
                                Add.Scale(new Vector2(-0.06f, -0.06f));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                            }));
                        })
                        },
                        new object[] { "C|GWS:CONTINUE", "D|SOFIAWORLDBACKDROP|IFPRESENT", "F|SOFIA|WORRIED", "T|Sofia|Well, it's a cave." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|EXCITED", "T|Sofia|Awesome, I love caves!" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "T|Sofia|Need to get me some more caves in my life, just, generally." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|HAPPY", "T|Sofia|Is this where the Mystic Sofia- Hello?" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|HEWWO???" },
                        new object[] { "C|GWS:CONTINUE", "M|CRIMINAL|TRUE", "A|CRIME SHACK|FADEINLONG", "T|???|...hewwo?", new VoidDel(delegate() { Sofia.CrookedFlag = 1; }) },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "T|Sofia|Um-" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|UNIMPRESSED", "A|CROOKED SOFIA|-600=0,1500,20||||", "T|Crooked Sofia|Why hewwo there indeed, my good fresh-faced stranger. ;)" },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|How're'yuh doing on this fine [R]TIME_VARIABLE_NULL[C:WHITE], if I might be askin'?" },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|Perhaps in the market for some... essence?", new VoidDel(delegate() { Sofia.CrookedFlag = 2; }) },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Oh God." },
                        new object[] { "C|TIME:500:ORSKIP", "F|SOFIA|WORRIED", "T|Sofia|Are *you* the Mystic Sofia?" },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|GRINNING" },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|Am- am I being the WHO now?" },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|THINKING", "T|Crooked Sofia|..." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|DOWNCAST", "T|Crooked Sofia|Why, who might be askin'?" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Well, uh, me?" },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|HAPPY", "T|Crooked Sofia|And who're'you being, sis?" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|I'm- well." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "T|Sofia|Okay, so, I got brought here through a portal by a mysterious clone of myself..." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|WORRIED", "F|SOFIA|CONSIDERING", "T|Sofia|...and then I was told that I had to, like, save the world or something..." },
                        new object[] { "C|TIME:1300:ORSKIP", "F|SOFIA|GRINNING", "T|Sofia|...so now I'm on a mission for this King, and-" },
                        new object[] { "C|TIME:400:ORSKIP", "F|CROOKED SOFIA|UNIMPRESSED", },
                        new object[] { "B|SOFIA_CROOKED_SECONDARY_BODY" },
                    };
                    break;
                case "SOFIA_CROOKED_SECONDARY_KNOWS_MYSTIC":
                    Script = new object[]
                    {
                        new object[] { "C|GWS:CONTINUE", "M|#NULL", "T||Ah. The wonderful beauty that is [C:138-0-255-255]ULTRASOFIAWORLD[C:WHITE].", new VoidDel(delegate()
                        {
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                WorldEntity Add = new WorldEntity("SOFIA", new Vector2(350, 405), (TAtlasInfo)Shell.AtlasDirectory["SOFIASPRITES"], 0.5f);
                                Add.ColourValue = new Color(0,0,0,0);
                                Add.CenterOrigin = true;
                                Add.SetAtlasFrame(new Point(2, 0));
                                Add.AnimationQueue.Add(Animation.Retrieve("FADEIN"));
                                Add.Scale(new Vector2(-0.06f, -0.06f));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                                Add = new WorldEntity("SOFIAWORLDBACKDROP", new Vector2(-600, 0), (TAtlasInfo)Shell.AtlasDirectory["SWBG"], 0.05f);
                                Add.ColourValue = new Color(0,0,0,0);
                                Add.AnimationQueue.Add(Animation.Retrieve("FADEIN"));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                            }));
                        })
                        },
                        new object[] { "C|GWS:CONTINUE", "T||Never before have you gazed upon a landscape so incredible. So amazing. So enticing." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|GRINNING", "T||So much so that you're perfectly happy to just skip away with reckless abandon into a part of it that you haven't visited before, even though you know perfectly well that you should be visiting the Mystic Sofia right now." },
                        new object[] { "C|GWS:CONTINUE", "T||Such is the arcane whimsy of the Sofia. It is your brand." },
                        new object[] { "C|GWS:CONTINUE", "A|SOFIAWORLDBACKDROP|FADEOUT", "T||You contemplate several things that are also your brand. Such as descending into mysterious valleys between hills." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|UNIMPRESSED", "T||The MAP describes this as the LOWLANDS, which sounds perfect for you, because you are of course very short." },
                        new object[] { "C|GWS:CONTINUE", "T||Get rekt! The narrator congratulates you on managing to locate this surprisingly obscure branch of the dialogue tree just to suffer that magnifcent and well-implemented burn." },
                        new object[] { "C|GWS:CONTINUE", "T||But regardless, you get an opportunity to get your dwarfsona on further when you round a corner and see a cave entrance.", new VoidDel(delegate()
                        {
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                WorldEntity Add = new WorldEntity("CROOKEDCAVEBG", new Vector2(0, -160), (TAtlasInfo)Shell.AtlasDirectory["CROOKEDCAVEBG"], 0.05f);
                                Add.ColourValue = new Color(0,0,0,0);
                                Add.AnimationQueue.Add(Animation.Retrieve("FADEINLONG"));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                                Add = new WorldEntity("CRIME SHACK", new Vector2(850, 300), (TAtlasInfo)Shell.AtlasDirectory["CRIMESHACK"], 0.46f);
                                Add.ColourValue = new Color(0,0,0,0);
                                Add.CenterOrigin = true;
                                Add.Scale(new Vector2(-0.2f, -0.2f));
                                Shell.RenderQueue.Add(Add);
                                Shell.UpdateQueue.Add(Add);
                                Add = new WorldEntity("CROOKED SOFIA", new Vector2(1430, 405), (TAtlasInfo)Shell.AtlasDirectory["CROOKEDSOFIA"], 0.48f);
                                Add.CenterOrigin = true;
                                Add.SetAtlasFrame(new Point(3, 1));
                                Add.Scale(new Vector2(-0.06f, -0.06f));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                            }));
                        })
                        },
                        new object[] { "C|GWS:CONTINUE", "D|SOFIAWORLDBACKDROP|IFPRESENT", "F|SOFIA|WORRIED", "T|Sofia|Huh, it's a cave entrance." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "T|Sofia|Normally I would comment on how much I love caves, but this one emits... an aura." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|CONSIDERING", "T|Sofia|Hmmmmmmmmmmmmmm." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|DOWNCAST", "T|Sofia|Well hey, I know what would make me feel better." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|LAUGHING", "T|Sofia|Exploring a strange, dark, dank cave!" },
                        new object[] { "C|TIME:800:ORSKIP", "A|CRIME SHACK|FADEINLONG", "T|Sofia|Let's gooooooooooooooooooo- oh." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|UNIMPRESSED" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|...well then." },
                        new object[] { "C|GWS:CONTINUE", "M|CRIMINAL|TRUE", "T|???|Hey.", new VoidDel(delegate() { Sofia.CrookedFlag = 1; }) },
                        new object[] { "C|GWS:CONTINUE", "T|???|Psst. [T:300]Hey kid." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|UNIMPRESSED", "A|CROOKED SOFIA|-600=0,1500,20||||", "T|Crooked Sofia|Wanna buy some [R]ESSENCE[C:WHITE]?", new VoidDel(delegate() { Sofia.CrookedFlag = 2; }) },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Oh no. Who are you then, Pirate Sofia?" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|CONSIDERING", "T|Sofia|(...double Pirate Sofia?)" },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|HAPPY", "T|Crooked Sofia|Me? Oh, I'm nobody important, Sister." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "F|CROOKED SOFIA|LAUGHING", "T|Crooked Sofia|Just another honest tradeswoman tryin'tuh drum up some business in these trying times." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|HAPPY", "T|Crooked Sofia|In the mood for some [R]ESSENCE[C:WHITE]? I have good stuff. Mine's the best you'll find on this side of ULTRASOFIAWORLD." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "T|Crooked Sofia|Really hits the spot if you know what I mean, Sister. Top quality [R]ESSENCE[C:WHITE]." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|I, uh. Um." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "F|CROOKED SOFIA|LAUGHING", "T|Sofia|Are you trying to... sell me drugs?" },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|Hah hah, hells no, Sister! Do you take me for some kinda unscrupulous businesser?" },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|EXCITED", "T|Crooked Sofia|Not to knock on you if that's your groove, 'coursewise." },
                        new object[] { "C|TIME:500:ORSKIP", "T|Sofia|Uh-" },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|HAPPY", "T|Crooked Sofia|Nah, what I have is all kinds of wicked better. Can't shine a light to any DRUGS you could find topside." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|DOWNCAST", "T|Crooked Sofia|Pure distilled [R]SOFIA ESSENCE[C:WHITE], fresh as twilight, as straight from the [R]SOURCE[C:WHITE]." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|LAUGHING", "T|Crooked Sofia|Has a real kick to it." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "T|Sofia|Right. Okay, all that aside, I, uh-" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|I'm fairly sure that I shouldn't be taking drugs right now." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|GRINNING", "T|Crooked Sofia|Ah, well, they all say that sister, course they do." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|EXCITED", "T|Crooked Sofia|But they also are being saying that before giving my wares a try, hear. ;)" },
                        new object[] { "C|TIME:1600:ORSKIP", "T|Sofia|Well, um, no, you see. I'm meant to be- I'm on this mission from this KING, and-" },
                        new object[] { "C|TIME:400:ORSKIP", "F|CROOKED SOFIA|UNIMPRESSED", },
                        new object[] { "B|SOFIA_CROOKED_SECONDARY_BODY" }
                    };
                    break;
                case "SOFIA_CROOKED_SECONDARY_BODY":
                    Script = new object[]
                    {
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|Wait now, the King sent you?", new VoidDel(delegate() { Sofia.ParanoidFlag = 1; }) },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "T|Crooked Sofia|Are you taking me for a fool, hear?" },
                        new object[] { "C|TIME:1200:ORSKIP", "T|Sofia|Well, you see-" },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|No, I won't be hearing it." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|DOWNCAST", "T|Crooked Sofia|At least you were being foolhardy enough to let me know what you were about, Sister." },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|I was about'tuh sell you some product and all." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|LAUGHING", "T|Crooked Sofia|But in my line of work we know better than to stay around to be prey to [C:138-0-255-255]KINGSWOMEN[C:WHITE]." },
                        new object[] { "C|TIME:3000:ORSKIP", "A|CROOKED SOFIA|FADEOUTLONG", "A|CRIME SHACK|FADEOUTLONG", "F|CROOKED SOFIA|UNIMPRESSED", "T|Crooked Sofia|Catch you later, Lawgirl." },
                        new object[] { "C|GWS:CONTINUE", "T||..." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|I-" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|UNIMPRESSED", "T|Sofia|...she's gone." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "T|Sofia|Well,  that was bizarre." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|I guess she was selling more of that... ESSENCE?" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|This place is weird." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Well, I guess I should head somewhere else now, now that I've exhausted this particularly esoteric avenue." },
                        new object[] { "C|GWS:impos_mapnavigate", "T||Use the [C:138-0-255-255]MAP[C:WHITE] to navigate to your next destination." },
                    };
                    break;
                case "SOFIA_CROOKED_SECONDARY_WANT_ESSENCE":
                    Script = new object[]
                    {
                        new object[] { "C|GWS:CONTINUE", "M|#NULL", "T||So, it seems that the name of the game here is [C:138-0-255-255]ESSENCE[C:WHITE].", new VoidDel(delegate()
                        {
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                WorldEntity Add = new WorldEntity("SOFIA", new Vector2(350, 405), (TAtlasInfo)Shell.AtlasDirectory["SOFIASPRITES"], 0.5f);
                                Add.ColourValue = new Color(0,0,0,0);
                                Add.CenterOrigin = true;
                                Add.SetAtlasFrame(new Point(2, 0));
                                Add.AnimationQueue.Add(Animation.Retrieve("FADEIN"));
                                Add.Scale(new Vector2(-0.06f, -0.06f));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                                Add = new WorldEntity("SOFIAWORLDBACKDROP", new Vector2(-600, 0), (TAtlasInfo)Shell.AtlasDirectory["SWBG"], 0.05f);
                                Add.ColourValue = new Color(0,0,0,0);
                                Add.AnimationQueue.Add(Animation.Retrieve("FADEIN"));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                            }));
                        })
                        },
                        new object[] { "C|GWS:CONTINUE", "T||You need to get yourself some more [C:138-0-255-255]SOFIA ESSENCE[C:WHITE]. Well, oh boy." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|LAUGHING", "T||You are going to get yourself *so much* [C:138-0-255-255]ESSENCE[C:WHITE]. That King and Coolface-McGlasses won't even know what hit them." },
                        new object[] { "C|GWS:CONTINUE", "T||But what will have hit them will have been you. With the [C:138-0-255-255]ESSENCE[C:WHITE]! So much [C:138-0-255-255]ESSENCE[C:WHITE] that you are sure you'll be twice the [C:138-0-255-255]SOFIA[C:WHITE] by the time this is over. Even after they, uh, extract it from you, or whatever." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "T||You still hope that that isn't going to hurt." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|GRINNING", "T||But regardless! You are going to completely smash this weird dimension quest. You just know you are!" },
                        new object[] { "C|TIME:2200:ORSKIP", "A|SOFIAWORLDBACKDROP|FADEOUT", "T||Just like you know that all the essence you could possibly need is going to be somewhere in this weird rocky valley here that you are now enteri- OHSHIT" },
                        new object[] { "C|TIME:4000:ORSKIP", "A|SOFIA|FALLSHOCK", "T||!" },
                        new object[] { "C|GWS:CONTINUE", "T||Shit, you slipped." },
                        new object[] { "C|GWS:CONTINUE", "T||Well, at the very least..." },
                        new object[] { "C|GWS:CONTINUE", "T||You've landed in the entrance to a cave. So, that's a plus.", new VoidDel(delegate()
                        {
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                WorldEntity Add = new WorldEntity("CROOKEDCAVEBG", new Vector2(0, -160), (TAtlasInfo)Shell.AtlasDirectory["CROOKEDCAVEBG"], 0.05f);
                                Add.ColourValue = new Color(0,0,0,0);
                                Add.AnimationQueue.Add(Animation.Retrieve("FADEINLONG"));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                                Add = new WorldEntity("CRIME SHACK", new Vector2(850, 300), (TAtlasInfo)Shell.AtlasDirectory["CRIMESHACK"], 0.46f);
                                Add.ColourValue = new Color(0,0,0,0);
                                Add.CenterOrigin = true;
                                Add.Scale(new Vector2(-0.2f, -0.2f));
                                Shell.RenderQueue.Add(Add);
                                Shell.UpdateQueue.Add(Add);
                                Add = new WorldEntity("CROOKED SOFIA", new Vector2(1430, 405), (TAtlasInfo)Shell.AtlasDirectory["CROOKEDSOFIA"], 0.48f);
                                Add.CenterOrigin = true;
                                Add.SetAtlasFrame(new Point(3, 1));
                                Add.Scale(new Vector2(-0.06f, -0.06f));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                            }));
                        })
                        },
                        new object[] { "C|GWS:CONTINUE", "D|SOFIAWORLDBACKDROP|IFPRESENT", "F|SOFIA|WORRIED", "T|Sofia|Huh, it's a cave entrance." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|UNIMPRESSED", "T|Sofia|This world continues to be full of exciting new surprises for me." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|CONSIDERING", "T|Sofia|But hey, I can spelunk if I need to. [T:300]I have been known to spelunk." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Spelunkation is an activity that is within my skillset." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|GRINNING", "T|Sofia|I'm sure this mysterious cave is exactly where I need to be!" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|HAPPY", "T|Sofia|In fact, I'm sure the person that I need is going to run into me in just a moment, and offer me all the essence that I could ever be looking for!" },
                        new object[] { "C|GWS:CONTINUE", "A|CRIME SHACK|FADEINLONG", "F|SOFIA|WORRIED", "T|Sofia|..." },
                        new object[] { "C|GWS:CONTINUE", "M|CRIMINAL|TRUE", "T|???|Hey." },
                        new object[] { "C|GWS:CONTINUE", "T|???|Psst. [T:300]Hey kid." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|UNIMPRESSED", "A|CROOKED SOFIA|-500=0,1500,20||||", "T|Crooked Sofia|Wanna buy some [R]ESSENCE[C:WHITE]?", new VoidDel(delegate() { Sofia.CrookedFlag = 3; }) },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|..." },
                        new object[] { "C|TIME:1000:ORSKIP", "F|SOFIA|EXCITED", "T|Sofia|You know what? I rather think I do." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|GRINNING" },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|Now an enthusiastic customer'er is what I'm liking to see!" },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|Why nots come on in and let's make ourselves acquaintancewise, Sis?" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|HAPPY", "T|Sofia|Yep, let's do that! Let's do this thing, that I totally normally do, and acquaint myself with, uh, underworld criminals in caves?" },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|LAUGHING", "T|Crooked Sofia|Come now! Where'ya'be getting that idea? [T:300]I am nothing but a trustworthy, innovative young entrepreneur." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|EXCITED", "T|Crooked Sofia|Last I checked, good business sense isn't a crime, Sister. ;)" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "T|Sofia|Well, I mean, your stall there does literally say \"Crime Shack\" on it, so, I mean..." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|LAUGHING", "T|Sofia|I guess I was drawing conclusions... based on that?" },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|Haha, pay it no mind, pay it no mind, my all-too-literate friend, my protege, you. Hah!" },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|Just a turn of phrase, a turn of phrase it is." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|CONSIDERING", "T|Crooked Sofia|I mean yes, now and then, the precurement of the true pure [R]SOFIA ESSENCE[C:WHITE] does necessitate some under-the-table dealings, ofcoursewise." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|EXCITED", "T|Crooked Sofia|But that is just the Way Of The World, is it not, Sister?" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|..." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|...If you say so." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|HAPPY", "T|Sofia|So, you say you have some... ESSENCE?" },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|GRINNING", "T|Crooked Sofia|Ha, *do I* have [R]ESSENCE[C:WHITE]?" },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|Only just. Only just the VERY BEST, Sister." },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|The only option for the discerning consumer such'as yourself, I'm thinking. I'm feeling." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|LAUGHING", "F|SOFIA|UNIMPRESSED", "T|Crooked Sofia|My stuff is the real top quality shit. High grade [R]ESSENCE[C:WHITE]. The best." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Wait, are you selling this as like, a drug?" },
                        new object[] { "C|TIME:800|ORSKIP", "F|CROOKED SOFIA|WORRIED" },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|Are you sure you're bein' knowing what I'm about, kid?" },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|Are you sure you're *ready* for my top quality, top tier product?" },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|..." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|GRINNING", "T|Crooked Sofia|BUT WHAT AM I SAYING! There're always being times for a first time, hear! A first tasting of [R]ESSENCE[C:WHITE]!" },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|EXCITED", "T|Crooked Sofia|And you will love it, yes? Course'ya'will. I'm the best place to start. The best [R]ESSENCE[C:WHITE] this side of [C:138-0-255-255]ULTRASOFIAWORLD[C:WHITE]." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "T|Crooked Sofia|In fact, listen'up, here. Tell'ya what." },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|I'm down to give you a TASTER. A little freebie, on the house." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|GRINNING", "T|Crooked Sofia|Let you get'a feel for my wares!" },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|HAPPY", "T|Crooked Sofia|Whadduya say?" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|I..." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|GRINNING", "T|Sofia|Think that that's just what I need!" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|UNIMPRESSED", "T|Sofia|(*coughcough* ...for my current mission only. *coughcough*)" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|GRINNING", "T|Crooked Sofia|Wonderful! Wonderful! Just reach out and be taking my hand, Sister." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|GRINNING", "T|Crooked Sofia|That'll be getting the transfer rolling. ;)" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "T|Sofia|..." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Okay..." },
                        new object[] { "C|GWS:CONTINUE", "T||You reach out and take the CROOKED SOFIA's hand." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|EXCITED", "T|Crooked Sofia|Excellent choice! Now hold still." },
                        new object[] { "C|TIME:2000:ORSKIP", "F|CROOKED SOFIA|DOWNCAST", "T||" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "T|Sofia|Uh, I don't feel anythin-" },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|GRINNING", "F|SOFIA|JUDGING", "T|Sofia|[R]Whoa.", new VoidDel(delegate()
                        {
                            Sofia.CrookedFlag = 4;
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                WorldEntity Add = new Sofia.EssenseGlow("ESSENCEGLOW", new Vector2(0, 0), (TAtlasInfo)Shell.AtlasDirectory["ESSENCEGLOW"], 0.6f);
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                            }));
                        })
                        },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|Told ya." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|[R]I feel... glowing." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|[R]Like I'm..." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|[R]Somehow, even more Sofia-like than I was before. Huh!" },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|Hah! Welcome to the wonders of [R]SOFIA ESSENCE[C:WHITE]. ;)" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|[R]I can see why they said this world is wanting for more of it..." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Wait, it's fading...", new VoidDel(delegate()
                        {
                            Sofia.EssenseGlow EG = (Sofia.EssenseGlow)Shell.GetEntityByName("ESSENCEGLOW");
                            EG.AnimationQueue.Add(Animation.Retrieve("fadeoutcolourpreserve"));
                            EG.TransientAnimation = true;
                        })
                        },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "D|ESSENCEGLOW|IFPRESENT", "T|Sofia|Huh. What happened?" },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|HAPPY", "T|Crooked Sofia|Good things can't be lasting forever. The initial effects fade quickwise." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "F|CROOKED SOFIA|EXCITED", "T|Crooked Sofia|But that essence is part of you now." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|I see..." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|CONSIDERING", "F|CROOKED SOFIA|LAUGHING", "T|Sofia|Well, that certainly was something." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|HAPPY", "T|Crooked Sofia|I bet you it was, Sis." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "T|Sofia|And you made this search for essence a... lot easier than I thought it was going to be." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|I thought this stuff was meant to be, uh, harder to find?" },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|Maybe for some people, but I have connections in high *and* low places, hear." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|EXCITED", "T|Crooked Sofia|CROOKED SOFIA has hookups for *all* the best stuff, Sister." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "F|CROOKED SOFIA|HAPPY", "T|Crooked Sofia|Trust me. I'm a businesswoman." },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|But now I've gone and helped you out all, you gotta tell me, Sis, what is your deal?" },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|GRINNING", "T|Crooked Sofia|Now I've gone and got myself a proper feel for you, I'm thinkin' that I'd venturewise that you'n't the type who commonly comes to my humble abode for an ESSENCE fix." },
                        new object[] { "B|SOFIA_CROOKED_SECONDARY_FINALIZE" }
                    };
                    break;
                case "SOFIA_CROOKED_SECONDARY_FINALIZE":
                    Script = new object[]
                    {
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|DOWNCAST", "T|Sofia|I mean, well. [T:400]Maybe so." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|LAUGHING", "T|Crooked Sofia|Come now Sister, you can't be playin' coy with me, now, no." },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|A handshake goes two ways! I've had a feel of your [R]ESSENCE[C:WHITE] now." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|GRINNING", "T|Crooked Sofia|I don't mean to go about and brag, but you see, I'm good at tasting the essence, hear? To find the best strains and sort the good stuff from the bad, rightwise as it should be. ;)" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "T|Crooked Sofia|I'm fairsure I could maybe be dancing the tango with the [C:138-0-255-255]MYSTIC SOFIA[C:WHITE] herself on that one." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|About that..." },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|Hold now, broskino. [T:200]I want to air my findings, because I've got me an inkling of a unique situation here, methinks." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|CONSIDERING", "T|Crooked Sofia|You don't have the [R]ESSENCE[C:WHITE] of one from around these parts. In fact... I have a feeling that perhaps your parts might be adjacent to ours entirely." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Okay, you got me." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|GRINNING", "T|Sofia|I'm an interdimensional traveller here to save the world through the power of my unconquerable aura.", new VoidDel(delegate() { Sofia.CrookedKnowledgeFlag = 1; }) },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|..." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "T|Crooked Sofia|Shiiiiiiiiiiiiiiiiit." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|LAUGHING", "T|Crooked Sofia|You're actually being telling the truth, now, aren't you?" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|You... actually believe me?" },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|I wouldn't, but your [R]ESSENCE[C:WHITE] rings true! I'm'all'n getting allkinds of vibes veritas, Sister." },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|I might be in the presence of a real-life alien world saver." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|HAPPY", "T|Sofia|Well, when you put it that way." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "T|Sofia|But yes, uhh..." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|I was brought here from another dimension, because I have the power to restore the... essence?" },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|[R]ESSENCE[C:WHITE]." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Right. To restore the full [R]ESSENCE[C:WHITE] to this land through the power of my unique Prime Sofia Aura, and you just helped me by giving me... some sort of... extra essence shot?" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|UNIMPRESSED", "T|Sofia|Honestly when I say it like that this whole insane narrative sounds even more absurd and contrived." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|GRINNING", "T|Crooked Sofia|It is surewise making sense to me, Sister! But then it is in my character that I'm almost always high. ;)" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|HAPPY", "T|Crooked Sofia|And this keeps getting better! Seems I helped some too." },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|You better get back to that quest of yours mighty quicksorts, hear?" },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|HAPPY", "F|SOFIA|THINKING", "T|Sofia|Wait, but, hold on. I'm bringing back the essence, but, like, I thought you were profiting from the shortfall?" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "T|Sofia|On second thoughts, I hope that that doesn't turn you against my cause. Whoops." },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|Naw. You think they'll ease up regulation after all these years of keeping it locked up? And besides, even I'm running dry these days. There more essence there is, the more there is for me to steal!" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|EXCITED", "F|CROOKED SOFIA|GRINNING", "T|Crooked Sofia|Uh, that is to say... Acquire through innovative business practices." },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|But, anyway, I think you'll be doing a true righteous thing pulling this little world of ours out of the proverbial gutter, hear." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Oh, well, I'm glad to hear!" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "T|Sofia|And I should probably get back to it, shouldn't I? Now that I've got more essence, I mean." },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|Be on your way, Sisterino. Always good to meet new folks and do good business. Glad to have been of service, no thanks required." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|GRINNING", "T|Crooked Sofia|Just remember dear old CROOKED SOFIA when you make the bigtime now, hey? Always good to have contacts. ;)" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|DOWNCAST", "T||The Crooked Sofia sweeps you a dramatic bow." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Uh, yeah. Thanks." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|LAUGHING", "T|Crooked Sofia|I said no need, hear!" },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|HAPPY", "T|Crooked Sofia|But I'll be seeing you. ;)" },
                        new object[] { "C|GWS:impos_mapnavigate", "F|SOFIA|THINKING", "T||Use the [C:138-0-255-255]MAP[C:WHITE] to navigate to your next destination." },
                    };
                    break;
                case "SOFIA_CROOKED_RETURN_PARANOID":
                    Script = new object[]
                    {
                        new object[] { "C|GWS:CONTINUE", "M|#NULL", "T||You decide to pursue your nigh-unquenchable thirst for shenanigans by traveling to where that weird cave is again.", new VoidDel(delegate()
                        {
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                WorldEntity Add = new WorldEntity("SOFIA", new Vector2(350, 405), (TAtlasInfo)Shell.AtlasDirectory["SOFIASPRITES"], 0.5f);
                                Add.ColourValue = new Color(0,0,0,0);
                                Add.CenterOrigin = true;
                                Add.SetAtlasFrame(new Point(2, 0));
                                Add.AnimationQueue.Add(Animation.Retrieve("FADEIN"));
                                Add.Scale(new Vector2(-0.06f, -0.06f));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                                Add = new WorldEntity("SOFIAWORLDBACKDROP", new Vector2(-600, 0), (TAtlasInfo)Shell.AtlasDirectory["SWBG"], 0.05f);
                                Add.ColourValue = new Color(0,0,0,0);
                                Add.AnimationQueue.Add(Animation.Retrieve("FADEIN"));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                            }));
                        })
                        },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "T||To be fair, you are but a simple Sofia in a strange land. Sometimes the urge to pursue shenanigans overcome you, and besides, you're still not 100% sure where your ultimate goal lies at the moment, and it totally could be in that cave!" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|UNIMPRESSED", "T||Although, equally, you have to admit that you're fairly sure that it isn't at the moment. The only person there was that weird eyepatch Sofia, and she didn't seems so friendly." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|GRINNING", "T||But hey, what do you know? Who knows what mysteries she's hiding within her shady shack..." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "T||Wait, you're actually not sure you particularly want to know, regardless." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "A|SOFIAWORLDBACKDROP|FADEOUT", "T||But, what the heck. You're here now anyway, so it wouldn't make much sense to throw the towel in at this particular moment." },
                        new object[] { "C|GWS:CONTINUE", "T||And besides, you didn't actually pack a towel.", new VoidDel(delegate()
                        {
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                WorldEntity Add = new WorldEntity("CROOKEDCAVEBG", new Vector2(0, -160), (TAtlasInfo)Shell.AtlasDirectory["CROOKEDCAVEBG"], 0.05f);
                                Add.ColourValue = new Color(0,0,0,0);
                                Add.AnimationQueue.Add(Animation.Retrieve("FADEINLONG"));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                                Add = new WorldEntity("CRIME SHACK", new Vector2(850, 300), (TAtlasInfo)Shell.AtlasDirectory["CRIMESHACK"], 0.46f);
                                Add.ColourValue = new Color(0,0,0,0);
                                Add.CenterOrigin = true;
                                Add.Scale(new Vector2(-0.2f, -0.2f));
                                Shell.RenderQueue.Add(Add);
                                Shell.UpdateQueue.Add(Add);
                                Add = new WorldEntity("CROOKED SOFIA", new Vector2(830, 405), (TAtlasInfo)Shell.AtlasDirectory["CROOKEDSOFIA"], 0.48f);
                                Add.ColourValue = new Color(0,0,0,0);
                                Add.CenterOrigin = true;
                                Add.SetAtlasFrame(new Point(3, 1));
                                Add.Scale(new Vector2(-0.06f, -0.06f));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                            }));
                        })
                        },
                        new object[] { "C|GWS:CONTINUE", "D|SOFIAWORLDBACKDROP|IFPRESENT", "F|SOFIA|WORRIED", "T|Sofia|So, here we are again, huh?" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Hello? Is anyone there?" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Spooky eyepatch Sofia? Hello?" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|DOWNCAST", "T|Sofia|Hmm." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "T|Sofia|Maybe if I walk a little further in..." },
                        new object[] { "C|GWS:CONTINUE", "A|CRIME SHACK|FADEINLONG", "F|SOFIA|WORRIED", "T|Sofia|Well, there's the world's most uninviting storefront again." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Yo! Eyepatch, if you're here, you're leaving your store unattended!" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|UNIMPRESSED", "T|Sofia|And that's a poor business practice." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "T|???|*mumbling*" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|He- hewwo?" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Hewwo, aw youw thewe? Yo I can't hear you." },
                        new object[] { "C|TIME:800:ORSKIP", "T|Sofia|Are you-" },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|JUDGING", "F|SOFIA|UNIMPRESSED", "A|CROOKED SOFIA|BLINKIN", "A|SOFIA|FALLSHOCK", "T|Crooked Sofia|BOOOOOOOO!" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "M|CRIMINAL|TRUE", "T|Sofia|AAAAAH! W- WHAT THE HELL?" },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|BEGONE, THOT!" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|I... You... What??" },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|I SAID BEGONE! GO AWAY!" },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|AGENTS OF THE KING ARE NOT WELCOME HERE!" },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|UNIMPRESSED", "T|Crooked Sofia|I mean, seriously, yo. Get out of my business, hear?" },
                        new object[] { "C|TIME:1000:ORSKIP", "T|Sofia|But I'm not-" },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|JUDGING", "T|Crooked Sofia|BUP!" },
                        new object[] { "C|TIME:1000:ORSKIP", "T|Sofia|But I..." },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|BU-YUP! I SAID LEAVE." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|UNIMPRESSED", "T|Crooked Sofia|This is my place, Sister. Like, for real." },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|I don't have time for people working on the King to be snoopwise up in my cool zone." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|This is not a cool zone! And I'm not a... I mean, I'm..." },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|Whatever, yo. I said my piece. Nothing for you here right nowish, hear?" },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|DOWNCAST", "T|Crooked Sofia|Get scramwise." },
                        new object[] { "C|GWS:CONTINUE", "A|CROOKED SOFIA|FADEOUT", "T|Sofia|I... [T:500]Oh." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|UNIMPRESSED", "T|Sofia|Well that didn't go well." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "T|Sofia|Guess I better get scramwise, at least for now." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|(I could try here again later, maybe?)" },
                        new object[] { "C|GWS:impos_mapnavigate", "T||Use the [C:138-0-255-255]MAP[C:WHITE] to navigate to your next destination." },
                    };
                    break;
                case "SOFIA_CROOKED_RETURN_CLUELESS":
                    Script = new object[]
                    {
                        InitCrookedBasic,
                        new object[] { "C|GWS:CONTINUE", "M|CRIMINAL|TRUE", "T|Sofia|Hello?" },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|LAUGHING", "A|CROOKED SOFIA|FADEIN", "T|Crooked Sofia|Ah, what have we here, now?" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|HAPPY", "T|Sofia|It is the me." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|EXCITED", "T|Crooked Sofia|So I am rightwise seeing." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|GRINNING", "T|Crooked Sofia|My new prodigal acquaintance returns." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Sofia in the flesh..." },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|Hah, we are all Sofia here, now." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|LAUGHING", "T|Crooked Sofia|But I will grant you, your havin' your own uniqueness Sofiawise." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|GRINNING", "T|Sofia|Damn straight!" },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|EXCITED", "T|Crooked Sofia|So, how might I be bein' helping you this fine eternity?" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "T|Sofia|I was hoping you could help me with that?" },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|Ah, a me helping-you-help-yourself situation." },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|The CROOKED SOFIA may be able to provide, if it can be worth my time, hear." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|HAPPY", "T|Crooked Sofia|But you're'gunna need to be giving me some specifics, Sister." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|That's a little difficult." },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|Oh? Speak more, yo." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|CONSIDERING", "T|Sofia|I'm really not sure why I'm here or what to do next." },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|I can't do much more than make you offers. I'm not much of a questmistress." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|GRINNING", "T|Crooked Sofia|Are you perhaps bein' in need of any of my brand of esoteric offerin's?" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|UNIMPRESSED", "T|Crooked Sofia|Perhaps in needs of some goods or services... beyond the norm? The CROOKED SOFIA can do you well for things you can't get elsewherewise, hear." },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|Need to access an [R]ESSENCE[C:WHITE] hookup? Or perhaps some... more esoteric services. ;)" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|I think I'm good at the moment, thanks." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "T|Sofia|Maybe just some advice on where to go next would help." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|HAPPY", "T|Crooked Sofia|Aw. Well, you've'got'cha map there, Sister." },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|I can't be offerin' you much more than to say, follow it around, talk to who you're thinkin' you need to talk to." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|GRINNING", "T|Crooked Sofia|That's straightforward enough that I'll give you it free of charge, hear?" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Fair enough, I guess." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|DOWNCAST", "T|Sofia|I'll head off then." },
                        new object[] { "C|GWS:impos_mapnavigate", "T||Use the [C:138-0-255-255]MAP[C:WHITE] to navigate to your next destination." },
                    };
                    break;
                case "SOFIA_CROOKED_RETURN_NO_ESSENCE":
                    Script = new object[]
                    {
                        InitCrookedBasic,
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Hello?" },
                        new object[] { "C|GWS:CONTINUE", "M|CRIMINAL|TRUE", "F|CROOKED SOFIA|LAUGHING", "A|CROOKED SOFIA|FADEIN", "T|Crooked Sofia|Ah, what have we here, now?" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|HAPPY", "T|Sofia|It is the me." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|EXCITED", "T|Crooked Sofia|So I am rightwise seeing." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|GRINNING", "T|Crooked Sofia|My new potential customer returns." },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|How might I be helping you?" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "T|Sofia|You sell... ESSENCE, right?" },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|[R]ESSENCE[C:WHITE]." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Right." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|EXCITED", "T|Crooked Sofia|You bet I do. The CROOKED SOFIA has *got what you need*." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "T|Crooked Sofia|The very best of the best. [R]PURE STRAIN ESSENCE[C:WHITE], as straight from the true [R]SOURCE[C:WHITE], hear." },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|You won't find elsewise quality anywhere else 'cross this here darkling plain, Sister." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|HAPPY", "T|Crooked Sofia|Again, save the [R]SOURCE[C:WHITE], or course." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|UNIMPRESSED", "T|Sofia|Okay, well, I'll reserve my judgement on those statements." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|EXCITED", "F|SOFIA|THINKING", "T|Sofia|But it just so happens that you actually *might* have what I need." },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|Oh ho! [T:300]Do that be rightwise, Sister?" },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|GRINNING", "T|Crooked Sofia|And here I was, the first time I was seeing you..." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|UNIMPRESSED", "T|Crooked Sofia|Thinking you mightn't quite be radicallike enough for my particular services." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Rude!" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|CONSIDERING", "T|Sofia|How dare you question my radness levels. [T:300]I'll have you know that they are practically unassailable." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|EXCITED", "T|Crooked Sofia|Rightly? ;D" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|HAPPY", "T|Sofia|You goddamn betcha rightly." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|GRINNING", "T|Sofia|My radness levels are so off-the-charts wild I could outrad Charles Radley on Radmusnight." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|His radness would pale compared to mine, you don't even know." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "T|Sofia|When it comes to radness, I am simply the best there is." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|GRINNING", "T|Crooked Sofia|Well okay then! Seems I was not knowing what company I was keeping, hear." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|HAPPY", "T|Crooked Sofia|Let it not fall upon me to denigrate the radness of such an individual as yourselfwise." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|HAPPY", "T|Crooked Sofia|So then, I take it you need some essence?" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "T|Crooked Sofia|In fact, listen'up, here. Tell'ya what." },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|Seeing as to your enthusiasm, and how I went and entirely underestimated your radness, like." },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|Put my right foot in it and all." },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|I'm down to give you a TASTER. A little freebie, on the house." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|GRINNING", "T|Crooked Sofia|Let you get'a feel for my wares!" },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|HAPPY", "T|Crooked Sofia|Whadduya say?" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|I..." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|GRINNING", "T|Sofia|Think that that's just what I need!" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|UNIMPRESSED", "T|Sofia|(*coughcough* ...for my current mission only. *coughcough*)" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|GRINNING", "T|Crooked Sofia|Wonderful! Wonderful! Just reach out and be taking my hand, Sister." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|GRINNING", "T|Crooked Sofia|That'll be getting the transfer rolling. ;)" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "T|Sofia|..." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Okay..." },
                        new object[] { "C|GWS:CONTINUE", "T||You reach out and take the CROOKED SOFIA's hand." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|EXCITED", "T|Crooked Sofia|Excellent choice! Now hold still." },
                        new object[] { "C|TIME:2000:ORSKIP", "F|CROOKED SOFIA|DOWNCAST", "T||" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "T|Sofia|Uh, I don't feel anythin-" },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|GRINNING", "F|SOFIA|JUDGING", "T|Sofia|[R]Whoa.", new VoidDel(delegate()
                        {
                            Sofia.CrookedFlag = 4;
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                WorldEntity Add = new Sofia.EssenseGlow("ESSENCEGLOW", new Vector2(0, 0), (TAtlasInfo)Shell.AtlasDirectory["ESSENCEGLOW"], 0.6f);
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                            }));
                        })
                        },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|Told ya." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|[R]I feel... glowing." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|[R]Like I'm..." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|[R]Somehow, even more Sofia-like than I was before. Huh!" },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|Hah! Welcome to the wonders of [R]SOFIA ESSENCE[C:WHITE]. ;)" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|[R]I can see why they said this world is wanting for more of it..." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Wait, it's fading...", new VoidDel(delegate()
                        {
                            Sofia.EssenseGlow EG = (Sofia.EssenseGlow)Shell.GetEntityByName("ESSENCEGLOW");
                            EG.AnimationQueue.Add(Animation.Retrieve("fadeoutcolourpreserve"));
                            EG.TransientAnimation = true;
                        })
                        },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "D|ESSENCEGLOW|IFPRESENT", "T|Sofia|Huh. What happened?" },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|HAPPY", "T|Crooked Sofia|Good things can't be lasting forever. The initial effects fade quickwise." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "F|CROOKED SOFIA|EXCITED", "T|Crooked Sofia|But that essence is part of you now." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|I see..." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|CONSIDERING", "F|CROOKED SOFIA|LAUGHING", "T|Sofia|Well, that certainly was something." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|HAPPY", "T|Crooked Sofia|I bet you it was, Sis." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "T|Sofia|And you made this search for essence a... lot easier than I thought it was going to be." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|I thought this stuff was meant to be, uh, harder to find?" },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|Maybe for some people, but I have connections in high *and* low places, hear." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|EXCITED", "T|Crooked Sofia|CROOKED SOFIA has hookups for *all* the best stuff, Sister." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "F|CROOKED SOFIA|HAPPY", "T|Crooked Sofia|Trust me. I'm a businesswoman." },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|But now I've gone and helped you out all, you gotta tell me, Sis, what is your deal?" },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|GRINNING", "T|Crooked Sofia|Now I've gone and got myself a proper feel for you, I'm thinkin' that I'd venturewise that you'n't the type who commonly comes to my humble abode for an ESSENCE fix..." },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|Radness notwithstanding. ;)" },
                        new object[] { "B|SOFIA_CROOKED_SECONDARY_FINALIZE" }
                    };
                    break;
                case "SOFIA_CROOKED_RETURN_NO_ESSENCE_PARANOID":
                    Script = new object[]
                    {
                        new object[] { "C|TIME:0", "M|#NULL", new VoidDel(delegate()
                        {
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                WorldEntity Add = new WorldEntity("SOFIA", new Vector2(350, 405), (TAtlasInfo)Shell.AtlasDirectory["SOFIASPRITES"], 0.5f);
                                Add.ColourValue = new Color(0,0,0,0);
                                Add.CenterOrigin = true;
                                Add.SetAtlasFrame(new Point(2, 0));
                                Add.Scale(new Vector2(-0.06f, -0.06f));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                                Add = new WorldEntity("CROOKEDCAVEBG", new Vector2(0, -160), (TAtlasInfo)Shell.AtlasDirectory["CROOKEDCAVEBG"], 0.05f);
                                Add.ColourValue = new Color(0,0,0,0);
                                Add.AnimationQueue.Add(Animation.Retrieve("FADEINLONG"));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                                Add = new WorldEntity("CRIME SHACK", new Vector2(850, 300), (TAtlasInfo)Shell.AtlasDirectory["CRIMESHACK"], 0.46f);
                                Add.ColourValue = new Color(0,0,0,0);
                                Add.CenterOrigin = true;
                                Add.Scale(new Vector2(-0.2f, -0.2f));
                                Shell.RenderQueue.Add(Add);
                                Shell.UpdateQueue.Add(Add);
                                Add = new WorldEntity("CROOKED SOFIA", new Vector2(930, 405), (TAtlasInfo)Shell.AtlasDirectory["CROOKEDSOFIA"], 0.48f);
                                Add.ColourValue = new Color(0,0,0,0);
                                Add.CenterOrigin = true;
                                Add.SetAtlasFrame(new Point(3, 1));
                                Add.Scale(new Vector2(-0.06f, -0.06f));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                            }));
                        })
                        },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "T||Returning to the cave, you find the air eerily still, and the cave entrance, ringed in stone that muffles any and all incoming sound, is otherworldly silent." },
                        new object[] { "C|GWS:CONTINUE", "A|SOFIA|FADEIN", "T||Then again, you suppose that seeing as you actually *are* in another world, perhaps that's the sort of thing you should expect." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|CONSIDERING", "T||Maybe another turn of phrase is required to convey the eeriness of your new surroundings. Perhaps..." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|DOWNCAST", "T||You decide that the cave is in fact normal-worldly silent. [T:400]You decide to add to that by silently contemplating the matter, only the faint echo of a distant breeze intruding." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "T||That is, until you suddenly hear a slight shuffle from within the cave, the *TOCK* of a rock falling on the ground, and what sounds almost like a very muffled expletive." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|...hello?" },
                        new object[] { "C|GWS:CONTINUE", "T|???|*shuffleshuffleshuffle*" },
                        new object[] { "C|GWS:CONTINUE", "A|CRIME SHACK|FADEINLONG", "T||You walk a little bit into the cave, catching sight of the CROOKED SOFIA's shack." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Uh, hello? Where are you..." },
                        new object[] { "C|GWS:CONTINUE", "T|???|(gehway)" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|UNIMPRESSED", "T|Sofia|Uh, sorry, what? What did you say?" },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|(i said go away, hear?)" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Uh, sorry, no can do." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Wait, are you behind that rock?" },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|(what? no. no, i said go a-)" },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|UNIMPRESSED", "A|CROOKED SOFIA|FADEIN", "T|Crooked Sofia|...way" },
                        new object[] { "C|GWS:CONTINUE", "M|CRIMINAL|TRUE", "F|CROOKED SOFIA|JUDGING", "T|Crooked Sofia|C'mon, now! I said to get yourself away, hear?" },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|I can't be busying myself with royal agents. All kind'sa bad for business." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|UNIMPRESSED", "T|Crooked Sofia|I thought I was making that clear last time you came my ways, Sister. [T:400]Can you not at least respect a poor heathen's wishes?" },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|Gave you more time than I should've then, not knowing your particular occupation'n'all." },
                        new object[] { "C|TIME:900:ORSKIP", "T|Sofia|Um, look, I-" },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|I said I wouldn't be hearing it! And if'you've been coming to pry into my affairs, spirit myself away to that castle even..." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|JUDGING", "T|Crooked Sofia|I'll have you know that I haven't been doing anything crookwise that you can prove, hear?" },
                        new object[] { "C|TIME:1500:ORSKIP", "T|Sofia|Your hut literally says \"Crime Shack\" on it." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|WORRIED" },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|JUDGING", "T|Crooked Sofia|IT'S BEING BESIDES THE POINT, I TELL YOU!" },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|UNIMPRESSED", "T|Crooked Sofia|Let me warn you, I'm knowing my liabilities, like, hear." },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|You can't force me anywhere. And if you try, well, don't let yourself be underestimating this one here, Sister." },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|I'm a master of the ESSENCE rightwise, and one wrong look and I could be pulling the SOFIA ESSENCE right out of your..." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|WORRIED", "T|Crooked Sofia|Your..." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "T|Crooked Sofia|Huh." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|...what?" },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|Now that I'm getting a proper scoping... you have quite the aura of ESSENCE on you, hear." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "T|Crooked Sofia|I'm something of an expert. Got that sense for the scent of it, and you are coming off of my charts sister." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|CONSIDERING", "T|Crooked Sofia|Who *are* you?" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Oh, me?" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|GRINNING", "T|Sofia|I'm babey." },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|..." },
                        new object[] { "C|TIME:1500:ORSKIP", "F|CROOKED SOFIA|THINKING", "T|Crooked Sofia|...well met, Babey." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|LAUGHING" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|I was joking." },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|Oh, rightwise?" },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|GRINNING", "T|Crooked Sofia|Well met, Wasjoking. ;)" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Hah, close enough for me." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|EXCITED", "T|Sofia|But yes, look." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|HAPPY", "T|Sofia|I'm not here to come after you, and I'm not really a \"Royal Agent\", or whatever!" },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|THINKING", "T|Crooked Sofia|But if I was hearing rightwise, you said..." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "T|Sofia|I mean, look, yes, I am on a mission for the King. But it has nothing to do with you, and I only met her today!", new VoidDel(delegate() { Sofia.ParanoidFlag = 0; })},
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|DOWNCAST", "T|Crooked Sofia|Okay, fine, I'll admit you have me intruigued." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|CONSIDERING", "T|Crooked Sofia|Mostly only for the flavour of your ESSENCE, hear." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "T|Crooked Sofia|It tastes of... elsewhere." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|That's probably because I'm from another dimension, and I was brought here by a crazy doppelganger of myself to a world full of other crazy me-doppelgangers in order to save everyone with the power of my pure essence, or my ultra friendship juice, or shounen anime spirit or whatever.", new VoidDel(delegate() { Sofia.CrookedKnowledgeFlag = 1; }) },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|UNIMPRESSED", "T|Sofia|You know, as you do." },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|..." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|THINKING", "T|Crooked Sofia|Well then, okay." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "T|Sofia|Do you actually believe me, or are you just playing me for a fool?" },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|I'd be speaking wrongly if I said that falsehood hadn't been crossing my mind, Sister." },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|But depths below, Sister. If what I'm sensin' from you is any sort of indication..." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|JUDGING", "T|Crooked Sofia|Shit, you're actually telling the truth, aren't you?" },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|HAPPY", "F|SOFIA|THINKING", "T|Sofia|Wow, you guys must be used to some EXTREME WEIRDNESS if that's all it took for you to believe me, huh?" },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|Well, you are being truthful, rightwise?" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|HAPPY", "T|Sofia|Yep, I am." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|EXCITED", "T|Crooked Sofia|Knew it. I was scoping those vibes, yo." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|HAPPY", "T|Crooked Sofia|But while I'm not a stranger to weirdness, you sure have spiced up my mornin', Sister." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|THINKING", "T|Crooked Sofia|Or, possibly evening. Or midday! Time can be strange here." },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|Is it like that in your world? Say, why are you here again, anyway?" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "T|Sofia|Oh my God, while you were busy accusing me of being a Cop you almost made me forget why I was here in the first place!" },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|GRINNING", "T|Crooked Sofia|Apologies, yo. How abouts we start over like, interdimensional stranger, and you tell the Crooked Sofia here *what you need*." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|HAPPY", "T|Sofia|Right, okay, see. [T:500]The Cool Sofia brought me here to this dimension through a portal, because they needed someone with pure Sofia Essence-" },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|[R]ESSENCE[C:WHITE]." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Uh, right, yeah. [R]SOFIA ESSENCE[C:WHITE]. Anyway, she and the King Sofia said they needed me to help restore the [R]ESSENCE[C:WHITE] to the land at the SOURCE." },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|Wig!" },
                        new object[] { "B|SOFIA_CROOKED_RETURN_HELPFUL_FINALIZE" }
                    };
                    break;
                case "SOFIA_CROOKED_RETURN_HELPFUL_FINALIZE":
                    Script = new object[]
                    {
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "T|Sofia|Quite. Anyway, yes, so I went to see the Mystic Sofia after I got a royal edict, but apparently I need some more, uh, local essence in order to perform the procedure." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Aaaaaaand I think you can probably help me with getting some more [R]ESSENCE[C:WHITE], so here I am." },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|Right... Well then. Well then!" },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|GRINNING", "T|Crooked Sofia|Why didn't you say so first thing!" },
                        new object[] { "C|TIME:1500:ORSKIP", "F|SOFIA|DOWNCAST", "T|Sofia|Well, you see, I-" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|UNIMPRESSED", "T|Crooked Sofia|But, no worries, no worries, hear. Look no further my otherworldly friend." },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|I can be hooking you up right away!" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|EXCITED", "T|Crooked Sofia|I'll even do it on the house!" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Aaaaaaand I think you can probably help me with getting some more [R]ESSENCE[C:WHITE], so here I am." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Wait, really? Wow, that will make this a lot easier than I expected it would be. Thank you!" },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|HAPPY", "F|SOFIA|THINKING", "T|Sofia|Wait, but, hold on. I'm bringing back the essence, but, like, I thought you were profiting from the shortfall?" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "T|Sofia|On second thoughts, I hope that that doesn't turn you against my cause. Whoops." },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|Naw. You think they'll ease up regulation after all these years of keeping it locked up? And besides, even I'm running dry these days. There more essence there is, the more there is for me to steal!" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|EXCITED", "F|CROOKED SOFIA|GRINNING", "T|Crooked Sofia|Uh, that is to say... Acquire through innovative business practices." },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|But, anyway, I think you'll be doing a true righteous thing pulling this little world of ours out of the proverbial gutter, hear." },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|And I like to be a part of these things when I can. Build up favours and such, by lending a little hand even when it's at my own expense." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Oh, well, I'm glad to hear!" },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|So, should we get down to business? I can do the ESSENCE TRANSFER right now, if you're needing it." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|HAPPY", "T|Sofia|I mean, I guess there's no time to waste!" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "T|Sofia|Um, I mean. What does it involve exactly..." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|I'm not sure I particularly want to, uh SNORT A WEED." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|LAUGHING", "F|SOFIA|JUDGING", "T|Sofia|INJECT AN ALCOMOHOL!" },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|Oh, no, nothing like that. Nothing so direct needed, hear." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|HAPPY", "T|Crooked Sofia|I can just channel it right into you." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Oh! Well that's okay then." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Go right ahead, I guess." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|EXCITED", "T|Crooked Sofia|Excellent! Hold still, mind. I will begin." },
                        new object[] { "C|TIME:2000:ORSKIP", "F|CROOKED SOFIA|DOWNCAST", "T||" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "T|Sofia|Uh, I don't feel anythin-" },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|GRINNING", "F|SOFIA|JUDGING", "T|Sofia|[R]Whoa.", new VoidDel(delegate()
                        {
                            Sofia.CrookedFlag = 4;
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                WorldEntity Add = new Sofia.EssenseGlow("ESSENCEGLOW", new Vector2(0, 0), (TAtlasInfo)Shell.AtlasDirectory["ESSENCEGLOW"], 0.6f);
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                            }));
                        })
                        },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|Told ya." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|[R]I feel... glowing." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|[R]Like I'm..." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|[R]Somehow, even more Sofia-like than I was before. Huh!" },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|Hah! Welcome to the wonders of [R]SOFIA ESSENCE[C:WHITE]. ;)" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|[R]I can see why they said this world is wanting for more of it..." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Wait, it's fading...", new VoidDel(delegate()
                        {
                            Sofia.EssenseGlow EG = (Sofia.EssenseGlow)Shell.GetEntityByName("ESSENCEGLOW");
                            EG.AnimationQueue.Add(Animation.Retrieve("fadeoutcolourpreserve"));
                            EG.TransientAnimation = true;
                        })
                        },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "D|ESSENCEGLOW|IFPRESENT", "T|Sofia|Huh. What happened?" },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|HAPPY", "T|Crooked Sofia|Good things can't be lasting forever. The initial effects fade quickwise." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "F|CROOKED SOFIA|EXCITED", "T|Crooked Sofia|But that essence is part of you now." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|I see..." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|CONSIDERING", "F|CROOKED SOFIA|LAUGHING", "T|Sofia|Well, that certainly was something." },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|There's more where that came from, Sister." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|UNIMPRESSED", "T|Sofia|Oh, I got that impression, trust me." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "T|Sofia|But hey, I should probably get back to it, shouldn't I? Saving the world, now that I've got more essence, I mean." },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|Be on your way, Sisterino. Always good to meet new folks and do good business. Glad to have been of service, no thanks required." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|GRINNING", "T|Crooked Sofia|Just remember dear old CROOKED SOFIA when you make the bigtime now, hey? Always good to have contacts. ;)" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|DOWNCAST", "T||The Crooked Sofia sweeps you a dramatic bow." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Uh, yeah. Thanks." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|LAUGHING", "T|Crooked Sofia|I said no need, hear!" },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|HAPPY", "T|Crooked Sofia|But I'll be seeing you. ;)" },
                        new object[] { "C|GWS:impos", "F|SOFIA|THINKING", "T||Use the [C:138-0-255-255]MAP[C:WHITE] to navigate to your next destination." },
                        new object[] { "C|GWS:impos_mapnavigate", "T||Use the [C:138-0-255-255]MAP[C:WHITE] to navigate to your next destination." },
                    };
                    break;
                case "SOFIA_CROOKED_RETURN_NO_ESSENCE_MISSION_KNOWLEDGE":
                    Script = new object[]
                    {
                        InitCrookedBasic,
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Hello?" },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|LAUGHING", "A|CROOKED SOFIA|FADEIN", "M|CRIMINAL|TRUE", "T|Crooked Sofia|Ah, what have we here, now?" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|HAPPY", "T|Sofia|It is the me." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|EXCITED", "T|Crooked Sofia|So I am rightwise seeing." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|GRINNING", "T|Crooked Sofia|Our new soon-to-be-saviour returns!" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Well, I'm still not too sure about that. But I'm doing my best I guess." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|HAPPY", "T|Crooked Sofia|Aren't we all, now? Aren't we all." },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|But hey, now you've taken time out of your quest to visit little ol' me again." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|EXCITED", "T|Crooked Sofia|That's some gracious attention, I'm being havin' a likin' for being kept in the loop, hear." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|GRINNING", "T|Sofia|Well, you are definitely one of the most interesting people I've met here to far." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|UNIMPRESSED", "T|Sofia|...for better or worse." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|LAUGHING", "T|Crooked Sofia|Wouldn't have it any other way, Sister." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|GRINNING", "T|Sofia|Fair enough." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|HAPPY", "T|Crooked Sofia|But say, how is your mission going?" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Well, actually, I came here to ask you for help." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|EXCITED", "T|Crooked Sofia|Really now? Not just to visit the ol' Crooked Sofia, now? I feel used, yo. USED." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|UNIMPRESSED", "F|CROOKED SOFIA|HAPPY", "T|Crooked Sofia|I kid, of course." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|HAPPY", "T|Crooked Sofia|I'm unsurprised that you are in need of my radical skillset. Coursewise you are." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|GRINNING", "T|Crooked Sofia|How can I make myself be of assistance?" },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|Assuming that is, as I am, that reintroduction with my person hasn't been putting you off seeking my help. ;)" },
                        new object[] { "B|SOFIA_CROOKED_RETURN_HELPFUL_FINALIZE" }
                    };
                    break;
                case "SOFIA_CROOKED_RETURN_NO_KNOWLEDGE":
                    Script = new object[]
                    {
                        InitCrookedBasic,
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Hello?" },
                        new object[] { "C|GWS:CONTINUE", "M|CRIMINAL|TRUE", "F|CROOKED SOFIA|LAUGHING", "A|CROOKED SOFIA|FADEIN", "T|Crooked Sofia|Ah, what have we here, now?" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|HAPPY", "T|Sofia|It is the me." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|EXCITED", "T|Crooked Sofia|So I am rightwise seeing." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|GRINNING", "T|Crooked Sofia|My new prodigal acquaintance returns." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Sofia in the flesh..." },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|Hah, we are all Sofia here, now." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|LAUGHING", "T|Crooked Sofia|But I will grant you, your havin' your own uniqueness Sofiawise." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|GRINNING", "T|Sofia|Damn straight!" },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|EXCITED", "T|Crooked Sofia|So, how might I be bein' helping you this fine eternity?" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|HAPPY", "T|Sofia|Well, I'm getting on pretty well with my quest to save the world." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|THINKING", "T|Crooked Sofia|..." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|CONSIDERING", "T|Crooked Sofia|Hear? You're on a quest to save the world?", new VoidDel(delegate() { Sofia.CrookedKnowledgeFlag = 1; })},
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "T|Sofia|...didn't I tell you that?" },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|Well, applying some thought to it, all right and fronted, course..." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|THINKING", "T|Crooked Sofia|I don't believe you did, no." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "T|Sofia|Oh, well crap." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|I guess I must have accidentally skipped some of the dialog trees where I spoke to you more, huh." },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|Sounds about right to me." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|GRINNING", "T|Crooked Sofia|Perhaps after this is all done, it'd do you rightwise to come back and explore some different options, now, hear." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|EXCITED", "T|Crooked Sofia|Find out more of the things I might have been saying to you, now." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|HAPPY", "T|Sofia|Sounds sensible!" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|WORRIED", "T|Sofia|But, wait, how do you know about this?" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "T|Sofia|Actually, within the conceit of this narrative, how do I know about this?" },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|THINKING", "T|Both|..." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|What?" },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|What?" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|CONSIDERING", "T|Sofia|I'm not actually sure, but I have a vague feeling things got a little meta there for a second." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|GRINNING", "T|Crooked Sofia|Happens to the best of us. I wouldn't be worrying about it, Sister." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Alright I guess." },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|Now, what were we talking about again?" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|EXCITED", "T|Sofia|Oh, right! My mission to save the world, yes." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|EXCITED", "T|Sofia|See, I'm not sure if I told you this, but I'm actually a dimensional traveller." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|HAPPY", "T|Sofia|I was brought here by the Cool Sofia from my world so that I could used my PURE SOFIA ESSENCE, to, like..." },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Restore life to the SOURCE on behalf of the King Sofia, and save the Kingdom/World/Whatever." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|THINKING", "T|Sofia|...I think." },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|Sounds pretty good to me. I'm just as invested in the future this would is having as any of us." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|HAPPY", "T|Sofia|Oh! And the essence you gave me is gonna help me do it! :D" },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|GRINNING", "T|Crooked Sofia|Wonderful! I had I feeling you were wanting it for reasons more than my usual folk, hear." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|EXCITED", "T|Crooked Sofia|I'm just glad the Crooked Sofia here could be of service in your cause, hear." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|GRINNING", "T|Sofia|Rightwise?" },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|LAUGHING", "T|Crooked Sofia|Rightwise!" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|Haha, well then, I better get going and finish saving the world, I guess." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|EXCITED", "T|Crooked Sofia|Sure thing, Sister! Don't let me be holding you!" },
                        new object[] { "C|GWS:impos_mapnavigate", "F|CROOKED SOFIA|HAPPY", "T||Use the [C:138-0-255-255]MAP[C:WHITE] to navigate to your next destination." },
                    };
                    break;
                case "SOFIA_CROOKED_FINAL":
                    Script = new object[]
                    {
                        InitCrookedBasic,
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|EXCITED", "T|Sofia|Hi there!" },
                        new object[] { "C|GWS:CONTINUE", "M|CRIMINAL|TRUE", "F|CROOKED SOFIA|LAUGHING", "A|CROOKED SOFIA|FADEIN", "T|Crooked Sofia|Ah, you come back to see me, Sister!" },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|GRINNING", "T|Crooked Sofia|World almost saved, yet?" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|HAPPY", "T|Sofia|I think things are close to wrapping up, yeah!" },
                        new object[] { "C|GWS:CONTINUE", "T|Sofia|I've got everything I need, I think." },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|Then all [C:138-0-255-255]ULTRASOFIAWORLD[C:WHITE] rightwise owes you thanks, hear!" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|GRINNING", "F|CROOKED SOFIA|LAUGHING", "T|Crooked Sofia|Or at least, will very soon. ;)" },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|EXCITED", "T|Sofia|Hahaha, I do my best, yo." },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|HAPPY", "T|Sofia|Do you want to come along for the finale?" },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|Aw, as grateful as I am, I've done played my part in these happenings, methinks." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|HAPPY", "T|Crooked Sofia|And even in times such as these, sticking my neck in the path of enquiry is most likely being a bad idea." },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|If I am correct the King will be there, and she and I..." },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|GRINNING", "T|Crooked Sofia|Don't quite see eye to eye. ;)" },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|HAPPY", "T|Sofia|Ah, fair enough!" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|EXCITED", "T|Sofia|I'll wish you luck, then. And thank you!" },
                        new object[] { "C|GWS:CONTINUE", "F|CROOKED SOFIA|GRINNING", "T|Crooked Sofia|It's me who should rightwise be thanking you, hear! As I was being saying." },
                        new object[] { "C|GWS:CONTINUE", "T|Crooked Sofia|Go forth and save the world, Sister! [T:400]Fare ye well!" },
                        new object[] { "C|GWS:CONTINUE", "F|SOFIA|GRINNING", "T|Sofia|Haha, will do!" },
                        new object[] { "C|GWS:impos_mapnavigate", "T||Use the [C:138-0-255-255]MAP[C:WHITE] to navigate to your next destination." },
                    };
                    break;
                case "SOFIA_EPILOGUE_SCENES":
                    Script = new object[]
                    {
                        new object[] { "C|TIME:0", new VoidDel(delegate()
                        {
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                WorldEntity Add = new WorldEntity("SOFIAWORLDBACKDROP", new Vector2(-600, 0), (TAtlasInfo)Shell.AtlasDirectory["SWBG"], 0.05f);
                                Add.ColourValue = new Color(0,0,0,0);
                                Add.AnimationQueue.Add(Animation.Retrieve("FADEIN"));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                            }));
                        })
                        },
                        new object[] { "C|TIME:3000", "T||", "A|WHITE-SHEET|BLINKOUT" },
                        new object[] { "C|TIME:5000", new VoidDel(delegate()
                        {
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                WorldEntity Add = new Sofia.BigSofia("BIG SOFIA", new Vector2(500, 400), (TAtlasInfo)Shell.AtlasDirectory["BIGSOFIA"], 0.4f, new ArrayList(new String[] { "SHIFTER" }));
                                Add.CenterOrigin = true;
                                Add.ColourValue = new Color(0,0,0,0);
                                Add.Rotate(-1.8f);
                                Add.Scale(new Vector2(-0.9f, -0.9f));
                                Add.AnimationQueue.Add(Animation.Retrieve("SPIRALOUT"));
                                SortedList FreshFadeFrames = Animation.CreateColourTween(new ColourShift(255f, 255f, 255f, 255f), 200, 20);
                                Animation FreshFade = new Animation("bigsofiafadequick");
                                FreshFade.WriteColouring(FreshFadeFrames);
                                Add.AnimationQueue.Add(FreshFade);
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                            }));
                        })
                        },
                        new object[] { "C|TIME:1500", "A|WHITE-SHEET|FADEIN" },
                        new object[] { "C|TIME:0", "D|SOFIAWORLDBACKDROP", "D|BIG SOFIA", new VoidDel(delegate()
                        {
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                WorldEntity Add = new WorldEntity("CASTLEEXTBG", new Vector2(0, 0), (TAtlasInfo)Shell.AtlasDirectory["CASTLEEXTBG"], 0.05f);
                                Add.ColourValue = new Color(0,0,0,0);
                                Add.AnimationQueue.Add(Animation.Retrieve("FADEIN"));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                                Add = new WorldEntity("GRADIENT-SHEET", new Vector2(-2560, 0), (TAtlasInfo)Shell.AtlasDirectory["WHITEGRADIENT"], 0.25f);
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                                Add = new WorldEntity("FAUX SOFIA", new Vector2(-640, 400), (TAtlasInfo)Shell.AtlasDirectory["BIGSOFIA"], 0.20f);
                                Add.SetAtlasFrame(new Point(1,0));
                                Add.CenterOrigin = true;
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                            }));
                        })
                        },
                        new object[] { "C|TIME:3000", "T||", "A|WHITE-SHEET|BLINKOUT" },
                        new object[] { "C|TIME:2000", "T||", "A|GRADIENT-SHEET|2560=0,1500,20||||", "A|FAUX SOFIA|2560=0,1500,20||||"},
                        new object[] { "C|TIME:0", "D|CASTLEEXTBG", "A|FAUX SOFIA|BLINKOUT", new VoidDel(delegate()
                        {
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                WorldEntity Add = new WorldEntity("CASTLEINTBG", new Vector2(0, 0), (TAtlasInfo)Shell.AtlasDirectory["CASTLEINTBG"], 0.05f);
                                Add.ColourValue = new Color(0,0,0,0);
                                Add.AnimationQueue.Add(Animation.Retrieve("FADEIN"));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                            }));
                        })
                        },
                        new object[] { "C|TIME:3000", "T||", "A|GRADIENT-SHEET|BLINKOUT", "A|GRADIENT-SHEET|-2560=0,20,20||||", "A|FAUX SOFIA|-2560=0,20,20||||" },
                        new object[] { "C|TIME:2000", "T||", "A|GRADIENT-SHEET|BLINKIN", "A|FAUX SOFIA|BLINKIN", "A|GRADIENT-SHEET|2560=0,1500,20||||", "A|FAUX SOFIA|2560=0,1500,20||||"},
                        new object[] { "C|TIME:0", "D|CASTLEINTBG", "D|FAUX SOFIA", new VoidDel(delegate()
                        {
                            Shell.GetEntityByName("WHITE-SHEET").LayerDepth = 0.999f;
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                WorldEntity Add = new WorldEntity("CROOKEDCAVEBG", new Vector2(0, -160), (TAtlasInfo)Shell.AtlasDirectory["CROOKEDCAVEBG"], 0.05f);
                                Add.ColourValue = new Color(0,0,0,0);
                                Add.AnimationQueue.Add(Animation.Retrieve("FADEIN"));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                                Add = new WorldEntity("CRIME SHACK", new Vector2(850, 300), (TAtlasInfo)Shell.AtlasDirectory["CRIMESHACK"], 0.46f);
                                Add.ColourValue = new Color(0,0,0,0);
                                Add.AnimationQueue.Add(Animation.Retrieve("FADEIN"));
                                Add.CenterOrigin = true;
                                Add.Scale(new Vector2(-0.2f, -0.2f));
                                Shell.RenderQueue.Add(Add);
                                Shell.UpdateQueue.Add(Add);
                                Add = new WorldEntity("CROOKED SOFIA", new Vector2(830, 405), (TAtlasInfo)Shell.AtlasDirectory["CROOKEDSOFIA"], 0.48f);
                                Add.ColourValue = new Color(0,0,0,0);
                                Add.AnimationQueue.Add(Animation.Retrieve("FADEIN"));
                                Add.CenterOrigin = true;
                                Add.SetAtlasFrame(new Point(0, 0));
                                Add.Scale(new Vector2(-0.06f, -0.06f));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                            }));
                        })
                        },
                        new object[] { "C|TIME:1500", "T||", "D|GRADIENT-SHEET" },
                        new object[] { "C|TIME:3000", "A|WHITE-SHEET|FADEINLONG", "T|Crooked Sofia|Hey, um, what the hells-wise?" },
                        new object[] { "C|TIME:3000", "M|#NULL", "D|#ALL" },
                        new object[] { "C|TIME:4000", "M|BIRDS|FALSE" },
                        new object[] { "C|TIME:3000", new VoidDel(delegate()
                        {
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                WorldEntity Add = new WorldEntity("HOUSEBG", new Vector2(0, 0), (TAtlasInfo)Shell.AtlasDirectory["HOUSEBG"], 0.05f);
                                Add.ColourValue = new Color(0,0,0,0);
                                Add.AnimationQueue.Add(Animation.Retrieve("FADEINLONG"));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                                String TextContent = System.DateTime.Now.DayOfWeek.ToString().ToUpper() + ". GILLETTE RESIDENCE. " + System.DateTime.Now.ToString("hh:mm tt") + "...";
                                TextEntity Text = new TextEntity("QUIET HOUSE TEXT", TextContent, new Vector2(640 - (Shell.Default.MeasureString(TextContent).X/2), 500), 0.1f);
                                Text.TypeWrite = false;
                                Text.ColourValue = new Color(0,0,0,0);
                                Shell.UpdateQueue.Add(Text);
                                Shell.RenderQueue.Add(Text);
                            }));
                        })
                        },
                        new object[] { "C|TIME:4000", "A|QUIET HOUSE TEXT|FADEIN" },
                        new object[] { "C|TIME:3000", "A|QUIET HOUSE TEXT|FADEOUT" },
                        new object[] { "C|TIME:3000", "S|R_CYMBAL", new VoidDel(delegate()
                        {
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                WorldEntity Add = new WorldEntity("FAUX SOFIA", new Vector2(640, 450), (TAtlasInfo)Shell.AtlasDirectory["BIGSOFIA"], 0.20f);
                                Add.SetAtlasFrame(new Point(3,0));
                                Add.CenterOrigin = true;
                                Add.ColourValue = new Color(0,0,0,0);
                                Add.AnimationQueue.Add(Animation.Retrieve("FADEINLONG"));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                            }));
                        })
                        },
                        new object[] { "C|TIME:3000", "M|#NULL", "S|#CLOSEALL", },
                        new object[] { "D|#ALL", "B|SOFIA_CREDITS" }
                    };
                    break;
                case "SOFIA_CREDITS":
                    Script = new object[]
                    {
                        new object[] { "C|TIME:2400", "M|CREDITS|FALSE", new VoidDel(delegate()
                        {
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                WorldEntity Add = new WorldEntity("LOGO", new Vector2(640, 360), (TAtlasInfo)Shell.AtlasDirectory["SOFIA_CREDITS_LOGO"], 0.20f);
                                Add.ColourValue = new Color(0,0,0,0);
                                Add.CenterOrigin = true;
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                                Add = new WorldEntity("CONTINUED", new Vector2(640, 400), (TAtlasInfo)Shell.AtlasDirectory["SOFIA_CREDITS_CONTINUED"], 0.20f);
                                Add.ColourValue = new Color(0,0,0,0);
                                Add.CenterOrigin = true;
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                                Add = new WorldEntity("NOTCONTINUED", new Vector2(640, 500), (TAtlasInfo)Shell.AtlasDirectory["SOFIA_CREDITS_NOTCONTINUED"], 0.20f);
                                Add.ColourValue = new Color(0,0,0,0);
                                Add.CenterOrigin = true;
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                            }));
                        })
                        },
                        new object[] { "C|TIME:2400", "A|LOGO|BLINKIN" },
                        new object[] { "C|TIME:2400", "A|CONTINUED|BLINKIN" },
                        new object[] { "C|TIME:1200", "A|NOTCONTINUED|BLINKIN" },
                        new object[] { "C|TIME:300", new VoidDel(delegate()
                        {
                            Shell.RunQueue.Add(new VoidDel(delegate()
                            {
                                WorldEntity Add = new WorldEntity("ROLL", new Vector2(640, 800), (TAtlasInfo)Shell.AtlasDirectory["SOFIA_CREDITS_ROLL"], 0.20f);
                                Add.CenterOrigin = true;
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                                Add = new WorldEntity("STARRING", new Vector2(640, 900), (TAtlasInfo)Shell.AtlasDirectory["SOFIA_CREDITS_STARRING"], 0.20f);
                                Add.CenterOrigin = true;
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                                Add = new WorldEntity("HERSELF", new Vector2(900, 1300), (TAtlasInfo)Shell.AtlasDirectory["SOFIA_CREDITS_HERSELF"], 0.20f);
                                Add.CenterOrigin = true;
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                                Add = new WorldEntity("SOFIA", new Vector2(380, 1300), (TAtlasInfo)Shell.AtlasDirectory["SOFIASPRITES"], 0.20f);
                                Add.CenterOrigin = true;
                                Add.AnimationQueue.Add(Animation.Retrieve("SOFIASLIDESHOW"));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                                Add = new WorldEntity("COOL", new Vector2(380, 2050), (TAtlasInfo)Shell.AtlasDirectory["SOFIA_CREDITS_COOL"], 0.20f);
                                Add.CenterOrigin = true;
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                                Add = new WorldEntity("COOL SOFIA", new Vector2(900, 2050), (TAtlasInfo)Shell.AtlasDirectory["COOLSOFIA"], 0.20f);
                                Add.CenterOrigin = true;
                                Add.AnimationQueue.Add(Animation.Retrieve("SOFIASLIDESHOW"));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                                Add = new WorldEntity("GOLEM", new Vector2(900, 2800), (TAtlasInfo)Shell.AtlasDirectory["SOFIA_CREDITS_GOLEM"], 0.20f);
                                Add.CenterOrigin = true;
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                                Add = new WorldEntity("GOLEM SOFIA", new Vector2(380, 2800), (TAtlasInfo)Shell.AtlasDirectory["GOLEMSOFIA"], 0.20f);
                                Add.CenterOrigin = true;
                                Add.ManualHorizontalFlip = true;
                                Add.AnimationQueue.Add(Animation.Retrieve("GOLEMSLIDESHOW"));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                                Add = new WorldEntity("KING", new Vector2(380, 3550), (TAtlasInfo)Shell.AtlasDirectory["SOFIA_CREDITS_KING"], 0.20f);
                                Add.CenterOrigin = true;
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                                Add = new WorldEntity("KING SOFIA", new Vector2(900, 3550), (TAtlasInfo)Shell.AtlasDirectory["KINGSOFIA"], 0.20f);
                                Add.CenterOrigin = true;
                                Add.AnimationQueue.Add(Animation.Retrieve("SOFIASLIDESHOW"));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                                Add = new WorldEntity("CROOKED", new Vector2(900, 4300), (TAtlasInfo)Shell.AtlasDirectory["SOFIA_CREDITS_CROOKED"], 0.20f);
                                Add.CenterOrigin = true;
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                                Add = new WorldEntity("CROOKED SOFIA", new Vector2(380, 4300), (TAtlasInfo)Shell.AtlasDirectory["CROOKEDSOFIA"], 0.20f);
                                Add.CenterOrigin = true;
                                Add.ManualHorizontalFlip = true;
                                Add.AnimationQueue.Add(Animation.Retrieve("SOFIASLIDESHOW"));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                                Add = new WorldEntity("MYSTIC", new Vector2(380, 5050), (TAtlasInfo)Shell.AtlasDirectory["SOFIA_CREDITS_MYSTIC"], 0.20f);
                                Add.CenterOrigin = true;
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                                Add = new WorldEntity("MYSTIC SOFIA", new Vector2(900, 5050), (TAtlasInfo)Shell.AtlasDirectory["MYSTICSOFIA"], 0.20f);
                                Add.CenterOrigin = true;
                                Add.AnimationQueue.Add(Animation.Retrieve("SOFIASLIDESHOW"));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                                Add = new WorldEntity("BIG", new Vector2(640, 5600), (TAtlasInfo)Shell.AtlasDirectory["SOFIA_CREDITS_BIG"], 0.20f);
                                Add.CenterOrigin = true;
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                                Add = new WorldEntity("BIG SOFIA", new Vector2(640, 6050), (TAtlasInfo)Shell.AtlasDirectory["BIGSOFIA"], 0.20f);
                                Add.CenterOrigin = true;
                                Add.AnimationQueue.Add(Animation.Retrieve("SOFIASLIDESHOW"));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                                Add = new WorldEntity("LETTER", new Vector2(300, 6500), (TAtlasInfo)Shell.AtlasDirectory["SOFIA_CREDITS_LETTER"], 0.20f);
                                Add.CenterOrigin = true;
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                                Add = new WorldEntity("LETTER OBJECT", new Vector2(300, 6700), (TAtlasInfo)Shell.AtlasDirectory["LETTER"], 0.20f);
                                Add.CenterOrigin = true;
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                                Add = new WorldEntity("PORTAL", new Vector2(800, 6500), (TAtlasInfo)Shell.AtlasDirectory["SOFIA_CREDITS_PORTAL"], 0.20f);
                                Add.CenterOrigin = true;
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                                Add = new WorldEntity("PORTAL OBJECT", new Vector2(650, 6730), (TAtlasInfo)Shell.AtlasDirectory["PORTAL"], 0.20f);
                                Add.CenterOrigin = true;
                                Add.Scale(new Vector2(-0.5f, -0.5f));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                                Add = new WorldEntity("MIRANDA", new Vector2(640, 7050), (TAtlasInfo)Shell.AtlasDirectory["SOFIA_CREDITS_MIRANDA"], 0.20f);
                                Add.CenterOrigin = true;
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                                Add = new WorldEntity("MIRANDA OBJECT", new Vector2(640, 7400), (TAtlasInfo)Shell.AtlasDirectory["BIGSOFIA"], 0.20f);
                                Add.CenterOrigin = true;
                                Add.SetAtlasFrame(new Point(2, 2));
                                Add.Scale(new Vector2(-0.25f, -0.25f));
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                                Add = new WorldEntity("PRODUCTION", new Vector2(640, 7520), (TAtlasInfo)Shell.AtlasDirectory["SOFIA_CREDITS_PRODUCTION"], 0.20f);
                                Add.CenterOrigin = true;
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                                Add = new WorldEntity("MYCREDITS", new Vector2(640, 8550), (TAtlasInfo)Shell.AtlasDirectory["SOFIA_CREDITS_MYCREDITS"], 0.20f);
                                Add.CenterOrigin = true;
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                                Add = new WorldEntity("SOUNDTRACK", new Vector2(640, 9900), (TAtlasInfo)Shell.AtlasDirectory["SOFIA_CREDITS_OST"], 0.20f);
                                Add.CenterOrigin = true;
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                                Add = new WorldEntity("LOGOS", new Vector2(640, 10900), (TAtlasInfo)Shell.AtlasDirectory["SOFIA_CREDITS_LOGOS"], 0.20f);
                                Add.CenterOrigin = true;
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                                Add = new WorldEntity("BIRTHDAY", new Vector2(640, 11600), (TAtlasInfo)Shell.AtlasDirectory["SOFIA_CREDITS_BIRTHDAY"], 0.20f);
                                Add.CenterOrigin = true;
                                Shell.UpdateQueue.Add(Add);
                                Shell.RenderQueue.Add(Add);
                            }));
                        })
                        },
                        new object[] { "C|TIME:900", new VoidDel(delegate()
                        {
                            Animation A = new Animation("creditsroll");
                            SortedList CreditsMovement = Animation.CreateVectorTween(new Vector2(0, -11300), 100800, 20);
                            A.WriteMovement(CreditsMovement);
                            A.AutoTrigger = false;
                            foreach(WorldEntity E in Shell.RenderQueue)
                            {
                                if(E.Name == "BIRTHDAY") { continue; }
                                E.AnimationQueue.Add(A.Clone());
                            }
                            A = new Animation("creditsroll");
                            CreditsMovement = Animation.CreateVectorTween(new Vector2(0, -11300), 100800, 20);
                            A.WriteMovement(CreditsMovement);
                            A.AutoTrigger = false;
                            Shell.GetEntityByName("BIRTHDAY").AnimationQueue.Add(A);
                        })
                        },
                        new object[] { "C|TIME:120000", new VoidDel(delegate()
                        {
                            Animation.GlobalManualTrigger("creditsroll");
                        })
                        },
                        new object[] { "C|TIME:4000", "A|BIRTHDAY|FADEOUTLONG" }
                    };
                    break;
            }
            return Script;
        }
    }
}
