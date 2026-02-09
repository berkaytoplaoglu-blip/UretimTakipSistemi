<%@ Page Title="İş Emirleri" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="IsEmirleri.aspx.cs" Inherits="UretimTakipSistemi.Admin.IsEmirleri" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .page-header { margin-bottom: 30px; }
        .page-header h1 { color: #333; font-size: 28px; }

        .filter-section { background: #f9f9f9; padding: 20px; border-radius: 8px; margin-bottom: 20px; }
        .filter-row { display: flex; gap: 15px; align-items: flex-end; }
        .filter-group { flex: 1; }
        .filter-group label { display: block; margin-bottom: 5px; font-weight: 500; font-size: 14px; }
        .filter-group select { width: 100%; padding: 8px; border: 1px solid #ddd; border-radius: 4px; }

        .btn { padding: 8px 16px; border: none; border-radius: 4px; font-size: 14px; font-weight: 600; cursor: pointer; }
        .btn-primary { background: #1e3c72; color: white; }
        .btn-primary:hover { background: #2a5298; }
        .btn-small { padding: 5px 12px; font-size: 12px; }
        .btn-warning { background: #ff9800; color: white; }
        .btn-warning:hover { background: #fb8c00; }
        .btn-danger { background: #f44336; color: white; }
        .btn-danger:hover { background: #da190b; }
        .btn-success { background: #4caf50; color: white; }
        .btn-success:hover { background: #45a049; }
        .btn-secondary { background: #666; color: white; }
        .btn-secondary:hover { background: #555; }

        .grid-container { overflow-x: auto; }
        table { width: 100%; border-collapse: collapse; background: white; }

        th { background: #1e3c72; color: white; padding: 12px; text-align: left; font-weight: 600; font-size: 14px; }
        td { padding: 12px; border-bottom: 1px solid #e0e0e0; font-size: 14px; }
        tr:hover { background: #f5f5f5; }

        .badge { padding: 4px 10px; border-radius: 12px; font-size: 12px; font-weight: 600; display: inline-block; margin: 2px; }
        .badge-success { background: #4caf50; color: white; }
        .badge-warning { background: #ff9800; color: white; }
        .badge-danger { background: #f44336; color: white; }
        .badge-info { background: #2196f3; color: white; }

        .action-buttons { display: flex; gap: 5px; }

        .alert { padding: 12px 20px; border-radius: 4px; margin-bottom: 20px; }
        .alert-success { background: #d4edda; border: 1px solid #c3e6cb; color: #155724; }
        .alert-error { background: #f8d7da; border: 1px solid #f5c6cb; color: #721c24; }

        /* Modal */
        .modal { display: none; position: fixed; z-index: 9999; left: 0; top: 0; width: 100%; height: 100%; background: rgba(0,0,0,0.5); }
        .modal.show { display: flex; justify-content: center; align-items: center; }
        .modal-content { background: white; padding: 30px; border-radius: 8px; max-width: 650px; width: 90%; max-height: 90vh; overflow-y: auto; }
        .modal-title { font-size: 20px; font-weight: bold; margin-bottom: 20px; color: #333; }

        .form-group { margin-bottom: 15px; }
        .form-group label { display: block; margin-bottom: 5px; font-weight: 500; font-size: 14px; }
        .form-group input, .form-group select { width: 100%; padding: 10px; border: 1px solid #ddd; border-radius: 4px; }

        .checkbox-group { display: flex; gap: 20px; margin-top: 10px; flex-wrap: wrap; }
        .checkbox-group label { display: flex; align-items: center; gap: 5px; }

        .modal-buttons { display: flex; gap: 10px; margin-top: 20px; }

        .durum-section { background: #f8f9fa; padding: 15px; border-radius: 6px; margin-bottom: 15px; border-left: 4px solid #1e3c72; }
        .durum-section.boyahane { border-left-color: #ff9800; }
        .durum-section.icparca { border-left-color: #2196f3; }
        .durum-section.enjeksiyon { border-left-color: #4caf50; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="page-header">
        <h1>📋 İş Emirleri</h1>
    </div>

    <asp:Panel ID="pnlMesaj" runat="server" Style="display:none;">
        <asp:Literal ID="litMesaj" runat="server"></asp:Literal>
    </asp:Panel>

    <div class="filter-section">

        <div class="filter-row">

            <div class="filter-group">

                <label>Durum</label>
                <asp:DropDownList ID="ddlDurum" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlDurum_SelectedIndexChanged">
                    <asp:ListItem Value="" Text="Tümü"></asp:ListItem>
                    <asp:ListItem Value="ACTIVE" Text="Aktif"></asp:ListItem>
                    <asp:ListItem Value="COMPLETED" Text="Tamamlanmış"></asp:ListItem>
                    <asp:ListItem Value="CANCELLED" Text="İptal"></asp:ListItem>
                </asp:DropDownList>
            </div>

            <div class="filter-group">
                <label>Bölüm</label>
                <asp:DropDownList ID="ddlBolum" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlBolum_SelectedIndexChanged">
                    <asp:ListItem Value="" Text="Tümü"></asp:ListItem>
                    <asp:ListItem Value="Boyahane" Text="Boyahane"></asp:ListItem>
                    <asp:ListItem Value="IcParca" Text="İç Parça"></asp:ListItem>
                    <asp:ListItem Value="Enjeksiyon" Text="Enjeksiyon"></asp:ListItem>
                </asp:DropDownList>
            </div>

            <div class="filter-group">
                <asp:Button ID="btnYenile" runat="server" CssClass="btn btn-primary" Text="🔄 Yenile" OnClick="btnYenile_Click" />
            </div>



            <div class="filter-section" style="margin-top:15px;">
    <div class="filter-row">
        <div class="filter-group">
            <label>Başlangıç Tarihi</label>
            <asp:TextBox ID="txtBasTarih" runat="server" TextMode="Date"></asp:TextBox>
        </div>

        <div class="filter-group">
            <label>Bitiş Tarihi</label>
            <asp:TextBox ID="txtBitTarih" runat="server" TextMode="Date"></asp:TextBox>
        </div>

        <div class="filter-group">
            <label>Excel Durum</label>
            <asp:DropDownList ID="ddlExcelDurum" runat="server">
                <asp:ListItem Value="ALL" Text="Tamamlanan + İptal"></asp:ListItem>
                <asp:ListItem Value="COMPLETED" Text="Sadece Tamamlanan"></asp:ListItem>
                <asp:ListItem Value="CANCELLED" Text="Sadece İptal"></asp:ListItem>
            </asp:DropDownList>
        </div>

        <div class="filter-group">
            <asp:Button ID="btnExcel" runat="server"
                CssClass="btn btn-success"
                Text="📥 Tamamlanan / İptal Excel"
                OnClick="btnExcel_Click" />
        </div>
    </div>
</div>





        </div>
    </div>

    <!-- ✅ ÜST TABLO (Devam Edenler) -->
    <div class="grid-container">
        <asp:GridView ID="gvIsEmirleri" runat="server" AutoGenerateColumns="false"
            DataKeyNames="IsEmriId"
            OnRowCommand="gvIsEmirleri_RowCommand"
            OnRowDataBound="gvIsEmirleri_RowDataBound"
            GridLines="None">
            <Columns>
                <asp:TemplateField HeaderText="Detay">
                    <ItemTemplate>
                        <asp:LinkButton ID="btnToggle" runat="server" CommandName="Toggle"
                            CommandArgument='<%# Eval("IsEmriId") %>' Text="➕" />
                    </ItemTemplate>
                </asp:TemplateField>

                <asp:BoundField DataField="IsEmriNo" HeaderText="İş Emri No" />
                <asp:BoundField DataField="UrunParcaKodu" HeaderText="Parça Kodu" />
                <asp:BoundField DataField="UrunAdi" HeaderText="Ürün Adı" />
                <asp:BoundField DataField="Grami" HeaderText="Gramı" DataFormatString="{0:N2}" />
                <asp:BoundField DataField="KalipNo" HeaderText="Kalıp No" />
                <asp:BoundField DataField="ToplamUretimAdet" HeaderText="Toplam Üretim" DataFormatString="{0:N0}" />
                <asp:BoundField DataField="ToplamHammaddeGram" HeaderText="Toplam Hammadde (gr)" DataFormatString="{0:N0}" />

                <asp:TemplateField HeaderText="Bölümler">
                    <ItemTemplate><asp:Literal ID="litBolumler" runat="server"></asp:Literal></ItemTemplate>
                </asp:TemplateField>

                <asp:TemplateField HeaderText="Durum">
                    <ItemTemplate>
                        <span class='badge <%# GetStatusBadgeClass(Eval("Durum").ToString()) %>'>
                            <%# GetStatusText(Eval("Durum").ToString()) %>
                        </span>
                    </ItemTemplate>
                </asp:TemplateField>

                <asp:BoundField DataField="OlusturmaTarihi" HeaderText="Oluşturma Tarihi" DataFormatString="{0:dd.MM.yyyy HH:mm}" />

                <asp:TemplateField HeaderText="">
                    <ItemTemplate>
                        <asp:Panel ID="pnlDetay" runat="server" Visible="false" style="margin-top:10px;padding:12px;border:1px solid #e0e0e0;border-radius:8px;background:#fafafa;">
                            <div style="display:flex;gap:12px;flex-wrap:wrap;">
                                <asp:Panel ID="pnlBoyahaneDurum" runat="server" Visible="false" style="padding:10px;border-radius:8px;border-left:4px solid #ff9800;background:#fff;">
                                    <b>🎨 Boyahane Durum:</b> <asp:Label ID="lblBoyahaneDurum" runat="server" />
                                </asp:Panel>
                                <asp:Panel ID="pnlIcParcaDurum" runat="server" Visible="false" style="padding:10px;border-radius:8px;border-left:4px solid #2196f3;background:#fff;">
                                    <b>🔧 İç Parça Durum:</b> <asp:Label ID="lblIcParcaDurum" runat="server" />
                                </asp:Panel>
                            </div>
                        </asp:Panel>
                    </ItemTemplate>
                </asp:TemplateField>

                <asp:TemplateField HeaderText="İşlemler">
                    <ItemTemplate>
                        <div class="action-buttons">
                            <asp:Button ID="btnDuzenle" runat="server" CssClass="btn btn-warning btn-small"
                                Text="✏️ Düzenle" CommandName="Duzenle" CommandArgument='<%# Eval("IsEmriId") %>' />
                            <asp:Button ID="btnIptal" runat="server" CssClass="btn btn-danger btn-small"
                                Text="❌ İptal" CommandName="Iptal" CommandArgument='<%# Eval("IsEmriId") %>'
                                Visible='<%# Eval("Durum").ToString() == "ACTIVE" %>'
                                OnClientClick="return confirm('Bu iş emrini iptal etmek istediğinize emin misiniz?');" />
                        </div>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>
    </div>

    <!-- ✅ ALT TABLO (Tüm Bölümleri Tamamlananlar) -->
    <div style="margin-top:20px;">
        <h3 style="margin:10px 0;">✅ Tüm Bölümleri Tamamlanan İş Emirleri</h3>
        <div class="grid-container">
            <asp:GridView ID="gvIsEmirleriTamamlanan" runat="server" AutoGenerateColumns="false"
                DataKeyNames="IsEmriId"
                OnRowCommand="gvIsEmirleri_RowCommand"
                OnRowDataBound="gvIsEmirleri_RowDataBound"
                GridLines="None">
                <Columns>
                    <asp:TemplateField HeaderText="Detay">
                        <ItemTemplate>
                            <asp:LinkButton ID="btnToggle" runat="server" CommandName="Toggle"
                                CommandArgument='<%# Eval("IsEmriId") %>' Text="➕" />
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:BoundField DataField="IsEmriNo" HeaderText="İş Emri No" />
                    <asp:BoundField DataField="UrunParcaKodu" HeaderText="Parça Kodu" />
                    <asp:BoundField DataField="UrunAdi" HeaderText="Ürün Adı" />
                    <asp:BoundField DataField="Grami" HeaderText="Gramı" DataFormatString="{0:N2}" />
                    <asp:BoundField DataField="KalipNo" HeaderText="Kalıp No" />
                    <asp:BoundField DataField="ToplamUretimAdet" HeaderText="Toplam Üretim" DataFormatString="{0:N0}" />
                    <asp:BoundField DataField="ToplamHammaddeGram" HeaderText="Toplam Hammadde (gr)" DataFormatString="{0:N0}" />

                    <asp:TemplateField HeaderText="Bölümler">
                        <ItemTemplate><asp:Literal ID="litBolumler" runat="server"></asp:Literal></ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Durum">
                        <ItemTemplate>
                            <span class='badge <%# GetStatusBadgeClass(Eval("Durum").ToString()) %>'>
                                <%# GetStatusText(Eval("Durum").ToString()) %>
                            </span>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:BoundField DataField="OlusturmaTarihi" HeaderText="Oluşturma Tarihi" DataFormatString="{0:dd.MM.yyyy HH:mm}" />

                    <asp:TemplateField HeaderText="">
                        <ItemTemplate>
                            <asp:Panel ID="pnlDetay" runat="server" Visible="false" style="margin-top:10px;padding:12px;border:1px solid #e0e0e0;border-radius:8px;background:#fafafa;">
                                <div style="display:flex;gap:12px;flex-wrap:wrap;">
                                    <asp:Panel ID="pnlBoyahaneDurum" runat="server" Visible="false" style="padding:10px;border-radius:8px;border-left:4px solid #ff9800;background:#fff;">
                                        <b>🎨 Boyahane Durum:</b> <asp:Label ID="lblBoyahaneDurum" runat="server" />
                                    </asp:Panel>
                                    <asp:Panel ID="pnlIcParcaDurum" runat="server" Visible="false" style="padding:10px;border-radius:8px;border-left:4px solid #2196f3;background:#fff;">
                                        <b>🔧 İç Parça Durum:</b> <asp:Label ID="lblIcParcaDurum" runat="server" />
                                    </asp:Panel>
                                </div>
                            </asp:Panel>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="İşlemler">
                        <ItemTemplate>
                            <div class="action-buttons">
                                <asp:Button ID="btnDuzenle" runat="server" CssClass="btn btn-warning btn-small"
                                    Text="✏️ Düzenle" CommandName="Duzenle" CommandArgument='<%# Eval("IsEmriId") %>' />
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>

                </Columns>
            </asp:GridView>
        </div>
    </div>

    <!-- Modal (senin mevcut modalın aynı kalsın) -->
    <div id="modalDuzenle" class="modal">
        <div class="modal-content">
            <div class="modal-title">✏️ İş Emri Düzenle</div>

            <asp:HiddenField ID="hfIsEmriId" runat="server" Value="0" />

            <div class="form-group">
                <label>İş Emri No</label>
                <asp:TextBox ID="txtIsEmriNo" runat="server" ReadOnly="true" style="background:#f5f5f5;"></asp:TextBox>
            </div>

            <div class="form-group">
                <label>Ürün / Parça</label>
                <asp:TextBox ID="txtUrunAdi" runat="server" ReadOnly="true" style="background:#f5f5f5;"></asp:TextBox>
            </div>

            <div class="form-group">
                <label>Bölümler</label>
                <div class="checkbox-group">
                    <label><asp:CheckBox ID="chkBoyahane" runat="server" /> 🎨 Boyahane</label>
                    <label><asp:CheckBox ID="chkIcParca" runat="server" /> 🔧 İç Parça</label>
                    <label><asp:CheckBox ID="chkEnjeksiyon" runat="server" /> ⚙️ Enjeksiyon</label>
                </div>
            </div>

            <div id="boyahaneSection" class="durum-section boyahane" style="display:none;">
                <div class="form-group">
                    <label>🎨 Boyahane Durum</label>
                    <asp:DropDownList ID="ddlBoyahaneDurum" runat="server">
                        <asp:ListItem Value="YENİ" Text="YENİ"></asp:ListItem>
                        <asp:ListItem Value="AKTİF" Text="AKTİF"></asp:ListItem>
                        <asp:ListItem Value="TAMAMLANDI" Text="TAMAMLANDI"></asp:ListItem>
                    </asp:DropDownList>
                </div>
            </div>

            <div id="icparcaSection" class="durum-section icparca" style="display:none;">
                <div class="form-group">
                    <label>🔧 İç Parça Durum</label>
                    <asp:DropDownList ID="ddlIcParcaDurum" runat="server">
                        <asp:ListItem Value="YENİ" Text="YENİ"></asp:ListItem>
                        <asp:ListItem Value="AKTİF" Text="AKTİF"></asp:ListItem>
                        <asp:ListItem Value="TAMAMLANDI" Text="TAMAMLANDI"></asp:ListItem>
                    </asp:DropDownList>
                </div>
            </div>

            <div id="enjeksiyonSection" class="durum-section enjeksiyon" style="display:none;">
                <div class="form-group">
                    <label>Çevrim Süresi (saniye)</label>
                    <asp:TextBox ID="txtCevrimSuresi" runat="server" TextMode="Number" placeholder="45"></asp:TextBox>
                </div>
                <div class="form-group">
                    <label>Soğuma Süresi (saniye)</label>
                    <asp:TextBox ID="txtSogumaSuresi" runat="server" TextMode="Number" placeholder="30"></asp:TextBox>
                </div>
            </div>

            <div class="form-group">
                <label>Durum</label>
                <asp:DropDownList ID="ddlDurumModal" runat="server">
                    <asp:ListItem Value="ACTIVE" Text="Aktif"></asp:ListItem>
                    <asp:ListItem Value="COMPLETED" Text="Tamamlanmış"></asp:ListItem>
                    <asp:ListItem Value="CANCELLED" Text="İptal"></asp:ListItem>
                </asp:DropDownList>
            </div>

            <div class="modal-buttons">
                <asp:Button ID="btnKaydet" runat="server" CssClass="btn btn-success" Text="💾 Kaydet" OnClick="btnKaydet_Click" />
                <button type="button" class="btn btn-secondary" onclick="closeModal()">❌ İptal</button>
            </div>
        </div>
    </div>
    <div style="margin-top:20px;">
    <h3 style="margin:10px 0;">❌ İptal Edilen İş Emirleri</h3>
    <div class="grid-container">
        <asp:GridView ID="gvIsEmirleriIptal" runat="server" AutoGenerateColumns="false"
            DataKeyNames="IsEmriId"
            OnRowCommand="gvIsEmirleri_RowCommand"
            OnRowDataBound="gvIsEmirleri_RowDataBound"
            GridLines="None">
            <Columns>
                <asp:TemplateField HeaderText="Detay">
                    <ItemTemplate>
                        <asp:LinkButton ID="btnToggle" runat="server" CommandName="Toggle"
                            CommandArgument='<%# Eval("IsEmriId") %>' Text="➕" />
                    </ItemTemplate>
                </asp:TemplateField>

                <asp:BoundField DataField="IsEmriNo" HeaderText="İş Emri No" />
                <asp:BoundField DataField="UrunParcaKodu" HeaderText="Parça Kodu" />
                <asp:BoundField DataField="UrunAdi" HeaderText="Ürün Adı" />
                <asp:BoundField DataField="Grami" HeaderText="Gramı" DataFormatString="{0:N2}" />
                <asp:BoundField DataField="KalipNo" HeaderText="Kalıp No" />
                <asp:BoundField DataField="ToplamUretimAdet" HeaderText="Toplam Üretim" DataFormatString="{0:N0}" />
                <asp:BoundField DataField="ToplamHammaddeGram" HeaderText="Toplam Hammadde (gr)" DataFormatString="{0:N0}" />

                <asp:TemplateField HeaderText="Bölümler">
                    <ItemTemplate><asp:Literal ID="litBolumler" runat="server"></asp:Literal></ItemTemplate>
                </asp:TemplateField>

                <asp:TemplateField HeaderText="Durum">
                    <ItemTemplate>
                        <span class='badge <%# GetStatusBadgeClass(Eval("Durum").ToString()) %>'>
                            <%# GetStatusText(Eval("Durum").ToString()) %>
                        </span>
                    </ItemTemplate>
                </asp:TemplateField>

                <asp:BoundField DataField="OlusturmaTarihi" HeaderText="Oluşturma Tarihi" DataFormatString="{0:dd.MM.yyyy HH:mm}" />

                <asp:TemplateField HeaderText="">
                    <ItemTemplate>
                        <asp:Panel ID="pnlDetay" runat="server" Visible="false" style="margin-top:10px;padding:12px;border:1px solid #e0e0e0;border-radius:8px;background:#fafafa;">
                            <div style="display:flex;gap:12px;flex-wrap:wrap;">
                                <asp:Panel ID="pnlBoyahaneDurum" runat="server" Visible="false" style="padding:10px;border-radius:8px;border-left:4px solid #ff9800;background:#fff;">
                                    <b>🎨 Boyahane Durum:</b> <asp:Label ID="lblBoyahaneDurum" runat="server" />
                                </asp:Panel>
                                <asp:Panel ID="pnlIcParcaDurum" runat="server" Visible="false" style="padding:10px;border-radius:8px;border-left:4px solid #2196f3;background:#fff;">
                                    <b>🔧 İç Parça Durum:</b> <asp:Label ID="lblIcParcaDurum" runat="server" />
                                </asp:Panel>
                            </div>
                        </asp:Panel>
                    </ItemTemplate>
                </asp:TemplateField>

                <asp:TemplateField HeaderText="İşlemler">
                    <ItemTemplate>
                        <div class="action-buttons">
                            <asp:Button ID="btnDuzenle" runat="server" CssClass="btn btn-warning btn-small"
                                Text="✏️ Düzenle" CommandName="Duzenle" CommandArgument='<%# Eval("IsEmriId") %>' />
                        </div>
                    </ItemTemplate>
                </asp:TemplateField>

            </Columns>
        </asp:GridView>
    </div>
</div>
    <script type="text/javascript">
        function openModal() { document.getElementById('modalDuzenle').classList.add('show'); }
        function closeModal() { document.getElementById('modalDuzenle').classList.remove('show'); }

        document.addEventListener('DOMContentLoaded', function () {
            var chkBoyahane = document.getElementById('<%= chkBoyahane.ClientID %>');
            var chkIcParca = document.getElementById('<%= chkIcParca.ClientID %>');
            var chkEnjeksiyon = document.getElementById('<%= chkEnjeksiyon.ClientID %>');

            if (chkBoyahane) chkBoyahane.onchange = function () { document.getElementById('boyahaneSection').style.display = this.checked ? 'block' : 'none'; };
            if (chkIcParca) chkIcParca.onchange = function () { document.getElementById('icparcaSection').style.display = this.checked ? 'block' : 'none'; };
            if (chkEnjeksiyon) chkEnjeksiyon.onchange = function () { document.getElementById('enjeksiyonSection').style.display = this.checked ? 'block' : 'none'; };
        });
    </script>
</asp:Content>
