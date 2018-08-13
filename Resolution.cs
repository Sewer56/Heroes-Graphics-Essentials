using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Reloaded.Process.Functions.X86Functions;
using static Reloaded.Native.WinAPI.WindowStyles;
using static Reloaded.Native.WinAPI.Constants;

namespace Reloaded_Mod_Template
{
    public class Graphics
    {
        /// <summary>
        /// Simple JSON file to read/write resolution from.
        /// </summary>
        public class GraphicsSettings
        {
            [JsonProperty(Required = Required.Default)]
            public int Width = 1280;

            [JsonProperty(Required = Required.Default)]
            public int Height = 720;

            [JsonProperty(Required = Required.Default)]
            public bool Borderless = true;

            [JsonProperty(Required = Required.Default)]
            public bool Resizable  = false;

            /// <summary>
            /// Parses an existing config, if exists. If not exists, assumes 720p and
            /// creates a new config.
            /// </summary>
            /// <param name="modDirectory">The directory where the config is to be saved/loaded from.</param>
            /// <returns></returns>
            public static GraphicsSettings ParseConfig(string modDirectory)
            {
                string saveLocation = Path.Combine(modDirectory + "\\Graphics.json");

                if (!File.Exists(saveLocation))
                    WriteConfig(new GraphicsSettings(), modDirectory);                        
                
                string resolutionFile = File.ReadAllText(saveLocation);
                return JsonConvert.DeserializeObject<GraphicsSettings>(resolutionFile);
            }

            /// <summary>
            /// Writes the current config to disk.
            /// </summary>
            /// <param name="config">The config to write to disk.</param>
            /// <param name="modDirectory">The directory where the config should be stored.</param>
            public static void WriteConfig(GraphicsSettings config, string modDirectory)
            {
                string saveLocation = Path.Combine(modDirectory + "\\Graphics.json");
                File.WriteAllText(saveLocation, JsonConvert.SerializeObject(config, Formatting.Indented));
            }

            /// <summary>
            /// Removes the border from the current window style that is passed into AdjustWindowRect on boot to set Window Style.
            /// </summary>
            /// <param name="currentWindowStyle">The window style which should be changed.</param>
            /// <returns>A modified version of the same style.</returns>
            public static uint SetBorderless(uint currentWindowStyle)
            {
                // Change the window style.
                currentWindowStyle &= ~WS_BORDER;
                currentWindowStyle &= ~WS_CAPTION;
                currentWindowStyle &= ~WS_MAXIMIZEBOX;
                currentWindowStyle &= ~WS_MINIMIZEBOX;

                return currentWindowStyle;
            }

            /// <summary>
            /// Removes the border from the current window style that is passed into AdjustWindowRect on boot to set Window Style. 
            /// </summary>
            /// <param name="currentWindowStyle">The window style which should be changed.</param>
            /// <returns>A modified version of the same style.</returns>
            public static uint SetResizable(uint currentWindowStyle)
            {
                // Change the window style.
                currentWindowStyle |= WS_SIZEBOX;

                return currentWindowStyle;
            }
        }

        [ReloadedFunction(new ReloadedFunctionAttribute.Register[0], ReloadedFunctionAttribute.Register.eax, ReloadedFunctionAttribute.StackCleanup.Callee, 0x20)]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int ConfigureGame(); // sub_4469F0, sets the initial game resolution, we will override it.
    }
}
