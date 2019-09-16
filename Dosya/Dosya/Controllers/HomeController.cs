using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Dosya.Models;

namespace Dosya.Controllers
{
    public class HomeController : Controller
    {

        SqlConnection cn = new SqlConnection("Data Source=LAPTOP-JKEAA17E\\MSSQLSERVER03;Initial Catalog=UploadFile;Integrated Security=True;MultipleActiveResultSets=True;Application Name=EntityFramework");
        //string icinde özel karakter kullanımı
        // GET: Home
        [HttpGet]
        public ActionResult Index()
        {
            DataTable Kullanicilar = new DataTable();
            cn.Open();
            SqlDataAdapter sqlData = new SqlDataAdapter("SELECT * FROM Users", cn);
            sqlData.Fill(Kullanicilar);
            return View(Kullanicilar);
        }
        [HttpPost]
        public ActionResult Index(FormCollection form)
        {
            try
            {
                string Ad = form["ad"];
                string Soyad = form["soyad"];
                int Yas = Convert.ToInt32(form["yas"]);
                DateTime Tarih = Convert.ToDateTime(form["tarih"]);
                SqlCommand cmm = new SqlCommand();
                cmm.Connection = cn;
                cmm.CommandText = $"INSERT INTO Users(Ad, Soyad, Yas, Tarih) VALUES('{Ad}', '{Soyad}', {Yas}, @tarih)";
                cmm.Parameters.AddWithValue("tarih", Tarih); //tarih gun ay yıl seklinde olması icin 
                cn.Open();
                cmm.ExecuteNonQuery();
                ViewBag.Mesaj = "Kayıt Eklendi.";
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                cn.Close();
            }

            DataTable Kullanicilar = new DataTable();
            cn.Open();
            SqlDataAdapter sqlData = new SqlDataAdapter("SELECT * FROM Users", cn);
            sqlData.Fill(Kullanicilar);
            return View(Kullanicilar);
        }
        //get 
        public ActionResult Edit(int Id)
        {
            Users users = new Users();
            DataTable Kullanicilar = new DataTable();
            cn.Open();
            string query = "SELECT *FROM Users Where Id=@Id";
            SqlDataAdapter sqlData = new SqlDataAdapter(query, cn);
            sqlData.SelectCommand.Parameters.AddWithValue("@Id", Id);
            sqlData.Fill(Kullanicilar);
            if (Kullanicilar.Rows.Count == 1)
            {
                users.Id = Convert.ToInt32(Kullanicilar.Rows[0][0].ToString());
                users.Ad = Kullanicilar.Rows[0]["Ad"].ToString();
                users.Soyad = Kullanicilar.Rows[0]["Soyad"].ToString();
                users.Yas = Convert.ToInt32(Kullanicilar.Rows[0]["Yas"].ToString());
                users.Tarih = Convert.ToDateTime(Kullanicilar.Rows[0]["Tarih"].ToString());

                return View(users);
            }
            else
                return RedirectToAction("Index");
        }
        [HttpPost]
        public ActionResult Edit(Users users)
        {
            try
            {
                SqlCommand cmm = new SqlCommand();
                cmm.Connection = cn;
                cmm.CommandText = "UPDATE Users SET Ad=@Ad , Soyad=@Soyad , Yas=@Yas , Tarih=@Tarih Where Id=@Id";
                cmm.Parameters.AddWithValue("@Id", users.Id);
                cmm.Parameters.AddWithValue("@Ad", users.Ad);
                cmm.Parameters.AddWithValue("@Soyad", users.Soyad);
                cmm.Parameters.AddWithValue("@Yas", users.Yas);
                cmm.Parameters.AddWithValue("@Tarih", users.Tarih);  //tarih gun ay yıl seklinde olması icin 
                cn.Open();
                cmm.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                cn.Close();
            }

            DataTable Kullanicilar = new DataTable();
            cn.Open();
            SqlDataAdapter sqlData = new SqlDataAdapter("SELECT * FROM Users", cn);
            sqlData.Fill(Kullanicilar);
            return RedirectToAction("Index");
        }
        //get delete
        public ActionResult Delete(int id)
        {
            try
            {
                SqlCommand cmm = new SqlCommand();
                cmm.Connection = cn;
                cmm.CommandText = "DELETE FROM Users Where Id=@Id";
                cmm.Parameters.AddWithValue("@Id", id);
                cn.Open();
                cmm.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                cn.Close();
            }

            DataTable Kullanicilar = new DataTable();
            cn.Open();
            SqlDataAdapter sqlData = new SqlDataAdapter("SELECT * FROM Users", cn);
            sqlData.Fill(Kullanicilar);
            return RedirectToAction("Index");
        }
    }
}

