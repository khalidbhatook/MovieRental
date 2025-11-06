
namespace MovieRental.Customer
{
    public interface ICustomerFeatures
    {
        Task<Customer> Save(Customer movie);
        List<Customer> GetAll();
    }

}
