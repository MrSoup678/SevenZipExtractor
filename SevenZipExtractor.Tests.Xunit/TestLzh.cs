using Xunit;

namespace SevenZipExtractor.Tests
{
    public class TestLzh : TestBase
    {
        // LZH does not provide folder as entry, only files

        [Fact]
        public void TestGuessAndExtractToStream_Fails()
        {
            Assert.Throws<SevenZipException>(() =>
            {
                byte[] fileMem = File.ReadAllBytes("Resources/lzh.lzh");
                this.TestExtractToStream(fileMem, this.TestEntriesWithoutFolder);
            });
        }

        [Fact]
        public void TestKnownFormatAndExtractToStream_OK()
        {
                byte[] fileMem = File.ReadAllBytes("Resources/lzh.lzh");
            this.TestExtractToStream(fileMem, this.TestEntriesWithoutFolder, SevenZipFormat.Lzh);
        }
    }
}