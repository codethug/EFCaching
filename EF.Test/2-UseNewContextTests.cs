using System;
using System.Data.Entity;
using System.Linq;
using EF.Data.Context;
using EF.Data.Models;
using EF.Test.Helpers;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EF.Test
{
	[TestClass]
	public class UseNewContextTests
	{
		[TestMethod]
		public void UsingANewContextWillRetrieveUpdatedData()
		{
			// Create initial data in database
			int sueID = CustomerHelpers.AddCustomer("Sue", "VA");

			using (var context1 = new EFTestContext())
			{
				// Get Sue's customer entity
				var sue = context1.Customers.Where(c => c.ID == sueID).AsNoTracking().First();
				// Name should be "Sue"
				sue.Name.Should().Be("Sue");
			}

			// Update data in other context (simulating a second user)
			using (var context2 = new EFTestContext())
			{
				context2.Customers.First(c => c.ID == sueID).Name = "Susan";
				context2.SaveChanges();
			}

			// Use a new context to retrieve the updated record
			using (var context3 = new EFTestContext())
			{
				var sueIsUpdated = context3.Customers.First(c => c.ID == sueID);
				sueIsUpdated.Name.Should().Be("Susan");
			}

			// Cleanup
			CustomerHelpers.DeleteCustomer(sueID);
		}

		[TestMethod]
		public void UsingANewContextWillNotShowDeletedData()
		{
			// Create initial data in database
			int sueID = CustomerHelpers.AddCustomer("Sue", "VA");
			Customer originalSue;

			using (var context1 = new EFTestContext())
			{
				// Get Sue's customer entity
				originalSue = context1.Customers.Where(c => c.ID == sueID).AsNoTracking().First();
				// Name should be "Sue"
				originalSue.Name.Should().Be("Sue");
			}

			// Delete data in other context (simulating a second user)
			using (var context2 = new EFTestContext())
			{
				var existingSue = context2.Customers.First(c => c.ID == sueID);
				context2.Customers.Remove(existingSue);
				context2.SaveChanges();
			}

			// Use a new context to retrieve the updated record
			using (var context3 = new EFTestContext())
			{
				var sueIsDeleted = context3.Customers.FirstOrDefault(c => c.ID == sueID);
				sueIsDeleted.Should().BeNull();
			}

			// Caution: the original entity you retrieved is still around, but not in the cache
			originalSue.Should().NotBeNull();
		}

	}
}
