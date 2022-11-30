using ScoreSaber.Models;
using System.Threading.Tasks;

namespace ScoreSaber.Services;

internal interface IPlatformLoginService
{
    Task<LocalPlatformUserInfo?> LoginAsync();
}