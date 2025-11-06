using Microsoft.AspNetCore.Mvc;
using MovieRental.Movie;

namespace MovieRental.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MovieController : ControllerBase
    {

        private readonly IMovieFeatures _features;

        public MovieController(IMovieFeatures features)
        {
            _features = features;
        }

        [HttpGet]
        public IActionResult Get()
        {

            try
            {
                return Ok(_features.GetAll());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "An error occurred while processing your request."
                });
            }

            
        }

        [HttpPost]
        public IActionResult Post([FromBody] Movie.Movie movie)
        {
            try
            {
                return Ok(_features.Save(movie));
            }

            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "An error occurred while processing your request."
                });
            }


        }
    }
}
