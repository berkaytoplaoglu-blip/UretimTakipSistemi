namespace UretimTakipSistemi.Api.Models
{
    public class IsEmriParcaDto
    {
        public string IsEmriNo { get; set; }
        public int ParcaId { get; set; }
        public string UrunParcaKodu { get; set; }
        public string UrunAdi { get; set; }
        public decimal Grami { get; set; }
        public string KalipNo { get; set; }
        public string Durum { get; set; }
    }
}
