using System.Text;
using Business.Services;
using Business.Settings;
using Database.Context;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers(options =>
{
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.Filters.Add(new AuthorizeFilter(policy));
});
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Legal Case Management API",
        Version = "v1"
    });

    // Define the Bearer security scheme
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",        // lowercase!
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token. Example: eyJhbGci..."
    });

    // ✅ NEW v10 syntax — use delegate + OpenApiSecuritySchemeReference
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});
// Settings
builder.Services.Configure<SSLCommerzSettings>(
    builder.Configuration.GetSection("SSLCommerz"));

builder.Services.AddHttpContextAccessor(); // For accessing claims

builder.Services.AddDbContext<LMSContext>(); // Context initialized

builder.Services.AddScoped<CaseService>();
builder.Services.AddScoped<CaseTypeService>();
builder.Services.AddScoped<CommentService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<HearingService>();
builder.Services.AddScoped<PaymentMethodService>();
builder.Services.AddScoped<PaymentService>();
builder.Services.AddScoped<RoleService>();
builder.Services.AddScoped<SmtpService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<FileService>();
builder.Services.AddScoped<OTPService>();

builder.Services.AddLogging();

var allowedOrigins = builder.Configuration.GetValue<string>("allowedOrigins")?.Split(",") ?? ["*"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("NgrokPolicy", policy => //Allowing Ngrok and bypassing SSL Certificate
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
var jwtKey = builder.Configuration.GetValue<string>("Jwt:Key");
var jwtIssuer = builder.Configuration.GetValue<string>("Jwt:Issuer");
var jwtExpires = builder.Configuration.GetValue<int>("Jwt:ExpiresInMinutes");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = ctx =>
        {
            Console.WriteLine("Auth failed: " + ctx.Exception.Message);
            return Task.CompletedTask;
        }
    };
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true, // optional
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey ?? ""))
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "LCMS API V1");
        c.RoutePrefix = string.Empty; // 🔥 This makes Swagger open at root URL
    });
}
//Allowing Ngrok to work as a proxy
app.Use(async (context, next) =>
{
    context.Request.Headers["ngrok-skip-browser-warning"] = "true";
    await next();
});

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors("NgrokPolicy");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();