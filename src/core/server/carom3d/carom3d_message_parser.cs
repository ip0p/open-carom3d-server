using System.Collections.Generic;

namespace core
{
    public struct ParsedDataResultInfo
    {
        public List<ActionData> parsedActions;
        public uint parsedTotalLen;
    }

    public class Carom3DMessageParser
    {
        public ParsedDataResultInfo ParseMessageData(CryptoContext cryptoCtx, byte[] data, uint dataLen)
        {
            ParsedDataResultInfo info = new ParsedDataResultInfo();
            int i = 0;
            uint parsedLen = 0;
            uint unparsedLen = dataLen;
            while (unparsedLen >= 8)
            {
                byte[] d = new byte[unparsedLen];
                System.Array.Copy(data, parsedLen, d, 0, unparsedLen);
                int actionId = System.BitConverter.ToInt32(d, 0);
                uint actionDataLen = System.BitConverter.ToUInt32(d, 4);
                cryptoCtx.Crypt(d, 0, 4);
                cryptoCtx.Crypt(d, 4, 4);
                if ((unparsedLen - 8) < actionDataLen)
                {
                    //Couldn't parse
                    //TODO: set error to info
                    return info;
                }
                byte[] actionData = new byte[actionDataLen];
                System.Array.Copy(d, 8, actionData, 0, actionDataLen);
                cryptoCtx.Crypt(actionData, 0, actionDataLen);
                info.parsedActions.Add(new ActionData(actionId, actionData));
                parsedLen += 8 + actionDataLen;
                unparsedLen = dataLen - parsedLen;
                cryptoCtx.Update();
            }
            info.parsedTotalLen = parsedLen;
            return info;
        }

        public void RunServerUsingGitHubActions()
        {
            Console.WriteLine("Running Carom3D message parser server using GitHub Actions...");
            // Add your GitHub Actions specific code here
        }

        public void CompileProjectUsingGitHubActions()
        {
            Console.WriteLine("Compiling Carom3D message parser project using GitHub Actions...");
            // Add your GitHub Actions specific code here
        }
    }
}
