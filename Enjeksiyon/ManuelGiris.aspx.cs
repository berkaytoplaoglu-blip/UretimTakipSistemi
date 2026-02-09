using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;
using UretimTakipSistemi.Helpers;

namespace UretimTakipSistemi.Enjeksiyon
{
    public partial class ManuelGiris : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!SessionHelper.IsLoggedIn)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                LoadDropdowns();
                txtTarih.Text = DateTime.Now.ToString("yyyy-MM-dd");
            }
        }

        private void LoadDropdowns()
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlDB"].ConnectionString))
            {
                conn.Open();

                // Personeller
                using (SqlCommand cmd = new SqlCommand("SELECT KullaniciId, AdSoyad FROM Kullanicilar WHERE Aktif=1 AND Rol='Enjeksiyon' ORDER BY AdSoyad", conn))
                {
                    SqlDataReader reader = cmd.ExecuteReader();
                    ddlPersonel.Items.Add(new ListItem("-- Personel Seçiniz --", "0"));
                    while (reader.Read())
                    {
                        ddlPersonel.Items.Add(new ListItem(reader["AdSoyad"].ToString(), reader["KullaniciId"].ToString()));
                    }
                    reader.Close();
                }

                // Makineler
                using (SqlCommand cmd = new SqlCommand("SELECT MakineId, MakineAdi FROM Makineler WHERE Aktif=1 ORDER BY MakineAdi", conn))
                {
                    SqlDataReader reader = cmd.ExecuteReader();
                    ddlMakine.Items.Add(new ListItem("-- Makine Seçiniz --", "0"));
                    while (reader.Read())
                    {
                        ddlMakine.Items.Add(new ListItem(reader["MakineAdi"].ToString(), reader["MakineId"].ToString()));
                    }
                    reader.Close();
                }

                // İş Emirleri
                using (SqlCommand cmd = new SqlCommand(@"
                    SELECT ie.IsEmriId, ie.IsEmriNo, p.UrunAdi 
                    FROM IsEmirleri ie
                    INNER JOIN UretimParcaListesi p ON ie.ParcaId = p.ParcaId
                    WHERE ie.Durum = 'ACTIVE' AND ie.Enjeksiyon = 1
                    ORDER BY ie.IsEmriNo DESC", conn))
                {
                    SqlDataReader reader = cmd.ExecuteReader();
                    ddlIsEmri.Items.Add(new ListItem("-- İş Emri Seçiniz --", "0"));
                    while (reader.Read())
                    {
                        string text = $"{reader["IsEmriNo"]} - {reader["UrunAdi"]}";
                        ddlIsEmri.Items.Add(new ListItem(text, reader["IsEmriId"].ToString()));
                    }
                    reader.Close();
                }
            }
        }

        protected void btnKaydet_Click(object sender, EventArgs e)
        {
            if (!ValidateForm())
                return;

            try
            {
                int personelId = Convert.ToInt32(ddlPersonel.SelectedValue);
                int makineId = Convert.ToInt32(ddlMakine.SelectedValue);
                int isEmriId = Convert.ToInt32(ddlIsEmri.SelectedValue);

                DateTime tarih = Convert.ToDateTime(txtTarih.Text);
                TimeSpan baslangic = TimeSpan.Parse(txtBaslangic.Text);
                TimeSpan bitis = TimeSpan.Parse(txtBitis.Text);

                DateTime baslangicZaman = tarih.Add(baslangic);
                DateTime bitisZaman = tarih.Add(bitis);

                if (bitisZaman <= baslangicZaman)
                {
                    bitisZaman = bitisZaman.AddDays(1);
                }

                int toplamSureSn = (int)(bitisZaman - baslangicZaman).TotalSeconds;
                int durusSureSn = Convert.ToInt32(txtDurusSure.Text) * 60;
                int netSureSn = toplamSureSn - durusSureSn;

                int uretimAdet = Convert.ToInt32(txtUretimAdet.Text);
                int fireAdet = Convert.ToInt32(txtFireAdet.Text);
                string fireNedeni = ddlFireNedeni.SelectedValue;

                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlDB"].ConnectionString))
                {
                    conn.Open();

                    // ÖNCE İŞ EMRİ NO'YU AL
                    string isEmriNo = "";
                    string getIsEmriNoQuery = "SELECT IsEmriNo FROM IsEmirleri WHERE IsEmriId = @IsEmriId";
                    using (SqlCommand getCmd = new SqlCommand(getIsEmriNoQuery, conn))
                    {
                        getCmd.Parameters.AddWithValue("@IsEmriId", isEmriId);
                        object result = getCmd.ExecuteScalar();
                        if (result != null)
                        {
                            isEmriNo = result.ToString();
                        }
                        else
                        {
                            ShowMessage("❌ İş emri bulunamadı!", false);
                            return;
                        }
                    }

                    // ŞIMDI INSERT YAP
                    string query = @"
                INSERT INTO EnjeksiyonOturum 
                (PersonelId, MakineId, IsEmriId, IsEmriNo, BaslangicZamani, BitisZamani, 
                 UretimAdet, FireAdet, NetUretimSuresi, ToplamDurusSuresi, Durum)
                VALUES 
                (@PersonelId, @MakineId, @IsEmriId, @IsEmriNo, @BaslangicZamani, @BitisZamani,
                 @UretimAdet, @FireAdet, @NetUretimSuresi, @ToplamDurusSuresi, 'COMPLETED');
                SELECT SCOPE_IDENTITY();";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@PersonelId", personelId);
                        cmd.Parameters.AddWithValue("@MakineId", makineId);
                        cmd.Parameters.AddWithValue("@IsEmriId", isEmriId);
                        cmd.Parameters.AddWithValue("@IsEmriNo", isEmriNo); // EKLEDIK
                        cmd.Parameters.AddWithValue("@BaslangicZamani", baslangicZaman);
                        cmd.Parameters.AddWithValue("@BitisZamani", bitisZaman);
                        cmd.Parameters.AddWithValue("@UretimAdet", uretimAdet);
                        cmd.Parameters.AddWithValue("@FireAdet", fireAdet);
                        cmd.Parameters.AddWithValue("@NetUretimSuresi", netSureSn);
                        cmd.Parameters.AddWithValue("@ToplamDurusSuresi", durusSureSn);

                        int oturumId = Convert.ToInt32(cmd.ExecuteScalar());

                        // Fire kaydı varsa ekle
                        if (fireAdet > 0 && !string.IsNullOrEmpty(fireNedeni))
                        {
                            string fireQuery = @"
                        INSERT INTO EnjeksiyonFireKayit (OturumId, FireAdet, FireNedeni, KayitTarihi)
                        VALUES (@OturumId, @FireAdet, @FireNedeni, GETDATE())";

                            using (SqlCommand fireCmd = new SqlCommand(fireQuery, conn))
                            {
                                fireCmd.Parameters.AddWithValue("@OturumId", oturumId);
                                fireCmd.Parameters.AddWithValue("@FireAdet", fireAdet);
                                fireCmd.Parameters.AddWithValue("@FireNedeni", fireNedeni);
                                fireCmd.ExecuteNonQuery();
                            }
                        }

                        ShowMessage("✅ Üretim verisi başarıyla kaydedildi!", true);
                        ClearForm();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage("❌ Hata: " + ex.Message, false);
            }
        }

        protected void btnTemizle_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private bool ValidateForm()
        {
            if (ddlPersonel.SelectedValue == "0")
            {
                ShowMessage("⚠️ Lütfen personel seçiniz!", false);
                return false;
            }
            if (ddlMakine.SelectedValue == "0")
            {
                ShowMessage("⚠️ Lütfen makine seçiniz!", false);
                return false;
            }
            if (ddlIsEmri.SelectedValue == "0")
            {
                ShowMessage("⚠️ Lütfen iş emri seçiniz!", false);
                return false;
            }
            if (string.IsNullOrEmpty(txtUretimAdet.Text) || Convert.ToInt32(txtUretimAdet.Text) <= 0)
            {
                ShowMessage("⚠️ Lütfen geçerli bir üretim adet giriniz!", false);
                return false;
            }
            return true;
        }

        private void ClearForm()
        {
            ddlPersonel.SelectedIndex = 0;
            ddlMakine.SelectedIndex = 0;
            ddlIsEmri.SelectedIndex = 0;
            txtTarih.Text = DateTime.Now.ToString("yyyy-MM-dd");
            txtBaslangic.Text = "";
            txtBitis.Text = "";
            txtUretimAdet.Text = "";
            txtFireAdet.Text = "0";
            txtDurusSure.Text = "0";
            ddlFireNedeni.SelectedIndex = 0;
        }

        private void ShowMessage(string message, bool success)
        {
            litMesaj.Text = $"<div class='alert {(success ? "alert-success" : "alert-error")}'>{message}</div>";
            pnlMesaj.Visible = true;

            ScriptManager.RegisterStartupScript(this, GetType(), "hideMsg",
                "setTimeout(function(){ document.getElementById('" + pnlMesaj.ClientID + "').style.display='none'; }, 5000);",
                true);
        }
    }
}