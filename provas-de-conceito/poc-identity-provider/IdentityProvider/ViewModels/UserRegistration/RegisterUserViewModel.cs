﻿using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IdentityProvider.ViewModels.UserRegistration
{
    public class RegisterUserViewModel
    {
        // credentials       
        [MaxLength(100)]
        public string Username { get; set; }

        [MaxLength(100)]
        public string Password { get; set; }

        // claims 
        [Required]
        [MaxLength(100)]
        public string Firstname { get; set; }

        [Required]
        [MaxLength(100)]
        public string Lastname { get; set; }

        [Required]
        [MaxLength(150)]
        public string Email { get; set; }

        [Required]
        [MaxLength(200)]
        public string Address { get; set; }

        [Required]
        [MaxLength(50)]
        public string Country { get; set; }

        public string Provider { get; set; }

        public string ProviderUserId { get; set; }

        public bool IsProvisioningFromExternal => Provider != null;

        public SelectList CountryCodes { get; set; } =
            new SelectList(
                new[] {
                    new { Id = "Brasil", Value = "Brasil" },
                    new { Id = "Japao", Value = "Japao" }
                }, 
                "Id",
                "Value");

        public string ReturnUrl { get; set; }
    }
}
