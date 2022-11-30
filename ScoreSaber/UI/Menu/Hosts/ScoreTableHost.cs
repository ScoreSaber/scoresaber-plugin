using BeatSaberMarkupLanguage.Attributes;
using PropertyChanged.SourceGenerator;
using ScoreSaber.UI.Menu.Models;
using System.Collections.Generic;
using System.Linq;

namespace ScoreSaber.UI.Menu.Hosts;

internal sealed partial class ScoreTableHost
{
    private const int MaximumCellCount = 10;

    [Notify(set: Setter.Private)]
    private List<ScoreTableCell> _cells = Enumerable.Range(0, MaximumCellCount).Select(_ => new ScoreTableCell()).ToList();

    [Notify(Getter.Private, Setter.Private)]
    [AlsoNotify(nameof(Loaded))]
    private bool _loading;

    [UIValue(nameof(Loaded))]
    private bool Loaded => !Loading;
}