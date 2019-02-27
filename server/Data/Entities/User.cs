namespace Diplomacy.Data.Entities
{
    using System;
    using Microsoft.AspNetCore.Identity;

    public class User : IdentityUser
    {
        public string LastName { get; set; }

        public string FirstName { get; set; }
    }
}
