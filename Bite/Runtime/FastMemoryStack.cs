using System;
using Bite.Runtime.Memory;

namespace Bite.Runtime
{
    public class FastMemoryStack
    {
        private FastMemorySpace[] m_FastMemorySpaces = new FastMemorySpace[1024];
        private int m_FastMemoryPointer = 0;

        public int Count => m_FastMemoryPointer;

        public FastMemorySpace Peek()
        {
            FastMemorySpace fastMemorySpace = m_FastMemorySpaces[m_FastMemoryPointer - 1];
            return fastMemorySpace;
        }

        public FastMemorySpace Pop()
        {
            FastMemorySpace fastMemorySpace = m_FastMemorySpaces[--m_FastMemoryPointer];
            return fastMemorySpace;
        }

        public void Push(FastMemorySpace fastMemorySpace)
        {
            m_FastMemorySpaces[m_FastMemoryPointer] = fastMemorySpace;

            if (m_FastMemoryPointer >= 1023)
            {
                throw new IndexOutOfRangeException("Call Stack Overflow");
            }
            m_FastMemoryPointer++;
        }
    }

public class DynamicSrslVariableStack
{
    private DynamicBiteVariable[] m_DynamicVariables = new DynamicBiteVariable[1024];
    private int m_DynamicVariablePointer = 0;

    public int Count
    {
        get => m_DynamicVariablePointer;
        set
        {
            m_DynamicVariablePointer = value;
        }
    }

    public DynamicBiteVariable Peek()
    {
        DynamicBiteVariable dynamicVariable = m_DynamicVariables[Count - 1];
        return dynamicVariable;
    }
    
    public DynamicBiteVariable Peek(int i)
    {
        DynamicBiteVariable dynamicVariable = m_DynamicVariables[i];
        return dynamicVariable;
    }

    public DynamicBiteVariable Pop()
    {
        DynamicBiteVariable dynamicVariable = m_DynamicVariables[--Count];
        return dynamicVariable;
    }

    
    public void Push(DynamicBiteVariable dynamicVar)
    {
        m_DynamicVariables[Count] = dynamicVar;

        if (Count >= 1023)
        {
            throw new IndexOutOfRangeException("Stack Overflow");
        }
        Count++;
    }
}

public class UsingStatementStack
{
    private object[] m_UsedObjects = new object[128];
    private int m_UsedObjectPointer = 0;

    public int Count
    {
        get => m_UsedObjectPointer;
        set
        {
            m_UsedObjectPointer = value;
        }
    }
    
    public void Pop()
    {
        object usedObject = m_UsedObjects[--Count];
        ((IDisposable)usedObject).Dispose();
    }

    
    public void Push(object usedObject)
    {
        m_UsedObjects[Count] = usedObject;

        if (Count >= 1023)
        {
            throw new IndexOutOfRangeException("Using Statement Overflow");
        }
        Count++;
    }
}
}