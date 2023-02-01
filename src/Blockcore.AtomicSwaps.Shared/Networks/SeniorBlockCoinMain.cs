using System.Net;
using Blockcore.Consensus;
using Blockcore.Consensus.BlockInfo;
using Blockcore.Consensus.Checkpoints;
using Blockcore.Consensus.ScriptInfo;
using Blockcore.Consensus.TransactionInfo;
using Blockcore.Networks;
using Blockcore.P2P;
using NBitcoin;
using NBitcoin.BitcoinCore;
using NBitcoin.BouncyCastle.Math;
using NBitcoin.DataEncoders;

namespace Blockcore.AtomicSwaps.Shared.Networks
{
    public class SeniorBlockCoinMain : Network
   {
      public SeniorBlockCoinMain()
      {
          SeniorBlockCoinSetup.CoinSetup setup = SeniorBlockCoinSetup.Instance.Setup;
          SeniorBlockCoinSetup.NetworkSetup network = SeniorBlockCoinSetup.Instance.Main;

         NetworkType = NetworkType.Mainnet;
         DefaultConfigFilename = setup.ConfigFileName; // The default name used for the SeniorBlockCoin configuration file.

         Name = network.Name;
         CoinTicker = network.CoinTicker;
         Magic = ConversionTools.ConvertToUInt32(setup.Magic);
         RootFolderName = network.RootFolderName;
         DefaultPort = network.DefaultPort;
         DefaultRPCPort = network.DefaultRPCPort;
         DefaultAPIPort = network.DefaultAPIPort;

         DefaultMaxOutboundConnections = 16;
         DefaultMaxInboundConnections = 109;
         MaxTipAge = 2 * 60 * 60;
         MinTxFee = 10000;
         MaxTxFee = Money.Coins(1).Satoshi;
         FallbackFee = 25000;
         MinRelayTxFee = 10000;
         MaxTimeOffsetSeconds = 25 * 60;
         DefaultBanTimeSeconds = 16000; // 500 (MaxReorg) * 64 (TargetSpacing) / 2 = 4 hours, 26 minutes and 40 seconds

         var consensusFactory = new PosConsensusFactory();

         // Create the genesis block.
         GenesisTime = network.GenesisTime;
         GenesisNonce = network.GenesisNonce;
         GenesisBits = network.GenesisBits;
         GenesisVersion = network.GenesisVersion;
         GenesisReward = network.GenesisReward;

         Block genesisBlock = CreateGenesisBlock(consensusFactory,
            GenesisTime,
            GenesisNonce,
            GenesisBits,
            GenesisVersion,
            GenesisReward,
            setup.GenesisText);

         Genesis = genesisBlock;

         var consensusOptions = new PosConsensusOptions
         {
            MaxBlockBaseSize = 1_000_000,
            MaxStandardVersion = 2,
            MaxStandardTxWeight = 100_000,
            MaxBlockSigopsCost = 20_000,
            MaxStandardTxSigopsCost = 20_000 / 5,
            WitnessScaleFactor = 4
         };

         Consensus = new Blockcore.Consensus.Consensus(
             consensusFactory: consensusFactory,
             consensusOptions: consensusOptions,
             coinType: setup.CoinType,
             hashGenesisBlock: genesisBlock.GetHash(),
             subsidyHalvingInterval: 3000000,
             majorityEnforceBlockUpgrade: 750,
             majorityRejectBlockOutdated: 950,
             majorityWindow: 1000,
             buriedDeployments: null,
             bip9Deployments: null,
             bip34Hash: null,
             minerConfirmationWindow: 2016, // nPowTargetTimespan / nPowTargetSpacing
             maxReorgLength: 500,
             defaultAssumeValid: null,
             maxMoney: Money.Coins(setup.MaxSupply),
             coinbaseMaturity: 50,
             premineHeight: 2,
             premineReward: Money.Coins(setup.PremineReward),
             proofOfWorkReward: Money.Coins(setup.PoWBlockReward),
             targetTimespan: TimeSpan.FromSeconds(14 * 24 * 60 * 60), // two weeks
             targetSpacing: setup.TargetSpacing,
             powAllowMinDifficultyBlocks: false,
             posNoRetargeting: false,
             powNoRetargeting: false,
             powLimit: new Target(new uint256("00000fffffffffffffffffffffffffffffffffffffffffffffffffffffffffff")),
             minimumChainWork: null,
             isProofOfStake: true,
             lastPowBlock: setup.LastPowBlock,
             proofOfStakeLimit: new BigInteger(uint256.Parse("00000fffffffffffffffffffffffffffffffffffffffffffffffffffffffffff").ToBytes(false)),
             proofOfStakeLimitV2: new BigInteger(uint256.Parse("000000000000ffffffffffffffffffffffffffffffffffffffffffffffffffff").ToBytes(false)),
             proofOfStakeReward: Money.Coins(setup.PoSBlockReward),
             proofOfStakeTimestampMask: setup.ProofOfStakeTimestampMask
         );

         Consensus.PosEmptyCoinbase = SeniorBlockCoinSetup.Instance.IsPoSv3();
         Consensus.PosUseTimeFieldInKernalHash = SeniorBlockCoinSetup.Instance.IsPoSv3();

         // TODO: Set your Base58Prefixes
         Base58Prefixes = new byte[12][];
         Base58Prefixes[(int)Base58Type.PUBKEY_ADDRESS] = new byte[] { (byte)network.PubKeyAddress };
         Base58Prefixes[(int)Base58Type.SCRIPT_ADDRESS] = new byte[] { (byte)network.ScriptAddress };
         Base58Prefixes[(int)Base58Type.SECRET_KEY] = new byte[] { (byte)network.SecretAddress };

         Base58Prefixes[(int)Base58Type.ENCRYPTED_SECRET_KEY_NO_EC] = new byte[] { 0x01, 0x42 };
         Base58Prefixes[(int)Base58Type.ENCRYPTED_SECRET_KEY_EC] = new byte[] { 0x01, 0x43 };
         Base58Prefixes[(int)Base58Type.EXT_PUBLIC_KEY] = new byte[] { (0x04), (0x88), (0xB2), (0x1E) };
         Base58Prefixes[(int)Base58Type.EXT_SECRET_KEY] = new byte[] { (0x04), (0x88), (0xAD), (0xE4) };
         Base58Prefixes[(int)Base58Type.PASSPHRASE_CODE] = new byte[] { 0x2C, 0xE9, 0xB3, 0xE1, 0xFF, 0x39, 0xE2 };
         Base58Prefixes[(int)Base58Type.CONFIRMATION_CODE] = new byte[] { 0x64, 0x3B, 0xF6, 0xA8, 0x9A };
         Base58Prefixes[(int)Base58Type.ASSET_ID] = new byte[] { 23 };

         Bech32Encoders = new Bech32Encoder[2];
         var encoder = new Bech32Encoder(network.CoinTicker.ToLowerInvariant());
         Bech32Encoders[(int)Bech32Type.WITNESS_PUBKEY_ADDRESS] = encoder;
         Bech32Encoders[(int)Bech32Type.WITNESS_SCRIPT_ADDRESS] = encoder;

         Checkpoints = network.Checkpoints;
         DNSSeeds = network.DNS.Select(dns => new DNSSeedData(dns, dns)).ToList();
         SeedNodes = network.Nodes.Select(node => new NBitcoin.Protocol.NetworkAddress(IPAddress.Parse(node), network.DefaultPort)).ToList();

         StandardScriptsRegistry = new SeniorBlockCoinStandardScriptsRegistry();

         // 64 below should be changed to TargetSpacingSeconds when we move that field.
         Assert(DefaultBanTimeSeconds <= Consensus.MaxReorgLength * 64 / 2);

         Assert(Consensus.HashGenesisBlock == uint256.Parse(network.HashGenesisBlock));
         Assert(Genesis.Header.HashMerkleRoot == uint256.Parse(network.HashMerkleRoot));

      }

    

      protected static Block CreateGenesisBlock(ConsensusFactory consensusFactory, uint nTime, uint nNonce, uint nBits, int nVersion, Money genesisReward, string genesisText)
      {
         Transaction txNew = consensusFactory.CreateTransaction();
         txNew.Version = 1;

         if (txNew is IPosTransactionWithTime posTx)
         {
            posTx.Time = nTime;
         }

         txNew.AddInput(new TxIn()
         {
            ScriptSig = new Script(Op.GetPushOp(0), new Op()
            {
               Code = (OpcodeType)0x1,
               PushData = new[] { (byte)42 }
            }, Op.GetPushOp(Encoders.ASCII.DecodeData(genesisText)))
         });

         txNew.AddOutput(new TxOut()
         {
            Value = genesisReward,
         });

         Block genesis = consensusFactory.CreateBlock();
         genesis.Header.BlockTime = Utils.UnixTimeToDateTime(nTime);
         genesis.Header.Bits = nBits;
         genesis.Header.Nonce = nNonce;
         genesis.Header.Version = nVersion;
         genesis.Transactions.Add(txNew);
         genesis.Header.HashPrevBlock = uint256.Zero;
         genesis.UpdateMerkleRoot();

         return genesis;
      }
   }

    public class SeniorBlockCoinStandardScriptsRegistry : StandardScriptsRegistry
   {
       // See MAX_OP_RETURN_RELAY in stratisX, <script.h>
       public const int MaxOpReturnRelay = 83;

       // Need a network-specific version of the template list
       private readonly List<ScriptTemplate> standardTemplates = new List<ScriptTemplate>
       {
           PayToPubkeyHashTemplate.Instance,
           PayToPubkeyTemplate.Instance,
           PayToScriptHashTemplate.Instance,
           PayToMultiSigTemplate.Instance,
           new TxNullDataTemplate(MaxOpReturnRelay),
           PayToWitTemplate.Instance
       };

       public override List<ScriptTemplate> GetScriptTemplates => standardTemplates;

       public override void RegisterStandardScriptTemplate(ScriptTemplate scriptTemplate)
       {
           if (!standardTemplates.Any(template => (template.Type == scriptTemplate.Type)))
           {
               standardTemplates.Add(scriptTemplate);
           }
       }

       public override bool IsStandardTransaction(Transaction tx, Network network)
       {
           return base.IsStandardTransaction(tx, network);
       }

       public override bool AreOutputsStandard(Network network, Transaction tx)
       {
           return base.AreOutputsStandard(network, tx);
       }

       public override ScriptTemplate GetTemplateFromScriptPubKey(Script script)
       {
           return standardTemplates.FirstOrDefault(t => t.CheckScriptPubKey(script));
       }

       public override bool IsStandardScriptPubKey(Network network, Script scriptPubKey)
       {
           return base.IsStandardScriptPubKey(network, scriptPubKey);
       }

       public override bool AreInputsStandard(Network network, Transaction tx, CoinsView coinsView)
       {
           return base.AreInputsStandard(network, tx, coinsView);
       }
   }

    internal class SeniorBlockCoinSetup
    {
        internal static SeniorBlockCoinSetup Instance = new SeniorBlockCoinSetup();

        internal CoinSetup Setup = new CoinSetup
        {
            FileNamePrefix = "seniorblockcoin",
            ConfigFileName = "seniorblockcoin.conf",
            Magic = "01-53-42-43",
            CoinType = 5006,
            PremineReward = 256000000,
            PoWBlockReward = 100,
            PoSBlockReward = 100,
            LastPowBlock = 25000,
            MaxSupply = 1024000000,
            GenesisText = "The world becomes decentralized",
            TargetSpacing = TimeSpan.FromSeconds(64),
            ProofOfStakeTimestampMask = 0x0000000F, // 0x0000003F // 64 sec
            PoSVersion = 4
        };

        internal NetworkSetup Main = new NetworkSetup
        {
            Name = "SeniorBlockCoinMain",
            RootFolderName = "seniorblockcoin",
            CoinTicker = "SBC",
            DefaultPort = 15006,
            DefaultRPCPort = 15007,
            DefaultAPIPort = 15008,
            PubKeyAddress = 63, // S 
            ScriptAddress = 125, // s
            SecretAddress = 125,
            GenesisTime = 1641366830,
            GenesisNonce = 886216,
            GenesisBits = 0x1E0FFFFF,
            GenesisVersion = 1,
            GenesisReward = Money.Zero,
            HashGenesisBlock = "00000766bf653d2d1916a934ea96b4b99082551a90dccf7658e5b7d7206eef28",
            HashMerkleRoot = "a0745296c43b4fdcc4a11f00e70fd961f8c52a70f9e8a87460f251c6400cf977",
            DNS = new[] { "seed.seniorblockchain.io", "seed.seniorblockchain.net" },
            Nodes = new[] { "188.40.181.18", "46.105.172.12", "51.89.132.133", "152.228.148.188" },
            Checkpoints = new Dictionary<int, CheckpointInfo>
            {
                // TODO: Add checkpoints as the network progresses.
            }
        };

        internal NetworkSetup RegTest = new NetworkSetup
        {
            Name = "SeniorBlockCoinRegTest",
            RootFolderName = "seniorblockcoinregtest",
            CoinTicker = "TSBC",
            DefaultPort = 25006,
            DefaultRPCPort = 25007,
            DefaultAPIPort = 25008,
            PubKeyAddress = 63,
            ScriptAddress = 125,
            SecretAddress = 125,
            GenesisTime = 1641366887,
            GenesisNonce = 8148,
            GenesisBits = 0x1F00FFFF,
            GenesisVersion = 1,
            GenesisReward = Money.Zero,
            HashGenesisBlock = "0000f6d99aebfa75bf6fa83d6585506d2d51ba1a7751b2883581260005310124",
            HashMerkleRoot = "a7719b19e90f6d0856f6a9dde26501c9d70c9a3c1d958c601cd13d2fc3d57a03",
            DNS = new[] { "seedregtest.seniorblockchain.io", "seedregtest.seniorblockchain.net" },
            Nodes = new[] { "188.40.181.18", "46.105.172.12", "51.89.132.133", "152.228.148.188" },
            Checkpoints = new Dictionary<int, CheckpointInfo>
            {
                // TODO: Add checkpoints as the network progresses.
            }
        };

        internal NetworkSetup Test = new NetworkSetup
        {
            Name = "SeniorBlockCoinTest",
            RootFolderName = "seniorblockcointest",
            CoinTicker = "TSBC",
            DefaultPort = 35006,
            DefaultRPCPort = 35007,
            DefaultAPIPort = 35008,
            PubKeyAddress = 63,
            ScriptAddress = 125,
            SecretAddress = 125,
            GenesisTime = 1641366888,
            GenesisNonce = 4218,
            GenesisBits = 0x1F0FFFFF,
            GenesisVersion = 1,
            GenesisReward = Money.Zero,
            HashGenesisBlock = "0004a118ebb009cbb2530f6e4a1166909252c45f0f1f28a517201dcbed24c317",
            HashMerkleRoot = "08b098c826e241820adc2de28a30412cdb70b500260f6c248cc426a4706e3d80",
            DNS = new[] { "seedtest.seniorblockchain.io", "seedtest.seniorblockchain.net" },
            Nodes = new[] { "188.40.181.18", "46.105.172.12", "51.89.132.133", "152.228.148.188" },
            Checkpoints = new Dictionary<int, CheckpointInfo>
            {
                // TODO: Add checkpoints as the network progresses.
            }
        };

        public bool IsPoSv3()
        {
            return Setup.PoSVersion == 3;
        }

        public bool IsPoSv4()
        {
            return Setup.PoSVersion == 4;
        }

        internal class CoinSetup
        {
            internal string FileNamePrefix;
            internal string ConfigFileName;
            internal string Magic;
            internal int CoinType;
            internal decimal PremineReward;
            internal decimal PoWBlockReward;
            internal decimal PoSBlockReward;
            internal decimal MaxSupply;
            internal int LastPowBlock;
            internal string GenesisText;
            internal TimeSpan TargetSpacing;
            internal uint ProofOfStakeTimestampMask;
            internal int PoSVersion;
        }

        internal class NetworkSetup
        {
            internal string Name;
            internal string RootFolderName;
            internal string CoinTicker;
            internal int DefaultPort;
            internal int DefaultRPCPort;
            internal int DefaultAPIPort;
            internal int PubKeyAddress;
            internal int ScriptAddress;
            internal int SecretAddress;
            internal uint GenesisTime;
            internal uint GenesisNonce;
            internal uint GenesisBits;
            internal int GenesisVersion;
            internal Money GenesisReward;
            internal string HashGenesisBlock;
            internal string HashMerkleRoot;
            internal string[] DNS;
            internal string[] Nodes;
            internal Dictionary<int, CheckpointInfo> Checkpoints;
        }
    }

}
