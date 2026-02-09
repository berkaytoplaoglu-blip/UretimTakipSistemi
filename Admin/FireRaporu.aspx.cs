using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.EnterpriseServices.CompensatingResourceManager;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using UretimTakipSistemi.Helpers;


namespace UretimTakipSistemi.Admin
{
    public partial class FireRaporu : Page
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
                LoadFilters();
            }
        }

        private void LoadFilters()
        {
            // Personel listesi
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlDB"].ConnectionString))
            {
                string query = "SELECT KullaniciId, AdSoyad FROM Kullanicilar WHERE Aktif = 1 ORDER BY AdSoyad";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    ddlPersonel.Items.Clear();
                    ddlPersonel.Items.Add(new ListItem("-- Tüm Personel --", "0"));

                    while (reader.Read())
                    {
                        ddlPersonel.Items.Add(new ListItem(
                            reader["AdSoyad"].ToString(),
                            reader["KullaniciId"].ToString()
                        ));
                    }
                }
            }

            // Makine listesi
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlDB"].ConnectionString))
            {
                string query = "SELECT MakineId, MakineAdi FROM Makineler WHERE Aktif = 1 AND Bolum = 'Enjeksiyon' ORDER BY MakineAdi";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    ddlMakine.Items.Clear();
                    ddlMakine.Items.Add(new ListItem("-- Tüm Makineler --", "0"));

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

        protected void btnRaporGetir_Click(object sender, EventArgs e)
        {
            DateTime baslangic, bitis;
            if (!DateTime.TryParse(txtBaslangic.Text, out baslangic) ||
                !DateTime.TryParse(txtBitis.Text, out bitis))
            {
                return;
            }

            int personelId = Convert.ToInt32(ddlPersonel.SelectedValue);
            int makineId = Convert.ToInt32(ddlMakine.SelectedValue);

            LoadFireRaporu(baslangic, bitis, personelId, makineId);
        }

        private void LoadFireRaporu(DateTime baslangic, DateTime bitis, int personelId, int makineId)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlDB"].ConnectionString))
            {
                // Detaylı fire kayıtları
                StringBuilder queryBuilder = new StringBuilder();
                queryBuilder.Append(@"
                    SELECT 
                        CAST(o.BaslangicZamani AS DATE) AS Tarih,
                        k.AdSoyad AS Personel,
                        m.MakineAdi AS Makine,
                        o.IsEmriNo,
                        ie.UrunAdi,
                        o.UretimAdet,
                        o.FireAdet,
                        ISNULL(f.FireNedeni, 'Belirtilmemiş') AS FireNedeni
                    FROM EnjeksiyonOturum o
                    INNER JOIN Kullanicilar k ON o.PersonelId = k.KullaniciId
                    INNER JOIN Makineler m ON o.MakineId = m.MakineId
                    INNER JOIN IsEmirleri ie ON o.IsEmriId = ie.IsEmriId
                    LEFT JOIN EnjeksiyonFireKayit f ON o.OturumId = f.OturumId
                    WHERE o.Durum = 'COMPLETED'
                        AND o.FireAdet > 0
                        AND CAST(o.BaslangicZamani AS DATE) BETWEEN @Baslangic AND @Bitis");

                if (personelId > 0)
                    queryBuilder.Append(" AND o.PersonelId = @PersonelId");

                if (makineId > 0)
                    queryBuilder.Append(" AND o.MakineId = @MakineId");

                queryBuilder.Append(" ORDER BY o.BaslangicZamani DESC");

                using (SqlCommand cmd = new SqlCommand(queryBuilder.ToString(), conn))
                {
                    cmd.Parameters.AddWithValue("@Baslangic", baslangic);
                    cmd.Parameters.AddWithValue("@Bitis", bitis);

                    if (personelId > 0)
                        cmd.Parameters.AddWithValue("@PersonelId", personelId);

                    if (makineId > 0)
                        cmd.Parameters.AddWithValue("@MakineId", makineId);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        gvFireDetay.DataSource = dt;
                        gvFireDetay.DataBind();
                        pnlVeriYok.Visible = false;

                        CalculateKPIs(dt);
                        GenerateCharts(baslangic, bitis, personelId, makineId);
                    }
                    else
                    {
                        gvFireDetay.DataSource = null;
                        gvFireDetay.DataBind();
                        pnlVeriYok.Visible = true;
                    }
                }
            }

            pnlRapor.Visible = true;
        }

        private void CalculateKPIs(DataTable dt)
        {
            int toplamFire = 0;
            int toplamUretim = 0;

            foreach (DataRow row in dt.Rows)
            {
                toplamFire += Convert.ToInt32(row["FireAdet"]);
                toplamUretim += Convert.ToInt32(row["UretimAdet"]);
            }

            decimal fireOrani = (toplamUretim + toplamFire) > 0
                ? (decimal)toplamFire / (toplamUretim + toplamFire) * 100
                : 0;

            litToplamFire.Text = toplamFire.ToString("N0");
            litFireOrani.Text = fireOrani.ToString("N2");

            // En çok fire yapan personel
            var personelGroups = dt.AsEnumerable()
                .GroupBy(r => r.Field<string>("Personel"))
                .Select(g => new
                {
                    Personel = g.Key,
                    ToplamFire = g.Sum(r => r.Field<int>("FireAdet"))
                })
                .OrderByDescending(x => x.ToplamFire)
                .FirstOrDefault();

            if (personelGroups != null)
            {
                litTopPersonel.Text = personelGroups.Personel;
                litTopPersonelFire.Text = personelGroups.ToplamFire.ToString("N0");
            }

            // En çok fire olan makine
            var makineGroups = dt.AsEnumerable()
                .GroupBy(r => r.Field<string>("Makine"))
                .Select(g => new
                {
                    Makine = g.Key,
                    ToplamFire = g.Sum(r => r.Field<int>("FireAdet"))
                })
                .OrderByDescending(x => x.ToplamFire)
                .FirstOrDefault();

            if (makineGroups != null)
            {
                litTopMakine.Text = makineGroups.Makine;
                litTopMakineFire.Text = makineGroups.ToplamFire.ToString("N0");
            }
        }

        private void GenerateCharts(DateTime baslangic, DateTime bitis, int personelId, int makineId)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<script>");

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlDB"].ConnectionString))
            {
                conn.Open();

                // 1. PERSONEL BAZLI FIRE
                StringBuilder personelQuery = new StringBuilder();
                personelQuery.Append(@"
            SELECT TOP 10
                k.AdSoyad AS Personel,
                SUM(o.FireAdet) AS ToplamFire
            FROM EnjeksiyonOturum o
            INNER JOIN Kullanicilar k ON o.PersonelId = k.KullaniciId
            WHERE o.Durum = 'COMPLETED'
                AND o.FireAdet > 0
                AND CAST(o.BaslangicZamani AS DATE) BETWEEN @Baslangic AND @Bitis");

                if (personelId > 0)
                    personelQuery.Append(" AND o.PersonelId = @PersonelId");

                personelQuery.Append(" GROUP BY k.AdSoyad ORDER BY ToplamFire DESC");

                using (SqlCommand cmd = new SqlCommand(personelQuery.ToString(), conn))
                {
                    cmd.Parameters.AddWithValue("@Baslangic", baslangic);
                    cmd.Parameters.AddWithValue("@Bitis", bitis);
                    if (personelId > 0)
                        cmd.Parameters.AddWithValue("@PersonelId", personelId);

                    SqlDataReader reader = cmd.ExecuteReader();

                    sb.AppendLine("var personelLabels = [];");
                    sb.AppendLine("var personelData = [];");

                    while (reader.Read())
                    {
                        string personel = reader["Personel"].ToString().Replace("'", "\\'");
                        int fire = Convert.ToInt32(reader["ToplamFire"]);

                        sb.AppendLine($"personelLabels.push('{personel}');");
                        sb.AppendLine($"personelData.push({fire});");
                    }

                    reader.Close();
                }

                // 2. MAKİNE BAZLI FIRE
                StringBuilder makineQuery = new StringBuilder();
                makineQuery.Append(@"
            SELECT TOP 10
                m.MakineAdi AS Makine,
                SUM(o.FireAdet) AS ToplamFire
            FROM EnjeksiyonOturum o
            INNER JOIN Makineler m ON o.MakineId = m.MakineId
            WHERE o.Durum = 'COMPLETED'
                AND o.FireAdet > 0
                AND CAST(o.BaslangicZamani AS DATE) BETWEEN @Baslangic AND @Bitis");

                if (makineId > 0)
                    makineQuery.Append(" AND o.MakineId = @MakineId");

                makineQuery.Append(" GROUP BY m.MakineAdi ORDER BY ToplamFire DESC");

                using (SqlCommand cmd = new SqlCommand(makineQuery.ToString(), conn))
                {
                    cmd.Parameters.AddWithValue("@Baslangic", baslangic);
                    cmd.Parameters.AddWithValue("@Bitis", bitis);
                    if (makineId > 0)
                        cmd.Parameters.AddWithValue("@MakineId", makineId);

                    SqlDataReader reader = cmd.ExecuteReader();

                    sb.AppendLine("var makineLabels = [];");
                    sb.AppendLine("var makineData = [];");

                    while (reader.Read())
                    {
                        string makine = reader["Makine"].ToString().Replace("'", "\\'");
                        int fire = Convert.ToInt32(reader["ToplamFire"]);

                        sb.AppendLine($"makineLabels.push('{makine}');");
                        sb.AppendLine($"makineData.push({fire});");
                    }

                    reader.Close();
                }

                // 3. FIRE NEDENLERİ
                using (SqlCommand cmd = new SqlCommand(@"
            SELECT 
                ISNULL(f.FireNedeni, 'Belirtilmemiş') AS Neden,
                SUM(f.FireAdet) AS ToplamFire
            FROM EnjeksiyonFireKayit f
            INNER JOIN EnjeksiyonOturum o ON f.OturumId = o.OturumId
            WHERE CAST(f.KayitTarihi AS DATE) BETWEEN @Baslangic AND @Bitis
            GROUP BY f.FireNedeni
            ORDER BY ToplamFire DESC", conn))
                {
                    cmd.Parameters.AddWithValue("@Baslangic", baslangic);
                    cmd.Parameters.AddWithValue("@Bitis", bitis);

                    SqlDataReader reader = cmd.ExecuteReader();

                    sb.AppendLine("var nedenLabels = [];");
                    sb.AppendLine("var nedenData = [];");

                    while (reader.Read())
                    {
                        string neden = reader["Neden"].ToString().Replace("'", "\\'");
                        int fire = Convert.ToInt32(reader["ToplamFire"]);

                        sb.AppendLine($"nedenLabels.push('{neden}');");
                        sb.AppendLine($"nedenData.push({fire});");
                    }

                    reader.Close();
                }
            }

            // CHART.JS KODLARI
            sb.AppendLine(@"
        // Personel Chart
        if (personelLabels.length > 0) {
            var ctxPersonel = document.getElementById('chartPersonel').getContext('2d');
            new Chart(ctxPersonel, {
                type: 'bar',
                data: {
                    labels: personelLabels,
                    datasets: [{
                        label: 'Fire Adet',
                        data: personelData,
                        backgroundColor: '#f44336',
                        borderColor: '#d32f2f',
                        borderWidth: 1
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: true,
                    scales: {
                        y: { beginAtZero: true }
                    }
                }
            });
        } else {
            document.getElementById('chartPersonel').parentElement.innerHTML = '<p style=""text-align:center;color:#999;padding:40px;"">Veri bulunamadı</p>';
        }

        // Makine Chart
        if (makineLabels.length > 0) {
            var ctxMakine = document.getElementById('chartMakine').getContext('2d');
            new Chart(ctxMakine, {
                type: 'bar',
                data: {
                    labels: makineLabels,
                    datasets: [{
                        label: 'Fire Adet',
                        data: makineData,
                        backgroundColor: '#ff9800',
                        borderColor: '#f57c00',
                        borderWidth: 1
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: true,
                    scales: {
                        y: { beginAtZero: true }
                    }
                }
            });
        } else {
            document.getElementById('chartMakine').parentElement.innerHTML = '<p style=""text-align:center;color:#999;padding:40px;"">Veri bulunamadı</p>';
        }

        // Neden Chart
        if (nedenLabels.length > 0) {
            var ctxNeden = document.getElementById('chartNeden').getContext('2d');
            new Chart(ctxNeden, {
                type: 'doughnut',
                data: {
                    labels: nedenLabels,
                    datasets: [{
                        data: nedenData,
                        backgroundColor: [
                            '#f44336', '#ff9800', '#ffeb3b', '#4caf50',
                            '#2196f3', '#9c27b0', '#795548', '#607d8b'
                        ]
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: true,
                    plugins: {
                        legend: { position: 'right' }
                    }
                }
            });
        } else {
            document.getElementById('chartNeden').parentElement.innerHTML = '<p style=""text-align:center;color:#999;padding:40px;"">Veri bulunamadı</p>';
        }
    ");

            sb.AppendLine("</script>");

            litChartScript.Text = sb.ToString();
        }

        protected void btnExcelAktar_Click(object sender, EventArgs e)
        {
            if (gvFireDetay.Rows.Count == 0)
            {
                return;
            }

            Response.Clear();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment;filename=FireRaporu_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xls");
            Response.Charset = "";
            Response.ContentType = "application/vnd.ms-excel";

            using (StringWriter sw = new StringWriter())
            {
                using (System.Web.UI.HtmlTextWriter htw = new System.Web.UI.HtmlTextWriter(sw))
                {
                    // Header bilgileri
                    htw.Write("<table border='1'>");
                    htw.Write("<tr><td colspan='9' style='background-color:#f44336; color:white; font-weight:bold; text-align:center; padding:10px;'>");
                    htw.Write("FIRE DETAY RAPORU</td></tr>");
                    htw.Write("<tr><td colspan='9'></td></tr>");
                    htw.Write("<tr><td><b>Rapor Tarihi:</b></td><td colspan='8'>" + DateTime.Now.ToString("dd.MM.yyyy HH:mm") + "</td></tr>");
                    htw.Write("<tr><td><b>Dönem:</b></td><td colspan='8'>" + txtBaslangic.Text + " - " + txtBitis.Text + "</td></tr>");
                    htw.Write("<tr><td colspan='9'></td></tr>");

                    // KPI'lar
                    htw.Write("<tr><td><b>Toplam Fire:</b></td><td>" + litToplamFire.Text + " adet</td></tr>");
                    htw.Write("<tr><td><b>Ortalama Fire Oranı:</b></td><td>" + litFireOrani.Text + " %</td></tr>");
                    htw.Write("<tr><td colspan='9'></td></tr>");

                    // GridView
                    gvFireDetay.RenderControl(htw);

                    htw.Write("</table>");

                    Response.Write(sw.ToString());
                    Response.End();
                }
            }
        }

        public override void VerifyRenderingInServerForm(Control control)
        {
            // Excel export için gerekli
        }

        // Bu metodlar public olmalı - ASPX'ten çağrılıyor
        public string GetFireClass(int fireAdet, int uretimAdet)
        {
            decimal oran = (uretimAdet + fireAdet) > 0
                ? (decimal)fireAdet / (uretimAdet + fireAdet) * 100
                : 0;

            if (oran >= 10) return "fire-high";
            if (oran >= 5) return "fire-medium";
            return "fire-low";
        }

        public string GetFireOrani(int fireAdet, int uretimAdet)
        {
            decimal oran = (uretimAdet + fireAdet) > 0
                ? (decimal)fireAdet / (uretimAdet + fireAdet) * 100
                : 0;

            return oran.ToString("N2");
        }


    }
}