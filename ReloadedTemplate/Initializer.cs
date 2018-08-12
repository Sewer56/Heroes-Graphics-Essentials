using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using LiteNetLib;
using Reloaded;
using Reloaded.Paths;
using Reloaded.Process;
using Reloaded.Process.Memory;
using System.Security;
using System.Security.Permissions;

namespace Reloaded_Mod_Template.ReloadedTemplate
{
    /// <summary>
    /// This class is simply a proxy for calling Init again from another AppDomain.
    /// </summary>
    public class InitProxy : MarshalByRefObject
    {
        public void Run(IntPtr portLocation)
        {
            Init.Initialize(portLocation);
        }
    }

    public class Init
    {
        /// <summary>
        /// Contains our child AppDomain used for init-ing mods in their own separate worlds.
        /// </summary>
        static AppDomain _childDomain;

        /// <summary>
        /// This file and/or Initializer.cs contains the DLL Template for Reloaded Mod Loader mods.
        /// If you are looking for user code, please see Program.cs
        /// </summary>
        /// <param name="portAddress">Stores the memory location of the port.</param>
        [DllExport]
        public static void Main(IntPtr portAddress)
        {
            // Retrieve Assemblies from the "Libraries" folder.
            AppDomain.CurrentDomain.AssemblyResolve += LocalAssemblyFinder.ResolveAppDomainAssembly;

            // Try restarting in another AppDomain if possible.
            try
            {
                // Give the new AppDomain full permissions.
                PermissionSet permissionSet = new PermissionSet(PermissionState.Unrestricted);
                permissionSet.AddPermission(new SecurityPermission(SecurityPermissionFlag.AllFlags));

                // The ApplicationBase of the new domain should be the directory containing the current DLL.
                AppDomainSetup appDomainSetup = new AppDomainSetup() { ApplicationBase = Path.GetDirectoryName(typeof(InitProxy).Assembly.Location) };
                _childDomain = AppDomain.CreateDomain("Reloaded", null, appDomainSetup, permissionSet);

                // Now make the new AppDomain load our code using our proxy.
                Type proxyType = typeof(InitProxy);
                var initProxy = (InitProxy)_childDomain.CreateInstanceFrom(proxyType.Assembly.Location, proxyType.FullName).Unwrap(); // Our AssemblyResolve will pick the missing DLL out.
                _childDomain.UnhandledException += _childDomain_UnhandledException; // Pass exceptions to default AppDomain on crashes.
                initProxy.Run(portAddress);
            }
            catch (Exception ex)
            {
                Initialize(portAddress);
            }            
        }

        /// <summary>
        /// Throws exceptions in the default AppDomain when/if the application crashes.
        /// VS may otherwise fail to get the stack trace.
        /// </summary>
        private static void _childDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            throw (Exception)e.ExceptionObject;
        }

        /// <summary>
        /// This file and/or Initializer.cs contains the DLL Template for Reloaded Mod Loader mods.
        /// If you are looking for user code, please see Program.cs
        /// </summary>
        /// <param name="portAddress">Stores the memory location of the port used to connect back to Reloaded-Assembler.</param>
        public static void Initialize(IntPtr portAddress)
        {
            AppDomain.CurrentDomain.AssemblyResolve += LocalAssemblyFinder.ResolveAssembly;
            InitializeInternal(portAddress);
        }

        /// <summary>
        /// This file contains the main entry code executed as part of the DLL template for Reloaded Mod Loader
        /// mods. It is very important that the entry method contains only AppDomain.CurrentDomain.AssemblyResolve
        /// due to otherwise possible problems with static initialization of Program.
        /// </summary>
        /// <param name="portAddress">Stores the memory location of the port.</param>
        public static void InitializeInternal(IntPtr portAddress)
        {
            // Initialize Client
            InitClient(portAddress);
            InitBindings();

            // Call Init
            try { Program.Init(); }
            catch (Exception Ex) { Bindings.PrintError($"Failure in initializing Reloaded Mod | {Ex.Message} | {Ex.StackTrace}"); } 
        }

        /// <summary>
        /// This is here because of the logic of the CLR.
        /// If the JIT tries to compile the method to execute, it will fail to find libreloaded,
        /// before we even set the assembly resolution path with AppDomain.CurrentDomain.AssemblyResolve.
        /// </summary>
        /// <param name="portAddress">Stores the memory location of the port.</param>
        static void InitClient(IntPtr portAddress)
        {
            // Setup Local Server Client
            EventBasedNetListener reloadedClientListener = new EventBasedNetListener();
            Client.ReloadedClient = new NetManager(reloadedClientListener, Strings.Loader.ServerConnectKey);
            Client.ReloadedClient.MaxConnectAttempts = 5;
            Client.ReloadedClient.ReconnectDelay = 100;
            Client.ReloadedClient.Start(IPAddress.Loopback, IPAddress.IPv6Loopback, 0);
            Client.ReloadedClient.Connect(IPAddress.Loopback.ToString(), Program.GameProcess.ReadMemory<int>(portAddress));
            Client.ReloadedClient.DisconnectTimeout = Int64.MaxValue;
        }

        /// <summary>
        /// Initializes the libReloaded bindings used for internal Reloaded Mod Loader functions
        /// such as printing to buffers, logging and other functions.
        /// </summary>
        static void InitBindings()
        {
            // Set local game process.
            Program.GameProcess = ReloadedProcess.GetCurrentProcess();
            Program.ExecutingGameLocation = Environment.GetCommandLineArgs()[0];
            Program.ModDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            // For our libraries in a separate AppDomain executing main.dll
            if (Program.ModDirectory.EndsWith("Libraries")) 
                Program.ModDirectory = Path.GetDirectoryName(Program.ModDirectory);

            // Set up Reloaded Mod Loader bindings.
            Bindings.PrintText += Client.Print;
            Bindings.PrintError += Client.PrintError;
            Bindings.PrintInfo += Client.PrintInfo;
            Bindings.PrintWarning += Client.PrintWarning;
            Bindings.TargetProcess = Program.GameProcess;
        }
    }
}
