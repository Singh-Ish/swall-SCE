﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewMaster.Packets
{
    public interface IVWP_Packet
    {

        byte Opcode
        {
            get;
        }
       
        byte[] encode();

    }
}
