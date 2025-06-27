using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreApp.Infrastructure.Auth
{
    public class JwtSettings
    {
        public string SecretKey { get; set; } = string.Empty;

        public string Issuer { get; set; } = "CoreApp";

        public string Audience { get; set; } = "CoreAppAPI";

        public int ExpirationMinutes { get; set; } = 60;
    }
}
