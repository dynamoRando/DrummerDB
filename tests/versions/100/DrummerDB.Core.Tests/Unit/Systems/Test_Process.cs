using Drummersoft.DrummerDB.Core.Systems;
using System.IO;
using System.Reflection;
using Xunit;

namespace Drummersoft.DrummerDB.Core.Tests.Unit.Systems
{
    public class Test_Process
    {
        [Fact]
        public void Test_Get_HostDbExtension()
        {
            string storageFolder = Path.Combine(TestConstants.TEST_TEMP_FOLDER, "Test1");

            var directoryInfo = new DirectoryInfo(storageFolder);

            if (directoryInfo.Exists)
            {
                foreach (var file in directoryInfo.EnumerateFiles())
                {
                    file.Delete();
                }
            }

            var process = new Process(storageFolder);
            process.Start();
            string HostDbExtension = ".drum";
            Assert.Equal(process.Settings.HostDbExtension, HostDbExtension);
        }

        /// <summary>
        /// Starts a Process and ensures the partialDbextension matches
        /// </summary>
        [Fact]
        public void Test_Get_PartialDbExtension()
        {
            string storageFolder = Path.Combine(TestConstants.TEST_TEMP_FOLDER, "Test2");

            var directoryInfo = new DirectoryInfo(storageFolder);

            if (directoryInfo.Exists)
            {
                foreach (var file in directoryInfo.EnumerateFiles())
                {
                    file.Delete();
                }
            }

            var process = new Process(storageFolder);
            process.Start();
            string partDBExtension = ".drumpart";
            Assert.Equal(process.Settings.PartialDbExtension, partDBExtension);
        }

        /// <summary>
        /// Starts a Process and ensures that the expected storage folder matches
        /// </summary>
        [Fact]
        public void Test_Get_DatabaseFolderLocation()
        {
            string storageFolder = Path.Combine(TestConstants.TEST_TEMP_FOLDER, "Test3");

            var directoryInfo = new DirectoryInfo(storageFolder);

            if (directoryInfo.Exists)
            {
                foreach (var file in directoryInfo.EnumerateFiles())
                {
                    file.Delete();
                }
            }

            var process = new Process(storageFolder);
            process.Start();
            string location = Path.Join(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), process.Settings.DatabaseFolderName);
            string processLocation = process.Settings.DatabaseFolder;

            Assert.Equal(processLocation, location);
        }
    }
}
