using System.Runtime.InteropServices;
using Reloaded.Process.Functions.X86Functions;

namespace Reloaded_Mod_Template.RenderWare.Custom
{
    public unsafe class RWAspect
    {
        /// <summary>
        /// Describes a RenderWare Camera View structure.
        /// </summary>
        public struct RWView
        {
            public float X;
            public float Y;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [ReloadedFunction(CallingConventions.Cdecl)]
        public delegate int CameraBuildPerspClipPlanes(RWCamera* RwCamera);

        /// <summary>
        /// Sets the aspect ratio of the current screen view.
        /// </summary>
        /// <param name="a1"></param>
        /// <param name="a2"></param>
        /// <returns></returns>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [ReloadedFunction(CallingConventions.Cdecl)]
        public delegate void RwCameraSetViewWindow(RWCamera* RwCamera, RWView* view);
    }
}
