using System.Text;
using Aduo.Core.Modules.ContentOps.Domain;
using Aduo.Core.Modules.ContentOps.Services;

namespace Aduo.Core.Tests.Content;

public class FileSnifferTests
{
    private static byte[] Bytes(params byte[] b) => b;

    [Fact]
    public void Mp3WithId3Tag_IsAudio()
    {
        var r = FileSniffer.Detect("ID3\u0004\0\0\0\0\0\0"u8, "a.mp3");

        Assert.Equal((MediaKind.Audio, "audio/mpeg"), (r!.Kind, r.ContentType));
    }

    [Fact]
    public void Mp3FrameSync_IsAudio()
    {
        var r = FileSniffer.Detect(Bytes(0xFF, 0xFB, 0x90, 0x00), "a.mp3");

        Assert.Equal(MediaKind.Audio, r!.Kind);
    }

    [Fact]
    public void Ogg_Png_Jpeg_Webp_AreRecognized()
    {
        Assert.Equal("audio/ogg", FileSniffer.Detect("OggS\0\u0002"u8, "x")!.ContentType);
        Assert.Equal("image/png", FileSniffer.Detect(Bytes(0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0), "x")!.ContentType);
        Assert.Equal("image/jpeg", FileSniffer.Detect(Bytes(0xFF, 0xD8, 0xFF, 0xE0), "x")!.ContentType);
        Assert.Equal("image/webp", FileSniffer.Detect("RIFF\0\0\0\0WEBPVP8 "u8, "x")!.ContentType);
    }

    [Fact]
    public void WindowsExecutableNamedMp3_IsRejected() // CO-T11
    {
        var exe = Bytes(0x4D, 0x5A, 0x90, 0x00, 0x03, 0x00, 0x00, 0x00, 0x04, 0x00);

        Assert.Null(FileSniffer.Detect(exe, "song.mp3"));
    }

    [Fact]
    public void CsvAndJson_RecognizedByContentAndExtension()
    {
        Assert.Equal("text/csv", FileSniffer.Detect(Encoding.UTF8.GetBytes("lemma,pos\nMutter,Noun\n"), "words.csv")!.ContentType);
        Assert.Equal("application/json", FileSniffer.Detect(Encoding.UTF8.GetBytes("[{\"a\":1}]"), "words.json")!.ContentType);
        Assert.Equal(MediaKind.ImportFile, FileSniffer.Detect(Encoding.UTF8.GetBytes("a,b"), "x.csv")!.Kind);
    }

    [Fact]
    public void JsonThatIsNotJson_AndBinaryAsCsv_AreRejected()
    {
        Assert.Null(FileSniffer.Detect("hello world"u8, "x.json"));
        Assert.Null(FileSniffer.Detect(Bytes(0x00, 0x01, 0x02, 0x03), "x.csv"));
        Assert.Null(FileSniffer.Detect(Bytes(0xC3, 0x28, 0xA0, 0xA1), "x.csv")); // UTF-8 không hợp lệ
    }

    [Fact]
    public void PlainTextWithUnknownExtension_IsRejected()
    {
        Assert.Null(FileSniffer.Detect("a,b,c"u8, "x.txt"));
        Assert.Null(FileSniffer.Detect("a,b,c"u8, null));
    }

    [Fact]
    public void CsvWithMultibyteCharCutAtTheBoundary_StillDetected()
    {
        var text = new string('a', 8191) + "ü"; // "ü" là 2 byte, bị cắt ở ranh giới 8192 byte
        var bytes = Encoding.UTF8.GetBytes(text).AsSpan(0, 8192).ToArray();

        Assert.NotNull(FileSniffer.Detect(bytes, "x.csv"));
    }

    [Theory]
    [InlineData("audio", 5L * 1024 * 1024)]
    [InlineData("image", 2L * 1024 * 1024)]
    public void SizeLimitsMatchTheDesign(string kind, long expected)
    {
        var r = kind == "audio" ? FileSniffer.Detect("ID3\0\0\0"u8, "a") : FileSniffer.Detect(Bytes(0xFF, 0xD8, 0xFF), "a");

        Assert.Equal(expected, r!.MaxBytes);
    }
}
