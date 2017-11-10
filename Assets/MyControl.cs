using SteamMultiplayer;
using Steamworks;

public class MyControl : NetworkControl{

    public override void Analyze(P2PPackage package, CSteamID steamid)
    {
        base.Analyze(package, steamid);

        if (package.type == P2PPackageType.AnimatorParamter)
        {
            
        }
    }
}
