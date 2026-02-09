<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="UretimTakipSistemi.Login" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Giriş - Üretim Takip Sistemi</title>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <style>
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            height: 100vh;
            display: flex;
            justify-content: center;
            align-items: center;
        }
        .login-container {
            background: white;
            padding: 40px;
            border-radius: 10px;
            box-shadow: 0 10px 40px rgba(0,0,0,0.3);
            width: 100%;
            max-width: 400px;
        }
        .login-header {
            text-align: center;
            margin-bottom: 30px;
        }
        .login-header h1 {
            color: #333;
            font-size: 28px;
            margin-bottom: 10px;
        }
        .login-header p {
            color: #666;
            font-size: 14px;
        }
        .form-group {
            margin-bottom: 20px;
        }
        .form-group label {
            display: block;
            margin-bottom: 8px;
            color: #333;
            font-weight: 500;
        }
        .form-group input {
            width: 100%;
            padding: 12px;
            border: 1px solid #ddd;
            border-radius: 5px;
            font-size: 14px;
            transition: border-color 0.3s;
        }
        .form-group input:focus {
            outline: none;
            border-color: #667eea;
        }
        .btn-login {
            width: 100%;
            padding: 14px;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            border: none;
            border-radius: 5px;
            font-size: 16px;
            font-weight: 600;
            cursor: pointer;
            transition: transform 0.2s;
        }
        .btn-login:hover {
            transform: translateY(-2px);
        }
        .error-message {
            background: #fee;
            border: 1px solid #fcc;
            color: #c33;
            padding: 12px;
            border-radius: 5px;
            margin-bottom: 20px;
            display: none;
        }
        .error-message.show {
            display: block;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="login-container">
            <div class="login-header">
                <h1>🏭 Üretim Takip</h1>
                <p>Sisteme giriş yapın</p>
            </div>

            <asp:Panel ID="pnlHata" runat="server" CssClass="error-message">
                <asp:Literal ID="litHata" runat="server"></asp:Literal>
            </asp:Panel>

            <div class="form-group">
                <label>Kullanıcı Adı</label>
                <asp:TextBox ID="txtKullaniciAdi" runat="server" placeholder="Kullanıcı adınızı girin"></asp:TextBox>
            </div>

            <div class="form-group">
                <label>Şifre</label>
                <asp:TextBox ID="txtSifre" runat="server" TextMode="Password" placeholder="Şifrenizi girin"></asp:TextBox>
            </div>

            <asp:Button ID="btnGiris" runat="server" CssClass="btn-login" Text="Giriş Yap" OnClick="btnGiris_Click" />
        </div>
    </form>
</body>
</html>