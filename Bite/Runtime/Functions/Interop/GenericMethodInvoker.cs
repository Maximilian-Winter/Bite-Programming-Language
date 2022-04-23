using System;
using System.Reflection;
using Bite.Runtime.Functions.ForeignInterface;
using Bite.Runtime.Memory;

namespace Bite.Runtime.Functions.Interop
{

public class GenericMethodInvoker : IBiteVmCallable
{
    private Type[] m_ArgTypes;

    private MethodInfo m_OriginalMethodInfo;

    public GenericMethodInvoker( MethodInfo methodInfo )
    {
        OriginalMethodInfo = methodInfo;

        ParameterInfo[] parameterInfos = methodInfo.GetParameters();
        m_ArgTypes = new Type[parameterInfos.Length];

        for (var i = 0; i < parameterInfos.Length; i++)
        {
            m_ArgTypes[i] = parameterInfos[i].ParameterType;
        }
    }

    public MethodInfo OriginalMethodInfo
    {
        get => m_OriginalMethodInfo;
        private set => m_OriginalMethodInfo = value;
    }

    public object Call( DynamicBiteVariable[] arguments )
    {
        object[] constructorArgs = new object[arguments.Length - 1];

        for (int i = 1; i < arguments.Length; i++)
        {
            constructorArgs[i - 1] = Convert.ChangeType(
                arguments[i].ToObject(),
                m_ArgTypes[i - 1] );

        }

        object value = OriginalMethodInfo.Invoke( arguments[0].ToObject(), constructorArgs );

        return value;
    }

}

}
