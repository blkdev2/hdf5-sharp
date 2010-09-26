//  
//  ValueTypeDataset.cs
//  hdf-sharp
//
//  Created by Markus Uhr on 23/09/2010.
//  Copyright 2010 Markus Uhr. All rights reserved.
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
