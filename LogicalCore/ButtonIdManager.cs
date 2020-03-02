using LogicalCore.TreeNodes;

namespace LogicalCore
{
    internal class ButtonIdManager
    {
        internal static string GetActionNameFromCallbackData(string data)
        {
            int indexOfFirstUnderscore = data.IndexOf("_");
            if (indexOfFirstUnderscore < 1)
            {
                return data;
            }
            string firstPart = data.Substring(0, indexOfFirstUnderscore);
            return firstPart;
        }

        internal static string GetInlineButtonId(ITreeNode node) => DefaultStrings.GoTo + '_' + node.Id;

        internal static string GetNextSubstring(string data, int indexOfPrevUnderscore, out int indexOfNextUnderscore)
        {
            indexOfNextUnderscore = data.IndexOf("_", indexOfPrevUnderscore + 1);

            if (indexOfNextUnderscore < 0) indexOfNextUnderscore = data.Length;

            return data.Substring(indexOfPrevUnderscore + 1, indexOfNextUnderscore - indexOfPrevUnderscore - 1);
        }

        internal static string GetNextSubstring(string data, int indexOfPrevUnderscore) =>
            GetNextSubstring(data, indexOfPrevUnderscore, out int indexOfNextUnderscore);

        internal static int GetIDFromCallbackData(string data)
        {
            string possibleID = GetNextSubstring(data, data.IndexOf("_"));

            int.TryParse(possibleID, out int nodeID);
            return nodeID;
        }

        internal static int GetPageFromCallbackData(string data)
        {
            int indexOf1Underscore = data.IndexOf("_");
            if (indexOf1Underscore < 0) return 0;

            int indexOf2Underscore = data.IndexOf("_", indexOf1Underscore + 1);
            if (indexOf2Underscore < 0) return 0;

            string possiblePage = data.Substring(indexOf2Underscore + 1);

            int.TryParse(possiblePage, out int page);
            return page;
        }
    }
}