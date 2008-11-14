// Dataspace.cs created with MonoDevelop
// User: uhrm at 09:07 11/13/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.Runtime.InteropServices;

namespace Hdf5
{
    public enum SpaceStatus
    {
        Error	      = -1,
        NotAllocated  =  0,
        PartAllocated =  1,
        Allocated     =  2,
    }
    
    public class Dataspace : Base
    {
        internal Dataspace(int raw) : base(raw) {}
        
        public Dataspace(ulong[] dimensions)
        {
            raw = H5Screate_simple(dimensions.Length, dimensions, null);
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
            int err = H5Sget_simple_extent_dims(raw, hres.AddrOfPinnedObject(), IntPtr.Zero);
            if (err < 0)
                throw new ApplicationException("Error getting dimensions of dataspace.");
            hres.Free();
            return result;
        }
        
        public int NumDimensions
        {
            get { return H5Sget_simple_extent_ndims(raw); }
        }
        
        // IDisposable stuff
        
        protected override void Dispose(bool disposing)
        {
            if (raw > 0)
                H5Sclose(raw);
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
    }
}
