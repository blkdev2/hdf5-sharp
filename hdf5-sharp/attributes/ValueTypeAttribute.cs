//  
//  ValueTypeAttribute.cs
//  hdf-sharp
//
//  Created by Markus Uhr on 27/09/2010.
//  Copyright 2010 Markus Uhr. All rights reserved.
//

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Hdf5
{
    internal class ValueTypeAttribute<T> : Attribute<T> where T : struct
    {
        public ValueTypeAttribute(int raw) : base(raw)
        {
        }
        
        public override T Read()
        {
            T[] buf = new T[1];
            using (Datatype mt = Datatype.FromValueType(typeof(T)))
            {
                GCHandle hbuf = GCHandle.Alloc(buf, GCHandleType.Pinned);
                try {
                Read(mt, hbuf.AddrOfPinnedObject());
                } finally {
                    hbuf.Free();
                }
            }
            return buf[0];
        }
        
        public override void Write(T data)
        {
            T[] buf = new T[] {data};
            using (Datatype mt = Datatype.FromValueType(typeof(T)))
            {
                GCHandle hbuf = GCHandle.Alloc(buf, GCHandleType.Pinned);
                try {
                    Write(mt, hbuf.AddrOfPinnedObject());
                } finally {
                    hbuf.Free();
                }
            }
        }
    }
}
