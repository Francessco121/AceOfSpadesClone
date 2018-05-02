/* GraphicsOptions.cs
 * Ethan Lafrenais
*/

using Dash.Engine.Diagnostics;
using Dash.Engine.IO;
using System.Collections;
using System.Collections.Generic;

namespace Dash.Engine.Graphics
{
    public class GraphicsOptions
    {
        public bool EnablePostProcessing
        {
            get { return ApplyFXAA; }
        }

        public bool ApplyFXAA;
        public bool RenderShadows;

        public int ShadowResolution = 1;
        public int ShadowPCFSamples;

        public FogQuality FogQuality;

        static Dictionary<string, GraphicsOptions> allOptions;

        static GraphicsOptions()
        {
            allOptions = new Dictionary<string, GraphicsOptions>();
        }

        public static GraphicsOptions Init(ConfigSection graphicsSection)
        {
            ConfigSection presets = graphicsSection.GetSection("Presets");
            if (presets == null)
            {
                DashCMD.WriteError("[game.cfg - GraphicsOptions] Graphics.Presets was not found!");
                return null;
            }

            string usedPreset = graphicsSection.GetString("preset");

            if (usedPreset == null)
            {
                DashCMD.WriteError("[game.cfg - GraphicsOptions] Graphics.preset was not found!");
                return null;
            }

            foreach (DictionaryEntry pair in presets.Children)
            {
                ConfigSection preset = pair.Value as ConfigSection;
                if (preset == null)
                {
                    DashCMD.WriteWarning("[game.cfg - GraphicsOptions] Invalid token '{0}' in Graphics.Presets", pair.Key);
                    continue;
                }

                bool fxaa = preset.GetBoolean("fxaa") ?? false;
                bool shadows = preset.GetBoolean("shadows") ?? false;
                int shadowRes = preset.GetInteger("shadowRes") ?? 1;
                int pcfSamples = preset.GetInteger("shadowPCF") ?? 1;
                FogQuality fogQuality = preset.GetEnum<FogQuality>("fogQuality") ?? FogQuality.Low;

                GraphicsOptions options = new GraphicsOptions()
                {
                    ApplyFXAA = fxaa,
                    RenderShadows = shadows,
                    ShadowResolution = shadowRes,
                    ShadowPCFSamples = pcfSamples,
                    FogQuality = fogQuality
                };

                allOptions.Add((string)pair.Key, options);
            }

            GraphicsOptions usedOptions;
            if (allOptions.TryGetValue(usedPreset, out usedOptions))
            {
                DashCMD.WriteStandard("[game.cfg - GraphicsOptions] Using preset '{0}'", usedPreset);
                return usedOptions;
            }
            else
            {
                DashCMD.WriteError("[game.cfg - GraphicsOptions] Specified preset '{0}' was not found!", usedPreset);
                return null;
            }
        }
    }
}
