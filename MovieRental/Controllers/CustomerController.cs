using Microsoft.AspNetCore.Mvc;
using MovieRental.Customer;

namespace MovieRental.Controllers
{
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerFeatures _features;

        public CustomerController(ICustomerFeatures features)
        {
            _features = features;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_features.GetAll());
        }

        [HttpPost]
        public IActionResult Post([FromBody] Customer.Customer customer)
        {
            try
            {
                return Ok(_features.Save(customer));
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
