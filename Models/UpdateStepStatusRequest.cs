using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BTL.Web.Models
{
    public class UpdateStepStatusRequest
    {
        [Required]
        [JsonPropertyName("orderId")]
        public int OrderId { get; set; }

        [Required]
        [JsonPropertyName("stepId")]
        public int StepId { get; set; }

        [Required]
        [JsonPropertyName("employeeId")]
        public int EmployeeId { get; set; }

        [Required]
        [JsonPropertyName("action")]
        public string Action { get; set; } = string.Empty; // "start" or "complete"
    }
}
