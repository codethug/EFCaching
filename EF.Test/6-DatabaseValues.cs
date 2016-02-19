using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using EF.Data.Context;
using EF.Test.Helpers;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EF.Test
{
	[TestClass]
	public class DatabaseValues
	{
		[TestMethod]
		public void GetDatabaseValuesWillUpdateTheCache()
		{
			// Create initial data in database
			int sueID = CustomerHelpers.AddCustomer("Sue", "VA");

			using (var context1 = new EFTestContext())
			{
				// Get Sue's customer entity
				var sue = context1.Customers.First(c => c.ID == sueID);
				// Name should be "Sue"
				sue.Name.Should().Be("Sue");

				// Update data in other context (simulating a second user)
				using (var context2 = new EFTestContext())
				{
					context2.Customers.First(c => c.ID == sueID).Name = "Susan";
					context2.SaveChanges();
				}

				// Verify the name has been updated in the database
				using (var context3 = new EFTestContext())
				{
					var sueIsUpdated = context3.Customers.First(c => c.ID == sueID);
					sueIsUpdated.Name.Should().Be("Susan");
				}

				// Verify we still have stale data in the cache
				var sueNotUpdated = context1.Customers.First(c => c.ID == sueID);
				sueNotUpdated.Name.Should().Be("Sue");

				// Use GetDatabaseValues to get a dictionary of the current db values (ignoring the cache)
				DbPropertyValues sueDbValues = context1.Entry(sueNotUpdated).GetDatabaseValues();
				sueDbValues["Name"].Should().Be("Susan");

				// Verify we still have stale data in the cache
				var sueStillNotUpdated = context1.Customers.First(c => c.ID == sueID);
				sueStillNotUpdated.Name.Should().Be("Sue");
			}

			// Cleanup
			CustomerHelpers.DeleteCustomer(sueID);

		}
	}
}
