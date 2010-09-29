//  
//  VlenValueTypeDataset.cs
//  hdf-sharp
//
//  Created by Markus Uhr on 28/09/2010.
//  Copyright 2010 Markus Uhr. All rights reserved.
//

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Hdf5
{
    internal class VlenValueTypeDataset<T> : Dataset<T[]> where T : struct
    {
        public VlenValueTypeDataset(int raw) : base(raw)
        {
        }
        
        public override void Read(Dataspace ms, Dataspace fs, Array buf)
        {
            using (Datatype mt = Datatype.VlenValueType(typeof(T)))
            {
                long[] dim;
                if (ms != Dataspace.All)
                    dim = ms.GetDimensions();
                else if (fs != Dataspace.All)
                    dim = fs.GetDimensions();
                else using (Dataspace sp = Space)
                    dim = sp.GetDimensions();
                int rank = dim.Length;
                // pin and read
                Array vbuf = Array.CreateInstance(typeof(VLen), dim);
                GCHandle hbuf = GCHandle.Alloc(vbuf, GCHandleType.Pinned);
                try {
                    Read(mt, ms, fs, hbuf.AddrOfPinnedObject());
                    try {
                        // marshal values
                        long[] idx = new long[rank];
                        while (idx[rank-1] < dim[rank-1])
                        {
                            ulong ptr = (ulong)((VLen)vbuf.GetValue(idx)).Ptr;
                            long len = ((VLen)vbuf.GetValue(idx)).Len;
                            uint size = (uint)Marshal.SizeOf(typeof(T));
                            T[] tmp = new T[len];
                            if (ptr != 0)
                            {
                                for (uint i=0; i<len; i++)
                                    tmp[i] = (T)Marshal.PtrToStructure((IntPtr)(ptr+i*size), typeof(T));
                            }
                            buf.SetValue(tmp, idx);
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
                        if (ms != Dataspace.All)
                            H5Dvlen_reclaim(mt.raw, ms.raw, 0, hbuf.AddrOfPinnedObject());
                        else if (fs != Dataspace.All)
                            H5Dvlen_reclaim(mt.raw, fs.raw, 0, hbuf.AddrOfPinnedObject());
                        else using (Dataspace sp = Space)
                            H5Dvlen_reclaim(mt.raw, sp.raw, 0, hbuf.AddrOfPinnedObject());
                    }
                } finally {
                    hbuf.Free();
                }
            }
        }
        
        public override void Write(Dataspace ms, Dataspace fs, Array buf)
        {
            using (Datatype mt = Datatype.VlenValueType(typeof(T)))
            {
                long[] dim;
                if (ms != Dataspace.All)
                    dim = ms.GetDimensions();
                else if (fs != Dataspace.All)
                    dim = fs.GetDimensions();
                else using (Dataspace sp = Space)
                    dim = sp.GetDimensions();
                int rank = dim.Length;
                // pin down all data arrays
                Array hbuf = Array.CreateInstance(typeof(GCHandle), dim);
                Array vbuf = Array.CreateInstance(typeof(VLen), dim);
                long[] idx = new long[rank];
                while (idx[rank-1] < dim[rank-1])
                {
                    T[] bidx = (T[])buf.GetValue(idx);
                    GCHandle hidx = GCHandle.Alloc(bidx, GCHandleType.Pinned);
                    hbuf.SetValue(hidx, idx);
                    vbuf.SetValue(new VLen(bidx.Length, hidx.AddrOfPinnedObject()), idx);
                    for (int i=0; i<rank; i++)
                    {
                        idx[i]++;
                        if (i < rank-1 && idx[i] == dim[i])
                            idx[i] = 0;
                        else
                            break;
                    }
                }
                GCHandle hvbuf = GCHandle.Alloc(vbuf, GCHandleType.Pinned);
                try {
                    // write data
                    Write(mt, ms, fs, hvbuf.AddrOfPinnedObject());
                } finally {
                    // cleaning up
                    hvbuf.Free();
                    idx = new long[rank];
                    while (idx[rank-1] < dim[rank-1])
                    {
                        GCHandle hidx = (GCHandle)hbuf.GetValue(idx);
                        hidx.Free();
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
}
