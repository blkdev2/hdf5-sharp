//  
// File.cs
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
    public enum FileObjectType
    {
        File     = 0x01,
        Dataset  = 0x02,
        Group    = 0x04,
        Datatype = 0x08,
        Attr     = 0x10,
        All      = 0x1F,
        Local    = 0x20
    }
    
    public class File : Location
    {
        internal File(int raw) : base(raw)
        {
        }
        
        public long GetOpenObjCount(FileObjectType types)
        {
            return (long)H5Fget_obj_count(raw, (uint)types);
        }
        
        public long Size
        {
            get
            {
                ulong size;
                int err = H5Fget_filesize(raw, out size);
                if (err < 0)
                    throw new ApplicationException("Error getting file size.");
                return (long)size;
            }
        }
        
        public string Name
        {
            get
            {
                long size = H5Fget_name(raw, IntPtr.Zero, 0);
                if (size < 0)
                    throw new ApplicationException("Error determining length of file name.");
                else if (size == 0)
                    return null;
                string name = null;
                IntPtr hname = Marshal.AllocHGlobal((int)size+1);
                try {
                    size = H5Fget_name(raw, hname, size+1);
                    if (size < 0)
                        throw new ApplicationException("Error getting file name.");
                    else if (size == 0)
                        return null;
                    name = Marshal.PtrToStringAnsi(hname);
                } finally {
                    Marshal.FreeHGlobal(hname);
                }
                return name;
            }
        }
        
        // IDisposable stuff
        
        protected override void Dispose (bool disposing)
        {
            int err = H5Fclose(raw);
            if (err < 0)
                throw new ApplicationException("Error closing file.");
            base.Dispose(disposing);
        }
        
        // static methods
        
        public static File Create(string filename, FileAccessFlags flags)
        {
            return new File(H5Fcreate(filename, (uint)flags, 0, 0));
        }
        
        public static File Open(string filename, FileAccessFlags flags)
        {
            return new File(H5Fopen(filename, (uint)flags, 0));
        }
        
        public static bool IsHdf5(string filename)
        {
            int result = H5Fis_hdf5(filename);
            if (result < 0)
                throw new ApplicationException("Error determining file type.");
            return result > 0;
        }
    }
}
