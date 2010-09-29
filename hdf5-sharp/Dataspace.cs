//  
// Dataspace.cs
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
        
        public Dataspace(long[] dimensions) : this(dimensions, null)
        {
        }
        
        public Dataspace(long[] dimensions, long[] maxdimensions)
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
        
        public void SelectHyperslab(SelectOperation op, long[] start, long[] stride, long[] count, long[] block)
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
        private static extern int H5Screate_simple(int rank, long[] dims, long[] maxdims);
        
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
