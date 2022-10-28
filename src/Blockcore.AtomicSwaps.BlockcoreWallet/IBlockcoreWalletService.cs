﻿using Microsoft.JSInterop;
using System;
using System.Numerics;
using System.Threading.Tasks;

namespace Blockcore.AtomicSwaps.BlockcoreWallet
{
    public interface IBlockcoreWalletService
    {

        ValueTask ConnectBlockcoreWallet();
        ValueTask DisposeAsync();
        ValueTask<bool> HasBlockcoreWallet();
        ValueTask<bool> IsSiteConnected();
        ValueTask<string> SignMessage(string msg);



    }
}