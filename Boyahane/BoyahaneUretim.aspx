<%@ Page Title="Boyahane Üretim" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="BoyahaneUretim.aspx.cs" Inherits="UretimTakipSistemi.Boyahane.BoyahaneUretim" %>

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
        .form-group select,
        .form-group textarea {
            padding: 10px;
            border: 1px solid #ddd;
            border-radius: 4px;
            font-size: 14px;
        }
        .form-group textarea {
            resize: vertical;
            min-height: 60px;
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
        .btn-primary {
            background: #1e3c72;
            color: white;
        }
        .btn-primary:hover {
            background: #2a5298;
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
        .records-title {
            color: #333;
            font-size: 20px;
            font-weight: 600;
            margin-bottom: 15px;
            padding-bottom: 10px;
            border-bottom: 2px solid #1e3c72;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="page-header">
        <h1>🎨 Boyahane Üretim Giriş</h1>
    </div>

    <asp:Panel ID="pnlMesaj" runat="server" Style="display:none;">
        <asp:Literal ID="litMesaj" runat="server"></asp:Literal>
    </asp:Panel>

    <div class="form-card">
        <h3 style="margin-bottom: 20px;">Üretim Bilgileri</h3>
        
        <asp:HiddenField ID="hfKayitId" runat="server" Value="0" />
        
        <div class="form-row">
            <div class="form-group">
                <label>Tarih *</label>
                <asp:TextBox ID="txtTarih" runat="server" TextMode="Date"></asp:TextBox>
            </div>
            <div class="form-group">
                <label>Vardiya *</label>
                <asp:DropDownList ID="ddlVardiya" runat="server">
                    <asp:ListItem Value="Sabah" Text="Sabah"></asp:ListItem>
                    <asp:ListItem Value="Oglen" Text="Öğlen"></asp:ListItem>
                    <asp:ListItem Value="Gece" Text="Gece"></asp:ListItem>
                </asp:DropDownList>
            </div>
        </div>

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
                <label>İş Emri No *</label>
                <asp:DropDownList ID="ddlIsEmri" runat="server"></asp:DropDownList>
            </div>
        </div>

        <div class="form-row">
            <div class="form-group">
                <label>Üretim Adet *</label>
                <asp:TextBox ID="txtUretimAdet" runat="server" placeholder="1000"></asp:TextBox>
            </div>
            <div class="form-group">
                <label>Fire Adet</label>
                <asp:TextBox ID="txtFireAdet" runat="server" Text="0" placeholder="0"></asp:TextBox>
            </div>
        </div>

        <div class="form-group">
            <label>Açıklama</label>
            <asp:TextBox ID="txtAciklama" runat="server" TextMode="MultiLine"></asp:TextBox>
        </div>

        <div class="button-group">
            <asp:Button ID="btnKaydet" runat="server" CssClass="btn btn-success" Text="💾 Kaydet" OnClick="btnKaydet_Click" />
            <asp:Button ID="btnGuncelle" runat="server" CssClass="btn btn-primary" Text="✏️ Güncelle" 
                OnClick="btnGuncelle_Click" Visible="false" />
            <asp:Button ID="btnTemizle" runat="server" CssClass="btn btn-warning" Text="🔄 Temizle" OnClick="btnTemizle_Click" />
        </div>
    </div>

    <div class="records-title">Son 20 Kayıt</div>
    
    <asp:GridView ID="gvKayitlar" runat="server" AutoGenerateColumns="false" 
        OnRowCommand="gvKayitlar_RowCommand" DataKeyNames="KayitId">
        <Columns>
            <asp:BoundField DataField="Tarih" HeaderText="Tarih" DataFormatString="{0:dd.MM.yyyy}" />
            <asp:BoundField DataField="Vardiya" HeaderText="Vardiya" />
            <asp:BoundField DataField="Personel" HeaderText="Personel" />
            <asp:BoundField DataField="MakineAdi" HeaderText="Makine" />
            <asp:BoundField DataField="IsEmriNo" HeaderText="İş Emri" />
            <asp:BoundField DataField="UretimAdet" HeaderText="Üretim" DataFormatString="{0:N0}" />
            <asp:BoundField DataField="FireAdet" HeaderText="Fire" DataFormatString="{0:N0}" />
            <asp:BoundField DataField="Aciklama" HeaderText="Açıklama" />
            <asp:TemplateField HeaderText="İşlemler">
                <ItemTemplate>
                    <div class="action-buttons">
                        <asp:Button ID="btnGetir" runat="server" CssClass="btn btn-primary btn-small" 
                            Text="✏️ Getir" CommandName="GetirKayit" 
                            CommandArgument='<%# Eval("KayitId") %>' />
                    </div>
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
    </asp:GridView>
</asp:Content>