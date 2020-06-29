namespace ClickView.CloudWatch.Metrics.RabbitMq.Options
{
    using System.ComponentModel.DataAnnotations;

    public class ManagementClientOptions
    {
        [Url]
        public string Host { get; set; }
        
        [Required]
        public string Username { get; set; }
        
        [Required]
        public string Password { get; set; }
    }
}