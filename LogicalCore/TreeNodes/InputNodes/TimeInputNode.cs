using System;

namespace LogicalCore
{
	public class TimeInputNode : UsualInputNode<TimeSpan>
    {
        public TimeInputNode(string name, string varName,
            IMetaMessage metaMessage = null, bool required = true, bool needBack = true, bool useCallbacks = false)
            : base(name, varName, new TryConvert<TimeSpan>((string text, out TimeSpan variable) =>
			{
				bool parsed = DateTime.TryParseExact(text.Trim(), "H:mm",
					System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.NoCurrentDateDefault,
					out DateTime dt);
				variable = dt - dt.Date;
				return parsed;
			}), metaMessage, required, needBack, useCallbacks) { }

        public TimeInputNode(string name, string varName,
            string description, bool required = true, bool needBack = true, bool useCallbacks = false)
            : this(name, varName, new MetaMessage(description ?? name), required, needBack, useCallbacks) { }
    }
}
