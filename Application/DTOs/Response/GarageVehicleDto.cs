namespace Application.DTOs.Response
{
    public class GarageVehicleDto
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Color { get; set; } = null!;
        public string Vin { get; set; } = null!;
        public string EngineNumber { get; set; } = null!;
        public DateTime? PurchaseDate { get; set; }
        public DateTime? WarrantyUntil { get; set; }
        public string Status { get; set; } = null!;
        public string Image { get; set; } = null!;
        public List<string> Benefits { get; set; } = new List<string>();
    }
}
