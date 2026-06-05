using System.Collections.Generic;
using UnityEngine;

namespace CommanderLayer.Game
{
    /// <summary>
    /// S0 de-risk instrumentation (behind the <c>CommanderDebug</c> config flag, default off). Logs
    /// structured "[S0:*]" lines to the BepInEx console so ONE playtest resolves the runtime unknowns:
    /// unit-id stability, kill/track pruning, and terrain water/land (which also seeds the P0.5 sandbox).
    /// Throwaway / foldable once findings are recorded in PROGRESS.md.
    /// </summary>
    public sealed class CommanderDebugProbe
    {
        private int _tick;
        private bool _terrainLogged;
        private readonly Dictionary<string, int> _firstSeenTick = new Dictionary<string, int>();

        public void Tick()
        {
            if (!Plugin.CommanderDebug) return;
            _tick++;
            LogRoster();   // UID stability
            LogTracking(); // KILL / prune detection
            if (!_terrainLogged) { _terrainLogged = true; LogTerrain(); } // one-shot grid for the sandbox
        }

        // UID: friendly unit persistent ids over time — are they stable & non-reused after death?
        private void LogRoster()
        {
            if (!GameManager.GetLocalHQ(out var hq) || hq == null) return;
            var units = UnitRegistry.allUnits;
            if (units == null) return;
            int n = 0;
            foreach (var u in units)
            {
                if (u == null || u.NetworkHQ != hq) continue;
                string id = u.persistentID.ToString();
                if (!_firstSeenTick.ContainsKey(id)) { _firstSeenTick[id] = _tick; Plugin.Log?.LogInfo($"[S0:UID] new {id} {u.unitName} t={_tick}"); }
                n++;
            }
            if (_tick % 5 == 0) Plugin.Log?.LogInfo($"[S0:UID] friendly count={n} distinctSeen={_firstSeenTick.Count} t={_tick}");
        }

        // KILL: known-enemy tracking entries — does destroying one prune it from trackingDatabase, and when?
        private void LogTracking()
        {
            if (_tick % 3 != 0) return;
            if (!GameManager.GetLocalHQ(out var hq) || hq == null) return;
            var db = hq.trackingDatabase;
            if (db == null) return;
            int disabled = 0;
            foreach (var kv in db)
                if (kv.Value != null && kv.Value.TryGetUnit(out var u) && u != null && u.disabled) disabled++;
            Plugin.Log?.LogInfo($"[S0:KILL] tracked={db.Count} disabledStillTracked={disabled} t={_tick}");
        }

        // TERRAIN: water-vs-land across a coarse grid → confirms the sampling API + seeds sandbox coordinates.
        private void LogTerrain()
        {
            var terrain = Terrain.activeTerrain;
            Plugin.Log?.LogInfo($"[S0:TERRAIN] activeTerrain={(terrain != null ? terrain.name : "null")} sea={Datum.SeaLevel.y:0.0}");
            if (terrain == null) return;
            float baseY = terrain.transform.position.y;
            for (int gz = -30000; gz <= 30000; gz += 10000)
                for (int gx = -30000; gx <= 30000; gx += 10000)
                {
                    var p = new Vector3(gx, 0f, gz);
                    float h = terrain.SampleHeight(p) + baseY;
                    string kind = h <= Datum.SeaLevel.y ? "WATER" : "land";
                    Plugin.Log?.LogInfo($"[S0:TERRAIN] ({gx},{gz}) h={h:0} {kind}");
                }
        }
    }
}
