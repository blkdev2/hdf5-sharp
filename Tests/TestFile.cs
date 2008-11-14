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
        private File f1;
        private File f2;
        
        [SetUp]
        public void SetUp()
        {
            f1 = File.Create("f1.h5", FileAccessFlags.Truncate);
            f2 = File.Open("f2.h5", FileAccessFlags.ReadOnly);
        }
        
        [TearDown]
        public void TearDown()
        {
            f1.Close();
            f2.Close();
        }
        
        public void TestFile1()
        {
            Assert.AreEqual("f1.h5", f1.Filename);
        }
        
        public void TestFile2()
        {
            Assert.AreEqual("f2.h5", f2.Filename);
            
            Assert.AreEqual(4, f2.NumObjects);
        }
    }
}
