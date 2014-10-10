using System;
using System.Collections.Generic;

namespace Domain
{
    public class DataFactory
    {
        private static Dealer[] _dealers;
        private static string[] _colors;
        private static string[] _types;
        private static string[] _options;
        private static string[] _packages;
        private static Dictionary<string, string> _images;
        private static Random _colorMeRandom;
        private static Random _typeMeRandom;
        private static Random _dealerMeRandom;
        private static Random _optionsMeRandom;
        private static Random _packageMeRandom;

        public DataFactory()
        {
            InitData();
        }

        public Listing[] CreateListings(int numberOfListingsToCreate)
        {
            List<Listing> listings = new List<Listing>();

            for (int i = 0; i < numberOfListingsToCreate; i++)
            {
                var listing = GetListing();
                listings.Add(listing);
            }

            return listings.ToArray();
        }

        private Listing GetListing()
        {
            Listing l = new Listing();
            l.Id = Guid.NewGuid();
            l.Color = GetColor();
            l.Options = GetOptions();
            l.Package = GetPackage();
            l.Type = GetType();
            l.Image = GetImage(l.Color, l.Type, l.Package);
            l.Dealer = GetDealer();

            return l;
        }

        private void InitData()
        {
            List<Dealer> dealers = new List<Dealer>();
            dealers.Add(new Dealer() { url = "http://www.amazon.com", name = "Amazon Motors" });
            dealers.Add(new Dealer() { url = "http://www.craigslist.com", name = "Craigslist" });
            dealers.Add(new Dealer() { url = "http://www.monstermotors.com", name = "Monster Motors" });
            dealers.Add(new Dealer() { url = "http://www.carsdirect.com", name = "Cars Direct" });
            dealers.Add(new Dealer() { url = "http://www.autotrader.com", name = "Auto Trader" });
            dealers.Add(new Dealer() { url = "http://www.cars.com", name = "Cars.com" });
            _dealers = dealers.ToArray();

            List<String> options = new List<string>();
            options.Add("soft top, half metal doors, 5 speed transmission, rock-trac part-time 4wd, normal duty suspension");
            options.Add("hard top, full metal doors, sway bar disconnect, fuel tank skid plate, transfer plate skid plate, hill start assist, electronic stability control, electronic roll mitigation, 6 speed transmission, command-trac shift on the fly 4wd, heavy duty suspension");
            options.Add("soft top, half metal doors, sway bar disconnect, fuel tank skid plate, transfer plate skid plate, hill start assist, electronic stability control, electronic roll mitigation, 6 speed transmission, command-trac shift on the fly 4wd, heavy duty suspension");
            options.Add("hard top, full metal doors, 5 speed transmission, rock-trac part-time 4wd, normal duty suspension");
            _options = options.ToArray();

            _colors = "ampd,anvil,black,billet,copperhead,dune,firecracker red,flame red,granite,silver,white".Split(',');
            _types = "wrangler,wrangler unlimited".Split(',');
            _packages = "sport,rubicon,sahara,sport s".Split(',');
            _colorMeRandom = new Random();
            _dealerMeRandom = new Random();
            _typeMeRandom = new Random();
            _optionsMeRandom = new Random();
            _packageMeRandom = new Random();

        }

        private string GetColor()
        {

            return _colors[_colorMeRandom.Next(0, _colors.Length)];
        }

        private Option[] GetOptions()
        {
            List<Option> result = new List<Option>();
            string[] options = _options[_optionsMeRandom.Next(0, _options.Length)].Split(',');
            foreach (string option in options)
            {
                result.Add(new Option() { Name = option});
            }
            return result.ToArray();
        }

        private string GetPackage()
        {
            return _packages[_packageMeRandom.Next(0, _packages.Length)];
        }

        private string GetType()
        {
            return _types[_typeMeRandom.Next(0, _types.Length)];
        }

        private string GetImage(string color, string type, string package)
        {
            return type.Replace(" ", "-") + "-" + package.Replace(" ", "-") + "-" + color.Replace(" ", "-") + ".png";
        }

        private Dealer GetDealer()
        {
            return _dealers[_dealerMeRandom.Next(0, _dealers.Length)];
        }
    }
}
