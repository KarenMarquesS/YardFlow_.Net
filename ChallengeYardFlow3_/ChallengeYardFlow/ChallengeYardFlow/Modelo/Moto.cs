namespace ChallengeYardFlow.Modelo
{
    public class Moto
    {

        public int Id { get; private set; } 
        public string Placa { get; set; } = null!;
        public string Modelo { get; set; } = null!;
        public int IdMotor { get; set; }
        public decimal ValorDiaria { get; set; }

        public void AtribuiMotoCodigo(int i)
        {
            Id = i;
        }
    }
}
