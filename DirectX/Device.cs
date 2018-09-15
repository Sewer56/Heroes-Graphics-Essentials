using Reloaded.Process.Functions.Utilities;
using Reloaded.Process.Functions.X86Functions;
using SharpDX.Direct3D9;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Reloaded;
using Reloaded.Process.Functions.X86Hooking;
using static Reloaded.Process.Native.Native;

namespace Reloaded_Mod_Template.DirectX
{
    public unsafe class Device
    {
        /*
            Utilizes Direct3D9 and then patches D3D9 device creation to use a depth buffer
            that would prevent flickering from happening.
        */

        private VirtualFunctionTable _direct3DVirtualFunctionTable;
        private VirtualFunctionTable _direct3DDeviceVirtualFunctionTable;
        private FunctionHook<CreateDevice> _createDeviceHook;
        private FunctionHook<Direct3D9DeviceResetDelegate> _resetDeviceHook;
        private Settings.D3D9Settings _dx9Settings;
        private int maxMSAAQuality = 0;

        public Device(Settings.D3D9Settings D3D9Settings, string modDirectory)
        {
            _dx9Settings = D3D9Settings;
            HookDevice(modDirectory);
        }

        /// <summary>
        /// Copies Crosire's D3D8To9 and hooks the DX9 device creation.
        /// </summary>
        /// <param name="dllDirectory">Directory containing Crosire's d3d8to9.</param>
        public void HookDevice(string dllDirectory)
        {
            // Copy crosire's d3d8to9 to game directory and then load it. (Game should use D3D9 now).
            if (! File.Exists("d3d8.dll"))
                File.Copy(dllDirectory + $"\\d3d8.dll", "d3d8.dll", true);

            // Load Crosire's D3D8To9 (which in turn loads D3D9 internally)
            LoadLibraryW("d3d8.dll");

            // Get our D3D Interface VTable
            using (Direct3D direct3D = new Direct3D())
            using (Form renderForm = new Form())
            using (SharpDX.Direct3D9.Device device = new SharpDX.Direct3D9.Device(direct3D, 0, DeviceType.NullReference, IntPtr.Zero, CreateFlags.HardwareVertexProcessing, new PresentParameters() { BackBufferWidth = 1, BackBufferHeight = 1, DeviceWindowHandle = renderForm.Handle }))
            {
                _direct3DVirtualFunctionTable = new VirtualFunctionTable(direct3D.NativePointer, Enum.GetNames(typeof(Interfaces.IDirect3D9)).Length);
                _direct3DDeviceVirtualFunctionTable = new VirtualFunctionTable(device.NativePointer, Enum.GetNames(typeof(Interfaces.IDirect3DDevice9)).Length);
            }

            // Hook D3D9 device creation.
            _createDeviceHook = _direct3DVirtualFunctionTable.TableEntries[(int)Interfaces.IDirect3D9.CreateDevice].CreateFunctionHook86<CreateDevice>(CreateDeviceImpl).Activate();
            _resetDeviceHook = _direct3DDeviceVirtualFunctionTable.TableEntries[(int)Interfaces.IDirect3DDevice9.Reset].CreateFunctionHook86<Direct3D9DeviceResetDelegate>(ResetDeviceImpl).Activate();
        }

        /* Hook DirectX Device Creation */
        private IntPtr CreateDeviceImpl(IntPtr direct3DPointer, uint adapter, DeviceType deviceType, IntPtr hFocusWindow, CreateFlags behaviorFlags, ref PresentParameters pPresentationParameters, int** ppReturnedDeviceInterface)
        {
            // Get D3D Interface (IDirect3D9)
            Direct3D d3d = new Direct3D(direct3DPointer);

            // Enable Hardware Vertex Processing..
            if (_dx9Settings.HardwareVertexProcessing)
            {
                behaviorFlags = behaviorFlags & ~CreateFlags.SoftwareVertexProcessing;
                behaviorFlags = behaviorFlags | CreateFlags.HardwareVertexProcessing;
            }

            // Get and Set max MSAA Quality
            if (_dx9Settings.EnableMSAA)
            {   
                bool msaaAvailable = d3d.CheckDeviceMultisampleType(0, DeviceType.Hardware, pPresentationParameters.BackBufferFormat,
                                        pPresentationParameters.Windowed, (MultisampleType)_dx9Settings.MSAALevel, out maxMSAAQuality);
                
                if (!msaaAvailable)
                {
                    Bindings.PrintError($"The user set MSAA Setting ({_dx9Settings.MSAALevel} Samples) is not supported on this hardware configuration.");
                    Bindings.PrintError($"MSAA will be disabled.");
                    _dx9Settings.EnableMSAA = false;
                }

                if (maxMSAAQuality > 0)
                    maxMSAAQuality -= 1;
            }

            // Check for AF Compatibility
            if (_dx9Settings.EnableAF)
            {
                var capabilities = d3d.GetDeviceCaps(0, DeviceType.Hardware);
                if (_dx9Settings.AFLevel > capabilities.MaxAnisotropy)
                {
                    Bindings.PrintError($"The user set Anisotropic Filtering Setting ({_dx9Settings.AFLevel} Samples) is not supported on this hardware configuration.");
                    Bindings.PrintError($"AF will be disabled.");
                    _dx9Settings.EnableAF = false;
                }
            }

            // Set present parameters.
            pPresentationParameters = SetPresentParameters(ref pPresentationParameters);

            return _createDeviceHook.OriginalFunction(direct3DPointer, adapter, deviceType, hFocusWindow, behaviorFlags, ref pPresentationParameters, ppReturnedDeviceInterface);
        }

        /* Hook DirectX Device Reset */
        private int ResetDeviceImpl(IntPtr device, ref PresentParameters pPresentationParameters)
        {
            pPresentationParameters = SetPresentParameters(ref pPresentationParameters);
            int result = _resetDeviceHook.OriginalFunction(device, ref pPresentationParameters);

            SetDeviceParameters(device);
            return result;
        }

        /* Sets the present parameters to user settings. */
        private PresentParameters SetPresentParameters(ref PresentParameters presentParameters)
        {
            // Toggle VSync.
            presentParameters.PresentationInterval = _dx9Settings.VSync ? PresentInterval.One : PresentInterval.Immediate;

            // Enable/Disable MSAA.
            if (_dx9Settings.EnableMSAA)
            {
                presentParameters.MultiSampleType       = (MultisampleType) _dx9Settings.MSAALevel;
                presentParameters.MultiSampleQuality    = maxMSAAQuality;
                presentParameters.SwapEffect            = SwapEffect.Discard;
            }

            return presentParameters;
        }

        /* Sets the different device parameters on each reset. */
        private void SetDeviceParameters(IntPtr device)
        {
            SharpDX.Direct3D9.Device localDevice = new SharpDX.Direct3D9.Device(device);

            if (_dx9Settings.EnableMSAA)
                localDevice.SetRenderState(RenderState.MultisampleAntialias, true);
            
            if (_dx9Settings.EnableAF)
            {
                localDevice.SetSamplerState(0, SamplerState.MagFilter, TextureFilter.Anisotropic);
                localDevice.SetSamplerState(0, SamplerState.MinFilter, TextureFilter.Anisotropic);
                localDevice.SetSamplerState(0, SamplerState.MaxAnisotropy, _dx9Settings.AFLevel);
            }

        }

        /*
            -------------------
            Function Signatures
            -------------------
        */

        [ReloadedFunction(CallingConventions.Stdcall)]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr CreateDevice(IntPtr direct3DPointer, uint adapter, DeviceType deviceType, IntPtr hFocusWindow, CreateFlags behaviorFlags, ref PresentParameters pPresentationParameters, int** ppReturnedDeviceInterface);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode, SetLastError = true)]
        [ReloadedFunction(CallingConventions.Stdcall)]
        public delegate int Direct3D9DeviceResetDelegate(IntPtr device, ref PresentParameters presentParameters);
    }
}
