using Packets;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ZeroMQ;


namespace Prototype1
{
    /// <summary>
    /// Interaction logic for ClientView.xaml
    /// </summary>
    public partial class Client : Window
    {
        private TcpListener masterUpdateListener;
        private byte slaveNumber;
        private int slaveCount;
        private string masterIP = App.masterIP;
        public Client(bool first)
        {
            InitializeComponent();
            masterUpdateListener = new TcpListener(IPAddress.Any, 4011);
            Console.WriteLine("Listening for Master Updates...");
            masterUpdateListener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            masterUpdateListener.Start();
            masterUpdateListener.BeginAcceptTcpClient(new AsyncCallback(MasterUpdate), masterUpdateListener);
            Request();
            this.slaveNumber = App.monitorPosition;
            this.slaveCount = App.monitorCount;
        }

        private void MasterUpdate(IAsyncResult ar)
        {
            try
            {
                masterUpdateListener.BeginAcceptTcpClient(new AsyncCallback(MasterUpdate), masterUpdateListener);
                Console.Out.WriteLine("Master Update Received");
                // Get the listener that handles the client request.
                TcpListener listener = (TcpListener)ar.AsyncState;

                // End the operation and display the received data on 
                // the console.
                TcpClient client = listener.EndAcceptTcpClient(ar);
                NetworkStream replyStream = client.GetStream();
                byte[] header = new byte[4];
                replyStream.Read(header, 0, 4);
                byte[] buffer = new byte[BitConverter.ToInt32(header, 0)];
                Console.Out.WriteLine("BufferSize: " + buffer.Length);
                int bytesRead = 0;
                //---read incoming stream---
                while (true)
                {
                    bytesRead += replyStream.Read(buffer, bytesRead, buffer.Length - bytesRead);
                    if (bytesRead == buffer.Length) { break; }
                }
                Console.Out.WriteLine("KinectData: " + bytesRead);
                client.Close();
                client.Client.Dispose();
                replyStream.Dispose();
                
                try
                {
                    if (!System.Windows.Application.Current.Dispatcher.CheckAccess())  //Must execute this method on the UI thread
                    {
                        //remove the current coaxer
                        System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            if (bytesRead == 8167)
                            {
                                camera.Source = null;
                                return;
                            }
                            Stream imageStreamSource = new MemoryStream(buffer);

                            PngBitmapDecoder decoder = new
                            PngBitmapDecoder(imageStreamSource, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.None);

                            BitmapSource source1 = decoder.Frames[0];

                            Bitmap bmp = GetBitmap(source1);
                            if (bmp == null) { return; }
                            int Height = bmp.Height;
                            int Width = bmp.Width;
                            int bitmapFraction = Width / slaveCount;
                            int startingPoint = ((slaveNumber - 1) * (Width / slaveCount));
                            if (bmp != null)
                            {
                                Bitmap bmp1 = CropBitmap(bmp, startingPoint, 0, bitmapFraction, Height);
                                BitmapSource source = CreateBitmapSourceFromGdiBitmap(bmp1);
                                camera.Source = null;
                                camera.Source = source;

                                bmp1.Dispose();
                            }
                            imageStreamSource.Dispose();
                            bmp.Dispose();

                        }));
                    }
                    else
                    {
                        if (bytesRead == 8167)
                        {
                            camera.Source = null;
                            return;
                        }
                        Stream imageStreamSource = new MemoryStream(buffer);

                        PngBitmapDecoder decoder = new
                        PngBitmapDecoder(imageStreamSource, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.None);

                        BitmapSource source1 = decoder.Frames[0];

                        Bitmap bmp = GetBitmap(source1);
                        if (bmp == null) { return; }
                        int Height = bmp.Height;
                        int Width = bmp.Width;
                        int bitmapFraction = Width / slaveCount;
                        int startingPoint = ((slaveNumber - 1) * (Width / slaveCount));
                        if (bmp != null)
                        {
                            Bitmap bmp1 = CropBitmap(bmp, startingPoint, 0, bitmapFraction, Height);
                            BitmapSource source = CreateBitmapSourceFromGdiBitmap(bmp1);
                            camera.Source = null;
                            camera.Source = source;

                            bmp1.Dispose();
                        }
                        imageStreamSource.Dispose();
                        bmp.Dispose();
                    }
                }
                catch (ArgumentException ae)//TODO ADD EXCEPTION HANDLING
                {
                    GC.Collect();
                    Console.Error.WriteAsync("Error" + ae.Message);
                }
                GC.Collect();
            }
            catch (Exception) { }//TODO ADD EXCEPTION HANDLING
        }

        public Bitmap CropBitmap(Bitmap bitmap,
                         int cropX, int cropY,
                         int cropWidth, int cropHeight)
        {
            System.Drawing.Rectangle rect = new System.Drawing.Rectangle(cropX, cropY, cropWidth, cropHeight);
            return bitmap.Clone(rect, bitmap.PixelFormat);
        }
        Bitmap GetBitmap(BitmapSource source)
        {
            try
            {
                Bitmap bmp = new Bitmap(source.PixelWidth, source.PixelHeight, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
                BitmapData data = bmp.LockBits(new System.Drawing.Rectangle(System.Drawing.Point.Empty, bmp.Size), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
                source.CopyPixels(Int32Rect.Empty, data.Scan0, data.Height * data.Stride, data.Stride); bmp.UnlockBits(data);
                return bmp;
            }
            catch (Exception)//TODO ADD EXCEPTION HANDLING
            {
                return null;
            }
        }

        public static BitmapSource CreateBitmapSourceFromGdiBitmap(Bitmap bitmap)
        {
            if (bitmap == null)
                return null;

            var rect = new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height);

            var bitmapData = bitmap.LockBits(
                rect,
                ImageLockMode.ReadWrite,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            try
            {
                var size = (rect.Width * rect.Height) * 4;

                return BitmapSource.Create(
                    bitmap.Width,
                    bitmap.Height,
                    bitmap.HorizontalResolution,
                    bitmap.VerticalResolution,
                    PixelFormats.Bgra32,
                    null,
                    bitmapData.Scan0,
                    size,
                    bitmapData.Stride);
            }
            finally
            {
                bitmap.UnlockBits(bitmapData);
                bitmap.Dispose();
            }
        }
        public void Request()
        {
            try
            {
                byte[] status = new byte[2];
                status[0] = (byte)1;
                status[1] = App.monitorPosition;
                TcpClient masterResponseSocket = new TcpClient(masterIP, 4000);
                NetworkStream masterResponse = masterResponseSocket.GetStream();

                //---send the text---
                Console.WriteLine("Sending Reconnection Message to Server");
                masterResponse.Write(status, 0, status.Length);
                //masterResponse.Dispose();
                //masterResponseSocket.Close();
            }
            catch { }//TODO ADD EXCEPTION HANDLING
        }
        public event EventHandler Hiding;

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            try
            {
                byte[] status = new byte[2];
                status[0] = (byte)0;
                status[1] = App.monitorPosition;
                TcpClient masterResponseSocket = new TcpClient(masterIP, 4000);
                NetworkStream masterResponse = masterResponseSocket.GetStream();

                //---send the text---
                Console.WriteLine("Sending Closing Message to Server");
                masterResponse.Write(status, 0, status.Length);
                masterResponse.Dispose();
                masterResponseSocket.Close();
                masterUpdateListener.Stop();
                masterUpdateListener.Server.Dispose();
                GC.Collect();
            }
            catch { }//TODO ADD EXCEPTION HANDLING
        }
    }

}
