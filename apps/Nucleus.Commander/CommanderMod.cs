using Nucleus.Abstractions;
using Nucleus.Composition;

namespace Nucleus.Commander
{
    /// <summary>
    /// Commander as a hosted mod: a thin wrapper over CommanderRuntime. Registers the CMD bezel button; the
    /// host makes it a native MFD bezel button + screen and hands the runtime the screen's content surface to
    /// render the Commander panel into.
    /// </summary>
    public sealed class CommanderMod : IMod
    {
        private readonly CommanderRuntime _runtime;

        public CommanderMod(CommanderRuntime runtime) { _runtime = runtime; }

        public ModInfo Info { get; } = new ModInfo
        {
            Id = "commander",
            DisplayName = "Commander",
            Version = CommanderPlugin.Version,
            Author = "Nucleus",
            Description = "Autonomous theater commander + manual map orders.",
        };

        public void Initialize(IModContext ctx)
        {
            // Publish the shared live campaign so Build/Squad/Warfare render their slices of the same state.
            ctx.ShareCampaign(_runtime.Campaign);

            // Claim the CMD bezel button; the host makes it a native bezel button + MFD screen and gives the
            // runtime that screen's content surface to render into.
            ctx.Buttons.RegisterMapButton(new MapButtonSpec
            {
                ModId = Info.Id,
                Label = "CMD",
                BuildContent = parent => _runtime.BuildPanel(parent),
            });
        }

        public void Tick(IModTickContext t) => _runtime.Tick();
        public void OnEnabled() { }
        public void OnDisabled() { }
        public void Shutdown() { }
    }
}
