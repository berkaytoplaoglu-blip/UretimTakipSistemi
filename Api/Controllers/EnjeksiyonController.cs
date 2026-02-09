using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Http;
using UretimTakipSistemi.Api.Filters;
using UretimTakipSistemi.Api.Models;

namespace UretimTakipSistemi.Api.Controllers
{
    [ApiKeyAuth]
    [RoutePrefix("api/enjeksiyon")]
    public class EnjeksiyonController : ApiController
    {
        private string CS => ConfigurationManager.ConnectionStrings["SqlDB"].ConnectionString;

        // ✅ 1) İş emri bazlı toplam üretim/fire + son bitiş + running bilgisi
        // GET  /api/enjeksiyon/isemri-toplam?bas=2026-02-01&bit=2026-02-05
        [HttpGet]
        [Route("isemri-toplam")]
        public IHttpActionResult IsEmriToplam(string bas, string bit)
        {
            if (!DateTime.TryParse(bas, out var dtBas) || !DateTime.TryParse(bit, out var dtBit))
                return BadRequest("bas/bit geçersiz. Örn: 2026-02-01");

            if (dtBit.Date < dtBas.Date)
                return BadRequest("bit, bas'tan küçük olamaz.");

            var list = new List<EnjeksiyonIsEmriToplamDto>();

            const string q = @"
WITH Finished AS (
    SELECT
        eo.IsEmriNo,
        SUM(ISNULL(eo.UretimAdet,0)) AS UretimToplam,
        SUM(ISNULL(eo.FireAdet,0))   AS FireToplam,
        MAX(eo.BitisZamani)          AS LastFinishTime
    FROM EnjeksiyonOturum eo
    WHERE eo.BitisZamani IS NOT NULL
      AND CONVERT(date, eo.BitisZamani) BETWEEN @Bas AND @Bit
    GROUP BY eo.IsEmriNo
),
ActiveCounts AS (
    SELECT
        eo.IsEmriNo,
        COUNT(*) AS ActiveSessionCount
    FROM EnjeksiyonOturum eo
    WHERE eo.BitisZamani IS NULL
    GROUP BY eo.IsEmriNo
)
SELECT
    f.IsEmriNo,
    f.UretimToplam,
    f.FireToplam,
    f.LastFinishTime,

    ISNULL(ie.Durum,'') AS IsEmriDurum,
    CASE WHEN ISNULL(ie.Durum,'') = 'ACTIVE' THEN 1 ELSE 0 END AS IsRunning,

    ISNULL(ac.ActiveSessionCount, 0) AS ActiveSessionCount
FROM Finished f
LEFT JOIN IsEmirleri ie ON ie.IsEmriNo = f.IsEmriNo
LEFT JOIN ActiveCounts ac ON ac.IsEmriNo = f.IsEmriNo
ORDER BY f.IsEmriNo;";

            using (var conn = new SqlConnection(CS))
            using (var cmd = new SqlCommand(q, conn))
            {
                cmd.Parameters.Add("@Bas", SqlDbType.Date).Value = dtBas.Date;
                cmd.Parameters.Add("@Bit", SqlDbType.Date).Value = dtBit.Date;

                conn.Open();
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        string last = "";
                        if (r["LastFinishTime"] != DBNull.Value)
                            last = Convert.ToDateTime(r["LastFinishTime"]).ToString("yyyy-MM-ddTHH:mm:ss");

                        list.Add(new EnjeksiyonIsEmriToplamDto
                        {
                            IsEmriNo = r["IsEmriNo"].ToString(),
                            UretimToplam = Convert.ToInt32(r["UretimToplam"]),
                            FireToplam = Convert.ToInt32(r["FireToplam"]),
                            LastFinishTime = last,
                            IsEmriDurum = r["IsEmriDurum"].ToString(),
                            IsRunning = Convert.ToInt32(r["IsRunning"]) == 1,
                            ActiveSessionCount = Convert.ToInt32(r["ActiveSessionCount"])
                        });
                    }
                }
            }

            return Ok(list);
        }

        // ✅ 2) İş emri → hangi parça üretiliyor
        // GET  /api/enjeksiyon/is-emri-parca
        [HttpGet]
        [Route("is-emri-parca")]
        public IHttpActionResult IsEmriParcaListesi()
        {
            var list = new List<IsEmriParcaDto>();

            const string q = @"
SELECT
    ie.IsEmriNo,
    ie.ParcaId,
    ie.UrunParcaKodu,
    ie.UrunAdi,
    ISNULL(upl.Grami,0) AS Grami,
    ISNULL(upl.KalipNo,'') AS KalipNo,
    ie.Durum
FROM IsEmirleri ie
LEFT JOIN UretimParcaListesi upl ON upl.ParcaId = ie.ParcaId
ORDER BY ie.IsEmriNo;";

            using (var conn = new SqlConnection(CS))
            using (var cmd = new SqlCommand(q, conn))
            {
                conn.Open();
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        list.Add(new IsEmriParcaDto
                        {
                            IsEmriNo = r["IsEmriNo"].ToString(),
                            ParcaId = Convert.ToInt32(r["ParcaId"]),
                            UrunParcaKodu = r["UrunParcaKodu"].ToString(),
                            UrunAdi = r["UrunAdi"].ToString(),
                            Grami = Convert.ToDecimal(r["Grami"]),
                            KalipNo = r["KalipNo"].ToString(),
                            Durum = r["Durum"].ToString()
                        });
                    }
                }
            }

            return Ok(list);
        }
    }
}
