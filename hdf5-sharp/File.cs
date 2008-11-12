// File.cs created with MonoDevelop
// User: uhrm at 17:28 11/12/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.Runtime.InteropServices;

namespace Hdf5
{
    public class File : Location
    {
        public File(string filename, FileAccessFlags flags)
        {
            raw = H5Fcreate(filename, (uint)flags, 0, 0);
            if (raw < 0)
                throw new ApplicationException();
        }
        
        // IDisposable stuff
        
        protected override void Dispose (bool disposing)
        {
            H5Fclose(raw);
            base.Dispose(disposing);
        }
        
        // imports

        [DllImport("hdf5")]
        private static extern int H5Fcreate(string filename, uint flags, int create_id, int access_id);

        [DllImport("hdf5")]
        private static extern int H5Fclose(int file_id);
    }
}
