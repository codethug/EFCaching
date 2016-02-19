using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EF.Data.Context;
using EF.Data.Models;

namespace EF.Test.Helpers
{
	public static class CustomerHelpers
	{
		public static int AddCustomer(string name, string state)
		{
			using (var context = new EFTestContext())
			{
				// Create initial data
				var newCustomer = context.Customers.Add(new Data.Models.Customer() { Name = name, State = state });
				context.SaveChanges();
				return newCustomer.ID;
			}
		}

		public static void DeleteCustomer(int customerID)
		{
			// Cleanup
			using (var contextCleanup = new EFTestContext())
			{
				var sue = contextCleanup.Customers.First(c => c.ID == customerID);
				contextCleanup.Customers.Remove(sue);
				contextCleanup.SaveChanges();
			}
		}
	}
}
