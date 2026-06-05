using UnityEngine;

namespace CommanderLayer.Ui
{
    /// <summary>
    /// Native HUD colors captured from the game (GameAssets.HUDFriendly/HUDHostile) by the composition root,
    /// with sensible fallbacks so the UI still works headless / in tests. Keeps Ui decoupled from game types.
    /// </summary>
    public static class NativeColors
    {
        public static Color Friendly = new Color(0.45f, 0.95f, 0.55f);
        public static Color Hostile = new Color(1f, 0.35f, 0.35f);
        public static bool Captured;
    }
}
