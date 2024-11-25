using System.ComponentModel.DataAnnotations;

namespace RectalCancerDiagnosisWeb.Models
{
    public class User
    {
        [Key]
        public int UserID { get; set; }
        [StringLength(50)]
        public string? Name { get; set; }
        [StringLength(50)]
        public string? Surname { get; set; }
        [StringLength(50)]
        public string? Username { get; set; }
        [StringLength(50)]
        public string? Password { get; set; }
        [StringLength(50)]
        public string? Email { get; set; }
        
    }
}
