using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MyNotice.Include;
using MyNotice.Models.Tables;
using System.Data;

namespace MyNotice.Models
{
	public class DAL
	{
		private readonly DBConnectionContext _context;
		public DAL(DBConnectionContext context)
		{
			_context = context;
		}

		#region 공지사항 샘플 목록 테스트
		///<summary>
		/// 샘플 데이터 10건을 가져오는 쿼리 - GetNoticeSampleTenData()
		/// </summary>
		public DataTable GetNoticeSampleTenData() 
		{
			SqlConnection db = _context.Database.GetDbConnection() as SqlConnection;
			db.Open();

			DataTable dt = new DataTable();

			using (SqlCommand cmd = db.CreateCommand())
			{
				string SQL = @"
					SELECT
						TOP 10 Idx,
						BTitle,
						BUserName,
						BRegDate,
						BHitCount
					FROM
						NOTICE
					WHERE
						BDel = 0
					ORDER BY
						BRegDate DESC
				";
				cmd.CommandText = SQL;
				cmd.CommandType = CommandType.Text;

				SqlDataAdapter da = new SqlDataAdapter(cmd);
				da.Fill(dt);
			}

			db.Close();

			return dt;
		}
        #endregion

        //공지사항
        #region 공지사항 전체 게시글 수 가져오기 - GetNoticeAllCount()
        /// <summary>
        /// 공지사항에 등록된 전체 게시글 수를 가져온다.
        /// <param name="searchCondition">검색조건</param>
        /// <param name="SearchKeyword">검색어</param>
        /// <returns>전체 게시글 수</returns>
        /// </summary>
        public int GetNoticeAllCount(string searchCondition, string searchKeyword)
		{
			int allCnt = 0;

            SqlConnection db = _context.Database.GetDbConnection() as SqlConnection;
            db.Open();

            using (SqlCommand cmd = db.CreateCommand())
            {
                string SQL = @"
                    SELECT
						COUNT(*)
					FROM
						Notice
					WHERE
						BDel = 0
                ";
                if (!string.IsNullOrEmpty(searchKeyword.ToString()))
                {
                    switch (searchCondition.ToString())
                    {
                        case "1":
                            SQL += @" AND BTitle LIKE @searchKeyword";//공지사항 제목
                            break;
                        case "2":
                            SQL += @" AND BCONTENT LIKE @searchKeyword";//공지사항 내용
                            break;
                    }
                }
                cmd.CommandText = SQL;
				cmd.CommandType = CommandType.Text;
                if (!string.IsNullOrEmpty(searchKeyword.ToString()))
                {
                    cmd.Parameters.AddWithValue("@searchKeyword", "%" + searchKeyword.ToString() + "%");
                }

                allCnt = Convert.ToInt32(cmd.ExecuteScalar());
            }

            db.Close();

            return allCnt;
		}
        #endregion

        #region 공지사항 가져오기 - GetNoticeList()
        /// <summary>
        /// 공지사항에 등록된 게시글을 페이징에 맞게 가져온다.
        /// <param name="page">검색조건</param>
        /// <param name="searchCondition">검색조건</param>
        /// <param name="searchKeyword">검색어</param>
        /// <returns>전체 게시글 수</returns>
        /// </summary>
        public DataTable GetNoticeList(int page, string searchCondition, string searchKeyword)
        {
            DataTable dt = new DataTable();

            SqlConnection db = _context.Database.GetDbConnection() as SqlConnection;
            db.Open();

            using (SqlCommand cmd = db.CreateCommand())
            {
                string SQL = @"
                    WITH List
                    AS
                    (
	                    SELECT
							ROW_NUMBER() OVER (ORDER BY BRegDate DESC) AS 'RowNum'
							, Idx, BTitle, BContent, BUserName, BRegDate, BHitCount, BDel
						FROM Notice
						WHERE BDel = 0";
                if (!string.IsNullOrEmpty(searchKeyword.ToString()))
                {
                    switch (searchCondition.ToString())
                    {
                        case "1":
                            SQL += @" AND BTitle LIKE @searchKeyword";//공지사항 제목
                            break;
                        case "2":
                            SQL += @" AND BCONTENT LIKE @searchKeyword";//공지사항 내용
                            break;
                    }
                }
                SQL += @")
                    SELECT
	                    *
                    FROM List
                    WHERE RowNum BETWEEN @page * 10 + 1 AND (@page + 1) * 10
                ";
                cmd.CommandText = SQL;
                cmd.CommandType = System.Data.CommandType.Text;
                if (!string.IsNullOrEmpty(searchKeyword.ToString()))
                {
                    cmd.Parameters.AddWithValue("@searchKeyword", "%" + searchKeyword.ToString() + "%");
                }
                cmd.Parameters.AddWithValue("@page", page);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }

            db.Close();

            return dt;
        }
        #endregion

        #region 공지사항 글 등록 처리 - SetNoticeWriteOk()
        /// <summary>
        /// 공지사항 글 등록
        /// <param name="model">모델</param>
        /// <returns></returns>
        /// </summary>
        public void SetNoticeWriteOk(Notice model)
        {
            SqlConnection db = _context.Database.GetDbConnection() as SqlConnection;
            db.Open();

            using (SqlCommand cmd = db.CreateCommand())
            {
                string SQL = @"
                    INSERT INTO Notice(Idx, BTitle, BContent, BUserName, BRegDate, BHitCount, BDel)
                    SELECT
	                    (SELECT ISNULL(MAX(Idx), 0) + 1 FROM Notice)
	                    , @BTitle
	                    , @BContent
	                    , @BUserName
	                    , GETDATE()--BRegDate
	                    , 0--BHitCount
	                    , 0--BDel
                ";
                cmd.CommandText = SQL;
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@BTitle", model.BTitle);
                cmd.Parameters.AddWithValue("@BContent", model.BContent);
                cmd.Parameters.AddWithValue("@BUserName", model.BUserName);

                cmd.ExecuteNonQuery();
            }

            db.Close();
        }
        #endregion

        #region 공지사항 조회수 증가 - SetNoticeHitCountOk()
        /// <summary>
        /// 공지사항 조회수 증가
        /// <param name="idx">공지사항 PK</param>
        /// <returns></returns>
        /// </summary>
        public void SetNoticeHitCountOk(int idx)
        {
            SqlConnection db = _context.Database.GetDbConnection() as SqlConnection;
            db.Open();

            using (SqlCommand cmd = db.CreateCommand())
            {
                string SQL = @"
                    UPDATE
                        Notice
                    SET
                        BHitCount = BHitCount + 1
                    WHERE
                        Idx = @Idx
                ";
                cmd.CommandText = SQL;
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@Idx", idx);

                cmd.ExecuteNonQuery();
            }

            db.Close();
        }
        #endregion

        #region 공지사항 보기 - GetNoticeView()
        /// <summary>
        /// 공지사항에 등록된 게시글을 페이징에 맞게 가져온다.
        /// <param name="Idx">공지사항 PK</param>
        /// <returns>Idx 값에 따른 공지사항</returns>
        /// </summary>
        public DataTable GetNoticeView(int idx)
        {
            DataTable dt = new DataTable();

            SqlConnection db = _context.Database.GetDbConnection() as SqlConnection;
            db.Open();

            using (SqlCommand cmd = db.CreateCommand())
            {
                string SQL = @"
                    SELECT
	                    Idx,
	                    BTitle,
	                    BContent,
	                    BUserName,
	                    BRegDate,
	                    BHitCount
                    FROM
	                    Notice
                    WHERE
	                    Idx = @Idx
                ";
                cmd.CommandText = SQL;
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.Parameters.AddWithValue("@Idx", idx);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }

            db.Close();

            return dt;
        }
        #endregion

        #region 공지사항 글 수정 처리 - SetNoticeModifyOk()
        /// <summary>
        /// 공지사항 글 수정
        /// <param name="model">모델</param>
        /// <param name="idx">수정하고자 하는 게시글 PK</param>
        /// <returns></returns>
        /// </summary>
        public void SetNoticeModifyOk(Notice model, int idx)
        {
            SqlConnection db = _context.Database.GetDbConnection() as SqlConnection;
            db.Open();

            using (SqlCommand cmd = db.CreateCommand())
            {
                string SQL = @"
                    UPDATE
	                    Notice
                    SET
	                    BTitle = @BTitle,
	                    BContent = @BContent,
	                    BUserName = @BUserName
                    WHERE
	                    BDel = 0
	                    AND Idx = @Idx
                ";
                cmd.CommandText = SQL;
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@BTitle", model.BTitle);
                cmd.Parameters.AddWithValue("@BContent", model.BContent);
                cmd.Parameters.AddWithValue("@BUserName", model.BUserName);
                cmd.Parameters.AddWithValue("@Idx", idx);

                cmd.ExecuteNonQuery();
            }

            db.Close();
        }
        #endregion

        #region 공지사항 글 삭제 처리 - SetNoticeDeleteOk()
        /// <summary>
        /// 공지사항 글 수정
        /// <param name="model">모델</param>
        /// <param name="idx">수정하고자 하는 게시글 PK</param>
        /// <returns></returns>
        /// </summary>
        public void SetNoticeDeleteOk(int idx)
        {
            SqlConnection db = _context.Database.GetDbConnection() as SqlConnection;
            db.Open();

            using (SqlCommand cmd = db.CreateCommand())
            {
                string SQL = @"
                    UPDATE
	                    Notice
                    SET
	                    BDel = 1
                    WHERE
	                    BDel = 0
	                    AND Idx = @Idx
                ";
                cmd.CommandText = SQL;
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@Idx", idx);

                cmd.ExecuteNonQuery();
            }

            db.Close();
        }
        #endregion
    }
}
