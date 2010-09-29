// 
// TestDataset.cs
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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using NUnit.Framework;

namespace Hdf5.Tests
{
    [TestFixture]
    public class TestDataset
    {
        private struct Triplet
        {
            public readonly int i;
            public readonly int j;
            public readonly double v;
            public Triplet(int i, int j, double v) { this.i=i; this.j=j; this.v=v; }
        }
        
        [Test]
        public void TestCreateWithFile()
        {
            string tmpfile = System.IO.Path.GetTempFileName();
            try
            {
                using (Hdf5.File h5file = Hdf5.File.Create(tmpfile, Hdf5.FileAccessFlags.Truncate))
                {
                    using (Hdf5.Dataset<int> h5ds = Hdf5.Dataset<int>.Create(h5file, "T1", new long[] {128}));
                }
            
                using (Hdf5.File h5file = Hdf5.File.Open(tmpfile, Hdf5.FileAccessFlags.ReadOnly))
                using (Hdf5.Dataset<int> h5ds = Hdf5.Dataset<int>.Open(h5file, "T1"))
                {
                    using (Hdf5.Datatype h5tp = h5ds.Type)
                    {
                        Assert.AreEqual(Hdf5.DatatypeClass.Integer, h5tp.Class);
                        Assert.AreEqual(4, h5tp.Size);
                    }
                    using (Hdf5.Dataspace h5sp = h5ds.Space)
                    {
                        Assert.AreEqual(1, h5sp.NumDimensions);
                        Assert.AreEqual(128, h5sp.GetDimensions()[0]);
                    }
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
                    using (Hdf5.Dataset<int> h5ds = Hdf5.Dataset<int>.Create(h5group, "T1", new long[] {128}));
                }
            
                using (Hdf5.File h5file = Hdf5.File.Open(tmpfile, Hdf5.FileAccessFlags.ReadOnly))
                using (Hdf5.Dataset<int> h5ds = Hdf5.Dataset<int>.Open(h5file, "G1/T1"))
                {
                    using (Hdf5.Datatype h5tp = h5ds.Type)
                    {
                        Assert.AreEqual(Hdf5.DatatypeClass.Integer, h5tp.Class);
                        Assert.AreEqual(4, h5tp.Size);
                    }
                    using (Hdf5.Dataspace h5sp = h5ds.Space)
                    {
                        Assert.AreEqual(1, h5sp.NumDimensions);
                        Assert.AreEqual(128, h5sp.GetDimensions()[0]);
                    }
                }
            } finally {
                System.IO.File.Delete(tmpfile);
            }
        }
        
        [Test]
        public void TestDataset_Int32_1D()
        {
            string tmpfile = System.IO.Path.GetTempFileName();
            try
            {
                using (Hdf5.File h5file = Hdf5.File.Create(tmpfile, Hdf5.FileAccessFlags.Truncate))
                {
                    int[] data = new int[] {0, 2, 3, 7, 6, 9};
                    using (Hdf5.Dataset<int> h5ds = Hdf5.Dataset<int>.CreateWithData(h5file, "T1", data));
                }
            
                using (Hdf5.File h5file = Hdf5.File.Open(tmpfile, Hdf5.FileAccessFlags.ReadOnly))
                using (Hdf5.Dataset<int> h5ds = Hdf5.Dataset<int>.Open(h5file, "T1"))
                {
                    using (Hdf5.Dataspace h5sp = h5ds.Space)
                    {
                        Assert.AreEqual(1, h5sp.NumDimensions);
                    }
                    
                    Assert.AreEqual(0, h5ds[0]); Assert.AreEqual(2, h5ds[1]);
                    Assert.AreEqual(3, h5ds[2]); Assert.AreEqual(7, h5ds[3]);
                    Assert.AreEqual(6, h5ds[4]); Assert.AreEqual(9, h5ds[5]);
                    
                    int[] T1 = (int[])h5ds.Read();
                    Assert.AreEqual(6, T1.GetLength(0));
                    Assert.AreEqual(0, T1[0]); Assert.AreEqual(2, T1[1]);
                    Assert.AreEqual(3, T1[2]); Assert.AreEqual(7, T1[3]);
                    Assert.AreEqual(6, T1[4]); Assert.AreEqual(9, T1[5]);
                    
                    using (Hdf5.Dataspace h5fs = h5ds.Space)
                    {
                        h5fs.SelectHyperslab(SelectOperation.Set, new long[] {1}, null, new long[] {4}, null);
                        Assert.IsTrue(h5fs.IsSelectionValid);
                        int[] H1 = new int[4];
                        using (Hdf5.Dataspace h5ms = new Hdf5.Dataspace(new long[] {4}))
                            h5ds.Read(h5ms, h5fs, H1);
                        Assert.AreEqual(2, H1[0]);
                        Assert.AreEqual(3, H1[1]);
                        Assert.AreEqual(7, H1[2]);
                        Assert.AreEqual(6, H1[3]);
                    }
                }
            } finally {
                System.IO.File.Delete(tmpfile);
            }
        }
        
        [Test]
        public void TestDataset_Int32_2D()
        {
            string tmpfile = System.IO.Path.GetTempFileName();
            try
            {
                using (Hdf5.File h5file = Hdf5.File.Create(tmpfile, Hdf5.FileAccessFlags.Truncate))
                {
                    int[,] data = new int[,] {{0, 1, 2, 3},
                                              {1, 2, 3, 4},
                                              {2, 3, 4, 5},
                                              {3, 4, 5, 6}};
                    using (Hdf5.Dataset<int> h5ds = Hdf5.Dataset<int>.CreateWithData(h5file, "T1", data));
                }
            
                using (Hdf5.File h5file = Hdf5.File.Open(tmpfile, Hdf5.FileAccessFlags.ReadOnly))
                using (Hdf5.Dataset<int> h5ds = Hdf5.Dataset<int>.Open(h5file, "T1"))
                {
                    using (Hdf5.Dataspace h5sp = h5ds.Space)
                    {
                        Assert.AreEqual(2, h5sp.NumDimensions);
                    }
                    
                    Assert.AreEqual(0, h5ds[0,0]); Assert.AreEqual(1, h5ds[0,1]);
                    Assert.AreEqual(2, h5ds[0,2]); Assert.AreEqual(3, h5ds[0,3]);
                    Assert.AreEqual(1, h5ds[1,0]); Assert.AreEqual(2, h5ds[1,1]);
                    Assert.AreEqual(3, h5ds[1,2]); Assert.AreEqual(4, h5ds[1,3]);
                    Assert.AreEqual(2, h5ds[2,0]); Assert.AreEqual(3, h5ds[2,1]);
                    Assert.AreEqual(4, h5ds[2,2]); Assert.AreEqual(5, h5ds[2,3]);
                    Assert.AreEqual(3, h5ds[3,0]); Assert.AreEqual(4, h5ds[3,1]);
                    Assert.AreEqual(5, h5ds[3,2]); Assert.AreEqual(6, h5ds[3,3]);
                    
                    int[,] T1 = (int[,])h5ds.Read();
                    Assert.AreEqual(4, T1.GetLength(0));
                    Assert.AreEqual(4, T1.GetLength(1));
                    Assert.AreEqual(0, T1[0,0]); Assert.AreEqual(1, T1[0,1]);
                    Assert.AreEqual(2, T1[0,2]); Assert.AreEqual(3, T1[0,3]);
                    Assert.AreEqual(1, T1[1,0]); Assert.AreEqual(2, T1[1,1]);
                    Assert.AreEqual(3, T1[1,2]); Assert.AreEqual(4, T1[1,3]);
                    Assert.AreEqual(2, T1[2,0]); Assert.AreEqual(3, T1[2,1]);
                    Assert.AreEqual(4, T1[2,2]); Assert.AreEqual(5, T1[2,3]);
                    Assert.AreEqual(3, T1[3,0]); Assert.AreEqual(4, T1[3,1]);
                    Assert.AreEqual(5, T1[3,2]); Assert.AreEqual(6, T1[3,3]);
                    
                    using (Hdf5.Dataspace h5fs = h5ds.Space)
                    {
                        h5fs.SelectHyperslab(SelectOperation.Set, new long[] {0,1}, null, new long[] {4,1}, null);
                        Assert.IsTrue(h5fs.IsSelectionValid);
                        int[,] V1 = new int[4,1];
                        using (Hdf5.Dataspace h5ms = new Hdf5.Dataspace(new long[] {4,1}))
                            h5ds.Read(h5ms, h5fs, V1);
                        Assert.AreEqual(1, V1[0,0]);
                        Assert.AreEqual(2, V1[1,0]);
                        Assert.AreEqual(3, V1[2,0]);
                        Assert.AreEqual(4, V1[3,0]);
                    }
                    
                    using (Hdf5.Dataspace h5fs = h5ds.Space)
                    {
                        h5fs.SelectHyperslab(SelectOperation.Set, new long[] {2,1}, null, new long[] {1,3}, null);
                        Assert.IsTrue(h5fs.IsSelectionValid);
                        int[,] H2 = new int[1,3];
                        using (Hdf5.Dataspace h5ms = new Hdf5.Dataspace(new long[] {1,3}))
                            h5ds.Read(h5ms, h5fs, H2);
                        Assert.AreEqual(3, H2[0,0]);
                        Assert.AreEqual(4, H2[0,1]);
                        Assert.AreEqual(5, H2[0,2]);
                    }
                }
            } finally {
                System.IO.File.Delete(tmpfile);
            }
        }
        
        [Test]
        public void TestDataset_Double_1D()
        {
            string tmpfile = System.IO.Path.GetTempFileName();
            try
            {
                using (Hdf5.File h5file = Hdf5.File.Create(tmpfile, Hdf5.FileAccessFlags.Truncate))
                {
                    double[] data = new double[] { 0.0, 1.0, 6.0, 7.0, 2.0, 5.0};
                    using (Hdf5.Dataset<double> h5ds = Hdf5.Dataset<double>.CreateWithData(h5file, "T2", data));
                }
            
                using (Hdf5.File h5file = Hdf5.File.Open(tmpfile, Hdf5.FileAccessFlags.ReadOnly))
                using (Hdf5.Dataset<double> h5ds = Hdf5.Dataset<double>.Open(h5file, "T2"))
                {
                    using (Hdf5.Dataspace h5sp = h5ds.Space)
                    {
                        Assert.AreEqual(1, h5sp.NumDimensions);
                    }

                    Assert.AreEqual(0.0, h5ds[0]); Assert.AreEqual(1.0, h5ds[1]);
                    Assert.AreEqual(6.0, h5ds[2]); Assert.AreEqual(7.0, h5ds[3]);
                    Assert.AreEqual(2.0, h5ds[4]); Assert.AreEqual(5.0, h5ds[5]);

                    double[] T2 = (double[])h5ds.Read();
                    Assert.AreEqual(6, T2.GetLength(0));
                    Assert.AreEqual(0.0, T2[0]); Assert.AreEqual(1.0, T2[1]);
                    Assert.AreEqual(6.0, T2[2]); Assert.AreEqual(7.0, T2[3]);
                    Assert.AreEqual(2.0, T2[4]); Assert.AreEqual(5.0, T2[5]);

                    using (Hdf5.Dataspace h5fs = h5ds.Space)
                    {
                        h5fs.SelectHyperslab(SelectOperation.Set, new long[] {1}, null, new long[] {4}, null);
                        Assert.IsTrue(h5fs.IsSelectionValid);
                        double[] H1 = new double[4];
                        using (Hdf5.Dataspace h5ms = new Hdf5.Dataspace(new long[] {4}))
                            h5ds.Read(h5ms, h5fs, H1);
                        Assert.AreEqual(1.0, H1[0]);
                        Assert.AreEqual(6.0, H1[1]);
                        Assert.AreEqual(7.0, H1[2]);
                        Assert.AreEqual(2.0, H1[3]);
                    }
                }
            } finally {
                System.IO.File.Delete(tmpfile);
            }
        }
        
        [Test]
        public void TestDataset_Double_2D()
        {
            string tmpfile = System.IO.Path.GetTempFileName();
            try
            {
                using (Hdf5.File h5file = Hdf5.File.Create(tmpfile, Hdf5.FileAccessFlags.Truncate))
                {
                    double[,] data = new double[,] {{ 0.0,  1.0,  2.0,  3.0},
                                                    { 4.0,  5.0,  6.0,  7.0},
                                                    { 8.0,  9.0, 10.0, 11.0},
                                                    {12.0, 13.0, 14.0, 15.0}};
                    using (Hdf5.Dataset<double> h5ds = Hdf5.Dataset<double>.CreateWithData(h5file, "T2", data));
                }
            
                using (Hdf5.File h5file = Hdf5.File.Open(tmpfile, Hdf5.FileAccessFlags.ReadOnly))
                using (Hdf5.Dataset<double> h5ds = Hdf5.Dataset<double>.Open(h5file, "T2"))
                {
                    using (Hdf5.Dataspace h5sp = h5ds.Space)
                    {
                        Assert.AreEqual(2, h5sp.NumDimensions);
                    }

                    Assert.AreEqual( 0.0, h5ds[0,0]); Assert.AreEqual( 1.0, h5ds[0,1]);
                    Assert.AreEqual( 2.0, h5ds[0,2]); Assert.AreEqual( 3.0, h5ds[0,3]);
                    Assert.AreEqual( 4.0, h5ds[1,0]); Assert.AreEqual( 5.0, h5ds[1,1]);
                    Assert.AreEqual( 6.0, h5ds[1,2]); Assert.AreEqual( 7.0, h5ds[1,3]);
                    Assert.AreEqual( 8.0, h5ds[2,0]); Assert.AreEqual( 9.0, h5ds[2,1]);
                    Assert.AreEqual(10.0, h5ds[2,2]); Assert.AreEqual(11.0, h5ds[2,3]);
                    Assert.AreEqual(12.0, h5ds[3,0]); Assert.AreEqual(13.0, h5ds[3,1]);
                    Assert.AreEqual(14.0, h5ds[3,2]); Assert.AreEqual(15.0, h5ds[3,3]);

                    double[,] T2 = (double[,])h5ds.Read();
                    Assert.AreEqual(4, T2.GetLength(0));
                    Assert.AreEqual(4, T2.GetLength(1));
                    Assert.AreEqual( 0.0, T2[0,0]); Assert.AreEqual( 1.0, T2[0,1]);
                    Assert.AreEqual( 2.0, T2[0,2]); Assert.AreEqual( 3.0, T2[0,3]);
                    Assert.AreEqual( 4.0, T2[1,0]); Assert.AreEqual( 5.0, T2[1,1]);
                    Assert.AreEqual( 6.0, T2[1,2]); Assert.AreEqual( 7.0, T2[1,3]);
                    Assert.AreEqual( 8.0, T2[2,0]); Assert.AreEqual( 9.0, T2[2,1]);
                    Assert.AreEqual(10.0, T2[2,2]); Assert.AreEqual(11.0, T2[2,3]);
                    Assert.AreEqual(12.0, T2[3,0]); Assert.AreEqual(13.0, T2[3,1]);
                    Assert.AreEqual(14.0, T2[3,2]); Assert.AreEqual(15.0, T2[3,3]);

                    using (Hdf5.Dataspace h5fs = h5ds.Space)
                    {
                        h5fs.SelectHyperslab(SelectOperation.Set, new long[] {0,1}, null, new long[] {4,1}, null);
                        Assert.IsTrue(h5fs.IsSelectionValid);
                        double[,] V1 = new double[4,1];
                        using (Hdf5.Dataspace h5ms = new Hdf5.Dataspace(new long[] {4,1}))
                            h5ds.Read(h5ms, h5fs, V1);
                        Assert.AreEqual( 1.0, V1[0,0]);
                        Assert.AreEqual( 5.0, V1[1,0]);
                        Assert.AreEqual( 9.0, V1[2,0]);
                        Assert.AreEqual(13.0, V1[3,0]);
                    }
                    
                    using (Hdf5.Dataspace h5fs = h5ds.Space)
                    {
                        h5fs.SelectHyperslab(SelectOperation.Set, new long[] {2,1}, null, new long[] {1,3}, null);
                        Assert.IsTrue(h5fs.IsSelectionValid);
                        double[,] H2 = new double[1,3];
                        using (Hdf5.Dataspace h5ms = new Hdf5.Dataspace(new long[] {1,3}))
                            h5ds.Read(h5ms, h5fs, H2);
                        Assert.AreEqual( 9.0, H2[0,0]);
                        Assert.AreEqual(10.0, H2[0,1]);
                        Assert.AreEqual(11.0, H2[0,2]);
                    }
                }
            } finally {
                System.IO.File.Delete(tmpfile);
            }
        }
        
        [Test]
        public void TestDataset_Double_Var()
        {
            string tmpfile = System.IO.Path.GetTempFileName();
            try
            {
                using (Hdf5.File h5file = Hdf5.File.Create(tmpfile, Hdf5.FileAccessFlags.Truncate))
                {
                    double[][] data = new double[][] {new double[] { 0.0},
                                                      new double[] { 1.1,  2.2},
                                                      new double[] {},
                                                      new double[] { 3.3,  4.4,  5.5},
                                                      new double[] { 6.6,  7.7,  8.8,  9.9}};
                    using (Hdf5.Dataset<double[]> h5ds = Hdf5.Dataset<double[]>.CreateWithData(h5file, "T3", data));
                }
            
                using (Hdf5.File h5file = Hdf5.File.Open(tmpfile, Hdf5.FileAccessFlags.ReadOnly))
                using (Hdf5.Dataset<double[]> h5ds = Hdf5.Dataset<double[]>.Open(h5file, "T3"))
                {
                    using (Hdf5.Dataspace h5sp = h5ds.Space)
                    {
                        Assert.AreEqual(1, h5sp.NumDimensions);
                    }
                    double[][] T3 = (double[][])h5ds.Read();
                    Assert.AreEqual(5, T3.Length);
                    Assert.AreEqual(1, T3[0].Length);
                    Assert.AreEqual(0.0, T3[0][0]);
                    Assert.AreEqual(2, T3[1].Length);
                    Assert.AreEqual(1.1, T3[1][0]);
                    Assert.AreEqual(2.2, T3[1][1]);
                    Assert.AreEqual(0, T3[2].Length);
                    Assert.AreEqual(3, T3[3].Length);
                    Assert.AreEqual(3.3, T3[3][0]);
                    Assert.AreEqual(4.4, T3[3][1]);
                    Assert.AreEqual(5.5, T3[3][2]);
                    Assert.AreEqual(4, T3[4].Length);
                    Assert.AreEqual(6.6, T3[4][0]);
                    Assert.AreEqual(7.7, T3[4][1]);
                    Assert.AreEqual(8.8, T3[4][2]);
                    Assert.AreEqual(9.9, T3[4][3]);
                }
            } finally {
                System.IO.File.Delete(tmpfile);
            }
        }
        
        [Test]
        public void TestDataset_Struct_1D()
        {
            string tmpfile = System.IO.Path.GetTempFileName();
            try
            {
                using (Hdf5.File h5file = Hdf5.File.Create(tmpfile, Hdf5.FileAccessFlags.Truncate))
                {
                    Triplet[] data = new Triplet[] {new Triplet(0, 0, 5.0),
                                                    new Triplet(0, 1, 3.0),
                                                    new Triplet(1, 0, 8.0),
                                                    new Triplet(1, 2, 1.0),
                                                    new Triplet(2, 0, 6.0),
                                                    new Triplet(2, 3, 9.0),
                                                    new Triplet(3, 0, 2.0),
                                                    new Triplet(3, 4, 4.0)};
                    using (Hdf5.Dataset<Triplet> h5ds = Hdf5.Dataset<Triplet>.CreateWithData(h5file, "T4", data));
                }
            
                using (Hdf5.File h5file = Hdf5.File.Open(tmpfile, Hdf5.FileAccessFlags.ReadOnly))
                using (Hdf5.Dataset<Triplet> h5ds = Hdf5.Dataset<Triplet>.Open(h5file, "T4"))
                {
                    using (Hdf5.Dataspace h5sp = h5ds.Space)
                    {
                        Assert.AreEqual(1, h5sp.NumDimensions);
                    }

                    Assert.AreEqual(  0, h5ds[0].i); Assert.AreEqual(  0, h5ds[0].j); Assert.AreEqual(5.0, h5ds[0].v);
                    Assert.AreEqual(  0, h5ds[1].i); Assert.AreEqual(  1, h5ds[1].j); Assert.AreEqual(3.0, h5ds[1].v);
                    Assert.AreEqual(  1, h5ds[2].i); Assert.AreEqual(  0, h5ds[2].j); Assert.AreEqual(8.0, h5ds[2].v);
                    Assert.AreEqual(  1, h5ds[3].i); Assert.AreEqual(  2, h5ds[3].j); Assert.AreEqual(1.0, h5ds[3].v);
                    Assert.AreEqual(  2, h5ds[4].i); Assert.AreEqual(  0, h5ds[4].j); Assert.AreEqual(6.0, h5ds[4].v);
                    Assert.AreEqual(  2, h5ds[5].i); Assert.AreEqual(  3, h5ds[5].j); Assert.AreEqual(9.0, h5ds[5].v);
                    Assert.AreEqual(  3, h5ds[6].i); Assert.AreEqual(  0, h5ds[6].j); Assert.AreEqual(2.0, h5ds[6].v);
                    Assert.AreEqual(  3, h5ds[7].i); Assert.AreEqual(  4, h5ds[7].j); Assert.AreEqual(4.0, h5ds[7].v);

                    Triplet[] T4 = (Triplet[])h5ds.Read();
                    Assert.AreEqual(8, T4.Length);
                    Assert.AreEqual(  0, T4[0].i); Assert.AreEqual(  0, T4[0].j); Assert.AreEqual(5.0, T4[0].v);
                    Assert.AreEqual(  0, T4[1].i); Assert.AreEqual(  1, T4[1].j); Assert.AreEqual(3.0, T4[1].v);
                    Assert.AreEqual(  1, T4[2].i); Assert.AreEqual(  0, T4[2].j); Assert.AreEqual(8.0, T4[2].v);
                    Assert.AreEqual(  1, T4[3].i); Assert.AreEqual(  2, T4[3].j); Assert.AreEqual(1.0, T4[3].v);
                    Assert.AreEqual(  2, T4[4].i); Assert.AreEqual(  0, T4[4].j); Assert.AreEqual(6.0, T4[4].v);
                    Assert.AreEqual(  2, T4[5].i); Assert.AreEqual(  3, T4[5].j); Assert.AreEqual(9.0, T4[5].v);
                    Assert.AreEqual(  3, T4[6].i); Assert.AreEqual(  0, T4[6].j); Assert.AreEqual(2.0, T4[6].v);
                    Assert.AreEqual(  3, T4[7].i); Assert.AreEqual(  4, T4[7].j); Assert.AreEqual(4.0, T4[7].v);
                }
            } finally {
                System.IO.File.Delete(tmpfile);
            }
        }
        
        [Test]
        public void TestDataset_String_1D()
        {
            string tmpfile = System.IO.Path.GetTempFileName();
            try
            {
                using (Hdf5.File h5file = Hdf5.File.Create(tmpfile, Hdf5.FileAccessFlags.Truncate))
                {
                    string[] data = new string[] {"String 1",
                                                  "Some other string",
                                                  "",
                                                  "Third string which is quite a bit longer",
                                                  "Last!"};
                    using (Hdf5.Dataset<string> h5ds = Hdf5.Dataset<string>.CreateWithData(h5file, "S1", data));
                }
            
                using (Hdf5.File h5file = Hdf5.File.Open(tmpfile, Hdf5.FileAccessFlags.ReadOnly))
                using (Hdf5.Dataset<string> h5ds = Hdf5.Dataset<string>.Open(h5file, "S1"))
                {
                    using (Hdf5.Dataspace h5sp = h5ds.Space)
                    {
                        Assert.AreEqual(1, h5sp.NumDimensions);
                    }

                    Assert.AreEqual("String 1", h5ds[0]);
                    Assert.AreEqual("Some other string", h5ds[1]);
                    Assert.AreEqual("", h5ds[2]);
                    Assert.AreEqual("Third string which is quite a bit longer", h5ds[3]);
                    Assert.AreEqual("Last!", h5ds[4]);

                    string[] S1 = (string[])h5ds.Read();
                    Assert.AreEqual(5, S1.Length);
                    Assert.AreEqual("String 1", S1[0]);
                    Assert.AreEqual("Some other string", S1[1]);
                    Assert.AreEqual("", S1[2]);
                    Assert.AreEqual("Third string which is quite a bit longer", S1[3]);
                    Assert.AreEqual("Last!", S1[4]);
                }
            } finally {
                System.IO.File.Delete(tmpfile);
            }
        }
        
/*        [Test]
        public void TestDataset_String_Var2D()
        {
            string tmpfile = System.IO.Path.GetTempFileName();
            try
            {
                using (Hdf5.File h5file = Hdf5.File.Create(tmpfile, Hdf5.FileAccessFlags.Truncate))
                {
                    string[][] data = new string[][] {new string[] {"S(1,1)", "S(1,2)"},
                                                      new string[] {"S(2,1)"},
                                                      new string[] {},
                                                      null,
                                                      new string[] {"S(5,1)", null, "", "S(5,4)"},
                                                      new string[] {"S(6,1)", "S(6,2)", "S(6,3)"}};
                    using (Hdf5.Dataset h5ds = Hdf5.Dataset.CreateWithData(h5file, "Sx", data));
                }
                
                using (Hdf5.File h5file = Hdf5.File.Open(tmpfile, Hdf5.FileAccessFlags.ReadOnly))
                using (Hdf5.Dataset h5ds = Hdf5.Dataset.Open(h5file, "Sx"))
                {
                    using (Hdf5.Dataspace h5sp = h5ds.Space)
                    {
                        Assert.AreEqual(1, h5sp.NumDimensions);
                    }
                    string[][] Sx = h5ds.ReadStringVlenArray();
                    Assert.AreEqual(6, Sx.Length);
                    
                    Assert.AreEqual(2, Sx[0].Length);
                    Assert.AreEqual("S(1,1)", Sx[0][0]);
                    Assert.AreEqual("S(1,2)", Sx[0][1]);
                    
                    Assert.AreEqual(1, Sx[1].Length);
                    Assert.AreEqual("S(2,1)", Sx[1][0]);
                    
                    Assert.IsNull(Sx[2]);
                    Assert.IsNull(Sx[3]);
                    
                    Assert.AreEqual(4, Sx[4].Length);
                    Assert.AreEqual("S(5,1)", Sx[4][0]);
                    Assert.AreEqual(null, Sx[4][1]);
                    Assert.AreEqual("", Sx[4][2]);
                    Assert.AreEqual("S(5,4)", Sx[4][3]);
                    
                    Assert.AreEqual(3, Sx[5].Length);
                    Assert.AreEqual("S(6,1)", Sx[5][0]);
                    Assert.AreEqual("S(6,2)", Sx[5][1]);
                    Assert.AreEqual("S(6,3)", Sx[5][2]);
                }
            } finally {
                System.IO.File.Delete(tmpfile);
            }
        }
*/        
        [Test]
        public void TestDataset_String_2D()
        {
            string tmpfile = System.IO.Path.GetTempFileName();
            try
            {
                using (Hdf5.File h5file = Hdf5.File.Create(tmpfile, Hdf5.FileAccessFlags.Truncate))
                {
                    string[,] data = new string[,] {{"S(1,1)", "S(1,2)"},
                                                    {"S(2,1)", "S(2,2)"}};
                    using (Hdf5.Dataset<string> h5ds = Hdf5.Dataset<string>.CreateWithData(h5file, "S2", data));
                }
            
                using (Hdf5.File h5file = Hdf5.File.Open(tmpfile, Hdf5.FileAccessFlags.ReadOnly))
                using (Hdf5.Dataset<string> h5ds = Hdf5.Dataset<string>.Open(h5file, "S2"))
                {
                    using (Hdf5.Dataspace h5sp = h5ds.Space)
                    {
                        Assert.AreEqual(2, h5sp.NumDimensions);
                    }
                    string[,] S2 = (string[,])h5ds.Read();
                    Assert.AreEqual(2, S2.GetLength(0));
                    Assert.AreEqual(2, S2.GetLength(1));
                    Assert.AreEqual("S(1,1)", S2[0,0]);
                    Assert.AreEqual("S(1,2)", S2[0,1]);
                    Assert.AreEqual("S(2,1)", S2[1,0]);
                    Assert.AreEqual("S(2,2)", S2[1,1]);
                }
            } finally {
                System.IO.File.Delete(tmpfile);
            }
        }

        [Test]
        public void TestDataset_BitField_1D()
        {
            string tmpfile = System.IO.Path.GetTempFileName();
            try
            {
                using (Hdf5.File h5file = Hdf5.File.Create(tmpfile, Hdf5.FileAccessFlags.Truncate))
                {
                    BitVector32[] data = new BitVector32[] {new BitVector32(0x235897), new BitVector32(0x01234A)};
                    using (Hdf5.Dataset<BitVector32> h5ds = Hdf5.Dataset<BitVector32>.CreateWithData(h5file, "BA1", data));
                }
            
                using (Hdf5.File h5file = Hdf5.File.Open(tmpfile, Hdf5.FileAccessFlags.ReadOnly))
                using (Hdf5.Dataset<BitVector32> h5ds = Hdf5.Dataset<BitVector32>.Open(h5file, "BA1"))
                {
                    using (Hdf5.Dataspace h5sp = h5ds.Space)
                    {
                        Assert.AreEqual(1, h5sp.NumDimensions);
                    }
                    BitVector32[] data = (BitVector32[])h5ds.Read();
                    Assert.AreEqual(2, data.Length);

                    Assert.AreEqual(true,  data[0][1<< 0]); Assert.AreEqual(true,  data[0][1<< 1]);
                    Assert.AreEqual(true,  data[0][1<< 2]); Assert.AreEqual(false, data[0][1<< 3]);
                    Assert.AreEqual(true,  data[0][1<< 4]); Assert.AreEqual(false, data[0][1<< 5]);
                    Assert.AreEqual(false, data[0][1<< 6]); Assert.AreEqual(true,  data[0][1<< 7]);
                    
                    Assert.AreEqual(false, data[0][1<< 8]); Assert.AreEqual(false, data[0][1<< 9]);
                    Assert.AreEqual(false, data[0][1<<10]); Assert.AreEqual(true,  data[0][1<<11]);
                    Assert.AreEqual(true,  data[0][1<<12]); Assert.AreEqual(false, data[0][1<<13]);
                    Assert.AreEqual(true,  data[0][1<<14]); Assert.AreEqual(false, data[0][1<<15]);
                    
                    Assert.AreEqual(true,  data[0][1<<16]); Assert.AreEqual(true,  data[0][1<<17]);
                    Assert.AreEqual(false, data[0][1<<18]); Assert.AreEqual(false, data[0][1<<19]);
                    Assert.AreEqual(false, data[0][1<<20]); Assert.AreEqual(true,  data[0][1<<21]);
                    Assert.AreEqual(false, data[0][1<<22]); Assert.AreEqual(false, data[0][1<<23]);
                    
                    Assert.AreEqual(false, data[0][1<<24]); Assert.AreEqual(false, data[0][1<<25]);
                    Assert.AreEqual(false, data[0][1<<26]); Assert.AreEqual(false, data[0][1<<27]);
                    Assert.AreEqual(false, data[0][1<<28]); Assert.AreEqual(false, data[0][1<<29]);
                    Assert.AreEqual(false, data[0][1<<30]); Assert.AreEqual(false, data[0][1<<31]);

                    Assert.AreEqual(false, data[1][1<< 0]); Assert.AreEqual(true,  data[1][1<< 1]);
                    Assert.AreEqual(false, data[1][1<< 2]); Assert.AreEqual(true,  data[1][1<< 3]);
                    Assert.AreEqual(false, data[1][1<< 4]); Assert.AreEqual(false, data[1][1<< 5]);
                    Assert.AreEqual(true,  data[1][1<< 6]); Assert.AreEqual(false, data[1][1<< 7]);
                    
                    Assert.AreEqual(true,  data[1][1<< 8]); Assert.AreEqual(true,  data[1][1<< 9]);
                    Assert.AreEqual(false, data[1][1<<10]); Assert.AreEqual(false, data[1][1<<11]);
                    Assert.AreEqual(false, data[1][1<<12]); Assert.AreEqual(true,  data[1][1<<13]);
                    Assert.AreEqual(false, data[1][1<<14]); Assert.AreEqual(false, data[1][1<<15]);
                    
                    Assert.AreEqual(true,  data[1][1<<16]); Assert.AreEqual(false, data[1][1<<17]);
                    Assert.AreEqual(false, data[1][1<<18]); Assert.AreEqual(false, data[1][1<<19]);
                    Assert.AreEqual(false, data[1][1<<20]); Assert.AreEqual(false, data[1][1<<21]);
                    Assert.AreEqual(false, data[1][1<<22]); Assert.AreEqual(false, data[1][1<<23]);
                    
                    Assert.AreEqual(false, data[1][1<<24]); Assert.AreEqual(false, data[1][1<<25]);
                    Assert.AreEqual(false, data[1][1<<26]); Assert.AreEqual(false, data[1][1<<27]);
                    Assert.AreEqual(false, data[1][1<<28]); Assert.AreEqual(false, data[1][1<<29]);
                    Assert.AreEqual(false, data[1][1<<30]); Assert.AreEqual(false, data[1][1<<31]);
                }
            } finally {
                System.IO.File.Delete(tmpfile);
            }
        }
    }
}
