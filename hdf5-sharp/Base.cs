//  
// Base.cs
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
using System.Runtime.InteropServices;

namespace Hdf5
{
    public abstract class Base : IDisposable
    {
        internal int raw;
        
        internal Base()
        {
        }
        
        internal Base(int raw)
        {
            if (raw < 0)
                throw new ApplicationException("Invalid ID.");
            this.raw = raw;
        }
        
        ~Base()
        {
            Dispose(false);
        }
        
        // IDisposable stuff
        
        private bool disposed = false;
        
        public void Dispose()
        {
            if (!disposed)
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
        }
        
        protected virtual void Dispose(bool disposing)
        {
            disposed = true;
        }
        
        // HDF5 attribute imports

        [DllImport("libhdf5.so")]
        internal static extern int H5Acreate2(int obj_id, string attr_name, int type_id, int space_id, int acpl_id, int aapl_id);
        
        [DllImport("libhdf5.so")]
        internal static extern int H5Aclose(int obj_id);
        
        [DllImport("libhdf5.so")]
        internal static extern int H5Aopen_name(int obj_id, string attr_name);
        
        [DllImport("libhdf5.so")]
        internal static extern int H5Aread(int attr_id, int mem_type_id, IntPtr buf);
        
        [DllImport("libhdf5.so")]
        internal static extern int H5Awrite(int attr_id, int mem_type_id, IntPtr buf);
        
        internal static long H5Aget_name(int attr_id, long size, IntPtr buf)
        {
            return (long)H5Aget_name(attr_id, (IntPtr)size, buf);
        }
        
        [DllImport("libhdf5.so")]
        internal static extern IntPtr H5Aget_name(int attr_id, IntPtr size, IntPtr buf);
        
        [DllImport("libhdf5.so")]
        internal static extern int H5Aget_space(int attr_id);
        
        [DllImport("libhdf5.so")]
        internal static extern int H5Aget_type(int attr_id);
        
        // HDF5 dataset imports
        
        [DllImport("libhdf5.so")]
        internal static extern int H5Dcreate2(int loc_id, string name, int type_id, int space_id, int lcpl_id, int dcpl_id, int dapl_id);
        
        [DllImport("libhdf5.so")]
        internal static extern int H5Dopen2(int loc_id, string name, int dapl_id);
        
        [DllImport("libhdf5.so")]
        internal static extern int H5Dclose(int dataset_id);
        
        [DllImport("libhdf5.so")]
        internal static extern int H5Dget_space(int dataset_id);
        
        [DllImport("libhdf5.so")]
        internal static extern int H5Dget_space_status(int dataset_id, out SpaceStatus status);
        
        [DllImport("libhdf5.so")]
        internal static extern int H5Dget_type(int dataset_id);
        
        [DllImport("libhdf5.so")]
        internal static extern int H5Dread(int dataset_id, int mem_type_id, int mem_space_id, int file_space_id, int xfer_plist_id, IntPtr buf);
        
        [DllImport("libhdf5.so")]
        internal static extern int H5Dwrite(int dataset_id, int mem_type_id, int mem_space_id, int file_space_id, int xfer_plist_id, IntPtr buf);
        
//        internal static int H5Dvlen_get_buf_size(int dataset_id, int type_id, int space_id, out long size)
//        {
//            IntPtr tmp;
//            int result = H5Dvlen_get_buf_size(dataset_id, type_id, space_id, out tmp);
//            size = (long)tmp;
//            return result;
//        }
        
//        [DllImport("libhdf5.so")]
//        internal static extern int H5Dvlen_get_buf_size(int dataset_id, int type_id, int space_id, out IntPtr size);
        
        [DllImport("libhdf5.so")]
        internal static extern int H5Dvlen_reclaim(int type_id, int space_id, int plist_id, IntPtr buf);
        
        // HDF5 file imports

        [DllImport("libhdf5.so")]
        internal static extern int H5Fcreate(string filename, uint flags, int create_id, int access_id);

        [DllImport("libhdf5.so")]
        internal static extern int H5Fopen(string filename, uint flags, int access_id);

        [DllImport("libhdf5.so")]
        internal static extern int H5Fclose(int file_id);
        
        [DllImport("libhdf5.so")]
        internal static extern int H5Fflush(int loc, FileScope scope);
        
        [DllImport("libhdf5.so")]
        internal static extern int H5Fget_filesize(int file_id, out ulong size);

        internal static long H5Fget_name(int file_id, IntPtr name, long size)
        {
            return (long)H5Fget_name(file_id, name, (IntPtr)size);
        }

        [DllImport("libhdf5.so")]
        internal static extern IntPtr H5Fget_name(int file_id, IntPtr name, IntPtr size);

        [DllImport("libhdf5.so")]
        internal static extern IntPtr H5Fget_obj_count(int file_id, uint types);

        internal static long H5Fget_obj_ids(int file_id, uint types, long max_objs, IntPtr obj_id_list)
        {
            return (long)H5Fget_obj_ids(file_id, types, (IntPtr)max_objs, obj_id_list);
        }

        [DllImport("libhdf5.so")]
        internal static extern IntPtr H5Fget_obj_ids(int file_id, uint types, IntPtr max_objs, IntPtr obj_id_list);

        [DllImport("libhdf5.so")]
        internal static extern int H5Fis_hdf5(string filename);
        
        // HDF5 group imports

        [DllImport("libhdf5.so")]
        internal static extern int H5Gcreate2(int loc, string name, long size_hint, int gcpl_id, int gapl_id);

        [DllImport("libhdf5.so")]
        internal static extern int H5Gopen2(int loc, string name, int gapl_id);

        [DllImport("libhdf5.so")]
        internal static extern int H5Gclose(int group_id);

        internal static int H5Gget_num_objs(int loc, out long num_obj)
        {
            IntPtr tmp;
            int result = H5Gget_num_objs(loc, out tmp);
            num_obj = (long)tmp;
            return result;
        }
        
        [DllImport("libhdf5.so")]
        internal static extern int H5Gget_num_objs(int loc, out IntPtr num_obj);
        
        [DllImport("libhdf5.so")]
        internal static extern IntPtr H5Gget_objname_by_idx(int loc, ulong idx, IntPtr name, IntPtr size);
        
        [DllImport("libhdf5.so")]
        internal static extern ObjectType H5Gget_objtype_by_idx(int loc, ulong idx);
        
        // HDF5 dataspace imports
        
        [DllImport("libhdf5.so")]
        internal static extern int H5Screate_simple(int rank, long[] dims, long[] maxdims);
        
        [DllImport("libhdf5.so")]
        internal static extern int H5Sclose(int space_id);
        
        [DllImport("libhdf5.so")]
        internal static extern int H5Sget_simple_extent_ndims(int space_id);
        
        [DllImport("libhdf5.so")]
        internal static extern int H5Sget_simple_extent_dims(int space_id, IntPtr dims, IntPtr maxdims);
        
        [DllImport("libhdf5.so")]
        internal static extern int H5Sis_simple(int space_id);
        
        [DllImport("libhdf5.so")]
        internal static extern int H5Sselect_all(int space_id);
        
        [DllImport("libhdf5.so")]
        internal static extern int H5Sselect_none(int space_id);
        
        [DllImport("libhdf5.so")]
        internal static extern int H5Sselect_hyperslab(int space_id, SelectOperation op, IntPtr start, IntPtr stride, IntPtr count, IntPtr block);
        
        [DllImport("libhdf5.so")]
        internal static extern int H5Sselect_valid(int space_id);
        
        // HDF5 datatype imports
        
        [DllImport("libhdf5.so")]
        internal static extern int H5Tcreate(DatatypeClass klass, IntPtr size);
        
        [DllImport("libhdf5.so")]
        internal static extern int H5Tvlen_create(int base_type_id);
        
        [DllImport("libhdf5.so")]
        internal static extern int H5Tcopy(int type_id);
        
        [DllImport("libhdf5.so")]
        internal static extern int H5Tinsert(int type_id, string name, IntPtr offset, int field_id);
        
        [DllImport("libhdf5.so")]
        internal static extern int H5Tclose(int type_id);
        
        [DllImport("libhdf5.so")]
        internal static extern DatatypeClass H5Tget_class(int type_id);
        
        [DllImport("libhdf5.so")]
        internal static extern IntPtr H5Tget_size(int type_id);
        
        [DllImport("libhdf5.so")]
        internal static extern int H5Tset_size(int type_id, long size);
        
        [DllImport("libhdf5.so")]
        internal static extern ByteOrder H5Tget_order(int type_id);
        
        [DllImport("libhdf5.so")]
        internal static extern int H5Tset_order(int type_id, ByteOrder order);
        
        [DllImport("libhdf5.so")]
        internal static extern int H5Tis_variable_str(int type_id);
    }
}
