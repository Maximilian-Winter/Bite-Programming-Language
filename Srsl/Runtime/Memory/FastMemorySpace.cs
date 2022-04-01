using System.Collections.Generic;
using Srsl.Runtime.Bytecode;

namespace Srsl.Runtime.Memory
{

    public class FastMemorySpace
    {
        public FastMemorySpace m_EnclosingSpace;
        public Dictionary<string, DynamicSrslVariable> NamesToProperties = new Dictionary<string, DynamicSrslVariable>();
        public DynamicSrslVariable[] Properties;
        public string Name;
        public int StackCountAtBegin = 0;

        private int currentMemoryPointer = 0;
        #region Public

        public BinaryChunk CallerChunk;
        public int CallerIntructionPointer;
        public FastMemorySpace(string name, FastMemorySpace enclosingSpace, int stackCount, BinaryChunk callerChunk, int callerInstructionPointer, int memberCount)
        {
            CallerChunk = callerChunk;
            CallerIntructionPointer = callerInstructionPointer;
            m_EnclosingSpace = enclosingSpace;
            Name = name;
            StackCountAtBegin = stackCount;
            Properties = new DynamicSrslVariable[memberCount];
        }

        public void ResetPropertiesArray(int memberCount)
        {
            Properties = new DynamicSrslVariable[memberCount];
            currentMemoryPointer = 0;
        }

        public virtual void Define(DynamicSrslVariable value)
        {
            Properties[currentMemoryPointer] = value;

            currentMemoryPointer++;
        }

        public virtual void Define(DynamicSrslVariable value, string idStr, bool addToProperties = true)
        {
            if ( addToProperties )
            {
                Properties[currentMemoryPointer] = value;
                if (!string.IsNullOrEmpty(idStr))
                {
                    NamesToProperties.Add(idStr, Properties[currentMemoryPointer]);
                }
                currentMemoryPointer++;
            }
            else
            {
                if (!string.IsNullOrEmpty(idStr))
                {
                    NamesToProperties.Add(idStr, value);
                }
            }
        }

        public virtual DynamicSrslVariable Get(string idStr, bool calledFromGlobalMemorySpace = false)
        {
            if (NamesToProperties.ContainsKey(idStr))
            {
                return NamesToProperties[idStr];
            }

            if (m_EnclosingSpace != null && !calledFromGlobalMemorySpace)
            {
                return m_EnclosingSpace.Get(idStr);
            }
            return DynamicVariableExtension.ToDynamicVariable();
        }

        public virtual bool Exist(string idStr, bool calledFromGlobalMemorySpace = false)
        {
            if (NamesToProperties.ContainsKey(idStr))
            {
                return true;
            }

            if (m_EnclosingSpace != null && !calledFromGlobalMemorySpace)
            {
                return m_EnclosingSpace.Exist(idStr);
            }
            return false;
        }

        public virtual void Put(string idStr, DynamicSrslVariable value)
        {
            if (NamesToProperties.ContainsKey(idStr))
            {
                NamesToProperties[idStr] = value;
                return;
            }

            if (m_EnclosingSpace != null)
            {
                m_EnclosingSpace.Put(idStr, value);
            }
        }
        public virtual DynamicSrslVariable Get(int moduleId, int depth, int classId, int id)
        {
            if (moduleId >= 0)
            {
                FastMemorySpace currentMemorySpace = this;
                while (currentMemorySpace.m_EnclosingSpace != null)
                {
                    currentMemorySpace = currentMemorySpace.m_EnclosingSpace;
                }

                if (currentMemorySpace is FastGlobalMemorySpace fastGlobalMemorySpace)
                {
                    if (classId >= 0)
                    {
                        FastMemorySpace fms = fastGlobalMemorySpace.Modules[moduleId].Properties[classId].ObjectData as FastMemorySpace;
                        return fms.Properties[id];
                    }

                    return fastGlobalMemorySpace.Modules[moduleId].Properties[id];
                }

                return DynamicVariableExtension.ToDynamicVariable();
            }

            FastMemorySpace memorySpace = this;
            for (int i = 0; i < depth; i++)
            {
                memorySpace = memorySpace.m_EnclosingSpace;
            }

            if (memorySpace != null && id < memorySpace.Properties.Length)
            {
                return memorySpace.Properties[id];
            }

            return DynamicVariableExtension.ToDynamicVariable();
        }

        public virtual bool Exist(int moduleId, int depth, int classId, int id)
        {
            if (moduleId >= 0)
            {
                FastMemorySpace currentMemorySpace = this;
                while (currentMemorySpace.m_EnclosingSpace != null)
                {
                    currentMemorySpace = currentMemorySpace.m_EnclosingSpace;
                }

                if (currentMemorySpace is FastGlobalMemorySpace fastGlobalMemorySpace)
                {
                    if (classId >= 0)
                    {
                        FastMemorySpace fms = fastGlobalMemorySpace.Modules[moduleId].Properties[classId].ObjectData as FastMemorySpace;

                        return fms.currentMemoryPointer > id;
                    }
                    return fastGlobalMemorySpace.Modules[moduleId].currentMemoryPointer > id;
                }
                return false;
            }

            FastMemorySpace memorySpace = this;
            for (int i = 0; i < depth; i++)
            {
                memorySpace = m_EnclosingSpace;
            }

            if (memorySpace != null && id < memorySpace.currentMemoryPointer)
            {
                if (classId >= 0)
                {
                    FastMemorySpace fms = memorySpace.Properties[classId].ObjectData as FastMemorySpace;
                    return fms.currentMemoryPointer > id;
                }
                return memorySpace.currentMemoryPointer > id;
            }

            return false;
        }

        public virtual void Put(int moduleId, int depth, int classId, int id, DynamicSrslVariable value)
        {
            if (moduleId >= 0)
            {
                FastMemorySpace currentMemorySpace = this;
                while (currentMemorySpace.m_EnclosingSpace != null)
                {
                    currentMemorySpace = currentMemorySpace.m_EnclosingSpace;
                }

                if (currentMemorySpace is FastGlobalMemorySpace fastGlobalMemorySpace)
                {
                    if (classId >= 0)
                    {
                        FastMemorySpace fms = fastGlobalMemorySpace.Modules[moduleId].Properties[classId].ObjectData as FastMemorySpace;
                        fms.Properties[id] = value;
                    }
                    fastGlobalMemorySpace.Modules[moduleId].Properties[id] = value;
                }
                return;
            }

            FastMemorySpace memorySpace = this;
            for (int i = 0; i < depth; i++)
            {
                memorySpace = m_EnclosingSpace;
            }

            if (memorySpace != null && id < memorySpace.currentMemoryPointer)
            {
                if (classId >= 0)
                {
                    FastMemorySpace fms = memorySpace.Properties[classId].ObjectData as FastMemorySpace;
                    fms.Properties[id] = value;
                }
                memorySpace.Properties[id] = value;
            }
        }
        public FastMemorySpace GetEnclosingSpace()
        {
            if (m_EnclosingSpace != null)
            {
                return m_EnclosingSpace;
            }

            return null;
        }

        public override string ToString()
        {
            return Name;
        }

        #endregion
    }

}
