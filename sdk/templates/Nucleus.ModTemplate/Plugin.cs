using BepInEx;
using CommanderLayer.Abstractions;

namespace MyMod
{
    /// <summary>
    /// BepInEx entry point. Hard-depends on the Nucleus platform so the host loads first, then registers this
    /// mod with it. The platform owns the shared canvas, tick pump, and game services; your logic lives in
    /// <see cref="Mod"/>.
    /// </summary>
    [BepInPlugin("com.example.mymod", "MyMod", "0.1.0")]
    [BepInDependency(ModPlatform.Guid, BepInDependency.DependencyFlags.HardDependency)]
    public class Plugin : BaseUnityPlugin
    {
        private void Awake() => ModPlatform.Register(new Mod());
    }
}
