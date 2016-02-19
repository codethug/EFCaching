using System;
using System.Data.Entity;
using System.Linq;
using EF.Data.Context;
using EF.Test.Helpers;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EF.Test
{
	[TestClass]
	public class AsNoTrackingTests
	{
		[TestMethod]
		public void AsNoTrackingDoesntAddDataToCache()
		{
			// Create initial data in database
			int sueID = CustomerHelpers.AddCustomer("Sue", "VA");

			using (var context1 = new EFTestContext())
			{
				// Get Sue's customer entity
				var sue = context1.Customers.Where(c => c.ID == sueID).AsNoTracking().First();
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

				// We can retrieve the updated record from context1, which tells us that the 
				// cache on context1 is empty (before we retrieved the data
				var sueStillUpdated = context1.Customers.First(c => c.ID == sueID);
				sueStillUpdated.Name.Should().Be("Susan");
			}

			// Cleanup
			CustomerHelpers.DeleteCustomer(sueID);
		}

		[TestMethod]
		public void AsNoTrackingDoesntReadFromTheCache()
		{
			// Create initial data in database
			int sueID = CustomerHelpers.AddCustomer("Sue", "VA");

			using (var context1 = new EFTestContext())
			{
				// Get Sue's customer entity, storing it in the cache
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

				// Use our original context to retrieve the data, bypassing the cache
				var sueStillUpdated = context1.Customers.Where(c => c.ID == sueID).AsNoTracking().ToList().First();
				sueStillUpdated.Name.Should().Be("Susan");
			}

			// Cleanup
			CustomerHelpers.DeleteCustomer(sueID);
		}


		[TestMethod]
		public void AsNoTrackingDoesntReturnDeletedData()
		{
			// Create initial data in database
			int sueID = CustomerHelpers.AddCustomer("Sue", "VA");

			using (var context1 = new EFTestContext())
			{
				// Get Sue's customer entity
				var sue = context1.Customers.Where(c => c.ID == sueID).AsNoTracking().First();
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

				// But our original context has the stale data cached
				var sueStillUpdated = context1.Customers.First(c => c.ID == sueID);
				sueStillUpdated.Name.Should().Be("Susan");
			}

			// Cleanup
			CustomerHelpers.DeleteCustomer(sueID);
		}

	}
}
