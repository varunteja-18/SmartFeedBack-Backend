using FeedbackApp.API.Models;

public class Feedback
{
    public int Id { get; set; }
    public string Category { get; set; } = "";
    public string Comment { get; set; } = "";
    public string Username { get; set; } = "";
    public int UserId { get; set; }

    public User User { get; set; }
}
