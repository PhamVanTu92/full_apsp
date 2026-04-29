namespace BackEndAPI.Service.Events
{
    public record TelemetryEvent
    {
        public string DeviceId { get; set; } = "";
        public DateTime Timestamp { get; set; }
        public double Temperature { get; set; }
    }
}
