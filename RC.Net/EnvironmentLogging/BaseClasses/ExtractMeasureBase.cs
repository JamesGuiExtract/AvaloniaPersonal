namespace ExtractEnvironmentService
{
    public class ExtractMeasureBase 
    {
        public string Customer { get; set; }
        public string Context { get; set; }
        public string Entity { get; set; }
        public string MeasurementType { get; set; }
        public int MeasurementInterval { get; set; }
        public bool PinThread { get; set; }
        public bool Enabled { get; set; }
    }
}
