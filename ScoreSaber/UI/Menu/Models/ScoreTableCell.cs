using BeatSaberMarkupLanguage.Attributes;
using PropertyChanged.SourceGenerator;
using TMPro;

namespace ScoreSaber.UI.Menu.Models;

internal sealed partial class ScoreTableCell
{
    [Notify]
    private int _rank;

    [Notify]
    private string _name = "Not Defined";

    [Notify]
    private int _score;

    [Notify]
    private bool _visible = true;

    [UIComponent("name")]
    private readonly TMP_Text _nameComponent = null!;

    private void OnNameChanged(string _, string __)
    {
        _nameComponent.ForceMeshUpdate(true);
    }
}