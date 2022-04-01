using System;

namespace Srsl.Runtime.Memory
{

    public class ObjectPoolFastMemory
    {
        private FastMemorySpace[] m_FastCallMemorySpaces = new FastMemorySpace[1024];
        private int m_FastMemorySpacePointer = 0;

        public ObjectPoolFastMemory()
        {
            for ( int i = 0; i < 1024; i++ )
            {
                m_FastCallMemorySpaces[i] = new FastMemorySpace( "", null, 0, null, 0, 0 );
            }
        }
        
        
        public FastMemorySpace Get()
        {
            FastMemorySpace fastMemorySpace = m_FastCallMemorySpaces[m_FastMemorySpacePointer];
            fastMemorySpace.Properties = Array.Empty<DynamicBiteVariable>();
            fastMemorySpace.NamesToProperties.Clear();
            
            if ( m_FastMemorySpacePointer >= 1023 )
            {
                throw new IndexOutOfRangeException("Memory Pool Overflow");
            }
            
            m_FastMemorySpacePointer++;
            return fastMemorySpace;
        }

        public void Return(FastMemorySpace item)
        {
            if ( item == m_FastCallMemorySpaces[m_FastMemorySpacePointer - 1] )
            {
                m_FastMemorySpacePointer--;
            }
        }
        /*
        private readonly Queue<T> _objects;
        private readonly Func<T> _objectGenerator;

        public ObjectPoolFastMemory(Func<T> objectGenerator, int poolInitSize)
        {
            _objectGenerator = objectGenerator ?? throw new ArgumentNullException(nameof(objectGenerator));
            _objects = new Queue<T>();

            for (int i = 0; i < poolInitSize; i++)
            {
                _objects.Enqueue(objectGenerator());
            }
        }

        public T Get()
        {
            if (_objects.Count > 0)
            {
                return _objects.Dequeue();
            }

            return _objectGenerator();
        }

        public void Return(T item)
        {
            FastMemorySpace fastMemorySpace = (item as FastMemorySpace);
            fastMemorySpace.Properties = Array.Empty<DynamicSrslVariable>();
            fastMemorySpace.NamesToProperties.Clear();
            _objects.Enqueue(item);
        }*/
    }

}
