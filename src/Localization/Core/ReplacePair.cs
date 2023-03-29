namespace Localization2
{
    public class ReplacePair
    {
        public ReplacePair(object orginal, string replace)
        {
            Orginal = orginal;
            Replace = replace;
        }

        public object Orginal { get; }

        public string Replace { get; }
    }
}
