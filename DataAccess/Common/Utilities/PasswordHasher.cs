﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Common.Utilities
{
    public  class PasswordHasher
    {
        public static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                var builder = new StringBuilder();
                foreach (var b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public static bool VerifyHashedPassword(string hashedPassword, string password)
        {
           
            var hashedInputPassword = HashPassword(password);

            
            return hashedInputPassword.Equals(hashedPassword, StringComparison.OrdinalIgnoreCase);
        }
    }
}
