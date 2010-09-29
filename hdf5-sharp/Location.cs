// 
// Location.cs
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
    public enum ObjectType
    {
        Unknown   =  -1, /* Unknown object type     */
        Link      =   0, /* Object is a symbolic link   */
        Group     =   1, /* Object is a group       */
        Dataset   =   2, /* Object is a dataset     */
        Type      =   3, /* Object is a named data type */
    }
    
    public abstract class Location : Base
    {
        internal Location(int raw) : base(raw) {}
        
        public void Close()
        {
            Dispose();
        }
        
        public string GetObjectName(int index)
        {
            if (index < 0 || index >= NumObjects)
                throw new ArgumentOutOfRangeException("index");
            long size = (long)H5Gget_objname_by_idx(raw, (ulong)index, IntPtr.Zero, (IntPtr)0);
            if (size < 0)
                throw new ApplicationException("Error determining length of object name.");
            else if (size == 0)
                return null;
            IntPtr hname = Marshal.AllocHGlobal((int)size+1);
            size = (long)H5Gget_objname_by_idx(raw, (ulong)index, hname, (IntPtr)(size+1));
            if (size < 0)
                throw new ApplicationException("Error getting object name.");
            else if (size == 0)
                return null;
            string name = Marshal.PtrToStringAnsi(hname);
            Marshal.FreeHGlobal(hname);
            return name;
        }
        
        public ObjectType GetObjectType(int index)
        {
            if (index < 0 || index >= NumObjects)
                throw new ArgumentOutOfRangeException("index");
            return H5Gget_objtype_by_idx(raw, (ulong)index);
        }
        
        public int NumObjects
        {
            get
            {
                ulong num_obj;
                int err = H5Gget_num_objs(raw, out num_obj);
                if (err < 0)
                    throw new ApplicationException("Error determining number of objects.");
                return (int)num_obj;
            }
        }
        
        // imports
        
        [DllImport("hdf5")]
        private static extern int H5Gget_num_objs(int loc, out ulong num_obj);
        
        [DllImport("hdf5")]
        private static extern IntPtr H5Gget_objname_by_idx(int loc, ulong idx, IntPtr name, IntPtr size);
        
        [DllImport("hdf5")]
        private static extern ObjectType H5Gget_objtype_by_idx(int loc, ulong idx);
    }
}
