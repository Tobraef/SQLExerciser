using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace SQLExerciser.Models.ViewModel
{
    public class Login
    {
        [Required]
        [Display(Description = "Account name")]
        public string AccountName { get; set; }

        [Required]
        [Display(Description = "Password")]
        public string Password { get; set; }

        public string ReturnUrl { get; set; }
    }
}