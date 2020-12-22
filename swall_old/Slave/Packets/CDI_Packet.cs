using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packets
{
    public class CDI_Packet : IVWP_Packet
    {

        public enum CDI_Type : byte
        {
            Active = 0x01,
            Idle = 0x02
        }

        public enum CDI_Mode : byte
        {
            Create = 0x01,
            Destroy = 0x02
        }

        private CDI_Type _type;

        public CDI_Type Type
        {
            get { return _type; }
        }

        private CDI_Mode _mode;

        public CDI_Mode Mode
        {
            get { return _mode; }
        }

        public byte Opcode
        {
            get {  return 0x02; }
        }

        public CDI_Packet(CDI_Type type, CDI_Mode mode)
        {
            _type = type;
            _mode = mode;
        }

        public byte[] encode()
        {
            return new byte[3] { Opcode, (byte)_type, (byte)_mode };
        }

        public static CDI_Packet decode(byte[] packet)
        {
            if (PacketErrorChecker.validateCDI(packet))
            {
                return new CDI_Packet((CDI_Type) packet[1], (CDI_Mode) packet[2]);
            }
            else
            {
                throw new ArgumentException("Invalid Packet Format");
            }
        }
    }
}
