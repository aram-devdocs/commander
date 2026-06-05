namespace CommanderLayer.Core.Command
{
    /// <summary>
    /// How much the player has taken over a given entity (commander / operation / squad). The autonomy
    /// ladder: <see cref="Auto"/> = the AI runs it; <see cref="Assisted"/> = the AI proposes and the player
    /// confirms; <see cref="Manual"/> = the player drives it directly. Taking a "slice" = setting one entity
    /// to Manual; the AI fills the rest. Default is Auto so "do nothing = the game still runs".
    /// </summary>
    public enum AutonomyLevel
    {
        Auto,
        Assisted,
        Manual
    }
}
