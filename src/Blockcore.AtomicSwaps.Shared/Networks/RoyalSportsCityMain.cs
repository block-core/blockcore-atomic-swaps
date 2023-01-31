using Blockcore.Consensus;
using Blockcore.Consensus.BlockInfo;
using Blockcore.Consensus.Checkpoints;
using Blockcore.Consensus.ScriptInfo;
using Blockcore.Consensus.TransactionInfo;
using Blockcore.Networks.City.Networks;
using Blockcore.P2P;
using NBitcoin;
using NBitcoin.BitcoinCore;
using NBitcoin.BouncyCastle.Math;
using NBitcoin.DataEncoders;
using System.Net;

namespace Blockcore.Networks.RoyalSportsCity.Networks
{
    public class RoyalSportsCityMain : Network
   {
      public RoyalSportsCityMain()
      {
          RoyalSportsCitySetup.CoinSetup setup = RoyalSportsCitySetup.Instance.Setup;
          RoyalSportsCitySetup.NetworkSetup network = RoyalSportsCitySetup.Instance.Main;

         NetworkType = NetworkType.Mainnet;
         DefaultConfigFilename = setup.ConfigFileName; // The default name used for the RoyalSportsCity configuration file.

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
         MinTxFee = 1000000;
         MaxTxFee = Money.Coins(1).Satoshi;
         FallbackFee = 250000;
         MinRelayTxFee = 1000000;
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
             subsidyHalvingInterval: 70312500,
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

         Consensus.PosEmptyCoinbase = RoyalSportsCitySetup.Instance.IsPoSv3();
         Consensus.PosUseTimeFieldInKernalHash = RoyalSportsCitySetup.Instance.IsPoSv3();

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

         DNSSeeds = network.DNS.Select(dns => new DNSSeedData(dns, dns)).ToList();
         SeedNodes = network.Nodes.Select(node => new NBitcoin.Protocol.NetworkAddress(IPAddress.Parse(node), network.DefaultPort)).ToList();

         StandardScriptsRegistry = new RoyalSportsCityStandardScriptsRegistry();

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

   public class RoyalSportsCityStandardScriptsRegistry : StandardScriptsRegistry
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

    internal class RoyalSportsCitySetup
    {
        internal static RoyalSportsCitySetup Instance = new RoyalSportsCitySetup();

        internal CoinSetup Setup = new CoinSetup
        {
            FileNamePrefix = "royalsportscity",
            ConfigFileName = "royalsportscity.conf",
            Magic = "01-52-53-43",
            CoinType = 6599,
            PremineReward = 3000000000,
            PoWBlockReward = 210,
            PoSBlockReward = 21,
            LastPowBlock = 2100,
            MaxSupply = 21000000000,
            GenesisText = "Decentralized Royal  Sports City",
            TargetSpacing = TimeSpan.FromSeconds(64),
            ProofOfStakeTimestampMask = 0x0000000F, // 0x0000003F // 64 sec
            PoSVersion = 4
        };

        internal NetworkSetup Main = new NetworkSetup
        {
            Name = "RoyalSportsCityMain",
            RootFolderName = "royalsportscity",
            CoinTicker = "RSC",
            DefaultPort = 14001,
            DefaultRPCPort = 14002,
            DefaultAPIPort = 14003,
            PubKeyAddress = 60, // R 
            ScriptAddress = 122, // r
            SecretAddress = 122,
            GenesisTime = 1641376672,
            GenesisNonce = 1019548,
            GenesisBits = 0x1E0FFFFF,
            GenesisVersion = 1,
            GenesisReward = Money.Zero,
            HashGenesisBlock = "000009b276ed4815cf1fde9078f2d21facd5ece23e6ccfb31c7f90f87563010f",
            HashMerkleRoot = "d8575410f6b019171ff0b4548814e7ae6fc3dd837d5ecc71d77bb21866ddb210",
            DNS = new[] { "seed.royalsportscity.com", "seed.royalsportscity.net" },
            Nodes = new[] { "188.40.181.18", "46.105.172.12", "51.89.132.133", "152.228.148.188" },
        };

        internal NetworkSetup RegTest = new NetworkSetup
        {
            Name = "RoyalSportsCityRegTest",
            RootFolderName = "royalsportscityregtest",
            CoinTicker = "TRSC",
            DefaultPort = 24001,
            DefaultRPCPort = 24002,
            DefaultAPIPort = 24003,
            PubKeyAddress = 60, // R 
            ScriptAddress = 122, // r
            SecretAddress = 122,
            GenesisTime = 1641376738,
            GenesisNonce = 128636,
            GenesisBits = 0x1F00FFFF,
            GenesisVersion = 1,
            GenesisReward = Money.Zero,
            HashGenesisBlock = "00007a766fd57aef404cc1ae16d74f616074c924d41aeed23e727f9a00b83c01",
            HashMerkleRoot = "f547b27d5390b11f578ff3b0855b9b21547bbca3b54f39820c2de371001ea71f",
            DNS = new[] { "seedregtest.royalsportscity.com", "seedregtest.royalsportscity.net" },
            Nodes = new[] { "188.40.181.18", "46.105.172.12", "51.89.132.133", "152.228.148.188" },
        };

        internal NetworkSetup Test = new NetworkSetup
        {
            Name = "RoyalSportsCityTest",
            RootFolderName = "royalsportscitytest",
            CoinTicker = "TRSC",
            DefaultPort = 34001,
            DefaultRPCPort = 34002,
            DefaultAPIPort = 34003,
            PubKeyAddress = 60, // R 
            ScriptAddress = 122, // r
            SecretAddress = 122,
            GenesisTime = 1641376746,
            GenesisNonce = 7719,
            GenesisBits = 0x1F0FFFFF,
            GenesisVersion = 1,
            GenesisReward = Money.Zero,
            HashGenesisBlock = "0004e385d4eb63c3a8f2a190117330a3dbef67782b33a5a29e1b554ed357d5df",
            HashMerkleRoot = "92711a71fa912dd3419c5e52bee7edc64ce1d60260ba76664f49c39367734d83",
            DNS = new[] { "seedtest.royalsportscity.com", "seedtest.royalsportscity.net" },
            Nodes = new[] { "188.40.181.18", "46.105.172.12", "51.89.132.133", "152.228.148.188" },
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
