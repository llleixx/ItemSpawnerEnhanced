using System;

namespace ItemSpawnerEnhanced.Core;

internal static class EditDistance
{
    public static int DamerauLevenshtein(string source, string target, int maximum)
    {
        if (Math.Abs(source.Length - target.Length) > maximum)
            return maximum + 1;

        int[,] distance = new int[source.Length + 1, target.Length + 1];
        for (int i = 0; i <= source.Length; i++)
            distance[i, 0] = i;
        for (int j = 0; j <= target.Length; j++)
            distance[0, j] = j;

        for (int i = 1; i <= source.Length; i++)
        {
            int rowMinimum = int.MaxValue;
            for (int j = 1; j <= target.Length; j++)
            {
                int substitution = source[i - 1] == target[j - 1] ? 0 : 1;
                int value = Math.Min(
                    Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1),
                    distance[i - 1, j - 1] + substitution);

                if (i > 1 && j > 1 && source[i - 1] == target[j - 2] && source[i - 2] == target[j - 1])
                    value = Math.Min(value, distance[i - 2, j - 2] + 1);

                distance[i, j] = value;
                rowMinimum = Math.Min(rowMinimum, value);
            }

            if (rowMinimum > maximum)
                return maximum + 1;
        }

        return distance[source.Length, target.Length];
    }
}

