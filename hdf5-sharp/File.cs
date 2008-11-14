// File.cs created with MonoDevelop
// User: uhrm at 17:28 11/12/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.Runtime.InteropServices;

namespace Hdf5
{
    public enum FileScope
    {
        Local   = 0,
        Global  = 1,
        Down    = 2
    }
    
    public class File : Location
    {
        public File(string filename, FileAccessFlags flags)
        {
            raw = H5Fcreate(filename, (uint)flags, 0, 0);
            if (raw < 0)
                throw new ApplicationException();
        }
        
        public void Flush()
        {
            Flush(FileScope.Local);
        }
        
        public void Flush(FileScope scope)
        {
        }
        
        public ulong Filesize
        {
            get
            {
                ulong size;
                int err = H5Fget_filesize(raw, out size);
                if (err < 0)
                    throw new ApplicationException("Error getting file size.");
                return size;
            }
        }
        
        // IDisposable stuff
        
        protected override void Dispose (bool disposing)
        {
            H5Fclose(raw);
            base.Dispose(disposing);
        }
        
        public static bool IsHdf5(string filename)
        {
            int result = H5Fis_hdf5(filename);
            if (result < 0)
                throw new ApplicationException("Error determining file type.");
            return result > 0;
        }
        
        // imports

        [DllImport("hdf5")]
        private static extern int H5Fcreate(string filename, uint flags, int create_id, int access_id);

        [DllImport("hdf5")]
        private static extern int H5Fclose(int file_id);

        [DllImport("hdf5")]
        private static extern int H5Fget_filesize(int file_id, out ulong size);

        [DllImport("hdf5")]
        private static extern int H5Fis_hdf5(string filename);
    }
}
