// TestAttribute.cs created with MonoDevelop
// User: uhrm at 17:31 09/26/2009
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.IO;
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
                    using (Hdf5.Attribute h5at = Hdf5.Attribute.CreateWithData<int>(h5file, "A1", 1));
                }
            
                using (Hdf5.File h5file = Hdf5.File.Open(tmpfile, Hdf5.FileAccessFlags.ReadOnly))
                using (Hdf5.Attribute h5at = Hdf5.Attribute.Open(h5file, "A1"))
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
                    Assert.AreEqual(1, h5at.ReadValue<int>());
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
                    using (Hdf5.Attribute h5at = Hdf5.Attribute.CreateWithData<int>(h5group, "A1", 1));
                }
            
                using (Hdf5.File h5file = Hdf5.File.Open(tmpfile, Hdf5.FileAccessFlags.ReadOnly))
                using (Hdf5.Group h5group = Hdf5.Group.Open(h5file, "G1"))
                using (Hdf5.Attribute h5at = Hdf5.Attribute.Open(h5group, "A1"))
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
                    Assert.AreEqual(1, h5at.ReadValue<int>());
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
                using (Hdf5.Dataset h5ds = Hdf5.Dataset.Create(h5group, "T1", Hdf5.Datatype.Int32LE, new Hdf5.Dataspace(new ulong[] {128}, new ulong[] {128})))
                {
                    using (Hdf5.Attribute h5at = Hdf5.Attribute.CreateWithData(h5ds, "A1", "attrtest"));
                }
            
                using (Hdf5.File h5file = Hdf5.File.Open(tmpfile, Hdf5.FileAccessFlags.ReadOnly))
                using (Hdf5.Dataset h5ds = Hdf5.Dataset.Open(h5file, "G1/T1"))
                using (Hdf5.Attribute h5at = Hdf5.Attribute.Open(h5ds, "A1"))
                {
                    using (Hdf5.Datatype h5tp = h5at.Type)
                    {
                        Assert.AreEqual(Hdf5.DatatypeClass.String, h5tp.Class);
                        Assert.AreEqual(8, h5tp.Size);
                    }
                    using (Hdf5.Dataspace h5sp = h5at.Space)
                    {
                        Assert.AreEqual(1, h5sp.NumDimensions);
                        Assert.AreEqual(1, h5sp.GetDimensions()[0]);
                    }
                    Assert.AreEqual("attrtest", h5at.ReadString());
                }
            } finally {
                System.IO.File.Delete(tmpfile);
            }
        }
    }
}
