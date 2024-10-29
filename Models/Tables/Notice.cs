using System.ComponentModel.DataAnnotations;

namespace MyNotice.Models.Tables
{
	public class Notice
	{
		[Key]
		public int Idx { get; set; }
		public string BTitle { get; set; }
		public string BContent { get; set; }
		public string BUserName { get; set; }
		public DateTime BRegDate { get; set; }
		public int BHitCount { get; set; }
		public bool BDel { get; set; }
	}
}
