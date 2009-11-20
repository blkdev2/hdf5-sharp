//  
//  Base.cs
//  hdf-sharp
//
//  Created by Markus Uhr on 12/11/2008.
//  Copyright 2008 Markus Uhr. All rights reserved.
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
            Dispose(false);
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
