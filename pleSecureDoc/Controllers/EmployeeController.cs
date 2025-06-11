using Microsoft.AspNetCore.Mvc;
using pleSecureDoc.Models;
using pleSecureDoc.Services;
using System;
using System.Threading.Tasks;

namespace pleSecureDoc.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly FirebaseService _firebase;
        private readonly EmailService _email;

        public EmployeeController(FirebaseService firebase, EmailService email)
        {
            _firebase = firebase;
            _email = email;
        }

        public IActionResult RequestDocument() => View();

        [HttpPost]
        public async Task<IActionResult> RequestDocument(string documentName, string employeeId)
        {
            var request = new DocumentRequest
            {
                RequestId = Guid.NewGuid().ToString(),
                EmployeeId = employeeId,
                DocumentName = documentName,
                RequestTime = DateTime.UtcNow,
                IsApproved = false,
                IsFaceConfirmed = false
            };

            await _firebase.LogRequest(request);
            await _email.SendApprovalEmail("kgomotsosele80@gmail.com", request.RequestId);

            return RedirectToAction("RequestStatus", new { id = request.RequestId });
        }

        public async Task<IActionResult> RequestStatus(string id)
        {
            var request = await _firebase.GetRequest(id);
            return View(request);
        }
    }
}
