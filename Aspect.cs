using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Reloaded.Process.Functions.X86Functions;
using Reloaded_Mod_Template.RenderWare;

namespace Reloaded_Mod_Template
{
    public unsafe class Aspect
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
