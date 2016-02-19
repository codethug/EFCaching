using System.Data.Entity;
using EF.Data.Models;

namespace EF.Data.Context
{
	public class EFTestContext : DbContext
	{
		public DbSet<Customer> Customers { get; set; }
	}
}