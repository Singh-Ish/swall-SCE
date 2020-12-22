using InteractionCoaxer.PoiCoaxer;
using NewMaster.Packets;
using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace NewMaster
{
    public class ICModelController : IDisposable
    {
        private int portB = 70;
        Socket socket;

        private KinectSensor kinectSensor;
        private BodyFrameReader frameSourceReader;
        private Body[] bodies;

        private PoiFromKinectBodyCoaxerModel model;

        public ICModelController()
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            kinectSensor = KinectSensor.GetDefault();
            model = new PoiFromKinectBodyCoaxerModel();
            model.TrackingJoint = JointType.Head;
            model.Mapper = kinectSensor.CoordinateMapper;
            model.PlanerWidth = kinectSensor.ColorFrameSource.FrameDescription.Width;
            model.PlanerHeight = kinectSensor.ColorFrameSource.FrameDescription.Height;

            kinectSensor.Open();

            kinectSensor.IsAvailableChanged += KinectSensor_IsAvailableChanged;

            KinectSensor_IsAvailableChanged(null, null);
        }

        public void StartView(string view, IPAddress ip)
        {
            SIC_Packet SIC = new SIC_Packet(view);
            EndPoint ep = new IPEndPoint(ip, portB);
            byte[] data = SIC.encode();
            byte[] response = new byte[2];
            bool hasTimedOut;
            do
            {
                hasTimedOut = false;
                socket.SendTo(data, ep);
                try
                {
                    socket.ReceiveFrom(response, ref ep);
                    if (!PacketErrorChecker.validateACK(response))
                    {
                        hasTimedOut = true;
                    }
                }
                catch (SocketException e)//TODO ADD EXCEPTION HANDLING
                {
                    if (e.SocketErrorCode == SocketError.TimedOut)
                    {
                        //Retry registration if receiving ACK timed out
                        hasTimedOut = true;
                    }
                    else
                    {
                      //throw e;
                    }
                }
            } while (hasTimedOut);
           // System.Diagnostics.Debug.WriteLine("ACK received for SIC");

        }

        public void UpdateSlaves(IPAddress[] slavePool)
        {
            foreach (var ip in slavePool)
            {
                if (model.POICount != 0)
                {
                    UIC_Packet UIC = new UIC_Packet(model.encode());
                    socket.SendTo(UIC.encode(), new IPEndPoint(ip, portB));
                   // Console.Out.WriteLine("Sent UIC");
                }
            }
        }
        private void KinectSensor_IsAvailableChanged(object sender, IsAvailableChangedEventArgs e)
        {
            
            if (kinectSensor.IsAvailable)
            {
                frameSourceReader = kinectSensor.BodyFrameSource.OpenReader();
                frameSourceReader.FrameArrived += FrameSourceReader_FrameArrived;
                if (!kinectSensor.IsOpen)
                {
                    kinectSensor.Open();
                }
            }
            else if (frameSourceReader != null)
            {
                frameSourceReader.FrameArrived -= FrameSourceReader_FrameArrived;
                frameSourceReader.Dispose();
                frameSourceReader = null;
            }
        }

        public void FrameSourceReader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyFrame != null)
                {
                    if (bodies == null)
                    {
                        // creates an array of bodies using the amount of bodies the Kinect can track (in v2 this is 6)
                        bodies = new Body[bodyFrame.BodyCount];
                        model.POICount = bodyFrame.BodyCount;
                    }

                    // The first time GetAndRefreshBodyData is called, Kinect will allocate each Body in the array.
                    // As long as those body objects are not disposed and not set to null in the array,
                    // those body objects will be re-used.
                    bodyFrame.GetAndRefreshBodyData(bodies);
                }
            }

            if (bodies != null)
            {
                model.Update(bodies);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (frameSourceReader != null)
                {
                    frameSourceReader.FrameArrived -= FrameSourceReader_FrameArrived;
                    frameSourceReader.Dispose();
                    frameSourceReader = null;
                }
            }
            if (kinectSensor != null)
            {
                kinectSensor.IsAvailableChanged -= KinectSensor_IsAvailableChanged;
                kinectSensor = null;
            }
        }

    }
}
