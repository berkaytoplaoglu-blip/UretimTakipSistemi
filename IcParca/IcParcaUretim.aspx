<%@ Page Title="İç Parça Üretim" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="IcParcaUretim.aspx.cs" Inherits="UretimTakipSistemi.IcParca.IcParcaUretim" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .page-header { margin-bottom: 30px; }
        .page-header h1 { color: #333; font-size: 28px; }

        .form-card { background: #f9f9f9; border: 1px solid #e0e0e0; border-radius: 8px; padding: 25px; margin-bottom: 20px; }
        .form-row { display: grid; grid-template-columns: repeat(auto-fit, minmax(220px, 1fr)); gap: 15px; margin-bottom: 15px; }
        .form-group { display: flex; flex-direction: column; }
        .form-group label { margin-bottom: 5px; color: #333; font-weight: 600; font-size: 14px; }

        /* ✅ Hafif gri input */
        .form-group input,
        .form-group select,
        .form-group textarea {
            padding: 10px;
            border: 1px solid #d6d6d6;
            border-radius: 6px;
            font-size: 14px;
            background: #f1f3f5;
            color: #111;
        }
        .form-group input:focus,
        .form-group select:focus,
        .form-group textarea:focus {
            outline: none;
            border-color: #1e3c72;
            background: #eef2f7;
        }

        .form-group textarea { resize: vertical; min-height: 60px; }

        .btn { padding: 10px 16px; border: none; border-radius: 6px; font-size: 14px; font-weight: 800; cursor: pointer; transition: all .2s; }
        .btn-success { background: #4caf50; color: white; } .btn-success:hover { background: #45a049; }
        .btn-warning { background: #ff9800; color: white; } .btn-warning:hover { background: #fb8c00; }
        .btn-primary { background: #1e3c72; color: white; } .btn-primary:hover { background: #2a5298; }
        .btn-light { background: #e9ecef; color: #333; border: 1px solid #d0d0d0; }
        .btn-light:hover { background: #dde2e6; }
        .btn-small { padding: 6px 10px; font-size: 12px; border-radius: 6px; }

        .button-group { display: flex; gap: 10px; margin-top: 15px; flex-wrap: wrap; }

        .alert { padding: 12px 20px; border-radius: 6px; margin-bottom: 20px; }
        .alert-success { background: #d4edda; border: 1px solid #c3e6cb; color: #155724; }
        .alert-error { background: #f8d7da; border: 1px solid #f5c6cb; color: #721c24; }

        table { width: 100%; border-collapse: collapse; background: white; }
        th { background: #1e3c72; color: white; padding: 12px; text-align: left; font-weight: 800; font-size: 14px; }
        td { padding: 12px; border-bottom: 1px solid #e0e0e0; font-size: 14px; }
        tr:hover { background: #f5f5f5; }

        .records-title { color: #333; font-size: 20px; font-weight: 900; margin: 20px 0 12px; padding-bottom: 10px; border-bottom: 2px solid #1e3c72; }
        .action-buttons { display: flex; gap: 8px; }

        .info-card {
            background: #fff;
            border: 1px solid #e0e0e0;
            border-left: 6px solid #2196f3;
            border-radius: 10px;
            padding: 14px 16px;
            margin: 10px 0 18px;
        }
        .info-card h4 { margin: 0 0 10px; font-size: 16px; color: #1e3c72; }
        .info-grid { display:grid; grid-template-columns: repeat(auto-fit, minmax(220px, 1fr)); gap: 10px; }
        .kv { font-size: 14px; }
        .kv b { color:#333; }
        .mini { font-size: 12px; color:#666; margin-top:6px; }

        .time-inline { display:flex; gap:8px; align-items:center; }
        .time-inline input { width: 140px; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="page-header">
        <h1>🔧 İç Parça Montaj Üretim Giriş</h1>
    </div>

    <asp:Panel ID="pnlMesaj" runat="server" Style="display:none;">
        <asp:Literal ID="litMesaj" runat="server"></asp:Literal>
    </asp:Panel>

    <div class="form-card">
        <h3 style="margin-bottom: 12px;">Üretim Bilgileri</h3>

        <asp:HiddenField ID="hfKayitId" runat="server" Value="0" />

        <div class="form-row">
            <div class="form-group">
                <label>Tarih *</label>
                <asp:TextBox ID="txtTarih" runat="server" TextMode="Date"></asp:TextBox>
            </div>

            <div class="form-group">
                <label>Vardiya *</label>
                <asp:DropDownList ID="ddlVardiya" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlVardiya_SelectedIndexChanged">
                    <asp:ListItem Value="Gunduz" Text="Gündüz Vardiyası"></asp:ListItem>
                    <asp:ListItem Value="Gece" Text="Gece Vardiyası"></asp:ListItem>
                </asp:DropDownList>
                <div class="mini">Gündüz: 07:30-17:30 / Gece: 19:30-07:30 (düzenlenebilir)</div>
            </div>
        </div>

        <div class="form-row">
            <div class="form-group">
                <label>Personel *</label>
                <asp:DropDownList ID="ddlPersonel" runat="server"></asp:DropDownList>
                <div class="mini">İpucu: Listeye tıkla, klavyeden isim yaz → otomatik o isme atlar.</div>
            </div>

            <div class="form-group">
                <label>Hat *</label>
                <asp:DropDownList ID="ddlHat" runat="server"></asp:DropDownList>
            </div>
        </div>

        <div class="form-row">
            <div class="form-group">
                <label>İş Emri No *</label>
                <asp:DropDownList ID="ddlIsEmri" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlIsEmri_SelectedIndexChanged"></asp:DropDownList>
            </div>
        </div>

        <asp:Panel ID="pnlIsEmriInfo" runat="server" CssClass="info-card" Visible="false">
            <h4>📌 Seçilen İş Emri Bilgileri</h4>
            <div class="info-grid">
                <div class="kv"><b>İş Emri:</b> <asp:Label ID="lblInfoIsEmriNo" runat="server" /></div>
                <div class="kv"><b>Ürün:</b> <asp:Label ID="lblInfoUrunAdi" runat="server" /></div>
                <div class="kv"><b>Parça Kodu:</b> <asp:Label ID="lblInfoParcaKodu" runat="server" /></div>
                <div class="kv"><b>Gram:</b> <asp:Label ID="lblInfoGram" runat="server" /></div>
                <div class="kv"><b>Kalıp No:</b> <asp:Label ID="lblInfoKalip" runat="server" /></div>
                <div class="kv"><b>Boyahane Durum:</b> <asp:Label ID="lblInfoBoyahane" runat="server" /></div>
                <div class="kv"><b>İç Parça Durum:</b> <asp:Label ID="lblInfoIcParca" runat="server" /></div>
            </div>
        </asp:Panel>

        <div class="form-row">
            <div class="form-group">
                <label>Başlangıç Saati</label>
                <div class="time-inline">
                    <asp:TextBox ID="txtBasSaat" runat="server" placeholder="07:30"></asp:TextBox>
                    <asp:Button ID="btnBasSimdi" runat="server" CssClass="btn btn-light btn-small" Text="Şimdi" OnClick="btnBasSimdi_Click" />
                </div>
            </div>

            <div class="form-group">
                <label>Bitiş Saati</label>
                <div class="time-inline">
                    <asp:TextBox ID="txtBitSaat" runat="server" placeholder="17:30"></asp:TextBox>
                    <asp:Button ID="btnBitSimdi" runat="server" CssClass="btn btn-light btn-small" Text="Şimdi" OnClick="btnBitSimdi_Click" />
                </div>
            </div>

            <div class="form-group">
                <label>Duraksama (Saat)</label>
                <asp:TextBox ID="txtDuraksamaSaat" runat="server" placeholder="Örn: 1,5"></asp:TextBox>
                <div class="mini">Saat cinsinden: 1,5 = 1 saat 30 dk</div>
            </div>

            <div class="form-group">
                <label>Net Çalışma (Saat)</label>
                <asp:TextBox ID="txtNetCalismaSaat" runat="server" ReadOnly="true"></asp:TextBox>
                <div class="mini">Bitiş-Başlangıç - Duraksama otomatik hesaplanır.</div>
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
            <label>Açıklama (opsiyonel)</label>
            <asp:TextBox ID="txtAciklama" runat="server" TextMode="MultiLine"></asp:TextBox>
        </div>

        <div class="button-group">
            <asp:Button ID="btnKaydet" runat="server" CssClass="btn btn-success" Text="💾 Kaydet" OnClick="btnKaydet_Click" />
            <asp:Button ID="btnGuncelle" runat="server" CssClass="btn btn-primary" Text="✏️ Güncelle" OnClick="btnGuncelle_Click" Visible="false" />
            <asp:Button ID="btnTemizle" runat="server" CssClass="btn btn-warning" Text="🔄 Temizle" OnClick="btnTemizle_Click" />
        </div>
    </div>

    <div class="records-title">Son 20 Kayıt</div>

    <asp:GridView ID="gvKayitlar" runat="server" AutoGenerateColumns="false" OnRowCommand="gvKayitlar_RowCommand" DataKeyNames="KayitId">
        <Columns>
            <asp:BoundField DataField="Tarih" HeaderText="Tarih" DataFormatString="{0:dd.MM.yyyy}" />
            <asp:BoundField DataField="Vardiya" HeaderText="Vardiya" />
            <asp:BoundField DataField="Personel" HeaderText="Personel" />
            <asp:BoundField DataField="Hat" HeaderText="Hat" />
            <asp:BoundField DataField="IsEmriNo" HeaderText="İş Emri" />
            <asp:BoundField DataField="UretimAdet" HeaderText="Üretim" DataFormatString="{0:N0}" />
            <asp:BoundField DataField="FireAdet" HeaderText="Fire" DataFormatString="{0:N0}" />

            <asp:BoundField DataField="BaslangicSaat" HeaderText="Baş." DataFormatString="{0:hh\\:mm}" />
            <asp:BoundField DataField="BitisSaat" HeaderText="Bitiş" DataFormatString="{0:hh\\:mm}" />
            <asp:BoundField DataField="DuraksamaSaat" HeaderText="Duraksama(s)" DataFormatString="{0:N2}" />
            <asp:BoundField DataField="NetCalismaSaat" HeaderText="Net(s)" DataFormatString="{0:N2}" />

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
