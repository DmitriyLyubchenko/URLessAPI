using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using URLessCore.Interfaces;
using URLessCore.Models.RequestModels;
using URLessCore.Models.ResponseModels;

namespace URLess.Controllers
{
    /// <summary>
    /// Controller for Urls shorting
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class UrlController : ControllerBase
    {
        private readonly IUrlService _urlService;

        public UrlController(IUrlService urlService) 
        {
            _urlService = urlService;
        }

        /// <summary>
        /// Get original Url and redirect to it
        /// </summary>
        /// <param name="id">Shorted Url</param>
        /// <returns>Original Url or Not Found</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status301MovedPermanently)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUrl([FromRoute][StringLength(6, MinimumLength = 6)] string id) 
        {
            var url = await _urlService.GetUrl(id);

            if (url == null)
            {
                return NotFound();
            }

            return RedirectPermanent(url.Original);
        }

        /// <summary>
        /// Create new shorted Url
        /// </summary>
        /// <param name="request">Url to be shorted</param>
        /// <returns>Response with shorted and original Url</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(UrlResponse))]
        public async Task<IActionResult> CreateUrl([FromBody] CreateUrlRequest request) 
        {
            var result = await _urlService.CreateUrl(request.Url);

            return Created("/", result);
        }
    }
}
