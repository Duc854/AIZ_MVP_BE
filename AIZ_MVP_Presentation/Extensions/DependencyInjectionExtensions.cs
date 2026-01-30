using AIZ_MVP_Bussiness.Abstractions;
using AIZ_MVP_Bussiness.Services;
using AIZ_MVP_Data.Abstractions;
using AIZ_MVP_Data.Context;
using AIZ_MVP_Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AIZ_MVP_Presentation.Extensions
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            //Add DBContext
            services.AddDbContext<AIZMvpDbContext>(
                options => options.UseSqlServer(
                    configuration.GetConnectionString("SQLSeverConnection"))
            );
            //Add custom extension
            services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
            services.AddScoped<ITokenProvider, JwtTokenProvider>();
            //Add Repository
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<IJobDescriptionRepository, JobDescriptionRepository>();
            services.AddScoped<ILicenseRepository, LicenseRepository>();
            services.AddScoped<IInterviewSessionRepository, InterviewSessionRepository>();
            services.AddScoped<IInterviewTurnRepository, InterviewTurnRepository>();
            services.AddScoped<IInterviewAnswerRepository, InterviewAnswerRepository>();
            services.AddScoped<IInterviewEvaluationRepository, InterviewEvaluationRepository>();
            services.AddScoped<IPaymentRepository, PaymentRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            //Add Service
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IJobDescriptionService, JobDescriptionService>();
            services.AddScoped<IInterviewSessionService, InterviewSessionService>();
            services.AddScoped<IInterviewTurnService, InterviewTurnService>();
            services.AddScoped<IInterviewAnswerService, InterviewAnswerService>();
            services.AddScoped<IInterviewEvaluationService, InterviewEvaluationService>();
            services.AddScoped<IPaymentService, PaymentService>();
            return services;
        }
    }
}
