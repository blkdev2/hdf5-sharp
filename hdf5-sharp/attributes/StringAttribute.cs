//  
// StringAttribute.cs
// hdf-sharp
//
// Copyright 2010 Markus Uhr <markus.uhr@bsse.ethz.ch>.
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions
// are met:
//     * Redistributions of source code must retain the above copyright
//       notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright
//       notice, this list of conditions and the following disclaimer in
//       the documentation and/or other materials provided with the
//       distribution.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
// "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
// OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
// LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
// DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
// THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Hdf5
{
    internal class StringAttribute : Attribute<string>
    {
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
