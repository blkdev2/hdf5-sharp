//  
// H5.cs
// hdf-sharp
//
// Copyright 2008-2010 Markus Uhr <markus.uhr@bsse.ethz.ch>.
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions
// are met:
//     * Redistributions of source code must retain the above copyright
//       notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright
//       notice, this list of conditions and the following disclaimer in
//       the documentation and/or other materials provided with the
//       distribution.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
// "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
// OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
// LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
// DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
// THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
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

        // HDF5 library imports

        [DllImport("hdf5")]
        internal static extern int H5open();

        [DllImport("hdf5")]
        internal static extern int H5check_version(uint majnum, uint minnum, uint relnum);

        [DllImport("hdf5")]
        internal static extern int H5garbage_collect();

        [DllImport("hdf5")]
        internal static extern int H5get_libversion(out uint majnum, out uint minnum, out uint relnum);
    }
}

