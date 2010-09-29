// 
// TestAttribute.cs
// hdf-sharp
//
// Copyright 2009-2010 Markus Uhr <markus.uhr@bsse.ethz.ch>.
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
using System.IO;
using System.Runtime.InteropServices;
using NUnit.Framework;

using Hdf5;

namespace Hdf5.Tests
{
    [TestFixture]
    public class TestHdf5Attribute
    {
        [Test]
        public void TestCreateWithFile()
        {
            string tmpfile = System.IO.Path.GetTempFileName();
            try
            {
                using (Hdf5.File h5file = Hdf5.File.Create(tmpfile, Hdf5.FileAccessFlags.Truncate))
                {
                    using (Hdf5.Attribute<int> h5at = Hdf5.Attribute<int>.CreateWithData(h5file, "A1", 1));
                }
            
                using (Hdf5.File h5file = Hdf5.File.Open(tmpfile, Hdf5.FileAccessFlags.ReadOnly))
                using (Hdf5.Attribute<int> h5at = Hdf5.Attribute<int>.Open(h5file, "A1"))
                {
                    using (Hdf5.Datatype h5tp = h5at.Type)
                    {
                        Assert.AreEqual(Hdf5.DatatypeClass.Integer, h5tp.Class);
                        Assert.AreEqual(4, h5tp.Size);
                    }
                    using (Hdf5.Dataspace h5sp = h5at.Space)
                    {
                        Assert.AreEqual(1, h5sp.NumDimensions);
                        Assert.AreEqual(1, h5sp.GetDimensions()[0]);
                    }
                    Assert.AreEqual(1, h5at.Read());
                }
            } finally {
                System.IO.File.Delete(tmpfile);
            }
        }
        
        [Test]
        public void TestCreateWithGroup()
        {
            string tmpfile = System.IO.Path.GetTempFileName();
            try
            {
                using (Hdf5.File h5file = Hdf5.File.Create(tmpfile, Hdf5.FileAccessFlags.Truncate))
                using (Hdf5.Group h5group = Hdf5.Group.Create(h5file, "G1"))
                {
                    using (Hdf5.Attribute<int> h5at = Hdf5.Attribute<int>.CreateWithData(h5group, "A1", 1));
                }
            
                using (Hdf5.File h5file = Hdf5.File.Open(tmpfile, Hdf5.FileAccessFlags.ReadOnly))
                using (Hdf5.Group h5group = Hdf5.Group.Open(h5file, "G1"))
                using (Hdf5.Attribute<int> h5at = Hdf5.Attribute<int>.Open(h5group, "A1"))
                {
                    using (Hdf5.Datatype h5tp = h5at.Type)
                    {
                        Assert.AreEqual(Hdf5.DatatypeClass.Integer, h5tp.Class);
                        Assert.AreEqual(4, h5tp.Size);
                    }
                    using (Hdf5.Dataspace h5sp = h5at.Space)
                    {
                        Assert.AreEqual(1, h5sp.NumDimensions);
                        Assert.AreEqual(1, h5sp.GetDimensions()[0]);
                    }
                    Assert.AreEqual(1, h5at.Read());
                }
            } finally {
                System.IO.File.Delete(tmpfile);
            }
        }
        
        [Test]
        public void TestCreateWithDataset()
        {
            string tmpfile = System.IO.Path.GetTempFileName();
            try
            {
                using (Hdf5.File h5file = Hdf5.File.Create(tmpfile, Hdf5.FileAccessFlags.Truncate))
                using (Hdf5.Group h5group = Hdf5.Group.Create(h5file, "G1"))
                using (Hdf5.Dataset<int> h5ds = Hdf5.Dataset<int>.Create(h5group, "T1", new long[] {128}))
                {
                    using (Hdf5.Attribute<string> h5at = Hdf5.Attribute<string>.CreateWithData(h5ds, "A1", "attrtest"));
                }
            
                using (Hdf5.File h5file = Hdf5.File.Open(tmpfile, Hdf5.FileAccessFlags.ReadOnly))
                using (Hdf5.Dataset<int> h5ds = Hdf5.Dataset<int>.Open(h5file, "G1/T1"))
                using (Hdf5.Attribute<string> h5at = Hdf5.Attribute<string>.Open(h5ds, "A1"))
                {
                    using (Hdf5.Datatype h5tp = h5at.Type)
                        Assert.IsTrue(h5tp.IsVariableString);
                    using (Hdf5.Dataspace h5sp = h5at.Space)
                    {
                        Assert.AreEqual(1, h5sp.NumDimensions);
                        Assert.AreEqual(1, h5sp.GetDimensions()[0]);
                    }
                    Assert.AreEqual("attrtest", h5at.Read());
                }
            } finally {
                System.IO.File.Delete(tmpfile);
            }
        }
    }
}
