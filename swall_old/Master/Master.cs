using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using NewMaster.Packets;
using System.Timers;
using System.IO;
using System.Diagnostics;
using System.Text;

namespace NewMaster
{
    class Master
    {
        private int portA = 69;
        private int portC = 71;
        private int portD = 72;

        private EndPoint endPointA;
        private EndPoint endPointC;
        private EndPoint endPointD;

        private Socket slaveSocketD;
        private IPAddress[] slavePool;
        private int slavesConnected;
        private AsyncCallback callback;

        private InteractionState[] interactionStates;
        private List<AwaitingAck> awaitingAckList;
        private Dictionary<int, string[]> mediaNameDict;
        private Timer idleTimer;
        private Timer UICTimer;
        private const double idleTimerInterval = 10000;
        private int nextMediaIndex = 0;
        private object lockObj = new object();
        private Process firstProc;
        private TcpListener multiScreenListener;
        private Timer multiScreenTimer;
        private byte[] reply;
        private string[] info;
        private bool multiScreenMode;
        private TcpListener updateListener;
        private TcpListener multiScreenUpdateListener;
        private TcpListener pausePlayListener;
        private byte[] multiScreenAck;
        private int approvedSlaves;
        private bool slave1ACKReceived;
        private bool slave2ACKReceived;
        private bool slave3ACKReceived;
        private bool slave4ACKReceived;
        private bool slave1Connected;
        private bool slave2Connected;
        private bool slave3Connected;
        private bool slave4Connected;
        private bool first = true;
        private TcpListener connectionListener;
        private TcpListener slaveACKListener;
        private TcpListener idleListener;
        private ICModelController icmc;
        private bool isUpdating = false;
        private double multiStartTime = 0;
        private TimeSpan pauseTime;
        private int initiatingSlave;
        private bool multiScreenActive = false;
        private List<string> multiGroup;
        public Master()
        {
            multiGroup = new List<string>();
            multiScreenAck = new byte[] { 0, 0, 0, 0 };
            updateListener = new TcpListener(IPAddress.Any, 4079);
            Console.WriteLine("Listening...");
            updateListener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            updateListener.Start();
            updateListener.BeginAcceptSocket(new AsyncCallback(OnReceiveMultiScreenClose), updateListener);
            multiScreenUpdateListener = new TcpListener(IPAddress.Any, 3004);
            Console.WriteLine("Listening...");
            multiScreenUpdateListener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            multiScreenUpdateListener.Start();
            multiScreenUpdateListener.BeginAcceptSocket(new AsyncCallback(OnReceiveMultiScreenUpdate), multiScreenUpdateListener);
            pausePlayListener = new TcpListener(IPAddress.Any, 3005);
            Console.WriteLine("Listening...");
            pausePlayListener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            pausePlayListener.Start();
            pausePlayListener.BeginAcceptSocket(new AsyncCallback(PausePlayUpdate), pausePlayListener);
            connectionListener = new TcpListener(IPAddress.Any, portD);
            Console.WriteLine("Listening...");
            connectionListener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            connectionListener.Start();
            connectionListener.BeginAcceptSocket(new AsyncCallback(OnSlaveConnect), connectionListener);
            idleListener = new TcpListener(IPAddress.Any, portA);
            Console.WriteLine("Listening...");
            idleListener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            idleListener.Start();
            idleListener.BeginAcceptSocket(new AsyncCallback(OnDataReceived), idleListener);
            slaveACKListener = new TcpListener(IPAddress.Any, 4086);
            Console.WriteLine("Listening...");
            slaveACKListener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            slaveACKListener.Start();
            slaveACKListener.BeginAcceptTcpClient(new AsyncCallback(DoAcceptTcpClientCallback), slaveACKListener);
            endPointA = new IPEndPoint(IPAddress.Any, portA);
            endPointC = new IPEndPoint(IPAddress.Any, portC);
            endPointD = new IPEndPoint(IPAddress.Any, portD);

            slaveSocketD = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            //slaveSocketA.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            slavePool = new IPAddress[App.monitorCount];
            slavesConnected = 0;

            interactionStates = new InteractionState[slavePool.Length];

            for (var i = 0; i < slavePool.Length; i++)
            {
                interactionStates[i] = new InteractionState();
            }

            awaitingAckList = new List<AwaitingAck>();
            mediaNameDict = new Dictionary<int, string[]>();

            #region Load filenames
            string pathToIdleResources = Directory.GetCurrentDirectory() + @"\Resources\IdleMedia";

            var paths = Directory.GetFiles(pathToIdleResources + @"\Size1");
            mediaNameDict[1] = new string[paths.Length];
            for (int i = 0; i < paths.Length; i++)
            {
                mediaNameDict[1][i] = Path.GetFileName(paths[i]);
            }

            paths = Directory.GetFiles(pathToIdleResources + @"\Size2");
            mediaNameDict[2] = new string[paths.Length];
            for (int i = 0; i < paths.Length; i++)
            {
                mediaNameDict[2][i] = Path.GetFileName(paths[i]);
            }

            paths = Directory.GetFiles(pathToIdleResources + @"\Size3");
            mediaNameDict[3] = new string[paths.Length];
            for (int i = 0; i < paths.Length; i++)
            {
                mediaNameDict[3][i] = Path.GetFileName(paths[i]);
            }

            paths = Directory.GetFiles(pathToIdleResources + @"\Size4");
            mediaNameDict[4] = new string[paths.Length];
            for (int i = 0; i < paths.Length; i++)
            {
                mediaNameDict[4][i] = Path.GetFileName(paths[i]);
            }
            #endregion

            //If port A is currently open close it and re-open to use
            if ((from p in System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().GetActiveUdpListeners() where p.Port == portA select p).Count() == 1)
            {
                Console.WriteLine("Port Taken");
            }
        }


        public void Run()
        {
            do
            {
                //wait for slaves to register
                System.Threading.Thread.Sleep(1000);
            } while (slavesConnected != slavePool.Length);
            Console.WriteLine("Total Slaves Connected");
            first = false;
            // Start Idle interaction for all slaves at the startup.
            for (var i = 0; i < slavePool.Length; i++)
            {
                ReConnect(i, true);
                interactionStates[i].State = InteractionState.STATE.Idle;
            }
            multiScreenListener = new TcpListener(IPAddress.Any, 4078);
            Console.WriteLine("Listening...");
            multiScreenListener.Start();
            multiScreenListener.BeginAcceptTcpClient(new AsyncCallback(MultiScreenRequested), multiScreenListener);
            // Wait 3s before starting timer.
            idleTimer = new Timer();
            idleTimer.Elapsed += UpdateIdleMedia;
            idleTimer.AutoReset = false;
            idleTimer.Start();

            //Notify Slaves to start Bird View
            string newView = "BIRD";
            icmc = new ICModelController();
            //Timer to update the view on slaves
            UICTimer = new Timer();
            UICTimer.AutoReset = true;
            UICTimer.Interval = 100; // 10 updates/second
            UICTimer.Elapsed += ((sender, args) => icmc.UpdateSlaves(slavePool));

            // Start with default Interaction Coaxer view
            foreach (var slave in slavePool)
            {
                icmc.StartView(newView, slave);
            }

            UICTimer.Start();

            // After Initial InteractionCoaxer is created, logic to change it to something different should be incorporated here
            /*do
            {
                System.Threading.Thread.Sleep(1000 * 60 * 15); // Change IC every 15 minutes.
                // Change IC view to something different.
                // newView = ...
                foreach (var slave in slavePool)
                {
                    icmc.StartView(newView, slave);
                }

            } while (true);*/
        }

        private void ReConnect(int slaveIndex, bool start)
        {
            try
            {
                CDI_Packet CDI = new CDI_Packet(CDI_Packet.CDI_Type.Idle, CDI_Packet.CDI_Mode.Create);
                TcpClient slaveUpdateSocket = new TcpClient(slavePool[slaveIndex].ToString(), portA);
                slaveUpdateSocket.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                NetworkStream slaveUpdate = slaveUpdateSocket.GetStream();
                slaveUpdate.Write(CDI.encode(), 0, CDI.encode().Length);
                slaveUpdate.Dispose();
                slaveUpdateSocket.Close();
                //SendUntilACKReceive(CDI, slaveSocketA, slaveIndex, new IPEndPoint(slavePool[slaveIndex], portA));
                interactionStates[slaveIndex].State = InteractionState.STATE.Idle;
                if (!start)
                {
                    string newView = "BIRD";
                    icmc.StartView(newView, slavePool[slaveIndex]);
                    UpdateIdleMedia(null, null);
                }
            }
            catch (Exception e) {
                Console.Out.WriteLine(e);
                string text = "Class Master, line 239";
                System.IO.File.WriteAllText(@"C:\Users\Public\ErrorLog\errors.txt", text);
            }//TODO ADD EXCEPTION HANDLING
        }
        private void MultiScreenRequested(IAsyncResult ar)
        {
            try
            {
                multiScreenListener.BeginAcceptTcpClient(new AsyncCallback(MultiScreenRequested), multiScreenListener);
                Console.Out.WriteLine("Update Received");
                byte[] buffer = new byte[700];
                // Get the listener that handles the client request.
                TcpListener listener = (TcpListener)ar.AsyncState;

                // End the operation and display the received data on 
                // the console.
                TcpClient client = listener.EndAcceptTcpClient(ar);
                NetworkStream replyStream = client.GetStream();

                //---read incoming stream---
                int bytesRead = replyStream.Read(buffer, 0, buffer.Length);
                var packet = System.Text.Encoding.UTF8.GetString(buffer);
                //Console.Out.WriteLine("got FileName: " + packet);
                info = packet.Split('\t');
                initiatingSlave = Int32.Parse(info[1]);
                if (info[0].Equals("MultiScreen"))
                {
                    Console.Out.WriteLine("Multi-Screen Requested");
                    for (var i = 0; i < slavePool.Length; i++)
                    {
                        if (interactionStates[i].State == InteractionState.STATE.Idle)
                        {
                            CDI_Packet CDI = new CDI_Packet(CDI_Packet.CDI_Type.Active, CDI_Packet.CDI_Mode.Create);
                            TcpClient slaveUpdateSocket = new TcpClient(slavePool[i].ToString(), portA);
                            slaveUpdateSocket.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                            NetworkStream slaveUpdate = slaveUpdateSocket.GetStream();
                            slaveUpdate.Write(CDI.encode(), 0, CDI.encode().Length);
                            slaveUpdate.Dispose();
                            slaveUpdateSocket.Close();
                            interactionStates[i].State = InteractionState.STATE.Active;
                        }
                        //reply[i + 1] = 1;
                    }
                    client.Close();
                    multiScreenTimer = new Timer();
                    multiScreenTimer.Elapsed += SendMultiScreenRequest;
                    multiScreenTimer.Interval = 8000;
                    multiScreenTimer.AutoReset = false;
                    multiScreenTimer.Enabled = true;
                    
                }
            }
            catch (Exception) { 
                string text = "Class Master, line 292";
                System.IO.File.WriteAllText(@"C:\Users\Public\ErrorLog\errors.txt", text);
            }//TODO ADD EXCEPTION HANDLING
        }
        private void SendMultiScreenRequest(object sender, ElapsedEventArgs e)
        {
            try
            {
                multiScreenTimer.Stop();
                multiScreenTimer.Enabled = false;
                multiScreenTimer.Dispose();
                multiScreenTimer = null;
                slave1Connected = false;
                slave2Connected = false;
                slave3Connected = false;
                slave4Connected = false;
                multiScreenAck = new byte[] { 0, 0, 0, 0 };
                slave1ACKReceived = false;
                slave2ACKReceived = false;
                slave3ACKReceived = false;
                slave4ACKReceived = false;
                approvedSlaves = 0;
                string request = "MultiScreen\t";
                for (var i = 0; i < slavePool.Length; i++)
                {
                    if (slavePool[i].ToString().Equals(App.slave1IP))
                    {
                        slave1Connected = true;
                    }
                    else if (slavePool[i].ToString().Equals(App.slave2IP))
                    {
                        slave2Connected = true;
                    }
                    else if (slavePool[i].ToString().Equals(App.slave3IP))
                    {
                        slave3Connected = true;
                    }
                    else if (slavePool[i].ToString().Equals(App.slave4IP))
                    {
                        slave4Connected = true;
                    }
                    TcpClient slaveUpdateSocket = new TcpClient(slavePool[i].ToString(), 4086);
                    slaveUpdateSocket.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                    NetworkStream slaveUpdate = slaveUpdateSocket.GetStream();
                    slaveUpdate.Write(Encoding.ASCII.GetBytes(request), 0, Encoding.ASCII.GetBytes(request).Length);
                    Console.Out.WriteLine("Sent Multi-Screen Request To Slave " + slavePool[i].ToString());
                    slaveUpdate.Dispose();
                    slaveUpdateSocket.Close();
                }
            }
            catch (Exception) { 
                string text = "Class Master, line 343";
                System.IO.File.WriteAllText(@"C:\Users\Public\ErrorLog\errors.txt", text);
            }//TODO ADD EXCEPTION HANDLING
        }
        public void DoAcceptTcpClientCallback(IAsyncResult ar)
        {
            slaveACKListener.BeginAcceptTcpClient(new AsyncCallback(DoAcceptTcpClientCallback), slaveACKListener);
            try
            {
                byte[] slaveData = new byte[2];
                // Get the listener that handles the client request.
                TcpListener listener = (TcpListener)ar.AsyncState;

                // End the operation and display the received data on 
                // the console.
                TcpClient client = listener.EndAcceptTcpClient(ar);
                NetworkStream replyStream = client.GetStream();

                //---read incoming stream---
                int bytesRead = replyStream.Read(slaveData, 0, 2);
                if (slaveData[0] == 1)
                {
                    if (slaveData[1] == 1)
                    {
                        multiScreenAck[slaveData[1] - 1] = slaveData[0];
                        approvedSlaves++;
                        slave1ACKReceived = true;
                    }
                    else if (slaveData[1] == 2)
                    {
                        multiScreenAck[slaveData[1] - 1] = slaveData[0];
                        approvedSlaves++;
                        slave2ACKReceived = true;
                    }
                    else if (slaveData[1] == 3)
                    {
                        multiScreenAck[slaveData[1] - 1] = slaveData[0];
                        approvedSlaves++;
                        slave3ACKReceived = true;
                    }
                    else if (slaveData[1] == 4)
                    {
                        multiScreenAck[slaveData[1] - 1] = slaveData[0];
                        approvedSlaves++;
                        slave4ACKReceived = true;
                    }

                }
                else if (slaveData[0] == 0)
                {
                    if (slaveData[1] == 1)
                    {
                        multiScreenAck[slaveData[1] - 1] = slaveData[0];
                        slave1ACKReceived = true;
                    }
                    else if (slaveData[1] == 2)
                    {
                        multiScreenAck[slaveData[1] - 1] = slaveData[0];
                        slave2ACKReceived = true;
                    }
                    else if (slaveData[1] == 3)
                    {
                        multiScreenAck[slaveData[1] - 1] = slaveData[0];
                        slave3ACKReceived = true;
                    }
                    else if (slaveData[1] == 4)
                    {
                        multiScreenAck[slaveData[1] - 1] = slaveData[0];
                        slave4ACKReceived = true;
                    }

                }
                replyStream.Dispose();
                client.Close();
                if (slave1Connected == slave1ACKReceived && slave2Connected == slave2ACKReceived && slave3Connected == slave3ACKReceived && slave4Connected == slave4ACKReceived)
                {
                    SendMultiScreenReply();
                }
            }
            catch (Exception) { 
                string text = "Class Master, line 423";
                System.IO.File.WriteAllText(@"C:\Users\Public\ErrorLog\errors.txt", text);
            }//TODO ADD EXCEPTION HANDLING
        }
        private void SendMultiScreenReply()
        {
            List<string> newMultiGroup = new List<string>();
            try
            {
                for (int j = initiatingSlave - 2; j >= 0; j--)
                {
                    if (multiScreenAck[j] == 1)
                    {
                        newMultiGroup.Add(slavePool[j].ToString());
                        multiScreenAck[j] = 0;
                    }
                    else
                    {
                        break;
                    }
                }
                for (int k = initiatingSlave; k < multiScreenAck.Length; k++)
                {
                    if (multiScreenAck[k] == 1)
                    {
                        newMultiGroup.Add(slavePool[k].ToString());
                        multiScreenAck[k] = 0;

                    }
                    else
                    {
                        break;
                    }
                }
                Console.Out.WriteLine("Initiating Slave: " + initiatingSlave);
                newMultiGroup.Add(slavePool[initiatingSlave - 1].ToString());
                var unsortedIps = newMultiGroup.ToArray();
                newMultiGroup = unsortedIps.Select(Version.Parse).OrderBy(arg => arg).Select(arg => arg.ToString()).ToList();
                Console.Out.WriteLine("Multi Group Count: " + newMultiGroup.Count);
                var currentTime = DateTime.Now;
                multiStartTime = DateTime.Now.AddSeconds(10).ToLocalTime().TimeOfDay.TotalMilliseconds;
                double mid = multiGroup.Count / 2;
                for (var i = 0; i < newMultiGroup.Count; i++)
                {
                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    if (multiScreenAck[initiatingSlave - 1] == 0 || newMultiGroup.Count == 1 || multiScreenActive)
                    {
                        Console.Out.WriteLine(multiScreenAck[initiatingSlave - 1]);
                        Console.Out.WriteLine(newMultiGroup.Count);
                        Console.Out.WriteLine(multiScreenActive);
                        sb.Append("Disapproved").Append("\t");
                    }
                    else
                    {
                        if (i == Math.Ceiling(mid))
                        {
                            sb.Append(info[0]).Append("\t").Append(multiStartTime.ToString()).Append("\t").Append(info[2]).Append("\t").Append(info[3]).Append("\t").Append(info[4]).Append("\t").Append(newMultiGroup.Count.ToString()).Append("\t").Append((i + 1)).Append("\t").Append("hassound").Append("\t");
                        }
                        else
                        {
                            sb.Append(info[0]).Append("\t").Append(multiStartTime.ToString()).Append("\t").Append(info[2]).Append("\t").Append(info[3]).Append("\t").Append(info[4]).Append("\t").Append(newMultiGroup.Count.ToString()).Append("\t").Append((i + 1)).Append("\t").Append("no").Append("\t");
                        }
                        multiGroup = newMultiGroup;
                    }
                    Console.Out.WriteLine(sb.ToString());
                    TcpClient slaveUpdateSocket = new TcpClient(newMultiGroup[i].ToString(), 4080);
                    slaveUpdateSocket.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                    NetworkStream slaveUpdate = slaveUpdateSocket.GetStream();
                    slaveUpdate.Write(Encoding.ASCII.GetBytes(sb.ToString()), 0, Encoding.ASCII.GetBytes(sb.ToString()).Length);
                    Console.Out.WriteLine("Sent Multi-Screen Reply To Slave " + slavePool[i].ToString() + "Pos: " + (i + 1));
                    slaveUpdate.Dispose();
                    slaveUpdateSocket.Close();
                    
                }
                multiScreenAck[initiatingSlave - 1] = 0;
                multiScreenActive = true;
                for (var i = 0; i < multiScreenAck.Length; i++)
                {
                    if (multiScreenAck[i] == 1)
                    {
                        System.Text.StringBuilder sb = new System.Text.StringBuilder();
                        sb.Append("Disapproved").Append("\t");
                        multiScreenAck[i] = 0;
                        TcpClient slaveUpdateSocket = new TcpClient(slavePool[i].ToString(), 4080);
                        slaveUpdateSocket.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                        NetworkStream slaveUpdate = slaveUpdateSocket.GetStream();
                        slaveUpdate.Write(Encoding.ASCII.GetBytes(sb.ToString()), 0, Encoding.ASCII.GetBytes(sb.ToString()).Length);
                        Console.Out.WriteLine("Sent Multi-Screen Reply To Slave " + slavePool[i].ToString() + "Pos: " + (i + 1));
                        slaveUpdate.Dispose();
                        slaveUpdateSocket.Close();
                    }
                }
            }
            catch (Exception) { 
                string text = "Class Master, line 517";
                System.IO.File.WriteAllText(@"C:\Users\Public\ErrorLog\errors.txt", text);
            }//TODO ADD EXCEPTION HANDLING
        }
        private void OnReceiveMultiScreenClose(IAsyncResult asyn)
        {
            try
            {
                updateListener.BeginAcceptSocket(new AsyncCallback(OnReceiveMultiScreenClose), updateListener);
                Console.Out.WriteLine("Multi-Screen Closing Message Received");
                byte[] statusData = new byte[1];
                // Get the listener that handles the client request.
                TcpListener listener = (TcpListener)asyn.AsyncState;

                // End the operation and display the received data on 
                // the console.
                TcpClient client = listener.EndAcceptTcpClient(asyn);
                NetworkStream replyStream = client.GetStream();

                //---read incoming stream---
                int bytesRead = replyStream.Read(statusData, 0, statusData.Length);
                replyStream.Dispose();
                client.Close();
                if (statusData[0] == 0)
                {

                    for (var i = 0; i < slavePool.Length; i++)
                    {
                        try
                        {
                            TcpClient slaveUpdateSocket = new TcpClient(slavePool[i].ToString(), 4079);
                            slaveUpdateSocket.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                            NetworkStream slaveUpdate = slaveUpdateSocket.GetStream();
                            slaveUpdate.Write(statusData, 0, statusData.Length);
                            slaveUpdate.Dispose();
                            slaveUpdateSocket.Close();
                        }
                        catch (Exception e) { }//TODO ADD EXCEPTION HANDLING
                    }
                    multiScreenActive = false;
                }
            }
            catch (Exception) { 
                string text = "Class Master, line 560";
                System.IO.File.WriteAllText(@"C:\Users\Public\ErrorLog\errors.txt", text);
            }//TODO ADD EXCEPTION HANDLING
        }

        private void OnReceiveMultiScreenUpdate(IAsyncResult asyn)
        {
            try
            {
                multiScreenUpdateListener.BeginAcceptSocket(new AsyncCallback(OnReceiveMultiScreenUpdate), multiScreenUpdateListener);
                Console.Out.WriteLine("Multi-Screen Player Update");
                byte[] buffer = new byte[500];
                // Get the listener that handles the client request.
                TcpListener listener = (TcpListener)asyn.AsyncState;

                // End the operation and display the received data on 
                // the console.
                TcpClient client = listener.EndAcceptTcpClient(asyn);
                NetworkStream replyStream = client.GetStream();

                //---read incoming stream---
                int bytesRead = replyStream.Read(buffer, 0, buffer.Length);
                Console.Out.WriteLine(bytesRead);
                byte[] statusData = new byte[bytesRead];
                Array.Copy(buffer, 0, statusData, 0, bytesRead);
                replyStream.Dispose();
                client.Close();
                double newTime = BitConverter.ToDouble(statusData, 0);
                Console.Out.WriteLine("PUpdate: " + newTime);
                double newOffset = newTime - (DateTime.Now.ToLocalTime().TimeOfDay.TotalMilliseconds - multiStartTime);
                multiStartTime = multiStartTime - newOffset;
                Console.Out.WriteLine("NewOffset: " + newOffset);
                byte[] data = BitConverter.GetBytes(newOffset);
                pauseTime = DateTime.Now.ToLocalTime().TimeOfDay;
                for (var i = 0; i < multiGroup.Count; i++)
                {
                    try
                    {
                        TcpClient slaveUpdateSocket = new TcpClient(multiGroup[i].ToString(), 3004);
                        slaveUpdateSocket.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                        NetworkStream slaveUpdate = slaveUpdateSocket.GetStream();
                        slaveUpdate.Write(data, 0, data.Length);
                        slaveUpdate.Dispose();
                        slaveUpdateSocket.Close();
                    }
                    catch (Exception e) { 
                        string text = "Class Master, line 606";
                        System.IO.File.WriteAllText(@"C:\Users\Public\ErrorLog\errors.txt", text);
                    }//TODO ADD EXCEPTION HANDLING
                }
            }
            catch (Exception) { 
                string text = "Class Master, line 612";
                System.IO.File.WriteAllText(@"C:\Users\Public\ErrorLog\errors.txt", text);
            }//TODO ADD EXCEPTION HANDLING

        }

        private void PausePlayUpdate(IAsyncResult asyn)
        {
            pausePlayListener.BeginAcceptSocket(new AsyncCallback(PausePlayUpdate), pausePlayListener);
            try
            {
                Console.Out.WriteLine("Multi-Screen Player Update");
                byte[] buffer = new byte[1];
                // Get the listener that handles the client request.
                TcpListener listener = (TcpListener)asyn.AsyncState;

                // End the operation and display the received data on 
                // the console.
                TcpClient client = listener.EndAcceptTcpClient(asyn);
                NetworkStream replyStream = client.GetStream();

                //---read incoming stream---
                int bytesRead = replyStream.Read(buffer, 0, buffer.Length);
                if (buffer[0] == 0)
                {
                    pauseTime = DateTime.Now.ToLocalTime().TimeOfDay;
                    for (var i = 0; i < multiGroup.Count; i++)
                    {
                        try
                        {
                            TcpClient slaveUpdateSocket = new TcpClient(multiGroup[i].ToString(), 3005);
                            slaveUpdateSocket.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                            NetworkStream slaveUpdate = slaveUpdateSocket.GetStream();
                            slaveUpdate.Write(buffer, 0, buffer.Length);
                            slaveUpdate.Dispose();
                            slaveUpdateSocket.Close();
                        }
                        catch (Exception e) { 
                            string text = "Class Master, line 650";
                            System.IO.File.WriteAllText(@"C:\Users\Public\ErrorLog\errors.txt", text);
                        }//TODO ADD EXCEPTION HANDLING
                    }

                }
                else if (buffer[0] == 1)
                {
                    double pause = (DateTime.Now.ToLocalTime().TimeOfDay.Subtract(pauseTime)).TotalMilliseconds;
                    multiStartTime = multiStartTime + pause;
                    Console.Out.WriteLine("PauseTime: " + pause);
                    byte[] reply = BitConverter.GetBytes(pause);
                    for (var i = 0; i < multiGroup.Count; i++)
                    {
                        try
                        {
                            TcpClient slaveUpdateSocket = new TcpClient(multiGroup[i].ToString(), 3005);
                            slaveUpdateSocket.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                            NetworkStream slaveUpdate = slaveUpdateSocket.GetStream();
                            slaveUpdate.Write(reply, 0, reply.Length);
                            slaveUpdate.Dispose();
                            slaveUpdateSocket.Close();
                        }
                        catch (Exception e) { 
                            string text = "Class Master, line 674";
                            System.IO.File.WriteAllText(@"C:\Users\Public\ErrorLog\errors.txt", text);
                        }//TODO ADD EXCEPTION HANDLING
                    }
                }
                replyStream.Dispose();
                client.Close();
            }
            catch (Exception) { 
                string text = "Class Master, line 683";
                System.IO.File.WriteAllText(@"C:\Users\Public\ErrorLog\errors.txt", text);
            }//TODO ADD EXCEPTION HANDLING
        }

        private void OnSlaveConnect(IAsyncResult asyn)
        {
            try
            {
                connectionListener.BeginAcceptSocket(new AsyncCallback(OnSlaveConnect), connectionListener);
                byte[] connectionData = new byte[2];
                // Get the listener that handles the client request.
                TcpListener listener = (TcpListener)asyn.AsyncState;

                // End the operation and display the received data on 
                // the console.
                TcpClient client = listener.EndAcceptTcpClient(asyn);
                NetworkStream replyStream = client.GetStream();

                //---read incoming stream---
                int bytesRead = replyStream.Read(connectionData, 0, connectionData.Length);
                replyStream.Dispose();
                client.Close();

                try
                {
                    RSV_Packet RSV = RSV_Packet.decode(connectionData, (byte)App.monitorCount);

                    Console.WriteLine("Slave connected, Monitor position: " + RSV.Position);

                    // Note: Monitor positions are 1-indexed.

                    // In the case of a missed ACK from a slave, it should send the same RSV again.
                    // If the same slave registers twice, the amount of slaves connected should not increase.
                    if (slavePool[RSV.Position - 1] == null)
                    {
                        slavesConnected++;
                        if (RSV.Position == 1)
                        {
                            slavePool[RSV.Position - 1] = IPAddress.Parse(App.slave1IP);
                        }
                        else if (RSV.Position == 2)
                        {
                            slavePool[RSV.Position - 1] = IPAddress.Parse(App.slave2IP);
                        }
                        else if (RSV.Position == 3)
                        {
                            slavePool[RSV.Position - 1] = IPAddress.Parse(App.slave3IP);
                        }
                        else if (RSV.Position == 4)
                        {
                            slavePool[RSV.Position - 1] = IPAddress.Parse(App.slave4IP);
                        }
                    }
                    else
                    {
                        ReConnect(RSV.Position - 1, false);
                    }
                }
                catch (ArgumentException ae)//TODO ADD EXCEPTION HANDLING
                {
                    string text = "Class Master, line 744";
                    System.IO.File.WriteAllText(@"C:\Users\Public\ErrorLog\errors.txt", text);
                    // Packet received was not a valid RSV packet.
                }
                catch (IndexOutOfRangeException ie)//TODO ADD EXCEPTION HANDLING
                {
                    throw new IndexOutOfRangeException("Slaves connected was greater than maximum possible slaves.", ie);
                }
            }
            catch (Exception) { 
                string text = "Class Master, line 754";
                System.IO.File.WriteAllText(@"C:\Users\Public\ErrorLog\errors.txt", text);
            }//TODO ADD EXCEPTION HANDLING
        }

        void UpdateIdleMedia(object sender, ElapsedEventArgs args)
        {
            try
            {
                if (isUpdating) { return; }
                isUpdating = true;
                idleTimer.Stop();
                // Split screens up into groups of largest possible size.
                var groups = new List<List<int>>();

                // Find the first occurance of an idle interaction.
                // Look to the right for additional idle interactions that can be part of the group.
                // If a non-idle interaction is found, that's the end of the group, start looking for the next group beginning until list is empty.
                bool startedGroup = false;

                for (int slaveIndex = 0; slaveIndex < interactionStates.Length; slaveIndex++)
                {
                    if (interactionStates[slaveIndex].State == InteractionState.STATE.Idle)
                    {
                        if (!startedGroup)
                        {
                            startedGroup = true;
                            groups.Add(new List<int>());
                        }
                        groups.Last().Add(slaveIndex);
                    }
                    else
                    {
                        if (startedGroup) startedGroup = false;
                    }
                }

                // group is a list of slaves adjacent to each other.
                for (int i = 0; i < groups.Count; i++)
                {
                    var group = groups[i];
                    // Get a media item appropriate for the group size.
                    // Choose the next media item to play.
                    nextMediaIndex = nextMediaIndex % mediaNameDict[group.Count].Length; // Amount of media items available in the group size.
                    string newMedia = mediaNameDict[group.Count][nextMediaIndex];
                    nextMediaIndex++;
                    Console.WriteLine($"Sending media to idle group: {newMedia}.");

                    // Update the slaves in the group.
                    for (int j = 0; j < group.Count; j++)
                    {
                        int slaveIndex = group[j];
                        // group.Count will be the amount of App-Spaces in the group, j refers to the position in the group,
                        // and needs to be converted from 0-indexed to 1-indexed
                        UCM_Packet UCM = new UCM_Packet((byte)group.Count, (byte)(j + 1), newMedia);

                        var ep = new IPEndPoint(slavePool[slaveIndex], portC);
                        try
                        {
                            TcpClient slaveUpdateSocket = new TcpClient(slavePool[slaveIndex].ToString(), portC);
                            slaveUpdateSocket.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                            NetworkStream slaveUpdate = slaveUpdateSocket.GetStream();
                            slaveUpdate.Write(UCM.encode(), 0, UCM.encode().Length);
                            slaveUpdate.Dispose();
                            slaveUpdateSocket.Close();
                            interactionStates[slaveIndex].ActiveMedia = newMedia;
                        }
                        catch (SocketException e)//TODO ADD EXCEPTION HANDLING
                        {
                            Console.Out.WriteLine(e);
                            string text = "Class Master, line 823";
                            System.IO.File.WriteAllText(@"C:\Users\Public\ErrorLog\errors.txt", text);
                            //interactionStates[slaveIndex].State = InteractionState.STATE.Active;
                        }
                    }
                }
                idleTimer.Interval = idleTimerInterval;
                idleTimer.Start();
                isUpdating = false;
            }
            catch (Exception) { 
                string text = "Class Master, line 834";
                System.IO.File.WriteAllText(@"C:\Users\Public\ErrorLog\errors.txt", text);
            }//TODO ADD EXCEPTION HANDLING
        }

        private void OnDataReceived(IAsyncResult asyn)
        {
            idleListener.BeginAcceptSocket(new AsyncCallback(OnDataReceived), idleListener);
            try
            {
                byte[] buffer = new byte[4];
                byte[] data = new byte[3];
                // Get the listener that handles the client request.
                TcpListener listener = (TcpListener)asyn.AsyncState;

                // End the operation and display the received data on 
                // the console.
                TcpClient client = listener.EndAcceptTcpClient(asyn);
                NetworkStream replyStream = client.GetStream();

                //---read incoming stream---
                int bytesRead = replyStream.Read(buffer, 0, buffer.Length);

                int slaveIndex = buffer[0] - 1;
                Array.Copy(buffer, 1, data, 0, (buffer.Length - 1));

                #region Create/Destroy Interaction
                try
                {
                    // App-Space has sent a request to create/destroy an interaction
                    Console.Out.WriteLine("CDI Length: " + buffer.Length);
                    CDI_Packet CDI = CDI_Packet.decode(data);
                    Console.Out.WriteLine("GOT CDI");
                    if (CDI.Mode == CDI_Packet.CDI_Mode.Create)
                    {
                        Console.Out.WriteLine("GOT Create");
                        TcpClient slaveUpdateSocket = new TcpClient(slavePool[slaveIndex].ToString(), portA);
                        slaveUpdateSocket.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                        NetworkStream slaveUpdate = slaveUpdateSocket.GetStream();
                        slaveUpdate.Write(CDI.encode(), 0, CDI.encode().Length);
                        slaveUpdate.Dispose();
                        slaveUpdateSocket.Close();
                        // Need to perform logic to determine if the request can be granted.
                        // If so, send back the CDI and await acknowledge to update state.
                        if (interactionStates[slaveIndex].State != InteractionState.STATE.Active)
                        {
                            Console.Out.WriteLine("CDI: Create Active");

                            // Change state.
                            if (CDI.Type == CDI_Packet.CDI_Type.Active)
                            {
                                Console.Out.WriteLine("CDI: Create Active");
                                interactionStates[slaveIndex].State = InteractionState.STATE.Active;
                            }
                            else if (interactionStates[slaveIndex].State != InteractionState.STATE.Idle) // If not already idle.
                            {
                                Console.Out.WriteLine("CDI: Create Active");
                                interactionStates[slaveIndex].State = InteractionState.STATE.Idle;
                            }
                            UpdateIdleMedia(null, null);
                        }
                        else if (interactionStates[slaveIndex].State == InteractionState.STATE.Active)
                        {
                            Console.Out.WriteLine("CDI: Create Active");

                            // Change state.
                            if (CDI.Type == CDI_Packet.CDI_Type.Active)
                            {
                                Console.Out.WriteLine("CDI: Create Active");
                                interactionStates[slaveIndex].State = InteractionState.STATE.Active;
                            }
                            else if (interactionStates[slaveIndex].State != InteractionState.STATE.Idle) // If not already idle.
                            {
                                Console.Out.WriteLine("CDI: Create Active");
                                interactionStates[slaveIndex].State = InteractionState.STATE.Idle;
                            }
                            UpdateIdleMedia(null, null);
                        }
                        // If the state is the same, it is likely a duplicate CDI. If it isn't, the CDI can still be safely ignored
                        else if ((interactionStates[slaveIndex].State == InteractionState.STATE.Active && CDI.Type == CDI_Packet.CDI_Type.Active)
                              || interactionStates[slaveIndex].State == InteractionState.STATE.Idle && CDI.Type == CDI_Packet.CDI_Type.Idle)
                        {
                            return;
                        }
                        // If the request cannot be granted, send a response with the Mode as Destroy to indicate this.
                        else
                        {
                            //CDI_Packet badResponse = new CDI_Packet(CDI.Type, CDI_Packet.CDI_Mode.Destroy);
                            //Task.Run(() => SendUntilACKReceive(badResponse, sb.Socket, slaveIndex, ep));
                        }
                    }
                }
                catch (ArgumentException)//TODO ADD EXCEPTION HANDLING
                {
                    string text = "Class Master, line 928";
                    System.IO.File.WriteAllText(@"C:\Users\Public\ErrorLog\errors.txt", text);
                }

                #endregion


                
            }
            catch (Exception) {
                string text = "Class Master, line 938";
                System.IO.File.WriteAllText(@"C:\Users\Public\ErrorLog\errors.txt", text);
            //TODO NEED TO HANDLE EXCEPTIONS
            }
        }

    }




    class InteractionState
    {
        public enum STATE
        {
            None,
            Active,
            Idle,
        }

        public STATE State { get; set; } = STATE.None;

        public string ActiveMedia { get; set; }
    }

    class AwaitingAck
    {
        public IVWP_Packet Packet { get; private set; }
        public IPEndPoint EndPoint { get; private set; }
        public int SlaveIndex { get; private set; }

        public AwaitingAck(IVWP_Packet packet, int slaveIndex, IPEndPoint ep)
        {
            Packet = packet;
            EndPoint = ep;
            SlaveIndex = slaveIndex;
        }

        public bool IsWaitingFor(ACK_Packet ACK, int slaveIndex, IPEndPoint ep)
        {
            return (Packet.Opcode == ACK.PreviousOpcode) && (SlaveIndex == slaveIndex) && (EndPoint.Equals(ep));
        }
    }
}
