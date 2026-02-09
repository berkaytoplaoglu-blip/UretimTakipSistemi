<%@ Page Title="Gelişmiş Raporlar" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="RaporlarGelismis.aspx.cs" Inherits="UretimTakipSistemi.Admin.RaporlarGelismis" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script src="https://cdn.jsdelivr.net/npm/chart.js@4.4.0/dist/chart.umd.min.js"></script>
    <style>
        .page-header {
            margin-bottom: 30px;
        }
        .page-header h1 {
            color: #333;
            font-size: 28px;
            margin-bottom: 10px;
        }
        .report-categories {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
            gap: 20px;
            margin-bottom: 30px;
        }
        .category-card {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 25px;
            border-radius: 10px;
            cursor: pointer;
            transition: all 0.3s;
            text-align: center;
        }
        .category-card:hover {
            transform: translateY(-5px);
            box-shadow: 0 10px 30px rgba(102, 126, 234, 0.3);
        }
        .category-card.active {
            background: linear-gradient(135deg, #11998e 0%, #38ef7d 100%);
        }
        .category-icon {
            font-size: 48px;
            margin-bottom: 10px;
        }
        .category-title {
            font-size: 18px;
            font-weight: bold;
        }
        .filter-panel {
            background: #f9f9f9;
            padding: 25px;
            border-radius: 8px;
            margin-bottom: 30px;
            display: none;
        }
        .filter-panel.show {
            display: block;
        }
        .filter-row {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
            gap: 15px;
            margin-bottom: 15px;
        }
        .filter-group {
            display: flex;
            flex-direction: column;
        }
        .filter-group label {
            margin-bottom: 5px;
            font-weight: 500;
            font-size: 14px;
        }
        .filter-group input,
        .filter-group select {
            padding: 10px;
            border: 1px solid #ddd;
            border-radius: 4px;
        }
        .btn {
            padding: 10px 20px;
            border: none;
            border-radius: 4px;
            font-size: 14px;
            font-weight: 600;
            cursor: pointer;
        }
        .btn-primary {
            background: #1e3c72;
            color: white;
        }
        .btn-primary:hover {
            background: #2a5298;
        }
        .kpi-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
            gap: 20px;
            margin-bottom: 30px;
        }
        .kpi-card {
            background: white;
            padding: 20px;
            border-radius: 8px;
            border-left: 4px solid #1e3c72;
            box-shadow: 0 2px 8px rgba(0,0,0,0.1);
        }
        .kpi-card.success {
            border-left-color: #4caf50;
        }
        .kpi-card.warning {
            border-left-color: #ff9800;
        }
        .kpi-card.danger {
            border-left-color: #f44336;
        }
        .kpi-label {
            font-size: 13px;
            color: #666;
            margin-bottom: 8px;
        }
        .kpi-value {
            font-size: 32px;
            font-weight: bold;
            color: #333;
        }
        .kpi-unit {
            font-size: 14px;
            color: #999;
            margin-top: 5px;
        }
        .chart-container {
            background: white;
            padding: 25px;
            border-radius: 8px;
            margin-bottom: 20px;
            box-shadow: 0 2px 8px rgba(0,0,0,0.1);
        }
        .chart-title {
            font-size: 18px;
            font-weight: bold;
            color: #333;
            margin-bottom: 20px;
            padding-bottom: 10px;
            border-bottom: 2px solid #1e3c72;
        }
        .chart-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(400px, 1fr));
            gap: 20px;
        }
        table {
            width: 100%;
            border-collapse: collapse;
            background: white;
        }
        th {
            background: #1e3c72;
            color: white;
            padding: 12px;
            text-align: left;
            font-weight: 600;
        }
        td {
            padding: 12px;
            border-bottom: 1px solid #e0e0e0;
        }
        tr:hover {
            background: #f5f5f5;
        }
        .no-data {
            text-align: center;
            padding: 40px;
            color: #999;
            font-size: 16px;
        }
        @media print {
    .filter-panel,
    .btn,
    .nav-menu,
    .header {
        display: none !important;
    }
    
    .content-wrapper {
        box-shadow: none;
        padding: 0;
    }
    .verimlilik-table th:last-child {
        background: #4caf50 !important;
        font-size: 16px;
    }
    
    .verimlilik-table td:last-child {
        font-weight: bold !important;
        font-size: 18px !important;
        text-align: center !important;
    }
    
    .kpi-card,
    .chart-container {
        page-break-inside: avoid;
    }
    
    table {
        page-break-inside: auto;
    }
    
    tr {
        page-break-inside: avoid;
        page-break-after: auto;
    }
}
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="page-header">
        <h1>📊 Gelişmiş Analiz ve Raporlama</h1>
        <p>Detaylı üretim analizleri, verimlilik grafikleri ve performans raporları</p>
    </div>

    <div class="report-categories">
        <div class="category-card" data-category="verimlilik" onclick="selectCategory('verimlilik')">
    <div class="category-icon">⚡</div>
    <div class="category-title">Verimlilik Analizi</div>
</div>
<div class="category-card" data-category="makine" onclick="selectCategory('makine')">
    <div class="category-icon">🏭</div>
    <div class="category-title">Makine Performansı</div>
</div>
        <div class="category-card" onclick="selectCategory('personel')">
            <div class="category-icon">👥</div>
            <div class="category-title">Personel Performansı</div>
        </div>
        <div class="category-card" onclick="selectCategory('fire')">
            <div class="category-icon">🔥</div>
            <div class="category-title">Fire Analizi</div>
        </div>
        <div class="category-card" onclick="selectCategory('durus')">
            <div class="category-icon">⏸️</div>
            <div class="category-title">Duruş Analizi</div>
        </div>
        <div class="category-card" onclick="selectCategory('urun')">
            <div class="category-icon">📦</div>
            <div class="category-title">Ürün Bazlı Rapor</div>
        </div>
    </div>

    <asp:HiddenField ID="hfSelectedCategory" runat="server" Value="" />

    <div id="filterPanel" class="filter-panel">
        <div class="filter-row">
            <div class="filter-group">
                <label>Başlangıç Tarihi</label>
                <asp:TextBox ID="txtBaslangic" runat="server" TextMode="Date"></asp:TextBox>
            </div>
            <div class="filter-group">
                <label>Bitiş Tarihi</label>
                <asp:TextBox ID="txtBitis" runat="server" TextMode="Date"></asp:TextBox>
            </div>
            <div class="filter-group">
                <label>Bölüm</label>
                <asp:DropDownList ID="ddlBolum" runat="server">
                    <asp:ListItem Value="" Text="Tümü"></asp:ListItem>
                    <asp:ListItem Value="Enjeksiyon" Text="Enjeksiyon"></asp:ListItem>
                    <asp:ListItem Value="Boyahane" Text="Boyahane"></asp:ListItem>
                    <asp:ListItem Value="IcParca" Text="İç Parça"></asp:ListItem>
                </asp:DropDownList>
            </div>
        </div>
        <div>
            <asp:Button ID="btnRaporGetir" runat="server" CssClass="btn btn-primary" 
                Text="📊 Rapor Oluştur" OnClick="btnRaporGetir_Click" />
        </div>
        <div class="filter-row">
    <div class="filter-group">
        <label>Başlangıç Tarihi</label>
        <asp:TextBox ID="TextBox1" runat="server" TextMode="Date"></asp:TextBox>
    </div>
    <div class="filter-group">
        <label>Bitiş Tarihi</label>
        <asp:TextBox ID="TextBox2" runat="server" TextMode="Date"></asp:TextBox>
    </div>
    <div class="filter-group">
        <label>Bölüm</label>
        <asp:DropDownList ID="DropDownList1" runat="server">
            <asp:ListItem Value="" Text="Tümü"></asp:ListItem>
            <asp:ListItem Value="Enjeksiyon" Text="Enjeksiyon"></asp:ListItem>
            <asp:ListItem Value="Boyahane" Text="Boyahane"></asp:ListItem>
            <asp:ListItem Value="IcParca" Text="İç Parça"></asp:ListItem>
        </asp:DropDownList>
    </div>
</div>
<div style="display: flex; gap: 10px;">
    <asp:Button ID="Button1" runat="server" CssClass="btn btn-primary" 
        Text="📊 Rapor Oluştur" OnClick="btnRaporGetir_Click" />
    <asp:Button ID="btnPrintPreview" runat="server" CssClass="btn btn-warning" 
        Text="🖨️ Yazdırma Önizleme" OnClientClick="window.print(); return false;" 
        Visible="false" />
  
</div>
    </div>

    <asp:Panel ID="pnlRapor" runat="server" Visible="false">
    <!-- KPI Kartları -->
    <div class="kpi-grid" id="kpiGrid" runat="server"></div>

    <!-- Grafikler -->
    <div class="chart-grid">
        <div class="chart-container">
            <div class="chart-title">Günlük Üretim Trendi</div>
            <canvas id="chartTrend"></canvas>
        </div>
        <div class="chart-container">
            <div class="chart-title">Bölüm Bazlı Dağılım</div>
            <canvas id="chartDistribution"></canvas>
        </div>
    </div>

    <!-- Detaylı Tablo -->
    <div class="chart-container">
        <div class="chart-title">Detaylı Veri Tablosu</div>
        <asp:GridView ID="gvDetay" runat="server" AutoGenerateColumns="true"></asp:GridView>
    </div>

    <asp:Literal ID="litChartData" runat="server"></asp:Literal>
</asp:Panel>

<script type="text/javascript">
    function selectCategory(category) {
        var cards = document.querySelectorAll('.category-card');
        cards.forEach(function (card) {
            card.classList.remove('active');
        });

        // this kullanarak tıklanan elementi bul (onclick içinde this kullan)
        var allCards = document.querySelectorAll('.category-card');
        for (var i = 0; i < allCards.length; i++) {
            if (allCards[i].getAttribute('data-category') === category) {
                allCards[i].classList.add('active');
                break;
            }
        }

        document.getElementById('<%= hfSelectedCategory.ClientID %>').value = category;
        document.getElementById('filterPanel').classList.add('show');
    }

    window.onload = function () {
        var today = new Date().toISOString().split('T')[0];
        var weekAgo = new Date();
        weekAgo.setDate(weekAgo.getDate() - 7);
        var weekAgoStr = weekAgo.toISOString().split('T')[0];

        document.getElementById('<%= txtBaslangic.ClientID %>').value = weekAgoStr;
        document.getElementById('<%= txtBitis.ClientID %>').value = today;
    };
</script>

</asp:Content>