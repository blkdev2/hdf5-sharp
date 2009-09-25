// Group.cs created with MonoDevelop
// User: uhrm at 15:16 11/14/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Hdf5
{
    public class Group : Location
    {
        internal Group(int raw) : base(raw)
        {
        }
        
        // IDisposable stuff
        
        protected override void Dispose (bool disposing)
        {
            H5Gclose(raw);
            base.Dispose(disposing);
        }
        
        // static creation methods
        
        public static Group Create(Location loc, string name)
        {
            return new Group(H5Gcreate2(loc.raw, name, 0, 0, 0));
        }
        
        public static Group Open(Location loc, string name)
        {
            return new Group(H5Gopen2(loc.raw, name, 0));
        }
        
        // imports

        [DllImport("hdf5")]
        private static extern int H5Gcreate2(int loc, string name, long size_hint, int gcpl_id, int gapl_id);

        [DllImport("hdf5")]
        private static extern int H5Gopen2(int loc, string name, int gapl_id);

        [DllImport("hdf5")]
        private static extern int H5Gclose(int group_id);
    }
}
