

using System;


namespace Opc.Ae
{
    [Serializable]
    public class SubCondition : ICloneable
    {
        private string m_name;
        private string m_definition;
        private int m_severity = 1;
        private string m_description;

        public string Name
        {
            get => m_name;
            set => m_name = value;
        }

        public string Definition
        {
            get => m_definition;
            set => m_definition = value;
        }

        public int Severity
        {
            get => m_severity;
            set => m_severity = value;
        }

        public string Description
        {
            get => m_description;
            set => m_description = value;
        }

        public override string ToString() => Name;

        public virtual object Clone() => MemberwiseClone();
    }
}
