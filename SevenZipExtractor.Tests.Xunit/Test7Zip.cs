using SevenZipExtractor.Enum;
namespace SevenZipExtractor.Tests.Xunit
{
    
    public class Test7Zip : TestBase
    {
        // 7Z does not provide folder as entry, only files

        [Fact]
        public void TestGuessAndExtractToStream_OK()
        {
            byte[] fileMem = File.ReadAllBytes("Resources/SevenZip.7z");
            this.TestExtractToStream(fileMem, this.TestEntriesWithoutFolder);
        }


        [Fact]
        public void TestKnownFormatAndExtractToStream_OK()
        {
            byte[] fileMem = File.ReadAllBytes("Resources/SevenZip.7z");
            this.TestExtractToStream(fileMem, this.TestEntriesWithoutFolder, SevenZipFormat.SevenZip);
        }
    }
}