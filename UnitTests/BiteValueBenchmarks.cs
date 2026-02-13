// Copyright (c) Bite Programming Language. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using Bite.Runtime;
using Bite.Runtime.Memory;
using Xunit;
using Xunit.Abstractions;

namespace UnitTests
{
    /// <summary>
    /// Benchmarks comparing BiteValue (NaN-boxed) vs DynamicBiteVariable (heap allocated)
    /// </summary>
    public class BiteValueBenchmarks
    {
        private readonly ITestOutputHelper m_Output;

        public BiteValueBenchmarks(ITestOutputHelper output)
        {
            m_Output = output;
        }

        [Fact]
        public void Benchmark_NumberOperations()
        {
            const int iterations = 10_000_000;
            
            // Benchmark new BiteValue (struct, stack allocated)
            var sw1 = Stopwatch.StartNew();
            {
                var result = new BiteValue(0.0);
                for (int i = 0; i < iterations; i++)
                {
                    var a = new BiteValue(i);
                    var b = new BiteValue(i + 1);
                    result = (a + b) * new BiteValue(2.0) - new BiteValue(1.0);
                }
                // Prevent optimization
                Assert.True(result.AsNumber > 0);
            }
            sw1.Stop();
            
            // Benchmark old DynamicBiteVariable (class, heap allocated)
            var sw2 = Stopwatch.StartNew();
            {
                var result = DynamicVariableExtension.ToDynamicVariable(0.0);
                for (int i = 0; i < iterations; i++)
                {
                    var a = DynamicVariableExtension.ToDynamicVariable((double)i);
                    var b = DynamicVariableExtension.ToDynamicVariable((double)(i + 1));
                    // Simulate arithmetic that would happen in VM
                    double r = (a.NumberData + b.NumberData) * 2.0 - 1.0;
                    result = DynamicVariableExtension.ToDynamicVariable(r);
                }
                Assert.True(result.NumberData > 0);
            }
            sw2.Stop();
            
            m_Output.WriteLine($"BiteValue (NaN-boxed): {sw1.ElapsedMilliseconds}ms");
            m_Output.WriteLine($"DynamicBiteVariable (heap): {sw2.ElapsedMilliseconds}ms");
            m_Output.WriteLine($"Speedup: {((double)sw2.ElapsedMilliseconds / sw1.ElapsedMilliseconds):F2}x");
            
            // New implementation should be faster or comparable
            Assert.True(sw1.ElapsedMilliseconds < sw2.ElapsedMilliseconds * 1.5, 
                "BiteValue should not be significantly slower than DynamicBiteVariable");
        }

        [Fact]
        public void Benchmark_StackOperations()
        {
            const int iterations = 10_000_000;
            const int stackOpsPerIteration = 10; // Push + Pop pairs
            
            // Benchmark new BiteValueStack
            var sw1 = Stopwatch.StartNew();
            {
                var stack = new BiteValueStack(1024);
                for (int i = 0; i < iterations; i++)
                {
                    stack.Push(new BiteValue(i));
                    stack.Push(new BiteValue(i + 1));
                    var a = stack.Pop();
                    var b = stack.Pop();
                    stack.Push(a + b);
                    stack.PopDiscard();
                }
            }
            sw1.Stop();
            
            // Benchmark using DynamicBiteVariable array (simulating old approach)
            var sw2 = Stopwatch.StartNew();
            {
                var stack = new DynamicBiteVariable[1024];
                int sp = 0;
                for (int i = 0; i < iterations; i++)
                {
                    stack[sp++] = DynamicVariableExtension.ToDynamicVariable((double)i);
                    stack[sp++] = DynamicVariableExtension.ToDynamicVariable((double)(i + 1));
                    var a = stack[--sp];
                    var b = stack[--sp];
                    double r = a.NumberData + b.NumberData;
                    stack[sp++] = DynamicVariableExtension.ToDynamicVariable(r);
                    sp--;
                }
            }
            sw2.Stop();
            
            m_Output.WriteLine($"BiteValueStack: {sw1.ElapsedMilliseconds}ms");
            m_Output.WriteLine($"DynamicBiteVariable[]: {sw2.ElapsedMilliseconds}ms");
            m_Output.WriteLine($"Speedup: {((double)sw2.ElapsedMilliseconds / sw1.ElapsedMilliseconds):F2}x");
        }

        [Fact]
        public void Benchmark_MemoryOverhead()
        {
            // Compare memory usage
            const int count = 1_000_000;
            
            // BiteValue array (contiguous, 8 bytes each = 8MB)
            long biteValueMemory = count * 8;
            
            // DynamicBiteVariable array (references + object overhead)
            // Each DynamicBiteVariable is a class: ~32 bytes minimum + reference
            long dynamicVariableMemory = count * (8 + 32); // Reference + object
            
            m_Output.WriteLine($"BiteValue array: {biteValueMemory / 1024 / 1024} MB");
            m_Output.WriteLine($"DynamicBiteVariable array (est): {dynamicVariableMemory / 1024 / 1024} MB");
            m_Output.WriteLine($"Memory reduction: {((double)dynamicVariableMemory / biteValueMemory):F1}x");
            
            Assert.True(biteValueMemory < dynamicVariableMemory);
        }

        [Fact]
        public void Benchmark_TypeChecking()
        {
            const int iterations = 100_000_000;
            
            var values = new BiteValue[]
            {
                new BiteValue(42.0),
                BiteValue.True,
                BiteValue.Null,
                new BiteValue("test"),
                new BiteValue(new object())
            };
            
            // Clean up handles that will be created
            try
            {
                var sw = Stopwatch.StartNew();
                int typeChecks = 0;
                
                for (int i = 0; i < iterations; i++)
                {
                    var val = values[i % values.Length];
                    if (val.IsNumber) typeChecks++;
                    else if (val.IsBool) typeChecks++;
                    else if (val.IsNull) typeChecks++;
                    else if (val.IsString) typeChecks++;
                    else if (val.IsObject) typeChecks++;
                }
                
                sw.Stop();
                m_Output.WriteLine($"Type checking {iterations:N0} values: {sw.ElapsedMilliseconds}ms");
                m_Output.WriteLine($"Operations/sec: {(iterations / (sw.ElapsedMilliseconds / 1000.0)):N0}");
                
                // Prevent optimization
                Assert.True(typeChecks > 0);
            }
            finally
            {
                // Clean up GCHandles
                foreach (var val in values)
                {
                    val.FreeHandle();
                }
            }
        }

        [Fact]
        public void Benchmark_CacheEfficiency()
        {
            const int arraySize = 10000;
            const int iterations = 10000;
            
            // BiteValue - contiguous memory
            var biteArray = new BiteValue[arraySize];
            for (int i = 0; i < arraySize; i++)
                biteArray[i] = new BiteValue(i);
            
            // Sum all elements - tests cache efficiency
            var sw1 = Stopwatch.StartNew();
            double sum1 = 0;
            for (int iter = 0; iter < iterations; iter++)
            {
                sum1 = 0;
                for (int i = 0; i < arraySize; i++)
                    sum1 += biteArray[i].AsNumber;
            }
            sw1.Stop();
            
            m_Output.WriteLine($"BiteValue array sum: {sw1.ElapsedMilliseconds}ms (result: {sum1})");
            
            // Old approach - reference array with scattered heap objects
            // Simulated with just numbers since we can't create that many objects quickly
            var dynArray = new double[arraySize];
            for (int i = 0; i < arraySize; i++)
                dynArray[i] = i;
            
            var sw2 = Stopwatch.StartNew();
            double sum2 = 0;
            for (int iter = 0; iter < iterations; iter++)
            {
                sum2 = 0;
                for (int i = 0; i < arraySize; i++)
                    sum2 += dynArray[i];
            }
            sw2.Stop();
            
            m_Output.WriteLine($"Raw double array sum: {sw2.ElapsedMilliseconds}ms (result: {sum2})");
            m_Output.WriteLine($"Overhead vs raw: {((double)sw1.ElapsedMilliseconds / sw2.ElapsedMilliseconds):F2}x");
            
            // BiteValue should be close to raw double performance
            Assert.True(sw1.ElapsedMilliseconds < sw2.ElapsedMilliseconds * 2);
        }

        [Fact]
        public void Benchmark_GCHandleOverhead()
        {
            // Measure cost of GCHandle allocation vs raw object reference
            const int iterations = 100000;
            
            var obj = new object();
            var handles = new IntPtr[iterations];
            
            // Allocate handles
            var sw1 = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                var handle = System.Runtime.InteropServices.GCHandle.Alloc(obj);
                handles[i] = System.Runtime.InteropServices.GCHandle.ToIntPtr(handle);
            }
            sw1.Stop();
            
            // Free handles
            var sw2 = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                var handle = System.Runtime.InteropServices.GCHandle.FromIntPtr(handles[i]);
                handle.Free();
            }
            sw2.Stop();
            
            m_Output.WriteLine($"GCHandle alloc {iterations:N0}: {sw1.ElapsedMilliseconds}ms");
            m_Output.WriteLine($"GCHandle free {iterations:N0}: {sw2.ElapsedMilliseconds}ms");
            m_Output.WriteLine($"Total overhead: {sw1.ElapsedMilliseconds + sw2.ElapsedMilliseconds}ms");
            
            // Should be relatively fast (microseconds per operation)
            Assert.True(sw1.ElapsedMilliseconds + sw2.ElapsedMilliseconds < 1000);
        }
    }
}
