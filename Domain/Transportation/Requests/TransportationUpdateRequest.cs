namespace Domain.Transportation.Requests
{
    public class TransportationUpdateRequest
    {
        public int TransportId { get; set; }
        public int? PermitId { get; set; }
        public int? SourcePremisesId { get; set; }

        public int? UserId { get; set; }
        public string? SourceAddress { get; set; }
        public int? DestinationPremisesId { get; set; }
        public string? DestinationAddress { get; set; }
        public int? TransporterId { get; set; }
        public string? VehicleDetails { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? ItemsDocument { get; set; }
        public string? ReasonForTransport { get; set; }
        public string? Description { get; set; }
        public string? Status { get; set; }
        public int VetId { get; set; }
    }
}