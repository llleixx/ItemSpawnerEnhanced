using ItemSpawnerEnhanced.Core;
using NUnit.Framework;

namespace ItemSpawnerEnhanced.Core.Tests;

public sealed class ControllerBindingRulesTests
{
    [TestCase(null, "")]
    [TestCase("", "")]
    [TestCase("  None  ", "")]
    [TestCase("<Gamepad>/select", "<Gamepad>/select")]
    [TestCase("  <Gamepad>/rightTrigger  ", "<Gamepad>/rightTrigger")]
    public void NormalizeBindingPath_ReturnsExpectedValue(string? path, string expected) =>
        Assert.That(ControllerBindingRules.NormalizeBindingPath(path), Is.EqualTo(expected));
}
