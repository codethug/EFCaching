using System.ComponentModel.DataAnnotations;

namespace EF.Data.Models
{
	public class Customer
	{
		[Key]
		public int ID { get; set; }
		[Required, MaxLength(255)]
		public string Name { get; set; }
		[MaxLength(255)]
		public string State { get; set; }
	}
}