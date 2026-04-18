using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Application.DTOs.Auth
{
    public class EmailConfirmWithOTP
    {
        [Required(ErrorMessage = "ClientOTP is required")]
        public string ClientOTP { get; set; } = string.Empty;
    }
}
