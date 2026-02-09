<%@ Page Title="Enjeksiyon Terminal" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="EnjeksiyonTerminal.aspx.cs" Inherits="UretimTakipSistemi.Enjeksiyon.EnjeksiyonTerminal" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        * { box-sizing: border-box; }
        :root{
            --pad: clamp(10px, 1.4vw, 20px);
            --gap: clamp(10px, 1.4vw, 20px);
            --radius: clamp(8px, 1vw, 14px);

            --fs-xs: clamp(12px, 1.0vw, 14px);
            --fs-sm: clamp(13px, 1.1vw, 16px);
            --fs-md: clamp(14px, 1.2vw, 18px);
            --fs-lg: clamp(18px, 1.8vw, 28px);
            --fs-xl: clamp(26px, 3.2vw, 44px);

            --btn-pad: clamp(16px, 2.2vw, 40px);
            --btn-fs: clamp(16px, 2.2vw, 28px);
        }

        .terminal-container {
            background: #1a1a1a;
            color: white;
            min-height: calc(100vh - 200px);
            padding: var(--pad);
            border-radius: var(--radius);
            max-width: 1300px;
            margin: 0 auto;
        }

        .terminal-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            background: #2a2a2a;
            padding: var(--pad);
            border-radius: var(--radius);
            margin-bottom: var(--gap);
            gap: var(--gap);
        }

        .header-left {
            display: flex;
            flex-direction: column;
            gap: clamp(6px, 0.8vw, 10px);
            min-width: 0;
        }

        .machine-name {
            font-size: var(--fs-lg);
            font-weight: 800;
            color: #4caf50;
            white-space: nowrap;
            overflow: hidden;
            text-overflow: ellipsis;
            max-width: 62vw;
        }

        .personel-name {
            font-size: var(--fs-md);
            color: #fff;
            white-space: nowrap;
            overflow: hidden;
            text-overflow: ellipsis;
            max-width: 62vw;
        }

        .header-right { text-align: right; }

        .live-date {
            font-size: var(--fs-md);
            color: #bbb;
            margin-bottom: 4px;
            white-space: nowrap;
        }

        .live-clock {
            font-size: var(--fs-xl);
            font-weight: 900;
            color: #4fc3f7;
            font-family: 'Courier New', monospace;
            letter-spacing: 1px;
        }

        .main-content {
            display: grid;
            grid-template-columns: 2fr 1fr;
            gap: var(--gap);
            margin-bottom: var(--gap);
        }

        .info-panel, .status-panel {
            background: #2a2a2a;
            padding: calc(var(--pad) + 4px);
            border-radius: var(--radius);
        }

        .status-panel {
            display: flex;
            flex-direction: column;
            gap: clamp(10px, 1.2vw, 16px);
            justify-content: flex-start;
        }

        .info-row {
            display: flex;
            justify-content: space-between;
            gap: var(--gap);
            padding: clamp(8px, 1vw, 12px) 0;
            border-bottom: 1px solid #444;
        }

        .info-label { color: #bbb; font-size: var(--fs-sm); }
        .info-value { color: #fff; font-size: var(--fs-sm); font-weight: 700; text-align: right; }

        .status-box {
            background: #666;
            padding: clamp(18px, 2.0vw, 30px);
            border-radius: var(--radius);
            text-align: center;
            font-size: clamp(18px, 2.4vw, 34px);
            font-weight: 900;
            text-transform: uppercase;
            transition: all 0.3s;
            line-height: 1.1;
        }

        .status-idle { background: #666; color: #ccc; }

        .status-active {
            background: #4caf50;
            color: white;
            box-shadow: 0 0 20px rgba(76, 175, 80, 0.5);
            animation: pulse 2s infinite;
        }

        @keyframes pulse {
            0%, 100% { opacity: 1; }
            50% { opacity: 0.85; }
        }

        .status-paused {
            background: #ff9800;
            color: white;
            box-shadow: 0 0 20px rgba(255, 152, 0, 0.5);
        }

        .counters {
            display: grid;
            grid-template-columns: repeat(3, 1fr);
            gap: clamp(10px, 1.2vw, 16px);
            margin-top: var(--gap);
        }

        .counter-box {
            background: #1a1a1a;
            padding: clamp(12px, 1.6vw, 20px);
            border-radius: var(--radius);
            text-align: center;
        }

        .counter-label { font-size: var(--fs-xs); color: #bbb; margin-bottom: 8px; }
        .counter-value {
            font-size: clamp(18px, 2.6vw, 40px);
            font-weight: 900;
            color: #4fc3f7;
            font-family: 'Courier New', monospace;
        }
        .counter-unit { font-size: var(--fs-xs); color: #999; margin-top: 6px; }

        .button-panel { display: grid; grid-template-columns: repeat(2, 1fr); gap: var(--gap); }

        .industrial-btn {
            padding: var(--btn-pad);
            border: none;
            border-radius: clamp(10px, 1vw, 14px);
            font-size: var(--btn-fs);
            font-weight: 900;
            cursor: pointer;
            text-transform: uppercase;
            transition: all 0.2s;
            box-shadow: 0 5px 15px rgba(0,0,0,0.3);
            min-height: clamp(64px, 8vh, 120px);
        }

        .industrial-btn:disabled { opacity: 0.3; cursor: not-allowed; }
        .industrial-btn:not(:disabled):hover { transform: translateY(-2px); box-shadow: 0 8px 25px rgba(0,0,0,0.4); }

        .btn-start { background: linear-gradient(135deg, #4caf50 0%, #45a049 100%); color: white; }
        .btn-stop { background: linear-gradient(135deg, #ff9800 0%, #fb8c00 100%); color: white; }
        .btn-continue { background: linear-gradient(135deg, #2196f3 0%, #1976d2 100%); color: white; }
        .btn-finish { background: linear-gradient(135deg, #f44336 0%, #da190b 100%); color: white; }

        .modal {
            display: none;
            position: fixed;
            z-index: 9999;
            left: 0; top: 0;
            width: 100%; height: 100%;
            background: rgba(0,0,0,0.8);
        }

        .modal.show {
            display: flex;
            justify-content: center;
            align-items: center;
            padding: var(--pad);
        }

        .modal-content {
            background: #2a2a2a;
            padding: clamp(18px, 2.5vw, 40px);
            border-radius: var(--radius);
            width: min(600px, 96vw);
        }

        .modal-title { font-size: clamp(18px, 2.0vw, 26px); font-weight: 900; margin-bottom: 18px; color: white; }
        .form-group { margin-bottom: 18px; }
        .form-group label { display: block; margin-bottom: 8px; color: #bbb; font-size: var(--fs-sm); }
        .form-group select, .form-group input {
            width: 100%;
            padding: 12px;
            border: 1px solid #555;
            border-radius: 8px;
            background: #1a1a1a;
            color: white;
            font-size: var(--fs-md);
        }

        .modal-buttons { display: flex; gap: 12px; margin-top: 22px; }
        .modal-btn { flex: 1; padding: 14px; border: none; border-radius: 8px; font-size: var(--fs-md); font-weight: 800; cursor: pointer; }
        .modal-btn-confirm { background: #4caf50; color: white; }
        .modal-btn-cancel { background: #666; color: white; }

        .alert { padding: 15px 20px; border-radius: 8px; margin-bottom: 16px; font-size: var(--fs-md); }
        .alert-success { background: #4caf50; color: white; }
        .alert-error { background: #f44336; color: white; }
        .alert-warning { background: #ff9800; color: white; }

        @media (max-width: 980px) {
            .main-content { grid-template-columns: 1fr; }
            .button-panel { grid-template-columns: 1fr; }
            .machine-name, .personel-name { max-width: 84vw; }
            .counters { grid-template-columns: 1fr; }
        }

        @media (max-width: 520px) {
            .terminal-header { flex-direction: column; align-items: flex-start; }
            .header-right { text-align: left; width: 100%; }
            .live-date { white-space: normal; }
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="terminal-container">
        <asp:Panel ID="pnlMesaj" runat="server" Style="display:none;">
            <asp:Literal ID="litMesaj" runat="server"></asp:Literal>
        </asp:Panel>

        <div class="terminal-header">
            <div class="header-left">
                <div class="machine-name" id="machineName">-</div>
                <div class="personel-name" id="personelName">-</div>
            </div>
            <div class="header-right">
                <div class="live-date" id="liveDate"></div>
                <div class="live-clock" id="liveClock">00:00:00</div>
            </div>
        </div>

        <div class="main-content">
            <div class="info-panel">
                <h3 style="margin-bottom: 20px; color: #4fc3f7;">İş Emri Bilgileri</h3>
                <div class="info-row"><span class="info-label">İş Emri No:</span><span class="info-value" id="isEmriNo">-</span></div>
                <div class="info-row"><span class="info-label">Ürün Adı:</span><span class="info-value" id="urunAdi">-</span></div>
                <div class="info-row"><span class="info-label">Çevrim Süresi:</span><span class="info-value" id="cevrimSuresi">-</span></div>
                <div class="info-row"><span class="info-label">Soğuma Süresi:</span><span class="info-value" id="sogumaSuresi">-</span></div>
                <div class="info-row"><span class="info-label">Başlangıç Zamanı:</span><span class="info-value" id="baslangicZamani">-</span></div>

                <div class="counters">
                    <div class="counter-box">
                        <div class="counter-label">Toplam Süre</div>
                        <div class="counter-value" id="toplamSure">00:00:00</div>
                        <div class="counter-unit">saat:dakika:saniye</div>
                    </div>
                    <div class="counter-box">
                        <div class="counter-label">Toplam Duruş</div>
                        <div class="counter-value" id="toplamDurus">00:00:00</div>
                        <div class="counter-unit">saat:dakika:saniye</div>
                    </div>
                    <div class="counter-box">
                        <div class="counter-label">Net Üretim</div>
                        <div class="counter-value" id="netSure">00:00:00</div>
                        <div class="counter-unit">saat:dakika:saniye</div>
                    </div>
                </div>
            </div>

            <div class="status-panel">
                <div id="statusBox" class="status-box status-idle">HAZIR</div>
            </div>
        </div>

        <div class="button-panel">
            <asp:Button ID="btnBaslat" runat="server" CssClass="industrial-btn btn-start" Text="🟢 BAŞLAT" OnClientClick="return false;" />
            <asp:Button ID="btnDurdur" runat="server" CssClass="industrial-btn btn-stop" Text="🟡 DURDUR" OnClientClick="return false;" Enabled="false" />
            <asp:Button ID="btnDevam" runat="server" CssClass="industrial-btn btn-continue" Text="🔵 DEVAM" OnClientClick="return false;" Enabled="false" />
            <asp:Button ID="btnBitir" runat="server" CssClass="industrial-btn btn-finish" Text="🔴 BİTİR" OnClientClick="return false;" Enabled="false" />
        </div>
    </div>

    <!-- Başlat Modal -->
    <div id="modalBaslat" class="modal">
        <div class="modal-content">
            <div class="modal-title">🟢 Yeni Oturum Başlat</div>
            <div class="form-group">
                <label>Makine Seçiniz *</label>
                <asp:DropDownList ID="ddlMakine" runat="server" ClientIDMode="Static"></asp:DropDownList>
            </div>
            <div class="form-group">
                <label>İş Emri Seçiniz *</label>
                <asp:DropDownList ID="ddlIsEmri" runat="server" ClientIDMode="Static"></asp:DropDownList>
            </div>
            <div class="modal-buttons">
                <button type="button" class="modal-btn modal-btn-confirm" onclick="confirmBaslat()">BAŞLAT</button>
                <button type="button" class="modal-btn modal-btn-cancel" onclick="closeModal('modalBaslat')">İPTAL</button>
            </div>
        </div>
    </div>

    <!-- Durdur Modal -->
    <div id="modalDurdur" class="modal">
        <div class="modal-content">
            <div class="modal-title">🟡 Duruş Nedeni</div>
            <div class="form-group">
                <label>Duruş Nedeni Seçiniz *</label>
                <asp:DropDownList ID="ddlDurusNedeni" runat="server" ClientIDMode="Static"></asp:DropDownList>
            </div>
            <div class="modal-buttons">
                <button type="button" class="modal-btn modal-btn-confirm" onclick="confirmDurdur()">DURDUR</button>
                <button type="button" class="modal-btn modal-btn-cancel" onclick="closeModal('modalDurdur')">İPTAL</button>
            </div>
        </div>
    </div>

    <!-- Bitir Modal -->
    <div id="modalBitir" class="modal">
        <div class="modal-content">
            <div class="modal-title">🔴 Oturumu Bitir</div>
            <div class="form-group">
                <label>Üretim Adet *</label>
                <input type="number" id="txtUretimAdet" min="0" placeholder="1000" />
            </div>
            <div class="form-group">
                <label>Fire Adet</label>
                <input type="number" id="txtFireAdet" min="0" value="0" placeholder="0" onchange="checkFireReason()" />
            </div>
            <div class="form-group" id="fireReasonGroup" style="display:none;">
                <label>Fire Nedeni *</label>
                <asp:DropDownList ID="ddlFireNedeni" runat="server" ClientIDMode="Static"></asp:DropDownList>
            </div>
            <div class="modal-buttons">
                <button type="button" class="modal-btn modal-btn-confirm" onclick="confirmBitir()">BİTİR</button>
                <button type="button" class="modal-btn modal-btn-cancel" onclick="closeModal('modalBitir')">İPTAL</button>
            </div>
        </div>
    </div>

    <asp:HiddenField ID="hfOturumId" runat="server" Value="0" />
    <asp:HiddenField ID="hfBaslangicZamani" runat="server" Value="" />
    <asp:HiddenField ID="hfToplamDurus" runat="server" Value="0" />

    <script type="text/javascript">
        var updateInterval;
        var clockInterval;
        var oturumId = 0;
        var durusBaslangic = null;
        var oturumBaslangicZamani = null;

        window.onload = function () {
            initializePage();
            startClock();
            checkActiveSession();
        };

        function initializePage() {
            document.getElementById('<%= btnBaslat.ClientID %>').onclick = function () { openModal('modalBaslat'); return false; };
            document.getElementById('<%= btnDurdur.ClientID %>').onclick = function () { openModal('modalDurdur'); return false; };
            document.getElementById('<%= btnDevam.ClientID %>').onclick = function () { devamEt(); return false; };
            document.getElementById('<%= btnBitir.ClientID %>').onclick = function () { openModal('modalBitir'); return false; };

            updateInterval = setInterval(function () {
                if (oturumId > 0) checkActiveSession();
            }, 5000);
        }

        function startClock() {
            updateClock();
            clockInterval = setInterval(updateClock, 1000);
        }

        function updateClock() {
            var now = new Date();

            var days = ['Pazar', 'Pazartesi', 'Salı', 'Çarşamba', 'Perşembe', 'Cuma', 'Cumartesi'];
            var months = ['Ocak', 'Şubat', 'Mart', 'Nisan', 'Mayıs', 'Haziran', 'Temmuz', 'Ağustos', 'Eylül', 'Ekim', 'Kasım', 'Aralık'];

            document.getElementById('liveDate').textContent =
                days[now.getDay()] + ', ' + now.getDate() + ' ' + months[now.getMonth()] + ' ' + now.getFullYear();

            document.getElementById('liveClock').textContent =
                String(now.getHours()).padStart(2, '0') + ':' +
                String(now.getMinutes()).padStart(2, '0') + ':' +
                String(now.getSeconds()).padStart(2, '0');

            if (oturumId > 0 && oturumBaslangicZamani) updateCounters();
        }

        function updateCounters() {
            if (!oturumBaslangicZamani) return;

            var now = new Date();
            var elapsedSeconds = Math.floor((now - oturumBaslangicZamani) / 1000);

            var toplamDurus = parseInt(document.getElementById('<%= hfToplamDurus.ClientID %>').value || '0', 10);

            if (document.getElementById('statusBox').textContent === 'DURUŞTA' && durusBaslangic) {
                var currentDurusSeconds = Math.floor((now - durusBaslangic) / 1000);
                toplamDurus += currentDurusSeconds;
            }

            var netSeconds = Math.max(0, elapsedSeconds - toplamDurus);

            document.getElementById('toplamSure').textContent = formatTime(elapsedSeconds);
            document.getElementById('toplamDurus').textContent = formatTime(toplamDurus);
            document.getElementById('netSure').textContent = formatTime(netSeconds);
        }

        function formatTime(seconds) {
            var h = Math.floor(seconds / 3600);
            var m = Math.floor((seconds % 3600) / 60);
            var s = seconds % 60;
            return String(h).padStart(2, '0') + ':' + String(m).padStart(2, '0') + ':' + String(s).padStart(2, '0');
        }

        function checkActiveSession() {
            PageMethods.GetActiveSession(onGetSessionSuccess, onGetSessionError);
        }

        function onGetSessionSuccess(result) {
            if (result && result.OturumId > 0) {
                oturumId = result.OturumId;

                document.getElementById('<%= hfOturumId.ClientID %>').value = result.OturumId;
                document.getElementById('<%= hfBaslangicZamani.ClientID %>').value = result.BaslangicZamani;
                document.getElementById('<%= hfToplamDurus.ClientID %>').value = result.ToplamDurusSuresi;

                oturumBaslangicZamani = new Date(result.BaslangicZamani);

                // ✅ DURUŞTA ise duruş başlangıcı DB’den gelsin (sayfa kapanıp açınca sıfırlamasın)
                if (result.Durum === 'PAUSED' && result.DurusBaslangicZamani) {
                    durusBaslangic = new Date(result.DurusBaslangicZamani);
                }
                if (result.Durum === 'ACTIVE') {
                    durusBaslangic = null;
                }

                loadSessionDetails(result.OturumId);
                updateButtonStates(result.Durum);
            } else {
                resetScreen();
            }
        }

        function onGetSessionError(error) {
            console.error('Oturum kontrol hatası:', error);
        }

        function loadSessionDetails(oturumIdParam) {
            PageMethods.GetSessionDetails(oturumIdParam, onSessionDetailsSuccess, onSessionDetailsError);
        }

        function onSessionDetailsSuccess(result) {
            if (result) {
                document.getElementById('machineName').textContent = '🏭 ' + result.Makine;
                document.getElementById('personelName').textContent = '👤 ' + result.Personel;
                document.getElementById('isEmriNo').textContent = result.IsEmriNo;
                document.getElementById('urunAdi').textContent = result.UrunAdi;
                document.getElementById('cevrimSuresi').textContent = result.CevrimSuresi + ' saniye';
                document.getElementById('sogumaSuresi').textContent = result.SogumaSuresi + ' saniye';
                document.getElementById('baslangicZamani').textContent = result.BaslangicZamaniStr;
            }
        }

        function onSessionDetailsError(error) {
            console.error('Oturum detay hatası:', error);
        }

        function updateButtonStates(durum) {
            var btnBaslat = document.getElementById('<%= btnBaslat.ClientID %>');
            var btnDurdur = document.getElementById('<%= btnDurdur.ClientID %>');
            var btnDevam = document.getElementById('<%= btnDevam.ClientID %>');
            var btnBitir = document.getElementById('<%= btnBitir.ClientID %>');
            var statusBox = document.getElementById('statusBox');

            btnBaslat.disabled = true;
            btnDurdur.disabled = true;
            btnDevam.disabled = true;
            btnBitir.disabled = true;

            if (durum === 'ACTIVE') {
                btnDurdur.disabled = false;
                btnBitir.disabled = false;
                statusBox.className = 'status-box status-active';
                statusBox.textContent = 'ÇALIŞIYOR';
            }
            else if (durum === 'PAUSED') {
                btnDevam.disabled = false;
                btnBitir.disabled = false;
                statusBox.className = 'status-box status-paused';
                statusBox.textContent = 'DURUŞTA';

                // ✅ Eğer DB’den gelmediyse (ilk durdurma anı) en son çare now
                if (!durusBaslangic) durusBaslangic = new Date();
            }
            else {
                btnBaslat.disabled = false;
                statusBox.className = 'status-box status-idle';
                statusBox.textContent = 'HAZIR';
            }
        }

        function resetScreen() {
            oturumId = 0;
            durusBaslangic = null;
            oturumBaslangicZamani = null;

            document.getElementById('<%= hfOturumId.ClientID %>').value = '0';
            document.getElementById('machineName').textContent = '-';
            document.getElementById('personelName').textContent = '-';
            document.getElementById('isEmriNo').textContent = '-';
            document.getElementById('urunAdi').textContent = '-';
            document.getElementById('cevrimSuresi').textContent = '-';
            document.getElementById('sogumaSuresi').textContent = '-';
            document.getElementById('baslangicZamani').textContent = '-';
            document.getElementById('toplamSure').textContent = '00:00:00';
            document.getElementById('toplamDurus').textContent = '00:00:00';
            document.getElementById('netSure').textContent = '00:00:00';

            updateButtonStates('IDLE');
        }

        function openModal(modalId) { document.getElementById(modalId).classList.add('show'); }
        function closeModal(modalId) { document.getElementById(modalId).classList.remove('show'); }

        function checkFireReason() {
            var fireAdet = parseInt(document.getElementById('txtFireAdet').value || '0', 10);
            document.getElementById('fireReasonGroup').style.display = (fireAdet > 0) ? 'block' : 'none';
        }

        function confirmBaslat() {
            var makineId = document.getElementById('ddlMakine').value;
            var isEmriId = document.getElementById('ddlIsEmri').value;

            if (makineId === '0' || isEmriId === '0') { alert('Lütfen makine ve iş emri seçiniz!'); return; }

            var btnConfirm = event.target;
            btnConfirm.disabled = true;
            btnConfirm.textContent = 'Başlatılıyor...';

            PageMethods.OturumBaslat(parseInt(makineId, 10), parseInt(isEmriId, 10), onBaslatSuccess, onBaslatError);
        }

        function onBaslatSuccess(result) {
            if (result.Success) location.reload();
            else {
                closeModal('modalBaslat');
                showMessage(result.Message, 'error');
                var btnConfirm = document.querySelector('#modalBaslat .modal-btn-confirm');
                if (btnConfirm) { btnConfirm.disabled = false; btnConfirm.textContent = 'BAŞLAT'; }
            }
        }

        function onBaslatError(error) {
            closeModal('modalBaslat');
            var btnConfirm = document.querySelector('#modalBaslat .modal-btn-confirm');
            if (btnConfirm) { btnConfirm.disabled = false; btnConfirm.textContent = 'BAŞLAT'; }
            showMessage('Hata: ' + error.get_message(), 'error');
        }

        function confirmDurdur() {
            var nedenId = document.getElementById('ddlDurusNedeni').value;
            if (nedenId === '0') { alert('Lütfen duruş nedeni seçiniz!'); return; }

            var currentOturumId = parseInt(document.getElementById('<%= hfOturumId.ClientID %>').value, 10);
            PageMethods.OturumDurdur(currentOturumId, parseInt(nedenId, 10), onDurdurSuccess, onDurdurError);
        }

        function onDurdurSuccess(result) {
            closeModal('modalDurdur');
            if (result.Success) {
                durusBaslangic = new Date(); // anlık UI
                showMessage(result.Message, 'warning');
                setTimeout(checkActiveSession, 500); // DB’den gerçek duruş başlangıcını çeker
            } else showMessage(result.Message, 'error');
        }

        function onDurdurError(error) {
            closeModal('modalDurdur');
            showMessage('Hata: ' + error.get_message(), 'error');
        }

        function devamEt() {
            var currentOturumId = parseInt(document.getElementById('<%= hfOturumId.ClientID %>').value, 10);
            PageMethods.OturumDevam(currentOturumId, onDevamSuccess, onDevamError);
        }

        function onDevamSuccess(result) {
            if (result.Success) {
                durusBaslangic = null;
                showMessage(result.Message, 'success');
                setTimeout(checkActiveSession, 500);
            } else showMessage(result.Message, 'error');
        }

        function onDevamError(error) {
            showMessage('Hata: ' + error.get_message(), 'error');
        }

        function confirmBitir() {
            var uretimAdet = document.getElementById('txtUretimAdet').value;
            var fireAdet = document.getElementById('txtFireAdet').value || '0';

            if (!uretimAdet || parseInt(uretimAdet, 10) <= 0) { alert('Lütfen geçerli bir üretim adet giriniz!'); return; }

            var fireNedeni = '';
            if (parseInt(fireAdet, 10) > 0) {
                var ddl = document.getElementById('ddlFireNedeni');
                if (ddl.value === '0') { alert('Fire girildi! Lütfen fire nedeni seçiniz!'); return; }
                fireNedeni = ddl.options[ddl.selectedIndex].text;
            }

            var currentOturumId = parseInt(document.getElementById('<%= hfOturumId.ClientID %>').value, 10);
            PageMethods.OturumBitir(currentOturumId, parseInt(uretimAdet, 10), parseInt(fireAdet, 10), fireNedeni, onBitirSuccess, onBitirError);
        }

        function onBitirSuccess(result) {
            closeModal('modalBitir');
            if (result.Success) {
                showMessage(result.Message, 'success');
                setTimeout(function () {
                    resetScreen();
                    document.getElementById('txtUretimAdet').value = '';
                    document.getElementById('txtFireAdet').value = '0';
                    document.getElementById('fireReasonGroup').style.display = 'none';
                }, 1200);
            } else showMessage(result.Message, 'error');
        }

        function onBitirError(error) {
            closeModal('modalBitir');
            showMessage('Hata: ' + error.get_message(), 'error');
        }

        function showMessage(message, type) {
            var alertClass = 'alert-success';
            if (type === 'error') alertClass = 'alert-error';
            if (type === 'warning') alertClass = 'alert-warning';

            document.getElementById('<%= litMesaj.ClientID %>').innerHTML = '<div class="alert ' + alertClass + '">' + message + '</div>';
            document.getElementById('<%= pnlMesaj.ClientID %>').style.display = 'block';

            setTimeout(function () {
                document.getElementById('<%= pnlMesaj.ClientID %>').style.display = 'none';
            }, 5000);
        }

        window.onbeforeunload = function () {
            if (updateInterval) clearInterval(updateInterval);
            if (clockInterval) clearInterval(clockInterval);
        };
    </script>
</asp:Content>
