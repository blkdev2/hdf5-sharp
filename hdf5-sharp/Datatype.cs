// Datatype.cs created with MonoDevelop
// User: uhrm at 20:58 11/12/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.Runtime.InteropServices;

namespace Hdf5
{

    public enum DatatypeClass
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

    public class Datatype : Base
    {
        private static IntPtr hdf5_handle;

        private static IntPtr H5T_STD_I8LE_g;
        private static IntPtr H5T_STD_I16LE_g;
        private static IntPtr H5T_STD_I32LE_g;
        private static IntPtr H5T_STD_I64LE_g;
        private static IntPtr H5T_STD_U8LE_g;
        private static IntPtr H5T_STD_U16LE_g;
        private static IntPtr H5T_STD_U32LE_g;
        private static IntPtr H5T_STD_U64LE_g;
        private static IntPtr H5T_STD_B8LE_g;
        private static IntPtr H5T_STD_B16LE_g;
        private static IntPtr H5T_STD_B32LE_g;
        private static IntPtr H5T_STD_B64LE_g;
        private static IntPtr H5T_STD_REF_OBJ_g;
        private static IntPtr H5T_STD_REF_DSETREG_g;
        private static IntPtr H5T_IEEE_F32LE_g;
        private static IntPtr H5T_IEEE_F64LE_g;
//        private static IntPtr H5T_NATIVE_CHAR_g;
        private static IntPtr H5T_C_S1_g;
        
        private bool can_close;
        
        static Datatype()
        {
            H5.Open();
            hdf5_handle = dlopen(null, RTLD_LAZY);
            if (hdf5_handle == IntPtr.Zero)
                throw new ApplicationException(String.Format("Error loading libhdf5. Reason {0}", Marshal.PtrToStringAnsi(dlerror())));
            H5T_STD_I8LE_g        = dlsym(hdf5_handle, "H5T_STD_I8LE_g");
            H5T_STD_I16LE_g       = dlsym(hdf5_handle, "H5T_STD_I16LE_g");
            H5T_STD_I32LE_g       = dlsym(hdf5_handle, "H5T_STD_I32LE_g");
            H5T_STD_I64LE_g       = dlsym(hdf5_handle, "H5T_STD_I64LE_g");
            H5T_STD_U8LE_g        = dlsym(hdf5_handle, "H5T_STD_U8LE_g");
            H5T_STD_U16LE_g       = dlsym(hdf5_handle, "H5T_STD_U16LE_g");
            H5T_STD_U32LE_g       = dlsym(hdf5_handle, "H5T_STD_U32LE_g");
            H5T_STD_U64LE_g       = dlsym(hdf5_handle, "H5T_STD_U64LE_g");
            H5T_STD_B8LE_g        = dlsym(hdf5_handle, "H5T_STD_B8LE_g");
            H5T_STD_B16LE_g       = dlsym(hdf5_handle, "H5T_STD_B16LE_g");
            H5T_STD_B32LE_g       = dlsym(hdf5_handle, "H5T_STD_B32LE_g");
            H5T_STD_B64LE_g       = dlsym(hdf5_handle, "H5T_STD_B64LE_g");
            H5T_STD_REF_OBJ_g     = dlsym(hdf5_handle, "H5T_STD_REF_OBJ_g");
            H5T_STD_REF_DSETREG_g = dlsym(hdf5_handle, "H5T_STD_REF_DSETREG_g");
            H5T_IEEE_F32LE_g      = dlsym(hdf5_handle, "H5T_IEEE_F32LE_g");
            H5T_IEEE_F64LE_g      = dlsym(hdf5_handle, "H5T_IEEE_F64LE_g");
//            H5T_NATIVE_CHAR_g     = dlsym(hdf5_handle, "H5T_NATIVE_CHAR_g");
            H5T_C_S1_g            = dlsym(hdf5_handle, "H5T_C_S1_g");
            STD_I8LE        = new Datatype(Marshal.ReadInt32(H5T_STD_I8LE_g),        false);
            STD_I16LE       = new Datatype(Marshal.ReadInt32(H5T_STD_I16LE_g),       false);
            STD_I32LE       = new Datatype(Marshal.ReadInt32(H5T_STD_I32LE_g),       false);
            STD_I64LE       = new Datatype(Marshal.ReadInt32(H5T_STD_I64LE_g),       false);
            STD_U8LE        = new Datatype(Marshal.ReadInt32(H5T_STD_U8LE_g),        false);
            STD_U16LE       = new Datatype(Marshal.ReadInt32(H5T_STD_U16LE_g),       false);
            STD_U32LE       = new Datatype(Marshal.ReadInt32(H5T_STD_U32LE_g),       false);
            STD_U64LE       = new Datatype(Marshal.ReadInt32(H5T_STD_U64LE_g),       false);
            STD_B8LE        = new Datatype(Marshal.ReadInt32(H5T_STD_B8LE_g),        false);
            STD_B16LE       = new Datatype(Marshal.ReadInt32(H5T_STD_B16LE_g),       false);
            STD_B32LE       = new Datatype(Marshal.ReadInt32(H5T_STD_B32LE_g),       false);
            STD_B64LE       = new Datatype(Marshal.ReadInt32(H5T_STD_B64LE_g),       false);
            STD_REF_OBJ     = new Datatype(Marshal.ReadInt32(H5T_STD_REF_OBJ_g),     false);
            STD_REF_DSETREG = new Datatype(Marshal.ReadInt32(H5T_STD_REF_DSETREG_g), false);
            IEEE_F32LE      = new Datatype(Marshal.ReadInt32(H5T_IEEE_F32LE_g),      false);
            IEEE_F64LE      = new Datatype(Marshal.ReadInt32(H5T_IEEE_F64LE_g),      false);
//            NATIVE_CHAR     = new Datatype(Marshal.ReadInt32(H5T_NATIVE_CHAR_g),     false);
            C_S1            = new Datatype(Marshal.ReadInt32(H5T_C_S1_g),            false);
            CUSTOM_STRING   = C_S1.Copy();
            CUSTOM_STRING.Size = -1; // variable
            dlclose(hdf5_handle);
        }
        
        internal Datatype(int raw, bool can_close) : base(raw)
        {
            this.can_close = can_close;
        }
        
        public Datatype Copy()
        {
            return new Datatype(H5Tcopy(raw), true);
        }
        
        public DatatypeClass Class
        {
            get { return H5Tget_class(raw); }
        }
        
        public long Size
        {
            get { return H5Tget_size(raw); }
            set
            {
                int err = H5Tset_size(raw, value);
                if (err < 0)
                    throw new ApplicationException(String.Format("Error setting size to {0}.", value));
            }
        }
        
        public bool IsVariableString
        {
            get
            {
                int err = H5Tis_variable_str(raw);
                if (err < 0)
                    throw new ApplicationException("Error determining whether datatype is variable length string.");
                return err > 0;
            }
        }
        
        // IDisposable stuff
        
        protected override void Dispose (bool disposing)
        {
            if (can_close)
                H5Tclose(raw);
        }

        
        // built-in data types
        
        public static Datatype STD_I8LE;
        public static Datatype STD_I16LE;
        public static Datatype STD_I32LE;
        public static Datatype STD_I64LE;
        public static Datatype STD_U8LE;
        public static Datatype STD_U16LE;
        public static Datatype STD_U32LE;
        public static Datatype STD_U64LE;
        public static Datatype STD_B8LE;
        public static Datatype STD_B16LE;
        public static Datatype STD_B32LE;
        public static Datatype STD_B64LE;
        public static Datatype STD_REF_OBJ;
        public static Datatype STD_REF_DSETREG;
        public static Datatype IEEE_F32LE;
        public static Datatype IEEE_F64LE;
//        public static Datatype NATIVE_CHAR;
        public static Datatype C_S1;
        public static Datatype CUSTOM_STRING;
        
        internal static Datatype Lookup(Type t)
        {
            if (t == typeof(sbyte))
                return STD_I8LE;
            else if (t == typeof(short))
                return STD_I16LE;
            else if (t == typeof(int))
                return STD_I32LE;
            else if (t == typeof(long))
                return STD_I64LE;
            else if (t == typeof(byte))
                return STD_U8LE;
            else if (t == typeof(ushort))
                return STD_U16LE;
            else if (t == typeof(uint))
                return STD_U32LE;
            else if (t == typeof(ulong))
                return STD_U64LE;
            else if (t == typeof(float))
                return IEEE_F32LE;
            else if (t == typeof(double))
                return IEEE_F64LE;
            else if (t == typeof(string))
                return CUSTOM_STRING;
            throw new ArgumentException(String.Format("Unsupported type {0}", t));
        }
        
        public static Datatype VariableLength<T>() where T : struct
        {
            int id = H5Tvlen_create(Lookup(typeof(T)).raw);
            if (id < 0)
                throw new ApplicationException(String.Format("Error creating variable length type (base {0})", typeof(T)));
            return new Datatype(id, true);
        }
        
        // dl import
        
        private static readonly int RTLD_LAZY = 1;
        
        [DllImport("dl")]
        private static extern IntPtr dlopen(string filename, int flags);
        
        [DllImport("dl")]
        private static extern int dlclose(IntPtr handle);
        
        [DllImport("dl")]
        private static extern IntPtr dlsym(IntPtr handle, string symbol);
        
        [DllImport("dl")]
        private static extern IntPtr dlerror();
        
        // hdf5 import
        
        [DllImport("hdf5")]
        private static extern int H5Tvlen_create(int base_type_id);
        
        [DllImport("hdf5")]
        private static extern int H5Tcopy(int type_id);
        
        [DllImport("hdf5")]
        private static extern int H5Tclose(int type_id);
        
        [DllImport("hdf5")]
        private static extern DatatypeClass H5Tget_class(int type_id);
        
        [DllImport("hdf5")]
        private static extern long H5Tget_size(int type_id);
        
        [DllImport("hdf5")]
        private static extern int H5Tset_size(int type_id, long size);
        
        [DllImport("hdf5")]
        private static extern int H5Tis_variable_str(int type_id);
    }
}
