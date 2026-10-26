using UnityEngine;

namespace GullyLabs.GameSdk
{
    /// <summary>
    /// GullyStudio brand kit tokens (2026-10) for sample scenes and quick UI: navy ground, Studio blue accent, cyan
    /// highlight, green for success, silver text, muted secondary text. Headings use Montserrat 800, labels 600.
    /// </summary>
    public static class GullyStudioTheme
    {
        public const string NavyHex = "#06121F";
        public const string Navy2Hex = "#07121F";
        public const string BlueHex = "#1E8BFF";
        public const string BlueDeepHex = "#1E6FFF";
        public const string CyanHex = "#19D3FF";
        public const string GreenHex = "#2EE68A";
        public const string SilverHex = "#E8EEF5";
        public const string MutedHex = "#8FA6C2";
        public const string FontFamily = "Montserrat";

        public static readonly Color32 Navy = new Color32(0x06, 0x12, 0x1F, 0xFF);
        public static readonly Color32 Navy2 = new Color32(0x07, 0x12, 0x1F, 0xFF);
        public static readonly Color32 Blue = new Color32(0x1E, 0x8B, 0xFF, 0xFF);
        public static readonly Color32 BlueDeep = new Color32(0x1E, 0x6F, 0xFF, 0xFF);
        public static readonly Color32 Cyan = new Color32(0x19, 0xD3, 0xFF, 0xFF);
        public static readonly Color32 Green = new Color32(0x2E, 0xE6, 0x8A, 0xFF);
        public static readonly Color32 Silver = new Color32(0xE8, 0xEE, 0xF5, 0xFF);
        public static readonly Color32 Muted = new Color32(0x8F, 0xA6, 0xC2, 0xFF);

        /// <summary>The "Studio" gradient: cyan at the top to deep blue at the bottom.</summary>
        public static Gradient StudioGradient()
        {
            var g = new Gradient();
            g.SetKeys(
                new[] { new GradientColorKey(Cyan, 0f), new GradientColorKey(BlueDeep, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
            return g;
        }

        /// <summary>Sample-scene default: solid navy clear colour, no skybox.</summary>
        public static void ApplyTo(Camera camera)
        {
            if (camera == null) return;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Navy;
        }
    }
}
