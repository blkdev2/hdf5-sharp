// Location.cs created with MonoDevelop
// User: uhrm at 17:26 11/12/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.Runtime.InteropServices;

namespace Hdf5
{
    public enum ObjectType
    {
        Unknown   =  -1, /* Unknown object type     */
        Link      =   0, /* Object is a symbolic link   */
        Group     =   1, /* Object is a group       */
        Dataset   =   2, /* Object is a dataset     */
        Type      =   3, /* Object is a named data type */
    }
    
    public abstract class Location : Base
    {
        internal Location(int raw) : base(raw) {}
        
        public void Close()
        {
            Dispose();
        }
        
        public string GetObjectName(int index)
        {
            if (index < 0 || index >= NumObjects)
                throw new ArgumentOutOfRangeException("index");
            long size = (long)H5Gget_objname_by_idx(raw, (ulong)index, IntPtr.Zero, (IntPtr)0);
            if (size < 0)
                throw new ApplicationException("Error determining length of object name.");
            else if (size == 0)
                return null;
            IntPtr hname = Marshal.AllocHGlobal((int)size+1);
            size = (long)H5Gget_objname_by_idx(raw, (ulong)index, hname, (IntPtr)(size+1));
            if (size < 0)
                throw new ApplicationException("Error getting object name.");
            else if (size == 0)
                return null;
            string name = Marshal.PtrToStringAnsi(hname);
            Marshal.FreeHGlobal(hname);
            return name;
        }
        
        public ObjectType GetObjectType(int index)
        {
            if (index < 0 || index >= NumObjects)
                throw new ArgumentOutOfRangeException("index");
            return H5Gget_objtype_by_idx(raw, (ulong)index);
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
        private static extern IntPtr H5Gget_objname_by_idx(int loc, ulong idx, IntPtr name, IntPtr size);
        
        [DllImport("hdf5")]
        private static extern ObjectType H5Gget_objtype_by_idx(int loc, ulong idx);
    }
}
