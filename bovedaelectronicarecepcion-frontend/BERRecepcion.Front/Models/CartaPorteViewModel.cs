namespace BERRecepcion.Front.Models
{
    public class CartaPorteViewModel
    {
        public CartaPorteMercanciasMercancia Mercancia { get; set; }
        public CartaPorteMercanciasAutotransporte Autotransporte { get; set; }
        public CartaPorteTiposFigura FiguraTransporte { get; set; }
        public CartaPorteUbicacion Origen { get; set; }
        public CartaPorteUbicacion Destino { get; set; }
    }
}
