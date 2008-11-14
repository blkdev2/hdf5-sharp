// Base.cs created with MonoDevelop
// User: uhrm at 17:24 11/12/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;

namespace Hdf5
{
    public abstract class Base : IDisposable
    {
        internal int raw;
        
        internal Base()
        {
        }
        
        internal Base(int raw)
        {
            if (raw < 0)
                throw new ApplicationException("Invalid ID.");
            this.raw = raw;
        }
        
        ~Base()
        {
            Dispose();
        }
        
        // IDisposable stuff
        
        private bool disposed = false;
        
        public void Dispose()
        {
            if (!disposed)
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
        }
        
        protected virtual void Dispose(bool disposing)
        {
            disposed = true;
        }
    }
}
