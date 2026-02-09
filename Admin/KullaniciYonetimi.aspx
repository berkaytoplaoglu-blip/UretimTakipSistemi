<%@ Page Title="Kullanıcı Yönetimi" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="KullaniciYonetimi.aspx.cs" Inherits="UretimTakipSistemi.Admin.KullaniciYonetimi" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .page-header {
            margin-bottom: 30px;
        }
        .page-header h1 {
            color: #333;
            font-size: 28px;
        }
        .form-card {
            background: #f9f9f9;
            border: 1px solid #e0e0e0;
            border-radius: 8px;
            padding: 25px;
            margin-bottom: 30px;
        }
        .form-row {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
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
        .btn {
            padding: 10px 20px;
            border: none;
            border-radius: 4px;
            font-size: 14px;
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
        .btn-danger {
            background: #f44336;
            color: white;
        }
        .btn-danger:hover {
            background: #da190b;
        }
        .button-group {
            display: flex;
            gap: 10px;
            margin-top: 15px;
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
            font-size: 14px;
        }
        td {
            padding: 12px;
            border-bottom: 1px solid #e0e0e0;
            font-size: 14px;
        }
        tr:hover {
            background: #f5f5f5;
        }
        .action-buttons {
            display: flex;
            gap: 5px;
        }
        .btn-small {
            padding: 5px 12px;
            font-size: 12px;
        }
        .badge {
            padding: 4px 10px;
            border-radius: 12px;
            font-size: 12px;
            font-weight: 600;
            display: inline-block;
        }
        .badge-admin {
            background: #1e3c72;
            color: white;
        }
        .badge-boyahane {
            background: #ff9800;
            color: white;
        }
        .badge-icparca {
            background: #4caf50;
            color: white;
        }
        .badge-enjeksiyon {
            background: #f44336;
            color: white;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="page-header">
        <h1>👥 Kullanıcı Yönetimi</h1>
        <p>Sistem kullanıcılarını yönet, yeni kullanıcı ekle</p>
    </div>

    <asp:Panel ID="pnlMesaj" runat="server" Style="display:none;">
        <asp:Literal ID="litMesaj" runat="server"></asp:Literal>
    </asp:Panel>

    <div class="form-card">
        <h3 style="margin-bottom: 20px;">Kullanıcı Bilgileri</h3>
        
        <asp:HiddenField ID="hfKullaniciId" runat="server" Value="0" />
        
        <div class="form-row">
            <div class="form-group">
                <label>Ad Soyad *</label>
                <asp:TextBox ID="txtAdSoyad" runat="server" MaxLength="100" placeholder="Ahmet Yılmaz"></asp:TextBox>
            </div>
            <div class="form-group">
                <label>Kullanıcı Adı *</label>
                <asp:TextBox ID="txtKullaniciAdi" runat="server" MaxLength="50" placeholder="ahmet.yilmaz"></asp:TextBox>
            </div>
        </div>

        <div class="form-row">
            <div class="form-group">
                <label>Şifre *</label>
                <asp:TextBox ID="txtSifre" runat="server" TextMode="Password" MaxLength="100" placeholder="••••••"></asp:TextBox>
            </div>
            <div class="form-group">
                <label>Rol / Bölüm *</label>
                <asp:DropDownList ID="ddlRol" runat="server">
                    <asp:ListItem Value="Admin" Text="Admin - Yönetici"></asp:ListItem>
                    <asp:ListItem Value="Boyahane" Text="Boyahane"></asp:ListItem>
                    <asp:ListItem Value="IcParca" Text="İç Parça"></asp:ListItem>
                    <asp:ListItem Value="Enjeksiyon" Text="Enjeksiyon"></asp:ListItem>
                </asp:DropDownList>
            </div>
        </div>

        <div class="button-group">
            <asp:Button ID="btnKaydet" runat="server" CssClass="btn btn-success" Text="💾 Kaydet" OnClick="btnKaydet_Click" />
            <asp:Button ID="btnGuncelle" runat="server" CssClass="btn btn-warning" Text="✏️ Güncelle" 
                OnClick="btnGuncelle_Click" Visible="false" />
            <asp:Button ID="btnTemizle" runat="server" CssClass="btn btn-warning" Text="🔄 Temizle" OnClick="btnTemizle_Click" />
        </div>
    </div>

    <h3 style="margin-bottom: 15px;">Kayıtlı Kullanıcılar</h3>
    
    <asp:GridView ID="gvKullanicilar" runat="server" AutoGenerateColumns="false" 
        OnRowCommand="gvKullanicilar_RowCommand" DataKeyNames="KullaniciId">
        <Columns>
            <asp:BoundField DataField="AdSoyad" HeaderText="Ad Soyad" />
            <asp:BoundField DataField="KullaniciAdi" HeaderText="Kullanıcı Adı" />
            <asp:TemplateField HeaderText="Rol / Bölüm">
                <ItemTemplate>
                    <%# GetRolBadge(Eval("Rol").ToString()) %>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:BoundField DataField="KayitTarihi" HeaderText="Kayıt Tarihi" DataFormatString="{0:dd.MM.yyyy HH:mm}" />
            <asp:TemplateField HeaderText="Durum">
                <ItemTemplate>
                    <%# Convert.ToBoolean(Eval("Aktif")) ? "<span style='color:green'>✓ Aktif</span>" : "<span style='color:red'>✗ Pasif</span>" %>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="İşlemler">
                <ItemTemplate>
                    <div class="action-buttons">
                        <asp:Button ID="btnDuzenle" runat="server" CssClass="btn btn-warning btn-small" 
                            Text="✏️ Düzenle" CommandName="Duzenle" 
                            CommandArgument='<%# Eval("KullaniciId") %>' />
                        <asp:Button ID="btnSil" runat="server" CssClass="btn btn-danger btn-small" 
                            Text="🗑️ Sil" CommandName="Sil" 
                            CommandArgument='<%# Eval("KullaniciId") %>' 
                            OnClientClick="return confirm('Bu kullanıcıyı silmek istediğinize emin misiniz?');" />
                    </div>
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
    </asp:GridView>
</asp:Content>