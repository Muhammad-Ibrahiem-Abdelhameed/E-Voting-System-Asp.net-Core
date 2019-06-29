using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;


namespace EVotingSystem.Models
{    
    public class Vote
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }        

        [Required]
        public string Title { get; set; }

        [Required]
        [Display(Name = "Vote Type")]        
        public string VoteType { get; set; }        

        [Required]
        [DataType(DataType.Date)]
        [Display(Name= "Start Date")]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }

        [Required]
        [Display(Name = "status")]      
        public string Status { get; set; }        

        [Display(Name = "State")]
        public int? StateId { get; set; }

        [Required]
        [Display(Name = "Devices")]
        public string Devices { get; set; }

        [DataType(DataType.MultilineText)]
        public string Description { get; set; }                        

        [ForeignKey("StateId")]
        public virtual State State { get; set; }
      
    }

    
}
