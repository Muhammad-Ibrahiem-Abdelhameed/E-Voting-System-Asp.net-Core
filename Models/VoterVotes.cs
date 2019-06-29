using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVotingSystem.Models
{
    public class VoterVotes
    {
        [Key]
        public string Id { get; set; }

        [Required]
        [Display(Name = "Voter Id")]
        public string VoterId { get; set; }

        [Required]
        [Display(Name = "Vote Id")]
        public string VoteId { get; set; }
                
        [Display(Name = "Is Voted")]
        public bool IsVoted { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime SignedInTime { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime LoggedOutTime { get; set; }

        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [ForeignKey("VoterId")]
        public virtual Voter Voter { get; set; }

        [ForeignKey("VoteId")]
        public virtual Vote Vote { get; set; }

        
    }
}
