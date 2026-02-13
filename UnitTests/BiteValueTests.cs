// Copyright (c) Bite Programming Language. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Bite.Runtime;
using Xunit;

namespace UnitTests
{
    /// <summary>
    /// Comprehensive tests for the BiteValue NaN-boxing implementation
    /// </summary>
    public class BiteValueTests
    {
        #region Type Checking Tests

        [Fact]
        public void Null_IsNull()
        {
            var val = BiteValue.Null;
            Assert.True(val.IsNull);
            Assert.False(val.IsNumber);
            Assert.False(val.IsBool);
            Assert.False(val.IsString);
            Assert.False(val.IsObject);
            Assert.Equal(BiteType.Null, val.Type);
        }

        [Fact]
        public void True_IsBool()
        {
            var val = BiteValue.True;
            Assert.True(val.IsBool);
            Assert.True(val.AsBool);
            Assert.True(val.IsTrue);
            Assert.False(val.IsNull);
            Assert.False(val.IsNumber);
            Assert.Equal(BiteType.Bool, val.Type);
        }

        [Fact]
        public void False_IsBool()
        {
            var val = BiteValue.False;
            Assert.True(val.IsBool);
            Assert.False(val.AsBool);
            Assert.False(val.IsTrue);
            Assert.False(val.IsNull);
            Assert.Equal(BiteType.Bool, val.Type);
        }

        [Fact]
        public void Number_IsNumber()
        {
            var val = new BiteValue(42.0);
            Assert.True(val.IsNumber);
            Assert.False(val.IsBool);
            Assert.False(val.IsNull);
            Assert.Equal(BiteType.Number, val.Type);
        }

        [Fact]
        public void Int_ConvertsToDouble()
        {
            var val = new BiteValue(42);
            Assert.True(val.IsNumber);
            Assert.Equal(42.0, val.AsNumber);
            Assert.Equal(42, val.AsInt);
        }

        [Fact]
        public void Float_ConvertsToDouble()
        {
            var val = new BiteValue(3.14f);
            Assert.True(val.IsNumber);
            Assert.Equal(3.14f, (float)val.AsNumber, 3);
        }

        [Fact]
        public void String_IsString()
        {
            var val = new BiteValue("hello");
            try
            {
                Assert.True(val.IsString);
                Assert.False(val.IsObject);
                Assert.True(val.IsReference);
                Assert.Equal(BiteType.String, val.Type);
                Assert.Equal("hello", val.AsString);
                Assert.Equal("hello", val.AsObject);
            }
            finally
            {
                // Clean up the GCHandle
                val.FreeHandle();
            }
        }

        [Fact]
        public void Object_IsObject()
        {
            var obj = new List<int> { 1, 2, 3 };
            var val = new BiteValue(obj);
            try
            {
                Assert.True(val.IsObject);
                Assert.False(val.IsString);
                Assert.True(val.IsReference);
                Assert.Equal(BiteType.Object, val.Type);
                Assert.Same(obj, val.AsObject);
                Assert.Same(obj, val.As<List<int>>());
            }
            finally
            {
                val.FreeHandle();
            }
        }

        [Fact]
        public void NullObject_IsNull()
        {
            var val = new BiteValue((object)null);
            Assert.True(val.IsNull);
        }

        [Fact]
        public void NullString_IsNull()
        {
            var val = new BiteValue((string)null);
            Assert.True(val.IsNull);
        }

        #endregion

        #region Boolean Truthiness Tests

        [Theory]
        [InlineData(0.0, false)]
        [InlineData(1.0, true)]
        [InlineData(-1.0, true)]
        [InlineData(0.0001, true)]
        public void Number_Truthiness(double number, bool expected)
        {
            var val = new BiteValue(number);
            Assert.Equal(expected, val.AsBool);
            Assert.Equal(expected, val.IsTrue);
        }

        [Fact]
        public void Object_IsTruthy()
        {
            var val = new BiteValue(new object());
            try
            {
                Assert.True(val.IsTrue);
                Assert.True(val.AsBool);
            }
            finally
            {
                val.FreeHandle();
            }
        }

        [Fact]
        public void String_IsTruthy()
        {
            var val = new BiteValue("");
            try
            {
                Assert.True(val.IsTrue); // Even empty string is truthy
                Assert.True(val.AsBool);
            }
            finally
            {
                val.FreeHandle();
            }
        }

        #endregion

        #region Arithmetic Tests

        [Fact]
        public void Add_TwoNumbers()
        {
            var a = new BiteValue(5.0);
            var b = new BiteValue(3.0);
            var result = a + b;
            
            Assert.True(result.IsNumber);
            Assert.Equal(8.0, result.AsNumber);
        }

        [Fact]
        public void Subtract_TwoNumbers()
        {
            var a = new BiteValue(10.0);
            var b = new BiteValue(4.0);
            var result = a - b;
            
            Assert.Equal(6.0, result.AsNumber);
        }

        [Fact]
        public void Multiply_TwoNumbers()
        {
            var a = new BiteValue(6.0);
            var b = new BiteValue(7.0);
            var result = a * b;
            
            Assert.Equal(42.0, result.AsNumber);
        }

        [Fact]
        public void Divide_TwoNumbers()
        {
            var a = new BiteValue(15.0);
            var b = new BiteValue(3.0);
            var result = a / b;
            
            Assert.Equal(5.0, result.AsNumber);
        }

        [Fact]
        public void Modulo_TwoNumbers()
        {
            var a = new BiteValue(17.0);
            var b = new BiteValue(5.0);
            var result = a % b;
            
            Assert.Equal(2.0, result.AsNumber);
        }

        [Fact]
        public void Negate_Number()
        {
            var a = new BiteValue(42.0);
            var result = -a;
            
            Assert.Equal(-42.0, result.AsNumber);
        }

        [Fact]
        public void Arithmetic_WithIntegers()
        {
            var a = new BiteValue(10);
            var b = new BiteValue(20);
            var result = a + b;
            
            Assert.Equal(30.0, result.AsNumber);
        }

        #endregion

        #region Comparison Tests

        [Theory]
        [InlineData(5.0, 5.0, true)]
        [InlineData(5.0, 3.0, false)]
        [InlineData(3.0, 5.0, false)]
        public void Equality_Numbers(double a, double b, bool expected)
        {
            var valA = new BiteValue(a);
            var valB = new BiteValue(b);
            
            Assert.Equal(expected, valA == valB);
            Assert.Equal(!expected, valA != valB);
        }

        [Fact]
        public void Equality_SameReferences()
        {
            var obj = new object();
            var valA = new BiteValue(obj);
            var valB = new BiteValue(obj);
            
            try
            {
                // Same object, different handles
                // Value equality should work
                Assert.Equal(valA.AsObject, valB.AsObject);
            }
            finally
            {
                valA.FreeHandle();
                valB.FreeHandle();
            }
        }

        [Theory]
        [InlineData(3.0, 5.0, true)]
        [InlineData(5.0, 3.0, false)]
        [InlineData(5.0, 5.0, false)]
        public void LessThan_Numbers(double a, double b, bool expected)
        {
            var valA = new BiteValue(a);
            var valB = new BiteValue(b);
            
            Assert.Equal(expected, valA < valB);
        }

        [Theory]
        [InlineData(5.0, 3.0, true)]
        [InlineData(3.0, 5.0, false)]
        [InlineData(5.0, 5.0, false)]
        public void GreaterThan_Numbers(double a, double b, bool expected)
        {
            var valA = new BiteValue(a);
            var valB = new BiteValue(b);
            
            Assert.Equal(expected, valA > valB);
        }

        [Theory]
        [InlineData(3.0, 5.0, true)]
        [InlineData(5.0, 5.0, true)]
        [InlineData(5.0, 3.0, false)]
        public void LessThanOrEqual_Numbers(double a, double b, bool expected)
        {
            var valA = new BiteValue(a);
            var valB = new BiteValue(b);
            
            Assert.Equal(expected, valA <= valB);
        }

        [Theory]
        [InlineData(5.0, 3.0, true)]
        [InlineData(5.0, 5.0, true)]
        [InlineData(3.0, 5.0, false)]
        public void GreaterThanOrEqual_Numbers(double a, double b, bool expected)
        {
            var valA = new BiteValue(a);
            var valB = new BiteValue(b);
            
            Assert.Equal(expected, valA >= valB);
        }

        #endregion

        #region Static Factory Tests

        [Fact]
        public void FromObject_Null()
        {
            var val = BiteValue.FromObject(null);
            Assert.True(val.IsNull);
        }

        [Fact]
        public void FromObject_Bool()
        {
            var val = BiteValue.FromObject(true);
            Assert.True(val.IsBool);
            Assert.True(val.AsBool);
        }

        [Fact]
        public void FromObject_Int()
        {
            var val = BiteValue.FromObject(42);
            Assert.True(val.IsNumber);
            Assert.Equal(42.0, val.AsNumber);
        }

        [Fact]
        public void FromObject_Double()
        {
            var val = BiteValue.FromObject(3.14);
            Assert.True(val.IsNumber);
            Assert.Equal(3.14, val.AsNumber);
        }

        [Fact]
        public void FromObject_String()
        {
            var val = BiteValue.FromObject("test");
            try
            {
                Assert.True(val.IsString);
                Assert.Equal("test", val.AsString);
            }
            finally
            {
                val.FreeHandle();
            }
        }

        [Fact]
        public void FromObject_Object()
        {
            var list = new List<int>();
            var val = BiteValue.FromObject(list);
            try
            {
                Assert.True(val.IsObject);
                Assert.Same(list, val.AsObject);
            }
            finally
            {
                val.FreeHandle();
            }
        }

        #endregion

        #region ToString Tests

        [Fact]
        public void ToString_Null()
        {
            Assert.Equal("null", BiteValue.Null.ToString());
        }

        [Fact]
        public void ToString_True()
        {
            Assert.Equal("true", BiteValue.True.ToString());
        }

        [Fact]
        public void ToString_False()
        {
            Assert.Equal("false", BiteValue.False.ToString());
        }

        [Fact]
        public void ToString_Number()
        {
            Assert.Equal("42", new BiteValue(42.0).ToString());
        }

        [Fact]
        public void ToString_String()
        {
            var val = new BiteValue("hello");
            try
            {
                Assert.Equal("hello", val.ToString());
            }
            finally
            {
                val.FreeHandle();
            }
        }

        [Fact]
        public void ToString_Object()
        {
            var val = new BiteValue(new List<int>());
            try
            {
                Assert.Contains("object:", val.ToString());
            }
            finally
            {
                val.FreeHandle();
            }
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void SpecialDoubles_AreHandled()
        {
            // These should not conflict with our type tags
            var inf = new BiteValue(double.PositiveInfinity);
            var negInf = new BiteValue(double.NegativeInfinity);
            var nan = new BiteValue(double.NaN);
            
            Assert.True(inf.IsNumber);
            Assert.True(negInf.IsNumber);
            Assert.True(nan.IsNumber); // Even NaN is a "number" type
        }

        [Fact]
        public void VeryLargeNumber()
        {
            var val = new BiteValue(1e308);
            Assert.True(val.IsNumber);
            Assert.Equal(1e308, val.AsNumber);
        }

        [Fact]
        public void VerySmallNumber()
        {
            var val = new BiteValue(1e-308);
            Assert.True(val.IsNumber);
            Assert.Equal(1e-308, val.AsNumber);
        }

        [Fact]
        public void NegativeZero()
        {
            var val = new BiteValue(-0.0);
            Assert.True(val.IsNumber);
            Assert.Equal(0.0, val.AsNumber);
        }

        [Fact]
        public void HashCode_Consistency()
        {
            var val1 = new BiteValue(42.0);
            var val2 = new BiteValue(42.0);
            
            Assert.Equal(val1.GetHashCode(), val2.GetHashCode());
        }

        [Fact]
        public void Equals_SameValue()
        {
            var val1 = new BiteValue(42.0);
            var val2 = new BiteValue(42.0);
            
            Assert.True(val1.Equals(val2));
            Assert.True(val1.Equals((object)val2));
        }

        #endregion

        #region Stack Tests

        [Fact]
        public void Stack_PushPop()
        {
            var stack = new BiteValueStack(100);
            
            stack.Push(new BiteValue(1.0));
            stack.Push(new BiteValue(2.0));
            stack.Push(new BiteValue(3.0));
            
            Assert.Equal(3, stack.Count);
            
            Assert.Equal(3.0, stack.Pop().AsNumber);
            Assert.Equal(2.0, stack.Pop().AsNumber);
            Assert.Equal(1.0, stack.Pop().AsNumber);
            
            Assert.Equal(0, stack.Count);
        }

        [Fact]
        public void Stack_Peek()
        {
            var stack = new BiteValueStack(100);
            
            stack.Push(new BiteValue(42.0));
            
            Assert.Equal(42.0, stack.Peek().AsNumber);
            Assert.Equal(42.0, stack.Peek().AsNumber); // Still there
            Assert.Equal(1, stack.Count);
        }

        [Fact]
        public void Stack_Dup()
        {
            var stack = new BiteValueStack(100);
            
            stack.Push(new BiteValue(5.0));
            stack.Dup();
            
            Assert.Equal(2, stack.Count);
            Assert.Equal(5.0, stack.Pop().AsNumber);
            Assert.Equal(5.0, stack.Pop().AsNumber);
        }

        [Fact]
        public void Stack_Swap()
        {
            var stack = new BiteValueStack(100);
            
            stack.Push(new BiteValue(1.0));
            stack.Push(new BiteValue(2.0));
            stack.Swap();
            
            Assert.Equal(1.0, stack.Pop().AsNumber);
            Assert.Equal(2.0, stack.Pop().AsNumber);
        }

        [Fact]
        public void Stack_Clear()
        {
            var stack = new BiteValueStack(100);
            
            stack.Push(new BiteValue(1.0));
            stack.Push(BiteValue.True);
            stack.Push(new BiteValue("test"));
            
            stack.Clear();
            
            Assert.Equal(0, stack.Count);
        }

        [Fact]
        public void Stack_OverflowThrows()
        {
            var stack = new BiteValueStack(2);
            
            stack.Push(BiteValue.Null);
            stack.Push(BiteValue.Null);
            
            Assert.Throws<StackOverflowException>(() => stack.Push(BiteValue.Null));
        }

        [Fact]
        public void Stack_UnderflowThrows()
        {
            var stack = new BiteValueStack(100);
            
            Assert.Throws<InvalidOperationException>(() => stack.Pop());
        }

        #endregion
    }
}
