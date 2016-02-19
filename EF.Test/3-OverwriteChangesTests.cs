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
	public class OverwriteChangesTests
	{
		[TestMethod]
		public void QueryWithOverwriteChangesWillUpdateCacheFromDatabase()
		{
			// Create initial data in database
			int sueID = CustomerHelpers.AddCustomer("Sue", "VA");

			using (var context1 = new EFTestContext())
			{
				// Get Sue's customer entity, adding it to the cache
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

				// Get Sue's customer entity using MergeOptions.OverwriteChanges, updating the entity in the cache
				var objectContext = ((IObjectContextAdapter)context1).ObjectContext;
				var objectQuery = objectContext.CreateObjectSet<Customer>().Where(c => c.ID == sueID);
				// Set the MergeOptions to overwrite what is in the cache
				(objectQuery as ObjectQuery<Customer>).MergeOption = MergeOption.OverwriteChanges;
				var sueFromOriginalContext = objectQuery.First();
				// Verify we retrieved the updated data
				sueFromOriginalContext.Name.Should().Be("Susan");

				// Get Sue from the cache again, verifying that the cache has been updated, this tme
				// querying without the OverwriteChanges MergeOption
				var sueFromOriginalContextWithoutOverwriteChanges = context1.Customers.First(c => c.ID == sueID);
				sueFromOriginalContextWithoutOverwriteChanges.Name.Should().Be("Susan");
			}

			// Cleanup
			CustomerHelpers.DeleteCustomer(sueID);
		}


		// Get item using MergeOption.OverwriteChanges.  Update item externally.  Get item normally.  Verify get get older item from cache.

		[TestMethod]
		public void StandardQueryWillNotOverwriteCacheAfterQueryWithOverwriteChanges()
		{
			// Create initial data in database
			int sueID = CustomerHelpers.AddCustomer("Sue", "VA");

			using (var context1 = new EFTestContext())
			{
				// Get Sue's customer entity using MergeOptions.OverwriteChanges, placing the entity in the cache
				var objectContext = ((IObjectContextAdapter)context1).ObjectContext;
				var objectQuery = objectContext.CreateObjectSet<Customer>().Where(c => c.ID == sueID);
				// Set the MergeOptions to overwrite what is in the cache
				(objectQuery as ObjectQuery<Customer>).MergeOption = MergeOption.OverwriteChanges;
				var sueNowInCache = objectQuery.First();
				// Verify we retrieved the original data
				sueNowInCache.Name.Should().Be("Sue");
	
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

				// Get Sue from the cache without the OverwriteChanges MergeOption
				var sueFromOriginalContext = context1.Customers.First(c => c.ID == sueID);
				// We should see the original data, because the cache hasn't been updated
				sueFromOriginalContext.Name.Should().Be("Sue");
			}

			// Cleanup
			CustomerHelpers.DeleteCustomer(sueID);
		}


		[TestMethod]
		public void QueryWithOverwriteChangesWillShowDeletedEntitiesAsDeleted()
		{
			// Create initial data in database
			int sueID = CustomerHelpers.AddCustomer("Sue", "VA");

			using (var context1 = new EFTestContext())
			{
				// Get Sue's customer entity, adding it to the cache
				var sue = context1.Customers.First(c => c.ID == sueID);
				// Name should be "Sue"
				sue.Name.Should().Be("Sue");

				// Delete data in other context (simulating a second user)
				using (var context2 = new EFTestContext())
				{
					var sueToDelete = context2.Customers.First(c => c.ID == sueID);
					context2.Customers.Remove(sueToDelete);
					context2.SaveChanges();
				}

				// Verify the data has been deleted from the database
				using (var context3 = new EFTestContext())
				{
					var sueIsDeleted = context3.Customers.FirstOrDefault(c => c.ID == sueID);
					sueIsDeleted.Should().BeNull();
				}

				// Get Sue's customer entity using MergeOptions.OverwriteChanges, updating the entity in the cache
				var objectContext = ((IObjectContextAdapter)context1).ObjectContext;
				var objectQuery = objectContext.CreateObjectSet<Customer>().Where(c => c.ID == sueID);
				// Set the MergeOptions to overwrite what is in the cache
				(objectQuery as ObjectQuery<Customer>).MergeOption = MergeOption.OverwriteChanges;
				var sueFromOriginalContext = objectQuery.FirstOrDefault();
				// Verify that sue does not exist
				sueFromOriginalContext.Should().BeNull();

				// Caution: the original sue object we created doesn't know it's been deleted
				context1.Entry(sue).State.Should().Be(EntityState.Unchanged);
				// Caution: The original object still exists
				sue.Should().NotBeNull();
				sue.Name.Should().Be("Sue");
			}
		}


	}
}
