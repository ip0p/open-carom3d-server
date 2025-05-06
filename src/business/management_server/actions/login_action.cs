using System;
using business.service;
using business.entity;

namespace business.management_server.actions
{
    public class LoginAction : ManagementServerAction<LoginData>
    {
        public override bool Validate(ActionData action)
        {
            int size = action.Data.Length;
            int expectedSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(LoginData));
            return size == expectedSize;
        }

        public override void Execute(ActionData action, User user, LoginData data)
        {
            Console.WriteLine($"Player logged in: {data.Username}, {data.Password}");

            Account account = AccountService.GetInstance().LogAccount(data.Username, data.Password);
            Player player;
            if (account == null)
            {
                account = AccountService.GetInstance().CreateAccount(data.Username, data.Password);
                player = AccountService.GetInstance().CreatePlayerFromAccount(account);
            }
            else
            {
                player = AccountService.GetInstance().GetPlayer(account);
            }

            account.SetPlayer(player);

            user.SetAccount(account);
            user.SetPlayer(player);

            SendLoginResult(0x00, user);
        }

        private void SendLoginResult(uint result, User user)
        {
            Player player = user.Player;

            MSLoginResult loginResult = new MSLoginResult
            {
                Result = result,
                PlayerName = new string('\0', 21),
                TotalPoints = (uint)player.Points,
                Unk = 0,
                AccountLevel = 0x3C,
                _Unk = 0,
                _Unk1 = 0,
                _Unk2 = new byte[6],
                _Unk3 = 0,
                _Unk4 = 0,
                _Unk5 = 0,
                _Unk6 = new byte[8],
                _Unk7 = 0,
                Ip = "127.0.0.1",
                Unk2 = 0,
                Unk3 = 0,
                Unk4 = 0,
                Unk5 = 0,
                Unk6 = 0,
                Unk7 = 0
            };

            Array.Copy(player.Name.ToCharArray(), loginResult.PlayerName.ToCharArray(), Math.Min(player.Name.Length, 20));
            loginResult.PlayerName = loginResult.PlayerName.Substring(0, 20) + '\0';

            ActionData actionData = new ActionData(0x05, loginResult.ToByteArray());
            user.SendAction(actionData);
        }
    }

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
    public struct MSLoginResult
    {
        public uint Result;
        [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValTStr, SizeConst = 21)]
        public string PlayerName;
        public uint TotalPoints;
        public uint Unk;
        public uint AccountLevel;
        public uint _Unk;
        public uint _Unk1;
        [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst = 6)]
        public byte[] _Unk2;
        public uint _Unk3;
        public uint _Unk4;
        public uint _Unk5;
        [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] _Unk6;
        public ushort _Unk7;
        [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValTStr, SizeConst = 16)]
        public string Ip;
        public uint Unk2;
        public uint Unk3;
        public uint Unk4;
        public uint Unk5;
        public uint Unk6;
        public uint Unk7;
    }

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
    public struct LoginData
    {
        [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValTStr, SizeConst = 21)]
        public string Username;
        [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValTStr, SizeConst = 13)]
        public string Password;
        [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValTStr, SizeConst = 4)]
        public string Country;
        [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst = 144)]
        public byte[] GpuInfo;
        public int Unk;
    }
}
