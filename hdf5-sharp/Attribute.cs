//  
//  Attribute.cs
//  hdf-sharp
//
//  Created by Markus Uhr on 22/11/2008.
//  Copyright 2008 Markus Uhr. All rights reserved.
//

using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace Hdf5
{
    public abstract class Attribute<T> : Base
    {
        public Attribute(int raw) : base(raw)
        {
        }
        
        public abstract T Read();
        
        protected void Read(Datatype mt, IntPtr buf)
        {
            int err = H5Aread(raw, mt.raw, buf);
            if (err < 0)
                throw new ApplicationException("Error reading attribute value.");
        }
        
        public abstract void Write(T data);
        
        protected void Write(Datatype mt, IntPtr buf)
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
                throw new ApplicationException(String.Format("Unsupported generic type {0}", gt));
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
