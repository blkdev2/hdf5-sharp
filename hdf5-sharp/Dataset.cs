//  
// Dataset.cs
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
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Hdf5
{
    internal struct VLen
    {
        private IntPtr len;
        private IntPtr ptr;
        public VLen(long l, IntPtr p) { len = (IntPtr)l; ptr = p; }
        public long Len { get { return (long)len; } }
        public IntPtr Ptr { get { return ptr; } }
    }
        
    public abstract class Dataset<T> : Base
    {
        internal Dataset(int raw) : base(raw)
        {
        }
        
        public void Close()
        {
            Dispose();
        }
        
        public virtual long[] GetDimensions()
        {
            long[] result;
            using (Dataspace ds = Space)
                result = ds.GetDimensions();
            return result;
        }
        
        public virtual Array Read()
        {
            Array result;
            using (Dataspace ds = Space)
                result = Array.CreateInstance(typeof(T), ds.GetDimensions());
            Read(Dataspace.All, Dataspace.All, result);
            return result;
        }
        
        public abstract void Read(Dataspace ms, Dataspace fs, Array buf);
        
        protected void Read(Datatype mt, Dataspace ms, Dataspace fs, IntPtr buf)
        {
            int err = H5Dread(raw, mt.raw, ms.raw, fs.raw, 0, buf);
            if (err < 0)
                throw new ApplicationException("Error reading data.");
        }
        
        public virtual void Write(Array buf)
        {
            Write(Dataspace.All, buf);
        }
        
        public virtual void Write(Dataspace fs, Array buf)
        {
            Write(Dataspace.All, fs, buf);
        }
        
        public abstract void Write(Dataspace ms, Dataspace fs, Array buf);
        
        protected void Write(Datatype mt, Dataspace ms, Dataspace fs, IntPtr buf)
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
        
        public T this[int i]
        {
            get { return this[(long)i]; }
            set { this[(long)i] = value; }
        }
        
        private long[] idx_tmp = new long[2];
        private long[] idx_one = new long[] {1};
        private long[] idx_oneone = new long[] {1, 1};
        private T[] idx_buf = new T[1];
        
        public T this[long i]
        {
            get
            {
                long[] dim = GetDimensions();
                if (dim.Length != 1)
                    throw new InvalidOperationException(String.Format("Cannot access {0}-dimensional array with one index.", dim.Length));
                if (i < 0 || i >= dim[0])
                    throw new IndexOutOfRangeException("i");
                using (Dataspace ms = new Dataspace(idx_one))
                using (Dataspace fs = new Dataspace(dim))
                {
                    // select i-th element
                    idx_tmp[0] = i;
                    fs.SelectHyperslab(SelectOperation.Set, idx_tmp, null, idx_one, null);
                    // read
                    Read(ms, fs, idx_buf);
                }
                return idx_buf[0];
            }
            set
            {
                long[] dim = GetDimensions();
                if (dim.Length != 1)
                    throw new InvalidOperationException(String.Format("Cannot access {0}-dimensional array with one index.", dim.Length));
                if (i < 0 || i >= dim[0])
                    throw new IndexOutOfRangeException("i");
                using (Dataspace ms = new Dataspace(idx_one))
                using (Dataspace fs = new Dataspace(dim))
                {
                    // select i-th element
                    idx_tmp[0] = i;
                    idx_buf[0] = value;
                    fs.SelectHyperslab(SelectOperation.Set, idx_tmp, null, idx_one, null);
                    // write
                    Write(ms, fs, idx_buf);
                }
            }
        }
        
        public T this[int i, int j]
        {
            get { return this[(long)i, (long)j]; }
            set { this[(long)i, (long)j] = value; }
        }
        
        public T this[long i, long j]
        {
            get
            {
                long[] dim = GetDimensions();
                if (dim.Length != 2)
                    throw new InvalidOperationException(String.Format("Cannot access {0}-dimensional array with one index.", dim.Length));
                if (i < 0 || i >= dim[0])
                    throw new IndexOutOfRangeException("i");
                if (j < 0 || j >= dim[1])
                    throw new IndexOutOfRangeException("j");
                using (Dataspace ms = new Dataspace(idx_oneone))
                using (Dataspace fs = new Dataspace(dim))
                {
                    // select (i,j)-th element
                    idx_tmp[0] = i;
                    idx_tmp[1] = j;
                    fs.SelectHyperslab(SelectOperation.Set, idx_tmp, null, idx_oneone, null);
                    // read
                    Read(ms, fs, idx_buf);
                }
                return idx_buf[0];
            }
            set
            {
                long[] dim = GetDimensions();
                if (dim.Length != 1)
                    throw new InvalidOperationException(String.Format("Cannot access {0}-dimensional array with one index.", dim.Length));
                if (i < 0 || i >= dim[0])
                    throw new IndexOutOfRangeException("i");
                if (j < 0 || j >= dim[1])
                    throw new IndexOutOfRangeException("j");
                using (Dataspace ms = new Dataspace(idx_oneone))
                using (Dataspace fs = new Dataspace(dim))
                {
                    // select (i,j)-th element
                    idx_tmp[0] = i;
                    idx_tmp[1] = j;
                    idx_buf[0] = value;
                    fs.SelectHyperslab(SelectOperation.Set, idx_tmp, null, idx_oneone, null);
                    // write
                    Write(ms, fs, idx_buf);
                }
            }
        }
        
        // IDisposable stuff
        
        protected override void Dispose(bool disposing)
        {
            H5Dclose(raw);
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
            else if (t.IsArray)
            {
                Type te = t.GetElementType();
                if (te.IsValueType)
                    return Datatype.VlenValueType(te, order);
            }
            throw new ArgumentException(String.Format("Unsupported type {0}.", t));
        }
        
//        private static Dictionary<Type,Type> dstype = new Dictionary<Type,Type>()
//        {
//            {typeof(int),    typeof(PrimitiveDataset<>)},
//            {typeof(long),   typeof(PrimitiveDataset<>)},
//            {typeof(double), typeof(PrimitiveDataset<>)}
//        };
        
        private static Dataset<T> CreateInstance(int raw)
        {
            Type gt = typeof(T);
            Type dst = null;
            if (gt.IsValueType)
            {
                dst = typeof(ValueTypeDataset<>).MakeGenericType(new Type[] {gt});
            }
            else if (gt == typeof(string))
            {
                dst = typeof(StringDataset);
            }
            else if (gt.IsArray)
            {
                gt = gt.GetElementType();
                if (gt.IsValueType)
                    dst = typeof(VlenValueTypeDataset<>).MakeGenericType(new Type[] {gt});
            }
            if (dst == null)
                throw new ApplicationException(String.Format("Unsupported generic type {0}", gt));
            object result = Activator.CreateInstance(dst, raw);
            return (Dataset<T>)result;
        }
        
        public static Dataset<T> Create(Location loc, string name, int[] space)
        {
            long[] lspace = new long[space.Length];
            for (int i=0; i<space.Length; i++)
                lspace[i] = (long)space[i];
            return Create(loc, name, lspace);
        }
        
        public static Dataset<T> Create(Location loc, string name, long[] space)
        {
            return Create(loc, name, ByteOrder.Native, space);
        }
        
        public static Dataset<T> Create(Location loc, string name, ByteOrder order, long[] space)
        {
            Dataset<T> result = null;
            using (Datatype dt = GetDatatypeFromType(typeof(T), order))
            using (Dataspace ds = new Dataspace(space))
                result = Create(loc, name, dt, ds);
            return result;
        }
        
        private static Dataset<T> Create(Location loc, string name, Datatype type, Dataspace space)
        {
            int raw = H5Dcreate2(loc.raw, name, type.raw, space.raw, 0, 0, 0);
            return CreateInstance(raw);
        }
        
        public static Dataset<T> CreateWithData(Location loc, string name, Array data)
        {
            return CreateWithData(loc, name, ByteOrder.Native, data);
        }
        
        public static Dataset<T> CreateWithData(Location loc, string name, ByteOrder order, Array data)
        {
            long[] dim = new long[data.Rank];
            for (int i=0; i<data.Rank; i++)
                dim[i] = data.GetLongLength(i);
            Dataset<T> result = null;
            using (Datatype dt = GetDatatypeFromType(typeof(T), order))
            using (Dataspace ds = new Dataspace(dim))
            {
                result = Create(loc, name, dt, ds);
                result.Write(data);
            }
            return result;
        }
        
        public static Dataset<T> Open(Location loc, string name)
        {
            int raw = H5Dopen2(loc.raw, name, 0);
            return CreateInstance(raw);
        }
        
        // imports
        
        [DllImport("hdf5")]
        protected static extern int H5Dcreate2(int loc_id, string name, int type_id, int space_id, int lcpl_id, int dcpl_id, int dapl_id);
        
        [DllImport("hdf5")]
        protected static extern int H5Dopen2(int loc_id, string name, int dapl_id);
        
        [DllImport("hdf5")]
        protected static extern int H5Dclose(int dataset_id);
        
        [DllImport("hdf5")]
        protected static extern int H5Dget_space(int dataset_id);
        
        [DllImport("hdf5")]
        protected static extern int H5Dget_space_status(int dataset_id, out SpaceStatus status);
        
        [DllImport("hdf5")]
        protected static extern int H5Dget_type(int dataset_id);
        
        [DllImport("hdf5")]
        protected static extern int H5Dread(int dataset_id, int mem_type_id, int mem_space_id, int file_space_id, int xfer_plist_id, IntPtr buf);
        
        [DllImport("hdf5")]
        protected static extern int H5Dwrite(int dataset_id, int mem_type_id, int mem_space_id, int file_space_id, int xfer_plist_id, IntPtr buf);
        
        [DllImport("hdf5")]
        protected static extern int H5Dvlen_get_buf_size(int dataset_id, int type_id, int space_id, out ulong size);
        
        [DllImport("hdf5")]
        protected static extern int H5Dvlen_reclaim(int type_id, int space_id, int plist_id, IntPtr buf);
    }
}
