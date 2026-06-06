using System;
using UnityEngine;

namespace Nucleus.Abstractions
{
    /// <summary>Identity + presentation metadata for a mod (stable Id is the config/registry/Thunderstore key).</summary>
    public sealed class ModInfo
    {
        public string Id;
        public string DisplayName;
        public string Version;
        public string Author;
        public string Description;
        public Sprite Icon;
    }

    /// <summary>
    /// A map-bezel button a mod registers. The host adds it as a NATIVE MFD bezel button paired with a native
    /// <c>MFDScreen</c> (so the game handles placement, the green "open" highlight, and auto-close when the map
    /// closes). The mod populates its screen once via <see cref="BuildContent"/> — given the screen's content
    /// RectTransform. <see cref="OnClick"/> is an optional extra hook fired when the screen is opened.
    /// </summary>
    public sealed class MapButtonSpec
    {
        public string ModId;
        public string Label;
        /// <summary>Populate the mod's native MFD screen. Called once with the screen's content RectTransform.</summary>
        public Action<RectTransform> BuildContent;
        /// <summary>Optional: fired when the native bezel button is pressed (screen opened).</summary>
        public Action OnClick;
        public Func<Color> LabelColor;
    }

    /// <summary>A row a mod contributes to the main-menu loader.</summary>
    public sealed class MenuItemSpec
    {
        public string ModId;
        public string Label;
        public Action OnClick;
    }
}
