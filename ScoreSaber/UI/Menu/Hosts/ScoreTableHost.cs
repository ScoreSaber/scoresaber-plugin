using JetBrains.Annotations;
using PropertyChanged.SourceGenerator;
using ScoreSaber.UI.Menu.Models;
using System.Collections.Generic;
using System.Linq;

namespace ScoreSaber.UI.Menu.Hosts;

internal sealed partial class ScoreTableHost
{
    private const int MaximumCellCount = 10;

    [Notify]
    [UsedImplicitly]
    private List<ScoreTableCell> _cells = Enumerable.Range(0, MaximumCellCount).Select(_ => new ScoreTableCell()).ToList();

    [Notify]
    [AlsoNotify(nameof(Loaded))]
    private bool _loading;

    public bool Loaded => !Loading;
}