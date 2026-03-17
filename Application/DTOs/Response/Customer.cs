using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Response
{

    public class ResCustomerPublicDto
    {
        public int id { get; set; }
        public string name { get; set; } = null!;
        public string? avatarUrl { get; set; }
    }
    public class ResCustomerPrivate : ResCustomerPublicDto
    {
        public string? email { get; set; }
    }
}
