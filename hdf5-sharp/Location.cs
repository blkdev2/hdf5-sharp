// Location.cs created with MonoDevelop
// User: uhrm at 17:26 11/12/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;

namespace Hdf5
{
    public abstract class Location : Base
    {
        public void Close()
        {
            Dispose();
        }
    }
}
