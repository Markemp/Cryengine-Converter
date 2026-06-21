namespace CgfConverter;

public record CryEngineOptions(
    string? MaterialFiles = null,
    string? ObjectDir = null,
    bool IncludeAnimations = false);
