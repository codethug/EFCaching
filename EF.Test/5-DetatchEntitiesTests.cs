using System;
using System.Linq;
using EF.Data.Context;
using EF.Test.Helpers;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.Entity;

namespace EF.Test
{
	[TestClass]
	public class UnitTest1
	{
		[TestMethod]
		public void DetachEntityWillForceReloadOnNextQuery()
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

				// Detatch the Entity
				context1.Entry(sue).State = EntityState.Detached;

				// Retrieve the updated record from context1
				var sueUpdatedInOriginalContext = context1.Customers.First(c => c.ID == sueID);
				sueUpdatedInOriginalContext.Name.Should().Be("Susan");
			}

			// Cleanup
			CustomerHelpers.DeleteCustomer(sueID);
		}
	}
}
