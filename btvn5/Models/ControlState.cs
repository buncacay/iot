using System.ComponentModel.DataAnnotations;

namespace btvn5.Models
{
    public class ControlState
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string State { get; set; } = "off";  // "on" hoặc "off"

        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }

    public class ControlRequest
    {
        public string State { get; set; }
    }
}
