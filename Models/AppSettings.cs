namespace RestimController.Models
{
    public class AppSettings
    {
        public required string AuthKey { get; set; }
        public required RestimInstance[] RestimInstances { get; set; }
    }
}