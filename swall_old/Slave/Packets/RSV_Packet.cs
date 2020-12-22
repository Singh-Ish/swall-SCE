using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packets
{
    public class RSV_Packet : IVWP_Packet
    {

        private byte _position;

        public byte Position
        {
            get { return _position; }
        }

        public RSV_Packet(byte position)
        {
            _position = position;
        }

        public byte Opcode
        {
            get { return 0x01; }
        }

        public static RSV_Packet decode(byte[] packet, byte monitorCount)
        {
            if (PacketErrorChecker.validateRSV(packet, monitorCount))
            {
                return new RSV_Packet(packet[1]);
            }
            else
            {
                throw new ArgumentException("Invalid Packet Format");
            }
        }

        public byte[] encode()
        {
            return new byte[2] { Opcode, _position };
        }
    }
}
