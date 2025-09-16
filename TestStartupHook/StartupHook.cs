using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;

public static class StartupHook
{
    // The runtime looks for this exact signature by convention.
    public static void Initialize()
    {
        // Write immediately to ensure we can see if this is called
        Console.WriteLine("[StartupHook] *** STARTUP HOOK CALLED ***");
        Console.Error.WriteLine("[StartupHook] *** STARTUP HOOK CALLED TO STDERR ***");
        
        try
        {
            // Central folder with your DLLs (Windows path in your case)
            var central = @"C:\ProgramData\BHoM\Assemblies";
            
            // Verify the directory exists
            if (!Directory.Exists(central))
            {
                Console.WriteLine($"[StartupHook] Warning: Directory does not exist: {central}");
                return;
            }

            Console.WriteLine($"[StartupHook] Initializing assembly resolver for: {central}");

            // Managed resolver: only called if default probing fails
            AssemblyLoadContext.Default.Resolving += (alc, name) =>
            {
                try
                {
                    var candidate = Path.Combine(central, name.Name + ".dll");
                    if (File.Exists(candidate))
                    {
                        Console.WriteLine($"[StartupHook] Loading assembly: {name.Name} from {candidate}");
                        return alc.LoadFromAssemblyPath(candidate);
                    }
                    else
                    {
                        Console.WriteLine($"[StartupHook] Assembly not found: {name.Name} at {candidate}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[StartupHook] Error loading assembly {name.Name}: {ex.Message}");
                }
                return null;
            };

            // Also handle AppDomain assembly resolve for compatibility
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                try
                {
                    var assemblyName = new AssemblyName(args.Name);
                    var candidate = Path.Combine(central, assemblyName.Name + ".dll");
                    if (File.Exists(candidate))
                    {
                        Console.WriteLine($"[StartupHook] AppDomain loading assembly: {assemblyName.Name} from {candidate}");
                        return Assembly.LoadFrom(candidate);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[StartupHook] Error in AppDomain assembly resolve for {args.Name}: {ex.Message}");
                }
                return null;
            };

            Console.WriteLine("[StartupHook] Assembly resolver initialized successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[StartupHook] Fatal error during initialization: {ex}");
        }
    }
}
