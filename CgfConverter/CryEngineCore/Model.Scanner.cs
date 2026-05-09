using System;
using System.Collections.Generic;
using System.IO;
using CgfConverter.Diagnostics.ChunkScanning;
using CgfConverter.Services;

namespace CgfConverter.CryEngineCore;

public partial class Model
{
    internal static ChunkScanFileResult ScanChunkIssues(string fileName, Stream stream, bool closeStream = false)
    {
        var records = new List<ChunkIssueRecord>();
        var model = new Model
        {
            FileName = fileName
        };

        try
        {
            using var reader = new EndiannessChangeableBinaryReader(stream);

            try
            {
                model.ReadFileHeader(reader);
            }
            catch (Exception ex)
            {
                records.Add(CreateFileFailureRecord(fileName, model, ChunkParserStage.FileHeader, ex));
                return CreateResult(fileName, model, records);
            }

            try
            {
                model.ReadChunkTable(reader);
            }
            catch (Exception ex)
            {
                records.Add(CreateFileFailureRecord(fileName, model, ChunkParserStage.ChunkTable, ex));
                return CreateResult(fileName, model, records);
            }

            for (var i = 0; i < model.chunkHeaders.Count; i++)
            {
                var header = model.chunkHeaders[i];
                Chunk chunk;

                try
                {
                    chunk = Chunk.New(header.ChunkType, header.Version);
                }
                catch (NotSupportedException ex)
                {
                    records.Add(CreateHeaderRecord(
                        ChunkIssueKind.UnsupportedChunkVersion,
                        fileName,
                        model,
                        header,
                        i,
                        ChunkParserStage.ChunkFactory,
                        ex));
                    continue;
                }
                catch (Exception ex)
                {
                    records.Add(CreateHeaderRecord(
                        ChunkIssueKind.ParseFailure,
                        fileName,
                        model,
                        header,
                        i,
                        ChunkParserStage.ChunkFactory,
                        ex));
                    continue;
                }

                chunk.Load(model, header);

                if (chunk is ChunkUnknown)
                {
                    records.Add(CreateHeaderRecord(
                        ChunkIssueKind.UnknownChunkType,
                        fileName,
                        model,
                        header,
                        i,
                        ChunkParserStage.ChunkFactory));
                }

                try
                {
                    chunk.Read(reader);
                }
                catch (Exception ex)
                {
                    records.Add(CreateHeaderRecord(
                        ChunkIssueKind.ParseFailure,
                        fileName,
                        model,
                        header,
                        i,
                        ChunkParserStage.ChunkRead,
                        ex,
                        chunk.DataSize));
                    continue;
                }

                try
                {
                    chunk.SkipBytes(reader);
                }
                catch (Exception ex)
                {
                    records.Add(CreateHeaderRecord(
                        ChunkIssueKind.ParseFailure,
                        fileName,
                        model,
                        header,
                        i,
                        ChunkParserStage.ChunkSkip,
                        ex,
                        chunk.DataSize));
                }
            }

            return CreateResult(fileName, model, records);
        }
        finally
        {
            if (closeStream)
                stream.Close();
        }
    }

    private static ChunkScanFileResult CreateResult(
        string fileName,
        Model model,
        IReadOnlyList<ChunkIssueRecord> records)
    {
        return new ChunkScanFileResult(
            fileName,
            model.FileSignature,
            model.FileSignature is null ? null : (uint)model.FileVersion,
            records);
    }

    private static ChunkIssueRecord CreateFileFailureRecord(
        string fileName,
        Model model,
        ChunkParserStage stage,
        Exception exception)
    {
        return new ChunkIssueRecord
        {
            IssueKind = ChunkIssueKind.ParseFailure,
            InputPath = fileName,
            Extension = Path.GetExtension(fileName).ToLowerInvariant(),
            ModelFileName = model.FileName,
            FileSignature = model.FileSignature,
            FileVersion = model.FileSignature is null ? null : (uint)model.FileVersion,
            ParserStage = stage,
            ExceptionType = exception.GetType().Name,
            ExceptionMessage = exception.Message
        };
    }

    private static ChunkIssueRecord CreateHeaderRecord(
        ChunkIssueKind issueKind,
        string fileName,
        Model model,
        ChunkHeader header,
        int occurrenceIndex,
        ChunkParserStage stage,
        Exception? exception = null,
        uint? dataSize = null)
    {
        var isIvo = model.FileVersion == FileVersion.x0900;
        uint? chunkSize = isIvo && header.Size == 0 ? null : header.Size;

        return new ChunkIssueRecord
        {
            IssueKind = issueKind,
            InputPath = fileName,
            Extension = Path.GetExtension(fileName).ToLowerInvariant(),
            ModelFileName = model.FileName,
            FileSignature = model.FileSignature,
            FileVersion = (uint)model.FileVersion,
            ChunkTypeValue = (uint)header.ChunkType,
            ChunkTypeHex = FormatHex((uint)header.ChunkType),
            ChunkTypeName = Enum.IsDefined(typeof(ChunkType), header.ChunkType) ? header.ChunkType.ToString() : null,
            ChunkVersionRawHex = FormatHex(header.VersionRaw),
            ChunkVersionHex = FormatHex(header.Version),
            ChunkId = header.ID,
            ChunkIdIsGenerated = isIvo,
            ChunkOffset = header.Offset,
            ChunkOffsetHex = FormatHex(header.Offset),
            ChunkSize = chunkSize,
            ChunkDataSize = dataSize,
            OccurrenceIndex = occurrenceIndex,
            ParserStage = stage,
            ExceptionType = exception?.GetType().Name,
            ExceptionMessage = exception?.Message
        };
    }

    private static string FormatHex(uint value)
    {
        return $"0x{value:X}";
    }
}
