// Location.cs created with MonoDevelop
// User: uhrm at 17:26 11/12/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.Runtime.InteropServices;

namespace Hdf5
{
    public abstract class Location : Base
    {
        public void Close()
        {
            Dispose();
        }
        
        public string GetObjectName(int index)
        {
            long size = H5Gget_objname_by_idx(raw, (ulong)index, IntPtr.Zero, 0);
            if (size < 0)
                throw new ApplicationException("Error determining length of object name.");
            else if (size == 0)
                return null;
            IntPtr hname = Marshal.AllocHGlobal((int)size+1);
            size = H5Gget_objname_by_idx(raw, (ulong)index, hname, size+1);
            if (size < 0)
                throw new ApplicationException("Error determining length of object name.");
            else if (size == 0)
                return null;
            string name = Marshal.PtrToStringAnsi(hname);
            Marshal.FreeHGlobal(hname);
            return name;
        }
        
        public int NumObjects
        {
            get
            {
                ulong num_obj;
                int err = H5Gget_num_objs(raw, out num_obj);
                if (err < 0)
                    throw new ApplicationException("Error determining number of objects.");
                return (int)num_obj;
            }
        }
        
        // imports
        
        [DllImport("hdf5")]
        private static extern int H5Gget_num_objs(int loc, out ulong num_obj);
        
        [DllImport("hdf5")]
        private static extern long H5Gget_objname_by_idx(int loc, ulong idx, IntPtr name, long size);
    }
}
