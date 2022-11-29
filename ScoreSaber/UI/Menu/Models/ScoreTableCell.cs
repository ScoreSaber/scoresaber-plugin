using PropertyChanged.SourceGenerator;

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
}