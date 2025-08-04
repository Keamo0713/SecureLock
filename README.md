# SecureLock

pleSecureDoc is a secure document approval system for organizations, built with ASP.NET MVC. It allows employees to request documents and employers to approve them via email within a time-bound window. Final confirmation uses face recognition for identity verification.

## Features

- Employees request confidential documents.
- Employers receive approval emails with a 5-minute response window.
- Final approval requires face verification using Azure Face API.
- Approval logs stored in Firebase Realtime Database.

## Tech Stack

- ASP.NET Core MVC (.NET 6+)
- Razor Views
- Azure Face API (Face Recognition)
- Firebase Realtime Database
- Gmail SMTP (for approval emails)
- HTML5, JavaScript (Webcam capture)

## Prerequisites

- [.NET 6 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
- Visual Studio 2022+ or VS Code
- Firebase Realtime Database
- Azure Face API Key & Endpoint
- Gmail account with "less secure apps" access enabled (or App Password)


<img width="921" height="680" alt="image" src="https://github.com/user-attachments/assets/b0cc2fd7-84e8-48aa-92f4-4929fd79fffc" />

