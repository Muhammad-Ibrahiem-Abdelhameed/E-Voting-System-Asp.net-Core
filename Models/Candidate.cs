using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace EVotingSystem.Models
{
    public class Candidate
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }

        [Required]
        [Display(Name = "Vote Id")]
        public string VoteId { get; set; }

        [Required]
        [Display(Name = "List Number")]
        public int ListNumber { get; set; }

        [Required]        
        public string Name { get; set; }

        [Required]
        public string Position { get; set; }

        [DataType(DataType.ImageUrl)]
        public string Picture { get; set; }
        
        [Display(Name="Current Voted")]
        public long CurrentVoted { get; set; }

        [ForeignKey("VoteId")]
        public virtual Vote Vote { get; set; }        
        

    }
}
