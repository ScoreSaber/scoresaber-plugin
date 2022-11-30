using IPA;
using IPA.Loader;
using ScoreSaber.Installers;
using ScoreSaber.Networking;
using SiraUtil.Attributes;
using SiraUtil.Zenject;
using IPALogger = IPA.Logging.Logger;

namespace ScoreSaber;

[Slog]
[NoEnableDisable]
[Plugin(RuntimeOptions.DynamicInit)]
internal sealed class Plugin
{
    public const string ApiUrl = "https://scoresaber.com/api";

    [Init]
    public Plugin(IPALogger logger, PluginMetadata metadata, Zenjector zenjector)
    {
        zenjector.UseHttpService();
        zenjector.UseLogger(logger);
        zenjector.Install<ScoreSaberCoreInstaller>(Location.App);
        zenjector.Install<ScoreSaberMenuInstaller>(Location.Menu);
        zenjector.Install<ScoreSaberMenuUIInstaller>(Location.Menu);

        zenjector.Install(Location.App, Container => Container.Bind<HttpOptions>().FromInstance(new HttpOptions
        {
            ApplicationName = "ScoreSaber-PC",
            Version = metadata.HVersion,
            BaseURL = ApiUrl,
        }));
    }
}