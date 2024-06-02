namespace MinigolfFriday.Extensions;

public static class ListExtensions
{
    public static T PopMaxScore<T>(this LinkedList<T> list, Func<T, int> scoreSelector)
    {
        if (list.Count == 0)
            throw new InvalidOperationException("The list is empty.");

        var maxScore = int.MinValue;
        LinkedListNode<T>? maxScoreNode = null;
        var current = list.First;
        while (current != null)
        {
            var score = scoreSelector(current.Value);
            if (score > maxScore)
            {
                maxScore = score;
                maxScoreNode = current;
            }
            current = current.Next;
        }

        list.Remove(maxScoreNode!);
        return maxScoreNode!.Value;
    }
}
