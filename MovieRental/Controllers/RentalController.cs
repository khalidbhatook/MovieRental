using Microsoft.AspNetCore.Mvc;
using MovieRental.Movie;
using MovieRental.PaymentProviders;
using MovieRental.Rental;

namespace MovieRental.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RentalController : ControllerBase
    {

        private const double MOVIE_RENT = 5.0;
        private readonly IRentalFeatures _features;

        public RentalController(IRentalFeatures features)
        {
            _features = features;
        }


        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Rental.Rental rental)
        {
            try
            {



                if (await ProcessPaymnet(rental.PaymentMethod, MOVIE_RENT * rental.DaysRented))
                    return Ok(await _features.Save(rental));
                else
                    return BadRequest(new
                    {
                        message = "Payment processing failed."
                    });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "An error occurred while processing your request."
                });
            }
        }

        [HttpGet("{customerName}")]
        public IActionResult Get(string customerName)
        {
            try
            {
                return Ok(_features.GetRentalsByCustomerName(customerName));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "An error occurred while processing your request."
                });
            }
            

            
        }

        private async Task<bool> ProcessPaymnet(string method, double amount)
        {
            

            switch (method)
            {
                case "MbWay":
                    MbWayProvider mbWayProvider = new MbWayProvider();
                    return await mbWayProvider.Pay(amount);
                case "PayPal":
                    PayPalProvider payPalProvider = new PayPalProvider();
                    return await payPalProvider.Pay(amount);
                default:
                    return false;
            }
        }
    }


}

