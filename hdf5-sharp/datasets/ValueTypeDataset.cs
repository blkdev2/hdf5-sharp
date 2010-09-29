// 
// ValueTypeDataset.cs
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

namespace Hdf5
{
    internal class ValueTypeDataset<T> : Dataset<T> where T : struct
    {
        public ValueTypeDataset(int raw) : base(raw)
        {
        }
        
        public override void Read(Dataspace ms, Dataspace fs, Array buf)
        {
            if (buf.GetType().GetElementType() != typeof(T))
                throw new ArgumentException("Incompatible type of \"buf\" parameter.");
            using (Datatype mt = Datatype.FromValueType(typeof(T)))
            {
                // pin and read
                GCHandle hres = GCHandle.Alloc(buf, GCHandleType.Pinned);
                try {
                    Read(mt, ms, fs, hres.AddrOfPinnedObject());
                } finally {
                    hres.Free();
                }
            }
        }
        
        public override void Write(Dataspace ms, Dataspace fs, Array buf)
        {
            if (buf.GetType().GetElementType() != typeof(T))
                throw new ArgumentException("Incompatible type of \"buf\" parameter.");
//            long[] msdim = ms.GetDimensions();
//            if (msdim.Length != data.Rank)
//                throw new ArgumentException("Memory dataspace incompatible with data array.");
            using (Datatype mt = Datatype.FromValueType(typeof(T)))
            {
                GCHandle hbuf = GCHandle.Alloc(buf, GCHandleType.Pinned);
                try {
                    Write(mt, ms, fs, hbuf.AddrOfPinnedObject());
                } finally {
                    hbuf.Free();
                }
            }
        }
    }
}
