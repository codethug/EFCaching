using System;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using EF.Data.Context;
using EF.Data.Models;
using EF.Test.Helpers;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EF.Test
{
	[TestClass]
	public class RefreshEntitiesTests
	{
		[TestMethod]
		public void ReloadEntityWillForceEFToUpdateEntityInCache()
		{
			// Create initial data in database
			int sueID = CustomerHelpers.AddCustomer("Sue", "VA");

			using (var context1 = new EFTestContext())
			{
				// Get Sue's customer entity
				var sue = context1.Customers.Where(c => c.ID == sueID).First();
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

				// Verify our original context has the old data
				var sueNotUpdated = context1.Customers.First(c => c.ID == sueID);
				sueNotUpdated.Name.Should().Be("Sue");

				// Force EF to reload Sue's data
				context1.Entry(sue).Reload();

				// Retrieve the updated record from context1, which shows us
				// that the cache has been updated
				var sueUpdatedInOriginalContext = context1.Customers.First(c => c.ID == sueID);
				sueUpdatedInOriginalContext.Name.Should().Be("Susan");
			}

			// Cleanup
			CustomerHelpers.DeleteCustomer(sueID);
		}

		[TestMethod]
		public void RefreshingMultipleEntitiesInEFWillForceEFToUpdateEntityInCache()
		{
			try
			{
				// Create initial data in database
				int sueID = CustomerHelpers.AddCustomer("Sue", "VA");
				int jimID = CustomerHelpers.AddCustomer("Jim", "VA");

				using (var context1 = new EFTestContext())
				{
					// Get Sue's and Jim's customer entities
					var sue = context1.Customers.Where(c => c.ID == sueID).First();
					var jim = context1.Customers.Where(c => c.ID == jimID).First();
					// Name should be "Sue"/"Jim"
					sue.Name.Should().Be("Sue");
					jim.Name.Should().Be("Jim");

					// Update data in other context (simulating a second user)
					using (var context2 = new EFTestContext())
					{
						context2.Customers.First(c => c.ID == sueID).Name = "Susan";
						context2.Customers.First(c => c.ID == jimID).Name = "James";
						context2.SaveChanges();
					}

					// Verify the name has been updated in the database
					using (var context3 = new EFTestContext())
					{
						var sueIsUpdated = context3.Customers.First(c => c.ID == sueID);
						sueIsUpdated.Name.Should().Be("Susan");
						var jimIsUpdated = context3.Customers.First(c => c.ID == jimID);
						jimIsUpdated.Name.Should().Be("James");
					}

					// Verify our original context has the old data
					var sueNotUpdated = context1.Customers.First(c => c.ID == sueID);
					sueNotUpdated.Name.Should().Be("Sue");
					var jimNotUpdated = context1.Customers.First(c => c.ID == jimID);
					jimNotUpdated.Name.Should().Be("Jim");

					// Force EF to refresh Sue's and Jim's data
					var objectContext = ((IObjectContextAdapter) context1).ObjectContext;
					var objectsToRefresh = new Customer[] {jim, sue};
					objectContext.Refresh(RefreshMode.StoreWins, objectsToRefresh);

					// You can also query the ObjectStateManager for the objects you need to refresh
					//var objectsToRefresh = objectContext.ObjectStateManager.GetObjectStateEntries(
					//	EntityState.Added | EntityState.Deleted | EntityState.Modified | EntityState.Unchanged)
					//	.Where(e => e.EntityKey != null)
					//	.Select(e => e.Entity)
					//	.OfType<Customer>()
					//	.Where(c => (new int[] {sueID, jimID}).Contains(c.ID));
					//objectContext.Refresh(RefreshMode.StoreWins, objectsToRefresh);

					// Verify our original context has the new data
					var sueUpdatedInOriginalContext = context1.Customers.First(c => c.ID == sueID);
					sueUpdatedInOriginalContext.Name.Should().Be("Susan");
					var jimUpdatedInOriginalContext = context1.Customers.First(c => c.ID == jimID);
					jimUpdatedInOriginalContext.Name.Should().Be("James");
				}

				// Cleanup
				CustomerHelpers.DeleteCustomer(sueID);
				CustomerHelpers.DeleteCustomer(jimID);

			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
			}
		}

	}
}
