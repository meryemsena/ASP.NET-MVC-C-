using PagedList;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace Dosya.Models
{
    public class BL
    {
        SqlHelper sql1 = new SqlHelper();

        public StaticPagedList<Rapor> RaporListBind(int page)
        {
            List<SqlParameter> prm = new List<SqlParameter>();
            prm.Add(new SqlParameter("offset", (page-1)*10)); //sayfa 1 se ilk 10, 2yse 10. veriden sonra,...işlemi yapar
            var reader= SqlHelper.ExecuteReader("select * from dbo.Rapor order by Id  OFFSET @offset ROWS FETCH NEXT 10 ROWS ONLY;", CommandType.Text, prm.ToArray());
            //offset kaçıncı satırdan sonra çalışacağını, rows fetch kaç veriyi çekeceğini ayarlar
            List<Rapor> lst = new List<Rapor>();
            while (reader.Read())
            {
                lst.Add(new Rapor
                {
                    hIslemTarih = Convert.ToDateTime(reader["hIslemTarih"]),
                    hKartNo = Convert.ToInt64(reader["hKartNo"]),
                    mTCKimlikNo = Convert.ToInt64(reader["mTCKimlikNo"]),
                    hIslemTutariYI = Convert.ToDecimal(reader["hIslemTutariYI"]),
                    hHareketTipi = Convert.ToString(reader["hHareketTipi"]),
                    iIslemAdi = Convert.ToString(reader["iIslemAdi"]),
                    hIslemAciklamasi = Convert.ToString(reader["hIslemAciklamasi"]),
                    hMerchName = Convert.ToString(reader["hMerchName"])

                });
            }

            int toplamKayit = SqlHelper.ExecuteScalar("select count(*) from Rapor"); //toplam veri sayısını çektik

            StaticPagedList<Rapor> pagedList = new StaticPagedList<Rapor>(lst, page, 10,toplamKayit);  //toplam veri sayısını da ekleyerek bu komutu her sayfaya uygulayabildik
            return pagedList;
        }
       
    }

    internal class SqlHelper
    {
        public static string connectionString = "Data Source=LAPTOP-JKEAA17E\\MSSQLSERVER03;Initial Catalog=UploadFile;Integrated Security=True;MultipleActiveResultSets=True;Application Name=EntityFramework";
      

        #region Reader

        /// Returns a datareader for the sql command
        public static SqlDataReader ExecuteReader(string cmdText, CommandType type, SqlParameter[] prms)
        {
            SqlConnection conn = new SqlConnection(connectionString);
            using (SqlCommand cmd = new SqlCommand(cmdText, conn))
            {
                cmd.CommandType = type;

                if (prms != null)
                {
                    foreach (SqlParameter p in prms)
                    {
                        cmd.Parameters.Add(p);
                    }
                }
                conn.Open();
                return cmd.ExecuteReader(CommandBehavior.CloseConnection);
            }
        }
        

        internal static DataSet  GetDataSet(object mainConnectionString, CommandType storedProcedure, string v, SqlParameter[] getBalance_Parm)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(v, con);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                con.Open();
                DataSet ds = new DataSet();
                da.Fill(ds);
                return ds;
            }

        }
        #endregion


        public static int ExecuteScalar(string cmdText) //veri sayısını çekmek için executescalar tanımladık
        {
            int toplamKayitSayisi = 0;
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(cmdText, conn))
            {
                conn.Open();
                toplamKayitSayisi = Convert.ToInt32(cmd.ExecuteScalar());
                conn.Close();
            }
            return toplamKayitSayisi;
        }
    }
}