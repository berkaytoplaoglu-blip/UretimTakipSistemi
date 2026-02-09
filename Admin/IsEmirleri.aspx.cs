using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;
using UretimTakipSistemi.Helpers;

namespace UretimTakipSistemi.Admin
{
    public partial class IsEmirleri : Page
    {
        private string CS => ConfigurationManager.ConnectionStrings["SqlDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!SessionHelper.IsLoggedIn || !SessionHelper.IsAdmin)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!IsPostBack)
                LoadIsEmirleri();
        }

        // ✅ normalize
        private string N(string s) => (s ?? "").Trim().ToUpperInvariant();

        // ✅ bölüm iptal mi?
        private bool IsIptal(string s)
        {
            s = N(s);
            return s == "IPTAL" || s == "İPTAL";
        }

        // ✅ bölüm tamam mı? (SADECE TAMAMLANDI)
        private bool IsTamam(string s)
        {
            s = N(s);
            return s == "TAMAMLANDI";
        }

        private void LoadIsEmirleri()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(CS))
                {
                    string query = @"
SELECT 
    ie.IsEmriId,
    ie.IsEmriNo,
    ie.UrunParcaKodu,
    ie.UrunAdi,
    ie.Grami,
    ie.KalipNo,
    ie.Boyahane,
    ie.IcParca,
    ie.Enjeksiyon,
    ISNULL(NULLIF(ie.BoyahaneDurum,''), 'YENİ') AS BoyahaneDurum,
    ISNULL(NULLIF(ie.IcParcaDurum,''),  'YENİ') AS IcParcaDurum,
    ie.Durum,
    ie.OlusturmaTarihi,

    ISNULL((SELECT SUM(ISNULL(eo.UretimAdet,0)) 
            FROM dbo.EnjeksiyonOturum eo 
            WHERE eo.IsEmriId = ie.IsEmriId), 0) AS ToplamUretimAdet,

    CAST(
        (
            ISNULL((SELECT SUM(ISNULL(eo.UretimAdet,0)) 
                    FROM dbo.EnjeksiyonOturum eo 
                    WHERE eo.IsEmriId = ie.IsEmriId), 0)
          + ISNULL((SELECT SUM(ISNULL(eo.FireAdet,0)) 
                    FROM dbo.EnjeksiyonOturum eo 
                    WHERE eo.IsEmriId = ie.IsEmriId), 0)
        ) * ISNULL(ie.Grami,0)
    AS DECIMAL(18,2)) AS ToplamHammaddeGram
FROM dbo.IsEmirleri ie
WHERE 1=1";

                    if (!string.IsNullOrEmpty(ddlDurum.SelectedValue))
                        query += " AND ie.Durum = @Durum";

                    if (!string.IsNullOrEmpty(ddlBolum.SelectedValue))
                    {
                        switch (ddlBolum.SelectedValue)
                        {
                            case "Boyahane": query += " AND ie.Boyahane = 1"; break;
                            case "IcParca": query += " AND ie.IcParca = 1"; break;
                            case "Enjeksiyon": query += " AND ie.Enjeksiyon = 1"; break;
                        }
                    }

                    // ✅ Enjeksiyon + ACTIVE olanlar en üstte
                    query += " ORDER BY CASE WHEN ie.Enjeksiyon=1 AND ie.Durum='ACTIVE' THEN 0 ELSE 1 END, ie.OlusturmaTarihi DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        if (!string.IsNullOrEmpty(ddlDurum.SelectedValue))
                            cmd.Parameters.AddWithValue("@Durum", ddlDurum.SelectedValue);

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        // ✅ 3 tablo: Devam / Tamamlanan / İptal
                        DataTable dtDevam = dt.Clone();
                        DataTable dtTamamlanan = dt.Clone();
                        DataTable dtIptal = dt.Clone();

                        foreach (DataRow r in dt.Rows)
                        {
                            // --- foreach (DataRow r in dt.Rows) İÇİNDE ---

                            bool boyahane = Convert.ToBoolean(r["Boyahane"]);
                            bool icparca = Convert.ToBoolean(r["IcParca"]);
                            bool enjeksiyon = Convert.ToBoolean(r["Enjeksiyon"]);

                            string boyDur = (r["BoyahaneDurum"] ?? "YENİ").ToString();
                            string icDur = (r["IcParcaDurum"] ?? "YENİ").ToString();
                            string anaDur = (r["Durum"] ?? "").ToString();

                            bool anaIptal = N(anaDur) == "CANCELLED";
                            bool boyIptal = boyahane && IsIptal(boyDur);
                            bool icIptal = icparca && IsIptal(icDur);

                            // 1) İPTAL -> iptal tablosu
                            if (anaIptal || boyIptal || icIptal)
                            {
                                dtIptal.ImportRow(r);
                                continue;
                            }

                            // 2) Enjeksiyon + ACTIVE -> her zaman üst tablo (devam eden)
                            bool anaActive = N(anaDur) == "ACTIVE";
                            if (enjeksiyon && anaActive)
                            {
                                dtDevam.ImportRow(r);
                                continue;
                            }

                            // 3) Tamamlandı sayılması (SADECE TAMAMLANDI)
                            bool boyTamam = !boyahane || IsTamam(boyDur);
                            bool icTamam = !icparca || IsTamam(icDur);

                            // ✅ Burada ana durum COMPLETED şartı istiyorsan açık bırak
                            bool anaCompleted = N(anaDur) == "COMPLETED";

                            // ✅ hepsi tamam: boyahane+icparca tamam VE ana durum completed
                            bool hepsiTamam = boyTamam && icTamam && anaCompleted;

                            if (hepsiTamam)
                                dtTamamlanan.ImportRow(r);
                            else
                                dtDevam.ImportRow(r);

                        }

                        gvIsEmirleri.DataSource = dtDevam;
                        gvIsEmirleri.DataBind();

                        gvIsEmirleriTamamlanan.DataSource = dtTamamlanan;
                        gvIsEmirleriTamamlanan.DataBind();

                        gvIsEmirleriIptal.DataSource = dtIptal;
                        gvIsEmirleriIptal.DataBind();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Yükleme hatası: " + ex.Message, false);
            }
        }

        protected void ddlDurum_SelectedIndexChanged(object sender, EventArgs e) => LoadIsEmirleri();
        protected void ddlBolum_SelectedIndexChanged(object sender, EventArgs e) => LoadIsEmirleri();
        protected void btnYenile_Click(object sender, EventArgs e) => LoadIsEmirleri();

        // ✅ 3 GRID için aynı RowDataBound
        protected void gvIsEmirleri_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow) return;

            DataRowView row = (DataRowView)e.Row.DataItem;

            Literal litBolumler = (Literal)e.Row.FindControl("litBolumler");
            if (litBolumler != null)
            {
                bool boyahane = Convert.ToBoolean(row["Boyahane"]);
                bool icParca = Convert.ToBoolean(row["IcParca"]);
                bool enjeksiyon = Convert.ToBoolean(row["Enjeksiyon"]);

                string html = "";
                if (enjeksiyon) html += "<span class='badge badge-success'>⚙️ Enjeksiyon</span>";
                if (boyahane) html += "<span class='badge badge-warning'>🎨 Boyahane</span>";
                if (icParca) html += "<span class='badge badge-info'>🔧 İç Parça</span>";

                litBolumler.Text = string.IsNullOrEmpty(html) ? "-" : html;
            }

            Panel pnlDetay = (Panel)e.Row.FindControl("pnlDetay");
            if (pnlDetay != null) pnlDetay.Visible = false;

            LinkButton btnToggle = (LinkButton)e.Row.FindControl("btnToggle");
            if (btnToggle != null) btnToggle.Text = "➕";
        }

        // ✅ 3 GRID için aynı RowCommand
        protected void gvIsEmirleri_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int isEmriId = Convert.ToInt32(e.CommandArgument);

            if (e.CommandName == "Toggle")
            {
                ToggleDetay(isEmriId);
                return;
            }

            if (e.CommandName == "Duzenle")
            {
                LoadIsEmri(isEmriId);
            }
            else if (e.CommandName == "Iptal")
            {
                IptalIsEmri(isEmriId);
            }
        }

        // ✅ 3 gridde detay aç/kapat
        private void ToggleDetay(int isEmriId)
        {
            ToggleDetayInGrid(gvIsEmirleri, isEmriId);
            ToggleDetayInGrid(gvIsEmirleriTamamlanan, isEmriId);
            ToggleDetayInGrid(gvIsEmirleriIptal, isEmriId);
        }

        private void ToggleDetayInGrid(GridView grid, int isEmriId)
        {
            if (grid == null) return;

            foreach (GridViewRow row in grid.Rows)
            {
                if (row.RowType != DataControlRowType.DataRow) continue;

                int rowId = Convert.ToInt32(grid.DataKeys[row.RowIndex].Value);

                Panel pnlDetay = (Panel)row.FindControl("pnlDetay");
                LinkButton btnToggle = (LinkButton)row.FindControl("btnToggle");

                if (pnlDetay == null || btnToggle == null) continue;

                if (rowId == isEmriId)
                {
                    bool yeniDurum = !pnlDetay.Visible;
                    pnlDetay.Visible = yeniDurum;
                    btnToggle.Text = yeniDurum ? "➖" : "➕";

                    Panel pnlBoyahane = (Panel)row.FindControl("pnlBoyahaneDurum");
                    Panel pnlIcParca = (Panel)row.FindControl("pnlIcParcaDurum");
                    Label lblBoyahane = (Label)row.FindControl("lblBoyahaneDurum");
                    Label lblIcParca = (Label)row.FindControl("lblIcParcaDurum");

                    using (SqlConnection conn = new SqlConnection(CS))
                    {
                        string q = @"
SELECT 
    Boyahane,
    IcParca,
    ISNULL(NULLIF(BoyahaneDurum,''),'YENİ') AS BoyahaneDurum,
    ISNULL(NULLIF(IcParcaDurum,''),'YENİ') AS IcParcaDurum
FROM dbo.IsEmirleri
WHERE IsEmriId = @id";

                        using (SqlCommand cmd = new SqlCommand(q, conn))
                        {
                            cmd.Parameters.AddWithValue("@id", isEmriId);
                            conn.Open();

                            using (SqlDataReader r = cmd.ExecuteReader())
                            {
                                if (r.Read())
                                {
                                    bool boyahane = Convert.ToBoolean(r["Boyahane"]);
                                    bool icparca = Convert.ToBoolean(r["IcParca"]);

                                    if (pnlBoyahane != null) pnlBoyahane.Visible = boyahane;
                                    if (pnlIcParca != null) pnlIcParca.Visible = icparca;

                                    if (lblBoyahane != null) lblBoyahane.Text = r["BoyahaneDurum"].ToString();
                                    if (lblIcParca != null) lblIcParca.Text = r["IcParcaDurum"].ToString();
                                }
                            }
                        }
                    }
                }
                else
                {
                    pnlDetay.Visible = false;
                    btnToggle.Text = "➕";
                }
            }
        }

        private void LoadIsEmri(int isEmriId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(CS))
                {
                    string query = @"
SELECT 
    IsEmriId, IsEmriNo, UrunAdi,
    Boyahane, IcParca, Enjeksiyon,
    ISNULL(NULLIF(BoyahaneDurum,''),'YENİ') AS BoyahaneDurum,
    ISNULL(NULLIF(IcParcaDurum,''),'YENİ')  AS IcParcaDurum,
    CevrimSuresi, SogumaSuresi, Durum
FROM dbo.IsEmirleri
WHERE IsEmriId = @IsEmriId";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IsEmriId", isEmriId);
                        conn.Open();

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                hfIsEmriId.Value = isEmriId.ToString();
                                txtIsEmriNo.Text = reader["IsEmriNo"].ToString();
                                txtUrunAdi.Text = reader["UrunAdi"].ToString();

                                bool boyahane = Convert.ToBoolean(reader["Boyahane"]);
                                bool icParca = Convert.ToBoolean(reader["IcParca"]);
                                bool enjeksiyon = Convert.ToBoolean(reader["Enjeksiyon"]);

                                chkBoyahane.Checked = boyahane;
                                chkIcParca.Checked = icParca;
                                chkEnjeksiyon.Checked = enjeksiyon;

                                txtCevrimSuresi.Text = reader["CevrimSuresi"] == DBNull.Value ? "" : reader["CevrimSuresi"].ToString();
                                txtSogumaSuresi.Text = reader["SogumaSuresi"] == DBNull.Value ? "" : reader["SogumaSuresi"].ToString();

                                ddlDurumModal.SelectedValue = reader["Durum"].ToString();

                                if (boyahane) ddlBoyahaneDurum.SelectedValue = reader["BoyahaneDurum"].ToString();
                                if (icParca) ddlIcParcaDurum.SelectedValue = reader["IcParcaDurum"].ToString();
                            }
                        }
                    }

                    string script = @"
openModal();
document.getElementById('boyahaneSection').style.display = " + (chkBoyahane.Checked ? "'block'" : "'none'") + @";
document.getElementById('icparcaSection').style.display = " + (chkIcParca.Checked ? "'block'" : "'none'") + @";
document.getElementById('enjeksiyonSection').style.display = " + (chkEnjeksiyon.Checked ? "'block'" : "'none'") + @";
";
                    ScriptManager.RegisterStartupScript(this, GetType(), "openModal", script, true);
                }
            }
            catch (Exception ex)
            {
                ShowMessage("İş emri yükleme hatası: " + ex.Message, false);
            }
        }

        protected void btnKaydet_Click(object sender, EventArgs e)
        {
            try
            {
                int isEmriId = Convert.ToInt32(hfIsEmriId.Value);
                if (ddlDurumModal.SelectedValue == "COMPLETED")
                {
                    if (HasActiveEnjeksiyonOturum(isEmriId))
                    {
                        ShowMessage("Aktif enjeksiyon terminali var. Önce terminali kapatınız.", false);
                        return;
                    }
                }

                using (SqlConnection conn = new SqlConnection(CS))
                {
                    conn.Open();

                    string query = @"
UPDATE dbo.IsEmirleri
SET Boyahane = @Boyahane,
    IcParca = @IcParca,
    Enjeksiyon = @Enjeksiyon,
    BoyahaneDurum = @BoyahaneDurum,
    IcParcaDurum = @IcParcaDurum,
    CevrimSuresi = @CevrimSuresi,
    SogumaSuresi = @SogumaSuresi,
    Durum = @Durum
WHERE IsEmriId = @IsEmriId";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IsEmriId", isEmriId);

                        cmd.Parameters.Add("@Boyahane", SqlDbType.Bit).Value = chkBoyahane.Checked;
                        cmd.Parameters.Add("@IcParca", SqlDbType.Bit).Value = chkIcParca.Checked;
                        cmd.Parameters.Add("@Enjeksiyon", SqlDbType.Bit).Value = chkEnjeksiyon.Checked;

                        // ✅ Bölüm seçili değilse otomatik YENİ
                        cmd.Parameters.AddWithValue("@BoyahaneDurum",
                            chkBoyahane.Checked ? (object)ddlBoyahaneDurum.SelectedValue : "YENİ");

                        cmd.Parameters.AddWithValue("@IcParcaDurum",
                            chkIcParca.Checked ? (object)ddlIcParcaDurum.SelectedValue : "YENİ");

                        cmd.Parameters.Add("@CevrimSuresi", SqlDbType.Int).Value =
                            (chkEnjeksiyon.Checked && !string.IsNullOrEmpty(txtCevrimSuresi.Text))
                                ? (object)Convert.ToInt32(txtCevrimSuresi.Text)
                                : DBNull.Value;

                        cmd.Parameters.Add("@SogumaSuresi", SqlDbType.Int).Value =
                            (chkEnjeksiyon.Checked && !string.IsNullOrEmpty(txtSogumaSuresi.Text))
                                ? (object)Convert.ToInt32(txtSogumaSuresi.Text)
                                : DBNull.Value;

                        cmd.Parameters.AddWithValue("@Durum", ddlDurumModal.SelectedValue);

                        cmd.ExecuteNonQuery();
                    }
                }

                ShowMessage("İş emri başarıyla güncellendi.", true);
                ScriptManager.RegisterStartupScript(this, GetType(), "closeAndReload",
                    "closeModal(); setTimeout(function(){ window.location.href='" + Request.RawUrl + "'; }, 300);", true);
            }
            catch (Exception ex)
            {
                ShowMessage("Güncelleme hatası: " + ex.Message, false);
            }
        }

        private void IptalIsEmri(int isEmriId)
        {
            try
            {
                // ✅ Aktif terminal varsa iptal ettirme
                if (HasActiveEnjeksiyonOturum(isEmriId))
                {
                    ShowMessage("Aktif enjeksiyon terminali var. Önce terminali kapatınız.", false);
                    return;
                }

                using (SqlConnection conn = new SqlConnection(CS))
                {
                    using (SqlCommand cmd = new SqlCommand(
                        "UPDATE dbo.IsEmirleri SET Durum='CANCELLED' WHERE IsEmriId=@id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", isEmriId);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                ShowMessage("İş emri iptal edildi.", true);
                LoadIsEmirleri();
            }
            catch (Exception ex)
            {
                ShowMessage("İptal hatası: " + ex.Message, false);
            }
        }

        private void ShowMessage(string message, bool success)
        {
            litMesaj.Text = $"<div class='alert {(success ? "alert-success" : "alert-error")}'>{message}</div>";
            pnlMesaj.Style["display"] = "block";

            ScriptManager.RegisterStartupScript(this, GetType(), "hideMsg",
                "setTimeout(function(){ var p = document.getElementById('" + pnlMesaj.ClientID + "'); if(p) p.style.display='none'; }, 5000);",
                true);
        }

        protected string GetStatusBadgeClass(string durum)
        {
            switch (durum)
            {
                case "ACTIVE": return "badge-success";
                case "COMPLETED": return "badge-warning";
                case "CANCELLED": return "badge-danger";
                default: return "";
            }
        }

        protected string GetStatusText(string durum)
        {
            switch (durum)
            {
                case "ACTIVE": return "AKTİF";
                case "COMPLETED": return "TAMAMLANDI";
                case "CANCELLED": return "İPTAL";
                default: return durum;
            }
        }


        protected void btnExcel_Click(object sender, EventArgs e)
        {
            DateTime bas = string.IsNullOrEmpty(txtBasTarih.Text)
                ? DateTime.MinValue
                : Convert.ToDateTime(txtBasTarih.Text);

            DateTime bit = string.IsNullOrEmpty(txtBitTarih.Text)
                ? DateTime.MaxValue
                : Convert.ToDateTime(txtBitTarih.Text).AddDays(1);

            string durum = ddlExcelDurum.SelectedValue; // ALL / COMPLETED / CANCELLED

            using (SqlConnection conn = new SqlConnection(CS))
            {
                string q = @"
SELECT
    ie.IsEmriNo      AS [İş Emri No],
    ie.UrunParcaKodu AS [Parça Kodu],
    ie.UrunAdi       AS [Ürün Adı],
    ie.Grami         AS [Gram],
    ie.KalipNo       AS [Kalıp No],
    ie.BoyahaneDurum AS [Boyahane Durum],
    ie.IcParcaDurum  AS [İç Parça Durum],
    ie.Durum         AS [Ana Durum],
    ie.OlusturmaTarihi AS [Oluşturma Tarihi],

    ISNULL((
        SELECT SUM(ISNULL(eo.UretimAdet,0))
        FROM dbo.EnjeksiyonOturum eo
        WHERE eo.IsEmriId = ie.IsEmriId
    ),0) AS [Toplam Üretim],

    CAST(
        (
            ISNULL((
                SELECT SUM(ISNULL(eo.UretimAdet,0))
                FROM dbo.EnjeksiyonOturum eo
                WHERE eo.IsEmriId = ie.IsEmriId
            ),0)
            +
            ISNULL((
                SELECT SUM(ISNULL(eo.FireAdet,0))
                FROM dbo.EnjeksiyonOturum eo
                WHERE eo.IsEmriId = ie.IsEmriId
            ),0)
        ) * ISNULL(ie.Grami,0)
    AS DECIMAL(18,2)) AS [Toplam Hammadde (gr)]
FROM dbo.IsEmirleri ie
WHERE
    ie.Durum IN ('COMPLETED','CANCELLED')
    AND ie.OlusturmaTarihi >= @bas
    AND ie.OlusturmaTarihi < @bit
";

                if (durum != "ALL")
                    q += " AND ie.Durum = @durum";

                q += " ORDER BY ie.OlusturmaTarihi DESC";

                using (SqlCommand cmd = new SqlCommand(q, conn))
                {
                    cmd.Parameters.AddWithValue("@bas", bas);
                    cmd.Parameters.AddWithValue("@bit", bit);

                    if (durum != "ALL")
                        cmd.Parameters.AddWithValue("@durum", durum);

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        ExportToExcelHtml(dt);
                    }
                }
            }
        }


        private void ExportToExcelHtml(DataTable dt)
        {
            Response.Clear();
            Response.Buffer = true;

            // ✅ Excel için HTML tablo
            Response.ContentType = "application/vnd.ms-excel";
            Response.ContentEncoding = System.Text.Encoding.UTF8;

            // ✅ UTF-8 BOM -> Türkçe karakter problemi biter
            Response.BinaryWrite(System.Text.Encoding.UTF8.GetPreamble());

            string fileName = "IsEmirleri_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".xls";
            Response.AddHeader("Content-Disposition", "attachment;filename=" + fileName);

            var sb = new System.Text.StringBuilder();

            sb.AppendLine("<html><head><meta charset='utf-8'></head><body>");
            sb.AppendLine("<table border='1' style='border-collapse:collapse;font-family:Segoe UI, Arial;font-size:12px;'>");

            // Header
            sb.AppendLine("<tr style='background:#1e3c72;color:#fff;font-weight:bold;'>");
            foreach (DataColumn col in dt.Columns)
                sb.Append("<td style='padding:6px;white-space:nowrap;'>" + System.Web.HttpUtility.HtmlEncode(col.ColumnName) + "</td>");
            sb.AppendLine("</tr>");

            // Rows
            foreach (DataRow row in dt.Rows)
            {
                sb.AppendLine("<tr>");
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    object val = row[i];

                    // Tarih formatı düzgün gelsin
                    if (val != DBNull.Value && dt.Columns[i].ColumnName == "Oluşturma Tarihi")
                    {
                        DateTime d;
                        if (DateTime.TryParse(val.ToString(), out d))
                            val = d.ToString("dd.MM.yyyy HH:mm");
                    }

                    // Sayılar Excel’de sayı olsun (binlik/ondalık bozulmasın)
                    string style = "padding:6px;";
                    if (dt.Columns[i].ColumnName == "Gram" ||
                        dt.Columns[i].ColumnName == "Toplam Üretim" ||
                        dt.Columns[i].ColumnName == "Toplam Hammadde (gr)")
                    {
                        style += "mso-number-format:'0.00';";
                        if (dt.Columns[i].ColumnName == "Toplam Üretim")
                            style = "padding:6px;mso-number-format:'0';";
                    }

                    sb.Append("<td style='" + style + "'>" + System.Web.HttpUtility.HtmlEncode(val == DBNull.Value ? "" : val.ToString()) + "</td>");
                }
                sb.AppendLine("</tr>");
            }

            sb.AppendLine("</table></body></html>");

            Response.Write(sb.ToString());
            Response.Flush();
            Response.End();
        }


        private bool HasActiveEnjeksiyonOturum(int isEmriId)
        {
            using (SqlConnection conn = new SqlConnection(CS))
            {
                // Durum alanını bilmiyorsak en sağlam kriter: BitisZamani IS NULL
                // (istersen Durum='ACTIVE' gibi ek kriter de eklenir)
                string q = @"
SELECT TOP 1 1
FROM dbo.EnjeksiyonOturum
WHERE IsEmriId = @id
  AND BitisZamani IS NULL
";
                using (SqlCommand cmd = new SqlCommand(q, conn))
                {
                    cmd.Parameters.AddWithValue("@id", isEmriId);
                    conn.Open();
                    object x = cmd.ExecuteScalar();
                    return x != null;
                }
            }
        }


















    }
}
