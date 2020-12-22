using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewMaster.Packets
{
    public class UCM_Packet : IVWP_Packet
    {
        private byte _size;

        public byte Size
        {
            get { return _size; }
        }

        private byte _position;

        public byte Position
        {
            get { return _position; }
        }

        private byte[] _filename;

        public string Filename
        {
            get { return Encoding.ASCII.GetString(_filename, 0, _filename.Length); }
        }

        public byte Opcode
        {
            get { return 0x05; }
        }

        public UCM_Packet(byte size, byte position, string filename)
        {
            _size = size;
            _position = position;
            _filename = Encoding.ASCII.GetBytes(filename + "\t");
        }


        public byte[] encode()
        {
            byte[] returnVal = new byte[4 + _filename.Length];

            returnVal[0] = Opcode;
            returnVal[1] = _size;
            returnVal[2] = _position;
            returnVal[returnVal.Length - 1] = 0x00;

            Buffer.BlockCopy(_filename, 0, returnVal, 3, _filename.Length);

            return returnVal;
        }

        public static UCM_Packet decode(byte[] packet, byte monitorCount)
        {
            if (PacketErrorChecker.validateUCM(packet, monitorCount))
            {
                return new UCM_Packet(packet[1], packet[2], Encoding.ASCII.GetString(packet, 3, packet.Length - 4));
            }
            else
            {
                throw new ArgumentException("Invalid Packet Format");
            }
        }
    }
}
