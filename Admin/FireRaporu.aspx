<%@ Page Title="Fire Detay Raporu" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="FireRaporu.aspx.cs" Inherits="UretimTakipSistemi.Admin.FireRaporu" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script src="https://cdn.jsdelivr.net/npm/chart.js@4.4.0/dist/chart.umd.min.js"></script>
    <style>
        .page-header {
            margin-bottom: 30px;
        }
        .page-header h1 {
            color: #333;
            font-size: 28px;
        }
        .filter-panel {
            background: #f9f9f9;
            padding: 25px;
            border-radius: 8px;
            margin-bottom: 30px;
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
            margin-right: 10px;
        }
        .btn-primary {
            background: #1e3c72;
            color: white;
        }
        .btn-primary:hover {
            background: #2a5298;
        }
        .btn-success {
            background: #4caf50;
            color: white;
        }
        .btn-success:hover {
            background: #45a049;
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
            border-left: 4px solid #f44336;
            box-shadow: 0 2px 8px rgba(0,0,0,0.1);
        }
        .kpi-label {
            font-size: 13px;
            color: #666;
            margin-bottom: 8px;
        }
        .kpi-value {
            font-size: 32px;
            font-weight: bold;
            color: #f44336;
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
            border-bottom: 2px solid #f44336;
        }
        .chart-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(400px, 1fr));
            gap: 20px;
            margin-bottom: 20px;
        }
        table {
            width: 100%;
            border-collapse: collapse;
            background: white;
        }
        th {
            background: #f44336;
            color: white;
            padding: 12px;
            text-align: left;
            font-weight: 600;
            font-size: 14px;
        }
        td {
            padding: 12px;
            border-bottom: 1px solid #e0e0e0;
            font-size: 14px;
        }
        tr:hover {
            background: #ffebee;
        }
        .fire-high {
            color: #f44336;
            font-weight: bold;
        }
        .fire-medium {
            color: #ff9800;
            font-weight: bold;
        }
        .fire-low {
            color: #4caf50;
            font-weight: bold;
        }
        .no-data {
            text-align: center;
            padding: 40px;
            color: #999;
            font-size: 16px;
        }
        <style>
    /* Mevcut CSS'lerin sonuna ekle */
    
    .chart-container {
        background: white;
        padding: 25px;
        border-radius: 8px;
        margin-bottom: 20px;
        box-shadow: 0 2px 8px rgba(0,0,0,0.1);
    }
    
    /* Yuvarlak grafik için özel container */
    .chart-container.doughnut-chart {
        max-width: 500px;
        margin-left: auto;
        margin-right: auto;
    }
    
    .chart-title {
        font-size: 18px;
        font-weight: bold;
        color: #333;
        margin-bottom: 20px;
        padding-bottom: 10px;
        border-bottom: 2px solid #f44336;
    }
    
    .chart-grid {
        display: grid;
        grid-template-columns: repeat(auto-fit, minmax(400px, 1fr));
        gap: 20px;
        margin-bottom: 20px;
    }
    
    /* Canvas boyutu */
    #chartNeden {
        max-height: 350px !important;
    }
</style>
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="page-header">
        <h1>🔥 Fire Detay Raporu</h1>
        <p>Personel, makine ve ürün bazlı fire analizi</p>
    </div>

    <div class="filter-panel">
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
                <label>Personel</label>
                <asp:DropDownList ID="ddlPersonel" runat="server"></asp:DropDownList>
            </div>
            <div class="filter-group">
                <label>Makine</label>
                <asp:DropDownList ID="ddlMakine" runat="server"></asp:DropDownList>
            </div>
        </div>
        <div>
            <asp:Button ID="btnRaporGetir" runat="server" CssClass="btn btn-primary" 
                Text="📊 Rapor Getir" OnClick="btnRaporGetir_Click" />
            <asp:Button ID="btnExcelAktar" runat="server" CssClass="btn btn-success" 
                Text="📥 Excel'e Aktar" OnClick="btnExcelAktar_Click" />
        </div>
    </div>

    <asp:Panel ID="pnlRapor" runat="server" Visible="false">
        <!-- KPI Kartları -->
        <div class="kpi-grid">
            <div class="kpi-card">
                <div class="kpi-label">Toplam Fire</div>
                <div class="kpi-value"><asp:Literal ID="litToplamFire" runat="server"></asp:Literal></div>
                <div class="kpi-unit">adet</div>
            </div>
            <div class="kpi-card">
                <div class="kpi-label">Ortalama Fire Oranı</div>
                <div class="kpi-value"><asp:Literal ID="litFireOrani" runat="server"></asp:Literal></div>
                <div class="kpi-unit">%</div>
            </div>
            <div class="kpi-card">
                <div class="kpi-label">En Çok Fire Yapan Personel</div>
                <div class="kpi-value" style="font-size:18px;"><asp:Literal ID="litTopPersonel" runat="server"></asp:Literal></div>
                <div class="kpi-unit"><asp:Literal ID="litTopPersonelFire" runat="server"></asp:Literal> adet</div>
            </div>
            <div class="kpi-card">
                <div class="kpi-label">En Çok Fire Olan Makine</div>
                <div class="kpi-value" style="font-size:18px;"><asp:Literal ID="litTopMakine" runat="server"></asp:Literal></div>
                <div class="kpi-unit"><asp:Literal ID="litTopMakineFire" runat="server"></asp:Literal> adet</div>
            </div>
        </div>

        <!-- Grafikler -->
        <div class="chart-grid">
            <div class="chart-container">
                <div class="chart-title">Personel Bazlı Fire Dağılımı</div>
                <canvas id="chartPersonel"></canvas>
            </div>
            <div class="chart-container">
                <div class="chart-title">Makine Bazlı Fire Dağılımı</div>
                <canvas id="chartMakine"></canvas>
            </div>
        </div>

        <div class="chart-container">
            <div class="chart-title">Fire Nedenleri Dağılımı</div>
            <canvas id="chartNeden"></canvas>
        </div>

        <!-- Detaylı Tablo -->
        <div class="chart-container">
            <div class="chart-title">Detaylı Fire Kayıtları</div>
            <asp:GridView ID="gvFireDetay" runat="server" AutoGenerateColumns="false">
                <Columns>
                    <asp:BoundField DataField="Tarih" HeaderText="Tarih" DataFormatString="{0:dd.MM.yyyy}" />
                    <asp:BoundField DataField="Personel" HeaderText="Personel" />
                    <asp:BoundField DataField="Makine" HeaderText="Makine" />
                    <asp:BoundField DataField="IsEmriNo" HeaderText="İş Emri" />
                    <asp:BoundField DataField="UrunAdi" HeaderText="Ürün" />
                    <asp:TemplateField HeaderText="Fire Adet">
                        <ItemTemplate>
                            <span class='<%# GetFireClass(Convert.ToInt32(Eval("FireAdet")), Convert.ToInt32(Eval("UretimAdet"))) %>'>
                                <%# Eval("FireAdet") %>
                            </span>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="UretimAdet" HeaderText="Üretim Adet" DataFormatString="{0:N0}" />
                    <asp:TemplateField HeaderText="Fire Oranı">
                        <ItemTemplate>
                            <span class='<%# GetFireClass(Convert.ToInt32(Eval("FireAdet")), Convert.ToInt32(Eval("UretimAdet"))) %>'>
                                <%# GetFireOrani(Convert.ToInt32(Eval("FireAdet")), Convert.ToInt32(Eval("UretimAdet"))) %>%
                            </span>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="FireNedeni" HeaderText="Fire Nedeni" />
                </Columns>
            </asp:GridView>

            <asp:Panel ID="pnlVeriYok" runat="server" Visible="false" CssClass="no-data">
                Seçili kriterlere uygun fire kaydı bulunamadı.
            </asp:Panel>
        </div>

        <asp:Literal ID="litChartScript" runat="server"></asp:Literal>
    </asp:Panel>

    <script type="text/javascript">
        window.onload = function() {
            var today = new Date().toISOString().split('T')[0];
            var monthAgo = new Date();
            monthAgo.setMonth(monthAgo.getMonth() - 1);
            var monthAgoStr = monthAgo.toISOString().split('T')[0];
            
            document.getElementById('<%= txtBaslangic.ClientID %>').value = monthAgoStr;
            document.getElementById('<%= txtBitis.ClientID %>').value = today;
        };
    </script>
</asp:Content>