using Nucleus.Abstractions;
using Nucleus.Core.Command;
using Nucleus.Core.Persistence;
using Nucleus.Ui;

namespace Nucleus.Warfare
{
    /// <summary>
    /// Nucleus Dynamic Warfare: a persistent two-faction war where both sides run the autonomous commander.
    /// Owns the <see cref="WarfareCampaign"/> and its save/resume to disk (resumes any existing save on load).
    /// The headless substrate — dual-faction stepping + lossless save/resume + continuation determinism — is
    /// proven in Nucleus.Sim.Tests; this mod drives it in-game. The per-faction battlefield views come from the
    /// "Nucleus Dynamic Warfare" mission (which grants both sides' rosters); until that mission runs, the WAR
    /// button reports campaign status and the campaign persists across sessions.
    /// </summary>
    public sealed class WarfareMod : IMod
    {
        private readonly string _savePath;
        private WarfareCampaign _campaign;
        private IModContext _ctx;
        private CommanderPanel _panel;
        // Live attrition feed: faction-name -> (alive units, airbases) from the last census, to diff for losses.
        private readonly System.Collections.Generic.Dictionary<string, (int units, int bases)> _lastCensus
            = new System.Collections.Generic.Dictionary<string, (int, int)>();
        private string _bluforFaction, _opforFaction;  // which mission faction maps to each war side

        public WarfareMod(string savePath) { _savePath = savePath; }

        public ModInfo Info { get; } = new ModInfo
        {
            Id = "warfare",
            DisplayName = "Warfare",
            Version = "0.1.0",
            Author = "Nucleus",
            Description = "Persistent two-faction dynamic war (both sides run the AI commander); save and resume.",
        };

        public void Initialize(IModContext ctx)
        {
            _ctx = ctx;
            _campaign = WarfareSave.Load(_savePath) ?? new WarfareCampaign();

            ctx.Log.Info("[NUCLEUS:SELFTEST] PASS warfare-mod-loaded");
            ctx.Log.Info($"[NUCLEUS:METRIC] warfareTurn={_campaign.Turn}");

            ctx.Buttons.RegisterMapButton(new MapButtonSpec
            {
                ModId = Info.Id,
                Label = "WAR",
                BuildContent = parent =>
                {
                    // Campaign view: operations + battle feed of the shared commander. Save/resume of the
                    // two-faction war is automatic (resume on load, persist on shutdown).
                    _panel = new CommanderPanel(parent, ctx.Ui.Theme, onArm: null, onClearAll: null,
                        onClearOrder: null, onToggleOpManual: id => ctx.Campaign?.ToggleOperationManual(id),
                        sections: CommanderPanel.PanelSections.Scoreboard | CommanderPanel.PanelSections.Operations
                                | CommanderPanel.PanelSections.Feed);
                    UiFactory.Stretch(_panel.Root);
                },
                OnClick = ReportStatus,
            });
        }

        /// <summary>The live campaign (for the mission driver / future Warfare panel).</summary>
        public WarfareCampaign Campaign => _campaign;

        /// <summary>Persist the current war so it can be resumed next session.</summary>
        public void Save() => WarfareSave.Save(_savePath, _campaign);

        private void ReportStatus()
        {
            _ctx?.Log.Info($"[Warfare] turn {_campaign.Turn} — Blufor: {_campaign.Blufor.Objectives.Count} obj / "
                + $"{_campaign.Blufor.Operations.Count} ops · Opfor: {_campaign.Opfor.Objectives.Count} obj / "
                + $"{_campaign.Opfor.Operations.Count} ops");
        }

        public void Tick(IModTickContext t)
        {
            FeedAttrition();
            var c = _ctx?.Campaign;
            if (_panel != null && c != null) _panel.RenderHq(c.Hq(), c.Catalog(), c.Funds());
            // The attrition board reads from the Warfare campaign (both factions' score/funds/losses + win state).
            if (_panel != null && _campaign != null) _panel.RenderScoreboard(_campaign.SnapshotBoard());
        }

        // Diff the live per-faction census against last tick; feed unit/base drops into the attrition score.
        // The two mission factions are mapped to the Blufor/Opfor war sides on first sight (sorted by name, so
        // the mapping is stable across sessions). Exception-safe — a missing census just doesn't advance score.
        private void FeedAttrition()
        {
            if (_ctx?.Game == null || _campaign == null) return;
            var census = _ctx.Game.WarCensus();
            if (census == null || census.Count == 0) return;

            // First sight: bind the two factions to the war sides (deterministic order) + show real names.
            if (_bluforFaction == null && census.Count >= 2)
            {
                var names = new System.Collections.Generic.List<string>();
                foreach (var f in census) names.Add(f.FactionName);
                names.Sort(System.StringComparer.Ordinal);
                _bluforFaction = names[0];
                _opforFaction = names[1];
                _campaign.War.Blufor.FactionName = _bluforFaction;
                _campaign.War.Opfor.FactionName = _opforFaction;
                _ctx.Log.Info($"[NUCLEUS:SELFTEST] PASS warfare-factions-bound blufor={_bluforFaction} opfor={_opforFaction}");
            }

            foreach (var f in census)
            {
                bool isBlu = f.FactionName == _bluforFaction;
                bool isOp = f.FactionName == _opforFaction;
                if (!isBlu && !isOp) continue;

                if (_lastCensus.TryGetValue(f.FactionName, out var prev))
                {
                    int unitDrop = prev.units - f.AliveUnits;
                    int baseDrop = prev.bases - f.Airbases;
                    if (unitDrop > 0) _campaign.RecordUnitLost(isBlu, unitDrop);
                    if (baseDrop > 0) _campaign.RecordBaseLost(isBlu, baseDrop);
                }
                _lastCensus[f.FactionName] = (f.AliveUnits, f.Airbases);
            }
        }

        public void OnEnabled() { }
        public void OnDisabled() { }
        public void Shutdown()
        {
            // Persist on shutdown so a multi-hour war survives quitting the game.
            if (_campaign != null && _campaign.Turn > 0) Save();
        }
    }
}
