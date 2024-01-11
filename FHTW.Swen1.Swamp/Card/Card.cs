using System.ComponentModel.DataAnnotations.Schema;

namespace FHTW.Swen1.Swamp
{
    public class Card
    {
 
        public string Id { get; set; }

        public string Name { get; set; }
   
        public double Damage { get; set; }

        public string PackageId { get; set; }

        public long UserId { get; set; }

        public string Type { get; set; }

        public Card()
        {
        }
    }
    
}
