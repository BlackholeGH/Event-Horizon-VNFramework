using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using Microsoft.Xna.Framework;
using System.Windows.Forms;

namespace VNFramework
{
    public static class PythonController
    {
        public static void Test()
        {
            PythonController.StartPythonProcess("C:\\Users\\Blackhole\\AppData\\Local\\Programs\\Python\\Python311\\python.exe");
            Console.WriteLine("Python process started. Press enter to continue.");
            Console.ReadLine();
            ulong key = PythonController.SocketInterface.AddNewSocketAsTask();
            ulong key2 = PythonController.SocketInterface.AddNewSocketAsTask();
            Console.WriteLine("Added socket: " + key);
            Console.WriteLine("Added socket: " + key2);
            while (true)
            {
                foreach (ulong curKey in PythonController.SocketInterface.OpenSockets)
                {
                    Console.WriteLine("Talking on socket: " + curKey);
                    Console.WriteLine("Enter data to send: ");
                    String dat = Console.ReadLine();
                    byte[] toSend = Encoding.UTF8.GetBytes(dat);
                    PythonController.SocketInterface.SendQuery(key, toSend, false);
                    Console.WriteLine("Data set. Hit enter to pull return if available.");
                    Console.ReadLine();
                    Console.WriteLine(Encoding.UTF8.GetString(PythonController.SocketInterface.GetQuery(key).Receive));
                }
            }
        }
        private static readonly string s_pyExecutable = "C:\\Users\\Blackhole\\AppData\\Local\\Programs\\Python\\Python311\\python.exe";
        public static Process StartPythonProcess(string pyScript)
        {
            ProcessStartInfo pyStart = new ProcessStartInfo();
            pyStart.FileName = s_pyExecutable;
            pyStart.Arguments = "-u " + pyScript;
            //pyStart.Arguments = pyScript;
            pyStart.UseShellExecute = false;
            pyStart.RedirectStandardOutput = true;
            Shell.WriteLine("Starting Python process at: " + pyScript);
            Process outProcess = Process.Start(pyStart);
            Shell.ActiveProcesses.Add(outProcess);
            outProcess.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
            {
                Shell.WriteLine("Python script " + pyScript.Remove(0, pyScript.LastIndexOf('\\') + 1) + ": " + e.Data);
                if(e.Data == "SOCKETS_OPEN_AND_READY")
                {
                    SocketInterface.SocketsOpenedFlag = true;
                }
                if (e.Data == "SYSTEM_SOCKET_ASSIGNED")
                {
                    IterativeMemBoTs.SystemSocketAssigned = true;
                }
            };
            outProcess.Exited += (object sender, EventArgs e) =>
            {
                SocketInterface.CloseAllSockets();
                SocketInterface.SocketsOpenedFlag = false;
                IterativeMemBoTs.SystemSocketAssigned = false;
            };
            outProcess.BeginOutputReadLine();
            return outProcess;
        }
        public static class SocketInterface
        {
            /// <summary>
            /// WorldEntity that will close the system socket and kill a socket host process when it is deleted or disposed.
            /// </summary>
            [Serializable]
            public class SocketCloserEntity : WorldEntity
            {
                [NonSerialized]
                Process _processToClose = null;
                public SocketCloserEntity(String name, Process processToClose) : base(name, new Vector2(), null, 0)
                {
                    _processToClose = processToClose;
                }
                Boolean _systemSocketsClosed = false;
                public override void ManualDispose()
                {
                    if (OpenSockets.Contains(0) && !_systemSocketsClosed)
                    {
                        CloseSocket(0);
                        SocketInterface.SocketsOpenedFlag = false;
                        IterativeMemBoTs.SystemSocketAssigned = false;
                        _systemSocketsClosed = true;
                    }
                    if (!(_processToClose is null))
                    {
                        Shell.WriteLine("Killing socket process via closer object.");
                        _processToClose.Kill();
                        _processToClose.Close();
                        if (Shell.ActiveProcesses.Contains(_processToClose)) { Shell.ActiveProcesses.Remove(_processToClose); }
                        _processToClose = null;
                    }
                    base.ManualDispose();
                }
            }
            private static Boolean s_socketsOpenedFlag = false;
            private static object s_socketLock = new object();
            public static Boolean SocketsOpenedFlag
            {
                get
                {
                    lock (s_socketLock)
                    {
                        return s_socketsOpenedFlag;
                    }
                }
                set
                {
                    lock (s_socketLock)
                    {
                        s_socketsOpenedFlag = value;
                    }
                }
            }
            [Serializable]
            public struct PySocketQuery
            {
                public PySocketQuery() { }
                public byte[] Send = new byte[0];
                public byte[] Receive = new byte[0];
                public Boolean LastSend = false;
                public Boolean LastReceive = false;
                public int AllowedSendAttempts = -1;
                public int AttemptReceiveAttempts = 1;
            }
            public static ulong CurrentSocketID
            {
                get;
                private set;
            }
            private static Dictionary<ulong, Socket> s_socketSockets = new Dictionary<ulong, Socket>();
            private static Dictionary<ulong, Task> s_socketTasks = new Dictionary<ulong, Task>();
            private static Dictionary<ulong, PySocketQuery> s_queries = new Dictionary<ulong, PySocketQuery>();
            private static Dictionary<ulong, Queue<byte[]>> s_dataSendQueue = new Dictionary<ulong, Queue<byte[]>>();
            public static List<ulong> OpenSockets
            {
                get
                {
                    return new List<ulong>(s_socketSockets.Keys);
                }
            }
            public static void AcknowledgeReceive(ulong key)
            {
                lock(s_queries)
                {
                    if(s_queries.ContainsKey(key))
                    {
                        PySocketQuery current = s_queries[key];
                        current.LastReceive = false;
                        s_queries[key] = current;
                    }
                }
            }
            public static PySocketQuery GetQuery(ulong key)
            {
                PySocketQuery output = new PySocketQuery();
                lock (s_queries)
                {
                    if (s_queries.ContainsKey(key)) { output = s_queries[key]; }
                }
                return output;
            }
            public static void SendQuery(ulong key, byte[] data, Boolean allowEnqueue)
            {
                SendQuery(key, data, allowEnqueue, -1, 1);
            }
            public static void SendQuery(ulong key, byte[] data, Boolean allowEnqueue, int allowedSendAttempts)
            {
                SendQuery(key, data, allowEnqueue, allowedSendAttempts, 1);
            }
            public static void SendQuery(ulong key, byte[] data, Boolean allowEnqueue, int allowedSendAttempts, int attemptReceiveAttempts)
            {
                if (allowEnqueue) { Shunt(); }
                if (data.Length > 1024) { return; }
                else
                {
                    if (s_dataSendQueue.ContainsKey(key) && s_dataSendQueue[key].Count > 0 && allowEnqueue)
                    {
                        s_dataSendQueue[key].Enqueue(data);
                    }
                    else
                    {
                        PySocketQuery query;
                        lock (s_queries)
                        {
                            if(!s_queries.ContainsKey(key)) { return; }
                            query = s_queries[key];
                        }
                        if (query.LastSend)
                        {
                            if (allowEnqueue) { s_dataSendQueue[key].Enqueue(data); }
                        }
                        else
                        {
                            query.LastSend = true;
                            query.LastReceive = false;
                            query.Send = data;
                            query.AllowedSendAttempts = allowedSendAttempts;
                            query.AttemptReceiveAttempts = attemptReceiveAttempts;
                            lock (s_queries)
                            {
                                s_queries[key] = query;
                            }
                        }
                    }
                }
            }
            public static void Shunt()
            {
                PySocketQuery query;
                foreach (ulong key in s_dataSendQueue.Keys)
                {
                    if (s_dataSendQueue[key].Count > 0)
                    {
                        lock (s_queries)
                        {
                            query = s_queries[key];
                            if (query.LastSend) { continue; }
                            else
                            {
                                query.Send = s_dataSendQueue[key].Dequeue();
                                query.LastReceive = false;
                                query.LastSend = true;
                                s_queries[key] = query;
                            }
                        }
                    }
                }
            }
            public static ulong AddNewSocketAsTask()
            {
                IPEndPoint ipEndPoint = new IPEndPoint(new IPAddress(new byte[] { 127, 0, 0, 1 }), 65432);
                ulong thisKey = CurrentSocketID;
                CurrentSocketID++;
                Socket newSocket = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                PySocketQuery queryObj = new PySocketQuery();
                queryObj.LastSend = false;
                queryObj.LastReceive = false;
                Task socketIOTask = new Task(async () =>
                {
                    ulong myKey = thisKey;
                    Socket mySocket = newSocket;
                    mySocket.SendTimeout = -1;
                    mySocket.ReceiveTimeout = 1000;
                    PySocketQuery myQuery;
                    byte[] send;
                    byte[] buffer = new byte[1024];
                    try
                    {
                        await mySocket.ConnectAsync(ipEndPoint);
                    }
                    catch(Exception e)
                    {
                        Shell.WriteLine("Socket connection " + myKey + " encountered an error while trying to connect. The socket task will now close.");
                    }
                    while (mySocket.Connected)
                    {
                        try
                        {
                            lock (s_queries)
                            {
                                myQuery = s_queries[myKey];
                            }
                            if (myQuery.LastSend)
                            {
                                //Console.WriteLine("Socket " + myKey + " is sending!");
                                if (!mySocket.Connected) { break; }
                                bool expectReceive = false;
                                if (myQuery.AllowedSendAttempts == -1)
                                {
                                    send = myQuery.Send;
                                    mySocket.Send(send, SocketFlags.None);
                                    expectReceive = true;
                                }
                                else if (myQuery.AllowedSendAttempts > 0)
                                {
                                    send = myQuery.Send;
                                    mySocket.Send(send, SocketFlags.None);
                                    myQuery.AllowedSendAttempts--;
                                    lock (s_queries)
                                    {
                                        s_queries[myKey] = myQuery;
                                    }
                                    expectReceive = true;
                                }
                                else if (myQuery.AllowedSendAttempts == 0)
                                {
                                    myQuery.LastSend = false;
                                    myQuery.LastReceive = false;
                                    myQuery.Send = new byte[1024];
                                    myQuery.Receive = new byte[1024];
                                    myQuery.AllowedSendAttempts = -1;
                                    myQuery.AttemptReceiveAttempts = 1;
                                    lock (s_queries)
                                    {
                                        s_queries[myKey] = myQuery;
                                    }
                                }
                                if (expectReceive)
                                {
                                    buffer = new byte[1024];
                                    while (myQuery.AttemptReceiveAttempts > 0 && !myQuery.LastReceive)
                                    {
                                        try
                                        {
                                            int receiveCode = mySocket.Receive(buffer, SocketFlags.None);
                                            myQuery.LastSend = false;
                                            myQuery.LastReceive = true;
                                            myQuery.Send = new byte[1024];
                                            myQuery.Receive = buffer;
                                            myQuery.AllowedSendAttempts = -1;
                                            myQuery.AttemptReceiveAttempts = 1;
                                            lock (s_queries)
                                            {
                                                s_queries[myKey] = myQuery;
                                            }
                                        }
                                        catch (SocketException e)
                                        {
                                            Console.WriteLine(myKey + " failed receive: " + (myQuery.AttemptReceiveAttempts - 1) + " left.");
                                            myQuery.AttemptReceiveAttempts -= 1;
                                            if(myQuery.AttemptReceiveAttempts <= 0)
                                            {
                                                myQuery.LastSend = false;
                                                myQuery.LastReceive = false;
                                                myQuery.Send = new byte[1024];
                                                myQuery.Receive = new byte[1024];
                                                myQuery.AllowedSendAttempts = -1;
                                                myQuery.AttemptReceiveAttempts = 1;
                                                lock (s_queries)
                                                {
                                                    s_queries[myKey] = myQuery;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                await Task.Delay(10);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Socket " + myKey + " faulted!");
                            Console.WriteLine(ex.ToString());
                            mySocket.Close();
                            mySocket.Dispose();
                        }
                    }
                });
                s_socketSockets.Add(thisKey, newSocket);
                s_queries.Add(thisKey, queryObj);
                s_socketTasks.Add(thisKey, socketIOTask);
                s_dataSendQueue.Add(thisKey, new Queue<byte[]>());
                socketIOTask.Start();
                Shell.WriteLine("Socket connection opened as ID " + thisKey + ".");
                return thisKey;
            }
            public static void CloseSocket(ulong socketID)
            {
                Shell.WriteLine("Closing socket with ID " + socketID + ".");
                if (s_socketSockets.ContainsKey(socketID))
                {
                    s_socketSockets[socketID].Close();
                    s_socketSockets.Remove(socketID);
                }
                if (s_queries.ContainsKey(socketID))
                {
                    s_queries.Remove(socketID);
                }
                if (s_dataSendQueue.ContainsKey(socketID))
                {
                    s_dataSendQueue.Remove(socketID);
                }
                if (s_socketTasks.ContainsKey(socketID))
                {
                    s_socketTasks[socketID].Dispose();
                    s_socketTasks.Remove(socketID);
                }
            }
            public static void CloseAllSockets()
            {
                Shell.WriteLine("Running all socket close with " + s_socketSockets.Keys.Count + " remaining to close.");
                foreach (ulong id in s_socketSockets.Keys) { CloseSocket(id); }
            }
        }
    }
}
