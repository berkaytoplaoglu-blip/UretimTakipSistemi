<%@ Page Title="İş Emri Aç" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="IsEmriAc.aspx.cs" Inherits="UretimTakipSistemi.Admin.IsEmriAc" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .page-header {
            margin-bottom: 30px;
        }
        .page-header h1 {
            color: #333;
            font-size: 28px;
            margin-bottom: 10px;
        }
        .form-card {
            background: #f9f9f9;
            border: 1px solid #e0e0e0;
            border-radius: 8px;
            padding: 25px;
            margin-bottom: 30px;
        }
        .section-title {
            color: #1e3c72;
            font-size: 18px;
            font-weight: 600;
            margin-bottom: 15px;
            padding-bottom: 10px;
            border-bottom: 2px solid #1e3c72;
        }
        .form-row {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
            gap: 15px;
            margin-bottom: 15px;
        }
        .form-group {
            display: flex;
            flex-direction: column;
        }
        .form-group label {
            margin-bottom: 5px;
            color: #333;
            font-weight: 500;
            font-size: 14px;
        }
        .form-group input,
        .form-group select {
            padding: 10px;
            border: 1px solid #ddd;
            border-radius: 4px;
            font-size: 14px;
        }
        .form-group input:focus,
        .form-group select:focus {
            outline: none;
            border-color: #1e3c72;
        }
        .search-container {
            position: relative;
        }
        .search-results {
            position: absolute;
            top: 100%;
            left: 0;
            right: 0;
            background: white;
            border: 1px solid #ddd;
            border-top: none;
            max-height: 300px;
            overflow-y: auto;
            z-index: 1000;
            display: none;
        }
        .search-results.show {
            display: block;
        }
        .search-item {
            padding: 12px;
            cursor: pointer;
            border-bottom: 1px solid #eee;
        }
        .search-item:hover {
            background: #f0f0f0;
        }
        .search-item-code {
            font-weight: bold;
            color: #1e3c72;
            margin-bottom: 3px;
        }
        .search-item-name {
            font-size: 13px;
            color: #666;
        }
        .selected-parca {
            background: #e8f5e9;
            border: 2px solid #4caf50;
            padding: 15px;
            border-radius: 8px;
            margin-bottom: 20px;
        }
        .selected-parca-title {
            font-weight: bold;
            color: #2e7d32;
            margin-bottom: 8px;
        }
        .selected-parca-detail {
            font-size: 14px;
            color: #333;
            margin-bottom: 5px;
        }
        .checkbox-group {
            display: flex;
            gap: 20px;
            margin-bottom: 20px;
        }
        .checkbox-item {
            display: flex;
            align-items: center;
            gap: 8px;
        }
        .checkbox-item input[type="checkbox"] {
            width: 20px;
            height: 20px;
            cursor: pointer;
        }
        .checkbox-item label {
            cursor: pointer;
            font-size: 15px;
            font-weight: 500;
        }
        .enjeksiyon-section {
            background: #fff3e0;
            border: 2px solid #ff9800;
            padding: 20px;
            border-radius: 8px;
            margin-top: 20px;
            display: none;
        }
        .enjeksiyon-section.show {
            display: block;
        }
        .btn {
            padding: 12px 25px;
            border: none;
            border-radius: 4px;
            font-size: 15px;
            font-weight: 600;
            cursor: pointer;
            transition: all 0.3s;
        }
        .btn-success {
            background: #4caf50;
            color: white;
        }
        .btn-success:hover {
            background: #45a049;
        }
        .btn-warning {
            background: #ff9800;
            color: white;
        }
        .btn-warning:hover {
            background: #fb8c00;
        }
        .button-group {
            display: flex;
            gap: 10px;
            margin-top: 20px;
        }
        .alert {
            padding: 12px 20px;
            border-radius: 4px;
            margin-bottom: 20px;
        }
        .alert-success {
            background: #d4edda;
            border: 1px solid #c3e6cb;
            color: #155724;
        }
        .alert-error {
            background: #f8d7da;
            border: 1px solid #f5c6cb;
            color: #721c24;
        }
        .required {
            color: red;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="page-header">
        <h1>➕ Yeni İş Emri Aç</h1>
    </div>

    <asp:Panel ID="pnlMesaj" runat="server" Style="display:none;">
        <asp:Literal ID="litMesaj" runat="server"></asp:Literal>
    </asp:Panel>

    <div class="form-card">
        <div class="section-title">1️⃣ İş Emri Bilgileri</div>
        
        <div class="form-row">
            <div class="form-group">
                <label>İş Emri No <span class="required">*</span></label>
                <asp:TextBox ID="txtIsEmriNo" runat="server" MaxLength="50" placeholder="IE-2024-001"></asp:TextBox>
            </div>
        </div>

        <div class="section-title">2️⃣ Ürün Seçimi</div>
        
        <div class="form-group">
            <label>Ürün Ara (Parça Kodu veya Ürün Adı) <span class="required">*</span></label>
            <div class="search-container">
                <asp:TextBox ID="txtUrunAra" runat="server" placeholder="PRC-001 veya Kapak Parçası..." 
                    AutoPostBack="false" onkeyup="aramaYap(this.value)"></asp:TextBox>
                <div id="searchResults" class="search-results"></div>
            </div>
        </div>

        <asp:HiddenField ID="hfParcaId" runat="server" Value="0" />
        
        <asp:Panel ID="pnlSeciliParca" runat="server" Visible="false" CssClass="selected-parca">
            <div class="selected-parca-title">✅ Seçili Ürün:</div>
            <div class="selected-parca-detail">
                <strong>Parça Kodu:</strong> <asp:Literal ID="litParcaKodu" runat="server"></asp:Literal>
            </div>
            <div class="selected-parca-detail">
                <strong>Ürün Adı:</strong> <asp:Literal ID="litUrunAdi" runat="server"></asp:Literal>
            </div>
            <div class="selected-parca-detail">
                <strong>Gramı:</strong> <asp:Literal ID="litGrami" runat="server"></asp:Literal> gr
            </div>
            <div class="selected-parca-detail">
                <strong>Kalıp No:</strong> <asp:Literal ID="litKalipNo" runat="server"></asp:Literal>
            </div>
        </asp:Panel>

        <div class="section-title">3️⃣ Hangi Bölümlerde İşlenecek?</div>
        
        <div class="checkbox-group">
            <div class="checkbox-item">
                <asp:CheckBox ID="chkBoyahane" runat="server" />
                <label for="<%= chkBoyahane.ClientID %>">🎨 Boyahane</label>
            </div>
            <div class="checkbox-item">
                <asp:CheckBox ID="chkIcParca" runat="server" />
                <label for="<%= chkIcParca.ClientID %>">🔧 İç Parça</label>
            </div>
            <div class="checkbox-item">
                <asp:CheckBox ID="chkEnjeksiyon" runat="server" onclick="toggleEnjeksiyonSection()" />
                <label for="<%= chkEnjeksiyon.ClientID %>">⚙️ Enjeksiyon</label>
            </div>
        </div>

        <div id="enjeksiyonSection" class="enjeksiyon-section">
            <div class="section-title">⚙️ Enjeksiyon Süre Bilgileri</div>
            
            <div class="form-row">
                <div class="form-group">
                    <label>Çevrim Süresi (saniye) <span class="required">*</span></label>
                    <asp:TextBox ID="txtCevrimSuresi" runat="server" placeholder="45"></asp:TextBox>
                </div>
                <div class="form-group">
                    <label>Soğuma Süresi (saniye) <span class="required">*</span></label>
                    <asp:TextBox ID="txtSogumaSuresi" runat="server" placeholder="30"></asp:TextBox>
                </div>
            </div>
        </div>

        <div class="button-group">
            <asp:Button ID="btnKaydet" runat="server" CssClass="btn btn-success" Text="💾 İş Emrini Aç" OnClick="btnKaydet_Click" />
            <asp:Button ID="btnTemizle" runat="server" CssClass="btn btn-warning" Text="🔄 Temizle" OnClick="btnTemizle_Click" />
        </div>
    </div>

    <script type="text/javascript">
        var searchTimeout;

        function aramaYap(value) {
            clearTimeout(searchTimeout);
            
            if (value.length < 2) {
                document.getElementById('searchResults').classList.remove('show');
                return;
            }

            searchTimeout = setTimeout(function() {
                PageMethods.ParcaAra(value, onSearchSuccess, onSearchError);
            }, 300);
        }

        function onSearchSuccess(result) {
            var container = document.getElementById('searchResults');
            
            if (result && result.length > 0) {
                var html = '';
                result.forEach(function(item) {
                    html += '<div class="search-item" onclick="parcaSec(' + item.ParcaId + ', \'' + 
                            escapeHtml(item.UrunParcaKodu) + '\', \'' + 
                            escapeHtml(item.UrunAdi) + '\', ' + 
                            item.Grami + ', \'' + 
                            escapeHtml(item.KalipNo || '') + '\')">';
                    html += '<div class="search-item-code">' + item.UrunParcaKodu + '</div>';
                    html += '<div class="search-item-name">' + item.UrunAdi + ' (' + item.Grami + ' gr)</div>';
                    html += '</div>';
                });
                container.innerHTML = html;
                container.classList.add('show');
            } else {
                container.innerHTML = '<div class="search-item">Sonuç bulunamadı</div>';
                container.classList.add('show');
            }
        }

        function onSearchError(error) {
            console.error('Arama hatası:', error);
        }

        function parcaSec(parcaId, kod, ad, grami, kalipNo) {
            document.getElementById('<%= hfParcaId.ClientID %>').value = parcaId;
            document.getElementById('<%= txtUrunAra.ClientID %>').value = kod + ' - ' + ad;
            document.getElementById('searchResults').classList.remove('show');
            
            __doPostBack('<%= btnGetirParca.UniqueID %>', '');
        }

        function toggleEnjeksiyonSection() {
            var checkbox = document.getElementById('<%= chkEnjeksiyon.ClientID %>');
            var section = document.getElementById('enjeksiyonSection');
            
            if (checkbox.checked) {
                section.classList.add('show');
            } else {
                section.classList.remove('show');
            }
        }

        function escapeHtml(text) {
            if (!text) return '';
            var map = {
                '&': '&amp;',
                '<': '&lt;',
                '>': '&gt;',
                '"': '&quot;',
                "'": '&#039;'
            };
            return text.replace(/[&<>"']/g, function(m) { return map[m]; });
        }

        window.onload = function() {
            toggleEnjeksiyonSection();
        };

        document.addEventListener('click', function(e) {
            var searchContainer = document.querySelector('.search-container');
            if (searchContainer && !searchContainer.contains(e.target)) {
                document.getElementById('searchResults').classList.remove('show');
            }
        });
    </script>

    <asp:Button ID="btnGetirParca" runat="server" OnClick="btnGetirParca_Click" style="display:none;" />
</asp:Content>