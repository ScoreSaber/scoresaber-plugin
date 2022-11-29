using IPA;
using IPA.Loader;
using SiraUtil.Attributes;
using SiraUtil.Zenject;
using IPALogger = IPA.Logging.Logger;

namespace ScoreSaber;

[Slog]
[NoEnableDisable]
[Plugin(RuntimeOptions.DynamicInit)]
internal class Plugin
{
    [Init]
    public Plugin(IPALogger logger, PluginMetadata metadata, Zenjector zenjector)
    {
        zenjector.UseLogger(logger);
    }
}