namespace RectalCancerDiagnosisWeb.Models
{
    public class PredictionResultViewModel
    {
        public string ResultMessage { get; set; } // Tahmin sonucu mesajı
        public string NiiPath { get; set; } // NII dosyasının yolu
        public string MRIName { get; set; } // MRI adı
        public string VisualizationBase64 { get; set; } // Base64 kodlu görselleştirme
    }
}
