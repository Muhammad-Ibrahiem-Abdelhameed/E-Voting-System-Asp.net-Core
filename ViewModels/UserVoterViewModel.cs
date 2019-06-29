using EVotingSystem.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EVotingSystem.ViewModels
{
    public class UserVoterViewModel : AppUserViewModel
    {        

        [Required]
        public Voter Voter { get; set; }
        /*[Required]
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
        public bool AllowPhone { get; set; }*/
    }
}
