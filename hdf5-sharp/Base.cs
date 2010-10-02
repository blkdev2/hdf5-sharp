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

        [DllImport("hdf5")]
        internal static extern int H5Acreate2(int obj_id, string attr_name, int type_id, int space_id, int acpl_id, int aapl_id);
        
        [DllImport("hdf5")]
        internal static extern int H5Aclose(int obj_id);
        
        [DllImport("hdf5")]
        internal static extern int H5Aopen_name(int obj_id, string attr_name);
        
        [DllImport("hdf5")]
        internal static extern int H5Aread(int attr_id, int mem_type_id, IntPtr buf);
        
        [DllImport("hdf5")]
        internal static extern int H5Awrite(int attr_id, int mem_type_id, IntPtr buf);
        
        [DllImport("hdf5")]
        internal static extern int H5Aget_name(int attr_id, IntPtr size, IntPtr buf);
        
        [DllImport("hdf5")]
        internal static extern int H5Aget_space(int attr_id);
        
        [DllImport("hdf5")]
        internal static extern int H5Aget_type(int attr_id);
        
        // HDF5 dataset imports
        
        [DllImport("hdf5")]
        internal static extern int H5Dcreate2(int loc_id, string name, int type_id, int space_id, int lcpl_id, int dcpl_id, int dapl_id);
        
        [DllImport("hdf5")]
        internal static extern int H5Dopen2(int loc_id, string name, int dapl_id);
        
        [DllImport("hdf5")]
        internal static extern int H5Dclose(int dataset_id);
        
        [DllImport("hdf5")]
        internal static extern int H5Dget_space(int dataset_id);
        
        [DllImport("hdf5")]
        internal static extern int H5Dget_space_status(int dataset_id, out SpaceStatus status);
        
        [DllImport("hdf5")]
        internal static extern int H5Dget_type(int dataset_id);
        
        [DllImport("hdf5")]
        internal static extern int H5Dread(int dataset_id, int mem_type_id, int mem_space_id, int file_space_id, int xfer_plist_id, IntPtr buf);
        
        [DllImport("hdf5")]
        internal static extern int H5Dwrite(int dataset_id, int mem_type_id, int mem_space_id, int file_space_id, int xfer_plist_id, IntPtr buf);
        
        [DllImport("hdf5")]
        internal static extern int H5Dvlen_get_buf_size(int dataset_id, int type_id, int space_id, out ulong size);
        
        [DllImport("hdf5")]
        internal static extern int H5Dvlen_reclaim(int type_id, int space_id, int plist_id, IntPtr buf);
        
        // HDF5 file imports

        [DllImport("hdf5")]
        internal static extern int H5Fcreate(string filename, uint flags, int create_id, int access_id);

        [DllImport("hdf5")]
        internal static extern int H5Fopen(string filename, uint flags, int access_id);

        [DllImport("hdf5")]
        internal static extern int H5Fclose(int file_id);
        
        [DllImport("hdf5")]
        internal static extern int H5Fflush(int loc, FileScope scope);
        
        [DllImport("hdf5")]
        internal static extern int H5Fget_filesize(int file_id, out ulong size);

        [DllImport("hdf5")]
        internal static extern int H5Fget_name(int file_id, IntPtr name, long size);

        [DllImport("hdf5")]
        internal static extern long H5Fget_obj_count(int file_id, uint types);

        [DllImport("hdf5")]
        internal static extern long H5Fget_obj_ids(int file_id, uint types, long max_objs, IntPtr obj_id_list);

        [DllImport("hdf5")]
        internal static extern int H5Fis_hdf5(string filename);
        
        // HDF5 group imports

        [DllImport("hdf5")]
        internal static extern int H5Gcreate2(int loc, string name, long size_hint, int gcpl_id, int gapl_id);

        [DllImport("hdf5")]
        internal static extern int H5Gopen2(int loc, string name, int gapl_id);

        [DllImport("hdf5")]
        internal static extern int H5Gclose(int group_id);

        [DllImport("hdf5")]
        internal static extern int H5Gget_num_objs(int loc, out ulong num_obj);
        
        [DllImport("hdf5")]
        internal static extern IntPtr H5Gget_objname_by_idx(int loc, ulong idx, IntPtr name, IntPtr size);
        
        [DllImport("hdf5")]
        internal static extern ObjectType H5Gget_objtype_by_idx(int loc, ulong idx);
        
        // HDF5 dataspace imports
        
        [DllImport("hdf5")]
        internal static extern int H5Screate_simple(int rank, long[] dims, long[] maxdims);
        
        [DllImport("hdf5")]
        internal static extern int H5Sclose(int space_id);
        
        [DllImport("hdf5")]
        internal static extern int H5Sget_simple_extent_ndims(int space_id);
        
        [DllImport("hdf5")]
        internal static extern int H5Sget_simple_extent_dims(int space_id, IntPtr dims, IntPtr maxdims);
        
        [DllImport("hdf5")]
        internal static extern int H5Sis_simple(int space_id);
        
        [DllImport("hdf5")]
        internal static extern int H5Sselect_all(int space_id);
        
        [DllImport("hdf5")]
        internal static extern int H5Sselect_none(int space_id);
        
        [DllImport("hdf5")]
        internal static extern int H5Sselect_hyperslab(int space_id, SelectOperation op, IntPtr start, IntPtr stride, IntPtr count, IntPtr block);
        
        [DllImport("hdf5")]
        internal static extern int H5Sselect_valid(int space_id);
        
        // HDF5 datatype imports
        
        [DllImport("hdf5")]
        internal static extern int H5Tcreate(DatatypeClass klass, IntPtr size);
        
        [DllImport("hdf5")]
        internal static extern int H5Tvlen_create(int base_type_id);
        
        [DllImport("hdf5")]
        internal static extern int H5Tcopy(int type_id);
        
        [DllImport("hdf5")]
        internal static extern int H5Tinsert(int type_id, string name, IntPtr offset, int field_id);
        
        [DllImport("hdf5")]
        internal static extern int H5Tclose(int type_id);
        
        [DllImport("hdf5")]
        internal static extern DatatypeClass H5Tget_class(int type_id);
        
        [DllImport("hdf5")]
        internal static extern IntPtr H5Tget_size(int type_id);
        
        [DllImport("hdf5")]
        internal static extern int H5Tset_size(int type_id, long size);
        
        [DllImport("hdf5")]
        internal static extern ByteOrder H5Tget_order(int type_id);
        
        [DllImport("hdf5")]
        internal static extern int H5Tset_order(int type_id, ByteOrder order);
        
        [DllImport("hdf5")]
        internal static extern int H5Tis_variable_str(int type_id);
    }
}
