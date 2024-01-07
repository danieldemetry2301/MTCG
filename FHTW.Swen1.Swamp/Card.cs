using System.ComponentModel.DataAnnotations.Schema;

namespace FHTW.Swen1.Swamp
{
    public class Card
    {
        [Column("Id")]
        public Guid? Id { get; set; }
        [Column("Name")]
        public string Name { get; set; }
        [Column("Damage")]
        public double? Damage { get; set; }
        [Column("PackageId")]
        public string PackageId { get; set; }
        [Column("UserId")]
        public long? UserId { get; set; }

        public Card()
        {
        }
    }
    
}
