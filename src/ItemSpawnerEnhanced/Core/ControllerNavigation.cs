using System;
using System.Collections.Generic;

namespace ItemSpawnerEnhanced.Core;

internal enum ControllerNavigationDirection
{
    Left,
    Right,
    Up,
    Down
}

internal static class ControllerNavigation
{
    public static int MoveInGrid(
        int currentIndex,
        int itemCount,
        int columnCount,
        ControllerNavigationDirection direction)
    {
        if (itemCount <= 0)
            return -1;

        int current = Math.Clamp(currentIndex, 0, itemCount - 1);
        int columns = Math.Max(1, columnCount);
        int row = current / columns;
        int column = current % columns;
        return direction switch
        {
            ControllerNavigationDirection.Left when column > 0 => current - 1,
            ControllerNavigationDirection.Right
                when column + 1 < columns && current + 1 < itemCount => current + 1,
            ControllerNavigationDirection.Up when row > 0 => current - columns,
            ControllerNavigationDirection.Down => MoveDown(current, column, itemCount, columns),
            _ => current
        };
    }

    public static int FindNearest(float horizontalPosition, IReadOnlyList<float> candidates)
    {
        if (candidates.Count == 0)
            return -1;

        int nearest = 0;
        float nearestDistance = Math.Abs(candidates[0] - horizontalPosition);
        for (int index = 1; index < candidates.Count; index++)
        {
            float distance = Math.Abs(candidates[index] - horizontalPosition);
            if (distance < nearestDistance)
            {
                nearest = index;
                nearestDistance = distance;
            }
        }

        return nearest;
    }

    public static int ResolveFallbackIndex(int previousIndex, int itemCount) =>
        itemCount <= 0 ? -1 : Math.Clamp(previousIndex, 0, itemCount - 1);

    public static int MoveRegion(int currentRegion, int direction, IReadOnlyList<bool> availableRegions)
    {
        if (availableRegions.Count == 0)
            return -1;

        int current = Math.Clamp(currentRegion, 0, availableRegions.Count - 1);
        if (direction == 0)
            return current;
        int step = direction < 0 ? -1 : 1;
        for (int offset = 1; offset <= availableRegions.Count; offset++)
        {
            int candidate = (current + step * offset + availableRegions.Count) % availableRegions.Count;
            if (availableRegions[candidate])
                return candidate;
        }

        return current;
    }

    private static int MoveDown(int current, int column, int itemCount, int columns)
    {
        int next = current + columns;
        if (next < itemCount)
            return next;

        int lastRowStart = itemCount - itemCount % columns;
        if (lastRowStart == itemCount)
            return current;
        if (current >= lastRowStart)
            return current;
        if (current < lastRowStart - columns)
            return current;

        int lastRowCount = itemCount - lastRowStart;
        return lastRowStart + Math.Min(column, lastRowCount - 1);
    }
}
