using Microsoft.AspNetCore.Mvc;
using pleSecureDoc.Models;
using pleSecureDoc.Services;
using System;
using System.Threading.Tasks;

namespace pleSecureDoc.Controllers
{
    public class EmployerController : Controller
    {
        private readonly FirebaseService _firebase;

        public EmployerController(FirebaseService firebase)
        {
            _firebase = firebase;
        }

        public async Task<IActionResult> ApproveRequest(string requestId)
        {
            if (string.IsNullOrWhiteSpace(requestId))
                return BadRequest("Invalid request ID.");

            var request = await _firebase.GetRequest(requestId);
            if (request == null)
                return NotFound("Document request not found.");

            if ((DateTime.UtcNow - request.RequestTime).TotalMinutes > 5)
                return View("ApprovalExpired");

            await _firebase.ApproveRequest(requestId);

            // Redirect to face recognition step
            return RedirectToAction("StartFaceRecognition", "Face", new { requestId });
        }
    }
}
