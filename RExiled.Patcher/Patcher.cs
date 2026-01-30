namespace RExiled.Patcher
{
    using System;
    using System.IO;
    using dnlib.DotNet;
    using dnlib.DotNet.Emit;

    internal static class Patcher
    {
        private static string path = string.Empty;

        private static void Main(string[] args)
        {
            try
            {
                if (args.Length != 1)
                {
                    Console.WriteLine("Provide the location of Assembly-CSharp.dll:");
                    string? input = Console.ReadLine();
                    if (input == null)
                    {
                        Console.WriteLine("Input stream ended unexpectedly.");
                        return;
                    }
                    string path = input;
                }
                else
                {
                    path = args[0];
                }

                if (string.IsNullOrEmpty(path))
                {
                    Console.WriteLine("Invalid input path.");
                    return;
                }

                ModuleDefMD? module = ModuleDefMD.Load(path);
                if (module == null)
                {
                    Console.WriteLine($"File {path} not found!");
                    return;
                }

                Console.WriteLine($"Loaded {module.Name}");
                Console.WriteLine("Resolving References...");

                var context = ModuleDef.CreateModuleContext();
                module.Context = context;
                ((AssemblyResolver)context.AssemblyResolver).AddToCache(module);

                Console.WriteLine("Injecting the Bootstrap Class.");

                string currentDir = Directory.GetCurrentDirectory();
                if (currentDir == null)
                {
                    Console.WriteLine("Failed to get current directory.");
                    return;
                }

                string bootstrapPath = Path.Combine(currentDir, "Exiled.Bootstrap.dll");
                if (bootstrapPath == null)
                {
                    Console.WriteLine("Failed to construct bootstrap path.");
                    return;
                }

                ModuleDefMD? bootstrap = ModuleDefMD.Load(bootstrapPath);
                if (bootstrap == null)
                {
                    Console.WriteLine("Failed to load Exiled.Bootstrap.dll");
                    return;
                }

                Console.WriteLine("Loaded " + bootstrap.Name);

                TypeDef? modClass = null;
                foreach (var type in bootstrap.Types)
                {
                    if (type.Name == "Bootstrap")
                    {
                        modClass = type;
                        Console.WriteLine($"Hooked to: \"{type.Namespace}.{type.Name}\"");
                        break;
                    }
                }

                if (modClass == null)
                {
                    Console.WriteLine("Bootstrap class not found in Exiled.Bootstrap.dll");
                    return;
                }

                var modRefType = modClass;
                bootstrap.Types.Remove(modClass);
                modRefType.DeclaringType = null;

                module.Types.Add(modRefType);

                MethodDef? call = FindMethod(modRefType, "Load");
                if (call == null)
                {
                    Console.WriteLine("Failed to get the \"Load\" method! Maybe you don't have permission?");
                    return;
                }

                Console.WriteLine("Injected!");
                Console.WriteLine("Injection completed!");
                Console.WriteLine("Patching code...");

                TypeDef? typeDef = FindType(module.Assembly, "ServerConsole");
                if (typeDef == null)
                {
                    Console.WriteLine("Type 'ServerConsole' not found in target assembly.");
                    return;
                }

                MethodDef? start = FindMethod(typeDef, "Start");

                if (start == null)
                {
                    start = new MethodDefUser(
                        "Start",
                        MethodSig.CreateInstance(module.CorLibTypes.Void),
                        MethodImplAttributes.IL | MethodImplAttributes.Managed,
                        MethodAttributes.Private | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);
                    typeDef.Methods.Add(start);
                }

                start.Body.Instructions.Insert(0, OpCodes.Call.ToInstruction(call));

                string outputPath = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(path)) ?? ".", "Assembly-CSharp-Exiled.dll");
                module.Write(outputPath);

                Console.WriteLine("Patching completed successfully!");
            }
            catch (Exception exception)
            {
                Console.WriteLine($"An error has occurred while patching: {exception}");
            }

            Console.Read();
        }

        private static MethodDef? FindMethod(TypeDef? type, string methodName)
        {
            if (type != null)
            {
                foreach (var method in type.Methods)
                {
                    if (method.Name == methodName)
                        return method;
                }
            }
            return null;
        }

        private static TypeDef? FindType(AssemblyDef? assembly, string path)
        {
            if (assembly == null)
                return null;

            foreach (var module in assembly.Modules)
            {
                foreach (var type in module.Types)
                {
                    if (type.FullName == path)
                        return type;
                }
            }
            return null;
        }
    }
}