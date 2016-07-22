﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransferDesk.Contracts.ReviewerIndex.ComplexTypes
{
   public class ReviewerEmails_Result
    {
        [Key]
        public int ID { get; set; }
        public int ReviewerMasterID { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }
        public DateTime? ModifiedDate { get; set; }

    }
}
