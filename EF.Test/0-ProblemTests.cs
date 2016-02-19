using System;
using System.Linq;
using EF.Data.Context;
using EF.Data.Models;
using EF.Test.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;

namespace EF.Test
{
	[TestClass]
	public class ProblemTests
	{
		[TestMethod]
		public void ContextFailsToReturnCachedDataUpdatedInDb()
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

				// But our original context has the stale data cached
				var sueNotUpdated = context1.Customers.First(c => c.ID == sueID);
				sueNotUpdated.Name.Should().Be("Sue");
			}

			// Cleanup
			CustomerHelpers.DeleteCustomer(sueID);
		}

		[TestMethod]
		public void CacheKnowsWhenDataIsDeletedInDb()
		{
			// Create initial data in database
			int sueID = CustomerHelpers.AddCustomer("Sue", "VA");

			using (var context1 = new EFTestContext())
			{
				// Get Sue's customer entity
				var sue = context1.Customers.First(c => c.ID == sueID);
				// Name should be "Sue"
				sue.Name.Should().Be("Sue");

				// Delete data using other context (simulating a second user)
				using (var context2 = new EFTestContext())
				{
					var sueToDelete = context2.Customers.First(c => c.ID == sueID);
					context2.Customers.Remove(sueToDelete);
					context2.SaveChanges();
				}

				// Verify record has been deleted from the database
				using (var context3 = new EFTestContext())
				{
					var sueIsDeleted = context3.Customers.FirstOrDefault(c => c.ID == sueID);
					sueIsDeleted.Should().BeNull();
				}

				// And our original context shows the record as deleted
				var sueNotDeleted = context1.Customers.FirstOrDefault(c => c.ID == sueID);
				sueNotDeleted.Should().BeNull();
			}
		}

		[TestMethod]
		public void CacheKnowsWhenDataIsAddedToDb()
		{
			// Create initial data in database
			int sueID = CustomerHelpers.AddCustomer("Sue", "VA");

			using (var context1 = new EFTestContext())
			{
				// Get count of customers in Virginia
				var numInVABefore = context1.Customers.Where(c => c.State == "VA").ToList().Count;

				// Add new customer using separate context (simulating a second user)
				int jamesID = CustomerHelpers.AddCustomer("James", "VA");

				// Verify new record exists in database using another context
				using (var context3 = new EFTestContext())
				{
					var jamesExists = context3.Customers.FirstOrDefault(c => c.ID == jamesID);
					jamesExists.Should().NotBeNull();
				}

				// Our original context shows the new record when queried
				var numInVAAfter = context1.Customers.Where(c => c.State == "VA").ToList().Count;
				numInVAAfter.Should().Be(numInVABefore + 1);
			}

			// Cleanup
			CustomerHelpers.DeleteCustomer(sueID);
		}

	}
}
