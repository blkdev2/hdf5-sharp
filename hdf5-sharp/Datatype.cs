//  
// Datatype.cs
// hdf-sharp
//
// Copyright 2008-2010 Markus Uhr <markus.uhr@bsse.ethz.ch>.
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
using System.Collections.Specialized;
using System.Reflection;
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

    public enum DatatypeOrder
    {
        Error         = -1,  /* error                                      */
        LittleEndian  = 0,   /* little endian                              */
        BigEndian     = 1,   /* bit endian                                 */
        Vax           = 2,   /* VAX mixed endian                           */
        None          = 3    /* no particular order (strings, bits,..)     */
    }
    
    public enum DatatypeSign
    {
        Error   = -1,  /* error                                      */
        None    = 0,   /* this is an unsigned type                   */
        Twos    = 1,   /* two's complement                           */
    }
    
    public enum ByteOrder
    {
        LittleEndian = 0,
        BigEndian = 1,
        Native = 2
    }

    public class Datatype : Base
    {
        private bool can_close;
        
        static Datatype()
        {
            H5.Open();
            IntPtr dl_handle = dlopen(null, RTLD_LAZY);
            if (dl_handle == IntPtr.Zero)
                throw new ApplicationException(String.Format("Error loading libhdf5. Reason {0}", Marshal.PtrToStringAnsi(dlerror())));
            INT8        = new Datatype(Marshal.ReadInt32(dlsym(dl_handle, "H5T_STD_I8LE_g")), false);
            INT16       = new Datatype[] {
                new Datatype(Marshal.ReadInt32(dlsym(dl_handle, "H5T_STD_I16LE_g")), false),
                new Datatype(Marshal.ReadInt32(dlsym(dl_handle, "H5T_STD_I16BE_g")), false),
                new Datatype(Marshal.ReadInt32(dlsym(dl_handle, "H5T_NATIVE_INT16_g")), false)};
            INT32       = new Datatype[] {
                new Datatype(Marshal.ReadInt32(dlsym(dl_handle, "H5T_STD_I32LE_g")), false),
                new Datatype(Marshal.ReadInt32(dlsym(dl_handle, "H5T_STD_I32BE_g")), false),
                new Datatype(Marshal.ReadInt32(dlsym(dl_handle, "H5T_NATIVE_INT32_g")), false)};
            INT64       = new Datatype[] {
                new Datatype(Marshal.ReadInt32(dlsym(dl_handle, "H5T_STD_I64LE_g")), false),
                new Datatype(Marshal.ReadInt32(dlsym(dl_handle, "H5T_STD_I64BE_g")), false),
                new Datatype(Marshal.ReadInt32(dlsym(dl_handle, "H5T_NATIVE_INT64_g")), false)};
            UINT8       = new Datatype(Marshal.ReadInt32(dlsym(dl_handle, "H5T_STD_U8LE_g")), false);
            UINT16      = new Datatype[] {
                new Datatype(Marshal.ReadInt32(dlsym(dl_handle, "H5T_STD_U16LE_g")), false),
                new Datatype(Marshal.ReadInt32(dlsym(dl_handle, "H5T_STD_U16BE_g")), false),
                new Datatype(Marshal.ReadInt32(dlsym(dl_handle, "H5T_NATIVE_UINT16_g")), false)};
            UINT32      = new Datatype[] {
                new Datatype(Marshal.ReadInt32(dlsym(dl_handle, "H5T_STD_U32LE_g")), false),
                new Datatype(Marshal.ReadInt32(dlsym(dl_handle, "H5T_STD_U32BE_g")), false),
                new Datatype(Marshal.ReadInt32(dlsym(dl_handle, "H5T_NATIVE_UINT32_g")), false)};
            UINT64      = new Datatype[] {
                new Datatype(Marshal.ReadInt32(dlsym(dl_handle, "H5T_STD_U64LE_g")), false),
                new Datatype(Marshal.ReadInt32(dlsym(dl_handle, "H5T_STD_U64BE_g")), false),
                new Datatype(Marshal.ReadInt32(dlsym(dl_handle, "H5T_NATIVE_UINT64_g")), false)};
            BINT8       = new Datatype(Marshal.ReadInt32(dlsym(dl_handle, "H5T_STD_B8LE_g")), false);
            BINT16      = new Datatype[] {
                new Datatype(Marshal.ReadInt32(dlsym(dl_handle, "H5T_STD_B16LE_g")), false),
                new Datatype(Marshal.ReadInt32(dlsym(dl_handle, "H5T_STD_B16BE_g")), false),
                new Datatype(Marshal.ReadInt32(dlsym(dl_handle, "H5T_NATIVE_B16_g")), false)};
            BINT32      = new Datatype[] {
                new Datatype(Marshal.ReadInt32(dlsym(dl_handle, "H5T_STD_B32LE_g")), false),
                new Datatype(Marshal.ReadInt32(dlsym(dl_handle, "H5T_STD_B32BE_g")), false),
                new Datatype(Marshal.ReadInt32(dlsym(dl_handle, "H5T_NATIVE_B32_g")), false)};
            BINT64      = new Datatype[] {
                new Datatype(Marshal.ReadInt32(dlsym(dl_handle, "H5T_STD_B64LE_g")), false),
                new Datatype(Marshal.ReadInt32(dlsym(dl_handle, "H5T_STD_B64BE_g")), false),
                new Datatype(Marshal.ReadInt32(dlsym(dl_handle, "H5T_NATIVE_B64_g")), false)};
//            STD_REF_OBJ     = new Datatype(Marshal.ReadInt32(dlsym(dl_handle, "H5T_STD_REF_OBJ_g")), false);
//            STD_REF_DSETREG = new Datatype(Marshal.ReadInt32(dlsym(dl_handle, "H5T_STD_REF_DSETREG_g")), false);
            FLOAT       = new Datatype[] {
                new Datatype(Marshal.ReadInt32(dlsym(dl_handle, "H5T_IEEE_F32LE_g")), false),
                new Datatype(Marshal.ReadInt32(dlsym(dl_handle, "H5T_IEEE_F32BE_g")), false),
                new Datatype(Marshal.ReadInt32(dlsym(dl_handle, "H5T_NATIVE_FLOAT_g")), false)};
            DOUBLE      = new Datatype[] {
                new Datatype(Marshal.ReadInt32(dlsym(dl_handle, "H5T_IEEE_F64LE_g")), false),
                new Datatype(Marshal.ReadInt32(dlsym(dl_handle, "H5T_IEEE_F64BE_g")), false),
                new Datatype(Marshal.ReadInt32(dlsym(dl_handle, "H5T_NATIVE_DOUBLE_g")), false)};
            CONST_STRING    = new Datatype(Marshal.ReadInt32(dlsym(dl_handle, "H5T_C_S1_g")), false);
            VLEN_STRING = CONST_STRING.Copy();
            VLEN_STRING.Size = -1; // variable
            dlclose(dl_handle);
        }
        
        internal Datatype(int raw, bool can_close) : base(raw)
        {
            this.can_close = can_close;
        }
        
        public Datatype Copy()
        {
            return new Datatype(H5Tcopy(raw), true);
        }
        
        public void Close()
        {
            Dispose();
        }
        
        public DatatypeClass Class
        {
            get { return H5Tget_class(raw); }
        }
        
        public long Size
        {
            get { return (long)H5Tget_size(raw); }
            set
            {
                int err = H5Tset_size(raw, value);
                if (err < 0)
                    throw new ApplicationException(String.Format("Error setting size to {0}.", value));
            }
        }
        
        public ByteOrder Order
        {
            get { return H5Tget_order(raw); }
            set
            {
                int err = H5Tset_order(raw, value);
                if (err < 0)
                    throw new ApplicationException(String.Format("Error setting byte order to {0}.", value));
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
        
        protected override void Dispose(bool disposing)
        {
            if (can_close)
                H5Tclose(raw);
            base.Dispose(disposing);
        }

        
        // built-in data types
        
        private static Datatype INT8;
        public static Datatype SByte
        {
            get { return INT8.Copy(); }
        }
        
        private static Datatype[] INT16;
        public static Datatype Int16LE
        {
            get { return INT16[0].Copy(); }
        }
        public static Datatype Int16BE
        {
            get { return INT16[1].Copy(); }
        }
        public static Datatype NativeInt16
        {
            get { return INT16[2].Copy(); }
        }
        
        private static Datatype[] INT32;
        public static Datatype Int32LE
        {
            get { return INT32[0].Copy(); }
        }
        public static Datatype Int32BE
        {
            get { return INT32[1].Copy(); }
        }
        public static Datatype NativeInt32
        {
            get { return INT32[2].Copy(); }
        }
        
        private static Datatype[] INT64;
        public static Datatype Int64LE
        {
            get { return INT64[0].Copy(); }
        }
        public static Datatype Int64BE
        {
            get { return INT64[0].Copy(); }
        }
        public static Datatype NativeInt64
        {
            get { return INT64[0].Copy(); }
        }
        
        private static Datatype UINT8;
        public static Datatype Byte
        {
            get { return UINT8.Copy(); }
        }
        
        private static Datatype[] UINT16;
        public static Datatype UInt16LE
        {
            get { return UINT16[0].Copy(); }
        }
        public static Datatype UInt16BE
        {
            get { return UINT16[1].Copy(); }
        }
        public static Datatype NativeUInt16
        {
            get { return UINT16[2].Copy(); }
        }
        
        private static Datatype[] UINT32;
        public static Datatype UInt32LE
        {
            get { return UINT32[0].Copy(); }
        }
        public static Datatype UInt32BE
        {
            get { return UINT32[1].Copy(); }
        }
        public static Datatype NativeUInt32
        {
            get { return UINT32[2].Copy(); }
        }
        
        private static Datatype[] UINT64;
        public static Datatype UInt64LE
        {
            get { return UINT64[0].Copy(); }
        }
        public static Datatype UInt64BE
        {
            get { return UINT64[1].Copy(); }
        }
        public static Datatype NativeUInt64
        {
            get { return UINT64[2].Copy(); }
        }
        
        private static Datatype BINT8;
        public static Datatype BitArray8
        {
            get { return BINT8.Copy(); }
        }
        
        private static Datatype[] BINT16;
        public static Datatype BitArray16LE
        {
            get { return BINT16[0].Copy(); }
        }
        public static Datatype BitArray16BE
        {
            get { return BINT16[1].Copy(); }
        }
        public static Datatype NativeBitArray16
        {
            get { return BINT16[2].Copy(); }
        }
        
        private static Datatype[] BINT32;
        public static Datatype BitArray32LE
        {
            get { return BINT32[0].Copy(); }
        }
        public static Datatype BitArray32BE
        {
            get { return BINT32[1].Copy(); }
        }
        public static Datatype NativeBitArray32
        {
            get { return BINT32[2].Copy(); }
        }
        
        private static Datatype[] BINT64;
        public static Datatype BitArray64LE
        {
            get { return BINT64[0].Copy(); }
        }
        public static Datatype BitArray64BE
        {
            get { return BINT64[1].Copy(); }
        }
        public static Datatype NativeBitArray64
        {
            get { return BINT64[2].Copy(); }
        }
        
//        private static Datatype STD_REF_OBJ;
//        private static Datatype STD_REF_DSETREG;
        
        private static Datatype[] FLOAT;
        public static Datatype SingleLE
        {
            get { return FLOAT[0].Copy(); }
        }
        public static Datatype SingleBE
        {
            get { return FLOAT[1].Copy(); }
        }
        public static Datatype NativeSingle
        {
            get { return FLOAT[2].Copy(); }
        }
        
        private static Datatype[] DOUBLE;
        public static Datatype DoubleLE
        {
            get { return DOUBLE[0].Copy(); }
        }
        public static Datatype DoubleBE
        {
            get { return DOUBLE[1].Copy(); }
        }
        public static Datatype NativeDouble
        {
            get { return DOUBLE[2].Copy(); }
        }
        
        private static Datatype CONST_STRING;
        public static Datatype ConstString
        {
            get { return CONST_STRING.Copy(); }
        }
        
        private static Datatype VLEN_STRING;
        public static Datatype VlenString
        {
            get { return VLEN_STRING.Copy(); }
        }
        
//        public static Datatype C_STRING
//        {
//            get { return CUSTOM_STRING.Copy(); }
//        }
        
        private static Datatype FromPrimitive(Type t, ByteOrder o)
        {
            if (t == typeof(sbyte))
                return INT8;
            else if (t == typeof(short))
                return INT16[(int)o];
            else if (t == typeof(int))
                return INT32[(int)o];
            else if (t == typeof(long))
                return INT64[(int)o];
            else if (t == typeof(byte))
                return UINT8;
            else if (t == typeof(ushort))
                return UINT16[(int)o];
            else if (t == typeof(uint))
                return UINT32[(int)o];
            else if (t == typeof(ulong))
                return UINT64[(int)o];
            else if (t == typeof(float))
                return FLOAT[(int)o];
            else if (t == typeof(double))
                return DOUBLE[(int)o];
            else if (t == typeof(BitVector32))
                return BINT32[(int)o];
            throw new ArgumentException(String.Format("Unsupported type {0}", t));
        }
        
        private static Datatype FromStruct(Type t, ByteOrder o)
        {
            long size = Marshal.SizeOf(t);
//            Console.WriteLine("*** size of compound type: {0}", size);
            Datatype result = new Datatype(H5Tcreate(DatatypeClass.Compound, (IntPtr)size), true);
            MemberInfo[] mi = t.FindMembers(MemberTypes.Field, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, null);
            long offset = 0;
            for (int i=0; i<mi.Length; i++)
            {
                string mname = mi[i].Name;
                Type mnative = ((FieldInfo)mi[i]).FieldType;
                if (!mnative.IsValueType)
                    throw new ArgumentException(String.Format("Member {0} is not a value type.", mname));
                Datatype mtype;
                if (mnative.IsPrimitive)
                    mtype = Datatype.FromPrimitive(mnative, o);
                else
                    mtype = Datatype.FromStruct(mnative, o);
                long msize = Marshal.SizeOf(((FieldInfo)mi[i]).FieldType);
//                Console.WriteLine("*** offset of member type: {0}", offset);
//                Console.WriteLine("*** size of member type: {0}", msize);
                int err = H5Tinsert(result.raw, mname, (IntPtr)offset, mtype.raw);
                if (err < 0)
                    throw new ApplicationException("Error inserting type into compound datatype.");
                offset += msize;
            }
            if (offset != size)
                throw new ApplicationException(String.Format("Error computing field offsets. True size: {0}; computed: {1}.", size, offset));
            return result;
        }
        
        public static Datatype FromValueType(Type t, ByteOrder o)
        {
            if (!t.IsValueType)
                throw new ArgumentException(String.Format("Type {0} is not a value type.", t));
            Datatype dt;
            if (t.IsPrimitive || t == typeof(BitVector32))
                dt = Datatype.FromPrimitive(t, o);
            else
                dt = Datatype.FromStruct(t, o);
            return dt;
        }
        
        public static Datatype FromValueType(Type t)
        {
            return FromValueType(t, ByteOrder.Native);
        }
        
//        public static Datatype BitArrayType(int w, ByteOrder o)
//        {
//            if (w == 8)
//                return BINT8;
//            else if (w == 16)
//                return BINT16[(int)o];
//            else if (w == 32)
//                return BINT32[(int)o];
//            else if (w == 64)
//                return BINT64[(int)o];
//            else
//                throw new ArgumentException(String.Format("Invalid value of parameter w ({0}).", w));
//        }
        
//        public static Datatype BitArrayType(int w)
//        {
//            return BitArrayType(w, ByteOrder.Native);
//        }
        
        public static Datatype VlenValueType(Type t)
        {
            return VlenValueType(t, ByteOrder.Native);
        }
        
        public static Datatype VlenValueType(Type t, ByteOrder o)
        {
            if (!t.IsValueType)
                throw new ArgumentException(String.Format("Type must be a value type (given {0}).", t));
            Datatype dt = Datatype.FromValueType(t);
            int id = H5Tvlen_create(dt.raw);
            if (id < 0)
                throw new ApplicationException(String.Format("Error creating variable length type (base {0}).", t));
            return new Datatype(id, true);
        }
        
        internal static Datatype VariableLengthString()
        {
            Datatype dt = Datatype.VlenString;
            int id = H5Tvlen_create(dt.raw);
            if (id < 0)
                throw new ApplicationException("Error creating variable length type (base string).");
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
        private static extern int H5Tcreate(DatatypeClass klass, IntPtr size);
        
        [DllImport("hdf5")]
        private static extern int H5Tvlen_create(int base_type_id);
        
        [DllImport("hdf5")]
        private static extern int H5Tcopy(int type_id);
        
        [DllImport("hdf5")]
        private static extern int H5Tinsert(int type_id, string name, IntPtr offset, int field_id);
        
        [DllImport("hdf5")]
        private static extern int H5Tclose(int type_id);
        
        [DllImport("hdf5")]
        private static extern DatatypeClass H5Tget_class(int type_id);
        
        [DllImport("hdf5")]
        private static extern IntPtr H5Tget_size(int type_id);
        
        [DllImport("hdf5")]
        private static extern int H5Tset_size(int type_id, long size);
        
        [DllImport("hdf5")]
        private static extern ByteOrder H5Tget_order(int type_id);
        
        [DllImport("hdf5")]
        private static extern int H5Tset_order(int type_id, ByteOrder order);
        
        [DllImport("hdf5")]
        private static extern int H5Tis_variable_str(int type_id);
    }
}
