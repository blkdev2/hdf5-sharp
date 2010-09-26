//  
//  StringDataset.cs
//  hdf-sharp
//
//  Created by Markus Uhr on 26/09/2010.
//  Copyright 2010 Markus Uhr. All rights reserved.
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
        
        // imports
        
        [DllImport("hdf5")]
        private static extern int H5Dvlen_reclaim(int type_id, int space_id, int plist_id, IntPtr buf);
    }
}
