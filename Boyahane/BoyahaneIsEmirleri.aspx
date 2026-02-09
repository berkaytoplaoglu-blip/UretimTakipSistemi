<%@ Page Title="Boyahane İş Emirleri" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="BoyahaneIsEmirleri.aspx.cs" Inherits="UretimTakipSistemi.Boyahane.BoyahaneIsEmirleri" %>

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
        .is-emri-card {
            background: white;
            border-radius: 8px;
            padding: 20px;
            margin-bottom: 20px;
            box-shadow: 0 2px 8px rgba(0,0,0,0.1);
            border-left: 5px solid #ff9800;
        }
        .is-emri-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 15px;
        }
        .is-emri-no {
            font-size: 20px;
            font-weight: bold;
            color: #333;
        }
        .is-emri-info {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
            gap: 15px;
            margin-bottom: 15px;
        }
        .info-item {
            display: flex;
            flex-direction: column;
        }
        .info-label {
            font-size: 12px;
            color: #666;
            margin-bottom: 5px;
        }
        .info-value {
            font-size: 16px;
            font-weight: 600;
            color: #333;
        }
        .durum-section {
            display: flex;
            gap: 10px;
            align-items: center;
            padding-top: 15px;
            border-top: 1px solid #e0e0e0;
        }
        .durum-section label {
            font-weight: 600;
            color: #333;
        }
        .durum-section select {
            padding: 8px 12px;
            border: 2px solid #ddd;
            border-radius: 4px;
            font-size: 14px;
            font-weight: 600;
        }
        .btn {
            padding: 8px 16px;
            border: none;
            border-radius: 4px;
            font-size: 14px;
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
        .badge {
            padding: 6px 12px;
            border-radius: 4px;
            font-size: 13px;
            font-weight: 600;
        }
        .badge-yeni {
            background: #4caf50;
            color: white;
        }
        .badge-aktif {
            background: #ff9800;
            color: white;
        }
        .badge-tamamlandi {
            background: #2196f3;
            color: white;
        }
        .badge-bos {
            background: #999;
            color: white;
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
        .empty-state {
            text-align: center;
            padding: 60px 20px;
            background: white;
            border-radius: 8px;
        }
        .empty-state h3 {
            color: #666;
            margin-bottom: 10px;
        }
        @media (max-width: 768px) {
            .is-emri-header {
                flex-direction: column;
                align-items: flex-start;
            }
            .durum-section {
                flex-direction: column;
                align-items: stretch;
            }
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="page-header">
        <h1>🎨 Boyahane İş Emirleri</h1>
        <p>Size atanan iş emirlerini görüntüleyin ve durum güncelleyin</p>
    </div>

    <asp:Panel ID="pnlMesaj" runat="server" Visible="false">
        <asp:Literal ID="litMesaj" runat="server"></asp:Literal>
    </asp:Panel>

    <asp:Repeater ID="rptIsEmirleri" runat="server" OnItemCommand="rptIsEmirleri_ItemCommand" OnItemDataBound="rptIsEmirleri_ItemDataBound">
        <ItemTemplate>
            <div class="is-emri-card">
                <div class="is-emri-header">
                    <div class="is-emri-no">📋 <%# Eval("IsEmriNo") %></div>
                    <asp:Literal ID="litDurumBadge" runat="server"></asp:Literal>
                </div>

                <div class="is-emri-info">
                    <div class="info-item">
                        <div class="info-label">Ürün Adı</div>
                        <div class="info-value"><%# Eval("UrunAdi") %></div>
                    </div>
                    <div class="info-item">
                        <div class="info-label">Parça Kodu</div>
                        <div class="info-value"><%# Eval("UrunParcaKodu") %></div>
                    </div>
                    <div class="info-item">
                        <div class="info-label">Gramı</div>
                        <div class="info-value"><%# Eval("Grami", "{0:N2}") %> gr</div>
                    </div>
                    <div class="info-item">
                        <div class="info-label">Kalıp No</div>
                        <div class="info-value"><%# Eval("KalipNo") ?? "-" %></div>
                    </div>
                </div>

                <div class="durum-section">
                    <label>Durum Güncelle:</label>
                    <asp:DropDownList ID="ddlDurum" runat="server">
                        <asp:ListItem Value="YENİ" Text="YENİ"></asp:ListItem>
                        <asp:ListItem Value="AKTİF" Text="AKTİF"></asp:ListItem>
                        <asp:ListItem Value="TAMAMLANDI" Text="TAMAMLANDI"></asp:ListItem>
                    </asp:DropDownList>
                    <asp:Button ID="btnGuncelle" runat="server" 
                        CssClass="btn btn-primary" 
                        Text="💾 Güncelle"
                        CommandName="Guncelle"
                        CommandArgument='<%# Eval("IsEmriId") %>' />
                </div>
            </div>
        </ItemTemplate>
    </asp:Repeater>

    <asp:Panel ID="pnlBos" runat="server" Visible="false">
        <div class="empty-state">
            <h3>📭 Henüz size atanmış iş emri bulunmamaktadır</h3>
            <p>Yeni iş emirleri eklendiğinde burada görünecektir.</p>
        </div>
    </asp:Panel>
</asp:Content>