//  
//  Dataspace.cs
//  hdf-sharp
//
//  Created by Markus Uhr on 13/11/2008.
//  Copyright 2008 Markus Uhr. All rights reserved.
//

using System;
using System.Runtime.InteropServices;

namespace Hdf5
{
    public enum SpaceStatus
    {
        Error         = -1,
        NotAllocated  =  0,
        PartAllocated =  1,
        Allocated     =  2,
    }
    
    public enum SelectOperation
    {
        NoOp    = -1,
        Set     =  0,
        Or      =  1,
        And     =  2,
        Xor     =  3,
        NotB    =  4,
        NotA    =  5,
        Append  =  6,
        Prepend =  7,
        Invalid =  8
    }
    
    public class Dataspace : Base
    {
        internal Dataspace(int raw) : base(raw) {}
        
        public Dataspace(ulong[] dimensions) : this(dimensions, null)
        {
        }
        
        public Dataspace(ulong[] dimensions, ulong[] maxdimensions)
        {
            raw = H5Screate_simple(dimensions.Length, dimensions, maxdimensions);
            if (raw < 0)
                throw new ApplicationException();
        }
        
        public void Close()
        {
            Dispose();
        }
        
        public long[] GetDimensions()
        {
            long[] result = new long[NumDimensions];
            GCHandle hres = GCHandle.Alloc(result, GCHandleType.Pinned);
            try {
                int err = H5Sget_simple_extent_dims(raw, hres.AddrOfPinnedObject(), IntPtr.Zero);
                if (err < 0)
                    throw new ApplicationException("Error getting dimensions of dataspace.");
            } finally {
                hres.Free();
            }
            return result;
        }
        
        public void SelectAll()
        {
            int err = H5Sselect_all(raw);
            if (err < 0)
                throw new ApplicationException("Error in selecting whole dataspace.");
        }
        
        public void SelectNone()
        {
            int err = H5Sselect_none(raw);
            if (err < 0)
                throw new ApplicationException("Error in deleting selection in dataspace.");
        }
        
        public void SelectHyperslab(SelectOperation op, ulong[] start, ulong[] stride, ulong[] count, ulong[] block)
        {
            GCHandle hstart  = GCHandle.Alloc(start,  GCHandleType.Pinned);
            GCHandle hstride = GCHandle.Alloc(stride, GCHandleType.Pinned);
            GCHandle hcount  = GCHandle.Alloc(count,  GCHandleType.Pinned);
            GCHandle hblock  = GCHandle.Alloc(block,  GCHandleType.Pinned);
            try {
                int err = H5Sselect_hyperslab(raw, op, hstart.AddrOfPinnedObject(), hstride.AddrOfPinnedObject(),
                                              hcount.AddrOfPinnedObject(), hblock.AddrOfPinnedObject());
                if (err < 0)
                    throw new ApplicationException("Error in selecting hyperslab.");
            } finally {
                hstart.Free();
                hstride.Free();
                hcount.Free();
                hblock.Free();
            }
        }
        
        public int NumDimensions
        {
            get { return H5Sget_simple_extent_ndims(raw); }
        }
        
        public bool IsSimple
        {
            get
            {
                int err = H5Sis_simple(raw);
                if (err < 0)
                    throw new ApplicationException("Error determining whether dataspace is simple.");
                return err > 0;
            }
        }
        
        public bool IsSelectionValid
        {
            get
            {
                int err = H5Sselect_valid(raw);
                if (err > 0)
                    return true;
                else if (err == 0)
                    return false;
                else
                    throw new ApplicationException("Error determining whether selection is valid.");
            }
        }
        
        // IDisposable stuff
        
        protected override void Dispose(bool disposing)
        {
            if (raw > 0)
            {
                int err = H5Sclose(raw);
                if (err < 0)
                    throw new ApplicationException("Error closing dataset.");
            }
            base.Dispose(disposing);
        }
        
        public static readonly Dataspace All = new Dataspace(0);
        
        // imports
        
        [DllImport("hdf5")]
        private static extern int H5Screate_simple(int rank, ulong[] dims, ulong[] maxdims);
        
        [DllImport("hdf5")]
        private static extern int H5Sclose(int space_id);
        
        [DllImport("hdf5")]
        private static extern int H5Sget_simple_extent_ndims(int space_id);
        
        [DllImport("hdf5")]
        private static extern int H5Sget_simple_extent_dims(int space_id, IntPtr dims, IntPtr maxdims);
        
        [DllImport("hdf5")]
        private static extern int H5Sis_simple(int space_id);
        
        [DllImport("hdf5")]
        private static extern int H5Sselect_all(int space_id);
        
        [DllImport("hdf5")]
        private static extern int H5Sselect_none(int space_id);
        
        [DllImport("hdf5")]
        private static extern int H5Sselect_hyperslab(int space_id, SelectOperation op, IntPtr start, IntPtr stride, IntPtr count, IntPtr block);
        
        [DllImport("hdf5")]
        private static extern int H5Sselect_valid(int space_id);
    }
}
