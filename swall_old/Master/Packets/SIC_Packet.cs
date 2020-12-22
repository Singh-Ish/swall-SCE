using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewMaster.Packets
{
    public class SIC_Packet : IVWP_Packet
    {
        private byte[] _data;

        public string Data
        {
            get { return Encoding.ASCII.GetString(_data, 0, _data.Length); }
        }

        public byte Opcode
        {
            get
            {
                return 0x03;
            }
        }

        public SIC_Packet(string data)
        {
            _data = Encoding.ASCII.GetBytes(data);
        }

        public byte[] encode()
        {
            byte[] returnVal = new byte[2 + _data.Length];

            returnVal[0] = Opcode;
            returnVal[returnVal.Length - 1] = 0x00;

            Buffer.BlockCopy(_data, 0, returnVal, 1, _data.Length);

            return returnVal;
        }

        public static SIC_Packet decode(byte[] packet)
        {
            if (PacketErrorChecker.validateSIC(packet))
            {
                return new SIC_Packet(Encoding.ASCII.GetString(packet, 1, packet.Length - 2));
            }
            else
            {
                throw new ArgumentException("Invalid Packet Format");
            }
        }
    }
}
