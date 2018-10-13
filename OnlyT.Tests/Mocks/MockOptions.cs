namespace OnlyT.Tests.Mocks
{
    using OnlyT.Services.Options;

    public static class MockOptions
    {
        public static Options Create()
        {
            var result = new Options();
            return result;
        }
    }
}
