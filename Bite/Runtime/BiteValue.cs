// Copyright (c) Bite Programming Language. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Bite.Runtime
{
    /// <summary>
    /// A NaN-boxed value type for the Bite VM.
    /// 
    /// Uses IEEE 754 quiet NaN space to store type tags and pointers,
    /// allowing all values to fit in 8 bytes without heap allocation.
    /// 
    /// Bit layout for non-numbers (quiet NaN):
    ///     64 bits: SEEEEEEE EEEEFFFF FFFFFFFF FFFFFFFF FFFFFFFF FFFFFFFF FFFFFFFF FFFFFFFF
    ///     S = sign bit (0)
    ///     E = exponent (all 1s = 0x7FF)
    ///     F = fraction/payload (used for type tag + data)
    ///     
    /// Type tags (high 16 bits of payload):
    ///     0x7FF8 = Null
    ///     0x7FF9 = True
    ///     0x7FFA = False  
    ///     0x7FFB = Object (pointer in lower 48 bits)
    ///     0x7FFC = String (pointer in lower 48 bits)
    ///     0x7FFD = Reserved for future use
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 8)]
    public readonly struct BiteValue : IEquatable<BiteValue>
    {
        #region Constants and Bit Masks

        // The raw bits storage (at same offset as DoubleValue for union behavior)
        [FieldOffset(0)]
        private readonly ulong m_Bits;

        // IEEE 754 masks
        private const ulong MASK_SIGN = 0x8000000000000000UL;
        private const ulong MASK_EXPONENT = 0x7FF0000000000000UL;
        private const ulong MASK_QUIET_NAN = 0x7FF8000000000000UL;
        private const ulong MASK_TYPE_TAG = 0xFFFF000000000000UL;
        private const ulong MASK_PAYLOAD = 0x0000FFFFFFFFFFFFUL;

        // Type tags (in the high 16 bits)
        private const ulong TAG_NUMBER = 0x0000000000000000UL;  // Not a NaN - actual double value
        private const ulong TAG_NULL = 0x7FF8000000000000UL;    // Standard quiet NaN
        private const ulong TAG_TRUE = 0x7FF9000000000000UL;
        private const ulong TAG_FALSE = 0x7FFA000000000000UL;
        private const ulong TAG_OBJECT = 0x7FFB000000000000UL;
        private const ulong TAG_STRING = 0x7FFC000000000000UL;
        
        // Sanity check: ensure our tags are valid quiet NaNs
        private const ulong QUIET_NAN_MIN = 0x7FF8000000000000UL;
        private const ulong QUIET_NAN_MAX = 0x7FFFFFFFFFFFFFFFUL;

        #endregion

        #region Static Singleton Values

        /// <summary>Null value singleton</summary>
        public static readonly BiteValue Null = new BiteValue(TAG_NULL);
        
        /// <summary>Boolean true singleton</summary>
        public static readonly BiteValue True = new BiteValue(TAG_TRUE);
        
        /// <summary>Boolean false singleton</summary>
        public static readonly BiteValue False = new BiteValue(TAG_FALSE);

        #endregion

        #region Constructors

        /// <summary>Private constructor from raw bits</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private BiteValue(ulong bits)
        {
            m_Bits = bits;
        }

        /// <summary>Create from double (numbers are stored directly)</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe BiteValue(double value)
        {
            m_Bits = *(ulong*)&value;
            
            // Ensure we don't accidentally create a tagged value
            // If the double happens to be a quiet NaN, we need to canonicalize it
            // This is extremely rare but possible
            if ((m_Bits & MASK_QUIET_NAN) == MASK_QUIET_NAN)
            {
                // Set the payload to avoid collision with our tags
                // Make it a "Number" NaN that's distinct from our type tags
                m_Bits = (m_Bits & ~MASK_TYPE_TAG) | 0x7FF7000000000000UL;
            }
        }

        /// <summary>Create from int (widened to double)</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BiteValue(int value) : this((double)value) { }

        /// <summary>Create from float (widened to double)</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BiteValue(float value) : this((double)value) { }

        /// <summary>Create from bool</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BiteValue(bool value)
        {
            m_Bits = value ? TAG_TRUE : TAG_FALSE;
        }

        /// <summary>Create from string (allocates GCHandle)</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BiteValue(string value)
        {
            if (value == null)
            {
                m_Bits = TAG_NULL;
            }
            else
            {
                // Pin the string and store the pointer
                GCHandle handle = GCHandle.Alloc(value, GCHandleType.Normal);
                IntPtr ptr = GCHandle.ToIntPtr(handle);
                m_Bits = TAG_STRING | (ulong)ptr.ToInt64();
            }
        }

        /// <summary>Create from object (allocates GCHandle)</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BiteValue(object value)
        {
            if (value == null)
            {
                m_Bits = TAG_NULL;
            }
            else if (value is string str)
            {
                // Strings get special handling
                GCHandle handle = GCHandle.Alloc(str, GCHandleType.Normal);
                IntPtr ptr = GCHandle.ToIntPtr(handle);
                m_Bits = TAG_STRING | (ulong)ptr.ToInt64();
            }
            else
            {
                // General object
                GCHandle handle = GCHandle.Alloc(value, GCHandleType.Normal);
                IntPtr ptr = GCHandle.ToIntPtr(handle);
                m_Bits = TAG_OBJECT | (ulong)ptr.ToInt64();
            }
        }

        #endregion

        #region Type Checking Properties

        /// <summary>Returns true if this value is a number (including numeric NaN/Infinity, excluding tagged values)</summary>
        public bool IsNumber
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                // If high 16 bits match any of our type tags, it's not a number
                ulong highBits = m_Bits & MASK_TYPE_TAG;
                return highBits != TAG_NULL && 
                       highBits != TAG_TRUE && 
                       highBits != TAG_FALSE && 
                       highBits != TAG_OBJECT && 
                       highBits != TAG_STRING;
            }
        }

        /// <summary>Returns true if this value is null</summary>
        public bool IsNull
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Bits == TAG_NULL;
        }

        /// <summary>Returns true if this value is a boolean</summary>
        public bool IsBool
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Bits == TAG_TRUE || m_Bits == TAG_FALSE;
        }

        /// <summary>Returns true if this value is true (boolean true or truthy)</summary>
        public bool IsTrue
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (m_Bits == TAG_FALSE || m_Bits == TAG_NULL) return false;
                if (IsNumber) return AsNumber != 0.0;
                return true; // Objects, strings, true are truthy
            }
        }

        /// <summary>Returns true if this value is a string</summary>
        public bool IsString
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (m_Bits & MASK_TYPE_TAG) == TAG_STRING;
        }

        /// <summary>Returns true if this value is an object (not string)</summary>
        public bool IsObject
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (m_Bits & MASK_TYPE_TAG) == TAG_OBJECT;
        }

        /// <summary>Returns true if this value is any reference type (string or object)</summary>
        public bool IsReference
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                ulong tag = m_Bits & MASK_TYPE_TAG;
                return tag == TAG_OBJECT || tag == TAG_STRING;
            }
        }

        /// <summary>Returns the BiteType enum for this value</summary>
        public BiteType Type
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (IsNumber) return BiteType.Number;
                if (m_Bits == TAG_NULL) return BiteType.Null;
                if (m_Bits == TAG_TRUE || m_Bits == TAG_FALSE) return BiteType.Bool;
                if ((m_Bits & MASK_TYPE_TAG) == TAG_STRING) return BiteType.String;
                if ((m_Bits & MASK_TYPE_TAG) == TAG_OBJECT) return BiteType.Object;
                return BiteType.Null; // Should not reach here
            }
        }

        #endregion

        #region Value Extraction

        /// <summary>Get value as double (only valid if IsNumber)</summary>
        public unsafe double AsNumber
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                ulong bits = m_Bits;
                return *(double*)&bits;
            }
        }

        /// <summary>Get value as int (truncates double if needed)</summary>
        public int AsInt
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => IsNumber ? (int)AsNumber : 0;
        }

        /// <summary>Get value as bool</summary>
        public bool AsBool
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (m_Bits == TAG_TRUE) return true;
                if (m_Bits == TAG_FALSE) return false;
                if (m_Bits == TAG_NULL) return false;
                if (IsNumber) return AsNumber != 0.0;
                return true; // Non-null references are truthy
            }
        }

        /// <summary>Get value as string (only valid if IsString)</summary>
        public string AsString
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (!IsString) return null;
                IntPtr ptr = new IntPtr((long)(m_Bits & MASK_PAYLOAD));
                GCHandle handle = GCHandle.FromIntPtr(ptr);
                return (string)handle.Target;
            }
        }

        /// <summary>Get value as object (works for both objects and strings)</summary>
        public object AsObject
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (!IsReference) return null;
                IntPtr ptr = new IntPtr((long)(m_Bits & MASK_PAYLOAD));
                GCHandle handle = GCHandle.FromIntPtr(ptr);
                return handle.Target;
            }
        }

        /// <summary>Get value as specific type</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T As<T>() where T : class
        {
            if (!IsReference) return null;
            IntPtr ptr = new IntPtr((long)(m_Bits & MASK_PAYLOAD));
            GCHandle handle = GCHandle.FromIntPtr(ptr);
            return handle.Target as T;
        }

        /// <summary>Get the raw bits (for debugging/advanced use)</summary>
        public ulong RawBits => m_Bits;

        #endregion

        #region Static Factory Methods

        /// <summary>Create from any C# object with type inference</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BiteValue FromObject(object value)
        {
            if (value == null) return Null;
            if (value is bool b) return new BiteValue(b);
            if (value is double d) return new BiteValue(d);
            if (value is float f) return new BiteValue(f);
            if (value is int i) return new BiteValue(i);
            if (value is string s) return new BiteValue(s);
            return new BiteValue(value);
        }

        /// <summary>Create a number value</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BiteValue Number(double value) => new BiteValue(value);

        /// <summary>Create a number value</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BiteValue Number(int value) => new BiteValue(value);

        /// <summary>Create a string value</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BiteValue String(string value) => new BiteValue(value);

        /// <summary>Create an object value</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BiteValue Object(object value) => new BiteValue(value);

        #endregion

        #region Arithmetic Operators

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BiteValue operator +(BiteValue a, BiteValue b)
        {
            return new BiteValue(a.AsNumber + b.AsNumber);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BiteValue operator -(BiteValue a, BiteValue b)
        {
            return new BiteValue(a.AsNumber - b.AsNumber);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BiteValue operator *(BiteValue a, BiteValue b)
        {
            return new BiteValue(a.AsNumber * b.AsNumber);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BiteValue operator /(BiteValue a, BiteValue b)
        {
            return new BiteValue(a.AsNumber / b.AsNumber);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BiteValue operator %(BiteValue a, BiteValue b)
        {
            return new BiteValue(a.AsNumber % b.AsNumber);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BiteValue operator -(BiteValue a)
        {
            return new BiteValue(-a.AsNumber);
        }

        #endregion

        #region Comparison Operators

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(BiteValue a, BiteValue b)
        {
            // Same bits = same value
            if (a.m_Bits == b.m_Bits) return true;
            
            // Both numbers? Compare numerically
            if (a.IsNumber && b.IsNumber) return a.AsNumber == b.AsNumber;
            
            // Different types are not equal
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(BiteValue a, BiteValue b)
        {
            return !(a == b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(BiteValue a, BiteValue b)
        {
            return a.AsNumber < b.AsNumber;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(BiteValue a, BiteValue b)
        {
            return a.AsNumber > b.AsNumber;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(BiteValue a, BiteValue b)
        {
            return a.AsNumber <= b.AsNumber;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(BiteValue a, BiteValue b)
        {
            return a.AsNumber >= b.AsNumber;
        }

        #endregion

        #region Equality and Hashing

        public override bool Equals(object obj)
        {
            return obj is BiteValue other && Equals(other);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(BiteValue other)
        {
            return this == other;
        }

        public override int GetHashCode()
        {
            return m_Bits.GetHashCode();
        }

        #endregion

        #region String Conversion

        public override string ToString()
        {
            if (IsNumber) return AsNumber.ToString();
            if (m_Bits == TAG_NULL) return "null";
            if (m_Bits == TAG_TRUE) return "true";
            if (m_Bits == TAG_FALSE) return "false";
            if (IsString) return AsString;
            if (IsObject) return $"object:{AsObject?.GetType().Name ?? "null"}";
            return $"unknown(0x{m_Bits:X16})";
        }

        #endregion

        #region Cleanup Helpers

        /// <summary>Free the GCHandle if this value holds a reference</summary>
        public void FreeHandle()
        {
            if (IsReference)
            {
                IntPtr ptr = new IntPtr((long)(m_Bits & MASK_PAYLOAD));
                GCHandle handle = GCHandle.FromIntPtr(ptr);
                handle.Free();
            }
        }

        /// <summary>Check if this value needs GC handle cleanup</summary>
        public bool NeedsCleanup => IsReference;

        #endregion
    }

    /// <summary>Type enumeration for Bite values</summary>
    public enum BiteType : byte
    {
        Null,
        Number,
        Bool,
        String,
        Object
    }
}
