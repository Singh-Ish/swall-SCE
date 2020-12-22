using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewMaster.Packets
{
    public class UIC_Packet : IVWP_Packet
    {
        private byte[] _data;

        public byte[] Data
        {
            get { return _data; }
        }

        public byte Opcode
        {
            get { return 0x04; }
        }

        public UIC_Packet(byte[] data)
        {
            _data = data;
        }

        public byte[] encode()
        {
            byte[] returnVal = new byte[2 + _data.Length];

            returnVal[0] = Opcode;
            returnVal[returnVal.Length - 1] = 0x00;

            Buffer.BlockCopy(_data, 0, returnVal, 1, _data.Length);

            return returnVal;
        }

        public static UIC_Packet decode(byte[] packet)
        {
            if (PacketErrorChecker.validateUIC(packet))
            {
                byte[] data = new byte[packet.Length - 2];
                Array.Copy(packet, 1, data, 0, packet.Length - 2);
                return new UIC_Packet(data);
            }
            else
            {
                throw new ArgumentException("Invalid Packet Format");
            }
        }
    }
}
