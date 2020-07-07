using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SQLExerciser.Models.DB
{
    public class User
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public DateTime Created { get; set; }

        [Required]
        public virtual Role Role { get; set; }

        [Required]
        public string Account { get; set; }

        [Required]
        public string Password { get; set; }

        public bool MailConfirmed { get; set; }
    }
}