using System.Collections.Generic;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;

internal sealed class ReplaceItem
{
    internal Item NewItem { get; private set; }
    internal Item.HolderHandler ForItemHolderHandler { get; private set; }

    internal ReplaceItem(Item newItem, Item.HolderHandler forItemHolderHandler)
    {
        NewItem = newItem;
        ForItemHolderHandler = forItemHolderHandler;
    }
}

internal sealed class GoToGame
{
    internal int MapIndexDesired { get; private set; }

    internal GoToGame(int mapIndex)
    { 
        MapIndexDesired = mapIndex; 
    }
}

internal sealed class GoToGameOver
{
}

internal sealed class GoToMenu
{
}

internal sealed class GobalScoreChangedInData
{
    internal int InIndex { get; private set; }
    internal float ValueMaxInIndex { get; private set; }
    internal float ValueChangedTo { get; private set; }

    internal GobalScoreChangedInData(int index, float valueMaxInIndex, float valueChangedTo) 
    {
        InIndex = index;
        ValueMaxInIndex = valueMaxInIndex;
        ValueChangedTo = valueChangedTo;
    }
}
