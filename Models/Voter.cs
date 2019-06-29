using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace EVotingSystem.Models
{
    public class Voter
    {
        [Key]
        [ForeignKey("ApplicationUser")]
        public string Id { get; set; }

        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Required]
        [MinLength(14)]
        [MaxLength(14)]
        [Display(Name = "National Id")]
        public string NationalId { get; set; }

        [Required]
        [Display(Name = "State")]
        public int StateId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Birth Date")]
        public DateTime BirthDate { get; set; }

        [Required]
        [Display(Name = "Allow Phone")]
        [DisplayFormat(NullDisplayText = "Allow Phone")]
        public bool AllowPhone { get; set; }

        [Display(Name = "Fingerprint")]
        [DataType(DataType.MultilineText)]
        public string Fingerprint { get; set; }

        [ForeignKey("StateId")]
        public virtual State State { get; set; }

        
        public virtual ApplicationUser ApplicationUser { get; set; }

        public virtual ICollection<VoterVotes> VoterVotes { get; set; }
    }
}
