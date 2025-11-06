# MovieRental Exercise

This is a dummy representation of a movie rental system.
Can you help us fix some issues and implement missing features?

**The app is throwing an error when we start, please help us. Also, tell us what caused the issue.**
#Solution 1 (Incorrect):

builder.Services.AddSingleton<MovieRentalDbContext>();//Program.cs
Description: This is not recommended because it creates a single instance of the DbContext for the entire application's lifetime. This can lead to concurrency issues, memory leaks, and data corruption as the same context is shared across all requests.

#Solution 2 (Correct):

builder.Services.AddScoped<MovieRentalDbContext>();//Program.cs
Description: This is the correct approach. It ensures a new instance of the MovieRentalDbContext is created for each HTTP request. This is safe for web applications as it prevents simultaneous modifications from different users and ensures proper resource disposal.

Root Cause: The initial use of AddSingleton was the primary cause of the startup error and subsequent potential runtime issues.

**The rental class has a method to save, but it is not async, can you make it async and explain to us what is the difference?**
Solution: The Save method in the RentalFeatures class and its interface was updated to be asynchronous.

Updated Code in RentalFeatures:

//RentalFeatures.cs
public async Task<Rental> Save(Rental rental)
{
    await _movieRentalDb.Rentals.AddAsync(rental);
    await _movieRentalDb.SaveChangesAsync();
    return rental;
}
Updated Interface IRentalFeatures:

//IRentalFeatures.cs
public interface IRentalFeatures
{
    Task<Rental> Save(Rental rental);
}
Updated Controller Endpoint:

//RentalController.cs
[HttpPost]
public async Task<IActionResult> Post([FromBody] Rental rental)
{
    if (await ProcessPayment(rental.PaymentMethod, MOVIE_RENT * rental.DaysRented))
        return Ok(await _features.Save(rental));
    else
        return BadRequest(new { message = "Payment processing failed." });
}
Difference Between Sync and Async:

Synchronous: The thread is blocked until the operation (like a database call) completes. This can severely limit the application's ability to handle multiple concurrent requests.

Asynchronous: The thread is released while waiting for an I/O operation (like a database call) to finish. This freed-up thread can be used to serve other requests, improving the application's scalability and responsiveness.


**Please finish the method to filter rentals by customer name, and add the new endpoint.**
Solution: A new method was added to filter rentals, and a corresponding API endpoint was created.

Method in RentalFeatures:

public IEnumerable<Rental> GetRentalsByCustomerName(string customerName)
{
    return _movieRentalDb.Rentals.Where(e => e.CustomerName.Equals(customerName));
}
New Endpoint in RentalController:

[HttpGet("{customerName}")]
public IActionResult Get(string customerName)
{
    return Ok(_features.GetRentalsByCustomerName(customerName));
}

**We noticed we do not have a table for customers, it is not good to have just the customer name in the rental.
   Can you help us add a new entity for this? Don't forget to change the customer name field to a foreign key, and fix your previous method!**
Solution: A new Customer entity was created, and the Rental class was refactored to use a foreign key relationship instead of a simple CustomerName string.

New Customer Entity:
//Customer.cs
public class Customer
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
}
Updated Rental Class:

Rental.cs
public class Rental
{
    // ... other properties ...
    public Customer Customer { get; set; }

    [ForeignKey("Customer")]
    public int CustomerId { get; set; }
}

Updated Filter Method: The GetRentalsByCustomerName method would now need to join the Rentals and Customers tables or use Include to filter based on the customer's name from the related Customer entity.

New Customer Controller (Basic Structure):

CustomerController.cs
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
    public IActionResult Post([FromBody] Customer customer)
    {
        return Ok(_features.Save(customer));
    }
}


 **In the MovieFeatures class, there is a method to list all movies, tell us your opinion about it.**
	Analysis: The current method that lists all movies is functional but not scalable or efficient.

	Recommendations:
	Implement Pagination: Fetching all movies at once can lead to performance issues and slow response times as the movie catalog grows. Implementing pagination (e.g., using Skip and Take) is crucial.

	Introduce Filtering and Categorization: Adding properties like Genre, ReleaseYear, or Rating would allow users to filter results and find relevant content more easily, further reducing the amount of data 		transferred.

**No exceptions are being caught in this api, how would you deal with these exceptions?**
	Solution: A global and consistent strategy for handling exceptions is necessary.

	Controller-Level Try-Catch: Use try-catch blocks in controller actions to handle specific known errors and return user-friendly messages. This prevents raw exception details from being exposed to the client.

	Global Exception Middleware: Implement a custom exception handling middleware to catch any unhandled exceptions across the entire API. This middleware can log the error and return a consistent, generic error 	response (HTTP 500).

	Structured Logging: Integrate a third-party logging library like Serilog or NLog. This allows for detailed logging of exceptions, requests, and other events, which is invaluable for debugging and monitoring 		the application in production.

## Challenge (Nice to have)
We need to implement a new feature in the system that supports automatic payment processing. Given the advancements in technology, it is essential to integrate multiple payment providers into our system.

Here are the specific instructions for this implementation:

* Payment Provider Classes:
    * In the "PaymentProvider" folder, you will find two classes that contain basic (dummy) implementations of payment providers. These can be used as a starting point for your work.
* RentalFeatures Class:
    * Within the RentalFeatures class, you are required to implement the payment processing functionality.
* Payment Provider Designation:
    * The specific payment provider to be used in a rental is specified in the Rental model under the attribute named "PaymentMethod".
* Extensibility:
    * The system should be designed to allow the addition of more payment providers in the future, ensuring flexibility and scalability.

**Solution** 
public class RentalController : ControllerBase
{
    private const double MOVIE_RENT = 5.0;
    private readonly IRentalFeatures _features;

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Rental rental)
    {
        try
        {
            double totalAmount = MOVIE_RENT * rental.DaysRented;
            bool isPaymentSuccessful = await ProcessPayment(rental.PaymentMethod, totalAmount);

            if (isPaymentSuccessful)
            {
                var savedRental = await _features.Save(rental);
                return Ok(savedRental);
            }
            else
            {
                return BadRequest(new { message = "Payment processing failed." });
            }
        }
        catch (Exception ex)
        {
            // Log the exception (ex) here using a logging framework.
            return StatusCode(500, new { message = "An error occurred while processing your request." });
        }
    }

    private async Task<bool> ProcessPayment(string method, double amount)
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
                return false; // Unsupported payment method
        }
    }
}
* Payment Failure Handling:
    * If the payment method fails during the transaction, the system should prevent the creation of the rental record. In such cases, no rental should be saved to the database.
