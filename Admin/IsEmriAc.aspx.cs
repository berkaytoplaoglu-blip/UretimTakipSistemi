using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Services;
using System.Web.UI;
using UretimTakipSistemi.Helpers;

namespace UretimTakipSistemi.Admin
{
    public partial class IsEmriAc : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!SessionHelper.IsLoggedIn || !SessionHelper.IsAdmin)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }
        }

        [WebMethod(EnableSession = true)]
        public static List<ParcaAramaModel> ParcaAra(string arama)
        {
            List<ParcaAramaModel> liste = new List<ParcaAramaModel>();

            try
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlDB"].ConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("sp_ParcaAra", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@Arama", SqlDbType.NVarChar, 100).Value = arama;

                        conn.Open();
                        SqlDataReader reader = cmd.ExecuteReader();

                        while (reader.Read())
                        {
                            liste.Add(new ParcaAramaModel
                            {
                                ParcaId = Convert.ToInt32(reader["ParcaId"]),
                                UrunParcaKodu = reader["UrunParcaKodu"].ToString(),
                                UrunAdi = reader["UrunAdi"].ToString(),
                                Grami = Convert.ToDecimal(reader["Grami"]),
                                KalipNo = reader["KalipNo"] != DBNull.Value ? reader["KalipNo"].ToString() : ""
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Parça Arama Hatası: " + ex.Message);
            }

            return liste;
        }

        protected void btnGetirParca_Click(object sender, EventArgs e)
        {
            int parcaId = Convert.ToInt32(hfParcaId.Value);

            if (parcaId > 0)
            {
                GetirParcaBilgileri(parcaId);
            }
        }

        private void GetirParcaBilgileri(int parcaId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlDB"].ConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("SELECT * FROM UretimParcaListesi WHERE ParcaId = @ParcaId", conn))
                    {
                        cmd.Parameters.Add("@ParcaId", SqlDbType.Int).Value = parcaId;

                        conn.Open();
                        SqlDataReader reader = cmd.ExecuteReader();

                        if (reader.Read())
                        {
                            litParcaKodu.Text = reader["UrunParcaKodu"].ToString();
                            litUrunAdi.Text = reader["UrunAdi"].ToString();
                            litGrami.Text = Convert.ToDecimal(reader["Grami"]).ToString("N2");
                            litKalipNo.Text = reader["KalipNo"] != DBNull.Value ? reader["KalipNo"].ToString() : "-";

                            pnlSeciliParca.Visible = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Parça bilgileri getirilemedi: " + ex.Message, false);
            }
        }

        protected void btnKaydet_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtIsEmriNo.Text))
            {
                ShowMessage("İş emri numarası girilmelidir!", false);
                return;
            }
            if (Convert.ToInt32(hfParcaId.Value) == 0)
            {
                ShowMessage("Lütfen bir ürün seçiniz!", false);
                return;
            }
            if (!chkBoyahane.Checked && !chkIcParca.Checked && !chkEnjeksiyon.Checked)
            {
                ShowMessage("En az bir bölüm seçilmelidir!", false);
                return;
            }
            int? cevrimSuresi = null;
            int? sogumaSuresi = null;
            if (chkEnjeksiyon.Checked)
            {
                if (string.IsNullOrWhiteSpace(txtCevrimSuresi.Text) ||
                    string.IsNullOrWhiteSpace(txtSogumaSuresi.Text))
                {
                    ShowMessage("Enjeksiyon seçildiğinde çevrim ve soğuma süreleri zorunludur!", false);
                    return;
                }
                int tempCevrim, tempSoguma;
                if (!int.TryParse(txtCevrimSuresi.Text, out tempCevrim) ||
                    !int.TryParse(txtSogumaSuresi.Text, out tempSoguma))
                {
                    ShowMessage("Çevrim ve soğuma süreleri sayısal değer olmalıdır!", false);
                    return;
                }
                cevrimSuresi = tempCevrim;
                sogumaSuresi = tempSoguma;
            }
            try
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlDB"].ConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("sp_IsEmriAc", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@IsEmriNo", SqlDbType.NVarChar, 50).Value = txtIsEmriNo.Text.Trim();
                        cmd.Parameters.Add("@ParcaId", SqlDbType.Int).Value = Convert.ToInt32(hfParcaId.Value);
                        cmd.Parameters.Add("@CevrimSuresi", SqlDbType.Int).Value =
                            cevrimSuresi.HasValue ? (object)cevrimSuresi.Value : DBNull.Value;
                        cmd.Parameters.Add("@SogumaSuresi", SqlDbType.Int).Value =
                            sogumaSuresi.HasValue ? (object)sogumaSuresi.Value : DBNull.Value;
                        cmd.Parameters.Add("@Boyahane", SqlDbType.Bit).Value = chkBoyahane.Checked;
                        cmd.Parameters.Add("@IcParca", SqlDbType.Bit).Value = chkIcParca.Checked;
                        cmd.Parameters.Add("@Enjeksiyon", SqlDbType.Bit).Value = chkEnjeksiyon.Checked;
                        cmd.Parameters.Add("@OlusturanKullaniciId", SqlDbType.Int).Value = SessionHelper.KullaniciId;
                        conn.Open();
                        SqlDataReader reader = cmd.ExecuteReader();
                        if (reader.Read())
                        {
                            int result = Convert.ToInt32(reader["Result"]);
                            string message = reader["Message"].ToString();

                            // ✅ YENİ EKLEME: İş emri ID'sini al
                            int yeniIsEmriId = result;

                            reader.Close();  // ✅ Reader'ı kapat

                            if (result > 0)
                            {
                                // ✅ YENİ EKLEME: Durum kolonlarını güncelle
                                string updateDurum = @"
                            UPDATE IsEmirleri 
                            SET BoyahaneDurum = @BoyahaneDurum,
                                IcParcaDurum = @IcParcaDurum
                            WHERE IsEmriId = @IsEmriId";

                                using (SqlCommand cmdDurum = new SqlCommand(updateDurum, conn))
                                {
                                    cmdDurum.Parameters.AddWithValue("@IsEmriId", yeniIsEmriId);
                                    cmdDurum.Parameters.AddWithValue("@BoyahaneDurum",
                                        chkBoyahane.Checked ? "YENİ" : "-");
                                    cmdDurum.Parameters.AddWithValue("@IcParcaDurum",
                                        chkIcParca.Checked ? "YENİ" : "-");
                                    cmdDurum.ExecuteNonQuery();
                                }

                                ShowMessage(message, true);
                                ClearForm();
                            }
                            else
                            {
                                ShowMessage(message, false);
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

        private void ClearForm()
        {
            txtIsEmriNo.Text = "";
            txtUrunAra.Text = "";
            hfParcaId.Value = "0";
            pnlSeciliParca.Visible = false;
            chkBoyahane.Checked = false;
            chkIcParca.Checked = false;
            chkEnjeksiyon.Checked = false;
            txtCevrimSuresi.Text = "";
            txtSogumaSuresi.Text = "";
        }

        private void ShowMessage(string message, bool success)
        {
            litMesaj.Text = $"<div class='alert {(success ? "alert-success" : "alert-error")}'>{message}</div>";
            ScriptManager.RegisterStartupScript(this, GetType(), "showMsg",
                $"document.getElementById('{pnlMesaj.ClientID}').style.display='block'; setTimeout(function(){{document.getElementById('{pnlMesaj.ClientID}').style.display='none';}}, 5000);",
                true);
        }
    }

    public class ParcaAramaModel
    {
        public int ParcaId { get; set; }
        public string UrunParcaKodu { get; set; }
        public string UrunAdi { get; set; }
        public decimal Grami { get; set; }
        public string KalipNo { get; set; }
    }
}