//  
// Attribute.cs
// hdf-sharp
//
// Copyright 2008-2010 Markus Uhr <markus.uhr@bsse.ethz.ch>.
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
using System.Collections;
using System.Runtime.InteropServices;

namespace Hdf5
{
    public abstract class Attribute<T> : Base
    {
        internal Attribute(int raw) : base(raw)
        {
        }
        
        public void Flush()
        {
            H5Fflush(raw, FileScope.Local);
        }
        
        public void Close()
        {
            Dispose();
        }
        
        public abstract T Read();
        
        internal void Read(Datatype mt, IntPtr buf)
        {
            int err = H5Aread(raw, mt.raw, buf);
            if (err < 0)
                throw new ApplicationException("Error reading attribute value.");
        }
        
        public abstract void Write(T data);
        
        protected internal void Write(Datatype mt, IntPtr buf)
        {
            int err = H5Awrite(raw, mt.raw, buf);
            if (err < 0)
                throw new ApplicationException("Error writing attribute value.");
        }
        
        public string Name
        {
            get
            {
                string result;
                long size = H5Aget_name(raw, (IntPtr)0, IntPtr.Zero);
                if (size < 0)
                    throw new ApplicationException("Error determining length of attribute name.");
                IntPtr buf = Marshal.AllocHGlobal((int)(size+1));
                try {
                    int err = H5Aget_name(raw, (IntPtr)(size+1), buf);
                    if (err < 0)
                        throw new ApplicationException("Error getting attribute name.");
                    result = Marshal.PtrToStringAnsi(buf);
                } finally {
                    Marshal.FreeHGlobal(buf);
                }
                return result;
            }
        }
        
        public Dataspace Space
        {
            get
            {
                int ds = H5Aget_space(raw);
                if (ds < 0)
                    throw new ApplicationException("Error getting dataspace of attribute.");
                return new Dataspace(ds);
            }
        }
        
        public Datatype Type
        {
            get
            {
                int dt = H5Aget_type(raw);
                if (dt < 0)
                    throw new ApplicationException("Error determining type of attribute.");
                return new Datatype(dt, true);
            }
        }
        
        // IDisposable stuff
        
        protected override void Dispose(bool disposing)
        {
            H5Aclose(raw);
            base.Dispose(disposing);
        }
        
        // static creation methods

        private static Datatype GetDatatypeFromType(Type t)
        {
            return GetDatatypeFromType(t, ByteOrder.Native);
        }
        
        private static Datatype GetDatatypeFromType(Type t, ByteOrder order)
        {
            if (t.IsValueType)
            {
                return Datatype.FromValueType(t, order);
            }
            else if (t == typeof(string))
            {
                return Datatype.VlenString;
            }
            else
            {
                throw new ArgumentException(String.Format("Unsupported type {0}.", t));
            }
        }
        
        private static Attribute<T> CreateInstance(int raw)
        {
            Type gt = typeof(T);
            Type dst = null;
            if (gt.IsValueType)
            {
                dst = typeof(ValueTypeAttribute<>).MakeGenericType(new Type[] {gt});
            }
            else if (gt == typeof(string))
            {
                dst = typeof(StringAttribute);
            }
            if (dst == null)
                throw new ApplicationException(String.Format("Unsupported generic type {0}.", gt));
            object result = Activator.CreateInstance(dst, raw);
            return (Attribute<T>)result;
        }
        
        public static Attribute<T> Create(Base obj, string name)
        {
            int raw = -1;
            using (Datatype dt = GetDatatypeFromType(typeof(T)))
            using (Dataspace sp = new Dataspace(new long[] {1}))
                raw = H5Acreate2(obj.raw, name, dt.raw, sp.raw, 0, 0);
            if (raw < 0)
                throw new ApplicationException("Error creating attribute.");
            return CreateInstance(raw);
        }
        
        public static Attribute<T> CreateWithData(Base obj, string name, T data)
        {
            Attribute<T> result = Create(obj, name);
            result.Write(data);
            return result;
        }
        
        public static Attribute<T> Open(Base obj, string name)
        {
            int raw = -1;
            using (Datatype dt = GetDatatypeFromType(typeof(T)))
            using (Dataspace sp = new Dataspace(new long[] {1}))
                raw = H5Aopen_name(obj.raw, name);
            if (raw < 0)
                throw new ApplicationException("Error opening attribute.");
            return CreateInstance(raw);
        }
        
        // imports
        
        [DllImport("hdf5")]
        private static extern int H5Fflush(int loc, FileScope scope);

        [DllImport("hdf5")]
        private static extern int H5Acreate2(int obj_id, string attr_name, int type_id, int space_id, int acpl_id, int aapl_id);
        
        [DllImport("hdf5")]
        private static extern int H5Aclose(int obj_id);
        
        [DllImport("hdf5")]
        private static extern int H5Aopen_name(int obj_id, string attr_name);
        
        [DllImport("hdf5")]
        private static extern int H5Aread(int attr_id, int mem_type_id, IntPtr buf);
        
        [DllImport("hdf5")]
        private static extern int H5Awrite(int attr_id, int mem_type_id, IntPtr buf);
        
        [DllImport("hdf5")]
        private static extern int H5Aget_name(int attr_id, IntPtr size, IntPtr buf);
        
        [DllImport("hdf5")]
        private static extern int H5Aget_space(int attr_id);
        
        [DllImport("hdf5")]
        private static extern int H5Aget_type(int attr_id);
    }
}
