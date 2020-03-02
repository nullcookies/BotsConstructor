using System;
using System.Linq;

namespace LogicalCore
{
	/// <summary>
	/// Структура для хранения метатекста со специальным значением и единицей измерения.
	/// </summary>
	/// <typeparam name="T">Тип хранимого непереводимого значения.</typeparam>
	public struct MetaValued<T> : ITranslatable
    {
		public int? ID { get; } // ID может и не быть
        public MetaText Text { get; }
        public T Value { get; set; }
        public MetaText Unit { get; }
        public bool UseSpaceForUnit { get; }
        public bool UnitBeforeValue { get; }

        public MetaValued(MetaText text, T value, MetaText unit, bool unitSpace = true, bool unitFirst = false, int? id = null)
        {
			ID = id;
            Text = text;
            Value = value;
            if (unit.ToString().Any(char.IsDigit)) throw new ArgumentException("Единица измерения не должна содержать цифры!");
            Unit = unit;
            UseSpaceForUnit = unitSpace;
            UnitBeforeValue = unitFirst;
        }

        public override string ToString()
        {
            string result = Text.ToString();

            result += " ";
            
            if(UnitBeforeValue)
            {
                result += Unit.ToString();
                if (UseSpaceForUnit) result += " ";
                result += Value.ToString();
            }
            else
            {
                result += Value.ToString();
                if (UseSpaceForUnit) result += " ";
                result += Unit.ToString();
            }

            return result;
        }

        public string ToString(ITranslator session)
        {
            string result = Text.ToString(session);

            result += " ";

            if (UnitBeforeValue)
            {
                result += Unit.ToString(session);
                if (UseSpaceForUnit) result += " ";
                result += Value.ToString(session);
            }
            else
            {
				result += Value.ToString(session);
				if (UseSpaceForUnit) result += " ";
                result += Unit.ToString(session);
            }

            return result;
        }

        public MetaText ToMetaText()
        {
            MetaText metaText = new MetaText(Text);

            metaText += " ";

            if (UnitBeforeValue)
            {
                metaText += Unit.ToString();
                if (UseSpaceForUnit) metaText += " ";
                metaText += Value.ToString();
            }
            else
            {
                metaText += Value.ToString();
                if (UseSpaceForUnit) metaText += " ";
                metaText += Unit.ToString();
            }

            return metaText;
        }

        public static implicit operator MetaText(MetaValued<T> metaValued) => metaValued.ToMetaText();
    }
}
