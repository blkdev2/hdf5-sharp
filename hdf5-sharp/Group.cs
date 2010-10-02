//  
// Group.cs
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
//using System.IO;
//using System.Runtime.InteropServices;

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
    }
}
