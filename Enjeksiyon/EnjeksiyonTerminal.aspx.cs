using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;
using UretimTakipSistemi.Helpers;

namespace UretimTakipSistemi.Enjeksiyon
{
    public partial class EnjeksiyonTerminal : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!SessionHelper.IsLoggedIn)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!SessionHelper.IsAdmin && !SessionHelper.IsEnjeksiyon)
            {
                Response.Redirect("~/Default.aspx");
                return;
            }

            if (!IsPostBack)
            {
                LoadDropdowns();
            }
        }

        private void LoadDropdowns()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlDB"].ConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("sp_MakineListesi", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@Bolum", SqlDbType.NVarChar, 20).Value = "Enjeksiyon";

                        conn.Open();
                        SqlDataReader reader = cmd.ExecuteReader();

                        ddlMakine.Items.Clear();
                        ddlMakine.Items.Add(new ListItem("-- Makine Seçiniz --", "0"));

                        while (reader.Read())
                        {
                            ddlMakine.Items.Add(new ListItem(
                                reader["MakineAdi"].ToString(),
                                reader["MakineId"].ToString()
                            ));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Makine yükleme hatası: " + ex.Message);
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlDB"].ConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("sp_AktifIsEmirleri", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@Bolum", SqlDbType.NVarChar, 20).Value = "Enjeksiyon";

                        conn.Open();
                        SqlDataReader reader = cmd.ExecuteReader();

                        ddlIsEmri.Items.Clear();
                        ddlIsEmri.Items.Add(new ListItem("-- İş Emri Seçiniz --", "0"));

                        while (reader.Read())
                        {
                            ddlIsEmri.Items.Add(new ListItem(
                                reader["IsEmriNo"].ToString() + " - " + reader["UrunAdi"].ToString(),
                                reader["IsEmriId"].ToString()
                            ));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("İş emri yükleme hatası: " + ex.Message);
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlDB"].ConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("sp_DurusNedenleriListesi", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        conn.Open();
                        SqlDataReader reader = cmd.ExecuteReader();

                        ddlDurusNedeni.Items.Clear();
                        ddlDurusNedeni.Items.Add(new ListItem("-- Duruş Nedeni Seçiniz --", "0"));

                        ddlFireNedeni.Items.Clear();
                        ddlFireNedeni.Items.Add(new ListItem("-- Fire Nedeni Seçiniz --", "0"));

                        while (reader.Read())
                        {
                            string nedenAdi = reader["NedenAdi"].ToString();
                            string nedenId = reader["NedenId"].ToString();

                            ddlDurusNedeni.Items.Add(new ListItem(nedenAdi, nedenId));
                            ddlFireNedeni.Items.Add(new ListItem(nedenAdi, nedenId));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Duruş nedeni yükleme hatası: " + ex.Message);
            }
        }

        // ✅ FIX: Sayfa kapanıp açılınca duruş sıfırlanmasın diye
        // ✅ FIX: Reader kapatınca null dönmesin diye (tüm alanları önce değişkene alıyoruz)
        [WebMethod(EnableSession = true)]
        public static ActiveSessionModel GetActiveSession()
        {
            try
            {
                int personelId = SessionHelper.KullaniciId;

                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlDB"].ConnectionString))
                using (SqlCommand cmd = new SqlCommand("sp_EnjeksiyonAktifOturumKontrol", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@PersonelId", SqlDbType.Int).Value = personelId;

                    conn.Open();

                    int oturumId = 0;
                    int makineId = 0;
                    string isEmriNo = "";
                    string durum = "";
                    string baslangicZamaniStr = "";
                    int toplamDurusSuresi = 0;

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                            return null;

                        oturumId = Convert.ToInt32(reader["OturumId"]);
                        makineId = Convert.ToInt32(reader["MakineId"]);
                        isEmriNo = reader["IsEmriNo"].ToString();
                        durum = reader["Durum"].ToString();
                        baslangicZamaniStr = Convert.ToDateTime(reader["BaslangicZamani"]).ToString("yyyy-MM-ddTHH:mm:ss");
                        toplamDurusSuresi = Convert.ToInt32(reader["ToplamDurusSuresi"]);
                    }

                    // PAUSED ise: açık duruş başlangıcını DB’den çek (Bitis IS NULL)
                    string durusBaslangicStr = null;
                    if (durum == "PAUSED")
                    {
                        using (SqlCommand cmdDurus = new SqlCommand(@"
                            SELECT TOP 1 Baslangic
                            FROM EnjeksiyonDurus
                            WHERE OturumId = @OturumId AND Bitis IS NULL
                            ORDER BY Baslangic DESC", conn))
                        {
                            cmdDurus.Parameters.Add("@OturumId", SqlDbType.Int).Value = oturumId;

                            object bas = cmdDurus.ExecuteScalar();
                            if (bas != null && bas != DBNull.Value)
                                durusBaslangicStr = Convert.ToDateTime(bas).ToString("yyyy-MM-ddTHH:mm:ss");
                        }
                    }

                    return new ActiveSessionModel
                    {
                        OturumId = oturumId,
                        MakineId = makineId,
                        IsEmriNo = isEmriNo,
                        Durum = durum,
                        BaslangicZamani = baslangicZamaniStr,
                        ToplamDurusSuresi = toplamDurusSuresi,
                        DurusBaslangicZamani = durusBaslangicStr
                    };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Aktif oturum kontrol hatası: " + ex.Message);
            }

            return null;
        }

        [WebMethod(EnableSession = true)]
        public static SessionDetailModel GetSessionDetails(int oturumId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlDB"].ConnectionString))
                using (SqlCommand cmd = new SqlCommand("sp_EnjeksiyonOturumDetay", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@OturumId", SqlDbType.Int).Value = oturumId;

                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        return new SessionDetailModel
                        {
                            Personel = reader["Personel"].ToString(),
                            Makine = reader["Makine"].ToString(),
                            IsEmriNo = reader["IsEmriNo"].ToString(),
                            UrunAdi = reader["UrunAdi"].ToString(),
                            CevrimSuresi = reader["CevrimSuresi"] != DBNull.Value ? Convert.ToInt32(reader["CevrimSuresi"]) : 0,
                            SogumaSuresi = reader["SogumaSuresi"] != DBNull.Value ? Convert.ToInt32(reader["SogumaSuresi"]) : 0,
                            BaslangicZamaniStr = Convert.ToDateTime(reader["BaslangicZamani"]).ToString("dd.MM.yyyy HH:mm:ss")
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Oturum detay hatası: " + ex.Message);
            }

            return null;
        }

        [WebMethod(EnableSession = true)]
        public static OperationResult OturumBaslat(int makineId, int isEmriId)
        {
            try
            {
                int personelId = SessionHelper.KullaniciId;

                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlDB"].ConnectionString))
                using (SqlCommand cmd = new SqlCommand("sp_EnjeksiyonOturumBaslat", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@PersonelId", SqlDbType.Int).Value = personelId;
                    cmd.Parameters.Add("@MakineId", SqlDbType.Int).Value = makineId;
                    cmd.Parameters.Add("@IsEmriId", SqlDbType.Int).Value = isEmriId;

                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        int result = Convert.ToInt32(reader["Result"]);
                        string message = reader["Message"].ToString();

                        return new OperationResult
                        {
                            Success = result > 0,
                            Message = message
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                return new OperationResult
                {
                    Success = false,
                    Message = "Hata: " + ex.Message
                };
            }

            return new OperationResult { Success = false, Message = "Bilinmeyen hata" };
        }

        [WebMethod(EnableSession = true)]
        public static OperationResult OturumDurdur(int oturumId, int durusNedeniId)
        {
            try
            {
                string durusNedeni = "";

                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlDB"].ConnectionString))
                using (SqlCommand cmd = new SqlCommand("SELECT NedenAdi FROM DurusNedenleri WHERE NedenId = @NedenId", conn))
                {
                    cmd.Parameters.Add("@NedenId", SqlDbType.Int).Value = durusNedeniId;
                    conn.Open();
                    object result = cmd.ExecuteScalar();
                    if (result != null)
                        durusNedeni = result.ToString();
                }

                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlDB"].ConnectionString))
                using (SqlCommand cmd = new SqlCommand("sp_EnjeksiyonOturumDurdur", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@OturumId", SqlDbType.Int).Value = oturumId;
                    cmd.Parameters.Add("@DurusNedeni", SqlDbType.NVarChar, 200).Value = durusNedeni;

                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        int result = Convert.ToInt32(reader["Result"]);
                        string message = reader["Message"].ToString();

                        return new OperationResult
                        {
                            Success = result > 0,
                            Message = message
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                return new OperationResult
                {
                    Success = false,
                    Message = "Hata: " + ex.Message
                };
            }

            return new OperationResult { Success = false, Message = "Bilinmeyen hata" };
        }

        [WebMethod(EnableSession = true)]
        public static OperationResult OturumDevam(int oturumId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlDB"].ConnectionString))
                using (SqlCommand cmd = new SqlCommand("sp_EnjeksiyonOturumDevam", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@OturumId", SqlDbType.Int).Value = oturumId;

                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        int result = Convert.ToInt32(reader["Result"]);
                        string message = reader["Message"].ToString();

                        return new OperationResult
                        {
                            Success = result > 0,
                            Message = message
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                return new OperationResult
                {
                    Success = false,
                    Message = "Hata: " + ex.Message
                };
            }

            return new OperationResult { Success = false, Message = "Bilinmeyen hata" };
        }

        [WebMethod(EnableSession = true)]
        public static OperationResult OturumBitir(int oturumId, int uretimAdet, int fireAdet, string fireNedeni)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlDB"].ConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("sp_EnjeksiyonOturumBitir", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@OturumId", SqlDbType.Int).Value = oturumId;
                        cmd.Parameters.Add("@UretimAdet", SqlDbType.Int).Value = uretimAdet;
                        cmd.Parameters.Add("@FireAdet", SqlDbType.Int).Value = fireAdet;

                        conn.Open();
                        SqlDataReader reader = cmd.ExecuteReader();

                        if (reader.Read())
                        {
                            int result = Convert.ToInt32(reader["Result"]);
                            string message = reader["Message"].ToString();

                            // Fire nedeni varsa kaydet
                            if (result > 0 && fireAdet > 0 && !string.IsNullOrEmpty(fireNedeni))
                            {
                                reader.Close();

                                string insertFireQuery = @"
                                    INSERT INTO EnjeksiyonFireKayit (OturumId, FireAdet, FireNedeni, KayitTarihi)
                                    VALUES (@OturumId, @FireAdet, @FireNedeni, GETDATE())";

                                using (SqlCommand cmdFire = new SqlCommand(insertFireQuery, conn))
                                {
                                    cmdFire.Parameters.Add("@OturumId", SqlDbType.Int).Value = oturumId;
                                    cmdFire.Parameters.Add("@FireAdet", SqlDbType.Int).Value = fireAdet;
                                    cmdFire.Parameters.Add("@FireNedeni", SqlDbType.NVarChar, 200).Value = fireNedeni;
                                    cmdFire.ExecuteNonQuery();
                                }
                            }

                            return new OperationResult
                            {
                                Success = result > 0,
                                Message = message
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return new OperationResult
                {
                    Success = false,
                    Message = "Hata: " + ex.Message
                };
            }

            return new OperationResult { Success = false, Message = "Bilinmeyen hata" };
        }
    }

    public class ActiveSessionModel
    {
        public int OturumId { get; set; }
        public int MakineId { get; set; }
        public string IsEmriNo { get; set; }
        public string Durum { get; set; }
        public string BaslangicZamani { get; set; }
        public int ToplamDurusSuresi { get; set; }

        // ✅ Yeni alan: sayfa kapanıp açılınca duruşun kaldığı yerden devam etmesi için
        public string DurusBaslangicZamani { get; set; }
    }

    public class SessionDetailModel
    {
        public string Personel { get; set; }
        public string Makine { get; set; }
        public string IsEmriNo { get; set; }
        public string UrunAdi { get; set; }
        public int CevrimSuresi { get; set; }
        public int SogumaSuresi { get; set; }
        public string BaslangicZamaniStr { get; set; }
    }

    public class OperationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}
