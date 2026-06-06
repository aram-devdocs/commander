using BepInEx.Logging;
using CommanderLayer.Abstractions;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CommanderLayer.Host
{
    /// <summary>
    /// In-process host (single plugin, Phase 3): owns the mod registry + the shared game services and drives
    /// the per-frame tick over enabled mods. The DynamicMap.Update Harmony postfix calls <see cref="Tick"/>.
    /// Canvas/bezel-button/loader ownership is introduced as Build/Squad arrive (Phase 4-5); for now Commander
    /// keeps its own runtime and the host adds only the registry/tick layer.
    /// </summary>
    public sealed class ModHost
    {
        private readonly ModRegistry _registry;
        private readonly GameServices _game = new GameServices();
        private readonly LogSink _log;

        public ModHost(ManualLogSource log)
        {
            _log = new LogSink(log);
            _registry = new ModRegistry(mod => new ModContext(mod, _log, _game));
            // Install the registration handler so mods (this assembly, and separate plugins in Phase 4+)
            // resolve through the host. Mods that registered earlier are flushed by SetHandler.
            ModPlatform.SetHandler(m => _registry.Add(m, enabled: true));
        }

        public ModRegistry Registry => _registry;

        /// <summary>Per-frame pump (from the DynamicMap.Update postfix): tick every enabled mod.</summary>
        public void Tick()
        {
            _registry.TickAll(new TickContext(
                mapOpen: DynamicMap.mapMaximized,
                dt: Time.unscaledDeltaTime,
                pointerOverUi: EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()));
        }

        private sealed class TickContext : IModTickContext
        {
            public TickContext(bool mapOpen, float dt, bool pointerOverUi)
            {
                MapOpen = mapOpen;
                UnscaledDeltaTime = dt;
                PointerOverModUi = pointerOverUi;
            }
            public bool MapOpen { get; }
            public float UnscaledDeltaTime { get; }
            public bool PointerOverModUi { get; }
        }
    }
}
