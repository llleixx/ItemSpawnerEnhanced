using ItemSpawnerEnhanced.Core;
using NUnit.Framework;

namespace ItemSpawnerEnhanced.Core.Tests;

public sealed class InputBindingRulesTests
{
    [TestCase(null, "")]
    [TestCase("", "")]
    [TestCase("  None  ", "")]
    [TestCase("<Keyboard>/f5", "<Keyboard>/f5")]
    [TestCase("<Gamepad>/select", "<Gamepad>/select")]
    [TestCase("  <Gamepad>/rightTrigger  ", "<Gamepad>/rightTrigger")]
    public void NormalizeBindingPath_ReturnsExpectedValue(string? path, string expected) =>
        Assert.That(InputBindingRules.NormalizeBindingPath(path), Is.EqualTo(expected));
}
