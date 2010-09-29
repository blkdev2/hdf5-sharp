//  
// StringDataset.cs
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
    internal class StringDataset : Dataset<string>
    {
        public StringDataset(int raw) : base(raw)
        {
        }
        
        public override void Read(Dataspace ms, Dataspace fs, Array buf)
        {
            if (buf.GetType().GetElementType() != typeof(string))
                throw new ArgumentException("Incompatible type of \"buf\" parameter.");
            using (Datatype mt = Datatype.VlenString)
            {
                long[] dim;
                if (ms == Dataspace.All)
                {
                    dim = new long[buf.Rank];
                    for (int i=0; i<buf.Rank; i++)
                        dim[i] = buf.GetLongLength(i);
                }
                else
                {
                    dim = ms.GetDimensions();
                }
                int rank = dim.Length;
//                if (rank != 1)
//                    throw new ApplicationException("Invalid data space.");
                Array mbuf = Array.CreateInstance(typeof(IntPtr), dim);
                // read data
                GCHandle hbuf = GCHandle.Alloc(mbuf, GCHandleType.Pinned);
                try {
                    // read char buf
                    Read(mt, ms, fs, hbuf.AddrOfPinnedObject());
                    try {
                        // marshal strings
                        long[] idx = new long[rank];
                        while (idx[rank-1] < dim[rank-1])
                        {
                            buf.SetValue(Marshal.PtrToStringAnsi((IntPtr)mbuf.GetValue(idx)), idx);
                            for (int i=0; i<rank; i++)
                            {
                                idx[i]++;
                                if (i < rank-1 && idx[i] == dim[i])
                                    idx[i] = 0;
                                else
                                    break;
                            }
                        }
                    } finally {
                        // cleaning up
                        int err = 0;
                        if (ms != Dataspace.All)
                            err = H5Dvlen_reclaim(mt.raw, ms.raw, 0, hbuf.AddrOfPinnedObject());
                        else if (fs != Dataspace.All)
                            err = H5Dvlen_reclaim(mt.raw, fs.raw, 0, hbuf.AddrOfPinnedObject());
                        else using (Dataspace sp = Space)
                            err = H5Dvlen_reclaim(mt.raw, sp.raw, 0, hbuf.AddrOfPinnedObject());
                        if (err < 0)
                            throw new ApplicationException("Error reclaiming vlen buffer.");
                    }
                } finally {
                    hbuf.Free();
                }
            }
        }
        
        public override void Write(Dataspace ms, Dataspace fs, Array buf)
        {
            if (buf.GetType().GetElementType() != typeof(string))
                throw new ArgumentException("Incompatible type of \"buf\" parameter.");
//            long[] msdim = ms.GetDimensions();
//            if (msdim.Length != data.Rank)
//                throw new ArgumentException("Memory dataspace incompatible with data array.");
            using (Datatype mt = Datatype.VlenString)
            {
                long[] dim;
                if (ms == Dataspace.All)
                {
                    dim = new long[buf.Rank];
                    for (int i=0; i<buf.Rank; i++)
                        dim[i] = buf.GetLongLength(i);
                }
                else
                {
                    dim = ms.GetDimensions();
                }
                int rank = dim.Length;
//                if (rank != 1)
//                    throw new ApplicationException("Invalid data space.");
                // marshal strings
                Array mbuf = Array.CreateInstance(typeof(IntPtr), dim);
                long[] idx = new long[rank];
                while (idx[rank-1] < dim[rank-1])
                {
                    string bidx = (string)buf.GetValue(idx);
                    if (bidx != null)
                    {
                        IntPtr hidx = Marshal.StringToHGlobalAnsi(bidx);
                        mbuf.SetValue(hidx, idx);
                    }
                    else
                    {
                        mbuf.SetValue(IntPtr.Zero, idx);
                    }
                    for (int i=0; i<rank; i++)
                    {
                        idx[i]++;
                        if (i < rank-1 && idx[i] == dim[i])
                            idx[i] = 0;
                        else
                            break;
                    }
                }
                // write data
                GCHandle hbuf = GCHandle.Alloc(mbuf, GCHandleType.Pinned);
                try {
                    Write(mt, ms, fs, hbuf.AddrOfPinnedObject());
                } finally {
                    hbuf.Free();
                }
                // clean up string copies
                idx = new long[rank];
                while (idx[rank-1] < dim[rank-1])
                {
                    IntPtr hidx = (IntPtr)mbuf.GetValue(idx);
                    if (hidx != IntPtr.Zero)
                        Marshal.FreeHGlobal(hidx);
                    for (int i=0; i<rank; i++)
                    {
                        idx[i]++;
                        if (i < rank-1 && idx[i] == dim[i])
                            idx[i] = 0;
                        else
                            break;
                    }
                }
            }
        }
    }
}
