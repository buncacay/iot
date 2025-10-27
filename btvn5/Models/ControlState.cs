using System.ComponentModel.DataAnnotations;

namespace btvn5.Models
{
   

    public class LedRequest
    {
        public string? User { get; set; }
        public string? State { get; set; }
    }

    public class SensorData
    {
        public double Temperature { get; set; }
        public double Humidity { get; set; }
    }

}
