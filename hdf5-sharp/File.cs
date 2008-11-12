// File.cs created with MonoDevelop
// User: uhrm at 17:28 11/12/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.Runtime.InteropServices;

namespace Hdf5
{
    [Flags]
    public enum FileAccessFlags
    {
        ReadOnly  = 0x0000,
        ReadWrite = 0x0001,
        Truncate  = 0x0002,
        Exclusive = 0x0004,
        Debug     = 0x0008,
        Create    = 0x0010
    }
    
    public class File : Location
    {
        public File(string filename, FileAccessFlags flags)
        {
            raw = H5Fcreate(filename, (uint)flags, 0, 0);
        }
        
        protected override void Dispose (bool disposing)
        {
            H5Fclose(raw);
        }
        
        // imports

        [DllImport("hdf5")]
        private static extern int H5Fcreate(string filename, uint flags, int create_id, int access_id);

        [DllImport("hdf5")]
        private static extern int H5Fclose(int file_id);
    }
}
