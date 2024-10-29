using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyNotice.Include;
using MyNotice.Models;
using MyNotice.Models.Tables;
using System.Data;
using static Azure.Core.HttpHeader;

namespace MyNotice.Controllers
{
	public class NoticeController : Controller
	{
        //DB작업을 위해
        private readonly DBConnectionContext _context;

		//paging 변수 start
		public int PageIndex { get; set; } = 0;
		public bool SearchMode { get; set; } = false;
		public string SearchCondition { get; set; } = "";
		public string SearchKeyword { get; set; } = "";
		public int TotalRecordCount { get; set; } = 0;
        //paging 변수 end

        public NoticeController(DBConnectionContext context) 
		{
			_context = context;
		}

		// 목록
		public IActionResult Index() //기본 ActionResult를 IActionResult로 변경함(여러 결과의 타입을 포함, 다양한 형태의 결과를 반환, 더 유연)
		{
			//검색 모드인지 아닌지 확인
			SearchMode = (!string.IsNullOrEmpty(Request.Query["SearchCondition"].ToString()) && !string.IsNullOrEmpty(Request.Query["SearchKeyword"].ToString()));
			if (SearchMode)//검색이라면 넘어온 변수의 값들을 저장
            {
                SearchCondition = Request.Query["SearchCondition"].ToString();
				SearchKeyword = Request.Query["SearchKeyword"].ToString();
            }

			//페이지 번호 유무 확인
			if (!string.IsNullOrEmpty(Request.Query["CurrentPage"].ToString()))
			{
				PageIndex = Convert.ToInt32(Request.Query["CurrentPage"].ToString()) - 1;//인덱스가 0번 부터라...
            }


			//데이터를 가지고 와볼까..
			DAL dal = new DAL(_context);// Models/DAL.cs에 있지

			//전체 게시글 수
			TotalRecordCount = dal.GetNoticeAllCount(SearchCondition, SearchKeyword);

			//전체 글
			DataTable dt = dal.GetNoticeList(PageIndex, SearchCondition, SearchKeyword);
			if (dt.Rows.Count == 0)
			{
				ViewBag.DataList = null;
			}
			else
			{
				ViewBag.DataList = dt;
			}

            //기타 페이징 관련된 것도 ViewBag에 저장해서 목록페이지에서 사용하자
            ViewBag.TotalRecordCount = TotalRecordCount;
            ViewBag.SearchMode = SearchMode;
            ViewBag.SearchCondition = SearchCondition;
            ViewBag.SearchKeyword = SearchKeyword;

            return View();
		}

        //등록
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        //등록 처리
        [HttpPost]
        public IActionResult Create(Notice model)
        {
            DAL dal = new DAL(_context);

			dal.SetNoticeWriteOk(model);

            return RedirectToAction("Index");
        }

		//보기
		[HttpGet]
		public IActionResult Details(int idx, int currentPage, string searchCondition, string searchKeyword)
		{
			DAL dal = new DAL(_context);

			//조회수 증가
			dal.SetNoticeHitCountOk(idx);

			//공지사항 PK에 따른 내용들 가져오기
			var noticeData = dal.GetNoticeView(idx);
			ViewBag.NoticeData = noticeData;

			return View();
		}

        //수정
        [HttpGet]
        public IActionResult Edit(int idx, int currentPage, string searchCondition, string searchKeyword)
        {
            DAL dal = new DAL(_context);

            //공지사항 PK에 따른 내용들 가져오기
            var noticeData = dal.GetNoticeView(idx);
            ViewBag.NoticeData = noticeData;

            return View();
        }

        //수정 처리
        [HttpPost]
        public IActionResult Edit(Notice model, int idx, int currentPage, string searchCondition, string searchKeyword)
        {
            DAL dal = new DAL(_context);

			dal.SetNoticeModifyOk(model, idx);

            return RedirectToAction("Index", new { CurrentPage = currentPage, SearchCondition = searchCondition, SearchKeyword = searchKeyword });
        }

        //삭제 처리
        [HttpGet]
        public IActionResult DeleteOk(int idx, int currentPage, string searchCondition, string searchKeyword)
        {
            DAL dal = new DAL(_context);

            dal.SetNoticeDeleteOk(idx);

            return RedirectToAction("Index", new { CurrentPage = currentPage, SearchCondition = searchCondition, SearchKeyword = searchKeyword });
        }
    }
}
