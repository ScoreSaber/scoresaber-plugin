using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using JetBrains.Annotations;
using PropertyChanged.SourceGenerator;
using System.Collections.Generic;
using UnityEngine.UI;

namespace ScoreSaber.UI.Menu.Hosts;

internal sealed partial class ScoreScopeHost
{
    [UsedImplicitly]
    [Notify(set: Setter.Private)]
    private List<IconSegmentedControl.DataItem> _scopes = new()
    {
        new IconSegmentedControl.DataItem(BeatSaberMarkupLanguage.Utilities.FindSpriteCached("GlobalIcon"), "Global"),
        new IconSegmentedControl.DataItem(BeatSaberMarkupLanguage.Utilities.FindSpriteCached("PlayerIcon"), "Player"),
        new IconSegmentedControl.DataItem(BeatSaberMarkupLanguage.Utilities.FindSpriteCached("FriendsIcon"), "Friends"),
        new IconSegmentedControl.DataItem(BeatSaberMarkupLanguage.Utilities.FindSpriteInAssembly("ScoreSaber.Resources.Country.png"), "Country"),
    };

    [UIComponent("ScopesSegmentedControl")]
    private readonly IconSegmentedControl _scopesSegmentedControl = null!;

    [UIAction("#post-parse")]
    public void Parsed()
    {
        // The BSML IconSegmentedControl prefab uses a HorizontalLayoutGroup, but
        // we want these icons to be vertical.
        var horizontalGroup = _scopesSegmentedControl.GetComponent<HorizontalLayoutGroup>();
        UnityEngine.Object.DestroyImmediate(horizontalGroup);
        _scopesSegmentedControl.gameObject.AddComponent<VerticalLayoutGroup>();
    }
}