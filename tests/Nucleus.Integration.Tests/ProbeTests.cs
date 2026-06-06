using Nucleus.Abstractions;
using Xunit;

namespace Nucleus.Integration.Tests
{
    /// <summary>
    /// Probe: can a net8.0 test load the Unity-referencing Nucleus.Abstractions assembly and construct its
    /// types? If yes, the host lifecycle is headless-testable with fakes; if this throws a TypeLoad/
    /// FileNotFound, the Unity-coupled host stays build+playtest-gated.
    /// </summary>
    public class ProbeTests
    {
        [Fact]
        public void Can_load_abstractions_and_construct_modinfo()
        {
            var info = new ModInfo { Id = "probe", DisplayName = "Probe", Version = "0.0.1" };
            Assert.Equal("probe", info.Id);
        }
    }
}
