<%@ Page Title="Dashboard" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Dashboard.aspx.cs" Inherits="UretimTakipSistemi.Admin.Dashboard" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .dashboard-header {
            margin-bottom: 30px;
        }
        .dashboard-header h1 {
            color: #333;
            font-size: 32px;
            margin-bottom: 5px;
        }
        .dashboard-header p {
            color: #666;
            font-size: 14px;
        }
        .kpi-container {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
            gap: 20px;
            margin-bottom: 40px;
        }
        .kpi-card {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 25px;
            border-radius: 10px;
            box-shadow: 0 4px 15px rgba(0,0,0,0.1);
        }
        .kpi-card.green {
            background: linear-gradient(135deg, #11998e 0%, #38ef7d 100%);
        }
        .kpi-card.orange {
            background: linear-gradient(135deg, #F09819 0%, #EDDE5D 100%);
        }
        .kpi-card.red {
            background: linear-gradient(135deg, #ee0979 0%, #ff6a00 100%);
        }
        .kpi-card.blue {
            background: linear-gradient(135deg, #2193b0 0%, #6dd5ed 100%);
        }
        .kpi-label {
            font-size: 14px;
            opacity: 0.9;
            margin-bottom: 10px;
        }
        .kpi-value {
            font-size: 36px;
            font-weight: bold;
        }
        .machine-grid {
            display: grid;
            grid-template-columns: repeat(6, 1fr);
            gap: 15px;
        }
        .machine-card {
            background: white;
            border: 2px solid #e0e0e0;
            border-radius: 8px;
            padding: 15px;
            text-align: center;
            transition: all 0.3s;
        }
        .machine-card.idle {
            border-color: #ccc;
            background: #f5f5f5;
        }
        .machine-card.active {
            border-color: #4caf50;
            background: #e8f5e9;
        }
        .machine-card.paused {
            border-color: #ff9800;
            background: #fff3e0;
        }
        .machine-name {
            font-weight: bold;
            font-size: 18px;
            margin-bottom: 10px;
            color: #333;
        }
        .machine-status {
            padding: 5px 10px;
            border-radius: 20px;
            font-size: 12px;
            font-weight: 600;
            display: inline-block;
            margin-bottom: 8px;
        }
        .status-idle {
            background: #e0e0e0;
            color: #666;
        }
        .status-active {
            background: #4caf50;
            color: white;
        }
        .status-paused {
            background: #ff9800;
            color: white;
        }
        .machine-detail {
            font-size: 11px;
            color: #666;
            margin-top: 5px;
        }
        .refresh-info {
            text-align: center;
            color: #999;
            font-size: 12px;
            margin-top: 20px;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="dashboard-header">
        <h1>📊 Dashboard</h1>
        <p>Gerçek zamanlı üretim takibi</p>
    </div>

    <div class="kpi-container">
        <div class="kpi-card green">
            <div class="kpi-label">Toplam Üretim (Bugün)</div>
            <div class="kpi-value" id="kpiUretim">0</div>
        </div>
        <div class="kpi-card red">
            <div class="kpi-label">Toplam Fire (Bugün)</div>
            <div class="kpi-value" id="kpiFire">0</div>
        </div>
        <div class="kpi-card orange">
            <div class="kpi-label">Fire Oranı (%)</div>
            <div class="kpi-value" id="kpiFireOrani">0.0</div>
        </div>
        <div class="kpi-card blue">
            <div class="kpi-label">Aktif Makine</div>
            <div class="kpi-value" id="kpiMakine">0/0</div>
        </div>
    </div>

    <h2 style="margin-bottom: 20px; color: #333;">Enjeksiyon Makineleri</h2>
    <div class="machine-grid" id="machineGrid">
    </div>

    <div class="refresh-info">
        🔄 Otomatik güncelleme: Her 10 saniyede bir
    </div>

    <script type="text/javascript">
        var updateInterval;

        function loadDashboard() {
            PageMethods.GetDashboardData(onDashboardSuccess, onDashboardError);
        }

        function onDashboardSuccess(result) {
            if (result) {
                var data = JSON.parse(result);
                
                document.getElementById('kpiUretim').textContent = data.ToplamUretim;
                document.getElementById('kpiFire').textContent = data.ToplamFire;
                document.getElementById('kpiFireOrani').textContent = data.FireOrani.toFixed(2);
                document.getElementById('kpiMakine').textContent = data.AktifMakine + '/' + data.ToplamMakine;
                
                var grid = document.getElementById('machineGrid');
                grid.innerHTML = '';
                
                data.Makineler.forEach(function(m) {
                    var card = document.createElement('div');
                    card.className = 'machine-card ' + m.Durum.toLowerCase();
                    
                    var statusClass = 'status-idle';
                    var statusText = 'BEKLEME';
                    
                    if (m.Durum === 'ACTIVE') {
                        statusClass = 'status-active';
                        statusText = 'ÇALIŞIYOR';
                    } else if (m.Durum === 'PAUSED') {
                        statusClass = 'status-paused';
                        statusText = 'DURUŞTA';
                    }
                    
                    var html = '<div class="machine-name">' + m.MakineAdi + '</div>';
                    html += '<div class="machine-status ' + statusClass + '">' + statusText + '</div>';
                    
                    if (m.Personel) {
                        html += '<div class="machine-detail">👤 ' + m.Personel + '</div>';
                    }
                    if (m.IsEmriNo) {
                        html += '<div class="machine-detail">📋 ' + m.IsEmriNo + '</div>';
                    }
                    
                    card.innerHTML = html;
                    grid.appendChild(card);
                });
            }
        }

        function onDashboardError(error) {
            console.error('Dashboard yükleme hatası:', error);
        }

        window.onload = function() {
            loadDashboard();
            updateInterval = setInterval(loadDashboard, 10000);
        };

        window.onbeforeunload = function() {
            if (updateInterval) {
                clearInterval(updateInterval);
            }
        };
    </script>
</asp:Content>