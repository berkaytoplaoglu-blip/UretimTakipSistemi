<%@ Page Title="Parça Listesi" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ParcaListesi.aspx.cs" Inherits="UretimTakipSistemi.Admin.ParcaListesi" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .page-header {
            margin-bottom: 30px;
        }
        .page-header h1 {
            color: #333;
            font-size: 28px;
        }
        .action-bar {
            display: flex;
            gap: 15px;
            margin-bottom: 20px;
            flex-wrap: wrap;
        }
        .btn {
            padding: 10px 20px;
            border: none;
            border-radius: 4px;
            font-size: 14px;
            font-weight: 600;
            cursor: pointer;
            transition: all 0.3s;
            text-decoration: none;
            display: inline-block;
        }
        .btn-success {
            background: #4caf50;
            color: white;
        }
        .btn-success:hover {
            background: #45a049;
        }
        .btn-primary {
            background: #1e3c72;
            color: white;
        }
        .btn-primary:hover {
            background: #2a5298;
        }
        .btn-secondary {
            background: #666;
            color: white;
        }
        .btn-warning {
            background: #ff9800;
            color: white;
            font-size: 12px;
            padding: 6px 12px;
        }
        .btn-danger {
            background: #f44336;
            color: white;
            font-size: 12px;
            padding: 6px 12px;
        }
        table {
            width: 100%;
            border-collapse: collapse;
            background: white;
            box-shadow: 0 2px 8px rgba(0,0,0,0.1);
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
        
        /* Manuel Ekleme Formu */
        .form-card {
            background: #e3f2fd;
            border: 2px solid #2196f3;
            border-radius: 8px;
            padding: 25px;
            margin-bottom: 25px;
        }
        .form-card h3 {
            margin-top: 0;
            color: #1565c0;
            font-size: 20px;
        }
        .form-row {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
            gap: 15px;
            margin-bottom: 20px;
        }
        .form-group {
            display: flex;
            flex-direction: column;
        }
        .form-group label {
            margin-bottom: 5px;
            font-weight: 600;
            color: #333;
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
            border-color: #2196f3;
        }
        
        /* Excel Yükleme */
        .upload-section {
            background: #fff3cd;
            border: 2px solid #ffc107;
            border-radius: 8px;
            padding: 25px;
            margin-bottom: 25px;
        }
        .upload-section h3 {
            margin-top: 0;
            color: #856404;
            font-size: 20px;
        }
        .file-input {
            margin: 15px 0;
            display: flex;
            gap: 10px;
            flex-wrap: wrap;
            align-items: center;
        }
        .file-input input[type="file"] {
            padding: 10px;
            border: 2px solid #ddd;
            border-radius: 4px;
            flex: 1;
            min-width: 250px;
        }
        .alert {
            padding: 15px 20px;
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
        .sample-format {
            background: white;
            padding: 20px;
            border-radius: 4px;
            margin-top: 15px;
            border: 1px solid #ddd;
        }
        .sample-format h4 {
            margin-top: 0;
            color: #333;
        }
        .sample-format code {
            background: #f5f5f5;
            padding: 3px 8px;
            border-radius: 3px;
            font-family: 'Courier New', monospace;
            color: #d63384;
        }
        @media (max-width: 768px) {
            .action-bar, .form-row, .file-input {
                flex-direction: column;
            }
            .file-input input[type="file"],
            .file-input .btn {
                width: 100%;
            }
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="page-header">
        <h1>Parca Listesi Yonetimi</h1>
        <p>Uretim parcalarini goruntule, manuel ekle veya Excel'den toplu yukle</p>
    </div>

    <asp:Panel ID="pnlMesaj" runat="server" Visible="false">
        <asp:Literal ID="litMesaj" runat="server"></asp:Literal>
    </asp:Panel>

    <!-- MANUEL EKLEME FORMU -->
    <div class="form-card">
        <h3>+ Yeni Parca Ekle (Manuel)</h3>
        <p>Tek bir parca bilgisi manuel olarak ekleyin</p>
        
        <div class="form-row">
            <div class="form-group">
                <label>Parca Kodu *</label>
                <asp:TextBox ID="txtParcaKodu" runat="server" placeholder="P001"></asp:TextBox>
            </div>
            <div class="form-group">
                <label>Urun Adi *</label>
                <asp:TextBox ID="txtUrunAdi" runat="server" placeholder="Kapak Parcasi"></asp:TextBox>
            </div>
            <div class="form-group">
                <label>Grami</label>
                <asp:TextBox ID="txtGrami" runat="server" TextMode="Number" placeholder="125.50"></asp:TextBox>
            </div>
            <div class="form-group">
                <label>Kalip No</label>
                <asp:TextBox ID="txtKalipNo" runat="server" placeholder="K-2024-001"></asp:TextBox>
            </div>
        </div>
        
        <div>
            <asp:Button ID="btnManuelKaydet" runat="server" CssClass="btn btn-success" 
                Text="Kaydet" OnClick="btnManuelKaydet_Click" />
            <asp:Button ID="btnTemizle" runat="server" CssClass="btn btn-secondary" 
                Text="Temizle" OnClick="btnTemizle_Click" />
        </div>
    </div>

    <!-- EXCEL YÜKLEME -->
    <div class="upload-section">
        <h3>Excel'den Toplu Parca Yukleme</h3>
        <p>Excel dosyasindan toplu parca verisi yukleyin. Var olan kayitlar guncellenir, yeni kayitlar eklenir.</p>
        
        <div class="file-input">
            <asp:FileUpload ID="fileUpload" runat="server" accept=".xlsx,.xls" />
            <asp:Button ID="btnYukle" runat="server" CssClass="btn btn-success" 
                Text="Excel Yukle ve Isle" OnClick="btnYukle_Click" />
            <asp:Button ID="btnOrnekIndir" runat="server" CssClass="btn btn-primary" 
                Text="Ornek Excel Indir" OnClick="btnOrnekIndir_Click" />
        </div>

        <div class="sample-format">
            <h4>Excel Dosyasi Formati:</h4>
            <p><strong>Excel dosyanizda su kolonlar sirayla bulunmalidir:</strong></p>
            <ol>
                <li><code>UrunParcaKodu</code> - Parca Kodu (Zorunlu, benzersiz olmali)</li>
                <li><code>UrunAdi</code> - Urun/Parca Adi (Zorunlu)</li>
                <li><code>Grami</code> - Parca Grami (Sayisal, opsiyonel)</li>
                <li><code>KalipNo</code> - Kalip Numarasi (Opsiyonel)</li>
            </ol>
        </div>
    </div>

    <!-- PARÇA LİSTESİ -->
    <div class="action-bar">
        <asp:Button ID="btnYenile" runat="server" CssClass="btn btn-primary" 
            Text="Listeyi Yenile" OnClick="btnYenile_Click" />
    </div>

    <asp:GridView ID="gvParcalar" runat="server" AutoGenerateColumns="false" 
        OnRowCommand="gvParcalar_RowCommand" DataKeyNames="ParcaId">
        <Columns>
            <asp:BoundField DataField="UrunParcaKodu" HeaderText="Parca Kodu" />
            <asp:BoundField DataField="UrunAdi" HeaderText="Urun Adi" />
            <asp:BoundField DataField="Grami" HeaderText="Grami" DataFormatString="{0:N2}" />
            <asp:BoundField DataField="KalipNo" HeaderText="Kalip No" />
            <asp:BoundField DataField="KayitTarihi" HeaderText="Kayit Tarihi" DataFormatString="{0:dd.MM.yyyy HH:mm}" />
            <asp:TemplateField HeaderText="Islemler">
                <ItemTemplate>
                    <asp:Button ID="btnSil" runat="server" CssClass="btn btn-danger" 
                        Text="Sil" CommandName="Sil" 
                        CommandArgument='<%# Eval("ParcaId") %>'
                        OnClientClick="return confirm('Bu parcayi silmek istediginize emin misiniz?');" />
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
    </asp:GridView>
</asp:Content>