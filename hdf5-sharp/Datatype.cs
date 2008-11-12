// Datatype.cs created with MonoDevelop
// User: uhrm at 20:58 11/12/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.Runtime.InteropServices;

namespace Hdf5
{

    public enum DataClass
    {
        NoClass          = -1,  /* error                                      */
        Integer          = 0,   /* integer types                              */
        Float            = 1,   /* floating-point types                       */
        Time             = 2,   /* date and time types                        */
        String           = 3,   /* character string types                     */
        Bitfield         = 4,   /* bit field types                            */
        Opaque           = 5,   /* opaque types                               */
        Compound         = 6,   /* compound types                             */
        Reference        = 7,   /* reference types                            */
        Enum             = 8,   /* enumeration types                          */
        Vlen             = 9,   /* Variable-Length types                      */
        Array            = 10   /* Array types                                */
    }

    public enum ByteOrder
    {
        Error         = -1,  /* error                                      */
        LittleEndian  = 0,   /* little endian                              */
        BigEndian     = 1,   /* bit endian                                 */
        Vax           = 2,   /* VAX mixed endian                           */
        None          = 3    /* no particular order (strings, bits,..)     */
    }
    
    public enum SignScheme
    {
        Error   = -1,  /* error                                      */
        None    = 0,   /* this is an unsigned type                   */
        Twos    = 1,   /* two's complement                           */
    }

    public enum H5Type
    {
         STD_I8BE,
         STD_I8LE,
         STD_I16BE,
         STD_I16LE,
         STD_I32BE,
         STD_I32LE,
         STD_I64BE,
         STD_I64LE,
         STD_U8BE,
         STD_U8LE,
         STD_U16BE,
         STD_U16LE,
         STD_U32BE,
         STD_U32LE,
         STD_U64BE,
         STD_U64LE,
         STD_B8BE,
         STD_B8LE,
         STD_B16BE,
         STD_B16LE,
         STD_B32BE,
         STD_B32LE,
         STD_B64BE,
         STD_B64LE,
         STD_REF_OBJ,
         STD_REF_DSETREG
    }

    
    public class Datatype : Base
    {
        private static IntPtr hdf5_handle;
        private static IntPtr H5T_STD_U32LE_g;
        
        private bool can_close;
        
        static Datatype()
        {
            hdf5_handle = dlopen("dl", RTLD_LAZY);
            H5T_STD_U32LE_g = dlsym(hdf5_handle, "H5T_STD_U32LE_g");
            STD_U32LE = new Datatype(Marshal.ReadInt32(H5T_STD_U32LE_g),false);
        }
        
        internal Datatype(int raw, bool can_close) : base(raw)
        {
            this.can_close = can_close;
        }
        
        // IDisposable stuff
        
        protected override void Dispose (bool disposing)
        {
            if (can_close)
                H5Tclose(raw);
        }

        
        // built-in data types
        
        public static Datatype STD_U32LE;
        
        // hdf5 import
        
        [DllImport("hdf5")]
        private static extern int H5Tclose(int type_id);
        
        // dl import
        
        private static readonly int RTLD_LAZY = 1;
        
        [DllImport("dl")]
        private static extern IntPtr dlopen(string filename, int flags);
        
        [DllImport("dl")]
        private static extern IntPtr dlsym(IntPtr handle, string symbol);
    }
}
