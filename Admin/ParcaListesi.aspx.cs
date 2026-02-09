using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;
using OfficeOpenXml;
using UretimTakipSistemi.Helpers;

namespace UretimTakipSistemi.Admin
{
    public partial class ParcaListesi : Page
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
                LoadParcalar();
            }
        }

        private void LoadParcalar()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlDB"].ConnectionString))
                {
                    string query = "SELECT * FROM UretimParcaListesi ORDER BY KayitTarihi DESC";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        gvParcalar.DataSource = dt;
                        gvParcalar.DataBind();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Hata: " + ex.Message, false);
            }
        }

        // MANUEL EKLEME
        protected void btnManuelKaydet_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtParcaKodu.Text))
            {
                ShowMessage("Parca kodu zorunludur!", false);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtUrunAdi.Text))
            {
                ShowMessage("Urun adi zorunludur!", false);
                return;
            }

            try
            {
                decimal grami = 0;
                if (!string.IsNullOrWhiteSpace(txtGrami.Text))
                {
                    if (!decimal.TryParse(txtGrami.Text, out grami))
                    {
                        ShowMessage("Grami gecerli bir sayi olmalidir!", false);
                        return;
                    }
                }

                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlDB"].ConnectionString))
                {
                    string query = @"
                        IF NOT EXISTS (SELECT 1 FROM UretimParcaListesi WHERE UrunParcaKodu = @UrunParcaKodu)
                        BEGIN
                            INSERT INTO UretimParcaListesi (UrunParcaKodu, UrunAdi, Grami, KalipNo, KayitTarihi)
                            VALUES (@UrunParcaKodu, @UrunAdi, @Grami, @KalipNo, GETDATE())
                        END
                        ELSE
                        BEGIN
                            SELECT 0
                        END";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UrunParcaKodu", txtParcaKodu.Text.Trim());
                        cmd.Parameters.AddWithValue("@UrunAdi", txtUrunAdi.Text.Trim());
                        cmd.Parameters.AddWithValue("@Grami", grami);
                        cmd.Parameters.AddWithValue("@KalipNo",
                            string.IsNullOrWhiteSpace(txtKalipNo.Text) ? (object)DBNull.Value : txtKalipNo.Text.Trim());

                        conn.Open();
                        object result = cmd.ExecuteScalar();

                        if (result != null && Convert.ToInt32(result) == 0)
                        {
                            ShowMessage("Bu parca kodu zaten mevcut!", false);
                            return;
                        }
                    }
                }

                ShowMessage("Parca basariyla eklendi!", true);
                ClearForm();
                LoadParcalar();
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
            txtParcaKodu.Text = "";
            txtUrunAdi.Text = "";
            txtGrami.Text = "";
            txtKalipNo.Text = "";
        }

        // EXCEL YÜKLEME
        protected void btnYukle_Click(object sender, EventArgs e)
        {
            if (!fileUpload.HasFile)
            {
                ShowMessage("Lutfen bir Excel dosyasi seciniz!", false);
                return;
            }

            string fileExtension = Path.GetExtension(fileUpload.FileName).ToLower();
            if (fileExtension != ".xlsx" && fileExtension != ".xls")
            {
                ShowMessage("Sadece .xlsx veya .xls dosyalari yuklenebilir!", false);
                return;
            }

            try
            {
                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

                using (var package = new ExcelPackage(fileUpload.FileContent))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];

                    if (worksheet.Dimension == null)
                    {
                        ShowMessage("Excel dosyasi bos!", false);
                        return;
                    }

                    int rowCount = worksheet.Dimension.Rows;
                    int basarili = 0;
                    int hatali = 0;
                    string hataMesajlari = "";

                    using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlDB"].ConnectionString))
                    {
                        conn.Open();

                        for (int row = 2; row <= rowCount; row++)
                        {
                            try
                            {
                                string parcaKodu = worksheet.Cells[row, 1].Text.Trim();
                                string urunAdi = worksheet.Cells[row, 2].Text.Trim();
                                string gramiStr = worksheet.Cells[row, 3].Text.Trim();
                                string kalipNo = worksheet.Cells[row, 4].Text.Trim();

                                if (string.IsNullOrEmpty(parcaKodu) && string.IsNullOrEmpty(urunAdi))
                                    continue;

                                if (string.IsNullOrEmpty(parcaKodu))
                                {
                                    hataMesajlari += $"Satir {row}: Parca kodu bos<br/>";
                                    hatali++;
                                    continue;
                                }

                                if (string.IsNullOrEmpty(urunAdi))
                                {
                                    hataMesajlari += $"Satir {row}: Urun adi bos<br/>";
                                    hatali++;
                                    continue;
                                }

                                decimal grami = 0;
                                if (!string.IsNullOrEmpty(gramiStr))
                                {
                                    gramiStr = gramiStr.Replace(",", ".");
                                    if (!decimal.TryParse(gramiStr, System.Globalization.NumberStyles.Any,
                                        System.Globalization.CultureInfo.InvariantCulture, out grami))
                                    {
                                        hataMesajlari += $"Satir {row}: Grami gecersiz ({gramiStr})<br/>";
                                        hatali++;
                                        continue;
                                    }
                                }

                                string query = @"
                                    IF NOT EXISTS (SELECT 1 FROM UretimParcaListesi WHERE UrunParcaKodu = @UrunParcaKodu)
                                    BEGIN
                                        INSERT INTO UretimParcaListesi (UrunParcaKodu, UrunAdi, Grami, KalipNo, KayitTarihi)
                                        VALUES (@UrunParcaKodu, @UrunAdi, @Grami, @KalipNo, GETDATE())
                                    END
                                    ELSE
                                    BEGIN
                                        UPDATE UretimParcaListesi 
                                        SET UrunAdi = @UrunAdi, Grami = @Grami, KalipNo = @KalipNo
                                        WHERE UrunParcaKodu = @UrunParcaKodu
                                    END";

                                using (SqlCommand cmd = new SqlCommand(query, conn))
                                {
                                    cmd.Parameters.AddWithValue("@UrunParcaKodu", parcaKodu);
                                    cmd.Parameters.AddWithValue("@UrunAdi", urunAdi);
                                    cmd.Parameters.AddWithValue("@Grami", grami);
                                    cmd.Parameters.AddWithValue("@KalipNo",
                                        string.IsNullOrEmpty(kalipNo) ? (object)DBNull.Value : kalipNo);
                                    cmd.ExecuteNonQuery();
                                }

                                basarili++;
                            }
                            catch (Exception rowEx)
                            {
                                hataMesajlari += $"Satir {row}: {rowEx.Message}<br/>";
                                hatali++;
                            }
                        }
                    }

                    string mesaj = $"<strong>Islem Tamamlandi!</strong><br/><br/>Basarili: {basarili} kayit";
                    if (hatali > 0)
                    {
                        mesaj += $"<br/>Hatali: {hatali} kayit<br/><br/><strong>Hata Detaylari:</strong><br/>{hataMesajlari}";
                    }

                    ShowMessage(mesaj, hatali == 0);
                    LoadParcalar();
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Excel isleme hatasi: " + ex.Message, false);
            }
        }

        protected void btnOrnekIndir_Click(object sender, EventArgs e)
        {
            try
            {
                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

                using (ExcelPackage package = new ExcelPackage())
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Parca Listesi");

                    worksheet.Cells[1, 1].Value = "UrunParcaKodu";
                    worksheet.Cells[1, 2].Value = "UrunAdi";
                    worksheet.Cells[1, 3].Value = "Grami";
                    worksheet.Cells[1, 4].Value = "KalipNo";

                    using (var range = worksheet.Cells[1, 1, 1, 4])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(79, 129, 189));
                        range.Style.Font.Color.SetColor(System.Drawing.Color.White);
                    }

                    worksheet.Cells[2, 1].Value = "P001";
                    worksheet.Cells[2, 2].Value = "Kapak Parcasi";
                    worksheet.Cells[2, 3].Value = 125.50;
                    worksheet.Cells[2, 4].Value = "K-2024-001";

                    worksheet.Cells[3, 1].Value = "P002";
                    worksheet.Cells[3, 2].Value = "Govde Parcasi";
                    worksheet.Cells[3, 3].Value = 340.75;
                    worksheet.Cells[3, 4].Value = "K-2024-002";

                    worksheet.Cells[4, 1].Value = "P003";
                    worksheet.Cells[4, 2].Value = "Yan Panel";
                    worksheet.Cells[4, 3].Value = 89.20;
                    worksheet.Cells[4, 4].Value = "K-2024-003";

                    worksheet.Cells.AutoFitColumns();

                    Response.Clear();
                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    Response.AddHeader("content-disposition", "attachment; filename=ParcaListesi_Ornek.xlsx");
                    Response.BinaryWrite(package.GetAsByteArray());
                    Response.End();
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Ornek dosya olusturma hatasi: " + ex.Message, false);
            }
        }

        protected void btnYenile_Click(object sender, EventArgs e)
        {
            LoadParcalar();
            ShowMessage("Liste yenilendi!", true);
        }

        protected void gvParcalar_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "Sil")
            {
                int parcaId = Convert.ToInt32(e.CommandArgument);
                SilParca(parcaId);
            }
        }

        private void SilParca(int parcaId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlDB"].ConnectionString))
                {
                    string query = "DELETE FROM UretimParcaListesi WHERE ParcaId = @ParcaId";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ParcaId", parcaId);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                ShowMessage("Parca basariyla silindi!", true);
                LoadParcalar();
            }
            catch (Exception ex)
            {
                ShowMessage("Silme hatasi: " + ex.Message, false);
            }
        }

        private void ShowMessage(string message, bool success)
        {
            litMesaj.Text = $"<div class='alert {(success ? "alert-success" : "alert-error")}'>{message}</div>";
            pnlMesaj.Visible = true;

            ScriptManager.RegisterStartupScript(this, GetType(), "hideMsg",
                "setTimeout(function(){ var panel = document.getElementById('" + pnlMesaj.ClientID + "'); if(panel) panel.style.display='none'; }, 8000);",
                true);
        }
    }
}