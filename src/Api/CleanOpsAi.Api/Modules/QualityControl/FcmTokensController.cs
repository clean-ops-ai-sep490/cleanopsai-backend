using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CleanOpsAi.Api.Modules.QualityControl
{
	[Route("api/[controller]")]
	[ApiController]
	public class FcmTokensController : ControllerBase
	{
		[HttpGet("firebase-status")]
		public async Task<IActionResult> GetFirebaseStatus()
		{
			try
			{
				var app = FirebaseAdmin.FirebaseApp.DefaultInstance;

				// test call tới Firebase (dry run)
				var message = new FirebaseAdmin.Messaging.Message
				{
					Token = "fake_token_for_test",
					Notification = new FirebaseAdmin.Messaging.Notification
					{
						Title = "Test",
						Body = "Test"
					}
				};

				await FirebaseAdmin.Messaging.FirebaseMessaging
					.DefaultInstance
					.SendAsync(message, dryRun: true);

				return Ok(new
				{
					Status = "Firebase connected OK",
					ProjectId = app.Options.ProjectId
				});
			}
			catch (Exception ex)
			{
				return BadRequest(new
				{
					Status = "Firebase connection failed",
					Error = ex.Message
				});
			}
		}
	}
}
