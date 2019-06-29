using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace EVotingSystem.Models
{
    public class State
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(20)]
        [Display(Name = "State")]
        public string Name { get; set; }

        public ICollection<Voter> Voters { get; set; }
    }
}
