// TestFile.cs created with MonoDevelop
// User: uhrm at 16:07 11/14/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using NUnit.Framework;

using Hdf5;

namespace Hdf5.Tests
{
    [TestFixture]
    public class TestFile
    {
        [Test]
        public void TestFileCreate()
        {
            string tmpfile = System.IO.Path.GetTempFileName();
            try
            {
                using (Hdf5.File h5file = Hdf5.File.Create(tmpfile, Hdf5.FileAccessFlags.Truncate))
                {
                    Assert.AreEqual(tmpfile, h5file.Filename);
                }
            } finally {
                System.IO.File.Delete(tmpfile);
            }
        }
        
        [Test]
        public void TestFileOpen()
        {
            string tmpfile = System.IO.Path.GetTempFileName();
            try
            {
                using (Hdf5.File.Create(tmpfile, Hdf5.FileAccessFlags.Truncate));
            
                using (Hdf5.File h5file = Hdf5.File.Open(tmpfile, Hdf5.FileAccessFlags.ReadOnly))
                {
                    Assert.AreEqual(tmpfile, h5file.Filename);
                }
            } finally {
                System.IO.File.Delete(tmpfile);
            }
        }
    }
}
