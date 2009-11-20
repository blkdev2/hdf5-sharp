//  
//  File.cs
//  hdf-sharp
//
//  Created by Markus Uhr on 12/11/2008.
//  Copyright 2008 Markus Uhr. All rights reserved.
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
        internal File(int raw) : base(raw)
        {
        }
        
        public void Flush()
        {
            Flush(FileScope.Local);
        }
        
        public void Flush(FileScope scope)
        {
        }
        
        public long Filesize
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
        
        public string Filename
        {
            get
            {
                long size = H5Fget_name(raw, IntPtr.Zero, 0);
                if (size < 0)
                    throw new ApplicationException("Error determining length of file name.");
                else if (size == 0)
                    return null;
                IntPtr hname = Marshal.AllocHGlobal((int)size+1);
                size = H5Fget_name(raw, hname, size+1);
                if (size < 0)
                    throw new ApplicationException("Error getting file name.");
                else if (size == 0)
                    return null;
                string name = Marshal.PtrToStringAnsi(hname);
                Marshal.FreeHGlobal(hname);
                return name;
            }
        }
        
        // IDisposable stuff
        
        protected override void Dispose (bool disposing)
        {
            H5Fclose(raw);
            base.Dispose(disposing);
        }
        
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
        
        // imports

        [DllImport("hdf5")]
        private static extern int H5Fcreate(string filename, uint flags, int create_id, int access_id);

        [DllImport("hdf5")]
        private static extern int H5Fopen(string filename, uint flags, int access_id);

        [DllImport("hdf5")]
        private static extern int H5Fclose(int file_id);

        [DllImport("hdf5")]
        private static extern int H5Fget_filesize(int file_id, out ulong size);

        [DllImport("hdf5")]
        private static extern int H5Fget_name(int file_id, IntPtr name, long size);

        [DllImport("hdf5")]
        private static extern int H5Fis_hdf5(string filename);
    }
}
