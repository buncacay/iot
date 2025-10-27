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
            public float Temperature { get; set; }
            public float Humidity { get; set; }
            public float Light { get; set; } // ánh sáng, 0-1023 hoặc theo % nếu ESP gửi
        }
    


}
