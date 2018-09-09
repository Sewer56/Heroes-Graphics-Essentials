using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Reloaded.Process.Functions.X86Functions;
using static Reloaded.Native.WinAPI.WindowStyles;
using static Reloaded.Native.WinAPI.Constants;
using static Reloaded.Process.Functions.X86Functions.ReloadedFunctionAttribute;

namespace Reloaded_Mod_Template
{
    public unsafe class Settings
    {
        public const string Descriptions =
@"
/*
    Width:              The width of the game window, in pixels.
    Height:             The height of the game window, in pixels.
    Fullscreen:         Sets the game to fullscreen mode, not complicated.
    Borderless:         Sets the game window to borderless if the game is not in fullscreen mode.
    Resizable:          Makes the game window resizable if the game is not in fullscreen mode.
    Disable2PFrameSkip: Disables the frame skipping behaviour in 2 player mode, allowing you to play at 60 FPS.
    AspectRatioLimit:   If the game window is below this aspect ratio, the widescreen hack 
                        scales the window vertically instead of horizontally.

    StupidlyFastLoadTimes:
                        Reimplements a bug to the extreme that happens while running the game in fullscreen on
                        modern hardware.
                        https://twitter.com/sewer56lol/status/1031387436683866112

    EnableAspectHack:
                        A hack to stop the game crashing on the stage titlecards with extreme aspect ratios.

    AlternateAspectScaling:
                        An alternative way of scaling the game's FOV.
                        Setting this locks the Aspect Ratio of the in-game HUD to the set AspectRatioLimit.
                        Normal behaviour with this flag off is a HUD shrink.

    DirectX 9 Settings (D3D9Settings):
        Enable:             Copies Crosire's D3D8To9 to the game directory if it does not exist and renders the game
                            through DirectX 9.

        EnableMSAA:         Enables Multi Sampling Anti Aliasing to reduce the amount of jagged edges shown on character
                            models.
        
        EnableAF:           Enables Anisotropic Filtering.

        MSAALevel:          Specifies the number of samples for MSAA; this value can be anywhere between 1 and 16.
                            Powers of two are recommended.

        AFLevel:            Specifies the level of anisotropic filtering used between 1 and 16.

        VSync:              Enabling this kills any possible tearing at the expense of possible input latency.

        HardwareVertexProcessing: 
                            If set to true; the GPU processes the vertex shaders increasing performance.
                            Otherwise the CPU processes vertex shaders.

    Default Settings (Basically Heroes Launcher Settings):
        Language:           Spoiler: This setting is useless; game stores language per-save.
            0 = Japanese    (Default Launcher Ignores this Language) 
            1 = English
            2 = French
            3 = Spanish
            4 = German
            5 = Italian
            6 = Korean      (Default Launcher Ignores this Language)

        SFXVolume: Volume of sound effects from 0 to 100.
        BGMVolume: Volume of music from 0 to 100.    
        ThreeDimensionalSound: Why would you want to disable this?
        SFXOn: Self explanatory.
        BGMOn: Self explanatory.
        SoftShadows: If this is false, all shadows are circles.
        MouseControl: Nobody likes playing with the mouse. (I think)
        CharmyShutup: Disables character action sounds such as Knuckles' famous 'Shoot! Rock! Yeah!'.
*/";

        public class DefaultSettings
        {
            public int Language  = 1;
            public int SFXVolume = 100;
            public int BGMVolume = 100;
            public bool ThreeDimensionalSound = true;
            public bool SFXOn = true;
            public bool BGMOn = true;
            public bool SoftShadows = true;
            public int  MouseControl = -1;
            public bool CharmyShutup = false;
        }

        /// <summary>
        /// Replicates resolution struct as stored in the Sonic Heroes executable.
        /// </summary>
        public struct NativeGraphicsSetting
        {
            public int Width;
            public int Height;
            public int BitsPerPixel;
            public int Unknown0;
            public int Unknown1;
        }

        /// <summary>
        /// Stores various DirectX 9 related settings.
        /// </summary>
        public class D3D9Settings
        {
            /// <summary>
            /// Enables Direct3D9 manipulation.
            /// </summary>
            [JsonProperty(Required = Required.Default)]
            public bool Enable = true;

            /// <summary>
            /// Enables Multi-Sampling Anti Aliasing
            /// </summary>
            [JsonProperty(Required = Required.Default)]
            public bool EnableMSAA = true;

            /// <summary>
            /// Enables Anisotropic Filtering
            /// </summary>
            [JsonProperty(Required = Required.Default)]
            public bool EnableAF = true;

            /// <summary>
            /// Sets the amount of MSAA Samples Taken.
            /// </summary>
            [JsonProperty(Required = Required.Default)]
            public int MSAALevel = 4;

            /// <summary>
            /// Sets the amount of MSAA Samples Taken.
            /// </summary>
            [JsonProperty(Required = Required.Default)]
            public int AFLevel = 16;

            /// <summary>
            /// Enabling kills tearing at the expense of possible slight input latency.
            /// </summary>
            [JsonProperty(Required = Required.Default)]
            public bool VSync = true;

            /// <summary>
            /// If set to true; the GPU processes the vertex shaders increasing performance.
            /// Otherwise the CPU processes vertex shaders.
            /// </summary>
            [JsonProperty(Required = Required.Default)]
            public bool HardwareVertexProcessing = true;
        }

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
            public bool Fullscreen = false;

            [JsonProperty(Required = Required.Default)]
            public bool Borderless = true;

            [JsonProperty(Required = Required.Default)]
            public bool Resizable  = false;

            [JsonProperty(Required = Required.Default)]
            public bool Disable2PFrameskip = true;

            [JsonProperty(Required = Required.Default)]
            public float AspectRatioLimit = 4/3F;

            [JsonProperty(Required = Required.Default)]
            public bool StupidlyFastLoadTimes = true;

            [JsonProperty(Required = Required.Default)]
            public bool EnableAspectHack = true;

            [JsonProperty(Required = Required.Default)]
            public bool AlternateAspectScaling = false;

            [JsonProperty(Required = Required.Default)]
            public D3D9Settings D3D9Settings = new D3D9Settings();

            [JsonProperty(Required = Required.Default)]
            public DefaultSettings DefaultSettings = new DefaultSettings();

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
                File.WriteAllText(saveLocation, JsonConvert.SerializeObject(config, Formatting.Indented) + Descriptions);                
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
    }
}
