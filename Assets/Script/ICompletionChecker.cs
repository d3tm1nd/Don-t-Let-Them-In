// ICompletionChecker.cs
// Place this file anywhere under Assets/Scripts.
// Provides a shared interface used by BedInteractSceneFlow / BedPhaseCompletionProvider.

public interface ICompletionChecker
{
    bool IsCompleted { get; }
}
