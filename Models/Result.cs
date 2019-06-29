using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVotingSystem.Models
{
    public class Result
    {
        [Key]
        [ForeignKey("Vote")]
        public string Id { get; set; }
        
        [Required]
        [Display(Name = "Total Voters")]
        public long TotalVoters { get; set; }        

        [Required]
        [Display(Name = "Current Voted")]
        public long CurrentVoted { get; set; }

        [Required]        
        [Display(Name = "Percentage")]
        public decimal Percentage
        {
            get
            {
                if(TotalVoters == 0)
                {
                    return 0;
                }
                else
                {
                    return (CurrentVoted / TotalVoters) * 100;
                }
                
            }
        }
        
        public string Winner { get; set; }

        [DataType(DataType.MultilineText)]
        public string Description { get; set; }
        
        public virtual Vote Vote { get; set; }

        public virtual ICollection<VoterVotes> VoterVotes { get; set; }

        [ForeignKey("Winner")]
        public virtual Candidate Candidate { get; set; }
    }
}
