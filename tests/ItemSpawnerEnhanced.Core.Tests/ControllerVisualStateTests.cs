using ItemSpawnerEnhanced.Core;
using NUnit.Framework;

namespace ItemSpawnerEnhanced.Core.Tests;

public sealed class ControllerVisualStateTests
{
    [TestCase(false, false, (int)UiPaletteToken.Surface, (int)UiPaletteToken.TextPrimary, false)]
    [TestCase(false, true, (int)UiPaletteToken.Surface, (int)UiPaletteToken.TextPrimary, true)]
    [TestCase(true, false, (int)UiPaletteToken.Accent, (int)UiPaletteToken.Panel, false)]
    [TestCase(true, true, (int)UiPaletteToken.Accent, (int)UiPaletteToken.Panel, true)]
    public void TagState_PreservesActivationAndFocusAsIndependentSignals(
        bool active,
        bool focused,
        int background,
        int foreground,
        bool showFocus)
    {
        ControlVisualState state = ControllerVisualState.ForTag(active, focused, false, false);
        Assert.Multiple(() =>
        {
            Assert.That(state.Background, Is.EqualTo((UiPaletteToken)background));
            Assert.That(state.Foreground, Is.EqualTo((UiPaletteToken)foreground));
            Assert.That(state.ShowFocus, Is.EqualTo(showFocus));
        });
    }

    [Test]
    public void CurrentTargetOption_RemainsActiveWhileFocused()
    {
        ControlVisualState state = ControllerVisualState.ForTargetOption(true, true, false);
        Assert.Multiple(() =>
        {
            Assert.That(state.Background, Is.EqualTo(UiPaletteToken.Accent));
            Assert.That(state.Foreground, Is.EqualTo(UiPaletteToken.Panel));
            Assert.That(state.ShowFocus, Is.True);
        });
    }

    [Test]
    public void UnavailableItem_FocusOnlyAddsOutline()
    {
        ControlVisualState state = ControllerVisualState.ForItem(false, true, false, false);
        Assert.Multiple(() =>
        {
            Assert.That(state.Background, Is.EqualTo(UiPaletteToken.DisabledSurface));
            Assert.That(state.Foreground, Is.EqualTo(UiPaletteToken.TextMuted));
            Assert.That(state.ShowFocus, Is.True);
        });
    }

    [Test]
    public void DisabledAction_NeverShowsFocus()
    {
        ControlVisualState state = ControllerVisualState.ForAction(false, true, false, false);
        Assert.Multiple(() =>
        {
            Assert.That(state.Background, Is.EqualTo(UiPaletteToken.DisabledSurface));
            Assert.That(state.Foreground, Is.EqualTo(UiPaletteToken.TextMuted));
            Assert.That(state.ShowFocus, Is.False);
        });
    }
}
