namespace CommanderLayer.Core.Command
{
    /// <summary>
    /// Tunable command rules. A single <see cref="RiskTolerance"/> master (0 = cautious, 1 = aggressive)
    /// derives the combined-arms thresholds in P2 (air-superiority ratio, residual-AD tolerance, soften
    /// fraction) and the force-sizing ratio. Pure data; layered per Commander/Operation/Squad later.
    /// </summary>
    public sealed class Doctrine
    {
        public float RiskTolerance { get; set; } = 0.5f;

        /// <summary>How many times the known threat an offensive force should outnumber (force-sizing).</summary>
        public float ForceRatio { get; set; } = 1.5f;
    }
}
