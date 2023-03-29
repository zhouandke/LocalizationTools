using System;

namespace Localization2
{
    public abstract class LocalizationBuilderBase
    {
        protected readonly ILts lts;

        protected LocalizationBuilderBase(ILts lts)
        {
            this.lts = lts;
        }

        public abstract bool IsMatch(Type type);

        public abstract double MatchOrder { get; }

        public abstract void SetLocalizationer(TypeInfo typeInfo);
    }
}
