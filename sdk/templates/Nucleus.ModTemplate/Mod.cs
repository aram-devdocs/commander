using CommanderLayer.Abstractions;

namespace MyMod
{
    /// <summary>
    /// Your mod. The host calls Initialize once (after the first scene), Tick every map frame while enabled,
    /// and OnEnabled/OnDisabled on the loader toggle. Use ctx.Ui to create a UI layer + a map-bezel button,
    /// ctx.Game for the shared roster/intel/commands, and ctx.Log to log through BepInEx.
    /// </summary>
    public sealed class Mod : IMod
    {
        private IModContext _ctx;

        public ModInfo Info { get; } = new ModInfo
        {
            Id = "mymod",
            DisplayName = "MyMod",
            Version = "0.1.0",
            Author = "you",
            Description = "A Nucleus mod for Nuclear Option.",
        };

        public void Initialize(IModContext ctx)
        {
            _ctx = ctx;
            _ctx.Log.Info("MyMod initialized.");

            // Example: claim a map-bezel button that toggles your screen.
            // _ctx.Buttons.RegisterMapButton(new MapButtonSpec { ModId = Info.Id, Label = "MOD", OnClick = Toggle });
        }

        public void Tick(IModTickContext t)
        {
            // Runs every map frame while your mod is enabled. e.g. read ctx.Game.Roster() and draw your UI.
        }

        public void OnEnabled() { }
        public void OnDisabled() { }
        public void Shutdown() { }
    }
}
