//  
//  Dataset.cs
//  hdf-sharp
//
//  Created by Markus Uhr on 12/11/2008.
//  Copyright 2008 Markus Uhr. All rights reserved.
//

using System;
using System.Collections;
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
            T[] result = new T[1];
            using (Datatype mt = Datatype.FromValueType(typeof(T)))
            {
                using (Dataspace ms = new Dataspace(new ulong[] {1}))
                {
                    // pin and read
                    GCHandle hres = GCHandle.Alloc(result, GCHandleType.Pinned);
                    try {
                        Read(mt, ms, Dataspace.All, hres.AddrOfPinnedObject());
                    } finally {
                        hres.Free();
                    }
                }
            }
            return result[0];
        }
        
        public void ReadValueArray<T>(Dataspace ms, Dataspace fs, Array buf) where T : struct
        {
            using (Datatype mt = Datatype.FromValueType(typeof(T)))
            {
                GCHandle hbuf = GCHandle.Alloc(buf, GCHandleType.Pinned);
                try {
                    Read(mt, ms, fs, hbuf.AddrOfPinnedObject());
                } finally {
                    hbuf.Free();
                }
            }
        }
        
        public Array ReadValueArray<T>() where T : struct
        {
            Array result;
            using (Dataspace ds = Space)
                result = Array.CreateInstance(typeof(T), ds.GetDimensions());
            ReadValueArray<T>(Dataspace.All, Dataspace.All, result);
            return result;
        }
        
        public void ReadBitArray(Dataspace fs, BitArray buf)
        {
            int[] mem = new int[(buf.Length+31)/32];
            using (Datatype mt = Datatype.NativeBitArray32)
            {
                GCHandle hmem = GCHandle.Alloc(mem, GCHandleType.Pinned);
                try {
                    Read(mt, Dataspace.All, fs, hmem.AddrOfPinnedObject());
                } finally {
                    hmem.Free();
                }
            }
            // TODO: speed up copy
            BitArray tmp = new BitArray(mem);
            for (int i=0; i<tmp.Length; i++)
                buf[i] = tmp[i];
        }
        
        public BitArray ReadBitArray()
        {
            long[] dim;
            using (Dataspace sp = Space)
            {
                dim = sp.GetDimensions();
                if (dim.Length != 1)
                    throw new ApplicationException("Unsupported data format.");
            }
            using (Datatype dt = Type)
            {
                if (dt.Class != DatatypeClass.Bitfield)
                    throw new InvalidOperationException("Underlying datatype is not a bitfield.");
                if (dt.Size != 4)
                    throw new NotImplementedException();
            }
            BitArray result = new BitArray((int)(32*dim[0]));
            ReadBitArray(Dataspace.All, result);
            return result;
        }
        
        public string ReadString()
        {
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
                    if (mt.Class != DatatypeClass.String)
                        throw new InvalidOperationException();
                    // create marshalled result array
                    IntPtr mresult = Marshal.AllocHGlobal((int)mt.Size);
                    try
                    {
                        // read
                        Read(mt, Dataspace.All, Dataspace.All, mresult);
                        // marshal result string
                        result = Marshal.PtrToStringAnsi(mresult, (int)mt.Size);
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
            Datatype mt = Datatype.VlenString;
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
        
        public string[][] ReadStringVlenArray()
        {
            // memory data type
            Datatype mt = Datatype.VariableLengthString();
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
            string[][] result = new string[len][];
            // marshal values
            for (int i=0; i<len; i++)
            {
                if (buf[i].Len > 0)
                {
                IntPtr[] sptr = new IntPtr[buf[i].Len];
                Marshal.Copy(buf[i].Ptr, sptr, 0, (int)buf[i].Len);
                result[i] = new string[buf[i].Len];
                for (int j=0; j<buf[i].Len; j++)
                    result[i][j] = Marshal.PtrToStringAnsi(sptr[j]);
                }
            }
            // cleaning up
            H5Dvlen_reclaim(mt.raw, ms.raw, 0, hbuf.AddrOfPinnedObject());
            hbuf.Free();
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
            using (Datatype mt = Datatype.FromValueType(typeof(T)))
            {
                GCHandle hdata = GCHandle.Alloc(data, GCHandleType.Pinned);
                try {
                    Write(mt, Dataspace.All, Dataspace.All, hdata.AddrOfPinnedObject());
                } finally {
                    hdata.Free();
                }
            }
        }
        
        public void Write<T>(Dataspace ms, Dataspace fs, T[] data) where T : struct
        {
            using (Datatype mt = Datatype.FromValueType(typeof(T)))
            {
                GCHandle hdata = GCHandle.Alloc(data, GCHandleType.Pinned);
                try {
                    Write(mt, ms, fs, hdata.AddrOfPinnedObject());
                } finally {
                    hdata.Free();
                }
            }
        }
        
        public void Write<T>(Dataspace fs, T[] data) where T : struct
        {
            Write(Dataspace.All, fs, data);
        }
        
        public void Write(Dataspace ms, Dataspace fs, BitArray data)
        {
            using (Datatype mt = Datatype.NativeBitArray32)
            {
                int[] mem = new int[(data.Length + 31)/32];
                data.CopyTo(mem, 0);
                GCHandle hmem = GCHandle.Alloc(mem, GCHandleType.Pinned);
                try {
                    Write(mt, ms, fs, hmem.AddrOfPinnedObject());
                } finally {
                    hmem.Free();
                }
            }
        }
        
        public void Write(Dataspace fs, BitArray data)
        {
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
            Datatype mt = Datatype.VlenString;
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
            using (Datatype mt = Datatype.FromValueType(typeof(T)))
            {
                GCHandle hdata = GCHandle.Alloc(data, GCHandleType.Pinned);
                try {
                    Write(mt, ms, ds, hdata.AddrOfPinnedObject());
                } finally {
                    hdata.Free();
                }
            }
        }
        
        public void Write<T>(Dataspace ds, T[,] data) where T : struct
        {
            // write data
            Write<T>(Dataspace.All, ds, data);
        }
        
        public void Write(Dataspace ms, Dataspace fs, string[,] data)
        {
            // memory data type
            Datatype mt = Datatype.VlenString;
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
        
        public void Write(Dataspace ms, Dataspace fs, string[][] data)
        {
            int len = data.Length;
            // memory data type
            Datatype mt = Datatype.VariableLengthString();
            // marshal strings
            IntPtr[][] mdata = new IntPtr[len][];
            for (int i=0; i<len; i++)
            {
                if (data[i] != null)
                {
                int l = data[i].Length;
                mdata[i] = new IntPtr[l];
                for (int j=0; j<l; j++)
                    mdata[i][j] = Marshal.StringToHGlobalAnsi(data[i][j]);
                }
                else
                {
                    mdata[i] = null;
                }
            }
            // pin down all data arrays
            GCHandle[] hmdata = new GCHandle[len];
            VLen[] buf = new VLen[len];
            for (int i=0; i<len; i++)
            {
                if (mdata[i] != null)
                {
                    hmdata[i] = GCHandle.Alloc(mdata[i], GCHandleType.Pinned);
                    buf[i] = new VLen(data[i].Length, hmdata[i].AddrOfPinnedObject());
                }
                else
                {
//                    hmdata[i] = null;
                    buf[i] = new VLen(0, IntPtr.Zero);
                }
            }
            // write data
            GCHandle hbuf = GCHandle.Alloc(buf, GCHandleType.Pinned);
            try {
                Write(mt, ms, fs, hbuf.AddrOfPinnedObject());
            } finally {
                hbuf.Free();
            }
            // cleaning up
            for (int i=0; i<len; i++)
                if (hmdata[i].IsAllocated)
                    hmdata[i].Free();
            // clean up string copies
            for (int i=0; i<len; i++)
            {
                if (data[i] != null)
                {
                int l = data[i].Length;
                for (int j=0; j<l; j++)
                    Marshal.FreeHGlobal(mdata[i][j]);
                }
            }
            mt.Close();
        }
        
        public void Write(Dataspace fs, string[][] data)
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
            return new Dataset(H5Dcreate2(loc.raw, name, type.raw, space.raw, 0, 0, 0));
        }
        
        public static Dataset CreateWithData<T>(Location loc, string name, ByteOrder order, T data) where T : struct
        {
            Dataset result = null;
            using (Datatype dt = Datatype.FromValueType(typeof(T), order))
            {
                using (Dataspace ds = new Dataspace(new ulong[] {1}))
                {
                    result = Dataset.Create(loc, name, dt, ds);
                    result.Write<T>(data);
                }
            }
            return result;
        }
        
        public static Dataset CreateWithData<T>(Location loc, string name, T data) where T : struct
        {
            return Dataset.CreateWithData<T>(loc, name, ByteOrder.Native, data);
        }
        
        public static Dataset CreateWithData(Location loc, string name, ByteOrder order, BitArray data)
        {
            Dataset result = null;
            using (Datatype dt = Datatype.BitArrayType(32, order))
            {
                using (Dataspace ds = new Dataspace(new ulong[] {(ulong)(data.Length+31)/32}))
                {
                    result = Dataset.Create(loc, name, dt, ds);
                    result.Write(ds, data);
                }
            }
            return result;
        }
        
        public static Dataset CreateWithData(Location loc, string name, BitArray data)
        {
            return Dataset.CreateWithData(loc, name, ByteOrder.LittleEndian, data);
        }
        
        public static Dataset CreateWithData<T>(Location loc, string name, ByteOrder order, T[] data) where T : struct
        {
            Dataset result = null;
            using (Datatype dt = Datatype.FromValueType(typeof(T), order))
            {
                using (Dataspace ds = new Dataspace(new ulong[] {(ulong)data.Length}))
                {
                    result = Dataset.Create(loc, name, dt, ds);
                    result.Write<T>(Dataspace.All, data);
                }
            }
            return result;
        }
        
        public static Dataset CreateWithData<T>(Location loc, string name, T[] data) where T : struct
        {
            return Dataset.CreateWithData<T>(loc, name, ByteOrder.Native, data);
        }
        
        public static Dataset CreateWithData(Location loc, string name, string data)
        {
            Dataset result = null;
            using (Datatype dt = Datatype.ConstString)
            {
                dt.Size = data.Length;
                using (Dataspace ds = new Dataspace(new ulong[] {1}))
                {
                    result = Dataset.Create(loc, name, dt, ds);
                    result.Write(data);
                }
            }
            return result;
        }
        
        public static Dataset CreateWithData(Location loc, string name, string[] data)
        {
            Dataset result = null;
            using (Datatype dt = Datatype.VlenString)
            {
                using (Dataspace ds = new Dataspace(new ulong[] {(ulong)data.Length}))
                {
                    result = Dataset.Create(loc, name, dt, ds);
                    result.Write(data);
                }
            }
            return result;
        }
        
        public static Dataset CreateWithData<T>(Location loc, string name, ByteOrder order, T[,] data) where T : struct
        {
            Dataset result = null;
            using (Datatype dt = Datatype.FromValueType(typeof(T), order))
            {
                using (Dataspace ds = new Dataspace(new ulong[] {(ulong)data.GetLength(0),
                                                                 (ulong)data.GetLength(1)}))
                {
                    result = Dataset.Create(loc, name, dt, ds);
                    result.Write(Dataspace.All, data);
                }
            }
            return result;
        }
        
        public static Dataset CreateWithData<T>(Location loc, string name, T[,] data) where T : struct
        {
            return Dataset.CreateWithData(loc, name, ByteOrder.Native, data);
        }
        
        public static Dataset CreateWithData(Location loc, string name, string[,] data)
        {
            Dataset result = null;
            using (Datatype dt = Datatype.VlenString)
            {
                using (Dataspace ds = new Dataspace(new ulong[] {(ulong)data.GetLength(0),
                                                                 (ulong)data.GetLength(1)}))
                {
                    result = Dataset.Create(loc, name, dt, ds);
                    result.Write(Dataspace.All, data);
                }
            }
            return result;
        }
        
        public static Dataset CreateWithData<T>(Location loc, string name, ByteOrder order, T[][] data) where T : struct
        {
            int len = data.Length;
            Dataset result = null;
            using (Datatype dt = Datatype.VariableLength<T>())
            {
                using (Dataspace ds = new Dataspace(new ulong[] {(ulong)len}))
                {
                    result = Dataset.Create(loc, name, dt, ds);
                    result.Write(Dataspace.All, data);
                }
            }
            return result;
        }
        
        public static Dataset CreateWithData<T>(Location loc, string name, T[][] data) where T : struct
        {
            return Dataset.CreateWithData(loc, name, ByteOrder.Native, data);
        }
        
        public static Dataset CreateWithData(Location loc, string name, string[][] data)
        {
            int len = data.Length;
            Dataset result = null;
            using (Datatype dt = Datatype.VariableLengthString())
            {
                using (Dataspace ds = new Dataspace(new ulong[] {(ulong)len}))
                {
                    result = Dataset.Create(loc, name, dt, ds);
                    result.Write(Dataspace.All, data);
                }
            }
            return result;
        }
        
        public static Dataset Open(Location loc, string name)
        {
            return new Dataset(H5Dopen2(loc.raw, name, 0));
        }
        
        // imports
        
        [DllImport("hdf5")]
        private static extern int H5Dcreate2(int loc_id, string name, int type_id, int space_id, int lcpl_id, int dcpl_id, int dapl_id);
        
        [DllImport("hdf5")]
        private static extern int H5Dopen2(int loc_id, string name, int dapl_id);
        
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
