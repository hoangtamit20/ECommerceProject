using Core.Domain;
using Core.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CET.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LinkHelperController : ControllerBase
    {
        private readonly ILogger<LinkHelperController> _logger;
        private readonly ICETRepository _cETRepository;

        public LinkHelperController(
            ICETRepository cETRepository,
            ILogger<LinkHelperController> logger)
        {
            _logger = logger;
            _cETRepository = cETRepository;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetLinkHelpers()
        {
            var a = RuntimeContext.CurrentAccessToken;
            return Ok(await _cETRepository.GetSet<LinkHelperEntity>().ToListAsync());
        }


        // [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddUrlEndpoint(string developmentEndpoint, string productionEndpoint)
        {
            if (string.IsNullOrEmpty(developmentEndpoint) && string.IsNullOrEmpty(productionEndpoint))
            {
                return BadRequest($"Data is invalid.");
            }
            try
            {
                var urlHelpers = await _cETRepository.GetSet<LinkHelperEntity>().FirstOrDefaultAsync();
                if (urlHelpers == null)
                {
                    await _cETRepository.AddAsync<LinkHelperEntity>(entity: new LinkHelperEntity()
                    {
                        DevelopmentEndpoint = developmentEndpoint,
                        ProductionEndpoint = productionEndpoint
                    });
                }
                else
                {
                    if (!string.IsNullOrEmpty(developmentEndpoint))
                    {
                        urlHelpers.DevelopmentEndpoint = developmentEndpoint;
                    }
                    if (!string.IsNullOrEmpty(productionEndpoint))
                    {
                        urlHelpers.ProductionEndpoint = productionEndpoint;
                    }
                    await _cETRepository.UpdateAsync<LinkHelperEntity>(entity: urlHelpers);
                }
                return Ok($"Add link successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(statusCode: StatusCodes.Status500InternalServerError, $"An error occured while add link.");
            }
        }
    }
}