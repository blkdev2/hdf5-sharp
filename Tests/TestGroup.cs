// TestGroup.cs created with MonoDevelop
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
    public class TestGroup
    {
		[Test]
        public void TestGroupCreate()
        {
			string tmpfile = System.IO.Path.GetTempFileName();
			
			using (Hdf5.File h5file = Hdf5.File.Create(tmpfile, Hdf5.FileAccessFlags.Truncate))
			using (Hdf5.Group h5group = Hdf5.Group.Create(h5file, "testgroup"));
			
			System.IO.File.Delete(tmpfile);
		}
			
		[Test]
        public void TestGroupOpen()
        {
			string tmpfile = System.IO.Path.GetTempFileName();
			
			using (Hdf5.File h5file = Hdf5.File.Create(tmpfile, Hdf5.FileAccessFlags.Truncate))
			using (Hdf5.Group h5group = Hdf5.Group.Create(h5file, "testgroup"));
			
			using (Hdf5.File h5file = Hdf5.File.Open(tmpfile, Hdf5.FileAccessFlags.ReadOnly))
			using (Hdf5.Group h5group = Hdf5.Group.Open(h5file, "testgroup"));
			
			System.IO.File.Delete(tmpfile);
		}
    }
}
