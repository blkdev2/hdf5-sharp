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
        private struct Triplet
        {
//            public int row;
//            public int col;
//            public double val;
            public int i;
            public int j;
            public double v;
//            public Triplet(int i, int j, double v) { row=i; col=j; val=v; }
            public Triplet(int i, int j, double v) { this.i=i; this.j=j; this.v=v; }
        }
        
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
            
            Assert.AreEqual(5, f2.NumObjects);
            
            Dataset ds;
            Dataspace sp;

            // dataset T1
            ds = Dataset.Open(f2, "T1");
            sp = ds.Space;
            Assert.AreEqual(2, sp.NumDimensions);
            int[,] T1 = (int[,])ds.ReadValue<int>();
            Assert.AreEqual(4, T1.GetLength(0));
            Assert.AreEqual(4, T1.GetLength(1));
            Assert.AreEqual(0, T1[0,0]);
            Assert.AreEqual(1, T1[0,1]);
            Assert.AreEqual(2, T1[0,2]);
            Assert.AreEqual(3, T1[0,3]);
            Assert.AreEqual(1, T1[1,0]);
            Assert.AreEqual(2, T1[1,1]);
            Assert.AreEqual(3, T1[1,2]);
            Assert.AreEqual(4, T1[1,3]);
            Assert.AreEqual(2, T1[2,0]);
            Assert.AreEqual(3, T1[2,1]);
            Assert.AreEqual(4, T1[2,2]);
            Assert.AreEqual(5, T1[2,3]);
            Assert.AreEqual(3, T1[3,0]);
            Assert.AreEqual(4, T1[3,1]);
            Assert.AreEqual(5, T1[3,2]);
            Assert.AreEqual(6, T1[3,3]);
            sp.Close();
            ds.Close();
            
            // dataset T2
            ds = Dataset.Open(f2, "T2");
            sp = ds.Space;
            Assert.AreEqual(2, sp.NumDimensions);
            double[,] T2 = (double[,])ds.ReadValue<double>();
            Assert.AreEqual(4, T2.GetLength(0));
            Assert.AreEqual(4, T2.GetLength(1));
            Assert.AreEqual( 0.0, T2[0,0]);
            Assert.AreEqual( 1.0, T2[0,1]);
            Assert.AreEqual( 2.0, T2[0,2]);
            Assert.AreEqual( 3.0, T2[0,3]);
            Assert.AreEqual( 4.0, T2[1,0]);
            Assert.AreEqual( 5.0, T2[1,1]);
            Assert.AreEqual( 6.0, T2[1,2]);
            Assert.AreEqual( 7.0, T2[1,3]);
            Assert.AreEqual( 8.0, T2[2,0]);
            Assert.AreEqual( 9.0, T2[2,1]);
            Assert.AreEqual(10.0, T2[2,2]);
            Assert.AreEqual(11.0, T2[2,3]);
            Assert.AreEqual(12.0, T2[3,0]);
            Assert.AreEqual(13.0, T2[3,1]);
            Assert.AreEqual(14.0, T2[3,2]);
            Assert.AreEqual(15.0, T2[3,3]);
            sp.Close();
            ds.Close();
            
            // dataset T3
            ds = Dataset.Open(f2, "T3");
            sp = ds.Space;
            Assert.AreEqual(1, sp.NumDimensions);
            double[][] T3 = ds.ReadVariableLength<double>();
            Assert.AreEqual(4, T3.Length);
            Assert.AreEqual(1, T3[0].Length);
            Assert.AreEqual(0.0, T3[0][0]);
            Assert.AreEqual(2, T3[1].Length);
            Assert.AreEqual(1.1, T3[1][0]);
            Assert.AreEqual(2.2, T3[1][1]);
            Assert.AreEqual(3, T3[2].Length);
            Assert.AreEqual(3.3, T3[2][0]);
            Assert.AreEqual(4.4, T3[2][1]);
            Assert.AreEqual(5.5, T3[2][2]);
            Assert.AreEqual(4, T3[3].Length);
            Assert.AreEqual(6.6, T3[3][0]);
            Assert.AreEqual(7.7, T3[3][1]);
            Assert.AreEqual(8.8, T3[3][2]);
            Assert.AreEqual(9.9, T3[3][3]);
            sp.Close();
            ds.Close();
            
            // dataset T4
            ds = Dataset.Open(f2, "T4");
            sp = ds.Space;
            Assert.AreEqual(1, sp.NumDimensions);
            Triplet[] T4 = (Triplet[])ds.ReadValue<Triplet>();
            Assert.AreEqual(8, T4.Length);
            Assert.AreEqual(  0, T4[0].i);
            Assert.AreEqual(  0, T4[0].j);
            Assert.AreEqual(5.0, T4[0].v);
            Assert.AreEqual(  0, T4[1].i);
            Assert.AreEqual(  1, T4[1].j);
            Assert.AreEqual(3.0, T4[1].v);
            Assert.AreEqual(  1, T4[2].i);
            Assert.AreEqual(  0, T4[2].j);
            Assert.AreEqual(8.0, T4[2].v);
            Assert.AreEqual(  1, T4[3].i);
            Assert.AreEqual(  2, T4[3].j);
            Assert.AreEqual(1.0, T4[3].v);
            Assert.AreEqual(  2, T4[4].i);
            Assert.AreEqual(  0, T4[4].j);
            Assert.AreEqual(6.0, T4[4].v);
            Assert.AreEqual(  2, T4[5].i);
            Assert.AreEqual(  3, T4[5].j);
            Assert.AreEqual(9.0, T4[5].v);
            Assert.AreEqual(  3, T4[6].i);
            Assert.AreEqual(  0, T4[6].j);
            Assert.AreEqual(2.0, T4[6].v);
            Assert.AreEqual(  3, T4[7].i);
            Assert.AreEqual(  4, T4[7].j);
            Assert.AreEqual(4.0, T4[7].v);
            sp.Close();
            ds.Close();
            
            Group g1 = Group.Open(f2, "G1");
            Assert.AreEqual(2, g1.NumObjects);
            
            // dataset S1
            ds = Dataset.Open(g1, "S1");
            sp = ds.Space;
            Assert.AreEqual(1, sp.NumDimensions);
            string[] S1 = (string[])ds.ReadString();
            Assert.AreEqual(4, S1.Length);
            Assert.AreEqual("String 1", S1[0]);
            Assert.AreEqual("Some other string", S1[1]);
            Assert.AreEqual("Third string which is quite a bit longer", S1[2]);
            Assert.AreEqual("Last!", S1[3]);
            sp.Close();
            ds.Close();
            
            // dataset S2
            ds = Dataset.Open(f2, "G1/S2");
            sp = ds.Space;
            Assert.AreEqual(2, sp.NumDimensions);
            string[,] S2 = (string[,])ds.ReadString();
            Assert.AreEqual(2, S2.GetLength(0));
            Assert.AreEqual(2, S2.GetLength(1));
            Assert.AreEqual("S(1,1)", S2[0,0]);
            Assert.AreEqual("S(1,2)", S2[0,1]);
            Assert.AreEqual("S(2,1)", S2[1,0]);
            Assert.AreEqual("S(2,2)", S2[1,1]);
            sp.Close();
            ds.Close();
            
        }
    }
}
