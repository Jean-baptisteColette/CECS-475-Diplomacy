using System;
using Diplomacy.Data.Entities;

namespace Diplomacy.Models
{
    public class UserToken
    {
        public User User { get; set; }
        public Token Token { get; set; }
    }
}
