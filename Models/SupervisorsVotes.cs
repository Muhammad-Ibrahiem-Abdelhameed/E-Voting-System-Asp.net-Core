using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace EVotingSystem.Models
{
    public class SupervisorsVotes
    {
        [Key]
        [Display(Name = "Supervisor Id")]
        public string SupervisorId { get; set; }
        
        [Display(Name = "Vote Id")]
        public string VoteId { get; set; }

        [ForeignKey("SupervisorId")]
        public virtual ApplicationUser ApplicationUser { get; set; }

        [ForeignKey("VoteId")]
        public virtual Vote Vote { get; set; }
    }
}
