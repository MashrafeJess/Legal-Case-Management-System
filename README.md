# JurisFlow - Legal Case Management System (Backend)

A comprehensive backend API for managing legal cases, hearings, payments, and client-lawyer interactions built with ASP.NET Core 10 and PostgreSQL.

## 🚀 Features

### Core Functionality
- **Case Management**: Create, update, track cases with full lifecycle management (Ongoing → Won/Lost/Closed)
- **Hearing Scheduling**: Schedule hearings with payment tracking and automated reminders
- **Payment Processing**: 
  - SSLCommerz integration for online payments
  - Cash payment recording
  - Automatic payment receipts via email
  - Consultation and hearing fee tracking
- **Comment System**: Case updates with status changes by lawyers
- **NOC (No Objection Certificate)**: Client applications with lawyer/admin approval
- **Email Notifications**:
  - OTP-based email verification
  - Payment receipts
  - Hearing reminders
  - NOC certificates
- **File Management**: Document uploads with secure storage and retrieval
- **User Management**: Role-based access (Admin, Lawyer, Client)
- **Reporting**: Monthly financial reports with profit/loss analysis

### Advanced Features
- **Background Jobs**: Automated hearing reminder emails
- **SMTP Configuration**: Admin-configurable email settings
- **Salary Management**: Lawyer salary tracking for profit calculation
- **Multi-tenant Architecture**: Role-based data filtering

## 📋 Prerequisites

- .NET 8.0 SDK or later
- PostgreSQL 14+
- Visual Studio 2022 or VS Code
- SSLCommerz Sandbox/Live Account (for payments)

## 🛠️ Installation

### 1. Clone the Repository
```bash
git clone https://github.com/yourusername/jurisflow-backend.git
cd jurisflow-backend
```

### 2. Database Setup

Update `appsettings.json` with your PostgreSQL connection:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=jurisflow_db;Username=postgres;Password=yourpassword"
  }
}
```

### 3. Apply Migrations
```bash
dotnet ef database update
```

### 4. Configure SSLCommerz

Update `appsettings.json`:
```json
{
  "SSLCommerz": {
    "StoreId": "your_store_id",
    "StorePassword": "your_store_password",
    "IsSandbox": true,
    "SuccessUrl": "http://localhost:4200/payment/success",
    "FailUrl": "http://localhost:4200/payment/failed",
    "CancelUrl": "http://localhost:4200/payment/cancelled",
    "IpnUrl": "https://your-ngrok-url/api/payment/ipn"
  }
}
```

### 5. Configure JWT
```json
{
  "Jwt": {
    "Key": "your-super-secret-key-minimum-32-characters-long",
    "Issuer": "JurisFlow",
    "Audience": "JurisFlowUsers"
  }
}
```

### 6. Run the Application
```bash
dotnet run --project Api
```

API will be available at: `https://localhost:7045`

## 📁 Project Structure
```
JurisFlow/
├── Api/                      # Web API Controllers
│   └── Controllers/
│       ├── AuthController.cs
│       ├── CaseController.cs
│       ├── PaymentController.cs
│       ├── NOCController.cs
│       └── ...
├── Business/                 # Business Logic
│   ├── Services/
│   │   ├── UserService.cs
│   │   ├── CaseService.cs
│   │   ├── PaymentService.cs
│   │   ├── NOCService.cs
│   │   └── ...
│   ├── DTO/                  # Data Transfer Objects
│   └── Jobs/
│       └── HearingReminderJob.cs
├── Database/                 # Data Layer
│   ├── Context/
│   │   └── LMSContext.cs
│   ├── Model/
│   │   ├── User.cs
│   │   ├── Case.cs
│   │   ├── Hearing.cs
│   │   ├── Payment.cs
│   │   ├── NOC.cs
│   │   └── ...
│   └── Migrations/
└── appsettings.json
```

## 🔐 Authentication & Authorization

### Roles
- **Admin**: Full system access
- **Lawyer**: Case management, comment system, NOC approval
- **Client**: View own cases, apply for NOC, make payments

### Registration Flow
1. User registers → Account created with `IsVerified = false`
2. OTP sent to email (3-minute expiry)
3. User verifies OTP → `IsVerified = true`
4. Login enabled

### Endpoints

#### Authentication
```
POST   /api/user/register       - Register new user
POST   /api/user/login          - Login
POST   /api/user/verify         - Verify OTP
POST   /api/user/resend-otp     - Resend verification OTP
PUT    /api/user/profile        - Update own profile
POST   /api/user/change-password- Change password
```

#### Cases
```
GET    /api/case/all            - Get all cases (role-filtered)
GET    /api/case/{id}           - Get case by ID
POST   /api/case/add            - Create case (Admin only)
PUT    /api/case/update         - Update case (Admin only)
DELETE /api/case/delete/{id}    - Delete case (Admin only)
```

#### Payments
```
POST   /api/payment/initiate    - Initiate SSLCommerz payment
POST   /api/payment/cash        - Record cash payment (Admin/Lawyer)
GET    /api/payment/success     - SSLCommerz success callback
GET    /api/payment/fail        - SSLCommerz fail callback
POST   /api/payment/ipn         - SSLCommerz IPN webhook
GET    /api/payment/all         - Get all payments (role-filtered)
GET    /api/payment/{id}        - Get payment by ID
```

#### Hearings
```
GET    /api/hearing/bycase/{caseId} - Get hearings by case
POST   /api/hearing/add             - Add hearing
PUT    /api/hearing/update          - Update hearing
DELETE /api/hearing/delete/{id}     - Delete hearing
```

#### Comments
```
GET    /api/comment/allbycase?caseId={id} - Get comments by case
POST   /api/comment/create                - Add comment
PUT    /api/comment/update                - Update comment
DELETE /api/comment/delete?commentId={id} - Delete comment
```

#### NOC
```
POST   /api/noc/apply           - Apply for NOC (Client)
POST   /api/noc/approve         - Approve NOC (Admin/Lawyer)
POST   /api/noc/reject          - Reject NOC (Admin/Lawyer)
GET    /api/noc/all             - Get all NOCs (role-filtered)
GET    /api/noc/by-case/{id}    - Get NOC by case ID
```

#### Reports
```
GET    /api/report/monthly?year=2026&month=2  - Monthly report
GET    /api/report/yearly?year=2026           - Yearly report (all 12 months)
```

#### Mail
```
POST   /api/mail/send           - Send email (Admin/Lawyer)
GET    /api/mail/all            - Get sent emails
```

## 💳 Payment Flow

### Online Payment (SSLCommerz)
1. Client initiates payment → `POST /api/payment/initiate`
2. Backend creates payment record with `Status = PENDING`
3. Returns SSLCommerz gateway URL
4. Client completes payment on SSLCommerz
5. SSLCommerz redirects to success/fail URL
6. Backend validates payment → Updates status to `SUCCESS`
7. Case/Hearing marked as paid
8. Payment receipt email sent automatically

### Cash Payment
1. Admin/Lawyer clicks "Record Cash" button
2. Backend creates payment with `Status = SUCCESS`
3. Case/Hearing marked as paid immediately
4. Payment receipt email sent

## 📧 Email System

### SMTP Configuration
Admins configure SMTP settings via `/api/smtp` endpoints:
- Host (e.g., smtp.gmail.com)
- Port (587 for TLS)
- Username & Password
- Sender email

### Automated Emails
1. **OTP Verification**: 6-digit code, 3-minute expiry
2. **Payment Receipts**: Professional receipt with transaction details
3. **Hearing Reminders**: Sent if no comment added after hearing date
4. **NOC Certificates**: Formal certificate when approved

## 🔔 Background Jobs

### Hearing Reminder Job
- Runs every **2 minutes** (testing) or **24 hours** (production)
- Checks hearings where:
  - `HearingDate` has passed
  - `ReminderSent = false`
  - No comment exists for the case
- Sends reminder email to lawyer
- Marks `ReminderSent = true` (one-time reminder)

Configuration in `Program.cs`:
```csharp
builder.Services.AddHostedService<HearingReminderJob>();
```

## 📊 Database Schema

### Core Tables
- **User**: Authentication, roles, salary assignment
- **Case**: Case details, status, fee tracking
- **Hearing**: Hearing schedule, payment status, reminder tracking
- **Payment**: Transaction records, SSLCommerz integration
- **Comment**: Case updates with status changes
- **NOC**: No Objection Certificate applications
- **MailLog**: Email tracking
- **Token**: OTP storage
- **Salary**: Salary tiers for lawyers
- **SmtpSettings**: Email configuration

### Key Relationships
```
User (1) ─── (*) Case (Handling Lawyer)
Case (1) ─── (*) Hearing
Case (1) ─── (*) Payment
Case (1) ─── (*) Comment
Case (1) ─── (1) NOC
User (*) ─── (1) Salary
```

## 🧪 Testing

### Manual Testing with Swagger
Navigate to: `https://localhost:7045/swagger`

### Test User Credentials
```
Admin:
Email: admin@jurisflow.com
Password: Admin@123

Lawyer:
Email: lawyer@jurisflow.com
Password: Lawyer@123

Client:
Email: client@jurisflow.com
Password: Client@123
```

### SSLCommerz Test Cards
```
Card Number: 4111 1111 1111 1111
CVV: Any 3 digits
Expiry: Any future date
```

## 🚦 API Response Format

All endpoints return standardized responses:

### Success Response
```json
{
  "success": true,
  "message": "Operation completed successfully",
  "data": { /* response data */ },
  "errorCode": null,
  "stackTrace": null
}
```

### Error Response
```json
{
  "success": false,
  "message": "Error description",
  "data": null,
  "errorCode": "ERROR_CODE",
  "stackTrace": "Stack trace (dev only)"
}
```

## 🔧 Configuration

### Environment-Specific Settings

**Development** (`appsettings.Development.json`):
- Enable detailed errors
- SSLCommerz Sandbox mode
- Logging: Debug level

**Production** (`appsettings.json`):
- Disable detailed errors
- SSLCommerz Live mode
- Logging: Warning level

## 📝 Migration Commands
```bash
# Create new migration
dotnet ef migrations add MigrationName

# Apply migrations
dotnet ef database update

# Remove last migration
dotnet ef migrations remove

# Generate SQL script
dotnet ef migrations script
```

## 🐛 Troubleshooting

### Common Issues

**1. Database Connection Failed**
- Verify PostgreSQL is running
- Check connection string in `appsettings.json`
- Ensure database exists

**2. SSLCommerz Payment Fails**
- Verify Store ID and Password
- Check if using Sandbox mode correctly
- Ensure callback URLs are accessible (use ngrok for localhost)

**3. Emails Not Sending**
- Check SMTP configuration in database
- For Gmail: Use App Password, not regular password
- Verify port 587 is not blocked by firewall

**4. JWT Token Invalid**
- Ensure JWT Key is at least 32 characters
- Check token expiry (default: 2 hours)
- Verify Issuer and Audience match

**5. Background Job Not Running**
- Check console logs for errors
- Verify `AddHostedService` is registered in `Program.cs`
- Check database for past hearings

## 📚 Additional Resources

- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core)
- [Entity Framework Core](https://docs.microsoft.com/ef/core)
- [SSLCommerz Integration Guide](https://developer.sslcommerz.com/)
- [PostgreSQL Documentation](https://www.postgresql.org/docs/)

## 📄 License

This project is licensed under the MIT License.

## 👥 Contributors

- **Mash** - Initial work and development

## 📞 Support

For issues and questions:
- Email: support@jurisflow.com
- GitHub Issues: [Create an issue](https://github.com/yourusername/jurisflow-backend/issues)

---

Built with ❤️ using ASP.NET Core 10
