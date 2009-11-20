//  
//  H5.cs
//  hdf-sharp
//
//  Created by Markus Uhr on 12/11/2008.
//  Copyright 2008 Markus Uhr. All rights reserved.
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
    
    public class H5
    {
        public static void Open()
        {
            int err = H5open();
            if (err < 0)
                throw new ApplicationException("Error opening HDF5 library.");
        }
        
        public static void CheckVersion(uint majnum, uint minnum, uint relnum)
        {
            H5check_version(majnum, minnum, relnum);
        }
        
        public static void GarbageCollect()
        {
            H5garbage_collect();
        }
        
        public static void GetVersion(out uint majnum, out uint minnum, out uint relnum)
        {
            H5get_libversion(out majnum, out minnum, out relnum);
        }

        // imports

        [DllImport("hdf5")]
        private static extern int H5open();

        [DllImport("hdf5")]
        private static extern int H5check_version(uint majnum, uint minnum, uint relnum);

        [DllImport("hdf5")]
        private static extern int H5garbage_collect();

        [DllImport("hdf5")]
        private static extern int H5get_libversion(out uint majnum, out uint minnum, out uint relnum);
    }

}

