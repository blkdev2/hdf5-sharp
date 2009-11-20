// Attribute.cs created with MonoDevelop
// User: uhrm at 14:47 11/22/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace Hdf5
{
    public class Attribute : Base
    {
        public Attribute(int raw) : base(raw)
        {
        }
        
        public T ReadValue<T>() where T : struct
        {
            T[] buf;
            using (Datatype mt = Datatype.FromValueType(typeof(T)))
            {
                buf = new T[1];
                GCHandle hbuf = GCHandle.Alloc(buf, GCHandleType.Pinned);
                try {
                    int err = H5Aread(raw, mt.raw, hbuf.AddrOfPinnedObject());
                    if (err < 0)
                        throw new ApplicationException("Error reading value from attribute.");
                } finally {
                    hbuf.Free();
                }
            }
            return buf[0];
        }
        
        public BitArray ReadBitArray()
        {
            int[] mem;
            using (Datatype mt = Datatype.BitArrayType(32))
            {
                long[] dim;
                using (Dataspace sp = Space)
                {
                    dim = sp.GetDimensions();
                }
                if (dim.Length != 1)
                    throw new NotSupportedException("Multidimensional bitfield attributes are not supported.");
                mem = new int[dim[0]];
                GCHandle hmem = GCHandle.Alloc(mem, GCHandleType.Pinned);
                try {
                    int err = H5Aread(raw, mt.raw, hmem.AddrOfPinnedObject());
                    if (err < 0)
                        throw new ApplicationException("Error reading value from attribute.");
                } finally {
                    hmem.Free();
                }
            }
            return new BitArray(mem);
        }
        
        public string ReadString()
        {
            string result;
            using (Datatype mt = Type)
            {
                if (mt.Class != DatatypeClass.String)
                    throw new InvalidOperationException();
                IntPtr buf = Marshal.AllocHGlobal((int)mt.Size);
                try {
                    int err = H5Aread(raw, mt.raw, buf);
                    if (err < 0)
                        throw new ApplicationException("Error reading string from attribute.");
                    result = Marshal.PtrToStringAuto(buf, (int)mt.Size);
                } finally {
                    Marshal.FreeHGlobal(buf);
                }
            }
            return result;
        }
        
        public void Write<T>(T data) where T : struct
        {
            T[] buf;
            using (Datatype mt = Datatype.FromValueType(typeof(T)))
            {
                buf = new T[1];
                buf[0] = data;
                GCHandle hbuf = GCHandle.Alloc(buf, GCHandleType.Pinned);
                try {
                    H5Awrite(raw, mt.raw, hbuf.AddrOfPinnedObject());
                } finally {
                    hbuf.Free();
                }
            }
        }
        
        public void Write(BitArray data)
        {
            int[] buf;
            buf = new int[(data.Length+31)/32];
            data.CopyTo(buf, 0);
            using (Datatype mt = Datatype.BitArrayType(32))
            {
                GCHandle hbuf = GCHandle.Alloc(buf, GCHandleType.Pinned);
                try {
                    H5Awrite(raw, mt.raw, hbuf.AddrOfPinnedObject());
                } finally {
                    hbuf.Free();
                }
            }
        }
        
        public void Write(string data)
        {
            using (Datatype mt = Type)
            {
                IntPtr buf = Marshal.StringToHGlobalAnsi(data);
                try {
                    H5Awrite(raw, mt.raw, buf);
                } finally {
                    Marshal.FreeHGlobal(buf);
                }
            }
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
        
        public static Attribute Create(Base obj, string name, Datatype dt, Dataspace ds)
        {
            int raw = H5Acreate2(obj.raw, name, dt.raw, ds.raw, 0, 0);
            if (raw < 0)
                throw new ApplicationException("Error creating attribute.");
            return new Attribute(raw);
        }
        
        public static Attribute CreateWithData<T>(Base obj, string name, T data) where T : struct
        {
            Attribute result;
            using (Datatype dt = Datatype.FromValueType(typeof(T)))
            {
                using (Dataspace ds = new Dataspace(new ulong[] {1}))
                {
                    result = Attribute.Create(obj, name, dt, ds);
                    result.Write<T>(data);
                }
            }
            return result;
        }
        
        public static Attribute CreateWithData(Base obj, string name, BitArray data)
        {
            Attribute result;
            using (Datatype dt = Datatype.BitArrayType(32))
            {
                using (Dataspace ds = new Dataspace(new ulong[] {(ulong)((data.Length+31)/32)}))
                {
                    result = Attribute.Create(obj, name, dt, ds);
                    result.Write(data);
                }
            }
            return result;
        }
        
        public static Attribute CreateWithData(Base obj, string name, string data)
        {
            Attribute result;
            using (Datatype dt = Datatype.ConstString)
            {
                dt.Size = data.Length;
                using (Dataspace ds = new Dataspace(new ulong[] {1}))
                {
                    result = Attribute.Create(obj, name, dt, ds);
                    result.Write(data);
                }
            }
            return result;
        }
        
        public static Attribute Open(Base obj, string name)
        {
            int raw = H5Aopen_name(obj.raw, name);
            if (raw < 0)
                throw new ApplicationException("Error opening attribute.");
            return new Attribute(raw);
        }
        
        
        
        // imports
        
        [DllImport("hdf5")]
        private static extern int H5Acreate2(int obj_id, string attr_name, int type_id, int space_id, int acpl_id, int aapl_id);
        
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
