namespace BackupApp.Errors;

public enum ErrorCode
{
    ConfigInitProblem,
    ConfigNotFound,
    ConfigInvalidFormat,
    SourceFolderMissing,
    TargetAccessDeniedConfigInitProblem
}
public class BackupException : Exception
{
    public ErrorCode Code { get; }
    public BackupException(ErrorCode code, string msg) : base(msg) => Code = code;
}
