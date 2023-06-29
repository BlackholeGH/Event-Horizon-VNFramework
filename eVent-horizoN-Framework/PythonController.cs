using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace VNFramework
{
    public static class PythonController
    {
        public static void Test()
        {
            PythonController.StartPythonProcess("C:\\Users\\Blackhole\\PycharmProjects\\Brains\\venv\\socketmanager.py");
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
        private static readonly string s_pyExecutable = "C:\\Users\\Blackhole\\PycharmProjects\\Brains\\venv\\Scripts\\python.exe";
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
            outProcess.BeginOutputReadLine();
            return outProcess;
        }
        public static class SocketInterface
        {
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
            public struct PySocketQuery
            {
                public byte[] Send;
                public byte[] Receive;
                public Boolean LastSend;
                public Boolean LastReceive;
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
                PySocketQuery output;
                lock (s_queries)
                {
                    output = s_queries[key];
                }
                return output;
            }
            public static void SendQuery(ulong key, byte[] data, Boolean allowEnqueue)
            {
                if (allowEnqueue) { Shunt(); }
                if (data.Length > 1024) { return; }
                else
                {
                    if (s_dataSendQueue[key].Count > 0 && allowEnqueue)
                    {
                        s_dataSendQueue[key].Enqueue(data);
                    }
                    else
                    {
                        PySocketQuery query;
                        lock (s_queries)
                        {
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
                    PySocketQuery myQuery;
                    byte[] send;
                    byte[] buffer = new byte[1024];
                    await mySocket.ConnectAsync(ipEndPoint);
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
                                send = myQuery.Send;
                                await mySocket.SendAsync(send, SocketFlags.None);
                                int receiveCode = await mySocket.ReceiveAsync(buffer, SocketFlags.None);
                                myQuery.LastSend = false;
                                myQuery.LastReceive = true;
                                myQuery.Send = new byte[1024];
                                myQuery.Receive = buffer;
                                lock (s_queries)
                                {
                                    s_queries[myKey] = myQuery;
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
                Shell.WriteLine("Socket connection successfully opened as ID " + thisKey + ".");
                return thisKey;
            }
            public static void CloseSocket(ulong socketID)
            {
                s_socketSockets[socketID].Close();
                s_socketSockets.Remove(socketID);
                s_socketTasks[socketID].Dispose();
                s_socketTasks.Remove(socketID);
                s_queries.Remove(socketID);
                s_dataSendQueue.Remove(socketID);
            }
            public static void CloseAllSockets()
            {
                foreach (ulong id in s_socketSockets.Keys) { CloseSocket(id); }
            }
        }
    }
}
