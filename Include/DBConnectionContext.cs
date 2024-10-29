using Microsoft.EntityFrameworkCore;

namespace MyNotice.Include
{
	public class DBConnectionContext : DbContext
	{
		public DBConnectionContext(DbContextOptions<DBConnectionContext> options) : base(options) { }
	}
}
