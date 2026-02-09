<%@ Page Title="Manuel Veri Girişi" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ManuelGiris.aspx.cs" Inherits="UretimTakipSistemi.Enjeksiyon.ManuelGiris" %>

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
            background: white;
            border-radius: 8px;
            padding: 30px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
            margin-bottom: 20px;
        }
        .form-row {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
            gap: 20px;
            margin-bottom: 20px;
        }
        .form-group {
            display: flex;
            flex-direction: column;
        }
        .form-group label {
            margin-bottom: 8px;
            font-weight: 600;
            color: #333;
            font-size: 14px;
        }
        .form-group input,
        .form-group select {
            padding: 12px;
            border: 1px solid #ddd;
            border-radius: 4px;
            font-size: 14px;
        }
        .form-group input:focus,
        .form-group select:focus {
            outline: none;
            border-color: #1e3c72;
        }
        .btn {
            padding: 12px 30px;
            border: none;
            border-radius: 4px;
            font-size: 16px;
            font-weight: 600;
            cursor: pointer;
            transition: all 0.3s;
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
            margin-left: 10px;
        }
        .btn-secondary:hover {
            background: #555;
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
        @media (max-width: 768px) {
            .form-row {
                grid-template-columns: 1fr;
            }
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="page-header">
        <h1>✍️ Manuel Veri Girişi</h1>
        <p>Enjeksiyon üretim verilerini manuel olarak kaydedin</p>
    </div>

    <asp:Panel ID="pnlMesaj" runat="server" Visible="false">
        <asp:Literal ID="litMesaj" runat="server"></asp:Literal>
    </asp:Panel>

    <div class="form-card">
        <h3 style="margin-bottom: 25px;">Üretim Bilgileri</h3>
        
        <div class="form-row">
            <div class="form-group">
                <label>Personel *</label>
                <asp:DropDownList ID="ddlPersonel" runat="server"></asp:DropDownList>
            </div>
            <div class="form-group">
                <label>Makine *</label>
                <asp:DropDownList ID="ddlMakine" runat="server"></asp:DropDownList>
            </div>
        </div>

        <div class="form-row">
            <div class="form-group">
                <label>İş Emri *</label>
                <asp:DropDownList ID="ddlIsEmri" runat="server"></asp:DropDownList>
            </div>
            <div class="form-group">
                <label>Tarih *</label>
                <asp:TextBox ID="txtTarih" runat="server" TextMode="Date"></asp:TextBox>
            </div>
        </div>

        <div class="form-row">
            <div class="form-group">
                <label>Başlangıç Saati *</label>
                <asp:TextBox ID="txtBaslangic" runat="server" TextMode="Time"></asp:TextBox>
            </div>
            <div class="form-group">
                <label>Bitiş Saati *</label>
                <asp:TextBox ID="txtBitis" runat="server" TextMode="Time"></asp:TextBox>
            </div>
        </div>

        <div class="form-row">
            <div class="form-group">
                <label>Üretim Adet *</label>
                <asp:TextBox ID="txtUretimAdet" runat="server" TextMode="Number" placeholder="0"></asp:TextBox>
            </div>
            <div class="form-group">
                <label>Fire Adet</label>
                <asp:TextBox ID="txtFireAdet" runat="server" TextMode="Number" placeholder="0" Text="0"></asp:TextBox>
            </div>
        </div>

        <div class="form-row">
            <div class="form-group">
                <label>Duruş Süresi (dakika)</label>
                <asp:TextBox ID="txtDurusSure" runat="server" TextMode="Number" placeholder="0" Text="0"></asp:TextBox>
            </div>
            <div class="form-group">
                <label>Fire Nedeni</label>
                <asp:DropDownList ID="ddlFireNedeni" runat="server">
                    <asp:ListItem Value="" Text="Seçiniz (opsiyonel)"></asp:ListItem>
                    <asp:ListItem Value="Kalıp Hatası" Text="Kalıp Hatası"></asp:ListItem>
                    <asp:ListItem Value="Malzeme Hatası" Text="Malzeme Hatası"></asp:ListItem>
                    <asp:ListItem Value="Operatör Hatası" Text="Operatör Hatası"></asp:ListItem>
                    <asp:ListItem Value="Makine Arızası" Text="Makine Arızası"></asp:ListItem>
                    <asp:ListItem Value="Diğer" Text="Diğer"></asp:ListItem>
                </asp:DropDownList>
            </div>
        </div>

        <div style="margin-top: 30px;">
            <asp:Button ID="btnKaydet" runat="server" CssClass="btn btn-primary" 
                Text="💾 Kaydet" OnClick="btnKaydet_Click" />
            <asp:Button ID="btnTemizle" runat="server" CssClass="btn btn-secondary" 
                Text="🔄 Temizle" OnClick="btnTemizle_Click" />
        </div>
    </div>
</asp:Content>