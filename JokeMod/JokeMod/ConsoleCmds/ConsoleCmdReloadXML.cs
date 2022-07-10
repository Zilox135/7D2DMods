using System;
using System.Collections.Generic;

public class ConsoleCmdReloadXML : ConsoleCmdAbstract
{
    public override string[] GetCommands()
    {
        return new string[2]
        {
            "ReloadXML",
            "RX"
        };
    }

    public override string GetDescription()
    {
        return "Reloads all XML while in-game";
    }

    public override string GetHelp()
    {
        return "Reloads all XML in-game without needing to reload a save.\n Usage:\n  ReloadXML";
    }
    public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
    {
        WorldStaticData.ReloadAllXmlsSync();
    }
}
