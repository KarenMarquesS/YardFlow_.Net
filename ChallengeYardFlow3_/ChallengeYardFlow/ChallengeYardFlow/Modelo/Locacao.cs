namespace ChallengeYardFlow.Modelo
{
    public class Locacao
    {
        public int Id { get; set; } 
        public int MotoId { get; set; } 
        public Moto Moto { get; set; } = null!; 

        public DateTime DataInicial { get; set; }
        public DateTime DataFinal { get; set; }

        public decimal ValorFinal { get; set; } 
    }
}
