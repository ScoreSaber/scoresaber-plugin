using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using PropertyChanged.SourceGenerator;
using ScoreSaber.UI.Menu.Models;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

namespace ScoreSaber.UI.Menu;

[HotReload(RelativePathToLayout = @"Views\ScoreView.bsml")]
[ViewDefinition("ScoreSaber.UI.Menu.Views.ScoreView.bsml")]
internal sealed partial class ScoreViewController : BSMLAutomaticViewController
{
    [Notify]
    private List<IconSegmentedControl.DataItem> _scopes = new();

    [Notify]
    private List<ScoreTableCell> _cells = Enumerable.Range(0, 10).Select(_ => new ScoreTableCell()).ToList();

    [UIComponent("ScopesSegmentedControl")]
    private readonly IconSegmentedControl _scopesSegmentedControl = null!;

    protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
    {
        if (firstActivation)
        {
            var global = BeatSaberMarkupLanguage.Utilities.FindSpriteCached("GlobalIcon");
            var player = BeatSaberMarkupLanguage.Utilities.FindSpriteCached("PlayerIcon");
            var friends = BeatSaberMarkupLanguage.Utilities.FindSpriteCached("FriendsIcon");
            var country = BeatSaberMarkupLanguage.Utilities.FindSpriteInAssembly("ScoreSaber.Resources.Country.png");

            _scopes.Add(new IconSegmentedControl.DataItem(global, "Global"));
            _scopes.Add(new IconSegmentedControl.DataItem(player, "Player"));
            _scopes.Add(new IconSegmentedControl.DataItem(friends, "Friends"));
            _scopes.Add(new IconSegmentedControl.DataItem(country, "Country"));
        }

        base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
    }

    [UIAction("#post-parse")]
    public void Parsed()
    {
        var horizontalGroup = _scopesSegmentedControl.GetComponent<HorizontalLayoutGroup>();
        DestroyImmediate(horizontalGroup);
        _scopesSegmentedControl.gameObject.AddComponent<VerticalLayoutGroup>();
    }
}