using CommanderLayer.Abstractions;
using CommanderLayer.Composition;

namespace CommanderLayer.Host
{
    /// <summary>
    /// Commander as the first hosted mod. Phase 3: a thin wrapper over the existing CommanderRuntime so the
    /// host/registry/tick pattern is introduced with behavior preserved exactly. Later phases move the
    /// Canvas/screen/bezel wiring onto the host services and slim this down.
    /// </summary>
    public sealed class CommanderMod : IMod
    {
        private readonly CommanderRuntime _runtime;

        public CommanderMod(CommanderRuntime runtime) { _runtime = runtime; }

        public ModInfo Info { get; } = new ModInfo
        {
            Id = "commander",
            DisplayName = "Commander",
            Version = Plugin.Version,
            Author = "Nucleus",
            Description = "Autonomous theater commander + manual map orders.",
        };

        // The runtime self-initializes on its first Tick (EnsureCanvas/EnsureScreen), so nothing to do here.
        public void Initialize(IModContext ctx) { }
        public void Tick(IModTickContext t) => _runtime.Tick();
        public void OnEnabled() { }
        public void OnDisabled() { }
        public void Shutdown() { }
    }
}
