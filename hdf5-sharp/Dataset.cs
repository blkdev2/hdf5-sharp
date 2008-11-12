// Dataset.cs created with MonoDevelop
// User: uhrm at 17:12 11/12/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.Runtime.InteropServices;

namespace Hdf5
{
    public class Dataset : Base
    {
        public Dataset(Location loc, string name, Datatype type)
        {
            raw = H5Dcreate(loc.raw, name, type.raw, 0, 0);
            if (raw < 0)
                throw new ApplicationException();
        }
        
        public void Close()
        {
            Dispose();
        }
        
        // IDisposable stuff
        
        protected override void Dispose (bool disposing)
        {
            H5Dclose(raw);
            base.Dispose(disposing);
        }

        
        // imports
        
        [DllImport("hdf5")]
        private static extern int H5Dcreate(int loc_id, string name, int type_id, int space_id, int dcpl_id);
        
        [DllImport("hdf5")]
        private static extern int H5Dclose(int dataset_id);
    }
}
