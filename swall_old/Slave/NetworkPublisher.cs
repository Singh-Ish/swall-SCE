﻿using System;
using ZeroMQ;

namespace Prototype1
{
    public class NetworkPublisher
    {
        private ZContext context;
        private ZSocket socket;
        private Boolean binded;

        public NetworkPublisher()
        {
            this.context = new ZContext();
            this.socket = new ZSocket(this.context, ZSocketType.PUB);
            this.binded = false;
        }

        public void Bind(String listeningPort)
        {
            String status = null;
            try
            {
                this.socket.Bind("tcp://*:" + listeningPort);
                this.binded = true;
                Console.Out.Write("tcp://*:" + listeningPort);
            }
            catch (ZException e)//TODO ADD EXCEPTION HANDLING
            {
                status = ("Socket connection failed, server cannot listen on port " + listeningPort + ": " + e.Message);
            }
        }

        public void SendString(String message, String topic)
        {
            lock (this.socket)
            {
                String status = null;
                if (this.binded)
                {
                    ZFrame frame = new ZFrame(string.Format(topic + " {0}", message));
                    try
                    {
                        this.socket.Send(frame);
                    }
                    catch (ZException e)//TODO ADD EXCEPTION HANDLING
                    {
                        status = "Cannot publish message: " + e.Message;
                    }
                }
                else
                {
                    status = ("Cannot publish message: Not binded");
                }
            }
        }


        public void SetConflate()
        {
            this.socket.SetOption(ZSocketOption.CONFLATE, 1);
        }

        public void SendByteArray(Byte[] byteArray)
        {
            String status = null;
            //lock (this.socket)
            //{
            if (this.binded)
            {
                ZFrame frame = new ZFrame(byteArray);
                try
                {
                    this.socket.Send(frame);
                }
                catch (ZException e)//TODO ADD EXCEPTION HANDLING
                {
                    status = "Cannot publish message: " + e.Message;
                }

            }
            else
            {
                status = ("Cannot publish message: Not binded");
            }
            //}
        }

        public void Close()
        {
            this.socket.Close();
            this.binded = false;
        }

        ~NetworkPublisher()
        {
            this.context.Dispose();
        }
    }
}
