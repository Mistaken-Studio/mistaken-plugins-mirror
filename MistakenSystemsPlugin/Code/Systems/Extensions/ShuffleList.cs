using System.Collections.Generic;

public static partial class Extensions
{
    public static void Shuffle<T>(this IList<T> list)
    {
        System.Random random = new System.Random();
        int i = list.Count;
        while (i > 1)
        {
            i--;
            int index = random.Next(i + 1);
            T value = list[index];
            list[index] = list[i];
            list[i] = value;
        }
    }
    public static void Shuffle<T>(this IList<T> list, int seed)
    {
        System.Random random = new System.Random(seed);
        int i = list.Count;
        while (i > 1)
        {
            i--;
            int index = random.Next(i + 1);
            T value = list[index];
            list[index] = list[i];
            list[i] = value;
        }
    }

    public static T[] Shuffle<T>(this T[] list)
    {
        System.Random random = new System.Random();
        int i = list.Length;
        while (i > 1)
        {
            i--;
            int index = random.Next(i + 1);
            T value = list[index];
            list[index] = list[i];
            list[i] = value;
        }
        return list;
    }
    public static T[] Shuffle<T>(this T[] list, int seed)
    {
        System.Random random = new System.Random(seed);
        int i = list.Length;
        while (i > 1)
        {
            i--;
            int index = random.Next(i + 1);
            T value = list[index];
            list[index] = list[i];
            list[i] = value;
        }
        return list;
    }
}