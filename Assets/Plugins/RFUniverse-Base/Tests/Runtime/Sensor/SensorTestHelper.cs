using NUnit.Framework;
using Robotflow.RFUniverse.Sensors;

namespace Robotflow.RFUniverse.Tests
{
    public static class SensorTestHelper
    {
        public static void CompareObservation(ISensor sensor, float[] expected)
        {
            string errorMessage;
            bool isOK = SensorHelper.CompareObservation(sensor, expected, out errorMessage);
            Assert.IsTrue(isOK, errorMessage);
        }

        public static void CompareObservation(ISensor sensor, float[,,] expected)
        {
            string errorMessage;
            bool isOK = SensorHelper.CompareObservation(sensor, expected, out errorMessage);
            Assert.IsTrue(isOK, errorMessage);
        }
    }
}
