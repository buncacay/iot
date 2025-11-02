using System.ComponentModel.DataAnnotations;

namespace btvn5.Models
{
   

    public class LedRequest
    {
        
        public string? State { get; set; }
    }

   
    public class SensorData
    {
        public float Temperature { get; set; }
        public float Humidity { get; set; }
        public int Light { get; set; } 
    }
    


}
