using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Response
{

    public class ResCustomerPrivateDto
    {
        public int id { get; set; }
        public string name { get; set; } = null!;
        public string? avatarUrl { get; set; }
        public string? email { get; set; }
        public string? address { get; set; }
        public string? phoneNumber { get; set; }
        public string? maskedPhoneNumber { get; set; }
        public bool isGoogleLinked { get; set; }
        public int totalOrders { get; set; }
    }

    public class ResCustomerVehicleDto
    {
        public int VehicleId { get; set; }
        public string ProductName { get; set; } = null!;
        public string ColorName { get; set; } = null!;
        public string Vin { get; set; } = null!;
        public string EngineNumber { get; set; } = null!;
        public string LicensePlate { get; set; } = "Đang cập nhật";
        public DateTime? PurchaseDate { get; set; }
        public DateTime? NextMaintenanceDate { get; set; }
        public string? ImageUrl { get; set; }
    }
}
