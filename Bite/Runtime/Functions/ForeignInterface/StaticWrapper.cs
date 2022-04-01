using System.Collections.Generic;
using System.Reflection;

namespace Srsl.Runtime.Functions.ForeignInterface
{

    public class StaticWrapper
    {
        System.Type _type;
        private Dictionary<string, FastMethodInfo> CachedMethods = new Dictionary<string, FastMethodInfo>();
        public StaticWrapper(System.Type type)
        {
            _type = type;
        }

        public object InvokeMember(string name, object[] args, System.Type[] argsTypes)
        {
            if (!CachedMethods.ContainsKey(name))
            {
                var method = _type.GetMethod(
                    name,
                    BindingFlags.Static | BindingFlags.Public,
                    null,
                    argsTypes,
                    null);

                FastMethodInfo fastMethodInfo = new FastMethodInfo(method);

                CachedMethods.Add(name, fastMethodInfo);
                return fastMethodInfo.Invoke(null, args);
            }
            else
            {
                return CachedMethods[name].Invoke(null, args);
            }
        }
    }

}
