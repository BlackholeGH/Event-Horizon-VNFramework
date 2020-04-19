using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using System.Runtime.Serialization;
using System.Reflection;

namespace VNFramework
{
    public static class EntityFactory
    {
        public static class SelectiveStringOps
        {
            public static int IndexOfExclosed(String Input, String ContainsString, char Encloser)
            {
                Boolean Exclosed = true;
                for (int i = 0; i < Input.Length; i++)
                {
                    if (Input[i] == Encloser) { Exclosed = !Exclosed; }
                    if (Input[i] == ContainsString[0] && Exclosed)
                    {
                        for (int ii = 0; ii < ContainsString.Length; ii++)
                        {
                            if ((i + ii >= Input.Length) || !(ContainsString[ii] == Input[i + ii])) { break; }
                            if (ii == ContainsString.Length - 1) { return i; }
                        }
                    }
                }
                return -1;
            }
            public static Boolean ContainsExclosed(String Input, char ContainsChar, char Encloser)
            {
                Boolean Exclosed = true;
                foreach (char C in Input)
                {
                    if (C == ContainsChar && Exclosed) { return true; }
                    if (C == Encloser) { Exclosed = !Exclosed; }
                }
                return false;
            }
            public static String RemoveExclosed(String Input, char RemChar, char Encloser)
            {
                String Output = "";
                Boolean Remove = true;
                foreach (char C in Input)
                {
                    if (C == RemChar && Remove) { continue; }
                    else { Output += C; }
                    if (C == Encloser) { Remove = !Remove; }
                }
                return Output;
            }
            public static String[] SplitAtExclosed(String Input, char SplitChar, char Encloser)
            {
                ArrayList Splits = new ArrayList();
                String Current = "";
                Boolean Split = true;
                foreach (char C in Input)
                {
                    if (C == SplitChar && Split)
                    {
                        Splits.Add(Current);
                        Current = "";
                    }
                    else { Current += C; }
                    if (C == Encloser) { Split = !Split; }
                }
                if (Current.Length > 0) { Splits.Add(Current); }
                return Splits.ToArray().Select(o => (String)o).ToArray();
            }
        }
        public static Object[] ExtractMethodValues(String MethodOrConstructorData)
        {
            String IsolatedIdentifier = MethodOrConstructorData.Remove(MethodOrConstructorData.IndexOf('('));
            String IsolatedParamList = MethodOrConstructorData.Remove(0, MethodOrConstructorData.IndexOf('(')).Remove(MethodOrConstructorData.LastIndexOf(')'));
            Object[] TrueParameters = ParseDataList(IsolatedParamList);
            return new Object[] { IsolatedIdentifier, TrueParameters };
        }
        public static Object[] ParseDataList(String DataList)
        {
            String[] PriorToParse = SelectiveStringOps.SplitAtExclosed(DataList, ',', '\"');
            Object[] Out = new Object[PriorToParse.Length];
            for(int i = 0; i < PriorToParse.Length; i++)
            {
                Out[i] = ParseRealData(PriorToParse[i]);
            }
            return Out;
        }
        public static Object ReturnMemberOrFuncValue(String PathTree, WorldEntity PresumptiveEntity, Object ExplicitSet)
        {
            String DP = PathTree;
            Type SuperType = typeof(ScriptProcessor);
            String StaticMemberTree = DP;
            Object InstancedObject = null;
            if (PresumptiveEntity is null)
            {
                if (DP.Contains("."))
                {
                    String TypeToFind = "VNFramework." + DP.Remove(DP.LastIndexOf('.'));
                    Type Find = Type.GetType(TypeToFind);
                    if (Find != null) { SuperType = Find; }
                    while (!(SuperType.IsAbstract && SuperType.IsSealed))
                    {
                        TypeToFind = TypeToFind.Remove(TypeToFind.LastIndexOf('.'));
                        Find = Type.GetType(TypeToFind);
                        if (Find != null) { SuperType = Find; }
                        else
                        {
                            SuperType = null;
                            break;
                        }
                    }
                    StaticMemberTree = DP.Replace(TypeToFind, "");
                }
            }
            else
            {
                InstancedObject = PresumptiveEntity;
            }
            String[] TreeElements = StaticMemberTree.Split('.');
            int Count = -1;
            foreach (String S in TreeElements)
            {
                Count++;
                if (S.Contains("(") && S.Contains(")"))
                {
                    String IsolatedIdentifier = DP.Remove(DP.IndexOf('('));
                    String IsolatedParamList = DP.Remove(0, DP.IndexOf('(')).Remove(DP.LastIndexOf(')'));
                    Object[] TrueParameters = ParseDataList(IsolatedParamList);
                    if(InstancedObject is null)
                    {
                        foreach (var me in SuperType.GetMethods())
                        {
                            if (me.Name == IsolatedIdentifier && me.GetParameters().Length == IsolatedParamList.Length)
                            {
                                Boolean InvocationFlag = true;
                                int i = 0;
                                foreach(ParameterInfo pi in me.GetParameters())
                                {
                                    if (pi.ParameterType != IsolatedParamList[i].GetType())
                                    {
                                        InvocationFlag = false;
                                        break;
                                    }
                                    i++;
                                }
                                if (InvocationFlag) { return me.Invoke(null, TrueParameters); }
                            }
                        }
                    }
                    else
                    {
                        foreach (var me in InstancedObject.GetType().GetMethods())
                        {
                            if (me.Name == IsolatedIdentifier && me.GetParameters().Length == IsolatedParamList.Length)
                            {
                                Boolean InvocationFlag = true;
                                int i = 0;
                                foreach (ParameterInfo pi in me.GetParameters())
                                {
                                    if (pi.ParameterType != IsolatedParamList[i].GetType())
                                    {
                                        InvocationFlag = false;
                                        break;
                                    }
                                    i++;
                                }
                                if (InvocationFlag) { return me.Invoke(InstancedObject, TrueParameters); }
                            }
                        }
                    }
                    return null;
                }
                else
                {
                    if (InstancedObject is null)
                    {
                        foreach (var p in SuperType.GetProperties())
                        {
                            if (p.Name == S)
                            {
                                InstancedObject = p.GetValue(null);
                                break;
                            }
                        }
                        if (InstancedObject != null) { continue; }
                        foreach (var field in SuperType.GetFields())
                        {
                            if (field.Name == S)
                            {
                                InstancedObject = field.GetValue(null);
                                break;
                            }
                        }
                        if (InstancedObject == null) { return null; }
                    }
                    else
                    {
                        Object Initial = InstancedObject;
                        foreach (var p in Initial.GetType().GetProperties())
                        {
                            if (p.Name == S)
                            {
                                if(ExplicitSet != null && Count == TreeElements.Length)
                                {
                                    p.SetValue(Initial, ExplicitSet);
                                }
                                InstancedObject = p.GetValue(Initial);
                                break;
                            }
                        }
                        if (InstancedObject != Initial) { continue; }
                        foreach (var field in Initial.GetType().GetFields())
                        {
                            if (field.Name == S)
                            {
                                if (ExplicitSet != null && Count == TreeElements.Length)
                                {
                                    field.SetValue(Initial, ExplicitSet);
                                }
                                InstancedObject = field.GetValue(Initial);
                                break;
                            }
                        }
                        if (InstancedObject == Initial) { return null; }
                    }
                    return InstancedObject;
                }
            }
            return null;
        }
        public static Object ParseRealData(String DataParameter)
        {
            try
            {
                String DP = DataParameter.Replace(";", "");
                if (DP[0] == '(' && DP.Contains(")"))
                {
                    DP = DP.Remove(0, SelectiveStringOps.IndexOfExclosed(DP, ")", '\"') + 1);
                }
                if (!(SelectiveStringOps.ContainsExclosed(DP, '(', '\"') && SelectiveStringOps.ContainsExclosed(DP, ')', '\"')) && !(SelectiveStringOps.ContainsExclosed(DP, '[', '\"') && SelectiveStringOps.ContainsExclosed(DP, ']', '\"')))
                {
                    if (DP[0] == '\"' && DP[DP.Length - 1] == '\"') { return DP.Remove(1, DP.Length - 2); }
                    else
                    {
                        float f;
                        if (float.TryParse(DP, out f))
                        {
                            if (DP.Contains("f") || DP.Contains(".")) { return float.Parse(DP.Replace("f", "")); }
                            else if (DP.Contains("d")) { return double.Parse(DP.Replace("d", "")); }
                            else { return int.Parse(DP); }
                        }
                        else
                        {
                            return ReturnMemberOrFuncValue(DP, null, null);
                        }
                    }
                }
                else if (SelectiveStringOps.ContainsExclosed(DP, '(', '\"') && SelectiveStringOps.ContainsExclosed(DP, ')', '\"'))
                {
                    if (DP.ToLower().StartsWith("new"))
                    {
                        String IsolatedIdentifier = DP.Remove(0, 3).Remove(DP.IndexOf('('));
                        String IsolatedParamList = DP.Remove(0, DP.IndexOf('(')).Remove(DP.LastIndexOf(')'));
                        Object[] TrueParameters = ParseDataList(IsolatedParamList);
                        return ConstructDynamicObject(IsolatedIdentifier, TrueParameters, true);
                    }
                    else
                    {
                        return ReturnMemberOrFuncValue(DP, null, null);
                    }
                }
                else if (SelectiveStringOps.ContainsExclosed(DP, '[', '\"') && SelectiveStringOps.ContainsExclosed(DP, ']', '\"'))
                {
                    if (DP.ToLower().StartsWith("new"))
                    {
                        String IsolatedIdentifier = DP.Remove(0, 3).Remove(DP.IndexOf('['));
                        String IsolatedLengthOrNone = DP.Remove(0, DP.IndexOf('[')).Remove(DP.LastIndexOf(']'));
                        String IsolatedCurls = null;
                        if (SelectiveStringOps.ContainsExclosed(DP, '{', '\"') && SelectiveStringOps.ContainsExclosed(DP, '}', '\"'))
                        {
                            IsolatedCurls = DP.Remove(0, DP.IndexOf('{')).Remove(DP.LastIndexOf('}'));
                        }
                        if(IsolatedCurls == null && IsolatedLengthOrNone.Length == 0) { return null; }
                        else
                        {
                            int RLength = 0;
                            if (IsolatedLengthOrNone.Length > 0)
                            {
                                RLength = int.Parse(IsolatedLengthOrNone);
                            }
                            else
                            {
                                RLength = SelectiveStringOps.SplitAtExclosed(IsolatedCurls, ',', '\"').Length;
                            }
                            Type RType = Type.GetType("System." + IsolatedIdentifier);
                            Object[] TempR = new Object[RLength];
                            if(IsolatedCurls != null)
                            {
                                Object[] Contents = ParseDataList(IsolatedCurls);
                                for(int i = 0; i < Contents.Length && i < TempR.Length; i++)
                                {
                                    TempR[i] = Contents[i];
                                }
                            }
                            return TempR.Select(x => Convert.ChangeType(x, RType)).ToArray();
                        }
                    }
                    else
                    {
                        String IndexableIdentifier = DP.Remove(DP.IndexOf('['));
                        String IndexValue = DP.Remove(0, DP.IndexOf('[') + 1).Remove(DP.LastIndexOf(']'));
                        Object TrueIndex = ReturnMemberOrFuncValue(IndexValue, null, null);
                        Object TrueDataStructure = ReturnMemberOrFuncValue(IndexableIdentifier, null, null);
                        Object IndexPull = null;
                        if (TrueDataStructure is IList && TrueIndex is int)
                        {
                            IndexPull = ((IList)TrueDataStructure)[(int)TrueIndex];
                        }
                        else if (TrueDataStructure is IDictionary)
                        {
                            IndexPull = ((IDictionary)TrueDataStructure)[TrueIndex];
                        }
                        return IndexPull;
                    }
                }
                return null;
            }
            catch (ArgumentException E) { return null; }
        }
        public static WorldEntity ConstructDynamicWorldEntity(String TypeName, Object[] ConstructorArgs)
        {
            if (!TypeName.ToUpper().Contains("VNFRAMEWORK.")) { TypeName = "VNFramework." + TypeName; }
            if (TypeName.ToUpper() != "VNFRAMEWORK.WORLDENTITY")
            {
                if(TypeName.Contains(".")) { TypeName = TypeName.Remove(0, TypeName.LastIndexOf('.')); }
                Assembly ThisAssembly = Assembly.GetExecutingAssembly();
                Type[] TheseTypes = ThisAssembly.GetTypes();
                foreach(Type T in TheseTypes)
                {
                    if(T.IsSubclassOf(typeof(WorldEntity)) && T.Name == TypeName)
                    {
                        TypeName = T.FullName;
                        break;
                    }
                }
            }
            return (WorldEntity)ConstructDynamicObject(TypeName, ConstructorArgs, false);
        }
        public static Object ConstructDynamicObject(String TypeName, Object[] ConstructorArgs, Boolean BroadSearch)
        {
            if(BroadSearch)
            {
                Assembly ThisAssembly = Assembly.GetExecutingAssembly();
                Type[] TheseTypes = ThisAssembly.GetTypes();
                foreach (Type T in TheseTypes)
                {
                    if (T.Name.ToUpper() == TypeName.ToUpper())
                    {
                        TypeName = T.FullName;
                        break;
                    }
                }
            }
            Type EntityType = Type.GetType(TypeName, false, false);
            if (EntityType == null) { return null; }
            ConstructorInfo[] ThisCInfo = EntityType.GetConstructors();
            foreach (ConstructorInfo C in ThisCInfo)
            {
                if (C.GetParameters().Length == ConstructorArgs.Length)
                {
                    Boolean SetAndMake = true;
                    for (int i = 0; i < ConstructorArgs.Length; i++)
                    {
                        if (C.GetParameters()[i].ParameterType != ConstructorArgs[i].GetType())
                        {
                            SetAndMake = false;
                            break;
                        }
                    }
                    if (SetAndMake)
                    {
                        return C.Invoke(ConstructorArgs);
                    }
                }
            }
            return null;
        }
        public static WorldEntity Process(String SchemeParams, String Identifier, WorldEntity CurrentEnt)
        {
            if(Identifier == "new")
            {
                Object[] FullConstructorInfo = ExtractMethodValues(SchemeParams);
                String EntName = (String)FullConstructorInfo[0];
                Object[] Args = (Object[])FullConstructorInfo[1];
                WorldEntity NewEntity = ConstructDynamicWorldEntity(EntName, Args);
                return NewEntity;
            }
            else
            {
                Type CurrentType = CurrentEnt.GetType();
                String ParameterBody = Identifier;
                String BaseIdentifier = "";
                if(Identifier.Contains("."))
                {
                    ParameterBody = Identifier.Remove(0, Identifier.IndexOf('.') + 1);
                    BaseIdentifier = Identifier.Remove(Identifier.IndexOf('.') + 1);
                }
                if (BaseIdentifier.ToUpper() == "SHELL")
                {
                    if (ParameterBody.ToUpper().Contains("UPDATEQUEUE")) { Shell.UpdateQueue.Add(CurrentEnt); }
                    if (ParameterBody.ToUpper().Contains("RENDERQUEUE")) { Shell.RenderQueue.Add(CurrentEnt); }
                }
                else
                {
                    if (SchemeParams.Length == 0)
                    {
                        ReturnMemberOrFuncValue(ParameterBody, CurrentEnt, null);
                    }
                    else
                    {
                        ReturnMemberOrFuncValue(ParameterBody, CurrentEnt, ParseRealData(SchemeParams));
                    }
                }
                return CurrentEnt;
            }
        }
        public static WorldEntity Assemble(String Schema)
        {
            WorldEntity ConstructedEntity = null;
            String[] Schemas = SelectiveStringOps.RemoveExclosed(Schema, ' ', '\"').Split('\n');
            foreach(String S in Schemas)
            {
                String RS = S;
                if (!RS.Contains("=")) { RS = RS + "="; }
                String[] SplitScheme = SelectiveStringOps.SplitAtExclosed(S, '=', '\"');
                String Identifier = SplitScheme[0];
                if (Identifier == "new") { ConstructedEntity = Process(SplitScheme[1], Identifier, null); }
                else { ConstructedEntity = Process(SplitScheme[1], Identifier, ConstructedEntity); }
            }
            return ConstructedEntity;
        }
    }
}