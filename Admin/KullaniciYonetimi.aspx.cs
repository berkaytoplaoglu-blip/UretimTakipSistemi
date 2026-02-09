using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;
using UretimTakipSistemi.Helpers;

namespace UretimTakipSistemi.Admin
{
    public partial class KullaniciYonetimi : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!SessionHelper.IsLoggedIn || !SessionHelper.IsAdmin)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                LoadKullanicilar();
            }
        }

        private void LoadKullanicilar()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlDB"].ConnectionString))
                {
                    string query = "SELECT KullaniciId, KullaniciAdi, AdSoyad, Rol, Aktif, KayitTarihi FROM Kullanicilar ORDER BY KayitTarihi DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        gvKullanicilar.DataSource = dt;
                        gvKullanicilar.DataBind();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Hata: " + ex.Message, false);
            }
        }

        protected void btnKaydet_Click(object sender, EventArgs e)
        {
            if (!ValidateForm()) return;

            try
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlDB"].ConnectionString))
                {
                    string query = "INSERT INTO Kullanicilar (KullaniciAdi, Sifre, AdSoyad, Rol, Aktif) VALUES (@KullaniciAdi, @Sifre, @AdSoyad, @Rol, 1)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@KullaniciAdi", txtKullaniciAdi.Text.Trim());
                        cmd.Parameters.AddWithValue("@Sifre", txtSifre.Text.Trim());
                        cmd.Parameters.AddWithValue("@AdSoyad", txtAdSoyad.Text.Trim());
                        cmd.Parameters.AddWithValue("@Rol", ddlRol.SelectedValue);

                        conn.Open();
                        cmd.ExecuteNonQuery();

                        ShowMessage("Kullanıcı eklendi.", true);
                        ClearForm();
                        LoadKullanicilar();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Hata: " + ex.Message, false);
            }
        }

        protected void btnGuncelle_Click(object sender, EventArgs e)
        {
            if (!ValidateForm()) return;

            try
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlDB"].ConnectionString))
                {
                    string query = "UPDATE Kullanicilar SET KullaniciAdi=@KullaniciAdi, AdSoyad=@AdSoyad, Rol=@Rol";
                    if (!string.IsNullOrEmpty(txtSifre.Text.Trim())) query += ", Sifre=@Sifre";
                    query += " WHERE KullaniciId=@KullaniciId";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@KullaniciId", Convert.ToInt32(hfKullaniciId.Value));
                        cmd.Parameters.AddWithValue("@KullaniciAdi", txtKullaniciAdi.Text.Trim());
                        cmd.Parameters.AddWithValue("@AdSoyad", txtAdSoyad.Text.Trim());
                        cmd.Parameters.AddWithValue("@Rol", ddlRol.SelectedValue);
                        if (!string.IsNullOrEmpty(txtSifre.Text.Trim()))
                            cmd.Parameters.AddWithValue("@Sifre", txtSifre.Text.Trim());

                        conn.Open();
                        cmd.ExecuteNonQuery();

                        ShowMessage("Kullanıcı güncellendi.", true);
                        ClearForm();
                        LoadKullanicilar();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Hata: " + ex.Message, false);
            }
        }

        protected void btnTemizle_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        protected void gvKullanicilar_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int kullaniciId = Convert.ToInt32(e.CommandArgument);
            if (e.CommandName == "Duzenle") LoadKullanici(kullaniciId);
            else if (e.CommandName == "Sil") DeleteKullanici(kullaniciId);
        }

        private void LoadKullanici(int kullaniciId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlDB"].ConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("SELECT * FROM Kullanicilar WHERE KullaniciId=@KullaniciId", conn))
                    {
                        cmd.Parameters.AddWithValue("@KullaniciId", kullaniciId);
                        conn.Open();
                        SqlDataReader reader = cmd.ExecuteReader();
                        if (reader.Read())
                        {
                            hfKullaniciId.Value = kullaniciId.ToString();
                            txtAdSoyad.Text = reader["AdSoyad"].ToString();
                            txtKullaniciAdi.Text = reader["KullaniciAdi"].ToString();
                            txtSifre.Text = "";
                            ddlRol.SelectedValue = reader["Rol"].ToString();
                            btnKaydet.Visible = false;
                            btnGuncelle.Visible = true;
                        }
                    }
                }
            }
            catch { }
        }

        private void DeleteKullanici(int kullaniciId)
        {
            if (kullaniciId == SessionHelper.KullaniciId)
            {
                ShowMessage("Kendi hesabınızı silemezsiniz!", false);
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlDB"].ConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("UPDATE Kullanicilar SET Aktif=0 WHERE KullaniciId=@KullaniciId", conn))
                    {
                        cmd.Parameters.AddWithValue("@KullaniciId", kullaniciId);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                        ShowMessage("Kullanıcı silindi.", true);
                        LoadKullanicilar();
                    }
                }
            }
            catch { }
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(txtAdSoyad.Text))
            {
                ShowMessage("Ad Soyad boş!", false);
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtKullaniciAdi.Text))
            {
                ShowMessage("Kullanıcı adı boş!", false);
                return false;
            }
            if (hfKullaniciId.Value == "0" && string.IsNullOrWhiteSpace(txtSifre.Text))
            {
                ShowMessage("Şifre boş!", false);
                return false;
            }
            return true;
        }

        private void ClearForm()
        {
            hfKullaniciId.Value = "0";
            txtAdSoyad.Text = "";
            txtKullaniciAdi.Text = "";
            txtSifre.Text = "";
            ddlRol.SelectedIndex = 0;
            btnKaydet.Visible = true;
            btnGuncelle.Visible = false;
        }

        private void ShowMessage(string message, bool success)
        {
            litMesaj.Text = "<div class='alert " + (success ? "alert-success" : "alert-error") + "'>" + message + "</div>";
            ScriptManager.RegisterStartupScript(this, GetType(), "msg", "document.getElementById('" + pnlMesaj.ClientID + "').style.display='block';setTimeout(function(){document.getElementById('" + pnlMesaj.ClientID + "').style.display='none';},5000);", true);
        }

        public string GetRolBadge(object rolObj)
        {
            string rol = rolObj.ToString();
            switch (rol)
            {
                case "Admin": return "<span class='badge badge-admin'>👑 Admin</span>";
                case "Boyahane": return "<span class='badge badge-boyahane'>🎨 Boyahane</span>";
                case "IcParca": return "<span class='badge badge-icparca'>🔧 İç Parça</span>";
                case "Enjeksiyon": return "<span class='badge badge-enjeksiyon'>⚙️ Enjeksiyon</span>";
                default: return rol;
            }
        }
    }
}