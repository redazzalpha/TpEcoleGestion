namespace TpGestionEcole.Models
{
    public class Module
    {

        public int Id { get; set; } 
        public string Nom { get; set; } 
        public string Resume{ get; set; }
        public string Infos{ get; set; }
        public string? logo { get; set; }

        public int? ParcoursId { get; set; } 


    }
}
