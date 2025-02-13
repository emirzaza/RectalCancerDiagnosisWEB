using System.ComponentModel.DataAnnotations;

namespace RectalCancerDiagnosisWeb.Models
{
    public class Results
    {
        [Key]
        public int ResultID { get; set; }

        public int userId { get; set; }

        public string? ResultName { get; set; }
        [StringLength(50)]
        public string? Description { get; set; }
        
        public string? Result { get; set; }
        

    }
}