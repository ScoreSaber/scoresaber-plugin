using PropertyChanged.SourceGenerator;

namespace ScoreSaber.UI
{
    internal partial class NotifyTest : ScoreSaberViewController
    {
        [Notify] private string _hello;
    }
}
