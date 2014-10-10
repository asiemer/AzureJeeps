using System;

namespace Domain
{
    public class Listing
    {
        public string Color { get; set; }
        public Option[] Options { get; set; }
        public string Package { get; set; }
        public string Type { get; set; }
        public string Image { get; set; }
        public Dealer Dealer { get; set; }
        public Guid Id { get; set; }
    }
}