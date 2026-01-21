using AIZ_MVP_Bussiness.Abstractions;
using AIZ_MVP_Bussiness.Dtos.ResponseDtos;
using AIZ_MVP_Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Wrappers;

namespace AIZ_MVP_Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobDescriptionController : ControllerBase
    {
        private readonly IJobDescriptionService _jobDescriptionService;
        public JobDescriptionController(IJobDescriptionService jobDescriptionService)
        {
            _jobDescriptionService = jobDescriptionService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllJobDescription()
        {
            var result = await _jobDescriptionService.GetAllJobDescription();
            return Ok(new ApiResponse<List<JobDescription>>
            {
                Data = result.Value
            });
        }
    }
}
