using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using UretimTakipSistemi.Helpers;

namespace UretimTakipSistemi.Admin
{
    public partial class RaporlarGelismis : Page
    {
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
            string kategori = hfSelectedCategory.Value;

            if (string.IsNullOrEmpty(kategori))
            {
                return;
            }

            DateTime baslangic, bitis;
            if (!DateTime.TryParse(txtBaslangic.Text, out baslangic) ||
                !DateTime.TryParse(txtBitis.Text, out bitis))
            {
                return;
            }

            string bolum = ddlBolum.SelectedValue;

            switch (kategori)
            {
                case "verimlilik":
                    ShowVerimlilikRaporu(baslangic, bitis, bolum);
                    break;
                case "makine":
                    ShowMakineRaporu(baslangic, bitis, bolum);
                    break;
                case "personel":
                    ShowPersonelRaporu(baslangic, bitis, bolum);
                    break;
                case "fire":
                    ShowFireRaporu(baslangic, bitis, bolum);
                    break;
                case "durus":
                    ShowDurusRaporu(baslangic, bitis, bolum);
                    break;
                case "urun":
                    ShowUrunRaporu(baslangic, bitis, bolum);
                    break;
            }

            pnlRapor.Visible = true;
        }

        // ===========================
        // ✅ HELPER: saniyeyi HH:mm:ss yap
        // ===========================
        private static string SecToHms(object secondsObj)
        {
            long s = 0;
            if (secondsObj != null && secondsObj != DBNull.Value)
                long.TryParse(Convert.ToString(secondsObj), out s);

            if (s < 0) s = 0;

            var ts = TimeSpan.FromSeconds(s);
            long totalHours = (long)ts.TotalHours;
            return totalHours.ToString("00") + ":" + ts.Minutes.ToString("00") + ":" + ts.Seconds.ToString("00");
        }

        // ===========================
        // ✅ HELPER: yüzde formatı
        // ===========================
        private static string PercentStr(object val, int decimals = 2)
        {
            decimal d = 0;
            if (val != null && val != DBNull.Value)
                decimal.TryParse(Convert.ToString(val), out d);

            return "%" + d.ToString("N" + decimals);
        }

        private void ShowVerimlilikRaporu(DateTime baslangic, DateTime bitis, string bolum)
        {
            kpiGrid.Controls.Clear();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlDB"].ConnectionString))
            {
                string query = @"
                    SELECT 
                        CAST(o.BaslangicZamani AS DATE) AS Tarih,
                        COUNT(*) AS OturumSayisi,
                        SUM(o.UretimAdet) AS Uretim,
                        SUM(o.FireAdet) AS Fire,
                        SUM(o.NetUretimSuresi) AS NetSureSn,
                        SUM(o.ToplamDurusSuresi) AS DurusSureSn
                    FROM EnjeksiyonOturum o
                    WHERE o.Durum = 'COMPLETED'
                        AND CAST(o.BaslangicZamani AS DATE) BETWEEN @Baslangic AND @Bitis
                    GROUP BY CAST(o.BaslangicZamani AS DATE)
                    ORDER BY Tarih";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Baslangic", baslangic);
                    cmd.Parameters.AddWithValue("@Bitis", bitis);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        DataTable displayTable = new DataTable();
                        displayTable.Columns.Add("Tarih", typeof(string));
                        displayTable.Columns.Add("Oturum", typeof(int));
                        displayTable.Columns.Add("Üretim", typeof(int));
                        displayTable.Columns.Add("Fire", typeof(int));
                        displayTable.Columns.Add("Fire %", typeof(string));
                        displayTable.Columns.Add("Net Süre", typeof(string));
                        displayTable.Columns.Add("Duruş", typeof(string));
                        displayTable.Columns.Add("Toplam", typeof(string));
                        displayTable.Columns.Add("Verimlilik %", typeof(string));
                        displayTable.Columns.Add("Saatlik", typeof(string));

                        int toplamUretim = 0;
                        int toplamFire = 0;
                        decimal toplamNetSaat = 0;
                        decimal toplamDurusSaat = 0;
                        decimal toplamVerimlilik = 0;

                        foreach (DataRow row in dt.Rows)
                        {
                            DateTime tarih = Convert.ToDateTime(row["Tarih"]);
                            int oturumSayisi = Convert.ToInt32(row["OturumSayisi"]);
                            int uretim = Convert.ToInt32(row["Uretim"]);
                            int fire = Convert.ToInt32(row["Fire"]);
                            int netSn = Convert.ToInt32(row["NetSureSn"]);
                            int durusSn = Convert.ToInt32(row["DurusSureSn"]);

                            decimal netSaat = TimeHelper.SecondsToHours(netSn);
                            decimal durusSaat = TimeHelper.SecondsToHours(durusSn);
                            decimal toplamSaat = netSaat + durusSaat;

                            decimal verimlilik = toplamSaat > 0 ? (netSaat / toplamSaat) * 100 : 0;
                            decimal saatlikUretim = netSaat > 0 ? uretim / netSaat : 0;
                            decimal fireOrani = (uretim + fire) > 0 ? (decimal)fire / (uretim + fire) * 100 : 0;

                            DataRow displayRow = displayTable.NewRow();
                            displayRow["Tarih"] = tarih.ToString("dd.MM.yyyy");
                            displayRow["Oturum"] = oturumSayisi;
                            displayRow["Üretim"] = uretim;
                            displayRow["Fire"] = fire;
                            displayRow["Fire %"] = "%" + fireOrani.ToString("N1");
                            displayRow["Net Süre"] = netSaat.ToString("N1") + " saat";
                            displayRow["Duruş"] = durusSaat.ToString("N1") + " saat";
                            displayRow["Toplam"] = toplamSaat.ToString("N1") + " saat";
                            displayRow["Verimlilik %"] = "%" + verimlilik.ToString("N1");
                            displayRow["Saatlik"] = saatlikUretim.ToString("N0") + " adet";
                            displayTable.Rows.Add(displayRow);

                            toplamUretim += uretim;
                            toplamFire += fire;
                            toplamNetSaat += netSaat;
                            toplamDurusSaat += durusSaat;
                            toplamVerimlilik += verimlilik;
                        }

                        decimal ortVerimlilik = dt.Rows.Count > 0 ? toplamVerimlilik / dt.Rows.Count : 0;
                        decimal ortSaatlik = toplamNetSaat > 0 ? toplamUretim / toplamNetSaat : 0;
                        decimal genelFireOrani = (toplamUretim + toplamFire) > 0
                            ? (decimal)toplamFire / (toplamUretim + toplamFire) * 100
                            : 0;

                        AddKpiCard("📦 Toplam Üretim", toplamUretim.ToString("N0"), "adet", "success");
                        AddKpiCard("⚡ Ort. Saatlik", ortSaatlik.ToString("N0"), "adet/saat", "");
                        AddKpiCard("📊 Ort. Verimlilik", ortVerimlilik.ToString("N1"), "%",
                            ortVerimlilik >= 80 ? "success" : ortVerimlilik >= 60 ? "warning" : "danger");
                        AddKpiCard("🔥 Fire Oranı", genelFireOrani.ToString("N1"), "%",
                            genelFireOrani < 5 ? "success" : "danger");

                        gvDetay.DataSource = displayTable;
                        gvDetay.DataBind();

                        foreach (GridViewRow gvRow in gvDetay.Rows)
                        {
                            if (gvRow.RowType == DataControlRowType.DataRow)
                            {
                                TableCell verimlilikCell = gvRow.Cells[8];
                                string verimlilikText = verimlilikCell.Text.Replace("%", "").Trim();

                                if (decimal.TryParse(verimlilikText, out decimal verimlilik))
                                {
                                    verimlilikCell.Font.Bold = true;
                                    if (verimlilik >= 80)
                                        verimlilikCell.ForeColor = System.Drawing.Color.Green;
                                    else if (verimlilik >= 60)
                                        verimlilikCell.ForeColor = System.Drawing.Color.Orange;
                                    else
                                        verimlilikCell.ForeColor = System.Drawing.Color.Red;
                                }

                                TableCell fireCell = gvRow.Cells[4];
                                string fireText = fireCell.Text.Replace("%", "").Trim();

                                if (decimal.TryParse(fireText, out decimal fire) && fire > 5)
                                {
                                    fireCell.ForeColor = System.Drawing.Color.Red;
                                    fireCell.Font.Bold = true;
                                }
                            }
                        }

                        GenerateChartScript(displayTable);
                    }
                    else
                    {
                        AddKpiCard("⚠️ Bilgi", "Veri Yok", "Seçili dönemde kayıt bulunamadı", "warning");
                    }
                }
            }
        }

        // ===========================
        // ✅ MAKİNE: NetSure/DurusSure HH:mm:ss + FireOrani %xx,xx
        // ===========================
        private void ShowMakineRaporu(DateTime baslangic, DateTime bitis, string bolum)
        {
            kpiGrid.Controls.Clear();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlDB"].ConnectionString))
            {
                // ✅ AVG yerine TOPLAM üzerinden hesapla (daha doğru)
                // ✅ NetSure / DurusSure saniye olarak çek
                string query = @"
                    SELECT 
                        m.MakineAdi,
                        COUNT(*) AS OturumSayisi,
                        SUM(o.UretimAdet) AS ToplamUretim,
                        SUM(o.FireAdet) AS ToplamFire,
                        SUM(o.NetUretimSuresi) AS NetSureSn,
                        SUM(o.ToplamDurusSuresi) AS DurusSureSn,
                        CASE 
                            WHEN SUM(o.UretimAdet + o.FireAdet) > 0
                            THEN CAST(SUM(o.FireAdet) * 100.0 / SUM(o.UretimAdet + o.FireAdet) AS DECIMAL(10,2))
                            ELSE 0
                        END AS FireOrani
                    FROM EnjeksiyonOturum o
                    INNER JOIN Makineler m ON o.MakineId = m.MakineId
                    WHERE o.Durum = 'COMPLETED'
                        AND CAST(o.BaslangicZamani AS DATE) BETWEEN @Baslangic AND @Bitis
                    GROUP BY m.MakineAdi
                    ORDER BY ToplamUretim DESC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Baslangic", baslangic);
                    cmd.Parameters.AddWithValue("@Bitis", bitis);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        DataRow topMakine = dt.Rows[0];
                        AddKpiCard("En Çok Üreten Makine", topMakine["MakineAdi"].ToString(),
                            Convert.ToInt32(topMakine["ToplamUretim"]).ToString("N0") + " adet", "success");

                        int toplamOturum = 0;
                        int toplamUretim = 0;
                        int toplamFire = 0;

                        foreach (DataRow row in dt.Rows)
                        {
                            toplamOturum += Convert.ToInt32(row["OturumSayisi"]);
                            toplamUretim += Convert.ToInt32(row["ToplamUretim"]);
                            toplamFire += Convert.ToInt32(row["ToplamFire"]);
                        }

                        decimal genelFireOrani = (toplamUretim + toplamFire) > 0
                            ? (decimal)toplamFire * 100m / (toplamUretim + toplamFire)
                            : 0;

                        AddKpiCard("Toplam Oturum", toplamOturum.ToString("N0"), "oturum", "");
                        AddKpiCard("Toplam Üretim", toplamUretim.ToString("N0"), "adet", "success");
                        AddKpiCard("🔥 Genel Fire", genelFireOrani.ToString("N2"), "%", genelFireOrani < 5 ? "success" : "danger");
                    }

                    // ✅ Görsel tablo: HH:mm:ss + % format
                    DataTable display = new DataTable();
                    display.Columns.Add("Makine", typeof(string));
                    display.Columns.Add("Oturum", typeof(int));
                    display.Columns.Add("Üretim", typeof(int));
                    display.Columns.Add("Fire", typeof(int));
                    display.Columns.Add("FireOrani", typeof(string));
                    display.Columns.Add("NetSure", typeof(string));
                    display.Columns.Add("DurusSure", typeof(string));
                    display.Columns.Add("ToplamSure", typeof(string));

                    foreach (DataRow row in dt.Rows)
                    {
                        long netSn = row["NetSureSn"] != DBNull.Value ? Convert.ToInt64(row["NetSureSn"]) : 0;
                        long durusSn = row["DurusSureSn"] != DBNull.Value ? Convert.ToInt64(row["DurusSureSn"]) : 0;

                        DataRow r = display.NewRow();
                        r["Makine"] = row["MakineAdi"].ToString();
                        r["Oturum"] = Convert.ToInt32(row["OturumSayisi"]);
                        r["Üretim"] = Convert.ToInt32(row["ToplamUretim"]);
                        r["Fire"] = Convert.ToInt32(row["ToplamFire"]);
                        r["FireOrani"] = PercentStr(row["FireOrani"], 2);
                        r["NetSure"] = SecToHms(netSn);
                        r["DurusSure"] = SecToHms(durusSn);
                        r["ToplamSure"] = SecToHms(netSn + durusSn);
                        display.Rows.Add(r);
                    }

                    gvDetay.DataSource = display;
                    gvDetay.DataBind();

                    // Fire oranını renklendir
                    foreach (GridViewRow row in gvDetay.Rows)
                    {
                        if (row.RowType != DataControlRowType.DataRow) continue;

                        // FireOrani kolonu index 4
                        TableCell fireCell = row.Cells[4];
                        string fireText = fireCell.Text.Replace("%", "").Trim();

                        if (decimal.TryParse(fireText, out decimal firePct))
                        {
                            fireCell.Font.Bold = true;
                            if (firePct >= 5)
                                fireCell.ForeColor = System.Drawing.Color.Red;
                        }
                    }

                    GenerateChartScript(display);
                }
            }
        }

        private void ShowPersonelRaporu(DateTime baslangic, DateTime bitis, string bolum)
        {
            kpiGrid.Controls.Clear();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlDB"].ConnectionString))
            {
                string query = @"
                    SELECT 
                        k.KullaniciId,
                        k.AdSoyad AS Personel,
                        k.Rol AS Bolum,
                        COUNT(*) AS OturumSayisi,
                        SUM(o.UretimAdet) AS ToplamUretim,
                        SUM(o.FireAdet) AS ToplamFire,
                        SUM(o.NetUretimSuresi) AS NetSureSaniye,
                        SUM(o.ToplamDurusSuresi) AS DurusSaniye,
                        CASE 
                            WHEN SUM(o.NetUretimSuresi + o.ToplamDurusSuresi) > 0 THEN
                                (CAST(SUM(o.NetUretimSuresi) AS FLOAT) / SUM(o.NetUretimSuresi + o.ToplamDurusSuresi)) * 100
                            ELSE 0
                        END AS VerimlilikYuzde
                    FROM Kullanicilar k
                    INNER JOIN EnjeksiyonOturum o ON k.KullaniciId = o.PersonelId
                    WHERE o.Durum = 'COMPLETED'
                        AND CAST(o.BaslangicZamani AS DATE) BETWEEN @Baslangic AND @Bitis
                    GROUP BY k.KullaniciId, k.AdSoyad, k.Rol
                    ORDER BY VerimlilikYuzde DESC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Baslangic", baslangic);
                    cmd.Parameters.AddWithValue("@Bitis", bitis);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        dt.Columns.Add("NetUretimSaati", typeof(string));
                        dt.Columns.Add("DurusSaati", typeof(string));
                        dt.Columns.Add("ToplamSaati", typeof(string));
                        dt.Columns.Add("SaatlikVerim", typeof(decimal));
                        dt.Columns.Add("SaatlikVerimStr", typeof(string));
                        dt.Columns.Add("VerimlilikStr", typeof(string));
                        dt.Columns.Add("FireOrani", typeof(decimal));

                        foreach (DataRow row in dt.Rows)
                        {
                            int netSn = Convert.ToInt32(row["NetSureSaniye"]);
                            int durusSn = Convert.ToInt32(row["DurusSaniye"]);
                            int toplamUretim = Convert.ToInt32(row["ToplamUretim"]);
                            int toplamFire = Convert.ToInt32(row["ToplamFire"]);
                            decimal verimlilik = Convert.ToDecimal(row["VerimlilikYuzde"]);

                            decimal netSaat = TimeHelper.SecondsToHours(netSn);
                            decimal durusSaat = TimeHelper.SecondsToHours(durusSn);
                            decimal toplamSaat = netSaat + durusSaat;

                            decimal saatlikVerim = netSaat > 0 ? toplamUretim / netSaat : 0;

                            decimal fireOrani = (toplamUretim + toplamFire) > 0
                                ? (decimal)toplamFire / (toplamUretim + toplamFire) * 100
                                : 0;

                            row["NetUretimSaati"] = netSaat.ToString("N1") + " saat";
                            row["DurusSaati"] = durusSaat.ToString("N1") + " saat";
                            row["ToplamSaati"] = toplamSaat.ToString("N1") + " saat";
                            row["SaatlikVerim"] = saatlikVerim;
                            row["SaatlikVerimStr"] = saatlikVerim.ToString("N0") + " adet/saat";
                            row["VerimlilikStr"] = "%" + verimlilik.ToString("N1");
                            row["FireOrani"] = fireOrani;
                        }

                        if (dt.Rows.Count > 0)
                        {
                            AddKpiCard("🏆 En Verimli",
                                dt.Rows[0]["Personel"].ToString(),
                                "%" + Convert.ToDecimal(dt.Rows[0]["VerimlilikYuzde"]).ToString("N1"),
                                "success");
                        }

                        int toplamGercek = 0;
                        decimal toplamNetSaat = 0;
                        decimal toplamVerimlilik = 0;

                        foreach (DataRow row in dt.Rows)
                        {
                            toplamGercek += Convert.ToInt32(row["ToplamUretim"]);
                            toplamNetSaat += TimeHelper.SecondsToHours(Convert.ToInt32(row["NetSureSaniye"]));
                            toplamVerimlilik += Convert.ToDecimal(row["VerimlilikYuzde"]);
                        }

                        AddKpiCard("📦 Toplam Üretim", toplamGercek.ToString("N0"), "adet", "success");
                        AddKpiCard("⏱️ Toplam Net Süre", toplamNetSaat.ToString("N1"), "saat", "");
                        AddKpiCard("📊 Ort. Verimlilik", (toplamVerimlilik / dt.Rows.Count).ToString("N1"), "%", "");
                        AddKpiCard("👥 Çalışan Sayısı", dt.Rows.Count.ToString(), "kişi", "");

                        DataTable displayTable = dt.DefaultView.ToTable(false,
                            "Personel", "Bolum", "OturumSayisi", "ToplamUretim", "SaatlikVerimStr",
                            "ToplamFire", "FireOrani", "NetUretimSaati", "DurusSaati",
                            "ToplamSaati", "VerimlilikStr");

                        displayTable.Columns["Personel"].ColumnName = "👤 Personel";
                        displayTable.Columns["Bolum"].ColumnName = "🏢 Bölüm";
                        displayTable.Columns["OturumSayisi"].ColumnName = "Oturum";
                        displayTable.Columns["ToplamUretim"].ColumnName = "✅ Üretim";
                        displayTable.Columns["SaatlikVerimStr"].ColumnName = "⚡ Saatlik Verim";
                        displayTable.Columns["ToplamFire"].ColumnName = "🔥 Fire";
                        displayTable.Columns["FireOrani"].ColumnName = "Fire %";
                        displayTable.Columns["NetUretimSaati"].ColumnName = "⏱️ Net Süre";
                        displayTable.Columns["DurusSaati"].ColumnName = "⏸️ Duruş";
                        displayTable.Columns["ToplamSaati"].ColumnName = "📅 Toplam";
                        displayTable.Columns["VerimlilikStr"].ColumnName = "📈 VERİMLİLİK";

                        gvDetay.DataSource = displayTable;
                        gvDetay.DataBind();

                        foreach (GridViewRow row in gvDetay.Rows)
                        {
                            if (row.RowType == DataControlRowType.DataRow)
                            {
                                TableCell verimlilikCell = row.Cells[row.Cells.Count - 1];
                                string verimlilikText = verimlilikCell.Text.Replace("%", "").Trim();

                                if (decimal.TryParse(verimlilikText, out decimal verimlilik))
                                {
                                    verimlilikCell.Font.Bold = true;
                                    verimlilikCell.Font.Size = 18;
                                    verimlilikCell.HorizontalAlign = HorizontalAlign.Center;

                                    if (verimlilik >= 80)
                                        verimlilikCell.ForeColor = System.Drawing.Color.Green;
                                    else if (verimlilik >= 60)
                                        verimlilikCell.ForeColor = System.Drawing.Color.Orange;
                                    else
                                        verimlilikCell.ForeColor = System.Drawing.Color.Red;
                                }

                                TableCell fireCell = row.Cells[6];
                                string fireText = fireCell.Text.Replace("%", "").Trim();
                                if (decimal.TryParse(fireText, out decimal fire))
                                {
                                    fireCell.Text = "%" + fire.ToString("N2");
                                    if (fire > 5) fireCell.ForeColor = System.Drawing.Color.Red;
                                }
                            }
                        }

                        GenerateChartScript(displayTable);
                    }
                    else
                    {
                        AddKpiCard("⚠️ Bilgi", "Veri Yok", "Seçili dönemde kayıt bulunamadı", "warning");
                    }
                }
            }
        }

        private void ShowFireRaporu(DateTime baslangic, DateTime bitis, string bolum)
        {
            kpiGrid.Controls.Clear();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlDB"].ConnectionString))
            {
                string query = @"
                    SELECT 
                        ISNULL(f.FireNedeni, 'Belirtilmemiş') AS FireNedeni,
                        COUNT(*) AS Adet,
                        SUM(f.FireAdet) AS ToplamFire,
                        AVG(CAST(f.FireAdet AS FLOAT)) AS OrtalamaFire
                    FROM EnjeksiyonFireKayit f
                    INNER JOIN EnjeksiyonOturum o ON f.OturumId = o.OturumId
                    WHERE CAST(f.KayitTarihi AS DATE) BETWEEN @Baslangic AND @Bitis
                    GROUP BY f.FireNedeni
                    ORDER BY ToplamFire DESC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Baslangic", baslangic);
                    cmd.Parameters.AddWithValue("@Bitis", bitis);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    int toplamFire = 0;
                    foreach (DataRow row in dt.Rows)
                    {
                        toplamFire += Convert.ToInt32(row["ToplamFire"]);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        DataRow topNeden = dt.Rows[0];
                        AddKpiCard("En Çok Fire Nedeni", topNeden["FireNedeni"].ToString(),
                            Convert.ToInt32(topNeden["ToplamFire"]).ToString("N0") + " adet", "danger");
                    }

                    AddKpiCard("Toplam Fire", toplamFire.ToString("N0"), "adet", "danger");
                    AddKpiCard("Fire Nedeni Sayısı", dt.Rows.Count.ToString(), "farklı neden", "warning");

                    gvDetay.DataSource = dt;
                    gvDetay.DataBind();

                    GenerateChartScript(dt);
                }
            }
        }

        // ===========================
        // ✅ DURUŞ: süreler HH:mm:ss
        // ===========================
        private void ShowDurusRaporu(DateTime baslangic, DateTime bitis, string bolum)
        {
            kpiGrid.Controls.Clear();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlDB"].ConnectionString))
            {
                string query = @"
                    SELECT 
                        d.DurusNedeni,
                        COUNT(*) AS DurusSayisi,
                        SUM(d.Sure) AS ToplamSureSn,
                        AVG(d.Sure) AS OrtalamaSureSn,
                        SUM(d.Sure) / 3600.0 AS ToplamSaat
                    FROM EnjeksiyonDurus d
                    INNER JOIN EnjeksiyonOturum o ON d.OturumId = o.OturumId
                    WHERE d.Bitis IS NOT NULL
                        AND CAST(d.Baslangic AS DATE) BETWEEN @Baslangic AND @Bitis
                    GROUP BY d.DurusNedeni
                    ORDER BY ToplamSureSn DESC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Baslangic", baslangic);
                    cmd.Parameters.AddWithValue("@Bitis", bitis);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    int toplamDurus = 0;
                    decimal toplamSaat = 0;
                    foreach (DataRow row in dt.Rows)
                    {
                        toplamDurus += Convert.ToInt32(row["DurusSayisi"]);
                        toplamSaat += row["ToplamSaat"] != DBNull.Value ? Convert.ToDecimal(row["ToplamSaat"]) : 0;
                    }

                    if (dt.Rows.Count > 0)
                    {
                        DataRow topNeden = dt.Rows[0];
                        AddKpiCard("En Çok Duruş Nedeni", topNeden["DurusNedeni"].ToString(),
                            Convert.ToDecimal(topNeden["ToplamSaat"]).ToString("N1") + " saat", "danger");
                    }

                    AddKpiCard("Toplam Duruş Sayısı", toplamDurus.ToString("N0"), "duruş", "warning");
                    AddKpiCard("Toplam Duruş Süresi", toplamSaat.ToString("N1"), "saat", "danger");

                    // ✅ Görsel tablo: HH:mm:ss
                    DataTable display = new DataTable();
                    display.Columns.Add("Duruş Nedeni", typeof(string));
                    display.Columns.Add("Adet", typeof(int));
                    display.Columns.Add("Toplam Süre", typeof(string));
                    display.Columns.Add("Ortalama Süre", typeof(string));
                    display.Columns.Add("Toplam Saat", typeof(string));

                    foreach (DataRow row in dt.Rows)
                    {
                        long toplamSn = row["ToplamSureSn"] != DBNull.Value ? Convert.ToInt64(row["ToplamSureSn"]) : 0;
                        long ortSn = row["OrtalamaSureSn"] != DBNull.Value ? Convert.ToInt64(row["OrtalamaSureSn"]) : 0;

                        DataRow r = display.NewRow();
                        r["Duruş Nedeni"] = row["DurusNedeni"].ToString();
                        r["Adet"] = Convert.ToInt32(row["DurusSayisi"]);
                        r["Toplam Süre"] = SecToHms(toplamSn);
                        r["Ortalama Süre"] = SecToHms(ortSn);
                        r["Toplam Saat"] = (row["ToplamSaat"] != DBNull.Value ? Convert.ToDecimal(row["ToplamSaat"]) : 0).ToString("N1");
                        display.Rows.Add(r);
                    }

                    gvDetay.DataSource = display;
                    gvDetay.DataBind();

                    GenerateChartScript(display);
                }
            }
        }

        // ===========================
        // ✅ ÜRÜN: FireOrani % + Net süre HH:mm:ss
        // ===========================
        private void ShowUrunRaporu(DateTime baslangic, DateTime bitis, string bolum)
        {
            kpiGrid.Controls.Clear();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlDB"].ConnectionString))
            {
                string query = @"
                    SELECT 
                        ie.UrunAdi,
                        ie.UrunParcaKodu,
                        COUNT(*) AS UretimSayisi,
                        SUM(o.UretimAdet) AS ToplamUretim,
                        SUM(o.FireAdet) AS ToplamFire,
                        CASE 
                            WHEN SUM(o.UretimAdet + o.FireAdet) > 0
                            THEN CAST(SUM(o.FireAdet) * 100.0 / SUM(o.UretimAdet + o.FireAdet) AS DECIMAL(10,2))
                            ELSE 0
                        END AS FireOrani,
                        SUM(o.NetUretimSuresi) AS NetSureSn
                    FROM EnjeksiyonOturum o
                    INNER JOIN IsEmirleri ie ON o.IsEmriId = ie.IsEmriId
                    WHERE o.Durum = 'COMPLETED'
                        AND CAST(o.BaslangicZamani AS DATE) BETWEEN @Baslangic AND @Bitis
                    GROUP BY ie.UrunAdi, ie.UrunParcaKodu
                    ORDER BY ToplamUretim DESC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Baslangic", baslangic);
                    cmd.Parameters.AddWithValue("@Bitis", bitis);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        DataRow topUrun = dt.Rows[0];
                        AddKpiCard("En Çok Üretilen", topUrun["UrunAdi"].ToString(),
                            Convert.ToInt32(topUrun["ToplamUretim"]).ToString("N0") + " adet", "success");
                    }

                    int toplamUretim = 0;
                    foreach (DataRow row in dt.Rows)
                    {
                        toplamUretim += Convert.ToInt32(row["ToplamUretim"]);
                    }

                    AddKpiCard("Toplam Üretim", toplamUretim.ToString("N0"), "adet", "success");
                    AddKpiCard("Ürün Çeşit Sayısı", dt.Rows.Count.ToString(), "farklı ürün", "");

                    // ✅ Görsel tablo
                    DataTable display = new DataTable();
                    display.Columns.Add("Ürün", typeof(string));
                    display.Columns.Add("Parça Kodu", typeof(string));
                    display.Columns.Add("Oturum", typeof(int));
                    display.Columns.Add("Üretim", typeof(int));
                    display.Columns.Add("Fire", typeof(int));
                    display.Columns.Add("Fire %", typeof(string));
                    display.Columns.Add("Net Süre", typeof(string));

                    foreach (DataRow row in dt.Rows)
                    {
                        long netSn = row["NetSureSn"] != DBNull.Value ? Convert.ToInt64(row["NetSureSn"]) : 0;

                        DataRow r = display.NewRow();
                        r["Ürün"] = row["UrunAdi"].ToString();
                        r["Parça Kodu"] = row["UrunParcaKodu"].ToString();
                        r["Oturum"] = Convert.ToInt32(row["UretimSayisi"]);
                        r["Üretim"] = Convert.ToInt32(row["ToplamUretim"]);
                        r["Fire"] = Convert.ToInt32(row["ToplamFire"]);
                        r["Fire %"] = PercentStr(row["FireOrani"], 2);
                        r["Net Süre"] = SecToHms(netSn);
                        display.Rows.Add(r);
                    }

                    gvDetay.DataSource = display;
                    gvDetay.DataBind();

                    // fire renklendir
                    foreach (GridViewRow row in gvDetay.Rows)
                    {
                        if (row.RowType != DataControlRowType.DataRow) continue;

                        TableCell fireCell = row.Cells[5]; // Fire % kolonu
                        string fireText = fireCell.Text.Replace("%", "").Trim();
                        if (decimal.TryParse(fireText, out decimal firePct))
                        {
                            fireCell.Font.Bold = true;
                            if (firePct >= 5) fireCell.ForeColor = System.Drawing.Color.Red;
                        }
                    }

                    GenerateChartScript(display);
                }
            }
        }

        private void AddKpiCard(string label, string value, string unit, string cssClass)
        {
            HtmlGenericControl card = new HtmlGenericControl("div");
            card.Attributes["class"] = "kpi-card " + cssClass;

            HtmlGenericControl labelDiv = new HtmlGenericControl("div");
            labelDiv.Attributes["class"] = "kpi-label";
            labelDiv.InnerText = label;

            HtmlGenericControl valueDiv = new HtmlGenericControl("div");
            valueDiv.Attributes["class"] = "kpi-value";
            valueDiv.InnerText = value;

            HtmlGenericControl unitDiv = new HtmlGenericControl("div");
            unitDiv.Attributes["class"] = "kpi-unit";
            unitDiv.InnerText = unit;

            card.Controls.Add(labelDiv);
            card.Controls.Add(valueDiv);
            card.Controls.Add(unitDiv);

            kpiGrid.Controls.Add(card);
        }

        private void GenerateChartScript(DataTable dt)
        {
            if (dt.Rows.Count == 0) return;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<script>");

            sb.AppendLine("var trendLabels = [");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string label = dt.Columns.Contains("Tarih")
                    ? Convert.ToDateTime(dt.Rows[i]["Tarih"]).ToString("dd/MM")
                    : dt.Rows[i][0].ToString();

                sb.Append("'" + label + "'");
                if (i < dt.Rows.Count - 1) sb.Append(",");
            }
            sb.AppendLine("];");

            sb.AppendLine("var trendData1 = [");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string colName = dt.Columns.Contains("Uretim") ? "Uretim" :
                                 dt.Columns.Contains("ToplamUretim") ? "ToplamUretim" :
                                 dt.Columns.Contains("ToplamFire") ? "ToplamFire" :
                                 dt.Columns[1].ColumnName;

                sb.Append(dt.Rows[i][colName]);
                if (i < dt.Rows.Count - 1) sb.Append(",");
            }
            sb.AppendLine("];");

            sb.AppendLine(@"
                var ctxTrend = document.getElementById('chartTrend').getContext('2d');
                new Chart(ctxTrend, {
                    type: 'line',
                    data: {
                        labels: trendLabels,
                        datasets: [{
                            label: 'Üretim',
                            data: trendData1,
                            borderColor: '#1e3c72',
                            backgroundColor: 'rgba(30, 60, 114, 0.1)',
                            tension: 0.4,
                            fill: true
                        }]
                    },
                    options: {
                        responsive: true,
                        maintainAspectRatio: true,
                        plugins: {
                            legend: { display: true }
                        },
                        scales: {
                            y: { beginAtZero: true }
                        }
                    }
                });

                var ctxDist = document.getElementById('chartDistribution').getContext('2d');
                new Chart(ctxDist, {
                    type: 'doughnut',
                    data: {
                        labels: trendLabels,
                        datasets: [{
                            data: trendData1,
                            backgroundColor: [
                                '#1e3c72', '#2a5298', '#4caf50', '#ff9800', 
                                '#f44336', '#9c27b0', '#00bcd4', '#ffeb3b'
                            ]
                        }]
                    },
                    options: {
                        responsive: true,
                        maintainAspectRatio: true,
                        plugins: {
                            legend: { position: 'bottom' }
                        }
                    }
                });
            ");

            sb.AppendLine("</script>");

            litChartData.Text = sb.ToString();
        }
    }
}
