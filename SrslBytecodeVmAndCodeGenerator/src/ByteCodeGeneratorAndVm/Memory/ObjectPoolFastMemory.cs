using System;
using System.Collections.Generic;

namespace Srsl_Parser.Runtime
{

    public class ObjectPoolFastMemory<T> where T : class
    {
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
            /*for ( int i = 0; i < fastMemorySpace.Properties.Count; i++ )
            {
                DynamicVariableExtension.ReturnDynamicSrslVariable( fastMemorySpace.Properties[i] );
            }*/
            fastMemorySpace.Properties = Array.Empty<DynamicSrslVariable>();
            fastMemorySpace.NamesToProperties.Clear();
            _objects.Enqueue(item);
        }
    }

}
