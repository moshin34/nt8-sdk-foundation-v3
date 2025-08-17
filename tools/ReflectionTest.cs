using System;
using System.IO;
using System.Reflection;

namespace Tools
{
    public static class ReflectionTest
    {
        private static readonly string[] RequiredTypes =
        {
            "NT8.SDK.Common.IOrders",
            "NT8.SDK.Common.OrderIds",
            "NT8.SDK.Common.OrderIntent"
        };

        public static int Main(string[] args)
        {
            string assemblyPath = args != null && args.Length > 0
                ? args[0]
                : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "NT8.SDK.Compat.dll");

            Assembly assembly;
            try
            {
                assembly = Assembly.LoadFrom(assemblyPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to load assembly: " + assemblyPath);
                Console.WriteLine(ex.Message);
                return 1;
            }

            bool missing = false;
            foreach (string typeName in RequiredTypes)
            {
                if (assembly.GetType(typeName, false) == null)
                {
                    Console.WriteLine("Missing type: " + typeName);
                    missing = true;
                }
            }

            return missing ? 1 : 0;
        }
    }
}
