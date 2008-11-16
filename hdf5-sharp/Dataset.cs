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
        
        public void Read(Datatype t, Dataspace ms, Dataspace fs, Array buf)
        {
            GCHandle hbuf = GCHandle.Alloc(buf, GCHandleType.Pinned);
            int err = H5Dread(raw, t.raw, ms.raw, fs.raw, 0, hbuf.AddrOfPinnedObject());
            hbuf.Free();
            if (err < 0)
                throw new ApplicationException("Error writing data.");
        }
        
        public Array ReadValue<T>() where T : struct
        {
            Type t = typeof(T);
            Datatype dt;
            if (t.IsPrimitive)
                dt = Datatype.Lookup(t);
            else
                dt = Datatype.FromStruct(t);
            Dataspace ds = Space;
            Array result = Array.CreateInstance(typeof(T), ds.GetDimensions());
            GCHandle hres = GCHandle.Alloc(result, GCHandleType.Pinned);
            int err = H5Dread(raw, dt.raw, Dataspace.All.raw, Dataspace.All.raw, 0,
                              hres.AddrOfPinnedObject());
            if (err < 0)
                throw new ApplicationException("Error reading data from dataset.");
            hres.Free();
            return result;
        }
        
        public Array ReadString()
        {
            Dataspace s = Space;
            int rank = s.NumDimensions;
            long[] dim = s.GetDimensions();
            Array mresult = Array.CreateInstance(typeof(IntPtr), dim);
            GCHandle hres = GCHandle.Alloc(mresult, GCHandleType.Pinned);
            int err = H5Dread(raw, Datatype.Lookup(typeof(string)).raw, Dataspace.All.raw,
                              Dataspace.All.raw, 0, hres.AddrOfPinnedObject());
            if (err < 0)
                throw new ApplicationException("Error reading data from dataset.");
            Array result = Array.CreateInstance(typeof(string), dim);
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
            H5Dvlen_reclaim(Datatype.CUSTOM_STRING.raw, s.raw, 0, hres.AddrOfPinnedObject());
            hres.Free();
            return result;
        }
        
        public T[][] ReadVariableLength<T>() where T : struct
        {
            Dataspace s = Space;
            int rank = s.NumDimensions;
            if (rank != 1)
                throw new ApplicationException("Invalid data space.");
            long[] dim = s.GetDimensions();
            long len = dim[0];
            VLen[] buf = new VLen[len];
            GCHandle hbuf = GCHandle.Alloc(buf, GCHandleType.Pinned);
            int err = H5Dread(raw, Datatype.VariableLength<T>().raw, Dataspace.All.raw,
                              Dataspace.All.raw, 0, hbuf.AddrOfPinnedObject());
            if (err < 0)
                throw new ApplicationException("Error reading data from dataset.");
            T[][] result = new T[len][];
            for (int i=0; i<len; i++)
            {
                result[i] = new T[buf[i].Len];
                ulong ptr = (ulong)buf[i].Ptr;
                uint size = (uint)Marshal.SizeOf(typeof(T));
                for (uint j=0; j<buf[i].Len; j++)
                    result[i][j] = (T)Marshal.PtrToStructure((IntPtr)(ptr+j*size), typeof(T));
            }
            H5Dvlen_reclaim( Datatype.VariableLength<T>().raw, s.raw, 0, hbuf.AddrOfPinnedObject());
            hbuf.Free();
            return result;
        }
        
        internal void Write(Datatype t, Dataspace ms, Dataspace fs, IntPtr buf)
        {
            int err = H5Dwrite(raw, t.raw, ms.raw, fs.raw, 0, buf);
            if (err < 0)
                throw new ApplicationException("Error writing data.");
        }
        
        public Dataspace Space
        {
            get { return new Dataspace(H5Dget_space(raw)); }
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
        
        public static Dataset CreateFromData<T>(Location loc, string name, T[] data) where T : struct
        {
            // create data set
            Type t = typeof(T);
            Datatype dt;
            if (t.IsPrimitive)
                dt = Datatype.Lookup(t);
            else
                dt = Datatype.FromStruct(t);
            Dataspace ds = new Dataspace(new ulong[] {(ulong)data.Length});
            Dataset result = Dataset.Create(loc, name, dt, ds);
            // pin down data
            GCHandle hdata = GCHandle.Alloc(data, GCHandleType.Pinned);
            // write data
            result.Write(dt, Dataspace.All, ds, hdata.AddrOfPinnedObject());
            // cleaning up
            hdata.Free();
            return result;
        }
        
        public static Dataset CreateFromData(Location loc, string name, string[] data)
        {
            // create and write data set
            Datatype t = Datatype.Lookup(typeof(string));
            Dataspace s = new Dataspace(new ulong[] {(ulong)data.Length});
            Dataset result = Dataset.Create(loc, name, t, s);
            // marshal strings
            IntPtr[] mdata = new IntPtr[data.Length];
            for (int i=0; i<data.Length; i++)
                mdata[i] = Marshal.StringToHGlobalAnsi(data[i]);
            // pin down data array
            GCHandle hmdata = GCHandle.Alloc(mdata, GCHandleType.Pinned);
            // write data set
            result.Write(t, Dataspace.All, s, hmdata.AddrOfPinnedObject());
            // cleaning up
            hmdata.Free();
            for (int i=0; i<data.Length; i++)
                Marshal.FreeHGlobal(mdata[i]);
            return result;
        }
        
        public static Dataset CreateFromData<T>(Location loc, string name, T[,] data) where T : struct
        {
            // create data set
            Type t = typeof(T);
            Datatype dt;
            if (t.IsPrimitive)
                dt = Datatype.Lookup(t);
            else
                dt = Datatype.FromStruct(t);
            Dataspace ds = new Dataspace(new ulong[] {(ulong)data.GetLength(0),
                                                      (ulong)data.GetLength(1)});
            Dataset result = Dataset.Create(loc, name, dt, ds);
            // pin down data array
            GCHandle hdata = GCHandle.Alloc(data, GCHandleType.Pinned);
            // write data set
            result.Write(dt, Dataspace.All, ds, hdata.AddrOfPinnedObject());
            // cleaning up
            hdata.Free();
            return result;
        }
        
        public static Dataset CreateFromData(Location loc, string name, string[,] data)
        {
            // create data set
            Datatype t = Datatype.Lookup(typeof(string));
            Dataspace s = new Dataspace(new ulong[] {(ulong)data.GetLength(0),
                                                     (ulong)data.GetLength(1)});
            Dataset result = Dataset.Create(loc, name, t, s);
            // marshal strings
            IntPtr[,] mdata = new IntPtr[data.GetLength(0),data.GetLength(1)];
            for (int i=0; i<data.GetLength(0); i++)
                for (int j=0; j<data.GetLength(1); j++)
                    mdata[i,j] = Marshal.StringToHGlobalAnsi(data[i,j]);
            // pin down data array
            GCHandle hmdata = GCHandle.Alloc(mdata, GCHandleType.Pinned);
            // write data set
            result.Write(t, Dataspace.All, s, hmdata.AddrOfPinnedObject());
            // cleaning up
            hmdata.Free();
            for (int i=0; i<data.GetLength(0); i++)
                for (int j=0; j<data.GetLength(1); j++)
                    Marshal.FreeHGlobal(mdata[i,j]);
            return result;
        }
        
        public static Dataset CreateFromData<T>(Location loc, string name, T[][] data) where T : struct
        {
            int len = data.Length;
            // create data set
            Datatype dt = Datatype.VariableLength<T>();
            Dataspace ds = new Dataspace(new ulong[] {(ulong)len});
            Dataset result = Dataset.Create(loc, name, dt, ds);
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
            result.Write(dt, Dataspace.All, ds, hbuf.AddrOfPinnedObject());
            // cleaning up
            hbuf.Free();
            for (int i=0; i<len; i++)
                hdata[i].Free();
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
        private static extern int H5Dread(int dataset_id, int mem_type_id, int mem_space_id, int file_space_id, int xfer_plist_id, IntPtr buf);
        
        [DllImport("hdf5")]
        private static extern int H5Dwrite(int dataset_id, int mem_type_id, int mem_space_id, int file_space_id, int xfer_plist_id, IntPtr buf);
        
        [DllImport("hdf5")]
        private static extern int H5Dvlen_get_buf_size(int dataset_id, int type_id, int space_id, out ulong size);
        
        [DllImport("hdf5")]
        private static extern int H5Dvlen_reclaim(int type_id, int space_id, int plist_id, IntPtr buf);
    }
}
