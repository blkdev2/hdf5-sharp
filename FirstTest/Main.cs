// Main.cs created with MonoDevelop
// User: uhrm at 17:01 11/12/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//
using System;

namespace Hdf5
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            H5.CheckVersion(1, 6, 4);
            uint maj, min, rel;
            H5.GetVersion(out maj, out min, out rel);
            Console.WriteLine("HDF5 library version {0}.{1}.{2}", maj, min, rel);
            
            File f = new File("first_test.h5", 0);
            Dataset ds = new Dataset(f, "T1", Datatype.STD_U32LE);
            
            ds.Close();
            f.Close();
        }
    }
}