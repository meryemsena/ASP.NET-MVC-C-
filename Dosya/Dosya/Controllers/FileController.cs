using Dosya.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.Mvc;
using System.Linq;
using PagedList;
using PagedList.Mvc;
using System.Web.Script.Serialization;

namespace Dosya.Controllers
{
    public class FileController : Controller
    {
        SqlConnection cn = new SqlConnection("Data Source=LAPTOP-JKEAA17E\\MSSQLSERVER03;Initial Catalog=UploadFile;Integrated Security=True;MultipleActiveResultSets=True;Application Name=EntityFramework");
        SqlCommand cmm = new SqlCommand();
        // GET: File
        public ActionResult Index()
        {
            return View();
        }
        private bool isValidContentType(string contentType)
        {
            return contentType.Equals("text/enriched")|| contentType.Equals("text/html")|| contentType.Equals("text/plain")|| contentType.Equals("text/rfc822-headers")|| contentType.Equals("text/richtext")|| contentType.Equals("text/sgml");
        }
        [HttpPost]
        public ActionResult Process(HttpPostedFileBase txt)
        {
            try //try catch ile kullanıcıya hatayı düzgün gösterebiliriz
            {
                if (!isValidContentType(txt.ContentType))
                {
                    ViewBag.Error = "Sadece .txt uzantılı dosya yükleyebilirsiniz.";
                    return View("Index");
                }

                List<string> data = new List<string>();
                using (System.IO.StreamReader reader = new System.IO.StreamReader(txt.InputStream))
                {
                    while (!reader.EndOfStream)
                    {
                        data.Add(reader.ReadLine());
                    }
                }


                DateTime dosyaTarih = DateTime.Parse(String.Format("{0:u}", data[0].Substring(103, 19)));
                int dosyaId = DosyaTarihKaydet(dosyaTarih);

                cmm.Connection = cn;
                cn.Open();
                for (int i = 4; i <= data.Count - 4; i++)
                {
                    Rapor r = new Rapor();
                    string row = data[i];
                    DateTime tarih = DateTime.Parse(String.Format("{0:u}", row.Substring(0, 23)));// "2008-03-09 16:05:07Z"              UniversalSortableDateTime

                    long hKartNo = long.Parse(row.Substring(55, 16));
                    long mTCKimlikNo = long.Parse(row.Substring(76, 11));
                    decimal hIslemTutariYI = Convert.ToDecimal(row.Substring(92, 7).Replace('.', ',')); //verilerde hassasiyet önemliyse float kullanılmamalı, veri tabanına atarken virgülden sonra yuvarlama yapabilir
                   // string tutar = hIslemTutariYI.ToString();
                    string hHareketTipi = row.Substring(114, 1);
                    string iIslemAdi = row.Substring(127, 5);
                    string hIslemAciklamasi = row.Substring(168, 39);
                    string hMerchName = row.Substring(209, 20);

                    cmm.CommandText = "INSERT INTO Rapor(hIslemTarih, hKartNo, mTCKimlikNo, hIslemTutariYI, hHareketTipi, iIslemAdi, hIslemAciklamasi, hMerchName, DosyaId) " +
                                    $"VALUES( @tarih, @hKartNo, @mTCKimlikNo, @tutar, @hHareketTipi, @iIslemAdi, @hIslemAciklamasi, @hMerchName, @dosyaId )";
                    cmm.Parameters.Clear();

                    cmm.Parameters.AddWithValue("tarih", tarih); //parametre tanımlama sql injection saldırılarını önler
                    cmm.Parameters.AddWithValue("hKartNo", hKartNo);
                    cmm.Parameters.AddWithValue("mTCKimlikNo", mTCKimlikNo);
                    cmm.Parameters.AddWithValue("tutar", hIslemTutariYI);
                    cmm.Parameters.AddWithValue("hHareketTipi", hHareketTipi);
                    cmm.Parameters.AddWithValue("iIslemAdi", iIslemAdi);
                    cmm.Parameters.AddWithValue("hIslemAciklamasi", hIslemAciklamasi);
                    cmm.Parameters.AddWithValue("hMerchName", hMerchName);
                    cmm.Parameters.AddWithValue("dosyaId", dosyaId);
                    cmm.ExecuteNonQuery();
                }
                ViewBag.Mesaj = "dosyanız yüklendi.";

                cn.Close();
            }
            catch (Exception ex)
            {
                ViewBag.Hata = ex.Message;
            }

            return View("Index");

        }

        private int DosyaTarihKaydet(DateTime tarih) //parametre dosyanın tarihi
        {
            cmm.Connection = cn;
            cn.Open();
            cmm.Parameters.AddWithValue("DosyaTarih", tarih);

            cmm.CommandText = "select count(*) from Dosya where DosyaTarih=@DosyaTarih "; //gelen tarihle aynı tarihteki kayıtları çeker
            int adet = (int)cmm.ExecuteScalar(); //eğer gelen tarihle aynı kayıt varsa adet 1, yoksa 0 olacak
            if (adet > 0)
            {
                cn.Close();
                throw new Exception("Bu Dosya Zaten Eklenmiş");
            }

            cmm.CommandText = "INSERT INTO Dosya(DosyaTarih)  output INSERTED.Id " + //gelen tarihin atanan id sini çektik
                                $"VALUES( @DosyaTarih )";

            int id = (int)cmm.ExecuteScalar(); //ilk satırda ilk column u döndürür, id i çekebiliriz böylece

            cn.Close();

            return id; //bu fonk id i döndürecek
        }
        // GET: File
        [HttpGet]
        public ActionResult Hareketler(int page=1) //sayfa 1 den başlayacak
        {

            BL businessLogic = new BL(); //bl class dan nesne olusturuldu
           var objRaporList = businessLogic.RaporListBind(page);
            return View(objRaporList);
        }
        // GET: Home
        [HttpGet]
        public ActionResult Harcamalar()
        {
            DataTable Harcamalar = new DataTable();
            cn.Open();
            SqlDataAdapter sqlData = new SqlDataAdapter("SELECT mTCKimlikNo, SUM(hIslemTutariYI) as mHarcama FROM Rapor GROUP BY mTCKimlikNo order by mHarcama desc; ", cn);
            sqlData.Fill(Harcamalar);
            return View(Harcamalar);
        }
        // GET: File
        public ActionResult Kurum()
        {
            cn.Open();
            DataTable Kurum = new DataTable();
            SqlDataAdapter sqlData = new SqlDataAdapter("SELECT top 5 hMerchName, SUM(hIslemTutariYI) as harcama FROM Rapor GROUP BY hMerchName order by harcama desc;", cn);            
            sqlData.Fill(Kurum);
            cn.Close();
            //listenin türünü ilk başta belirtmedik.      select data expression. m yerine istediğimiz bir ad yazabiliriz
            var list = Kurum.AsEnumerable().Select(m => new { name = m.Field<string>("hMerchName"), y = m.Field<double>("harcama") }).ToList(); 
            //js kodu json kullanarak istenilen türe çevirdik. name ve y ile aldığımız satırdan merch ve harcamayı çektik
            var jsonSerialiser = new JavaScriptSerializer();
            var json = jsonSerialiser.Serialize(list); //listeyi json olarak tanımladık.

            ViewBag.ChartData = json; //js kodunda viewbag tanımladık verileri gönderdik

            return View();
        }
        // GET: File
        public ActionResult Gunluk()
        {
            cn.Open();
            DataTable Gunluk = new DataTable();
            SqlDataAdapter sqlData = new SqlDataAdapter("SELECT hIslemTarih, SUM(hIslemTutariYI) as harcama FROM Rapor GROUP BY hIslemTarih ORDER BY harcama DESC;", cn);
            sqlData.Fill(Gunluk);
            cn.Close();
            //listenin türünü ilk başta belirtmedik.      select data expression. m yerine istediğimiz bir ad yazabiliriz
            var list = Gunluk.AsEnumerable().Select(m => new {y = m.Field<double>("harcama") }).ToList();
           // var listTarih = Gunluk.AsEnumerable().Select(m => new { x = m.Field<DateTime>("hIslemTarih") }).ToList();
            //js kodu json kullanarak istenilen türe çevirdik. name ve y ile aldığımız satırdan harcamayı çektik
            var jsonSerialiser = new JavaScriptSerializer();
            var json = jsonSerialiser.Serialize(list); //listeyi json olarak tanımladık.
          //  var jsonTarih = jsonSerialiser.Serialize(listTarih);
            ViewBag.GunlukChart = json; //js kodunda viewbag tanımladık verileri gönderdik
         //   ViewBag.HarcamaChart = jsonTarih;
            return View();
        }
    }
    


}