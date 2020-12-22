
using Microsoft.Kinect;
using NewMaster.Packets;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ZeroMQ;

namespace NewMaster
{
    public partial class MainWindow : Window
    {
        // private ParametersUpdater updater;
        //private KinectSensor kinectSensor;
        private string slave1IP = App.slave1IP;
        private string slave2IP = App.slave2IP;
        private string slave3IP = App.slave3IP;
        private string slave4IP = App.slave4IP;
        private byte[] connectionData;
        //private MultiSourceFrameReader multiFrameSourceReader = null;

        /// <summary>
        /// Size of the RGB pixel in the bitmap
        /// </summary>
        private readonly int bytesPerPixel = (PixelFormats.Bgr32.BitsPerPixel + 7) / 8;

        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        private KinectSensor kinectSensor = null;

        /// <summary>
        /// Coordinate mapper to map one type of point to another
        /// </summary>
        private CoordinateMapper coordinateMapper = null;

        /// <summary>
        /// Reader for depth/color/body index frames
        /// </summary>
        private MultiSourceFrameReader multiFrameSourceReader = null;

        /// <summary>
        /// Bitmap to display
        /// </summary>
        private WriteableBitmap bitmap = null;

        /// <summary>
        /// The size in bytes of the bitmap back buffer
        /// </summary>
        private uint bitmapBackBufferSize = 0;

        /// <summary>
        /// Intermediate storage for the color to depth mapping
        /// </summary>
        private DepthSpacePoint[] colorMappedToDepthPoints = null;
        /// <summary>
        /// In pixels
        /// This Assumes that all monitors have the same size/resolution
        /// </summary>
        public static double monitorHeight;

        /// <summary>
        /// In pixels
        /// This Assumes that all monitors have the same size/resolution
        /// </summary>
        public static double monitorWidth;

        /// <summary>
        /// Current status text to display
        /// </summary>
        // private string statusText = null;
        // private Timer timer;
        private const double timerInterval = ((10.00 * 60.00) * 10) * 1; // 18 seconds
        private byte[] colorArray;
        private byte[] bodyIndexArray;
        private ushort[] depthArray;
        private bool slave1Closed;
        private TcpListener listener;
        private bool closeRequested = false;
        private TcpListener requestListener;
        private BackgroundWorker bw1;
        private bool slave2Closed;
        private bool slave3Closed;
        private bool slave4Closed;
        private IPEndPoint UpdateEndpoint;
        private int attempt = 0;

        public MainWindow()
        {
            this.connectionData = new byte[2];
            slave1Closed = true;
            slave2Closed = true;
            slave3Closed = true;
            slave4Closed = true;

            listener = new TcpListener(IPAddress.Any, 4000);
            Console.WriteLine("Listening...");
            listener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            listener.Start();
            listener.BeginAcceptSocket(new AsyncCallback(slaveUpdate), listener);

            this.kinectSensor = KinectSensor.GetDefault();

            this.multiFrameSourceReader = this.kinectSensor.OpenMultiSourceFrameReader(FrameSourceTypes.Depth | FrameSourceTypes.Color | FrameSourceTypes.BodyIndex);

            this.multiFrameSourceReader.MultiSourceFrameArrived += this.Reader_MultiSourceFrameArrived;

            this.coordinateMapper = this.kinectSensor.CoordinateMapper;

            FrameDescription depthFrameDescription = this.kinectSensor.DepthFrameSource.FrameDescription;

            int depthWidth = depthFrameDescription.Width;
            int depthHeight = depthFrameDescription.Height;

            FrameDescription colorFrameDescription = this.kinectSensor.ColorFrameSource.FrameDescription;

            int colorWidth = colorFrameDescription.Width;
            int colorHeight = colorFrameDescription.Height;

            this.colorMappedToDepthPoints = new DepthSpacePoint[colorWidth * colorHeight];

            this.bitmap = new WriteableBitmap(colorWidth, colorHeight, 96.0, 96.0, PixelFormats.Bgra32, null);

            // Calculate the WriteableBitmap back buffer size
            this.bitmapBackBufferSize = (uint)((this.bitmap.BackBufferStride * (this.bitmap.PixelHeight - 1)) + (this.bitmap.PixelWidth * this.bytesPerPixel));
            this.colorArray = new Byte[colorWidth * colorHeight * ((PixelFormats.Bgra32.BitsPerPixel) / 8)];

            OpenKinectSensor();

            this.bw1 = new BackgroundWorker();
            bw1.DoWork += new DoWorkEventHandler(bw1_DoWork);
            bw1.RunWorkerCompleted += Bw1_RunWorkerCompleted;
            bw1.RunWorkerAsync();

            InitializeComponent();

        }

        private void OpenKinectSensor()
        {
            // Only one sensor is supported
            this.kinectSensor = KinectSensor.GetDefault();

            if (this.kinectSensor != null)
            {
                // open the sensor
                this.kinectSensor.Open();

            }
        }

        private void Reader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            Console.Out.WriteLine("Frame Arrived");
            int depthWidth = 0;
            int depthHeight = 0;

            DepthFrame depthFrame = null;
            ColorFrame colorFrame = null;
            BodyIndexFrame bodyIndexFrame = null;
            bool isBitmapLocked = false;

            MultiSourceFrame multiSourceFrame = e.FrameReference.AcquireFrame();
            Console.Out.WriteLine("Checkpoint 1");

            // If the Frame has expired by the time we process this event, return.
            if (multiSourceFrame == null)
            {
                return;
            }
            Console.Out.WriteLine("Checkpoint 2");

            // We use a try/finally to ensure that we clean up before we exit the function.  
            // This includes calling Dispose on any Frame objects that we may have and unlocking the bitmap back buffer.
            try
            {
                depthFrame = multiSourceFrame.DepthFrameReference.AcquireFrame();
                colorFrame = multiSourceFrame.ColorFrameReference.AcquireFrame();
                bodyIndexFrame = multiSourceFrame.BodyIndexFrameReference.AcquireFrame();

                // If any frame has expired by the time we process this event, return.
                // The "finally" statement will Dispose any that are not null.
                if ((depthFrame == null) || (colorFrame == null) || (bodyIndexFrame == null))
                {
                    /*if (depthFrame == null)
                        Console.Out.WriteLine("Null Depth Frame");
                    if (colorFrame == null)
                        Console.Out.WriteLine("Null Color Frame");
                    if (bodyIndexFrame == null)
                        Console.Out.WriteLine("Null Body Index Frame");
                    if (colorFrame == null && depthFrame == null)
                    {
                        this.attempt++;
                        if (attempt == 5)
                        {
                            Console.Out.WriteLine("Reinitializing Kinect");
                            kinectSensor.Close();
                            kinectSensor.Open();
                            attempt = 0;
                        }
                    }*/
                    return;
                }
                attempt = 0;
                Console.Out.WriteLine("Checkpoint 3");

                // Process Depth
                FrameDescription depthFrameDescription = depthFrame.FrameDescription;

                depthWidth = depthFrameDescription.Width;
                depthHeight = depthFrameDescription.Height;

                // Access the depth frame data directly via LockImageBuffer to avoid making a copy
                using (KinectBuffer depthFrameData = depthFrame.LockImageBuffer())
                {
                    this.coordinateMapper.MapColorFrameToDepthSpaceUsingIntPtr(
                    depthFrameData.UnderlyingBuffer,
                    depthFrameData.Size,
                    this.colorMappedToDepthPoints);
                }

                // Process Color

                // Lock the bitmap for writing
                this.bitmap.Lock();
                isBitmapLocked = true;

                this.depthArray = new ushort[depthWidth * depthHeight];
                this.bodyIndexArray = new byte[depthWidth * depthHeight];
                colorFrame.CopyConvertedFrameDataToIntPtr(this.bitmap.BackBuffer, this.bitmapBackBufferSize, ColorImageFormat.Bgra);

                // We're done with the DepthFrame 
                depthFrame.Dispose();
                depthFrame = null;

                // We're done with the ColorFrame 
                colorFrame.Dispose();
                colorFrame = null;

                // We'll access the body index data directly to avoid a copy
                using (KinectBuffer bodyIndexData = bodyIndexFrame.LockImageBuffer())
                {
                    unsafe
                    {
                        byte* bodyIndexDataPointer = (byte*)bodyIndexData.UnderlyingBuffer;

                        int colorMappedToDepthPointCount = this.colorMappedToDepthPoints.Length;

                        fixed (DepthSpacePoint* colorMappedToDepthPointsPointer = this.colorMappedToDepthPoints)
                        {
                            // Treat the color data as 4-byte pixels
                            uint* bitmapPixelsPointer = (uint*)this.bitmap.BackBuffer;

                            // Loop over each row and column of the color image
                            // Zero out any pixels that don't correspond to a body index
                            for (int colorIndex = 0; colorIndex < colorMappedToDepthPointCount; ++colorIndex)
                            {
                                float colorMappedToDepthX = colorMappedToDepthPointsPointer[colorIndex].X;
                                float colorMappedToDepthY = colorMappedToDepthPointsPointer[colorIndex].Y;

                                // The sentinel value is -inf, -inf, meaning that no depth pixel corresponds to this color pixel.
                                if (!float.IsNegativeInfinity(colorMappedToDepthX) &&
                                    !float.IsNegativeInfinity(colorMappedToDepthY))
                                {
                                    // Make sure the depth pixel maps to a valid point in color space
                                    int depthX = (int)(colorMappedToDepthX + 0.5f);
                                    int depthY = (int)(colorMappedToDepthY + 0.5f);

                                    // If the point is not valid, there is no body index there.
                                    if ((depthX >= 0) && (depthX < depthWidth) && (depthY >= 0) && (depthY < depthHeight))
                                    {
                                        int depthIndex = (depthY * depthWidth) + depthX;

                                        // If we are tracking a body for the current pixel, do not zero out the pixel
                                        if (bodyIndexDataPointer[depthIndex] != 0xff)
                                        {
                                            continue;
                                        }
                                    }
                                }

                                bitmapPixelsPointer[colorIndex] = 0;
                            }
                        }

                        this.bitmap.AddDirtyRect(new Int32Rect(0, 0, this.bitmap.PixelWidth, this.bitmap.PixelHeight));
                        Console.Out.WriteLine("Checkpoint 4");
                    }
                }
            }
            finally
            {
                if (isBitmapLocked)
                {
                    this.bitmap.Unlock();
                }

                if (depthFrame != null)
                {
                    depthFrame.Dispose();
                }

                if (colorFrame != null)
                {
                    colorFrame.Dispose();
                }

                if (bodyIndexFrame != null)
                {
                    bodyIndexFrame.Dispose();
                }
            }
            Console.Out.WriteLine("Checkpoint 5");
            CompressAndSend();
        }
        private void slaveUpdate(IAsyncResult asyn)
        {
            listener.BeginAcceptSocket(new AsyncCallback(slaveUpdate), listener);
            try
            {
                Console.Out.WriteLine("Slave Connection Update");
                byte[] buffer = new byte[500];
                // Get the listener that handles the client request.
                TcpListener lst = (TcpListener)asyn.AsyncState;

                // End the operation and display the received data on 
                // the console.
                TcpClient client = lst.EndAcceptTcpClient(asyn);
                NetworkStream nwStream = client.GetStream();

                //---read incoming stream---
                int bytesRead = nwStream.Read(connectionData, 0, 2);

                

                if (connectionData[0] == (byte)1)
                {
                    Console.Out.WriteLine("A Slave has Connected");
                    Console.Out.WriteLine("Position: " + connectionData[1]);
                    if (connectionData[1] == (byte)1)
                    {
                        this.slave1Closed = false;
                        Console.Out.WriteLine("Slave 1 is Connected");
                    }
                    else if (connectionData[1] == (byte)2)
                    {
                        this.slave2Closed = false;
                        Console.Out.WriteLine("Slave 2 is Connected");
                    }
                    else if (connectionData[1] == (byte)3)
                    {
                        this.slave3Closed = false;
                        Console.Out.WriteLine("Slave 3 is Connected");
                    }
                    else if (connectionData[1] == (byte)4)
                    {
                        this.slave4Closed = false;
                        Console.Out.WriteLine("Slave 4 is Connected");
                    }
                }
                else if (connectionData[0] == (byte)0)
                {
                    if (connectionData[1] == (byte)1)
                    {
                        this.slave1Closed = true;
                        Console.Out.WriteLine("Slave 1 has Quit");

                    }
                    else if (connectionData[1] == (byte)2)
                    {
                        this.slave2Closed = true;
                        Console.Out.WriteLine("Slave 2 has Quit");

                    }
                    else if (connectionData[1] == (byte)3)
                    {
                        this.slave3Closed = true;
                        Console.Out.WriteLine("Slave 3 has Quit");

                    }
                    else if (connectionData[1] == (byte)4)
                    {
                        this.slave4Closed = true;
                        Console.Out.WriteLine("Slave 4 has Quit");

                    }
                }
            }
            catch (Exception) { }//TODO ADD EXCEPTION HANDLING
        }

        public void CompressAndSend()
        {
            byte[] source = ImageSourceToBytes(this.bitmap);
            byte[] header = BitConverter.GetBytes(source.Length);
            byte[] data = new byte[source.Length + header.Length];
            data[0] = (byte) source.Length;
            Array.Copy(header, 0, data, 0, header.Length);
            Array.Copy(source, 0, data, header.Length, source.Length);
            foreach(byte b in header)
            {
                Console.Out.Write("Header:" + b);
            }
            Console.Out.WriteLine("PNG Array Size:" + data.Length);
            try
            {
                if (!slave1Closed)
                {
                    TcpClient slaveUpdateSocket = new TcpClient(App.slave1IP, 4011);
                    slaveUpdateSocket.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                    NetworkStream slaveUpdate = slaveUpdateSocket.GetStream();
                    slaveUpdate.Write(data, 0, data.Length);
                    Console.Out.WriteLine("Sent Kinect Data To Slave " + App.slave1IP);
                    slaveUpdate.Dispose();
                    slaveUpdateSocket.Close();
                }
                if (!slave2Closed)
                {
                    TcpClient slaveUpdateSocket = new TcpClient(App.slave2IP, 4011);
                    slaveUpdateSocket.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                    NetworkStream slaveUpdate = slaveUpdateSocket.GetStream();
                    slaveUpdate.Write(data, 0, data.Length);
                    Console.Out.WriteLine("Sent Kinect Data To Slave " + App.slave2IP);
                    slaveUpdate.Dispose();
                    slaveUpdateSocket.Close();
                }
                if (!slave3Closed)
                {
                    TcpClient slaveUpdateSocket = new TcpClient(App.slave3IP, 4011);
                    slaveUpdateSocket.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                    NetworkStream slaveUpdate = slaveUpdateSocket.GetStream();
                    slaveUpdate.Write(data, 0, data.Length);
                    Console.Out.WriteLine("Sent Kinect Data To Slave " + App.slave3IP);
                    slaveUpdate.Dispose();
                    slaveUpdateSocket.Close();
                }
                if (!slave4Closed)
                {
                    TcpClient slaveUpdateSocket = new TcpClient(App.slave4IP, 4011);
                    slaveUpdateSocket.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                    NetworkStream slaveUpdate = slaveUpdateSocket.GetStream();
                    slaveUpdate.Write(data, 0, data.Length);
                    Console.Out.WriteLine("Sent Kinect Data To Slave " + App.slave4IP);
                    slaveUpdate.Dispose();
                    slaveUpdateSocket.Close();
                }
            }
            catch (Exception e)//TODO ADD EXCEPTION HANDLING
            {
                Console.Out.WriteLine("Error Sending Message" + e.Message);
            }
        }

        public byte[] ImageSourceToBytes(ImageSource imageSource)
        {
            BitmapEncoder encoder = new PngBitmapEncoder();
            byte[] bytes = null;
            var bitmapSource = imageSource as BitmapSource;

            if (bitmapSource != null)
            {
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

                using (var stream = new MemoryStream())
                {
                    encoder.Save(stream);
                    bytes = stream.ToArray();
                }
            }
            return bytes;
        }
        void bw1_DoWork(object sender, DoWorkEventArgs e)
        {
            Master master = new Master();
            master.Run();
        }
        private void Bw1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            bw1.Dispose();
        }

        public KinectSensor KinectSensor
        {
            get
            {
                return this.kinectSensor;
            }
        }

        private void WindowClosing(object sender, CancelEventArgs e)
        {

            if (null != this.kinectSensor)
            {
                this.kinectSensor.Close();
                this.kinectSensor = null;
            }
        }

    }
}