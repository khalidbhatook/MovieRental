using MovieRental.Data;

namespace MovieRental.Customer
{
    public class CustomerFeatures:ICustomerFeatures
    {
        private readonly MovieRentalDbContext _movieRentalDb;

        public CustomerFeatures(MovieRentalDbContext movieRentalDb)
        {
            _movieRentalDb = movieRentalDb;
        }

        public async Task<Customer> Save(Customer customer)
        {
            await _movieRentalDb.Customers.AddAsync(customer);
            await _movieRentalDb.SaveChangesAsync();
            return customer;
        }

        public List<Customer> GetAll()
        {
            return _movieRentalDb.Customers.ToList();
        }

    }
}
