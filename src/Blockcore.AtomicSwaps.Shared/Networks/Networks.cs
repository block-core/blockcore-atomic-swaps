using Blockcore.Networks;

namespace Blockcore.AtomicSwaps.Shared.Networks
{
    public static class Networks
    {
        public static NetworksSelector Bitcoin
        {
            get
            {
                return new NetworksSelector(() => new BitcoinMain(), () => null, () => null);
            }
        }

        public static NetworksSelector Strax
        {
            get
            {
                return new NetworksSelector(() => new StraxMain(), () => null, () => null);
            }
        }

        public static NetworksSelector City
        {
            get
            {
                return new NetworksSelector(() => new CityMain(), () => null, () => null);
            }
        }

        public static NetworksSelector Implx
        {
            get
            {
                return new NetworksSelector(() => new  ImpleumMain(), () => null, () => null);
            }
        }

        public static NetworksSelector RSC
        {
            get
            {
                return new NetworksSelector(() => new RoyalSportsCityMain(), () => null, () => null);
            }
        }
        public static NetworksSelector SBC
        {
            get
            {
                return new NetworksSelector(() => new SeniorBlockCoinMain(), () => null, () => null);
            }
        }
    }
}