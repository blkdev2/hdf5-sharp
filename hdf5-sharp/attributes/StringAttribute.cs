//  
//  StringAttribute.cs
//  hdf-sharp
//
//  Created by Markus Uhr on 27/09/2010.
//  Copyright 2010 Markus Uhr. All rights reserved.
//

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Hdf5
{
    internal class StringAttribute : Attribute<string>
    {
        private static ASCIIEncoding encoding = new ASCIIEncoding();
        
        public StringAttribute(int raw) : base(raw)
        {
        }
        
        public override string Read()
        {
            string result = null;
            IntPtr[] buf = new IntPtr[1];
            using (Datatype mt = Type)
            using (Dataspace ms = Space)
            {
                GCHandle hbuf = GCHandle.Alloc(buf, GCHandleType.Pinned);
                try {
                    Read(mt, hbuf.AddrOfPinnedObject());
                    try {
                        result = Marshal.PtrToStringAnsi(buf[0]);
                    } finally {
                        int err = H5Dvlen_reclaim(mt.raw, ms.raw, 0, hbuf.AddrOfPinnedObject());
                        if (err < 0)
                            throw new ApplicationException("Error reclaiming memory.");
                    }
                } finally {
                    hbuf.Free();
                }
            }
            return result;
        }
        
        public override void Write(string data)
        {
            IntPtr[] buf = new IntPtr[1];
            using (Datatype mt = Type)
            using (Dataspace ms = Space)
            {
                buf[0] = Marshal.StringToHGlobalAnsi(data);
                try {
                    GCHandle hbuf = GCHandle.Alloc(buf, GCHandleType.Pinned);
                    try {
                        Write(mt, hbuf.AddrOfPinnedObject());
                    } finally {
                        hbuf.Free();
                    }
                } finally {
                    Marshal.FreeHGlobal(buf[0]);
                }
            }
        }
        
        // imports
        
        [DllImport("hdf5")]
        private static extern int H5Dvlen_reclaim(int type_id, int space_id, int plist_id, IntPtr buf);
    }
}
