using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;

namespace ScoreSaber.UI.Menu;

[HotReload(RelativePathToLayout = @"Views\PanelView.bsml")]
[ViewDefinition("ScoreSaber.UI.Menu.Views.PanelView.bsml")]
internal sealed class PanelViewController : BSMLAutomaticViewController
{
}