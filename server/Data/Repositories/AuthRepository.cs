namespace Diplomacy.Data.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    using Diplomacy.Data.Entities;
    using Dipomacy.Data;

    public class AuthRepository : IAuthRepository
    {
        private readonly DiplomacyContext _context;

        public AuthRepository(DiplomacyContext context)
        {
            this._context = context;
        }

    }
} 