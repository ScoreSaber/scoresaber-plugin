using PropertyChanged.SourceGenerator;
using ScoreSaber.Components;

namespace ScoreSaber.UI
{
    internal partial class NotifyTest : ScoreSaberViewController
    {
        [Notify] private string _hello;
    }
}
