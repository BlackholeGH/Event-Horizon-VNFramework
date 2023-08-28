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
    /// <summary>
    /// This class defines an interface for constructing and communicating with a remote Python process over a socket bridge
    /// </summary>
    public static class PythonController
    {
        /// <summary>
        /// Method to test basic socket functionality
        /// </summary>
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
        /// <summary>
        /// String for the local Python executable.
        /// This could be updated to be a dynamic lookup
        /// </summary>
        private static readonly string s_pyExecutable = "C:\\Users\\Blackhole\\AppData\\Local\\Programs\\Python\\Python311\\python.exe";
        /// <summary>
        /// Static method to start the Python process with a given script
        /// </summary>
        /// <param name="pyScript">String path to the script to execute</param>
        /// <returns>The Process object for the started process</returns>
        public static Process StartPythonProcess(string pyScript)
        {
            ProcessStartInfo pyStart = new ProcessStartInfo(); //Create process start info
            pyStart.FileName = s_pyExecutable; //Set the file name
            pyStart.Arguments = "-u " + pyScript; //Set the process arguments
            pyStart.UseShellExecute = false; //Set "shell execute" to false
            pyStart.RedirectStandardOutput = true; //Redirect the standard output so the Python script can print back to its mother process
            Shell.WriteLine("Starting Python process at: " + pyScript);
            Process outProcess = Process.Start(pyStart); //Start the process
            Shell.ActiveProcesses.Add(outProcess); //Register process with the VNF shell
            //An anonymous method is created here to receive output from the Python process
            outProcess.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
            {
                Shell.WriteLine("Python script " + pyScript.Remove(0, pyScript.LastIndexOf('\\') + 1) + ": " + e.Data);
                if(e.Data == "SOCKETS_OPEN_AND_READY")
                {
                    SocketInterface.SocketsOpenedFlag = true; //Set sockets open flag
                }
                if (e.Data == "SYSTEM_SOCKET_ASSIGNED")
                {
                    IterativeMemBoTs.SystemSocketAssigned = true; //Set system socket assigned flag
                }
            };
            //Anonymous method is created to close the socket connection when the Python process closes
            outProcess.Exited += (object sender, EventArgs e) =>
            {
                SocketInterface.CloseAllSockets();
                SocketInterface.SocketsOpenedFlag = false;
                IterativeMemBoTs.SystemSocketAssigned = false;
            };
            outProcess.BeginOutputReadLine(); //Begin reading from process
            return outProcess;
        }
        /// <summary>
        /// Defines the socket bridge interface, primarily for communication with the Python process
        /// </summary>
        public static class SocketInterface
        {
            /// <summary>
            /// WorldEntity that will close the system socket and kill a socket host process when it is deleted or disposed.
            /// </summary>
            [Serializable]
            public class SocketCloserEntity : WorldEntity
            {
                [NonSerialized]
                Process _processToClose = null; //Field for the process to close
                /// <summary>
                /// SocketCloserEntity constructor
                /// </summary>
                /// <param name="name">WorldEntity name</param>
                /// <param name="processToClose">Process to be closed</param>
                public SocketCloserEntity(String name, Process processToClose) : base(name, new Vector2(), null, 0)
                {
                    _processToClose = processToClose;
                }
                Boolean _systemSocketsClosed = false;
                /// <summary>
                /// ManualDispose method override
                /// </summary>
                public override void ManualDispose()
                {
                    if (OpenSockets.Contains(0) && !_systemSocketsClosed) //Close the system socket if it is opened
                    {
                        CloseSocket(0);
                        SocketInterface.SocketsOpenedFlag = false; //Update flags
                        IterativeMemBoTs.SystemSocketAssigned = false;
                        _systemSocketsClosed = true;
                    }
                    if (!(_processToClose is null)) //If the process is not null
                    {
                        Shell.WriteLine("Killing socket process via closer object.");
                        _processToClose.Kill(); //Kill the process
                        _processToClose.Close(); //Close the process
                        if (Shell.ActiveProcesses.Contains(_processToClose)) { Shell.ActiveProcesses.Remove(_processToClose); } //Register closure with the Shell
                        _processToClose = null;
                    }
                    base.ManualDispose();
                }
            }
            private static Boolean s_socketsOpenedFlag = false;
            private static object s_socketLock = new object();
            /// <summary>
            /// Boolean flag recording whether the socket bridge is opened
            /// </summary>
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
            /// <summary>
            /// Data structure representing socket query parameters for communicating with the python process
            /// </summary>
            [Serializable]
            public struct PySocketQuery
            {
                public PySocketQuery() { }
                public byte[] Send = new byte[0]; //Data to send
                public byte[] Receive = new byte[0]; //Data received
                public Boolean LastSend = false; //Whether the last operation was a send operation (awaiting receive)
                public Boolean LastReceive = false; //Whether the last operation was a receive operation (awaiting receive acknowledgement)
                public int AllowedSendAttempts = -1; //Number of allowed send attempts
                public int AttemptReceiveAttempts = 1; //Number of allowed receive attempts
            }
            /// <summary>
            /// Property representing the next ID for socket creation
            /// </summary>
            public static ulong CurrentSocketID
            {
                get;
                private set;
            }
            private static Dictionary<ulong, Socket> s_socketSockets = new Dictionary<ulong, Socket>(); //Dictionary of all Socket entities by their ID
            private static Dictionary<ulong, Task> s_socketTasks = new Dictionary<ulong, Task>(); //Dictionary of all Socket handler Tasks by their ID
            private static Dictionary<ulong, PySocketQuery> s_queries = new Dictionary<ulong, PySocketQuery>(); //Dictionary of all socket query structs by their ID
            private static Dictionary<ulong, Queue<byte[]>> s_dataSendQueue = new Dictionary<ulong, Queue<byte[]>>(); //Dictionary of all queued data sends by their ID
            /// <summary>
            /// List property of currently open socket IDs
            /// </summary>
            public static List<ulong> OpenSockets
            {
                get
                {
                    return new List<ulong>(s_socketSockets.Keys);
                }
            }
            /// <summary>
            /// Method to acknowledge that the data received over a socket has been read
            /// </summary>
            /// <param name="key">Socket ID key for the query list</param>
            public static void AcknowledgeReceive(ulong key)
            {
                lock(s_queries)
                {
                    if(s_queries.ContainsKey(key))
                    {
                        PySocketQuery current = s_queries[key];
                        current.LastReceive = false; //Set last receive to false to prepare for sending
                        s_queries[key] = current;
                    }
                }
            }
            /// <summary>
            /// Static lookup for the query struct associated with a given socket ID key
            /// </summary>
            /// <param name="key">The socket ID key for the query lookup</param>
            /// <returns>The associated PySocketQuery</returns>
            public static PySocketQuery GetQuery(ulong key)
            {
                PySocketQuery output = new PySocketQuery();
                lock (s_queries)
                {
                    if (s_queries.ContainsKey(key)) { output = s_queries[key]; }
                }
                return output;
            }
            /// <summary>
            /// Method to send a data query over a socket
            /// </summary>
            /// <param name="key">Socket ID key as ULong</param>
            /// <param name="data">Data to send as a byte array</param>
            /// <param name="allowEnqueue">Whether the data can be queued as a Boolean</param>
            public static void SendQuery(ulong key, byte[] data, Boolean allowEnqueue)
            {
                SendQuery(key, data, allowEnqueue, -1, 1);
            }
            /// <summary>
            /// Method to send a data query over a socket
            /// </summary>
            /// <param name="key">Socket ID key as ULong</param>
            /// <param name="data">Data to send as a byte array</param>
            /// <param name="allowEnqueue">Whether the data can be queued as a Boolean</param>
            /// <param name="allowedSendAttempts">Number of allowed send attempts</param>
            public static void SendQuery(ulong key, byte[] data, Boolean allowEnqueue, int allowedSendAttempts)
            {
                SendQuery(key, data, allowEnqueue, allowedSendAttempts, 1);
            }
            /// <summary>
            /// Method to send a data query over a socket
            /// </summary>
            /// <param name="key">Socket ID key as ULong</param>
            /// <param name="data">Data to send as a byte array</param>
            /// <param name="allowEnqueue">Whether the data can be queued as a Boolean</param>
            /// <param name="allowedSendAttempts">Number of allowed send attempts</param>
            /// <param name="attemptReceiveAttempts">Number of allowed receive attempts</param>
            public static void SendQuery(ulong key, byte[] data, Boolean allowEnqueue, int allowedSendAttempts, int attemptReceiveAttempts)
            {
                if (allowEnqueue) { Shunt(); } //If data queueing is allowed, update the queue stack
                if (data.Length > 1024) { return; } //Data is limited to one kibibyte
                else
                {
                    if (s_dataSendQueue.ContainsKey(key) && s_dataSendQueue[key].Count > 0 && allowEnqueue) //Enqueue data if this is allowed and the queue is full
                    {
                        s_dataSendQueue[key].Enqueue(data);
                    }
                    else
                    {
                        PySocketQuery query;
                        lock (s_queries)
                        {
                            if(!s_queries.ContainsKey(key)) { return; }
                            query = s_queries[key]; //Retrieve query struct
                        }
                        if (query.LastSend) //If the socket has still not received a reply to its last send
                        {
                            if (allowEnqueue) { s_dataSendQueue[key].Enqueue(data); } //Enqueue this data if that is enabled
                        }
                        else
                        {
                            query.LastSend = true; //Set query struct to send mode
                            query.LastReceive = false;
                            query.Send = data; //Set data to send
                            query.AllowedSendAttempts = allowedSendAttempts; //Set send attempts
                            query.AttemptReceiveAttempts = attemptReceiveAttempts; //Set receive attempts
                            lock (s_queries)
                            {
                                s_queries[key] = query; //Deploy updated query struct
                            }
                        }
                    }
                }
            }
            /// <summary>
            /// Static method to update the data queue and shunt queued data where possible
            /// </summary>
            public static void Shunt()
            {
                PySocketQuery query;
                foreach (ulong key in s_dataSendQueue.Keys) //For each socket ID in the queue
                {
                    if (s_dataSendQueue[key].Count > 0) //Check if the queue has data in it
                    {
                        lock (s_queries)
                        {
                            query = s_queries[key];
                            if (query.LastSend) { continue; } //If the socket is not waiting on a reply
                            else
                            {
                                query.Send = s_dataSendQueue[key].Dequeue(); //Dequeue the data
                                query.LastReceive = false; //Set the parameters
                                query.LastSend = true;
                                s_queries[key] = query; //Send the data
                            }
                        }
                    }
                }
            }
            /// <summary>
            /// Request that a new socket with its own task handler be opened
            /// </summary>
            /// <returns>Returns the ID of the newly opened socket</returns>
            public static ulong AddNewSocketAsTask()
            {
                IPEndPoint ipEndPoint = new IPEndPoint(new IPAddress(new byte[] { 127, 0, 0, 1 }), 65432); //Set socket IP endpoint as the loopback address on a given port
                ulong thisKey = CurrentSocketID; //Get next socket ID
                CurrentSocketID++; //Increment socket ID counter
                Socket newSocket = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp); //Create the socket entity
                PySocketQuery queryObj = new PySocketQuery(); //Create the associated query struct
                queryObj.LastSend = false;//Initialize query struct
                queryObj.LastReceive = false;
                //An asyncronous task is created in order to handle communication over this socket
                Task socketIOTask = new Task(async () =>
                {
                    ulong myKey = thisKey; //Set the key
                    Socket mySocket = newSocket;
                    mySocket.SendTimeout = -1; //Set the default sending timeout
                    mySocket.ReceiveTimeout = 1000; //Set the default receive timeout
                    PySocketQuery myQuery;
                    byte[] send;
                    byte[] buffer = new byte[1024];
                    try
                    {
                        await mySocket.ConnectAsync(ipEndPoint); //Attempt to connect
                    }
                    catch(Exception e)
                    {
                        Shell.WriteLine("Socket connection " + myKey + " encountered an error while trying to connect. The socket task will now close.");
                    }
                    while (mySocket.Connected) //If the socket connection went through
                    {
                        try
                        {
                            lock (s_queries)
                            {
                                myQuery = s_queries[myKey]; //Retrieve the current query in a thread safe way
                            }
                            if (myQuery.LastSend) //If there is a send operation waiting
                            {
                                if (!mySocket.Connected) { break; }
                                bool expectReceive = false;
                                if (myQuery.AllowedSendAttempts == -1)
                                {
                                    send = myQuery.Send;
                                    mySocket.Send(send, SocketFlags.None); //Send data once
                                    expectReceive = true;
                                }
                                else if (myQuery.AllowedSendAttempts > 0)
                                {
                                    send = myQuery.Send;
                                    mySocket.Send(send, SocketFlags.None); //Send data and decrement the send attempt counter
                                    myQuery.AllowedSendAttempts--;
                                    lock (s_queries)
                                    {
                                        s_queries[myKey] = myQuery;
                                    }
                                    expectReceive = true;
                                }
                                else if (myQuery.AllowedSendAttempts == 0) //If the send attempt counter is zero, reset the query and ignore it
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
                                if (expectReceive) //If a response is expected
                                {
                                    buffer = new byte[1024];
                                    while (myQuery.AttemptReceiveAttempts > 0 && !myQuery.LastReceive) //For each receive attempt
                                    {
                                        try
                                        {
                                            int receiveCode = mySocket.Receive(buffer, SocketFlags.None); //Try to receive the data
                                            myQuery.LastSend = false;
                                            myQuery.LastReceive = true; //Update query flags to reflect a new receive
                                            myQuery.Send = new byte[1024];
                                            myQuery.Receive = buffer; //Store received data
                                            myQuery.AllowedSendAttempts = -1;
                                            myQuery.AttemptReceiveAttempts = 1;
                                            lock (s_queries)
                                            {
                                                s_queries[myKey] = myQuery; //Deploy updated query struct
                                            }
                                        }
                                        catch (SocketException e) //If the receive fails
                                        {
                                            Console.WriteLine(myKey + " failed receive: " + (myQuery.AttemptReceiveAttempts - 1) + " left.");
                                            myQuery.AttemptReceiveAttempts -= 1; //Decrement the receive attempt counter
                                            if(myQuery.AttemptReceiveAttempts <= 0) //If the counter hits zero, reset the query parameters and do not try again
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
                                await Task.Delay(10); //The task loop is slightly delayed while it waits for activity
                            }
                        }
                        catch (Exception ex) //Exception for if something fails in the socket task
                        {
                            Console.WriteLine("Socket " + myKey + " faulted!");
                            Console.WriteLine(ex.ToString());
                            mySocket.Close();
                            mySocket.Dispose();
                        }
                    }
                });
                s_socketSockets.Add(thisKey, newSocket); //Add socket object to dict
                s_queries.Add(thisKey, queryObj); //Add query struct to dict
                s_socketTasks.Add(thisKey, socketIOTask); //Add task object to dict
                s_dataSendQueue.Add(thisKey, new Queue<byte[]>()); //Add socket data queue for this ID to dict
                socketIOTask.Start(); //Start the socket handler task
                Shell.WriteLine("Socket connection opened as ID " + thisKey + ".");
                return thisKey;
            }
            /// <summary>
            /// Method to close an open socket
            /// </summary>
            /// <param name="socketID">ID of the socket to close</param>
            public static void CloseSocket(ulong socketID)
            {
                Shell.WriteLine("Closing socket with ID " + socketID + ".");
                if (s_socketSockets.ContainsKey(socketID)) //Remove socket object
                {
                    s_socketSockets[socketID].Close();
                    s_socketSockets.Remove(socketID);
                }
                if (s_queries.ContainsKey(socketID)) //Remove query struct
                {
                    s_queries.Remove(socketID);
                }
                if (s_dataSendQueue.ContainsKey(socketID)) //Remove data queue
                {
                    s_dataSendQueue.Remove(socketID);
                }
                if (s_socketTasks.ContainsKey(socketID)) //Remove socket handler task
                {
                    s_socketTasks[socketID].Dispose();
                    s_socketTasks.Remove(socketID);
                }
            }
            /// <summary>
            /// Method to close every open socket
            /// </summary>
            public static void CloseAllSockets()
            {
                Shell.WriteLine("Running all socket close with " + s_socketSockets.Keys.Count + " remaining to close.");
                foreach (ulong id in s_socketSockets.Keys) { CloseSocket(id); }
            }
        }
    }
}
