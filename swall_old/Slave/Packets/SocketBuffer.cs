using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Packets
{
    public class SocketBuffer
    {
        Socket _socket;
        public Socket Socket
        {
            get { return _socket; }
        }
        byte[] _buffer = new byte[1024];
        public byte[] Buffer
        {
            get { return _buffer; }
        }

        public SocketBuffer(Socket socket)
        {
            _socket = socket;
        }
    }
}
