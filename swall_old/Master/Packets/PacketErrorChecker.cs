using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewMaster.Packets
{
    public class PacketErrorChecker
    {
        public enum Packet_Opcode : byte
        {     
            RSV = 0x01,
            CDI = 0x02,
            SIC = 0x03,
            UIC = 0x04,
            ACK = 0x06,
            UCM = 0x05,
            ERR = 0x07,  
        }

        public static bool validateRSV(byte[] contents, byte monitorCount)
        {
            if (contents.Length != 2
                || contents[0] != (byte)Packet_Opcode.RSV
                || contents[1] < 0x01
                || contents[1] > monitorCount)
            {
                return false;
            } 

            return true;
        }

        public static bool validateCDI(byte[] contents)
        {
            
            if (contents.Length != 3
                || contents[0] != (byte)Packet_Opcode.CDI
                || !Enum.IsDefined(typeof(CDI_Packet.CDI_Type), contents[1])
                || !Enum.IsDefined(typeof(CDI_Packet.CDI_Mode), contents[2]))
            {
                return false;
            }

            return true;
        }

        public static bool validateSIC(byte[] contents)
        {
 
            if (contents[0] != (byte)Packet_Opcode.SIC
                || contents[contents.Length - 1] != 0x00)
            {
                return false;
            }

            string data = Encoding.ASCII.GetString(contents, 1, contents.Length - 2);
            //todo: validate data

            return true;
        }

        public static bool validateUIC(byte[] contents)
        {

            if (contents[0] != (byte)Packet_Opcode.UIC
                || contents[contents.Length - 1] != 0x00)
            {
                return false;
            }

            string data = Encoding.ASCII.GetString(contents, 1, contents.Length - 2);
            //todo: validate data

            return true;
        }

        public static bool validateUCM(byte[] contents, byte monitorCount)
        {

            if (contents[0] != (byte)Packet_Opcode.UCM
                || contents[1] < 0x01
                || contents[1] > monitorCount
                || contents[2] < 0x01
                || contents[2] > contents[1]
                || contents[contents.Length - 1] != 0x00)
            {
                return false;
            }

            string filename = Encoding.ASCII.GetString(contents, 3, contents.Length - 4);
            //todo: validate filename

            return true;
        }

        public static bool validateACK(byte[] contents)
        {

            if (contents.Length != 2
                || contents[0] != (byte)Packet_Opcode.ACK
                || contents[1] < 0x01
                || contents[1] > 0x07)
            {
                return false;
            }

            return true;
        }

        public static bool validateERR(byte[] contents)
        {

            if (contents[0] != (byte)Packet_Opcode.ERR
                || contents[1] < 0x01
                || contents[1] > 0x03
                || contents[contents.Length - 1] != 0x00)
            {
                return false;
            }

            string message = Encoding.ASCII.GetString(contents, 2, contents.Length - 3);
            //todo: validate message

            return true;
        }

    }
}
