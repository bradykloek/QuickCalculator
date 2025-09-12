namespace QuickCalculator.Symbols
{
    internal abstract class CalcFunction
    {
        public abstract double Execute(List<double> args);
        public abstract int NumParameters();
    }
}
