using System.Collections.Generic;

namespace CSG.Models
{
    public static class RoleNames
    {
        public static string Admin = "Admin";
        public static string Operator = "Operator";
        public static string Technician = "Technician";
        public static string Customer = "Customer";
        public static string Passive = "Passive";
        
        public static List<string> Roles => new List<string>() { Admin, Operator, Technician,Customer, Passive };
    }
}