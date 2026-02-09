<%@ Page Title="Raporlar" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Raporlar.aspx.cs" Inherits="UretimTakipSistemi.Admin.Raporlar" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .page-header {
            margin-bottom: 30px;
        }
        .page-header h1 {
            color: #333;
            font-size: 28px;
        }
        .section-cards {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
            gap: 20px;
            margin-bottom: 30px;
        }
        .section-card {
            background: white;
            border: 2px solid #e0e0e0;
            border-radius: 10px;
            padding: 25px;
            cursor: pointer;
            transition: all 0.3s;
        }
        .section-card:hover {
            border-color: #1e3c72;
            box-shadow: 0 4px 15px rgba(30, 60, 114, 0.2);
        }
        .section-card.active {
            border-color: #1e3c72;
            background: #f0f4ff;
        }
        .section-icon {
            font-size: 48px;
            margin-bottom: 15px;
        }
        .section-title {
            font-size: 20px;
            font-weight: bold;
            color: #333;
            margin-bottom: 8px;
        }
        .section-desc {
            font-size: 14px;
            color: #666;
        }
        .report-filters {
            background: #f9f9f9;
            padding: 25px;
            border-radius: 8px;
            margin-bottom: 20px;
            display: none;
        }
        .report-filters.show {
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
        .report-results {
            margin-top: 20px;
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
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="page-header">
        <h1>📈 Raporlar</h1>
    </div>

    <div class="section-cards">
        <div class="section-card" onclick="selectSection('boyahane')">
            <div class="section-icon">🎨</div>
            <div class="section-title">Boyahane</div>
            <div class="section-desc">Boyahane üretim raporları</div>
        </div>
        <div class="section-card" onclick="selectSection('icparca')">
            <div class="section-icon">🔧</div>
            <div class="section-title">İç Parça</div>
            <div class="section-desc">İç parça üretim raporları</div>
        </div>
        <div class="section-card" onclick="selectSection('enjeksiyon')">
            <div class="section-icon">⚙️</div>
            <div class="section-title">Enjeksiyon</div>
            <div class="section-desc">Enjeksiyon üretim raporları</div>
        </div>
    </div>

    <asp:HiddenField ID="hfSeciliBolum" runat="server" Value="" />

    <div id="reportFilters" class="report-filters">
        <h3 style="margin-bottom: 20px;">Filtreler</h3>
        
        <div class="filter-row">
            <div class="filter-group">
                <label>Başlangıç Tarihi</label>
                <asp:TextBox ID="txtBaslangic" runat="server" TextMode="Date"></asp:TextBox>
            </div>
            <div class="filter-group">
                <label>Bitiş Tarihi</label>
                <asp:TextBox ID="txtBitis" runat="server" TextMode="Date"></asp:TextBox>
            </div>
        </div>

        <div>
            <asp:Button ID="btnRaporGetir" runat="server" CssClass="btn btn-primary" 
                Text="📊 Rapor Getir" OnClick="btnRaporGetir_Click" />
        </div>

    </div>
    <div class="report-results">
    <div style="margin-bottom: 20px; display: flex; justify-content: space-between; align-items: center;">
        <h3 style="margin: 0;">
            <asp:Literal ID="Literal1" runat="server"></asp:Literal>
        </h3>
        <asp:Button ID="btnExportExcel" runat="server" CssClass="btn btn-success" 
            Text="📥 Excel'e Aktar" OnClick="btnExportExcel_Click" Visible="false" />
    </div>
    
    <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="true">
    </asp:GridView>
    
    <asp:Panel ID="Panel1" runat="server" Visible="false" CssClass="no-data">
        Seçili tarih aralığında veri bulunamadı.
    </asp:Panel>
</div>
    <asp:Panel ID="pnlRapor" runat="server" CssClass="report-results" Visible="false">
        <h3 style="margin-bottom: 20px;">
            <asp:Literal ID="litRaporBaslik" runat="server"></asp:Literal>
        </h3>
        
        <asp:GridView ID="gvRapor" runat="server" AutoGenerateColumns="true">
        </asp:GridView>
        
        <asp:Panel ID="pnlVeriYok" runat="server" Visible="false" CssClass="no-data">
            Seçili tarih aralığında veri bulunamadı.
        </asp:Panel>
    </asp:Panel>

    <script type="text/javascript">
        function selectSection(section) {
            // event parametresini kaldırdık
            var clickedCard = null;

            // Tıklanan kartı bul
            var cards = document.querySelectorAll('.section-card');
            cards.forEach(function (card) {
                if (card.onclick && card.onclick.toString().includes(section)) {
                    clickedCard = card;
                }
                card.classList.remove('active');
            });

            if (clickedCard) {
                clickedCard.classList.add('active');
            }

            document.getElementById('<%= hfSeciliBolum.ClientID %>').value = section;
        document.getElementById('reportFilters').classList.add('show');

        var reportPanel = document.getElementById('<%= pnlRapor.ClientID %>');
        if (reportPanel) {
            reportPanel.style.display = 'none';
        }
    }

    window.onload = function() {
        var today = new Date().toISOString().split('T')[0];
        document.getElementById('<%= txtBaslangic.ClientID %>').value = today;
        document.getElementById('<%= txtBitis.ClientID %>').value = today;
        };
    </script>
</asp:Content>