using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web.UI;
using UretimTakipSistemi.Helpers;

namespace UretimTakipSistemi.Admin
{
    public partial class Raporlar : Page
    {
        private string CS => ConfigurationManager.ConnectionStrings["SqlDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!SessionHelper.IsLoggedIn || !SessionHelper.IsAdmin)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }
        }

        protected void btnRaporGetir_Click(object sender, EventArgs e)
        {
            string bolum = hfSeciliBolum.Value;
            if (string.IsNullOrEmpty(bolum)) return;

            if (!DateTime.TryParse(txtBaslangic.Text, out DateTime baslangic) ||
                !DateTime.TryParse(txtBitis.Text, out DateTime bitis))
            {
                return;
            }

            try
            {
                DataTable dt = null;

                switch (bolum)
                {
                    case "boyahane":
                        dt = GetBoyahaneRaporu(baslangic, bitis);
                        litRaporBaslik.Text = "🎨 Boyahane Üretim Raporu";
                        break;

                    case "icparca":
                        dt = GetIcParcaRaporu(baslangic, bitis);
                        litRaporBaslik.Text = "🔧 İç Parça Üretim Raporu";
                        break;

                    case "enjeksiyon":
                        dt = GetEnjeksiyonRaporu(baslangic, bitis);
                        litRaporBaslik.Text = "⚙️ Enjeksiyon Üretim Raporu";
                        break;
                }

                pnlRapor.Visible = true;

                if (dt != null && dt.Rows.Count > 0)
                {
                    gvRapor.DataSource = dt;
                    gvRapor.DataBind();

                    pnlVeriYok.Visible = false;
                    btnExportExcel.Visible = true;
                }
                else
                {
                    gvRapor.DataSource = null;
                    gvRapor.DataBind();

                    pnlVeriYok.Visible = true;
                    btnExportExcel.Visible = false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Rapor Hatası: " + ex.Message);
            }
        }

        protected void btnExportExcel_Click(object sender, EventArgs e)
        {
            if (gvRapor.Rows.Count == 0) return;

            Response.Clear();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment;filename=Rapor_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xls");
            Response.Charset = "UTF-8";
            Response.ContentEncoding = System.Text.Encoding.UTF8;
            Response.ContentType = "application/vnd.ms-excel";

            using (StringWriter sw = new StringWriter())
            using (HtmlTextWriter htw = new HtmlTextWriter(sw))
            {
                htw.Write("<html><head><meta http-equiv='Content-Type' content='text/html; charset=UTF-8'></head><body>");
                htw.Write("<table border='1'>");

                // Başlık satırı
                htw.Write("<tr><td colspan='" + gvRapor.Columns.Count + "' style='background-color:#1e3c72; color:white; font-weight:bold; text-align:center; padding:10px;'>");
                htw.Write((litRaporBaslik.Text ?? "Rapor").Replace("<br/>", " "));
                htw.Write("</td></tr>");

                htw.Write("<tr><td colspan='" + gvRapor.Columns.Count + "'></td></tr>");
                htw.Write("<tr><td><b>Rapor Tarihi:</b></td><td colspan='" + (gvRapor.Columns.Count - 1) + "'>" + DateTime.Now.ToString("dd.MM.yyyy HH:mm") + "</td></tr>");
                htw.Write("<tr><td><b>Dönem:</b></td><td colspan='" + (gvRapor.Columns.Count - 1) + "'>" + txtBaslangic.Text + " - " + txtBitis.Text + "</td></tr>");
                htw.Write("<tr><td colspan='" + gvRapor.Columns.Count + "'></td></tr>");

                // Grid
                gvRapor.RenderControl(htw);

                htw.Write("</table></body></html>");

                Response.Write(sw.ToString());
                Response.End();
            }
        }

        public override void VerifyRenderingInServerForm(Control control)
        {
            // Excel export için gerekli
        }

        private DataTable GetBoyahaneRaporu(DateTime baslangic, DateTime bitis)
        {
            using (SqlConnection conn = new SqlConnection(CS))
            {
                string query = @"
                    SELECT 
                        b.Tarih,
                        b.Vardiya,
                        k.AdSoyad AS Personel,
                        m.MakineAdi AS Makine,
                        b.IsEmriNo,
                        ie.UrunAdi,
                        SUM(b.UretimAdet) AS ToplamUretim,
                        SUM(b.FireAdet) AS ToplamFire,
                        CASE 
                            WHEN SUM(b.UretimAdet + b.FireAdet) > 0 
                            THEN CAST(SUM(b.FireAdet) * 100.0 / SUM(b.UretimAdet + b.FireAdet) AS DECIMAL(5,2))
                            ELSE 0 
                        END AS FireOrani
                    FROM BoyahaneUretim b
                    INNER JOIN Kullanicilar k ON b.PersonelId = k.KullaniciId
                    INNER JOIN Makineler m ON b.MakineId = m.MakineId
                    INNER JOIN IsEmirleri ie ON b.IsEmriId = ie.IsEmriId
                    WHERE b.Tarih BETWEEN @Baslangic AND @Bitis
                    GROUP BY b.Tarih, b.Vardiya, k.AdSoyad, m.MakineAdi, b.IsEmriNo, ie.UrunAdi
                    ORDER BY b.Tarih DESC, b.Vardiya";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add("@Baslangic", SqlDbType.Date).Value = baslangic.Date;
                    cmd.Parameters.Add("@Bitis", SqlDbType.Date).Value = bitis.Date;

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                }
            }
        }

        private DataTable GetIcParcaRaporu(DateTime baslangic, DateTime bitis)
        {
            using (SqlConnection conn = new SqlConnection(CS))
            {
                string query = @"
                    SELECT 
                        i.Tarih,
                        i.Vardiya,
                        k.AdSoyad AS Personel,
                        m.MakineAdi AS Hat,
                        i.IsEmriNo,
                        ie.UrunAdi,
                        SUM(i.UretimAdet) AS ToplamUretim,
                        SUM(i.FireAdet) AS ToplamFire,
                        CASE 
                            WHEN SUM(i.UretimAdet + i.FireAdet) > 0 
                            THEN CAST(SUM(i.FireAdet) * 100.0 / SUM(i.UretimAdet + i.FireAdet) AS DECIMAL(5,2))
                            ELSE 0 
                        END AS FireOrani
                    FROM IcParcaUretim i
                    INNER JOIN Kullanicilar k ON i.PersonelId = k.KullaniciId
                    INNER JOIN Makineler m ON i.HatId = m.MakineId
                    INNER JOIN IsEmirleri ie ON i.IsEmriId = ie.IsEmriId
                    WHERE i.Tarih BETWEEN @Baslangic AND @Bitis
                    GROUP BY i.Tarih, i.Vardiya, k.AdSoyad, m.MakineAdi, i.IsEmriNo, ie.UrunAdi
                    ORDER BY i.Tarih DESC, i.Vardiya";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add("@Baslangic", SqlDbType.Date).Value = baslangic.Date;
                    cmd.Parameters.Add("@Bitis", SqlDbType.Date).Value = bitis.Date;

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                }
            }
        }

        private DataTable GetEnjeksiyonRaporu(DateTime baslangic, DateTime bitis)
        {
            using (SqlConnection conn = new SqlConnection(CS))
            {
                // ✅ Sn kolon adı KALSIN ama değer HH:mm:ss gelsin
                string query = @"
                    SELECT 
                        CAST(o.BaslangicZamani AS DATE) AS Tarih,
                        k.AdSoyad AS Personel,
                        m.MakineAdi AS Makine,
                        o.IsEmriNo,
                        ie.UrunAdi,
                        ie.CevrimSuresi,
                        ie.SogumaSuresi,

                        -- Sn kolonları ama HH:mm:ss formatlı (595 yazmaz)
                        CONVERT(varchar(8), DATEADD(SECOND,
                            SUM(DATEDIFF(SECOND, o.BaslangicZamani, o.BitisZamani)), 0), 108) AS ToplamSureSn,

                        CONVERT(varchar(8), DATEADD(SECOND,
                            SUM(o.ToplamDurusSuresi), 0), 108) AS ToplamDurusSn,

                        CONVERT(varchar(8), DATEADD(SECOND,
                            SUM(o.NetUretimSuresi), 0), 108) AS NetUretimSn,

                        SUM(o.UretimAdet) AS ToplamUretim,
                        SUM(o.FireAdet) AS ToplamFire,
                        CASE 
                            WHEN SUM(o.UretimAdet + o.FireAdet) > 0 
                            THEN CAST(SUM(o.FireAdet) * 100.0 / SUM(o.UretimAdet + o.FireAdet) AS DECIMAL(5,2))
                            ELSE 0 
                        END AS FireOrani
                    FROM EnjeksiyonOturum o
                    INNER JOIN Kullanicilar k ON o.PersonelId = k.KullaniciId
                    INNER JOIN Makineler m ON o.MakineId = m.MakineId
                    INNER JOIN IsEmirleri ie ON o.IsEmriId = ie.IsEmriId
                    WHERE o.Durum = 'COMPLETED'
                      AND CAST(o.BaslangicZamani AS DATE) BETWEEN @Baslangic AND @Bitis
                    GROUP BY 
                        CAST(o.BaslangicZamani AS DATE),
                        k.AdSoyad,
                        m.MakineAdi,
                        o.IsEmriNo,
                        ie.UrunAdi,
                        ie.CevrimSuresi,
                        ie.SogumaSuresi
                    ORDER BY Tarih DESC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add("@Baslangic", SqlDbType.Date).Value = baslangic.Date;
                    cmd.Parameters.Add("@Bitis", SqlDbType.Date).Value = bitis.Date;

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                }
            }
        }
    }
}
