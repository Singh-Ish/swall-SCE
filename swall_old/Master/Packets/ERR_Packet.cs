using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewMaster.Packets
{
    public class ERR_Packet : IVWP_Packet
    {

        private byte _errorCode;

        public byte ErrorCode
        {
            get { return _errorCode; }
        }

        private byte[] _message;

        private string Message
        {
            get { return Encoding.ASCII.GetString(_message); }
        }

        public byte Opcode
        {
            get { return 0x07; }
        }

        public ERR_Packet(byte errorCode, string message)
        {
            _errorCode = errorCode;
            _message = Encoding.ASCII.GetBytes(message);
        }

        public byte[] encode()
        {
            byte[] returnVal = new byte[3 + _message.Length];

            returnVal[0] = Opcode;
            returnVal[1] = _errorCode;
            returnVal[returnVal.Length - 1] = 0x00;

            Buffer.BlockCopy(_message, 0, returnVal, 2, _message.Length);

            return returnVal;
        }

        public static ERR_Packet decode(byte[] packet)
        {
            if (PacketErrorChecker.validateERR(packet))
            {
                return new ERR_Packet(packet[1], Encoding.ASCII.GetString(packet, 2, packet.Length - 3));
            }
            else
            {
                throw new ArgumentException("Invalid Packet Format");
            }
        }
    }
}
