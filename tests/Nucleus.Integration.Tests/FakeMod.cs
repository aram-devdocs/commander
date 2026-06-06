using Nucleus.Abstractions;

namespace Nucleus.Integration.Tests
{
    /// <summary>A no-Unity IMod that records lifecycle calls, for driving ModRegistry headlessly.</summary>
    internal sealed class FakeMod : IMod
    {
        public FakeMod(string id) { Info = new ModInfo { Id = id, DisplayName = id, Version = "0.0.0" }; }

        public ModInfo Info { get; }
        public int Inits, Ticks, Enables, Disables, Shutdowns;

        public void Initialize(IModContext ctx) => Inits++;
        public void Tick(IModTickContext t) => Ticks++;
        public void OnEnabled() => Enables++;
        public void OnDisabled() => Disables++;
        public void Shutdown() => Shutdowns++;
    }
}
