using System.Collections.Generic;
using System.Linq;

namespace LogicalCore
{
    public enum ElementsLocation
    {
        /// <summary>
        /// Сверху вниз, двигаясь вправо.
        /// </summary>
        Иr, // Стандартное расположение
        // 1 4 7
        // 2 5 8
        // 3 6 9

        /// <summary>
        /// Слева направо, двигаясь вниз.
        /// </summary>
        Zd,
        // 1 2 3
        // 4 5 6
        // 7 8 9

        /// <summary>
        /// Снизу вверх, двигаясь вправо.
        /// </summary>
        Nr,
        // 3 6 9
        // 2 5 8
        // 1 4 7

        /// <summary>
        /// Слева направо, двигаясь вверх.
        /// </summary>
        Su,
        // 7 8 9
        // 4 5 6
        // 1 2 3

        /// <summary>
        /// Снизу вверх, двигаясь влево.
        /// </summary>
        Иl,
        // 9 6 3
        // 8 5 2
        // 7 4 1

        /// <summary>
        /// Справа налево, двигаясь вверх.
        /// </summary>
        Zu,
        // 9 8 7
        // 6 5 4
        // 3 2 1

        /// <summary>
        /// Сверху вниз, двигаясь влево.
        /// </summary>
        Nl,
        // 7 4 1
        // 8 5 2
        // 9 6 3

        /// <summary>
        /// Справа налево, двигаясь вниз.
        /// </summary>
        Sd,
        // 3 2 1
        // 6 5 4
        // 9 8 7
    }

    public static class LocationManager
    {
        internal static void SetElementsLocation<T>(ElementsLocation locationType, List<List<T>> elements)
        {
            int buttonsCount = elements.Select(list => list.Count).Sum();
            int maxCapacity = elements.Max((list) => list.Count);
            int rowsCount = elements.Count;
            // список всех кнопок в порядке их добавления
            T[] allButtons = new T[buttonsCount];
            // 1 4
            // 2 5 => 1, 2, 3, 4, 5
            // 3
            for (int column = 0, btnNumber = 0; column < maxCapacity; column++) // пробегаемся по столбцам
            {
                for (int row = 0; row < rowsCount; row++) // пробегаемся по рядам
                {
                    T btn = elements[row].ElementAtOrDefault(column);
                    if (!Equals(btn, default(T))) allButtons[btnNumber++] = btn;
                }
            }
            // список новых рядов кнопок
            T[][] newButtons = new T[rowsCount][];
            for (int btnRow = 0; btnRow < rowsCount; btnRow++)
            {
                newButtons[btnRow] = new T[maxCapacity];
            }
            int i, j;
            switch (locationType)
            {
                case ElementsLocation.Иr:
                    //Это стандартное расположение кнопок, ничего менять не нужно
                    return;

                case ElementsLocation.Zd:
                    i = 0;
                    j = 0;
                    foreach (var button in allButtons)
                    {
                        if (j >= maxCapacity)
                        {
                            i++;
                            j = 0;
                        }
                        //newButtons[i].Insert(j++, button);
                        newButtons[i][j++] = button;
                    }
                    break;

                case ElementsLocation.Nr:
                    i = rowsCount - 1;
                    j = 0;
                    foreach (var button in allButtons)
                    {
                        if (i < 0)
                        {
                            i = rowsCount - 1;
                            j++;
                        }
                        //newButtons[i--].Insert(j, button);
                        newButtons[i--][j] = button;
                    }
                    break;

                case ElementsLocation.Su:
                    i = rowsCount - 1;
                    j = 0;
                    foreach (var button in allButtons)
                    {
                        if (j >= maxCapacity)
                        {
                            i--;
                            j = 0;
                        }
                        //newButtons[i].Insert(j++, button);
                        newButtons[i][j++] = button;
                    }
                    break;

                case ElementsLocation.Иl:
                    i = rowsCount - 1;
                    j = maxCapacity - 1;
                    foreach (var button in allButtons)
                    {
                        if (i < 0)
                        {
                            i = rowsCount - 1;
                            j--;
                        }
                        //newButtons[i--].Insert(j, button);
                        newButtons[i--][j] = button;
                    }
                    break;

                case ElementsLocation.Zu:
                    i = rowsCount - 1;
                    j = maxCapacity - 1;
                    foreach (var button in allButtons)
                    {
                        if (j < 0)
                        {
                            i--;
                            j = maxCapacity - 1;
                        }
                        //newButtons[i].Insert(j--, button);
                        newButtons[i][j--] = button;
                    }
                    break;

                case ElementsLocation.Nl:
                    i = 0;
                    j = maxCapacity - 1;
                    foreach (var button in allButtons)
                    {
                        if (i >= rowsCount)
                        {
                            i = 0;
                            j--;
                        }
                        //newButtons[i++].Insert(j, button);
                        newButtons[i++][j] = button;
                    }
                    break;

                case ElementsLocation.Sd:
                    i = 0;
                    j = maxCapacity - 1;
                    foreach (var button in allButtons)
                    {
                        if (j < 0)
                        {
                            i++;
                            j = maxCapacity - 1;
                        }
                        //newButtons[i].Insert(j--, button);
                        newButtons[i][j--] = button;
                    }
                    break;

                default:
                    // а тут ничего не должно быть, switch у нас по всему enum
                    return;
            }

            elements.Clear();
            elements.AddRange(newButtons.Select(array => array.Where((button) => !Equals(button, default(T))).ToList()));
        }
    }
}
