using Nucleus.Abstractions;

namespace Nucleus.Build
{
    /// <summary>
    /// Manual production: purchase vehicles/bases/units. First milestone proves the separate plugin loads,
    /// registers with the host, and receives a working context (shared game services injected across the
    /// plugin boundary). The buy-menu UI lands once the host exposes real UI/button services (Phase 4 deepen).
    /// </summary>
    public sealed class BuildMod : IMod
    {
        private IModContext _ctx;

        public ModInfo Info { get; } = new ModInfo
        {
            Id = "build",
            DisplayName = "Build",
            Version = "0.1.0",
            Author = "Nucleus",
            Description = "Purchase vehicles, bases, and units.",
        };

        public void Initialize(IModContext ctx)
        {
            _ctx = ctx;
            // Self-test: proves the separate plugin registered and the host injected working game services.
            ctx.Log.Info("[NUCLEUS:SELFTEST] PASS build-mod-loaded");
            ctx.Log.Info($"[NUCLEUS:METRIC] buildFunds={(int)ctx.Game.Funds()}");

            // Claim a BLD bezel button; the host makes it a native bezel button + MFD screen. The buy-menu
            // content lands next phase; for now a native placeholder proves the screen opens with the highlight.
            ctx.Buttons.RegisterMapButton(new MapButtonSpec
            {
                ModId = Info.Id,
                Label = "BLD",
                BuildContent = parent => Nucleus.Ui.UiFactory.Placeholder(parent, "BUILD\n\nBuy menu — wiring up next."),
            });
        }

        public void Tick(IModTickContext t) { }
        public void OnEnabled() { }
        public void OnDisabled() { }
        public void Shutdown() { }
    }
}
