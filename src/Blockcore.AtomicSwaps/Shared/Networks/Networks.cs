using Blockcore.Networks.Bitcoin;
using Blockcore.Networks.RoyalSportsCity.Networks;
using Blockcore.Networks.SeniorBlockCoin.Networks;
using Blockcore.Networks.Strax;

namespace Blockcore.Networks
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
                return new NetworksSelector(() => new City.Networks.CityMain(), () => null, () => null);
            }
        }

        public static NetworksSelector Implx
        {
            get
            {
                return new NetworksSelector(() => new  Implx.ImpleumMain(), () => null, () => null);
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