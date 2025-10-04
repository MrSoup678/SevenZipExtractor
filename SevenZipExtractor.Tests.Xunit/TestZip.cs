
using SevenZipExtractor.Enum;
namespace SevenZipExtractor.Tests.Xunit
{
    
    public class TestZip : TestBase
    {
        [Fact]
        public void TestGuessAndExtractToStream_OK()
        {
            byte[] fileMem = File.ReadAllBytes("Resources/zip.zip");
            this.TestExtractToStream(fileMem, this.TestEntriesWithFolder);
        }

        [Fact]
        public void TestKnownFormatAndExtractToStream_OK()
        {
            byte[] fileMem = File.ReadAllBytes("Resources/zip.zip");
            this.TestExtractToStream(fileMem, this.TestEntriesWithFolder, SevenZipFormat.Zip);
        }

        [Fact]
        public void TestKnownFormatAndExtractToStream_WithPassword_OK()
        {
            byte[] fileMem = File.ReadAllBytes("Resources/zip-hello.zip");
            this.TestExtractToStream(fileMem, this.TestEntriesWithFolder, SevenZipFormat.Zip, "hello");
        }
    }
}