// Copyright (c) Bite Programming Language. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Bite.Runtime
{
    /// <summary>
    /// Manages GCHandles for reference-typed BiteValues.
    /// Provides efficient allocation and bulk cleanup.
    /// </summary>
    public sealed class HandleManager
    {
        // Reusable handle storage to reduce allocations
        private readonly Stack<GCHandle> m_FreeHandles;
        private readonly List<GCHandle> m_ActiveHandles;
        private readonly int m_Capacity;

        /// <summary>Number of active handles</summary>
        public int ActiveCount => m_ActiveHandles.Count;

        /// <summary>Creates a new handle manager</summary>
        /// <param name="initialCapacity">Expected number of concurrent handles</param>
        public HandleManager(int initialCapacity = 1024)
        {
            m_Capacity = initialCapacity;
            m_FreeHandles = new Stack<GCHandle>(initialCapacity);
            m_ActiveHandles = new List<GCHandle>(initialCapacity);
            
            // Pre-populate with some handles
            for (int i = 0; i < Math.Min(initialCapacity, 256); i++)
            {
                m_FreeHandles.Push(new GCHandle());
            }
        }

        /// <summary>Allocates a handle for an object</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IntPtr Alloc(object obj)
        {
            if (obj == null)
                return IntPtr.Zero;

            GCHandle handle = GCHandle.Alloc(obj, GCHandleType.Normal);
            m_ActiveHandles.Add(handle);
            return GCHandle.ToIntPtr(handle);
        }

        /// <summary>Allocates a handle for a string</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IntPtr AllocString(string str)
        {
            return Alloc(str);
        }

        /// <summary>Gets the target object from a handle pointer</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object GetTarget(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero)
                return null;

            GCHandle handle = GCHandle.FromIntPtr(ptr);
            return handle.Target;
        }

        /// <summary>Gets the target as a specific type</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetTarget<T>(IntPtr ptr) where T : class
        {
            return GetTarget(ptr) as T;
        }

        /// <summary>Gets the target string from a handle pointer</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetString(IntPtr ptr)
        {
            return GetTarget(ptr) as string;
        }

        /// <summary>Frees a single handle by pointer</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Free(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero)
                return;

            GCHandle handle = GCHandle.FromIntPtr(ptr);
            handle.Free();
        }

        /// <summary>Tracks an externally allocated handle for bulk cleanup</summary>
        public void Track(GCHandle handle)
        {
            m_ActiveHandles.Add(handle);
        }

        /// <summary>Frees all tracked handles</summary>
        public void ClearAll()
        {
            foreach (GCHandle handle in m_ActiveHandles)
            {
                if (handle.IsAllocated)
                    handle.Free();
            }
            m_ActiveHandles.Clear();
        }

        /// <summary>Frees handles that are no longer needed (conservative)</summary>
        public void CollectDeadHandles()
        {
            // This is a placeholder for more sophisticated GC integration
            // For now, we rely on explicit cleanup
        }
    }

    /// <summary>
    /// Helper struct for creating BiteValues with automatic handle cleanup tracking.
    /// </summary>
    public readonly struct HandleScope : IDisposable
    {
        private readonly HandleManager m_Manager;
        private readonly int m_StartCount;

        public HandleScope(HandleManager manager)
        {
            m_Manager = manager;
            m_StartCount = manager?.ActiveCount ?? 0;
        }

        public void Dispose()
        {
            // Could implement scope-based cleanup here
        }
    }
}
