using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyNotice.Include;
using MyNotice.Models;
using System.Data;

namespace MyNotice.Controllers
{
	public class SampleController : Controller
	{
		private readonly DBConnectionContext _context;//DB작업을 위해
		public SampleController(DBConnectionContext context)
		{
			_context = context;
		}

		// GET: SampleController
		public IActionResult Index() //기본 ActionResult를 IActionResult로 변경함(여러 결과의 타입을 포함, 다양한 형태의 결과를 반환, 더 유연)
		{
			//DAL.cs에서 "GetNoticeSampleTenData()" 이 부분을 가져오기 위해
			DAL dal = new DAL(_context);

			DataTable dt = dal.GetNoticeSampleTenData();
			if (dt.Rows.Count == 0)
			{
				ViewBag.DataList = null;//데이터가 없으면 DataList라는 곳에 null 값을 넣고
			}
			else
			{
				ViewBag.DataList = dt;//데이터가 있으면 DataList라는 곳에 dt로 가져온 값을 넣어줌
			}

			return View();
		}
	}
}
