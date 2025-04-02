using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MailSenderAPI.Models
{
    public class EmailExtension
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Extension { get; set; }

        [Required]
        public bool Status { get; set; } 
    }
}
