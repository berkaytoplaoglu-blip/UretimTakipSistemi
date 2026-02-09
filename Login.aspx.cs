using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using UretimTakipSistemi.Helpers;

namespace UretimTakipSistemi
{
    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (SessionHelper.IsLoggedIn)
                {
                    Response.Redirect("~/Default.aspx");
                }
            }
        }

        protected void btnGiris_Click(object sender, EventArgs e)
        {
            pnlHata.CssClass = "error-message";

            string kullaniciAdi = txtKullaniciAdi.Text.Trim();
            string sifre = txtSifre.Text.Trim();

            if (string.IsNullOrEmpty(kullaniciAdi) || string.IsNullOrEmpty(sifre))
            {
                ShowError("Kullanıcı adı ve şifre boş bırakılamaz!");
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlDB"].ConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("sp_KullaniciGiris", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@KullaniciAdi", kullaniciAdi);
                        cmd.Parameters.AddWithValue("@Sifre", sifre);

                        conn.Open();
                        SqlDataReader reader = cmd.ExecuteReader();

                        if (reader.Read())
                        {
                            SessionHelper.SetUser(
                                Convert.ToInt32(reader["KullaniciId"]),
                                reader["KullaniciAdi"].ToString(),
                                reader["AdSoyad"].ToString(),
                                reader["Rol"].ToString()
                            );

                            Response.Redirect("~/Default.aspx");
                        }
                        else
                        {
                            ShowError("Kullanıcı adı veya şifre hatalı!");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError("Bir hata oluştu. Lütfen tekrar deneyin.");
                System.Diagnostics.Debug.WriteLine("Login Hatası: " + ex.Message);
            }
        }

        private void ShowError(string message)
        {
            litHata.Text = message;
            pnlHata.CssClass = "error-message show";
        }
    }
}