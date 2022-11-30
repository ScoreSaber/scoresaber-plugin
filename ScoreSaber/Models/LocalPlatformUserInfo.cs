namespace ScoreSaber.Models;

internal class LocalPlatformUserInfo
{
    public string Id { get; }
    public string Name { get; }
    public string Nonce { get; }
    public string Friends { get; }
    public string AuthType { get; }

    public LocalPlatformUserInfo(string id, string name, string nonce, string friends, string authType)
    {
        Id = id;
        Name = name;
        Nonce = nonce;
        Friends = friends;
        AuthType = authType;
    }
}