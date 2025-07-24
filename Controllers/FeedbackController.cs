using System.Security.Claims;
using FeedbackApp.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FeedbackApp.API.Controllers
{
    [ApiController]
    [Route("api/feedback")]
    public class FeedbackController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FeedbackController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ POST: /api/feedback
        [HttpPost]
        [Authorize(Roles = "user")]
        public async Task<IActionResult> SubmitFeedback([FromBody] FeedbackDto dto)
        {
            var username = User.Identity?.Name;
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var feedback = new Feedback
            {
                Username = username!,
                Category = dto.Category,
                Comment = dto.Comment,
                UserId = userId
            };

            _context.Feedbacks.Add(feedback);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Feedback submitted successfully." });
        }

        // ✅ GET: /api/feedback/user
        [HttpGet("user")]
        [Authorize(Roles = "user")]
        public async Task<IActionResult> GetUserFeedbacks()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var feedbacks = await _context.Feedbacks
                .Where(f => f.UserId == userId)
                .ToListAsync();

            return Ok(feedbacks);
        }

        // ✅ GET: /api/feedback/all
        [HttpGet("all")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetAllFeedbacks()
        {
            var feedbacks = await _context.Feedbacks
                .Select(f => new
                {
                    f.Username,
                    f.Category,
                    f.Comment
                })
                .ToListAsync();

            return Ok(feedbacks);
        }
    }
}
