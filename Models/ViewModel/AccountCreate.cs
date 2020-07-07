using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

using System.ComponentModel;

namespace SQLExerciser.Models.ViewModel
{
    public class AccountCreate
    {
        [Required]
        [Display(Description = "Account name")]
        public string AccountName { get; set; }

        [Required]
        [Display(Description = "Your password")]
        public string Password { get; set; }

        [Required]
        [RegularExpression(".*?@\\w+\\.\\w+")]
        [Display(Description = "Email to send a confirmation mail. The email WILL NOT be used for any other purpose.")]
        public string Email { get; set; }

        public static string OkMessage => "Account created successfully! A confirmation email has been sent to the given email.";

        public string CreationMessage { get; set; }
    }
}