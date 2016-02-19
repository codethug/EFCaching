using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EF.Data.Context;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EF.Test.Helpers
{
	[TestClass]
	public class TransactionTest
	{
		protected EFTestContext context;
		protected DbContextTransaction transaction;

		[TestInitialize]
		public void TransactionTestStart()
		{
			context = new EFTestContext();
			transaction = context.Database.BeginTransaction();
		}

		[TestCleanup]
		public void TransactionTestEnd()
		{
			transaction.Rollback();
			transaction.Dispose();
			context.Dispose();
		}
	}
}
