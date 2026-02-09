using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;
using UretimTakipSistemi.Helpers;

namespace UretimTakipSistemi.IcParca
{
    public partial class IcParcaUretim : Page
    {
        private string CS => ConfigurationManager.ConnectionStrings["SqlDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!SessionHelper.IsLoggedIn)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!SessionHelper.IsAdmin && !SessionHelper.IsIcParca)
            {
                Response.Redirect("~/Default.aspx");
                return;
            }

            if (!IsPostBack)
            {
                txtTarih.Text = DateTime.Now.ToString("yyyy-MM-dd");
                ddlVardiya.SelectedValue = "Gunduz";
                SetVardiyaSaatleri();

                LoadDropdowns();
                AutoSelectPersonel();

                txtDuraksamaSaat.Text = "0";
                txtNetCalismaSaat.Text = "";

                LoadKayitlar();
                pnlIsEmriInfo.Visible = false;
            }
        }

        private void LoadDropdowns()
        {
            LoadPersonel();
            LoadHat();
            LoadIsEmri();
        }

        private void LoadPersonel()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(CS))
                using (SqlCommand cmd = new SqlCommand("sp_PersonelListesi", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
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
        }

        private void LoadHat()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(CS))
                using (SqlCommand cmd = new SqlCommand("sp_MakineListesi", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Bolum", SqlDbType.NVarChar, 20).Value = "IcParca";

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        ddlHat.Items.Clear();
                        ddlHat.Items.Add(new ListItem("-- Seçiniz --", "0"));

                        while (reader.Read())
                        {
                            ddlHat.Items.Add(new ListItem(
                                reader["MakineAdi"].ToString(),
                                reader["MakineId"].ToString()
                            ));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Hat yükleme hatası: " + ex.Message);
            }
        }

        private void LoadIsEmri()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(CS))
                using (SqlCommand cmd = new SqlCommand("sp_AktifIsEmirleri", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Bolum", SqlDbType.NVarChar, 20).Value = "IcParca";

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
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

        private void AutoSelectPersonel()
        {
            try
            {
                string uid = SessionHelper.KullaniciId.ToString();
                var item = ddlPersonel.Items.FindByValue(uid);
                if (item != null)
                {
                    ddlPersonel.ClearSelection();
                    item.Selected = true;
                }
            }
            catch { }
        }

        protected void ddlVardiya_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetVardiyaSaatleri();
        }

        private void SetVardiyaSaatleri()
        {
            if (ddlVardiya.SelectedValue == "Gece")
            {
                txtBasSaat.Text = "19:30";
                txtBitSaat.Text = "07:30";
            }
            else
            {
                txtBasSaat.Text = "07:30";
                txtBitSaat.Text = "17:30";
            }
        }

        protected void btnBasSimdi_Click(object sender, EventArgs e)
        {
            txtBasSaat.Text = DateTime.Now.ToString("HH:mm");
        }

        protected void btnBitSimdi_Click(object sender, EventArgs e)
        {
            txtBitSaat.Text = DateTime.Now.ToString("HH:mm");
        }

        protected void ddlIsEmri_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!int.TryParse(ddlIsEmri.SelectedValue, out int isEmriId) || isEmriId <= 0)
            {
                pnlIsEmriInfo.Visible = false;
                return;
            }

            LoadIsEmriInfo(isEmriId);
        }

        private void LoadIsEmriInfo(int isEmriId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(CS))
                using (SqlCommand cmd = new SqlCommand(@"
SELECT
    IsEmriId, IsEmriNo, UrunAdi, UrunParcaKodu, Grami, KalipNo,
    ISNULL(NULLIF(BoyahaneDurum,''),'YENİ') AS BoyahaneDurum,
    ISNULL(NULLIF(IcParcaDurum,''),'YENİ') AS IcParcaDurum
FROM dbo.IsEmirleri
WHERE IsEmriId=@id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", isEmriId);
                    conn.Open();

                    using (var r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            lblInfoIsEmriNo.Text = r["IsEmriNo"].ToString();
                            lblInfoUrunAdi.Text = r["UrunAdi"].ToString();
                            lblInfoParcaKodu.Text = r["UrunParcaKodu"].ToString();
                            lblInfoGram.Text = Convert.ToDecimal(r["Grami"]).ToString("N2") + " gr";
                            lblInfoKalip.Text = r["KalipNo"] == DBNull.Value ? "-" : r["KalipNo"].ToString();
                            lblInfoBoyahane.Text = r["BoyahaneDurum"].ToString();
                            lblInfoIcParca.Text = r["IcParcaDurum"].ToString();
                            pnlIsEmriInfo.Visible = true;
                        }
                        else
                        {
                            pnlIsEmriInfo.Visible = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                pnlIsEmriInfo.Visible = false;
                ShowMessage("İş emri bilgisi çekilemedi: " + ex.Message, false);
            }
        }

        private void LoadKayitlar()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(CS))
                using (SqlCommand cmd = new SqlCommand("sp_IcParcaSonKayitlar", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    gvKayitlar.DataSource = dt;
                    gvKayitlar.DataBind();
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
                int hatId = Convert.ToInt32(ddlHat.SelectedValue);
                int isEmriId = Convert.ToInt32(ddlIsEmri.SelectedValue);
                int uretimAdet = Convert.ToInt32(txtUretimAdet.Text);
                int fireAdet = Convert.ToInt32(txtFireAdet.Text);

                TimeSpan? basSaat = ParseTime(txtBasSaat.Text);
                TimeSpan? bitSaat = ParseTime(txtBitSaat.Text);

                decimal duraksamaSaat = ParseDecimalFlexible(txtDuraksamaSaat.Text, 0);
                decimal? netSaat = CalculateNetHours(basSaat, bitSaat, duraksamaSaat);
                txtNetCalismaSaat.Text = netSaat.HasValue ? netSaat.Value.ToString("N2") : "";

                using (SqlConnection conn = new SqlConnection(CS))
                using (SqlCommand cmd = new SqlCommand("sp_IcParcaKaydet", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@Tarih", SqlDbType.Date).Value = tarih;
                    cmd.Parameters.Add("@Vardiya", SqlDbType.NVarChar, 20).Value = ddlVardiya.SelectedValue == "Gece" ? "Gece" : "Gunduz";
                    cmd.Parameters.Add("@PersonelId", SqlDbType.Int).Value = personelId;
                    cmd.Parameters.Add("@HatId", SqlDbType.Int).Value = hatId;
                    cmd.Parameters.Add("@IsEmriId", SqlDbType.Int).Value = isEmriId;
                    cmd.Parameters.Add("@UretimAdet", SqlDbType.Int).Value = uretimAdet;
                    cmd.Parameters.Add("@FireAdet", SqlDbType.Int).Value = fireAdet;

                    cmd.Parameters.Add("@Aciklama", SqlDbType.NVarChar, 500).Value =
                        string.IsNullOrWhiteSpace(txtAciklama.Text) ? (object)DBNull.Value : txtAciklama.Text.Trim();

                    cmd.Parameters.Add("@KaydedenKullaniciId", SqlDbType.Int).Value = SessionHelper.KullaniciId;

                    cmd.Parameters.Add("@BaslangicSaat", SqlDbType.Time).Value = (object)ToTimeValue(basSaat) ?? DBNull.Value;
                    cmd.Parameters.Add("@BitisSaat", SqlDbType.Time).Value = (object)ToTimeValue(bitSaat) ?? DBNull.Value;
                    cmd.Parameters.Add("@DuraksamaSaat", SqlDbType.Decimal).Value = duraksamaSaat;
                    cmd.Parameters.Add("@NetCalismaSaat", SqlDbType.Decimal).Value = (object)netSaat ?? DBNull.Value;

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
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
                int hatId = Convert.ToInt32(ddlHat.SelectedValue);
                int isEmriId = Convert.ToInt32(ddlIsEmri.SelectedValue);
                int uretimAdet = Convert.ToInt32(txtUretimAdet.Text);
                int fireAdet = Convert.ToInt32(txtFireAdet.Text);

                TimeSpan? basSaat = ParseTime(txtBasSaat.Text);
                TimeSpan? bitSaat = ParseTime(txtBitSaat.Text);

                decimal duraksamaSaat = ParseDecimalFlexible(txtDuraksamaSaat.Text, 0);
                decimal? netSaat = CalculateNetHours(basSaat, bitSaat, duraksamaSaat);
                txtNetCalismaSaat.Text = netSaat.HasValue ? netSaat.Value.ToString("N2") : "";

                using (SqlConnection conn = new SqlConnection(CS))
                using (SqlCommand cmd = new SqlCommand("sp_IcParcaKayitGuncelle", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@KayitId", SqlDbType.Int).Value = kayitId;
                    cmd.Parameters.Add("@Tarih", SqlDbType.Date).Value = tarih;
                    cmd.Parameters.Add("@Vardiya", SqlDbType.NVarChar, 20).Value = ddlVardiya.SelectedValue == "Gece" ? "Gece" : "Gunduz";
                    cmd.Parameters.Add("@PersonelId", SqlDbType.Int).Value = personelId;
                    cmd.Parameters.Add("@HatId", SqlDbType.Int).Value = hatId;
                    cmd.Parameters.Add("@IsEmriId", SqlDbType.Int).Value = isEmriId;
                    cmd.Parameters.Add("@UretimAdet", SqlDbType.Int).Value = uretimAdet;
                    cmd.Parameters.Add("@FireAdet", SqlDbType.Int).Value = fireAdet;

                    cmd.Parameters.Add("@Aciklama", SqlDbType.NVarChar, 500).Value =
                        string.IsNullOrWhiteSpace(txtAciklama.Text) ? (object)DBNull.Value : txtAciklama.Text.Trim();

                    cmd.Parameters.Add("@BaslangicSaat", SqlDbType.Time).Value = (object)ToTimeValue(basSaat) ?? DBNull.Value;
                    cmd.Parameters.Add("@BitisSaat", SqlDbType.Time).Value = (object)ToTimeValue(bitSaat) ?? DBNull.Value;
                    cmd.Parameters.Add("@DuraksamaSaat", SqlDbType.Decimal).Value = duraksamaSaat;
                    cmd.Parameters.Add("@NetCalismaSaat", SqlDbType.Decimal).Value = (object)netSaat ?? DBNull.Value;

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
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
                using (SqlConnection conn = new SqlConnection(CS))
                using (SqlCommand cmd = new SqlCommand("sp_IcParcaKayitGetir", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@KayitId", SqlDbType.Int).Value = kayitId;

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            hfKayitId.Value = kayitId.ToString();
                            txtTarih.Text = Convert.ToDateTime(reader["Tarih"]).ToString("yyyy-MM-dd");

                            string vardiya = reader["Vardiya"].ToString();
                            ddlVardiya.SelectedValue = vardiya.Equals("Gece", StringComparison.OrdinalIgnoreCase) ? "Gece" : "Gunduz";
                            // Saatleri otomatik basma, kayıt varsa kayıt gelsin:
                            TimeSpan? bs = reader["BaslangicSaat"] == DBNull.Value ? (TimeSpan?)null : (TimeSpan)reader["BaslangicSaat"];
                            TimeSpan? bt = reader["BitisSaat"] == DBNull.Value ? (TimeSpan?)null : (TimeSpan)reader["BitisSaat"];

                            if (bs.HasValue) txtBasSaat.Text = bs.Value.ToString(@"hh\:mm"); else SetVardiyaSaatleri();
                            if (bt.HasValue) txtBitSaat.Text = bt.Value.ToString(@"hh\:mm");

                            ddlPersonel.SelectedValue = reader["PersonelId"].ToString();
                            ddlHat.SelectedValue = reader["HatId"].ToString();
                            ddlIsEmri.SelectedValue = reader["IsEmriId"].ToString();

                            txtUretimAdet.Text = reader["UretimAdet"].ToString();
                            txtFireAdet.Text = reader["FireAdet"].ToString();
                            txtAciklama.Text = reader["Aciklama"] != DBNull.Value ? reader["Aciklama"].ToString() : "";

                            decimal dur = reader["DuraksamaSaat"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["DuraksamaSaat"]);
                            txtDuraksamaSaat.Text = dur.ToString("N2");

                            decimal? net = reader["NetCalismaSaat"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["NetCalismaSaat"]);
                            txtNetCalismaSaat.Text = net.HasValue ? net.Value.ToString("N2") : "";

                            ddlIsEmri_SelectedIndexChanged(null, null);

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

            if (ddlHat.SelectedValue == "0")
            {
                ShowMessage("Hat seçilmelidir!", false);
                return false;
            }

            if (ddlIsEmri.SelectedValue == "0")
            {
                ShowMessage("İş emri seçilmelidir!", false);
                return false;
            }

            if (!int.TryParse(txtUretimAdet.Text, out int uretimAdet) || uretimAdet <= 0)
            {
                ShowMessage("Geçerli bir üretim adet giriniz!", false);
                return false;
            }

            if (!int.TryParse(txtFireAdet.Text, out int fireAdet) || fireAdet < 0)
            {
                ShowMessage("Geçerli bir fire adet giriniz!", false);
                return false;
            }

            var bs = ParseTime(txtBasSaat.Text);
            var bt = ParseTime(txtBitSaat.Text);
            if (!bs.HasValue || !bt.HasValue)
            {
                ShowMessage("Başlangıç/Bitiş saati HH:mm formatında olmalı. Örn: 07:30", false);
                return false;
            }

            decimal duraksama = ParseDecimalFlexible(txtDuraksamaSaat.Text, 0);
            if (duraksama < 0)
            {
                ShowMessage("Duraksama saati negatif olamaz!", false);
                return false;
            }

            // Net saat negatif olmasın
            var net = CalculateNetHours(bs, bt, duraksama);
            if (net.HasValue && net.Value < 0)
            {
                ShowMessage("Net çalışma süresi negatif çıktı. Saatleri/duraksamayı kontrol edin.", false);
                return false;
            }

            return true;
        }

        private void ClearForm()
        {
            hfKayitId.Value = "0";
            txtTarih.Text = DateTime.Now.ToString("yyyy-MM-dd");

            ddlVardiya.SelectedValue = "Gunduz";
            SetVardiyaSaatleri();

            AutoSelectPersonel();
            ddlHat.SelectedIndex = 0;
            ddlIsEmri.SelectedIndex = 0;

            txtUretimAdet.Text = "";
            txtFireAdet.Text = "0";
            txtDuraksamaSaat.Text = "0";
            txtNetCalismaSaat.Text = "";
            txtAciklama.Text = "";

            pnlIsEmriInfo.Visible = false;

            btnKaydet.Visible = true;
            btnGuncelle.Visible = false;
        }

        private TimeSpan? ParseTime(string s)
        {
            s = (s ?? "").Trim();
            if (TimeSpan.TryParseExact(s, "hh\\:mm", CultureInfo.InvariantCulture, out var ts))
                return ts;
            return null;
        }

        private object ToTimeValue(TimeSpan? ts)
        {
            if (!ts.HasValue) return null;
            return ts.Value;
        }

        private decimal ParseDecimalFlexible(string input, decimal def)
        {
            input = (input ?? "").Trim();
            if (string.IsNullOrWhiteSpace(input)) return def;

            if (decimal.TryParse(input, NumberStyles.Number, new CultureInfo("tr-TR"), out var v)) return v;
            if (decimal.TryParse(input, NumberStyles.Number, CultureInfo.InvariantCulture, out v)) return v;

            return def;
        }

        private decimal? CalculateNetHours(TimeSpan? bas, TimeSpan? bit, decimal duraksamaSaat)
        {
            if (!bas.HasValue || !bit.HasValue) return null;

            // gece vardiyası için bit < bas olabilir => +24 saat
            double basMin = bas.Value.TotalMinutes;
            double bitMin = bit.Value.TotalMinutes;
            if (bitMin < basMin) bitMin += 24 * 60;

            double farkSaat = (bitMin - basMin) / 60.0;
            decimal net = (decimal)farkSaat - duraksamaSaat;
            return Math.Round(net, 2);
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
