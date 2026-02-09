using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI; // SADECE BİR KEZ
using System.Web.UI.WebControls;
using UretimTakipSistemi.Helpers;

namespace UretimTakipSistemi.Boyahane
{
    public partial class BoyahaneUretim : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!SessionHelper.IsLoggedIn)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!SessionHelper.IsAdmin && !SessionHelper.IsBoyahane)
            {
                Response.Redirect("~/Default.aspx");
                return;
            }

            if (!IsPostBack)
            {
                txtTarih.Text = DateTime.Now.ToString("yyyy-MM-dd");
                LoadDropdowns();
                LoadKayitlar();
            }
        }

        private void LoadDropdowns()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlDB"].ConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("sp_PersonelListesi", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        conn.Open();
                        SqlDataReader reader = cmd.ExecuteReader();

                        ddlPersonel.Items.Clear();
                        ddlPersonel.Items.Add(new ListItem("-- Seçiniz --", "0"));

                        while (reader.Read())
                        {
                            ddlPersonel.Items.Add(new ListItem(
                                reader["AdSoyad"].ToString(),
                                reader["KullaniciId"].ToString()
                            ));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Personel yükleme hatası: " + ex.Message);
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlDB"].ConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("sp_MakineListesi", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@Bolum", SqlDbType.NVarChar, 20).Value = "Boyahane";

                        conn.Open();
                        SqlDataReader reader = cmd.ExecuteReader();

                        ddlMakine.Items.Clear();
                        ddlMakine.Items.Add(new ListItem("-- Seçiniz --", "0"));

                        while (reader.Read())
                        {
                            ddlMakine.Items.Add(new ListItem(
                                reader["MakineAdi"].ToString(),
                                reader["MakineId"].ToString()
                            ));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Makine yükleme hatası: " + ex.Message);
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlDB"].ConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("sp_AktifIsEmirleri", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@Bolum", SqlDbType.NVarChar, 20).Value = "Boyahane";

                        conn.Open();
                        SqlDataReader reader = cmd.ExecuteReader();

                        ddlIsEmri.Items.Clear();
                        ddlIsEmri.Items.Add(new ListItem("-- Seçiniz --", "0"));

                        while (reader.Read())
                        {
                            ddlIsEmri.Items.Add(new ListItem(
                                reader["IsEmriNo"].ToString() + " - " + reader["UrunAdi"].ToString(),
                                reader["IsEmriId"].ToString()
                            ));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("İş emri yükleme hatası: " + ex.Message);
            }
        }

        private void LoadKayitlar()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlDB"].ConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("sp_BoyahaneSonKayitlar", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        gvKayitlar.DataSource = dt;
                        gvKayitlar.DataBind();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Kayıtlar yükleme hatası: " + ex.Message);
            }
        }

        protected void btnKaydet_Click(object sender, EventArgs e)
        {
            if (!ValidateForm())
                return;

            try
            {
                DateTime tarih = DateTime.Parse(txtTarih.Text);
                int personelId = Convert.ToInt32(ddlPersonel.SelectedValue);
                int makineId = Convert.ToInt32(ddlMakine.SelectedValue);
                int isEmriId = Convert.ToInt32(ddlIsEmri.SelectedValue);
                int uretimAdet = Convert.ToInt32(txtUretimAdet.Text);
                int fireAdet = Convert.ToInt32(txtFireAdet.Text);

                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlDB"].ConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("sp_BoyahaneKaydet", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("@Tarih", SqlDbType.Date).Value = tarih;
                        cmd.Parameters.Add("@Vardiya", SqlDbType.NVarChar, 20).Value = ddlVardiya.SelectedValue;
                        cmd.Parameters.Add("@PersonelId", SqlDbType.Int).Value = personelId;
                        cmd.Parameters.Add("@MakineId", SqlDbType.Int).Value = makineId;
                        cmd.Parameters.Add("@IsEmriId", SqlDbType.Int).Value = isEmriId;
                        cmd.Parameters.Add("@UretimAdet", SqlDbType.Int).Value = uretimAdet;
                        cmd.Parameters.Add("@FireAdet", SqlDbType.Int).Value = fireAdet;
                        cmd.Parameters.Add("@Aciklama", SqlDbType.NVarChar, 500).Value =
                            string.IsNullOrWhiteSpace(txtAciklama.Text) ? (object)DBNull.Value : txtAciklama.Text.Trim();
                        cmd.Parameters.Add("@KaydedenKullaniciId", SqlDbType.Int).Value = SessionHelper.KullaniciId;

                        conn.Open();
                        SqlDataReader reader = cmd.ExecuteReader();

                        if (reader.Read())
                        {
                            int result = Convert.ToInt32(reader["Result"]);
                            string message = reader["Message"].ToString();

                            ShowMessage(message, result > 0);

                            if (result > 0)
                            {
                                ClearForm();
                                LoadKayitlar();
                            }
                        }
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
            if (!ValidateForm())
                return;

            try
            {
                int kayitId = Convert.ToInt32(hfKayitId.Value);
                DateTime tarih = DateTime.Parse(txtTarih.Text);
                int personelId = Convert.ToInt32(ddlPersonel.SelectedValue);
                int makineId = Convert.ToInt32(ddlMakine.SelectedValue);
                int isEmriId = Convert.ToInt32(ddlIsEmri.SelectedValue);
                int uretimAdet = Convert.ToInt32(txtUretimAdet.Text);
                int fireAdet = Convert.ToInt32(txtFireAdet.Text);

                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlDB"].ConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("sp_BoyahaneKayitGuncelle", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("@KayitId", SqlDbType.Int).Value = kayitId;
                        cmd.Parameters.Add("@Tarih", SqlDbType.Date).Value = tarih;
                        cmd.Parameters.Add("@Vardiya", SqlDbType.NVarChar, 20).Value = ddlVardiya.SelectedValue;
                        cmd.Parameters.Add("@PersonelId", SqlDbType.Int).Value = personelId;
                        cmd.Parameters.Add("@MakineId", SqlDbType.Int).Value = makineId;
                        cmd.Parameters.Add("@IsEmriId", SqlDbType.Int).Value = isEmriId;
                        cmd.Parameters.Add("@UretimAdet", SqlDbType.Int).Value = uretimAdet;
                        cmd.Parameters.Add("@FireAdet", SqlDbType.Int).Value = fireAdet;
                        cmd.Parameters.Add("@Aciklama", SqlDbType.NVarChar, 500).Value =
                            string.IsNullOrWhiteSpace(txtAciklama.Text) ? (object)DBNull.Value : txtAciklama.Text.Trim();

                        conn.Open();
                        SqlDataReader reader = cmd.ExecuteReader();

                        if (reader.Read())
                        {
                            int result = Convert.ToInt32(reader["Result"]);
                            string message = reader["Message"].ToString();

                            ShowMessage(message, result > 0);

                            if (result > 0)
                            {
                                ClearForm();
                                LoadKayitlar();
                            }
                        }
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

        protected void gvKayitlar_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "GetirKayit")
            {
                int kayitId = Convert.ToInt32(e.CommandArgument);
                GetirKayit(kayitId);
            }
        }

        private void GetirKayit(int kayitId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlDB"].ConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("sp_BoyahaneKayitGetir", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@KayitId", SqlDbType.Int).Value = kayitId;

                        conn.Open();
                        SqlDataReader reader = cmd.ExecuteReader();

                        if (reader.Read())
                        {
                            hfKayitId.Value = kayitId.ToString();
                            txtTarih.Text = Convert.ToDateTime(reader["Tarih"]).ToString("yyyy-MM-dd");
                            ddlVardiya.SelectedValue = reader["Vardiya"].ToString();
                            ddlPersonel.SelectedValue = reader["PersonelId"].ToString();
                            ddlMakine.SelectedValue = reader["MakineId"].ToString();
                            ddlIsEmri.SelectedValue = reader["IsEmriId"].ToString();
                            txtUretimAdet.Text = reader["UretimAdet"].ToString();
                            txtFireAdet.Text = reader["FireAdet"].ToString();
                            txtAciklama.Text = reader["Aciklama"] != DBNull.Value ? reader["Aciklama"].ToString() : "";

                            btnKaydet.Visible = false;
                            btnGuncelle.Visible = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Kayıt getirme hatası: " + ex.Message, false);
            }
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(txtTarih.Text))
            {
                ShowMessage("Tarih seçilmelidir!", false);
                return false;
            }

            if (ddlPersonel.SelectedValue == "0")
            {
                ShowMessage("Personel seçilmelidir!", false);
                return false;
            }

            if (ddlMakine.SelectedValue == "0")
            {
                ShowMessage("Makine seçilmelidir!", false);
                return false;
            }

            if (ddlIsEmri.SelectedValue == "0")
            {
                ShowMessage("İş emri seçilmelidir!", false);
                return false;
            }

            int uretimAdet;
            if (!int.TryParse(txtUretimAdet.Text, out uretimAdet) || uretimAdet <= 0)
            {
                ShowMessage("Geçerli bir üretim adet giriniz!", false);
                return false;
            }

            int fireAdet;
            if (!int.TryParse(txtFireAdet.Text, out fireAdet) || fireAdet < 0)
            {
                ShowMessage("Geçerli bir fire adet giriniz!", false);
                return false;
            }

            return true;
        }

        private void ClearForm()
        {
            hfKayitId.Value = "0";
            txtTarih.Text = DateTime.Now.ToString("yyyy-MM-dd");
            ddlVardiya.SelectedIndex = 0;
            ddlPersonel.SelectedIndex = 0;
            ddlMakine.SelectedIndex = 0;
            ddlIsEmri.SelectedIndex = 0;
            txtUretimAdet.Text = "";
            txtFireAdet.Text = "0";
            txtAciklama.Text = "";

            btnKaydet.Visible = true;
            btnGuncelle.Visible = false;
        }

        private void ShowMessage(string message, bool success)
        {
            litMesaj.Text = $"<div class='alert {(success ? "alert-success" : "alert-error")}'>{message}</div>";
            ScriptManager.RegisterStartupScript(this, GetType(), "showMsg",
                $"document.getElementById('{pnlMesaj.ClientID}').style.display='block'; setTimeout(function(){{document.getElementById('{pnlMesaj.ClientID}').style.display='none';}}, 5000);",
                true);
        }
    }
}