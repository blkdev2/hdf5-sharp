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
        private struct Triplet
        {
            public int i;
            public int j;
            public double v;
            public Triplet(int i, int j, double v) { this.i=i; this.j=j; this.v=v; }
        }
        
        public static void Main(string[] args)
        {
            //H5.CheckVersion(1, 6, 4);
            uint maj, min, rel;
            H5.GetVersion(out maj, out min, out rel);
            Console.WriteLine("HDF5 library version {0}.{1}.{2}", maj, min, rel);
            
            File f = File.Create("first_test.h5", FileAccessFlags.Truncate);
            Group g1 = Group.Create(f, "/G1");
            Group g2 = Group.Create(f, "/G2");
            Console.WriteLine("Create file first_test.h5");
            Dataset<int> ds1, ds2;
            Dataset<string> ds3, ds4;
            Dataset<double> ds5;
            Dataset<Triplet> ds6;
            //Dataset<ulong> ds7;
            //Dataset<string> ds8;
//            ds = Dataset.CreateFromData<int>(f, "T1", new int[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9});
            Console.Write("Adding dataset ds1... ");
            ds1 = Dataset<int>.CreateWithData(f, "T1", new int[,] {{0, 1, 2, 3},
                                                                   {1, 2, 3, 4},
                                                                   {2, 3, 4, 5},
                                                                   {3, 4, 5, 6}});
            Console.WriteLine("done.");
            Console.Write("Adding dataset ds2... ");
            ds2 = Dataset<int>.CreateWithData(f, "T2", new double[,] {{ 0.0,  1.0,  2.0,  3.0},
                                                                         { 4.0,  5.0,  6.0,  7.0},
                                                                         { 8.0,  9.0, 10.0, 11.0},
                                                                         {12.0, 13.0, 14.0, 15.0}});
            Console.WriteLine("done.");
            Console.Write("Adding dataset ds3... ");
            ds3 = Dataset<string>.CreateWithData(g1, "S1", new string[] {"String 1",
                                                                 "Some other string",
                                                                 "Third string which is quite a bit longer",
                                                                 "Last!"});
            Console.WriteLine("done.");
            Console.Write("Adding dataset ds4... ");
            ds4 = Dataset<string>.CreateWithData(g1, "S2", new string[,] {{"S(1,1)", "S(1,2)"},
                                                                  {"S(2,1)", "S(2,2)"}});
            Console.WriteLine("done.");
            Console.Write("Adding dataset ds5... ");
            ds5 = Dataset<double>.CreateWithData(f, "T3", new double[][] {new double[] { 0.0},
                                                                          new double[] { 1.1,  2.2},
                                                                          new double[] {},
                                                                          new double[] { 3.3,  4.4,  5.5},
                                                                          new double[] { 6.6,  7.7,  8.8,  9.9}});
            Console.WriteLine("done.");
            Console.Write("Adding dataset ds6... ");
            ds6 = Dataset<Triplet>.CreateWithData(f, "T4", new Triplet[] {new Triplet(0, 0, 5.0),
                                                                          new Triplet(0, 1, 3.0),
                                                                          new Triplet(1, 0, 8.0),
                                                                          new Triplet(1, 2, 1.0),
                                                                          new Triplet(2, 0, 6.0),
                                                                          new Triplet(2, 3, 9.0),
                                                                          new Triplet(3, 0, 2.0),
                                                                          new Triplet(3, 4, 4.0)});
            Console.WriteLine("done.");
            //Console.Write("Adding dataset ds7... ");
            //ds7 = Dataset<ulong>.CreateWithData(g2, "SC1", 123456);
            //Console.WriteLine("done.");
            //Console.Write("Adding dataset ds8... ");
            //ds8 = Dataset<string>.CreateWithData(g2, "SC2", "scalar string.");
            //Console.WriteLine("done.");
            //Attribute<sbyte> a7;
            //Attribute<string> a8;
            //Console.Write("Adding attribute to dataset ds7... ");
            //a7 = Attribute<sbyte>.CreateWithData(ds7, "A7", 1);
            //Console.WriteLine("done.");
            //Console.Write("Adding attribute to dataset ds8... ");
            //a8 = Attribute<string>.CreateWithData(ds8, "A8", "abc");
            //Console.WriteLine("done.");
            
            Console.WriteLine("\nNumber of objects: {0}", f.NumObjects);
            
            Console.WriteLine("\nds1:");
            int[,] T1 = (int[,])ds1.Read();
            for (int i=0; i<T1.GetLength(0); i++)
            {
                for (int j=0; j<T1.GetLength(1); j++)
                    Console.Write("{0} ", T1[i,j]);
                Console.WriteLine();
            }
            Console.WriteLine("\nds2:");
            double[,] T2 = (double[,])ds2.Read();
            for (int i=0; i<T2.GetLength(0); i++)
            {
                for (int j=0; j<T2.GetLength(1); j++)
                    Console.Write("{0,2} ", T2[i,j]);
                Console.WriteLine();
            }
            
            Console.WriteLine("\nds3:");
            string[] S1 = (string[])ds3.Read();
            for (int i=0; i<S1.GetLength(0); i++)
            {
                Console.WriteLine(S1[i]);
            }
            
            Console.WriteLine("\nds4:");
            string[,] S2 = (string[,])ds4.Read();
            for (int i=0; i<S2.GetLength(0); i++)
            {
                for (int j=0; j<S2.GetLength(1); j++)
                    Console.WriteLine("({0},{1}): {2}", i, j, S2[i,j]);
            }
            
            Console.WriteLine("\nds5:");
            double[][] T3 = (double[][])ds5.Read();
            for (int i=0; i<T3.Length; i++)
            {
                for (int j=0; j<T3[i].Length; j++)
                    Console.WriteLine("({0},{1}): {2}", i, j, T3[i][j]);
            }
            
            Console.WriteLine("\nds6:");
            Triplet[] T4 = (Triplet[])ds6.Read();
            for (int i=0; i<T4.Length; i++)
            {
                Console.WriteLine("({0},{1}): {2}", T4[i].i, T4[i].j, T4[i].v);
            }
            
//            Console.WriteLine("\nds7:");
//            //ulong SC1 = ds7.Read();
//            Console.WriteLine(SC1);
//            Console.WriteLine("\nds7, attribute A7:");
//            sbyte a7b = a7.Read();
//            Console.WriteLine(a7b);
//            
//            Console.WriteLine("\nds8:");
//            string SC2 = ds8.Read();
//            Console.WriteLine(SC2);
//            Console.WriteLine("\nds8, attribute A8:");
//            string a8s = a8.Read();
//            Console.WriteLine("{0}", a8s);
            
            ds1.Close();
            ds2.Close();
            ds3.Close();
            ds4.Close();
            ds5.Close();
            ds6.Close();
            //ds7.Close();
            //ds8.Close();
            g1.Close();
            g2.Close();
            f.Close();
        }
    }
}