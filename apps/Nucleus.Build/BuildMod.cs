using Nucleus.Abstractions;
using Nucleus.Ui;
using UnityEngine;

namespace Nucleus.Build
{
    /// <summary>
    /// Manual production: the buy menu. Renders the BUILD slice of the shared campaign (convoy catalog +
    /// funds) into its own native MFD screen, and queues purchases through the shared campaign so auto + manual
    /// buys share the one production queue.
    /// </summary>
    public sealed class BuildMod : IMod
    {
        private IModContext _ctx;
        private CommanderPanel _panel;

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
            ctx.Log.Info("[NUCLEUS:SELFTEST] PASS build-mod-loaded");
            ctx.Log.Info($"[NUCLEUS:METRIC] buildFunds={(int)ctx.Game.Funds()}");

            ctx.Buttons.RegisterMapButton(new MapButtonSpec
            {
                ModId = Info.Id,
                Label = "BLD",
                BuildContent = parent =>
                {
                    _panel = new CommanderPanel(parent, ctx.Ui.Theme, onArm: null, onClearAll: null,
                        onClearOrder: null, onBuyConvoy: name => ctx.Campaign?.BuyConvoy(name),
                        sections: CommanderPanel.PanelSections.Build);
                    UiFactory.Stretch(_panel.Root);
                },
            });
        }

        public void Tick(IModTickContext t)
        {
            var c = _ctx?.Campaign;
            if (_panel != null && c != null) _panel.RenderHq(c.Hq(), c.Mode(), c.Catalog(), c.Funds());
        }

        public void OnEnabled() { }
        public void OnDisabled() { }
        public void Shutdown() { }
    }
}
