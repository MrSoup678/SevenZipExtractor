

namespace SevenZipExtractor.Tests.Xunit
{
    
    public class TestRar : TestBase
    {
        [Fact]
        public void TestGuessAndExtractToStream_OK()
        {
            byte[] fileMem = File.ReadAllBytes("Resources/rar.rar");
            this.TestExtractToStream(fileMem, this.TestEntriesWithFolder);
        }

        [Fact]
        public void TestKnownFormatAndExtractToStream_OK()
        {
            byte[] fileMem = File.ReadAllBytes("Resources/rar.rar");
            this.TestExtractToStream(fileMem, this.TestEntriesWithFolder, SevenZipFormat.Rar5);
        }
    }
}