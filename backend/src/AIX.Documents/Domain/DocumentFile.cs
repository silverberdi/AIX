namespace AIX.Documents.Domain;

public sealed record DocumentFile(
    DocumentFileId Id,
    DocumentFileRole Role,
    string FileName,
    string ContentType,
    long SizeInBytes);
