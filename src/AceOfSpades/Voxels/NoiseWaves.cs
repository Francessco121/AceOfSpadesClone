/* NoiseWaves.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades
{
    public struct NoiseWaves
    {
        public double PrimaryWave;
        public double SecondaryWave;

        public NoiseWaves(double primeWave, double secondWave)
        {
            PrimaryWave = primeWave;
            SecondaryWave = secondWave;
        }
    }
}
