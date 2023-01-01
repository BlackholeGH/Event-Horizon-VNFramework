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
    /// <summary>
    /// The EntityFactory class contains methods that allow objects or delegates to be constructed, or functions to be executed, from String input.
    /// It constitutes what is essentially a rudimentary code interpreter to augment the VNF scripting system.
    /// </summary>
    public static class EntityFactory
    {        
        /// <summary>
        /// Returns a method identifier and "true" parsed parameter values from a String.
        /// </summary>
        /// <param name="MethodOrConstructorData">The method or constructor definition as a String.</param>
        /// <returns></returns>
        public static Object[] ExtractMethodValues(String MethodOrConstructorData)
        {
            String IsolatedIdentifier = MethodOrConstructorData.Remove(MethodOrConstructorData.IndexOf('('));
            String IsolatedParamList = MethodOrConstructorData.Remove(0, MethodOrConstructorData.IndexOf('(') + 1);
            IsolatedParamList = IsolatedParamList.Remove(IsolatedParamList.LastIndexOf(')'));
            Object[] TrueParameters = new Object[0];
            if (IsolatedParamList.Length > 0) { TrueParameters = ParseDataList(IsolatedParamList); }
            return new Object[] { IsolatedIdentifier, TrueParameters };
        }
        /// <summary>
        /// Returns an array of parsed data from a String formatted as a list, comma separated.
        /// </summary>
        /// <param name="DataList">Data formatted as a comma-separated list String.</param>
        /// <returns></returns>
        public static Object[] ParseDataList(String DataList)
        {
            String[] PriorToParse = VNFUtils.Strings.SplitAtExclosed(DataList, ',', new char[] { '(', '{' }, new char[] { ')', '}' }, '\"');
            Object[] Out = new Object[PriorToParse.Length];
            for(int i = 0; i < PriorToParse.Length; i++)
            {
                Out[i] = ParseRealData(PriorToParse[i]);
            }
            return Out;
        }
        /// <summary>
        /// Returns or sets the value of a member or method of a given class from a String input.
        /// </summary>
        /// <param name="PathTree">The class/method/member path.</param>
        /// <param name="PresumptiveEntity">An initial WorldEntity instance on which to perform the path tree search.</param>
        /// <param name="ExplicitSet">If non-null, the specified instance member will be set to this object or value.</param>
        /// <returns></returns>
        public static Object ReturnMemberOrFuncValue(String PathTree, WorldEntity PresumptiveEntity, Object ExplicitSet)
        {
            String DP = PathTree.TrimEnd(';');
            String TempDP = VNFUtils.Strings.ReplaceExclosed(DP, "(", "@", '\"');
            TempDP = VNFUtils.Strings.ReplaceExclosed(TempDP, ")", "@", '\"');
            TempDP = VNFUtils.Strings.ReplaceExclosedOutestTier(TempDP, ".", "&", '@');
            int Index = 0;
            foreach(Char c in DP)
            {
                if (TempDP[Index] == '@' && (c == '(' || c == ')')) { TempDP = TempDP.Remove(Index) + c + TempDP.Remove(0, Index + 1); }
                Index++;
            }
            DP = TempDP;
            Type SuperType = typeof(ScriptProcessor);
            Object InstancedObject = null;
            if (DP.ToUpper().StartsWith("TYPEOF("))
            {
                String TypeNameString = DP.Remove(DP.IndexOf(')')).Remove(0, 7);
                InstancedObject = VNFUtils.TypeOfNameString(TypeNameString, true);
                DP = DP.Remove(0, DP.IndexOf(')') + 2);
            }
            String StaticMemberTree = DP;
            if (PresumptiveEntity is null && InstancedObject is null)
            {
                if (DP.Contains("&"))
                {
                    String TypeToFind = DP;
                    TypeToFind = TypeToFind.Remove(TypeToFind.LastIndexOf('&'));
                    if(!TypeToFind.StartsWith("VNFramework&") && !TypeToFind.StartsWith("System&")) { TypeToFind = "VNFramework&" + TypeToFind; }
                    String TrueTTF = VNFUtils.Strings.ReplaceExclosed(TypeToFind, "&", ".", '\"');
                    TrueTTF = TrueTTF.Replace("\\+", "+");
                    Type Find = typeof(Shell).Assembly.GetType(TrueTTF);
                    if (Find == null) { Find = Type.GetType(TrueTTF); }
                    if (Find == null) { Find = typeof(System.Type).Assembly.GetType(TrueTTF); }
                    if (Find != null) { SuperType = Find; }
                    String TTFPreStaticSearch = TypeToFind;
                    while (!(SuperType.IsAbstract && SuperType.IsSealed))
                    {
                        if(SuperType == typeof(Shell))
                        {
                            InstancedObject = Shell.DefaultShell;
                            break;
                        }
                        TypeToFind = TypeToFind.Remove(TypeToFind.LastIndexOf('&'));
                        if(TypeToFind == "VNFramework" || TypeToFind.Length == 0)
                        {
                            TypeToFind = TTFPreStaticSearch;
                            break;
                        }
                        Find = Type.GetType(VNFUtils.Strings.ReplaceExclosed(TypeToFind, "&", ".", '\"'));
                        if (Find != null) { SuperType = Find; }
                        else
                        {
                            SuperType = null;
                            break;
                        }
                    }
                    if(!DP.StartsWith("VNFramework&") && TypeToFind.StartsWith("VNFramework&")) { TypeToFind = TypeToFind.Remove(0, 12); }
                    if (TypeToFind.Length > 0)
                    {
                        if (DP.StartsWith(TypeToFind)) { StaticMemberTree = DP.Remove(0, TypeToFind.Length); }
                    }
                    StaticMemberTree = StaticMemberTree.TrimStart('&');
                }
            }
            else if(InstancedObject is null)
            {
                InstancedObject = PresumptiveEntity;
            }
            String[] TreeElements = StaticMemberTree.Split('&');
            int Count = 0;
            foreach (String S in TreeElements)
            {
                Count++;
                /*
                 * Currently, a function return is assumed to be at the end of a parse tree.
                 * The entity factory interpreter isn't really designed to handle consecutive function calls (nested are fine, and are what this is for).
                 * However, future changes to allow this are likely possible by setting InstancedObject to the return from the Invoke calls instead of simply returning them.
                 * Doing that might just work out of the box, but I don't want to try it now and end up breaking this code again.
                 */
                if (S.Contains("(") && S.Contains(")"))
                {
                    String IsolatedIdentifier = S.Remove(S.IndexOf('('));
                    String IsolatedParamList = S.Remove(0, S.IndexOf('(') + 1);
                    IsolatedParamList = IsolatedParamList.Remove(IsolatedParamList.LastIndexOf(')'));
                    Object[] TrueParameters = new Object[0];
                    if (IsolatedParamList.Length > 0) { TrueParameters = ParseDataList(IsolatedParamList); }
                    if(InstancedObject is null)
                    {
                        foreach (var me in SuperType.GetMethods())
                        {
                            if (me.Name == IsolatedIdentifier && me.GetParameters().Length == TrueParameters.Length)
                            {
                                Boolean InvocationFlag = true;
                                int i = 0;
                                foreach(ParameterInfo pi in me.GetParameters())
                                {
                                    Type ParamType = pi.ParameterType;
                                    Type GivenType = TrueParameters[i].GetType();
                                    if (!(CanConvert(ParamType, GivenType) || ParamType.IsAssignableFrom(GivenType)))
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
                            if (me.Name == IsolatedIdentifier && me.GetParameters().Length == TrueParameters.Length)
                            {
                                Boolean InvocationFlag = true;
                                int i = 0;
                                foreach (ParameterInfo pi in me.GetParameters())
                                {
                                    Type ParamType = pi.ParameterType;
                                    if (!(TrueParameters[i] is null))
                                    {
                                        Type GivenType = TrueParameters[i].GetType();
                                        if (!(CanConvert(ParamType, GivenType) || ParamType.IsAssignableFrom(GivenType)))
                                        {
                                            InvocationFlag = false;
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        if (ParamType.IsValueType && Nullable.GetUnderlyingType(ParamType) == null)
                                        {
                                            InvocationFlag = false;
                                            break;
                                        }
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
                        if (InstancedObject != null) { continue; }
                        else if (Count == TreeElements.Length)
                        {
                            foreach (var method in SuperType.GetMethods())
                            {
                                if (method.Name == S)
                                {
                                    return method;
                                }
                            }
                        }
                        else if (InstancedObject == null) { return null; }
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
                        if (InstancedObject != Initial) { continue; }
                        else if (Count == TreeElements.Length)
                        {
                            foreach (var method in Initial.GetType().GetMethods())
                            {
                                if (method.Name == S)
                                {
                                    return new object[] { method, Initial };
                                }
                            }
                        }
                        else if (InstancedObject == Initial) { return null; }
                    }
                }
            }
            #pragma warning disable CS0252 // Possible unintended reference comparison; left hand side needs cast
            return InstancedObject != PresumptiveEntity ? InstancedObject : null;
            #pragma warning restore CS0252 // Possible unintended reference comparison; left hand side needs cast
        }
        /// <summary>
        /// Returns true if a String statement contains an internal basic data operator.
        /// </summary>
        /// <param name="DataStatement">A statement expressed as a String.</param>
        /// <returns></returns>
        public static Boolean PerformsInternalOperation(String DataStatement)
        {
            if(VNFUtils.Strings.IndexOfExclosed(DataStatement.Replace("\\+", ""), "+", '(', ')', '\"') > 0) { return true; }
            else if (VNFUtils.Strings.IndexOfExclosed(DataStatement.Replace("\\-", ""), "-", '(', ')', '\"') > 0) { return true; }
            else if (VNFUtils.Strings.IndexOfExclosed(DataStatement.Replace("\\*", ""), "*", '(', ')', '\"') > 0) { return true; }
            else if (VNFUtils.Strings.IndexOfExclosed(DataStatement.Replace("\\/", ""), "/", '(', ')', '\"') > 0) { return true; }
            else { return false; }
        }
        /// <summary>
        /// Splits a single String statement into an array of substatements and internal data operators.
        /// </summary>
        /// <param name="DataStatement">A statement expressed as a String.</param>
        /// <returns></returns>
        public static object[] ExtractStatementsAndOperators(String DataStatement)
        {
            ArrayList Out = new ArrayList();
            while(PerformsInternalOperation(DataStatement))
            {
                int FirstOpLoc = -1;
                char[] Operators = new char[] { '+', '-', '*', '/' };
                foreach(char Opr in Operators)
                {
                    int Ind = VNFUtils.Strings.IndexOfExclosed(DataStatement, new String(new char[] { Opr }), '(', ')', '\"');
                    String NewSearch = DataStatement;
                    while (Ind > 0 && DataStatement[Ind - 1] == '\\')
                    {
                        NewSearch = NewSearch.Remove(Ind) + "@" + NewSearch.Remove(0, Ind+1);
                        Ind = VNFUtils.Strings.IndexOfExclosed(NewSearch, new String(new char[] { Opr }), '(', ')', '\"');
                    }
                    if (Ind > 0 && (Ind < FirstOpLoc || FirstOpLoc < 0)) { FirstOpLoc = Ind; }
                }
                Out.Add(DataStatement.Remove(FirstOpLoc));
                DataStatement = DataStatement.Remove(0, FirstOpLoc);
                Out.Add(DataStatement[0]);
                DataStatement = DataStatement.Remove(0, 1);
            }
            if (DataStatement.Length > 0) { Out.Add(DataStatement); }
            return Out.ToArray();
        }
        /// <summary>
        /// Parses data from a single statement formatted as a String, including interpreting and resolving instant declarations and nested functions.
        /// </summary>
        /// <param name="DataParameter">Data in a String format.</param>
        /// <returns></returns>
        public static Object ParseRealData(String DataParameter)
        {
            try
            {
                if(DataParameter.Length == 0) { return null; }
                String DP = DataParameter.Replace(";", "");
                //Alert - this removes casts but is a little dirty, because it will blanket remove any nested statement. Beware when formatting data declarations. Will need to be changed for better math engine.
                if (DP[0] == '(' && DP.Contains(")"))
                {
                    DP = DP.Remove(0, VNFUtils.Strings.IndexOfExclosed(DP, ")", '\"') + 1);
                }
                if(PerformsInternalOperation(DataParameter))
                {
                    object[] StatementsAndOps = ExtractStatementsAndOperators(DataParameter);
                    for(int i = 0; i < StatementsAndOps.Length; i++)
                    {
                        if(StatementsAndOps[i] is String)
                        {
                            object TrueVal = ParseRealData((String)StatementsAndOps[i]);
                            StatementsAndOps[i] = TrueVal;
                        }
                    }
                    //Currently, the Entity factory's statement assembler does not process statements inside brackets first, and operations are performed left to right.
                    if(StatementsAndOps.Length == 0) { return null; }
                    object RunningTotal = StatementsAndOps[0];
                    for(int i = 1; i < StatementsAndOps.Length; i += 2)
                    {
                        object Operand2 = StatementsAndOps[i + 1];
                        switch(StatementsAndOps[i])
                        {
                            case '+':
                                RunningTotal = VNFUtils.MultiAdd(RunningTotal, Operand2);
                                break;
                            case '-':
                                RunningTotal = VNFUtils.MultiSubtract(RunningTotal, Operand2);
                                break;
                            case '*':
                                RunningTotal = VNFUtils.MultiMultiply(RunningTotal, Operand2);
                                break;
                            case '/':
                                RunningTotal = VNFUtils.MultiDivide(RunningTotal, Operand2);
                                break;
                        }
                    }
                    return RunningTotal;
                }
                else if (!(VNFUtils.Strings.ContainsExclosed(DP, '(', '\"') && VNFUtils.Strings.ContainsExclosed(DP, ')', '\"')) && !(VNFUtils.Strings.ContainsExclosed(DP, '[', '\"') && VNFUtils.Strings.ContainsExclosed(DP, ']', '\"')))
                {
                    if (DP[0] == '\"' && DP[DP.Length - 1] == '\"') { return DP.Substring(1, DP.Length - 2); }
                    else
                    {
                        float f;
                        if (float.TryParse(DP.TrimEnd(new char[] { 'f', 'd' }), out f))
                        {
                            if (DP.Contains("f") || (DP.Contains(".") && !DP.Contains("d"))) { return float.Parse(DP.Replace("f", "")); }
                            else if (DP.Contains("d")) { return double.Parse(DP.Replace("d", "")); }
                            else { return int.Parse(DP); }
                        }
                        else if(DP.ToUpper() == "TRUE" || DP.ToUpper() == "FALSE")
                        {
                            return DP.ToUpper() == "TRUE";
                        }
                        else if(DP.ToUpper() == "NULL")
                        {
                            return null;
                        }
                        else
                        {
                            return ReturnMemberOrFuncValue(DP, null, null);
                        }
                    }
                }
                else if (VNFUtils.Strings.ContainsExclosed(DP, '(', '\"') && VNFUtils.Strings.ContainsExclosed(DP, ')', '\"'))
                {
                    if (DP.ToLower().StartsWith("new"))
                    {
                        String IsolatedIdentifier = DP.Remove(0, 3);
                        IsolatedIdentifier = IsolatedIdentifier.Remove(IsolatedIdentifier.IndexOf('('));
                        String IsolatedParamList = DP.Remove(0, DP.IndexOf('(') + 1);
                        IsolatedParamList = IsolatedParamList.Remove(IsolatedParamList.LastIndexOf(')'));
                        Object[] TrueParameters = new Object[0];
                        if (IsolatedParamList.Length > 0) { TrueParameters = ParseDataList(IsolatedParamList); }
                        return ConstructDynamicObject(IsolatedIdentifier, TrueParameters, true);
                    }
                    else
                    {
                        return ReturnMemberOrFuncValue(DP, null, null);
                    }
                }
                else if (VNFUtils.Strings.ContainsExclosed(DP, '[', '\"') && VNFUtils.Strings.ContainsExclosed(DP, ']', '\"'))
                {
                    if (DP.ToLower().StartsWith("new"))
                    {
                        String IsolatedIdentifier = DP.Remove(0, 3);
                        IsolatedIdentifier = IsolatedIdentifier.Remove(IsolatedIdentifier.IndexOf('['));
                        String IsolatedLengthOrNone = DP.Remove(0, DP.IndexOf('[') + 1);
                        IsolatedLengthOrNone = IsolatedLengthOrNone.Remove(IsolatedLengthOrNone.LastIndexOf(']'));
                        String IsolatedCurls = null;
                        if (VNFUtils.Strings.ContainsExclosed(DP, '{', '\"') && VNFUtils.Strings.ContainsExclosed(DP, '}', '\"'))
                        {
                            IsolatedCurls = DP.Remove(0, DP.IndexOf('{') + 1);
                            IsolatedCurls = IsolatedCurls.Remove(IsolatedCurls.LastIndexOf('}'));
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
                                RLength = VNFUtils.Strings.SplitAtExclosed(IsolatedCurls, ',', '\"').Length;
                            }
                            Dictionary<String, String> AliasLookup = VNFUtils.TypeAliasLookup();
                            if(AliasLookup.ContainsKey(IsolatedIdentifier)) { IsolatedIdentifier = AliasLookup[IsolatedIdentifier]; }
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
                            Array TrueTypeArray = Array.CreateInstance(RType, TempR.Length);
                            Array.Copy(TempR, TrueTypeArray, TempR.Length);
                            return TrueTypeArray;
                        }
                    }
                    else
                    {
                        String IndexableIdentifier = DP.Remove(DP.IndexOf('['));
                        String IndexValue = DP.Remove(0, DP.IndexOf('[') + 1);
                        IndexValue = IndexValue.Remove(IndexValue.LastIndexOf(']'));
                        Object TrueIndex = ParseRealData(IndexValue);
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
            catch (ArgumentException) { return null; }
        }
        /// <summary>
        /// Dynamically constructs a WorldEntity or subclass based on the type name and provided constructor arguments.
        /// </summary>
        /// <param name="TypeName">The simple name of a WorldEntity type.</param>
        /// <param name="ConstructorArgs">An array of constructor arguments.</param>
        /// <returns></returns>
        public static WorldEntity ConstructDynamicWorldEntity(String TypeName, Object[] ConstructorArgs)
        {
            TypeName = TypeName.Replace("\\+", "+");
            if (!TypeName.ToUpper().Contains("VNFRAMEWORK.")) { TypeName = "VNFramework." + TypeName; }
            if (TypeName.ToUpper() != "VNFRAMEWORK.WORLDENTITY")
            {
                if(TypeName.Contains(".")) { TypeName = TypeName.Remove(0, TypeName.LastIndexOf('.') + 1); }
                Assembly ThisAssembly = Assembly.GetExecutingAssembly();
                Assembly VNFAssembly = typeof(Shell).Assembly;
                Type[] TheseTypes = ThisAssembly.GetTypes();
                if(ThisAssembly != VNFAssembly)
                {
                    TheseTypes = TheseTypes.Concat(VNFAssembly.GetTypes()).ToArray();
                }
                foreach(Type T in TheseTypes)
                {
                    if(T.IsSubclassOf(typeof(WorldEntity)) && T.Name == TypeName)
                    {
                        TypeName = T.AssemblyQualifiedName;
                        break;
                    }
                }
            }
            return (WorldEntity)ConstructDynamicObject(TypeName, ConstructorArgs, false);
        }
        /// <summary>
        /// Returns true if two types can be (directly) interconverted.
        /// </summary>
        /// <param name="Type1">The first type.</param>
        /// <param name="Type2">The second type.</param>
        /// <returns></returns>
        public static Boolean CanConvert(Type Type1, Type Type2)
        {
            Type PosNul1 = Nullable.GetUnderlyingType(Type1);
            Type PosNul2 = Nullable.GetUnderlyingType(Type2);
            if (PosNul1 != null) { Type1 = PosNul1; }
            if (PosNul2 != null) { Type2 = PosNul2; }
            if (Type1 == Type2) { return true; }
            try
            {
                Convert.ChangeType(Activator.CreateInstance(Type1), Type2);
                return true;
            }
            catch (Exception) { return false; }
        }
        /// <summary>
        /// Dynamically constructs and returns an object based on the type name and constructor arguments.
        /// </summary>
        /// <param name="TypeName">Either a qualified type name, or a simple type name to search for.</param>
        /// <param name="ConstructorArgs">Arguments for the type constructor.</param>
        /// <param name="BroadSearch">If true, the type name will be used as a search parameter and all project assemblies will be searched for a matching type name.</param>
        /// <returns></returns>
        public static Object ConstructDynamicObject(String TypeName, Object[] ConstructorArgs, Boolean BroadSearch)
        {
            Type EntityType = VNFUtils.TypeOfNameString(TypeName, BroadSearch);
            if (EntityType == null) { return null; }
            ConstructorInfo[] ThisCInfo = EntityType.GetConstructors();
            foreach (ConstructorInfo C in ThisCInfo)
            {
                if (C.GetParameters().Length == ConstructorArgs.Length)
                {
                    Boolean SetAndMake = true;
                    for (int i = 0; i < ConstructorArgs.Length; i++)
                    {
                        Type ParamType = C.GetParameters()[i].ParameterType;
                        Type GivenType = ConstructorArgs[i].GetType();
                        if (!(CanConvert(ParamType, GivenType) || ParamType.IsAssignableFrom(GivenType)))
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
        /// <summary>
        /// Processes an entity factory command, either creating or ammending a constructed WorldEntity.
        /// </summary>
        /// <param name="SchemeParams">The paramters/definition for the current scheme; the operation to be performed in the factory, as a String.</param>
        /// <param name="Identifier">An optional scheme identifier String, to specify unique operations such as new entity creation or retrieval of existing entities.</param>
        /// <param name="CurrentEnt">The current working WorldEntity. Null if there is no current entity.</param>
        /// <returns></returns>
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
            else if (Identifier == "get")
            {
                object NewEntity = (WorldEntity)ReturnMemberOrFuncValue(SchemeParams, null, null);
                if (NewEntity is WorldEntity) { return (WorldEntity)NewEntity; }
                else
                {
                    return null;
                }
            }
            else
            {
                Type CurrentType = CurrentEnt.GetType();
                String ParameterBody = Identifier;
                String BaseIdentifier = "";
                if(Identifier.Contains("."))
                {
                    ParameterBody = Identifier.Remove(0, Identifier.IndexOf('.') + 1);
                    BaseIdentifier = Identifier.Remove(Identifier.IndexOf('.'));
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
        /// <summary>
        /// Assembles a WorldEntity object based on a String schema/blueprint.
        /// </summary>
        /// <param name="Schema">The String "blueprint" for a WorldEntity.</param>
        /// <returns></returns>
        public static WorldEntity Assemble(String Schema)
        {
            WorldEntity ConstructedEntity = null;
            String[] Schemas = VNFUtils.Strings.RemoveExclosed(Schema, ' ', '\"').Split('\n');
            foreach(String S in Schemas)
            {
                String RS = S;
                if (!RS.Contains("=")) { RS = RS + "="; }
                String[] SplitScheme = VNFUtils.Strings.SplitAtExclosed(RS, '=', '\"');
                String Identifier = SplitScheme[0];
                if (Identifier == "new") { ConstructedEntity = Process(SplitScheme[1], Identifier, null); }
                else { ConstructedEntity = Process(SplitScheme[1], Identifier, ConstructedEntity); }
            }
            return ConstructedEntity;
        }
        /// <summary>
        /// Assmbles an anonymous delegate void of delegate type VoidDel based on a given String schema/blueprint.
        /// </summary>
        /// <param name="Schema">A blueprint for the delegate to be created.</param>
        /// <returns></returns>
        public static VoidDel AssembleVoidDelegate(String Schema)
        {
            VoidDel NewVoidDelegate = new VoidDel(delegate() { });
            String VDSchema = VNFUtils.Strings.RemoveExclosed(Schema, ' ', '\"').Split('\n')[0];
            if (VDSchema.StartsWith("do="))
            {
                NewVoidDelegate = new VoidDel(delegate () { ReturnMemberOrFuncValue(VDSchema.Remove(0, 3), null, null); });
            }
            else
            {
                object ExtractMethod = ParseRealData(VDSchema);
                if (ExtractMethod is MethodInfo)
                {
                    MethodInfo Method = ((MethodInfo)ExtractMethod);
                    if (Method.GetParameters().Length == 0) { NewVoidDelegate = new VoidDel(delegate () { Method.Invoke(null, null); }); }
                }
                else if (ExtractMethod is VoidDel)
                {
                    NewVoidDelegate = (VoidDel)ExtractMethod;
                }
                else if (ExtractMethod is object[])
                {
                    object[] EM = (object[])ExtractMethod;
                    if (EM[0] is MethodInfo)
                    {
                        MethodInfo Method = (MethodInfo)EM[0];
                        object OpObj = EM[1];
                        if (Method.GetParameters().Length == 0)
                        {
                            if (Method.DeclaringType == OpObj.GetType())
                            {
                                NewVoidDelegate = new VoidDel(delegate () { Method.Invoke(OpObj, null); });
                            }
                            else
                            {
                                NewVoidDelegate = new VoidDel(delegate () { Method.Invoke(null, null); });
                            }
                        }
                    }
                }
            }
            return NewVoidDelegate;
        }
    }
}