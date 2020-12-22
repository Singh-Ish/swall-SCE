using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packets
{
    public class ACK_Packet : IVWP_Packet
    {
        private byte _previousOpcode;

        public byte PreviousOpcode
        {
            get { return _previousOpcode; }
        }

        public byte Opcode
        {
            get { return 0x06; }
        }

        public ACK_Packet(byte previousOpcode)
        {
            _previousOpcode = previousOpcode;
        }

        public byte[] encode()
        {
            return new byte[2] { Opcode, _previousOpcode };
        }

        public static ACK_Packet decode(byte[] packet)
        {
            if (PacketErrorChecker.validateACK(packet))
            {
                return new ACK_Packet(packet[1]);
            } else
            {
                throw new ArgumentException("Invalid Packet Format");
            }
        }
    }
}
