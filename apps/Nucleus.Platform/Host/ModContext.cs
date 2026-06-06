using Nucleus.Abstractions;
using Nucleus.Ui;
using UnityEngine;

namespace Nucleus.Host
{
    /// <summary>
    /// The per-mod context the host hands a mod at initialize time. Phase 3: Log + the shared Game services
    /// are real; the UI surface and button registry are placeholders until the host owns the single Canvas and
    /// arbitrates bezel slots (Phase 4, when a second mod needs to share). Config binding returns the default
    /// for now (Commander reads the plugin config directly).
    /// </summary>
    internal sealed class ModContext : IModContext
    {
        private readonly IMod _mod;
        private readonly System.Func<Nucleus.Core.Command.ICampaign> _getCampaign;
        private readonly System.Action<Nucleus.Core.Command.ICampaign> _setCampaign;

        public ModContext(IMod mod, ILogSink log, IGameServices game, IButtonRegistry buttons,
            System.Func<Nucleus.Core.Command.ICampaign> getCampaign,
            System.Action<Nucleus.Core.Command.ICampaign> setCampaign)
        {
            _mod = mod;
            Log = log;
            Game = game;
            Buttons = buttons;
            _getCampaign = getCampaign;
            _setCampaign = setCampaign;
        }

        public ModInfo Info => _mod.Info;
        public bool IsEnabled => true;
        public ILogSink Log { get; }
        public IModUi Ui { get; } = new HostModUi();
        public IGameServices Game { get; }
        public IButtonRegistry Buttons { get; }   // host-owned, shared across mods (HostButtons)
        public T BindConfig<T>(string section, string key, T def, string description) => def;

        // Shared live campaign: the Commander publishes it once; every mod reads it (host-owned holder).
        public Nucleus.Core.Command.ICampaign Campaign => _getCampaign?.Invoke();
        public void ShareCampaign(Nucleus.Core.Command.ICampaign campaign) => _setCampaign?.Invoke(campaign);
    }

    /// <summary>Placeholder UI surface (a host-owned canvas layer is a later step). Commander uses its own
    /// runtime/canvas; Build/Squad reach the game via their bezel button + Game services for now.</summary>
    internal sealed class HostModUi : IModUi
    {
        public RectTransform CreateLayer(string name) => null;
        public Transform MapIconLayer => null;
        public Theme Theme { get; } = Theme.Default;
    }
}
