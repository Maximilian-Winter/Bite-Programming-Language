using System.IO;

namespace Bite.Modules
{
    internal class ModuleLoader
    {
        public static string LoadModule(string moduleName)
        {
            using (Stream stream =
                   typeof( ModuleLoader ).Assembly.GetManifestResourceStream( $"Bite.Modules.{moduleName}.bite" ))
            {
                StreamReader reader = new StreamReader( stream );
                return reader.ReadToEnd();
            }
        }
    }
}
