﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Models
{
    public class BookForUpdateDto : BookForManipulationDto
    {
       
        [Required(ErrorMessage = "You should fill out a description.")]
        public override string Description {
            get
            {
                return base.Description;
            }

            set
            {
                base.Description = value;
            }

        }
    }
}
