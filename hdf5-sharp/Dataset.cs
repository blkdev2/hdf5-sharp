// Dataset.cs created with MonoDevelop
// User: uhrm at 17:12 11/12/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.Runtime.InteropServices;

namespace Hdf5
{
    public class Dataset : Base
    {
        private struct VLen
        {
            private IntPtr len;
            private IntPtr ptr;
            public VLen(long l, IntPtr p) { len = (IntPtr)l; ptr = p; }
            public long Len { get { return (long)len; }}
            public IntPtr Ptr { get { return ptr; }}
        }
        
        internal Dataset(int raw) : base(raw)
        {
        }
        
        public void Close()
        {
            Dispose();
        }
        
//        public void Read(Datatype t, Dataspace ms, Dataspace fs, Array buf)
//        {
//            GCHandle hbuf = GCHandle.Alloc(buf, GCHandleType.Pinned);
//            int err = H5Dread(raw, t.raw, ms.raw, fs.raw, 0, hbuf.AddrOfPinnedObject());
//            hbuf.Free();
//            if (err < 0)
//                throw new ApplicationException("Error writing data.");
//        }
        
        public T ReadValue<T>() where T : struct
        {
            // memory data type
            Type t = typeof(T);
            Datatype mt;
            if (t.IsPrimitive)
                mt = Datatype.Lookup(t);
            else
                mt = Datatype.FromStruct(t);
            // memory data space
            Dataspace ms = new Dataspace(new ulong[] {1});
            // result
            T[] result = new T[1];
            // pin and read
            GCHandle hres = GCHandle.Alloc(result, GCHandleType.Pinned);
            Read(mt, ms, Dataspace.All, hres.AddrOfPinnedObject());
            // cleaning up
            hres.Free();
            ms.Close();
            mt.Close();
            return result[0];
        }
        
        public Array ReadValueArray<T>(Dataspace fs) where T : struct
        {
            if (fs != Dataspace.All && !fs.IsSimple)
                throw new ArgumentException("Dataspace is not simple.");
            // memory data type
            Type t = typeof(T);
            Datatype mt;
            if (t.IsPrimitive)
                mt = Datatype.Lookup(t);
            else
                mt = Datatype.FromStruct(t);
            // create result array
            Array result;
            if (fs == Dataspace.All)
                result = Array.CreateInstance(typeof(T), Space.GetDimensions());
            else
                result = Array.CreateInstance(typeof(T), fs.GetDimensions());
            // pin and read
            GCHandle hres = GCHandle.Alloc(result, GCHandleType.Pinned);
            Read(mt, Dataspace.All, fs, hres.AddrOfPinnedObject());
            // cleaning up
            hres.Free();
            mt.Close();
            return result;
        }
        
        public Array ReadValueArray<T>() where T : struct
        {
            return ReadValueArray<T>(Dataspace.All);
        }
        
        public string ReadString()
        {
            if (Type.Class != DatatypeClass.String)
                throw new InvalidOperationException();
            string result = null;
            // memory data space
            using (Dataspace ms = Space)
            {
                long[] dim = ms.GetDimensions();
                if (dim.Length != 1 || dim[0] != 1)
                    throw new InvalidOperationException();
                // memory data type
                using (Datatype mt = Type)
                {
                    // create marshalled result array
                    IntPtr mresult = Marshal.AllocHGlobal((int)(mt.Size+1));
                    try
                    {
                        // read
                        Read(mt, Dataspace.All, Dataspace.All, mresult);
                        // marshal result string
                        result = Marshal.PtrToStringAnsi(mresult);
                    }
                    finally
                    {
                        // cleaning up
                        Marshal.FreeHGlobal(mresult);
                    }
                }
            }
            return result;
        }
        
        public Array ReadStringArray()
        {
            // memory data type
            Datatype mt = Datatype.Lookup(typeof(string));
            // memory data space
            Dataspace ms = Space;
            int rank = ms.NumDimensions;
            long[] dim = ms.GetDimensions();
            // create marshalled result array
            Array mresult = Array.CreateInstance(typeof(IntPtr), dim);
            // pin and read
            GCHandle hres = GCHandle.Alloc(mresult, GCHandleType.Pinned);
            Read(mt, Dataspace.All, Dataspace.All, hres.AddrOfPinnedObject());
            // create actual result array
            Array result = Array.CreateInstance(typeof(string), dim);
            // marshal strings
            long[] idx = new long[rank];
            while (idx[rank-1] < dim[rank-1])
            {
                result.SetValue(Marshal.PtrToStringAnsi((IntPtr)mresult.GetValue(idx)), idx);
                for (int i=0; i<rank; i++)
                {
                    idx[i]++;
                    if (i < rank-1 && idx[i] == dim[i])
                        idx[i] = 0;
                    else
                        break;
                }
            }
            // cleaning up
            H5Dvlen_reclaim(mt.raw, ms.raw, 0, hres.AddrOfPinnedObject());
            hres.Free();
            ms.Close();
            mt.Close();
            return result;
        }
        
        public T[][] ReadVariableLength<T>() where T : struct
        {
            // memory data type
            Datatype mt = Datatype.VariableLength<T>();
            // memory data space
            Dataspace ms = Space;
            int rank = ms.NumDimensions;
            if (rank != 1)
                throw new ApplicationException("Invalid data space.");
            long[] dim = ms.GetDimensions();
            long len = dim[0];
            // pin and read
            VLen[] buf = new VLen[len];
            GCHandle hbuf = GCHandle.Alloc(buf, GCHandleType.Pinned);
            Read(mt, Dataspace.All, Dataspace.All, hbuf.AddrOfPinnedObject());
            // alloc actual result array
            T[][] result = new T[len][];
            // marshal values
            for (int i=0; i<len; i++)
            {
                result[i] = new T[buf[i].Len];
                ulong ptr = (ulong)buf[i].Ptr;
                uint size = (uint)Marshal.SizeOf(typeof(T));
                for (uint j=0; j<buf[i].Len; j++)
                    result[i][j] = (T)Marshal.PtrToStructure((IntPtr)(ptr+j*size), typeof(T));
            }
            // cleaning up
            H5Dvlen_reclaim(mt.raw, ms.raw, 0, hbuf.AddrOfPinnedObject());
            hbuf.Free();
            ms.Close();
            mt.Close();
            return result;
        }
        
        internal void Read(Datatype mt, Dataspace ms, Dataspace fs, IntPtr buf)
        {
            int err = H5Dread(raw, mt.raw, ms.raw, fs.raw, 0, buf);
            if (err < 0)
                throw new ApplicationException("Error reading data.");
        }
        
        public void Write<T>(T data) where T : struct
        {
            // memory data type
            Type t = typeof(T);
            Datatype mt;
            if (t.IsPrimitive)
                mt = Datatype.Lookup(t);
            else
                mt = Datatype.FromStruct(t);
            // pin down data
            GCHandle hdata = GCHandle.Alloc(data, GCHandleType.Pinned);
            // write data
            Write(mt, Dataspace.All, Dataspace.All, hdata.AddrOfPinnedObject());
            // cleaning up
            hdata.Free();
            mt.Close();
        }
        
        public void Write<T>(Dataspace ms, Dataspace fs, T[] data) where T : struct
        {
            // memory data type
            Type t = typeof(T);
            Datatype mt;
            if (t.IsPrimitive)
                mt = Datatype.Lookup(t);
            else
                mt = Datatype.FromStruct(t);
            // pin down data
            GCHandle hdata = GCHandle.Alloc(data, GCHandleType.Pinned);
            // write data
            Write(mt, ms, fs, hdata.AddrOfPinnedObject());
            // cleaning up
            hdata.Free();
            mt.Close();
        }
        
        public void Write<T>(Dataspace fs, T[] data) where T : struct
        {
            // write data
            Write(Dataspace.All, fs, data);
        }
        
        public void Write(string data)
        {
            IntPtr mdata = Marshal.StringToHGlobalAnsi(data);
            try {
                using (Datatype mt = Type) { Write(mt, Dataspace.All, Dataspace.All, mdata); }
            } finally {
                Marshal.FreeHGlobal(mdata);
            }
        }
        
        public void Write(string[] data)
        {
            // memory data type
            Datatype mt = Datatype.Lookup(typeof(string));
            // marshal strings
            IntPtr[] mdata = new IntPtr[data.Length];
            for (int i=0; i<data.Length; i++)
                mdata[i] = Marshal.StringToHGlobalAnsi(data[i]);
            // pin down data array
            GCHandle hmdata = GCHandle.Alloc(mdata, GCHandleType.Pinned);
            // write data
            Write(mt, Dataspace.All, Dataspace.All, hmdata.AddrOfPinnedObject());
            // cleaning up
            hmdata.Free();
            for (int i=0; i<data.Length; i++)
                Marshal.FreeHGlobal(mdata[i]);
            mt.Close();
        }
        
        public void Write<T>(Dataspace ms, Dataspace ds, T[,] data) where T : struct
        {
            // memory data type
            Type t = typeof(T);
            Datatype mt;
            if (t.IsPrimitive)
                mt = Datatype.Lookup(t);
            else
                mt = Datatype.FromStruct(t);
            // pin down data array
            GCHandle hdata = GCHandle.Alloc(data, GCHandleType.Pinned);
            // write data
            Write(mt, ms, ds, hdata.AddrOfPinnedObject());
            // cleaning up
            hdata.Free();
            mt.Close();
        }
        
        public void Write<T>(Dataspace ds, T[,] data) where T : struct
        {
            // write data
            Write<T>(Dataspace.All, ds, data);
        }
        
        public void Write(Dataspace ms, Dataspace fs, string[,] data)
        {
            // memory data type
            Datatype mt = Datatype.Lookup(typeof(string));
            // marshal strings
            IntPtr[,] mdata = new IntPtr[data.GetLength(0),data.GetLength(1)];
            for (int i=0; i<data.GetLength(0); i++)
                for (int j=0; j<data.GetLength(1); j++)
                    mdata[i,j] = Marshal.StringToHGlobalAnsi(data[i,j]);
            // pin down data array
            GCHandle hmdata = GCHandle.Alloc(mdata, GCHandleType.Pinned);
            // write data
            Write(mt, ms, fs, hmdata.AddrOfPinnedObject());
            // cleaning up
            hmdata.Free();
            for (int i=0; i<data.GetLength(0); i++)
                for (int j=0; j<data.GetLength(1); j++)
                    Marshal.FreeHGlobal(mdata[i,j]);
            mt.Close();
        }
        
        public void Write(Dataspace fs, string[,] data)
        {
            // write data
            Write(Dataspace.All, fs, data);
        }
        
        public void Write<T>(Dataspace ms, Dataspace fs, T[][] data) where T : struct
        {
            int len = data.Length;
            // memory data type
            Datatype mt = Datatype.VariableLength<T>();
            // pin down all data arrays
            GCHandle[] hdata = new GCHandle[len];
            VLen[] buf = new VLen[len];
            for (int i=0; i<len; i++)
            {
                hdata[i] = GCHandle.Alloc(data[i], GCHandleType.Pinned);
                buf[i] = new VLen(data[i].Length, hdata[i].AddrOfPinnedObject());
            }
            GCHandle hbuf = GCHandle.Alloc(buf, GCHandleType.Pinned);
            // write data
            Write(mt, ms, fs, hbuf.AddrOfPinnedObject());
            // cleaning up
            hbuf.Free();
            for (int i=0; i<len; i++)
                hdata[i].Free();
            mt.Close();
        }
        
        public void Write<T>(Dataspace fs, T[][] data) where T : struct
        {
            Write(Dataspace.All, fs, data);
        }
        
        internal void Write(Datatype mt, Dataspace ms, Dataspace fs, IntPtr buf)
        {
            int err = H5Dwrite(raw, mt.raw, ms.raw, fs.raw, 0, buf);
            if (err < 0)
                throw new ApplicationException("Error writing data.");
        }
        
        public Dataspace Space
        {
            get
            {
                int id = H5Dget_space(raw);
                if (id < 0)
                    throw new ApplicationException("Error getting space of dataset.");
                return new Dataspace(id);
            }
        }
        
        public Datatype Type
        {
            get
            {
                int id = H5Dget_type(raw);
                if (id < 0)
                    throw new ApplicationException("Error getting type of dataset.");
                return new Datatype(id, true);
            }
        }
        
        public SpaceStatus SpaceStatus
        {
            get
            {
                SpaceStatus result;
                int err = H5Dget_space_status(raw, out result);
                if (err < 0)
                    throw new ApplicationException();
                return result;
            }
        }
        
        // IDisposable stuff
        
        protected override void Dispose (bool disposing)
        {
            H5Dclose(raw);
            base.Dispose(disposing);
        }
        
        // static creation methods

        public static Dataset Create(Location loc, string name, Datatype type, Dataspace space)
        {
            return new Dataset(H5Dcreate(loc.raw, name, type.raw, space.raw, 0));
        }
        
        public static Dataset CreateWithData<T>(Location loc, string name, T data) where T : struct
        {
            // data type
            Type t = typeof(T);
            Datatype dt;
            if (t.IsPrimitive)
                dt = Datatype.Lookup(t);
            else
                dt = Datatype.FromStruct(t);
            // data space
            Dataspace ds = new Dataspace(new ulong[] {1});
            // data set
            Dataset result = Dataset.Create(loc, name, dt, ds);
            // write data
            result.Write<T>(data);
            // cleaning up
            ds.Close();
            dt.Close();
            return result;
        }
        
        public static Dataset CreateWithData<T>(Location loc, string name, T[] data) where T : struct
        {
            // data type
            Type t = typeof(T);
            Datatype dt;
            if (t.IsPrimitive)
                dt = Datatype.Lookup(t);
            else
                dt = Datatype.FromStruct(t);
            // data space
            Dataspace ds = new Dataspace(new ulong[] {(ulong)data.Length});
            // data set
            Dataset result = Dataset.Create(loc, name, dt, ds);
            // write data
            result.Write<T>(Dataspace.All, data);
            // cleaning up
            ds.Close();
            dt.Close();
            return result;
        }
        
        public static Dataset CreateWithData(Location loc, string name, string data)
        {
            // data type
            Datatype dt = Datatype.C_S1.Copy();
            dt.Size = data.Length;
            // data space
            Dataspace ds = new Dataspace(new ulong[] {1});
            // data set
            Dataset result = Dataset.Create(loc, name, dt, ds);
            // write data
            result.Write(data);
            // cleaning up
            ds.Close();
            dt.Close();
            return result;
        }
        
        public static Dataset CreateWithData(Location loc, string name, string[] data)
        {
            // data type
            Datatype dt = Datatype.Lookup(typeof(string));
            // data space
            Dataspace ds = new Dataspace(new ulong[] {(ulong)data.Length});
            // data set
            Dataset result = Dataset.Create(loc, name, dt, ds);
            // write data
            result.Write(data);
            // cleaning up
            ds.Close();
            dt.Close();
            return result;
        }
        
        public static Dataset CreateWithData<T>(Location loc, string name, T[,] data) where T : struct
        {
            // data type
            Type t = typeof(T);
            Datatype dt;
            if (t.IsPrimitive)
                dt = Datatype.Lookup(t);
            else
                dt = Datatype.FromStruct(t);
            // data space
            Dataspace ds = new Dataspace(new ulong[] {(ulong)data.GetLength(0),
                                                      (ulong)data.GetLength(1)});
            // data set
            Dataset result = Dataset.Create(loc, name, dt, ds);
            // write data
            result.Write(Dataspace.All, data);
            // cleaning up
            ds.Close();
            dt.Close();
            return result;
        }
        
        public static Dataset CreateWithData(Location loc, string name, string[,] data)
        {
            // data type
            Datatype dt = Datatype.Lookup(typeof(string));
            // data space
            Dataspace ds = new Dataspace(new ulong[] {(ulong)data.GetLength(0),
                                                      (ulong)data.GetLength(1)});
            // data set
            Dataset result = Dataset.Create(loc, name, dt, ds);
            // write data
            result.Write(Dataspace.All, data);
            // cleaning up
            dt.Close();
            ds.Close();
            return result;
        }
        
        public static Dataset CreateWithData<T>(Location loc, string name, T[][] data) where T : struct
        {
            int len = data.Length;
            // data type
            Datatype dt = Datatype.VariableLength<T>();
            // data space
            Dataspace ds = new Dataspace(new ulong[] {(ulong)len});
            // data set
            Dataset result = Dataset.Create(loc, name, dt, ds);
            // write data
            result.Write(Dataspace.All, data);
            // cleaning up
            ds.Close();
            dt.Close();
            return result;
        }
        
        public static Dataset Open(Location loc, string name)
        {
            return new Dataset(H5Dopen(loc.raw, name));
        }
        
        // imports
        
        [DllImport("hdf5")]
        private static extern int H5Dcreate(int loc_id, string name, int type_id, int space_id, int dcpl_id);
        
        [DllImport("hdf5")]
        private static extern int H5Dopen(int loc_id, string name);
        
        [DllImport("hdf5")]
        private static extern int H5Dclose(int dataset_id);
        
        [DllImport("hdf5")]
        private static extern int H5Dget_space(int dataset_id);
        
        [DllImport("hdf5")]
        private static extern int H5Dget_space_status(int dataset_id, out SpaceStatus status);
        
        [DllImport("hdf5")]
        private static extern int H5Dget_type(int dataset_id);
        
        [DllImport("hdf5")]
        private static extern int H5Dread(int dataset_id, int mem_type_id, int mem_space_id, int file_space_id, int xfer_plist_id, IntPtr buf);
        
        [DllImport("hdf5")]
        private static extern int H5Dwrite(int dataset_id, int mem_type_id, int mem_space_id, int file_space_id, int xfer_plist_id, IntPtr buf);
        
        [DllImport("hdf5")]
        private static extern int H5Dvlen_get_buf_size(int dataset_id, int type_id, int space_id, out ulong size);
        
        [DllImport("hdf5")]
        private static extern int H5Dvlen_reclaim(int type_id, int space_id, int plist_id, IntPtr buf);
    }
}
